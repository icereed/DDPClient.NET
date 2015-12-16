using Net.DDP.Client.Queueing;
using Newtonsoft.Json;

namespace Net.DDP.Client
{
    public class DDPClient : IClient
    {
        public const string DdpMessageTypeReady = "ready";
        public const string DdpMessageTypeAdded = "added";
        public const string DdpMessageTypeChanged = "changed";
        public const string DdpMessageTypeNosub = "nosub";
        public const string DdpMessageTypeRemoved = "removed";

        public const string DdpPropsMessage = "msg";
        public const string DdpPropsId = "id";
        public const string DdpPropsCollection = "collection";
        public const string DdpPropsFields = "fields";
        public const string DdpPropsSession = "session";
        public const string DdpPropsResult = "result";
        public const string DdpPropsError = "error";
        public const string DdpPropsSubs = "subs";

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

        public DDPClient() : this(new NullSubscriber())
        {
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
        public void ConnectWithSsl(string url)
        {
            _connector.Connect(url, true);
        }

        public void ConnectWithoutSsl(string url)
        {
            _connector.Connect(url, false);
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
        public void Close()
        {
            if (_connector.State != ConnectionState.Closed)
            {
                _connector.Close();
            }
        }

        /// <summary>
        /// Closes the DDP connection (<see cref="IDdpConnector.Close"/>).
        /// Please note, that some data might arrive even after disposing.
        /// </summary>
        public void Dispose()
        {
            Close();
            _queueHandler.Dispose();
        }
    }
}
