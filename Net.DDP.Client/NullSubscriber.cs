namespace Net.DDP.Client
{
    /// <summary>
    /// A <see cref="IDataSubscriber"/> implementation that does nothing at all. (Null-Pattern)
    /// </summary>
    class NullSubscriber : IDataSubscriber
    {
        public void DataReceived(dynamic data)
        {
        }

        public string Session { get; set; }
    }
}