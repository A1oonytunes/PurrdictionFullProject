namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the interface for a network entity that can interact with a network and perform computations.
    /// </summary>
    /// <typeparam name="T">The type of passive arguments used by the network entity.</typeparam>
    /// <typeparam name="E">The type of data stream used for active synchronization, must implement IDataStream.</typeparam>
    public interface INetworkEntity<T, E> : IEntity where E : IDataStream {
        /// <summary>
        /// Retrieves the associated network object.
        /// </summary>
        /// <returns>The INetworkElement associated with this entity.</returns>
        INetworkElement GetNetworkObject();

        /// <summary>
        /// Sets the associated network object.
        /// </summary>
        /// <param name="networkObject">The INetworkElement to associate with this entity.</param>
        void SetNetworkObject(INetworkElement networkObject);

        /// <summary>
        /// Performs computation logic specific to the network entity.
        /// </summary>
        new void Compute();

        /// <summary>
        /// Synchronizes the network entity's state with the network.
        /// </summary>
        new void Synchronize();

        /// <summary>
        /// Consumes data from the provided data stream to update the entity's state.
        /// </summary>
        /// <param name="reader">The data stream to read data from.</param>
        new void Consume(IDataStream reader);

        /// <summary>
        /// Performs active computation logic for the network entity.
        /// </summary>
        void ComputeActive();

        /// <summary>
        /// Performs passive computation logic for the network entity.
        /// </summary>
        void ComputePassive();

        /// <summary>
        /// Synchronizes the active state of the network entity using the provided writer.
        /// </summary>
        /// <param name="writer">The data stream to write active state data to.</param>
        void SynchonizeActive(E writer);

        /// <summary>
        /// Synchronizes the passive state of the network entity using the provided data.
        /// </summary>
        /// <param name="data">The passive arguments to synchronize with.</param>
        void SynchonizePassive(T data);

        /// <summary>
        /// Retrieves the active arguments for synchronization.
        /// </summary>
        /// <returns>The active arguments as a data stream.</returns>
        E GetActiveArguments();

        /// <summary>
        /// Retrieves the passive arguments for synchronization.
        /// </summary>
        /// <returns>The passive arguments of type T.</returns>
        T GetPassiveArguments();

        /// <summary>
        /// Extracts data from the provided data stream to update the entity's state.
        /// </summary>
        /// <param name="data">The data stream containing the data to extract.</param>
        void Extract(E data);

        /// <summary>
        /// Sends an event with the specified code and data over the network.
        /// </summary>
        /// <param name="eventCode">The event code to identify the event type.</param>
        /// <param name="writer">The data stream containing the event data.</param>
        /// <param name="mode">The delivery mode for the event (default is unreliable).</param>
        /// <param name="client">The target client.</param>
        void Send(int eventCode, DataStream writer, DeliveryMode mode = DeliveryMode.Unreliable, IClient client = null);
    }

}