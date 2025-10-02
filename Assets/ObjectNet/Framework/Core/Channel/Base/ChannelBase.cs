using System.Collections.Generic;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Base class for network communication channels.
    /// </summary>
    public abstract class ChannelBase : IChannel
    {

        /// <summary>
        /// Starts the channel.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops the channel.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Processes incoming data on the channel.
        /// </summary>
        /// <returns>Data bytes used during process</returns>
        public abstract List<byte[]> Process();

        /// <summary>
        /// Checks if the channel is connected.
        /// </summary>
        /// <returns>True if connected, false otherwise.</returns>
        public abstract bool IsConnected();

        /// <summary>
        /// Checks if the connection is lost.
        /// </summary>
        /// <returns>True if connection is lost, false otherwise.</returns>
        public abstract bool IsConnectionLost();

        /// <summary>
        /// Connects to the default endpoint.
        /// </summary>
        /// <returns>True if connection is successful, false otherwise.</returns>
        public abstract bool Connect();

        /// <summary>
        /// Return if this connection was connected at some point into the past
        /// </summary>
        /// <returns>True if connection is stablished, false otherwise.</returns>
        public abstract bool AlreadyConnected();

        /// <summary>
        /// Connects to the specified endpoint with timeout.
        /// </summary>
        /// <param name="ip">The IP address to connect to.</param>
        /// <param name="tcpPort">The TCP port to connect to.</param>
        /// <param name="udpPort">The UDP port to connect to.</param>
        /// <param name="timeout">The connection timeout in milliseconds.</param>
        /// <returns>True if connection is successful, false otherwise.</returns>
        public abstract bool Connect(string ip, ushort tcpPort, ushort udpPort, ushort timeout);

        /// <summary>
        /// Disconnects from the current endpoint.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Sends data over the channel with specified delivery mode and transport.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="mode">The delivery mode (default: Unreliable).</param>
        /// <param name="transport">The transport to use (default: null).</param>
        public abstract void Send(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable, ITransport transport = null);

        /// <summary>
        /// Transmits data over the channel with specified delivery mode and transport.
        /// </summary>
        /// <param name="data">The data to transmit.</param>
        /// <param name="mode">The delivery mode (default: Unreliable).</param>
        /// <param name="transport">The transport to use (default: null).</param>
        public abstract void Transmit(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable, ITransport transport = null);

        /// <summary>
        /// Registers a client to the channel.
        /// </summary>
        /// <param name="client">The client to register.</param>
        public abstract void RegisterClient(IClient client);

        /// <summary>
        /// Unregisters a client from the channel.
        /// </summary>
        /// <param name="client">The client to unregister.</param>
        public abstract void UnregisterClient(IClient client);

        /// <summary>
        /// Return a list of connected clients if this playter is the master on embedded mode
        /// </summary>
        /// <typeparam name="T">Type pf client that menhtod will return</typeparam>
        /// <returns>Connected clients</returns>
        public abstract List<IClient> GetConnectedClients();

        /// <summary>
        /// Gets the connected client with the specified connection ID.
        /// </summary>
        /// <param name="connectionId">The connection ID of the client.</param>
        /// <returns>The connected client with the specified ID.</returns>
        public abstract NetworkClient GetConnectedClient(int connectionId);

        /// <summary>
        /// Return a fake client for the server connection
        /// </summary>
        /// <returns>Instance of ICLient</returns>
        public abstract IClient GetLocalClient();

        /// <summary>
        /// Sets the transport class for the channel.
        /// </summary>
        /// <param name="transportClass">The transport class to set.</param>
        public abstract void SetTransport(string transportClass);

        /// <summary>
        /// Return the configured transport system namespace
        /// </summary>
        /// <returns>Configured transport system</returns>
        public abstract string GetConfiguredTransport();

        /// <summary>
        /// Sets the transport for the channel.
        /// </summary>
        /// <param name="transport">The transport to set.</param>
        public abstract void SetTransport(ITransport transport);

        /// <summary>
        /// Gets the transport used by the channel.
        /// </summary>
        /// <returns>The transport used by the channel.</returns>
        public abstract ITransport GetTransport();

        /// <summary>
        /// Return if transport system is already initialized
        /// </summary>
        /// <returns>true if initialized.</returns>
        public abstract bool IsTransportInitialized();
    }
}
