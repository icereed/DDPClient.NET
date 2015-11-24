using Net.DDP.Client.Queueing;
using Newtonsoft.Json;

namespace Net.DDP.Client
{
    public class DDPClient : IClient
    {
        public const string DDP_MESSAGE_TYPE_READY = "ready";
        public const string DDP_MESSAGE_TYPE_ADDED = "added";
        public const string DDP_MESSAGE_TYPE_CHANGED = "changed";
        public const string DDP_MESSAGE_TYPE_NOSUB = "nosub";
        public const string DDP_MESSAGE_TYPE_REMOVED = "removed";

        public const string DDP_PROPS_MESSAGE = "msg";
        public const string DDP_PROPS_ID = "id";
        public const string DDP_PROPS_COLLECTION = "collection";
        public const string DDP_PROPS_FIELDS = "fields";
        public const string DDP_PROPS_SESSION = "session";
        public const string DDP_PROPS_RESULT = "result";
        public const string DDP_PROPS_ERROR = "error";
        public const string DDP_PROPS_SUBS = "subs";

        private readonly IDdpConnector _connector;
        private int _uniqueId;
        private readonly IQueueProcessor<string> _queueHandler;

        public DDPClient(IDdpConnector connector, IQueueProcessor<string> queueProcessor)
        {
            _connector = connector;
            _queueHandler = queueProcessor;
            _connector.OnMessageReceived += (sender, args) => _queueHandler.QueueItem(args.Message);
            _uniqueId = 1;
        }

        public DDPClient(IDataSubscriber subscriber) : this(new DdpConnector(), new DefaultQueueProcessor<string>(new JsonDeserializeHelper(subscriber).Deserialize))
        {
        }

        public DDPClient(IDdpConnector connector) : this(connector, new DefaultQueueProcessor<string>(new JsonDeserializeHelper(new NullSubscriber()).Deserialize))
        {
        }

        /// <summary>
        /// Connects to a Meteor application websocket.
        /// </summary>
        /// <param name="url">The URL of the Meteor application without protocol and "/websocket" suffix. E.g. "localhost:3000"</param>
        /// <param name="useSSL">Whether to use SSL. True by default.</param>
        public void Connect(string url, bool useSSL = true)
        {
            _connector.Connect(url, useSSL);
        }

        /// <summary>
        /// Invokes a meteor server method passing any number of arguments.
        /// </summary>
        /// <param name="methodName">Name of method to invoke</param>
        /// <param name="args">Optional method arguments</param>
        public void Call(string methodName, params object[] args)
        {
            string message =
                $"\"msg\": \"method\",\"method\": \"{methodName}\",\"params\": {CreateJSonArray(args)},\"id\": \"{NextId()}\"";
            message = "{" + message + "}";
            _connector.Send(message);
        }

        /// <summary>
        /// Subscribe to a record set. Returns a handle.
        /// </summary>
        /// <param name="subscribeTo">Name of the subscription. Matches the name of the server's publish() call.</param>
        /// <param name="args">Optional arguments passed to publisher function on server.</param>
        /// <returns>A handle</returns>
        public int Subscribe(string subscribeTo, params object[] args)
        {
            string message =
                $"\"msg\": \"sub\",\"name\": \"{subscribeTo}\",\"params\": {CreateJSonArray(args)},\"id\": \"{NextId()}\"";
            message = "{" + message + "}";
            _connector.Send(message);
            return GetCurrentRequestId();
        }

        // TODO: Implement unsubscribe

        private string CreateJSonArray(params object[] args)
        {
            return args == null ? "[]" : JsonConvert.SerializeObject(args);
        }

        private int NextId()
        {
            return _uniqueId++;
        }

        public int GetCurrentRequestId()
        {
            return _uniqueId;
        }

        public IDdpStateTracker StateTracker => _connector;

        /// <summary>
        /// Closes the DDP connection (<see cref="IDdpConnector.Close"/>).
        /// Please note, that some data might arrive even after disposing.
        /// </summary>
        public void Dispose()
        {
            _connector.Close();
            _queueHandler.Dispose();
        }
    }
}
