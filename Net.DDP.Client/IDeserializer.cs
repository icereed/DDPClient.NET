namespace Net.DDP.Client
{
    public interface IDeserializer
    {

        /// <summary>
        /// Deserializes a string and will process it further.
        /// </summary>
        /// <param name="item">A string to deserialize.</param>
        void Deserialize(string item);
    }
}