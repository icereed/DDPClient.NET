using System;
using WebSocket4Net;

namespace Net.DDP.Client
{
    public interface IDdpStateTracker
    {
        event EventHandler OnConnecting;
        event EventHandler OnOpen;
        event EventHandler<DdpConnectionError> OnError;
        event EventHandler OnClosed;


        ConnectionState State { get; }
    }

    public interface IDdpConnector : IDdpStateTracker
    {
        event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        void Close();
        void Connect(string url, bool useSsl = true);
        void Send(string message);
    }

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