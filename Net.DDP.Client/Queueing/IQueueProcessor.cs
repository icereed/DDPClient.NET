using System;

namespace Net.DDP.Client.Queueing
{
    public interface IQueueProcessor<T> : IDisposable
    {
        void QueueItem(T item);
    }
}