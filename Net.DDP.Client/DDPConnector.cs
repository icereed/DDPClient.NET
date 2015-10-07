using System;
using WebSocket4Net;
using System.Net.Sockets;
using SuperSocket.ClientEngine;

namespace Net.DDP.Client
{
    public class DdpConnector : IDdpConnector
    {
        private WebSocket _socket;
        private string _url = string.Empty;
        private bool _isWait;

        public DdpConnector()
        {
            State = ConnectionState.Closed;
        }

        public void Connect(string url, bool useSsl = true)
        {
            if (State != ConnectionState.Closed)
            {
                throw new InvalidOperationException("The DDP connection must be closed in order to connect.");
            }
            State = ConnectionState.Connecting;
            RaiseOnConnecting();
            _url = $"{(useSsl ? "wss" : "ws")}://{url}/websocket";
            _socket = new WebSocket(_url);
            ApplyEventHandler(_socket);
            _socket.Open();
            _isWait = true;
            this.Wait();
            if (_socket.State != WebSocketState.Open)
            {
                throw new SocketException();
            }
        }


        #region Events

        public event EventHandler OnConnecting;
        public event EventHandler OnOpen;
        public event EventHandler<DdpConnectionError> OnError;
        public event EventHandler OnClosed;
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        protected virtual void RaiseOnConnecting()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler handler = OnConnecting;

            // Event will be null if there are no subscribers
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void RaiseOnOpen()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler handler = OnOpen;

            // Event will be null if there are no subscribers
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void RaiseOnError(Exception exception)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<DdpConnectionError> handler = OnError;

            // Event will be null if there are no subscribers
            handler?.Invoke(this, new DdpConnectionError(exception));
        }

        protected virtual void RaiseOnClosed()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler handler = OnClosed;

            // Event will be null if there are no subscribers
            handler?.Invoke(this, new EventArgs());
        }

        protected virtual void RaiseOnMessageReceived(MessageReceivedEventArgs args)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<MessageReceivedEventArgs> handler = OnMessageReceived;

            // Event will be null if there are no subscribers
            handler?.Invoke(this, args);
        }


        #endregion

        public ConnectionState State { get; private set; }

        private void ApplyEventHandler(WebSocket webSocket)
        {
            webSocket.MessageReceived += socket_MessageReceived;
            webSocket.Opened += _socket_Opened;
            webSocket.Opened += _onOpen;
            webSocket.Error += _onError;
            webSocket.Closed += _onClose;
        }

        public void Close()
        {
            if (State == ConnectionState.Closed)
            {
                throw new InvalidOperationException("The DDP connection may not be closed in order to close it.");
            }
            _socket.Close();
            _socket.MessageReceived -= socket_MessageReceived;
            State = ConnectionState.Closed;
            RaiseOnClosed();
            _socket.Opened -= _socket_Opened;
            _socket.Opened -= _onOpen;
            _socket.Error -= _onError;
            _socket.Closed -= _onClose;
        }

        public void Send(string message)
        {
            _socket.Send(message);
        }

        #region Mapping from websocket events to own events
        void _onOpen(object sender, EventArgs e)
        {
            State = ConnectionState.Open;
            RaiseOnOpen();
        }
        void _onClose(object sender, EventArgs e)
        {
            State = ConnectionState.Closed;
            RaiseOnClosed();
        }
        void _onError(object sender, ErrorEventArgs e)
        {
            RaiseOnError(e.Exception);
        }
        #endregion

        void _socket_Opened(object sender, EventArgs e)
        {
            this.Send("{\"msg\":\"connect\",\"version\":\"pre1\",\"support\":[\"pre1\"]}");
            _isWait = false;
        }

        void socket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            RaiseOnMessageReceived(e);
        }

        private void Wait()
        {
            while (_isWait && _socket.State == WebSocketState.Connecting)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

    }
}
