using System;

namespace Net.DDP.Client
{
    /// <summary>
    /// Interface to track the state of a DDP connection.
    /// </summary>
    public interface IDdpStateTracker
    {
        /// <summary>
        /// Gets emmited when the connector starts connecting.
        /// </summary>
        event EventHandler OnConnecting;

        /// <summary>
        /// Gets emitted when the connection is established.
        /// </summary>
        event EventHandler OnOpen;

        /// <summary>
        /// Gets emitted whenever an unhandled exception occurs.
        /// </summary>
        event EventHandler<DdpConnectionError> OnError;

        /// <summary>
        /// Gets emitted whenever the connection closed.
        /// </summary>
        event EventHandler OnClosed;

        /// <summary>
        /// The current state of the DDP connection.
        /// </summary>
        ConnectionState State { get; }
    }
}