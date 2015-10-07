using System;

namespace Net.DDP.Client
{
    public class DdpConnectionError : EventArgs
    {
        public DdpConnectionError(Exception e)
        {
            Message = e.Message;
            Exception = e;
        }

        public Exception Exception { get; private set; }

        public string Message { get; private set; }
    }
}