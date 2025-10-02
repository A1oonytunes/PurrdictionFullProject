// The namespace com.onlineobject.objectnet contains the definition of the IReceiveEvent interface.

namespace com.onlineobject.objectnet
{
    /// <summary>
    /// Represents an interface for receiving events.
    /// </summary>
    public interface IReceiveEvent
    {
        /// <summary>
        /// Gets the data stream reader for receiving events.
        /// </summary>
        /// <returns>An instance of IDataStream representing the data stream reader.</returns>
        IDataStream GetReader();
    }
}
