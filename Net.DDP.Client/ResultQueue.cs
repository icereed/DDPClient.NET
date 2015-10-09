using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Net.DDP.Client
{
    /// <summary>
    /// A queue that will process all queued data serially.
    /// </summary>
    public class ResultQueue : IDisposable
    {
        /// <summary>
        /// Gets used to block or continue the worker thread.
        /// </summary>
        private static ManualResetEvent _blockThreadEvent;

        private readonly Queue<string> _itemsQueue;
        private string _currentJsonItem;

        private readonly IDeserializer _deserializer;

        /// <summary>
        /// Signals whether the object is disposed or not.
        /// </summary>
        private bool _disposeFlag = false;

        /// <summary>
        /// Creates a new queue with a custom deserializer.
        /// </summary>
        /// <param name="deserializer">A deserializer that will deserialize all incoming data.</param>
        public ResultQueue(IDeserializer deserializer)
        {
            _itemsQueue = new Queue<string>();
            _deserializer = deserializer;
            _disposeFlag = false;

            _blockThreadEvent = new ManualResetEvent(false);
            var workerThread = new Thread(DeserilizationLoop);
            workerThread.Start();
        }

        /// <summary>
        /// Creates a new queue for results.
        /// </summary>
        /// <param name="subscriber">A subscriber which will receive all queued data serially.</param>
        public ResultQueue(IDataSubscriber subscriber) : this(new JsonDeserializeHelper(subscriber))
        {

        }

        /// <summary>
        /// Enqueues a new JSON item as string. The subscriber will get called with the processed object.
        /// </summary>
        /// <param name="jsonItem">A JSON item as string.</param>
        /// <exception cref="InvalidOperationException">Gets thrown if <see cref="Dispose"/> was already called.</exception>
        public void QueueItem(string jsonItem)
        {
            if (_disposeFlag)
            {
                throw new InvalidOperationException("Cannot queue item when object is already disposed.");
            }
            /* To avoid race condition, because some Thread could dequeue
            while another thread enqueues data. */
            lock (_itemsQueue)
            {
                _itemsQueue.Enqueue(jsonItem);
                _blockThreadEvent.Set(); // Unblocks worker thread
            }
        }

        /// <summary>
        /// Dequeues the next item in a thread safe manner and returns whether the queue was already empty or not.
        /// </summary>
        /// <returns>False if there is nothing to dequeue.</returns>
        private bool Dequeue()
        {
            lock (_itemsQueue)
            {
                if (_itemsQueue.Count > 0)
                {
                    _currentJsonItem = _itemsQueue.Dequeue();
                }
                else
                {
                    return false;
                }

                return true;
            }
        }


        /// <summary>
        /// Processes and dequeues all items. Will be executed in separate thread.
        /// </summary>
        private void DeserilizationLoop()
        {
            bool queueNotEmpty;

            // Loop while the dispose flag is false or the queue is not empty
            do
            {
                queueNotEmpty = Dequeue();

                if (queueNotEmpty)
                {
                    // Deserialize next item
                    _deserializer.Deserialize(_currentJsonItem);
                }
                else
                {
                    // Block thread and wait for enqueued item
                    _blockThreadEvent.Reset();
                }
            } while (!_disposeFlag || queueNotEmpty);
        }

        public void Dispose()
        {
            _disposeFlag = true;
            _blockThreadEvent.Set(); // Unblocks worker thread, in order DeserilizationLoop can finish
        }
    }
}
