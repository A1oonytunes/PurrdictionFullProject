namespace com.onlineobject.objectnet {
    /// <summary>
    /// Abstract base class for network events, providing a contract for network event handling.
    /// </summary>
    public abstract class NetworkEventBase : INetworkEvent {

        /// <summary>
        /// Retrieves the unique identifier for the network event.
        /// </summary>
        /// <returns>An integer representing the unique event identifier.</returns>
        public abstract int GetEvent();

        /// <summary>
        /// Executes the event using the provided data stream.
        /// </summary>
        /// <param name="reader">The data stream containing the event data.</param>
        public abstract void ExecuteEvent(IDataStream reader);

    }

}