using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Net.DDP.Client.Queueing
{
    /// <summary>
    ///     A queue that will process all queued data serially.
    /// </summary>
    public class DefaultQueueProcessor<T> : IQueueProcessor<T>
    {
        private readonly BlockingCollection<T> _blockingCollection;

        private readonly CancellationTokenSource _cancelationToken;


        /// <summary>
        ///     Data structure containing all queued and not yet processed items.
        /// </summary>
        private readonly Queue<T> _itemsQueue;

        /// <summary>
        ///     Will get invoked with every item of the queue.
        /// </summary>
        private readonly Action<T> _processor;

        /// <summary>
        ///     Signals whether the object is disposed or not.
        /// </summary>
        private bool _disposeFlag;

        private Thread _workerThread;


        /// <summary>
        ///     Creates a new queue with a custom processor.
        /// </summary>
        /// <param name="processor">A processor that will process all queued data.</param>
        public DefaultQueueProcessor(Action<T> processor)
        {
            _itemsQueue = new Queue<T>();
            _processor = processor;
            _disposeFlag = false;

            _blockingCollection = new BlockingCollection<T>(1024);
            _cancelationToken = new CancellationTokenSource();
            _workerThread = new Thread(ProcessingLoop);
            var ran = new Random();
            _workerThread.Name = "Default Queue Processor #" + ran.Next();
            _workerThread.Start();
        }


        /// <summary>
        ///     Enqueues a new item. The processing action will get called when all items before are processed.
        /// </summary>
        /// <param name="item">An item.</param>
        /// <exception cref="InvalidOperationException">Gets thrown if <see cref="Dispose" /> was already called.</exception>
        public void QueueItem(T item)
        {
            /** We lock this method, so no thread is able to add
            *   an item while some other thread is calling BlockUntilNothingLeftInQueue();
            */

            if (_disposeFlag)
            {
                Console.WriteLine("Cannot queue item when object is already disposed: " + item);
            }
            else
            {
                try
                {
                    _blockingCollection.Add(item, _cancelationToken.Token);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }


        public void Dispose()
        {
            _disposeFlag = true;
            _cancelationToken.Cancel(); // Unblocks worker thread so ProcessingLoop can finish
        }

        /// <summary>
        ///     Processes and dequeues all items. Will be executed in separate thread.
        /// </summary>
        private void ProcessingLoop()
        {
            // Loop while the dispose flag is false
            do
            {
                try
                {
                    var item = _blockingCollection.Take(_cancelationToken.Token);

                    if (!_disposeFlag)
                    {
                        _processor(item);
                    }
                }
                catch (OperationCanceledException)
                {
                }
            } while (!_disposeFlag);
        }

        /// <summary>
        ///     Blocks the calling thread until the queue is empty. Use only for testing!
        /// </summary>
        public void BlockUntilNothingLeftInQueue()
        {
            while (_blockingCollection.Count != 0)
            {
                ; // Block while count is not null
            }
        }
    }
}