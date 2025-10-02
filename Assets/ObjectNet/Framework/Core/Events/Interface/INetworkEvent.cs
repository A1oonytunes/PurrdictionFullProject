
namespace com.onlineobject.objectnet {
    // Define an interface for network-related events.
    public interface INetworkEvent : IEvent {

        /// <summary>
        /// Retrieves the unique identifier for the event.
        /// </summary>
        /// <returns>An integer representing the event ID.</returns>
        int GetEvent();

        /// <summary>
        /// Executes the event using the provided data stream.
        /// </summary>
        /// <param name="reader">The data stream to read event data from.</param>
        void ExecuteEvent(IDataStream reader);

    }

}