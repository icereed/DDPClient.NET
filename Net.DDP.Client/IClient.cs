using System;

namespace Net.DDP.Client
{
    public interface IClient : IDisposable
    {



        /// <summary>
        /// Connects to a Meteor application websocket with a SSL connection.
        /// </summary>
        /// <param name="url">The URL of the Meteor application without protocol and "/websocket" suffix. E.g. "localhost:3000"</param>
        void ConnectWithSsl(string url);

        /// <summary>
        /// Connects to a Meteor application websocket.
        /// </summary>
        /// <param name="url">The URL of the Meteor application without protocol and "/websocket" suffix. E.g. "localhost:3000"</param>
        void ConnectWithoutSsl(string url);

        /// <summary>
        /// Invokes a meteor server method passing any number of arguments.
        /// </summary>
        /// <param name="methodName">Name of method to invoke</param>
        /// <param name="args">Optional method arguments</param>
        void Call(string methodName, params object[] args);

        /// <summary>
        /// Subscribe to a record set. Returns a handle.
        /// </summary>
        /// <param name="subscribeTo">Name of the subscription. Matches the name of the server's publish() call.</param>
        /// <param name="args">Optional arguments passed to publisher function on server.</param>
        /// <returns>A handle</returns>
        int Subscribe(string subscribeTo, params object[] args);
        int GetCurrentRequestId();

        IDdpStateTracker StateTracker { get; }

        void Close();
    }
}
