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
    public class ResultQueue
    {
        private static ManualResetEvent _enqueuedEvent;
        private static Thread _workerThread;
        private readonly Queue<string> _itemsQueue;
        private string _currentJsonItem;
        private readonly IDeserializer _deserializer;

        /// <summary>
        /// Creates a new queue with a custom deserializer.
        /// </summary>
        /// <param name="deserializer">A deserializer that will deserialize all incoming data.</param>
        public ResultQueue(IDeserializer deserializer)
        {
            this._itemsQueue = new Queue<string>();
            this._deserializer = deserializer;

            _enqueuedEvent = new ManualResetEvent(false);
            _workerThread = new Thread(new ThreadStart(PerformDeserilization));
            _workerThread.Start();
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
        public void QueueItem(string jsonItem)
        {
            /* To avoid race condition, because some Thread could dequeue
            while another thread enqueues data. */
            lock (_itemsQueue)
            {
                _itemsQueue.Enqueue(jsonItem);
                _enqueuedEvent.Set();
            }
            RestartThreadIfNecessary();
        }

        /// <summary>
        /// Dequeues in a thread safe manner the next item and returns whether the queue was already empty or not.
        /// </summary>
        /// <returns>False if there is nothing to dequeue.</returns>
        private bool Dequeue()
        {
            lock (_itemsQueue)
            {
                if (_itemsQueue.Count > 0)
                {
                    _enqueuedEvent.Reset();
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
        /// If the worker thread already stopped it will get restarted.
        /// </summary>
        public void RestartThreadIfNecessary()
        {
            if (_workerThread.ThreadState != ThreadState.Stopped) return;
            _workerThread.Abort();
            _workerThread = new Thread(new ThreadStart(PerformDeserilization));
            _workerThread.Start();
        }

        /// <summary>
        /// Processes and dequeues all items. Will be executed in seperate thread.
        /// </summary>
        private void PerformDeserilization()
        {
            while (Dequeue())
            {
                _deserializer.Deserialize(_currentJsonItem);
            }
        }
    }
}
