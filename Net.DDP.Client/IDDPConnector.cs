using System;
using WebSocket4Net;

namespace Net.DDP.Client
{
    public interface IDdpConnector : IDdpStateTracker
    {
        /// <summary>
        /// Gets emitted everytime a message is received.
        /// </summary>
        event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        /// <summary>
        /// Closes the connection. This method is only valid to call if the state of the connection is not closed.
        /// </summary>
        void Close();

        /// <summary>
        /// Connects to a Meteor application websocket.
        /// </summary>
        /// <param name="url">The URL of the Meteor application without protocol and "/websocket" suffix. E.g. "localhost:3000"</param>
        /// <param name="useSSL">Whether to use SSL. True by default.</param>
        void Connect(string url, bool useSsl = true);

        /// <summary>
        /// Sends the message as it is over the wire to the DDP server.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        void Send(string message);
    }
}