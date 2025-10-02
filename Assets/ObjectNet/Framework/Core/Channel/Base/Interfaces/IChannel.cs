using System.Collections.Generic;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Interface for defining a communication channel.
    /// </summary>
    public interface IChannel {

        /// <summary>
        /// Starts the communication channel.
        /// </summary>
        void Start();

        /// <summary>
        /// Sets the transport for the communication channel by providing the transport class name.
        /// </summary>
        /// <param name="transportClass">The name of the transport class.</param>
        void SetTransport(string transportClass);

        /// <summary>
        /// Sets the transport for the communication channel using an instance of ITransport.
        /// </summary>
        /// <param name="transport">The transport instance to be set.</param>
        void SetTransport(ITransport transport);

        /// <summary>
        /// Retrieves the current transport used by the communication channel.
        /// </summary>
        /// <returns>The current transport instance.</returns>
        ITransport GetTransport();

        /// <summary>
        /// Return if transport system is already initialized
        /// </summary>
        /// <returns>true if initialized.</returns>
        public bool IsTransportInitialized();

        /// <summary>
        /// Registers a client with the communication channel.
        /// </summary>
        /// <param name="client">The client to be registered.</param>
        void RegisterClient(IClient client);

        /// <summary>
        /// Unregisters a client from the communication channel.
        /// </summary>
        /// <param name="client">The client to be unregistered.</param>
        void UnregisterClient(IClient client);

        /// <summary>
        /// Return a list of connected client's if this player is the master on embedded mode
        /// </summary>
        /// <returns>Connected clients</returns>
        List<IClient> GetConnectedClients();
        
        /// <summary>
        /// Sends data through the communication channel with the specified delivery mode and transport.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        /// <param name="mode">The delivery mode for the data (default: Unreliable).</param>
        /// <param name="transport">The transport to be used for sending the data (default: null).</param>
        void Send(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable, ITransport transport = null);

        /// <summary>
        /// Transmits data through the communication channel with the specified delivery mode and transport.
        /// </summary>
        /// <param name="data">The data to be transmitted.</param>
        /// <param name="mode">The delivery mode for the data (default: Unreliable).</param>
        /// <param name="transport">The transport to be used for transmitting the data (default: null).</param>
        void Transmit(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable, ITransport transport = null);

    }
}
