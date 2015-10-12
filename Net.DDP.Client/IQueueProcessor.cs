using System;

namespace Net.DDP.Client
{
    public interface IQueueProcessor : IDisposable
    {
        void QueueItem(string jsonItem);
    }
}