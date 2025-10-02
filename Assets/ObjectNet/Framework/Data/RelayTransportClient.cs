using System;
using System.Collections.Generic;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A sealed class that acts as a relay for an underlying transport client, implementing the ITransportClient interface.
    /// </summary>
    public sealed class RelayTransportClient : ITransportClient {

        // Holds a reference to the underlying transport mechanism.
        private ITransport sourceTransport;

        /// <summary>
        /// Initializes a new instance of the RelayTransportClient class with the specified transport.
        /// </summary>
        /// <param name="transport">The underlying transport mechanism to be used by the client.</param>
        public RelayTransportClient(ITransport transport) : base() {
            this.sourceTransport = transport;
        }

        /// <summary>
        /// Configures the transport client with the specified callbacks.
        /// </summary>
        /// <param name="onClientConnected">Callback invoked when a client connects.</param>
        /// <param name="onClientDisconnected">Callback invoked when a client disconnects.</param>
        /// <param name="onMessageReceived">Callback invoked when a message is received from a client.</param>
        public void Configure(Action<ITransportClient> onClientConnected, Action<ITransportClient> onClientDisconnected, Action<ITransportClient, byte[]> onMessageReceived) {
            this.sourceTransport.Configure(onClientConnected, onClientDisconnected, onMessageReceived);
        }

        /// <summary>
        /// Initiates a connection using the underlying transport mechanism.
        /// </summary>
        /// <returns>True if the connection was successful, otherwise false.</returns>
        public bool Connect() {
            return this.sourceTransport.Connect();
        }

        /// <summary>
        /// Disconnect transport system
        /// </summary>
        public void Disconnect() {
            this.sourceTransport.Disconnect();
        }

        /// <summary>
        /// Destroys the transport client, cleaning up any resources.
        /// </summary>
        public void Destroy() {
            this.sourceTransport.Destroy();
        }

        /// <summary>
        /// Retrieves the idle timeout value for the transport client.
        /// </summary>
        /// <returns>The idle timeout in milliseconds.</returns>
        public int GetIdleTimeout() {
            return this.sourceTransport.GetIdleTimeout();
        }

        /// <summary>
        /// Retrieves the IP address used by the transport client.
        /// </summary>
        /// <returns>The IP address as a string.</returns>
        public string GetIp() {
            return this.sourceTransport.GetIp();
        }

        /// <summary>
        /// Retrieves the port number used by the transport client.
        /// </summary>
        /// <param name="type">The type of port to retrieve (default is both).</param>
        /// <returns>The port number.</returns>
        public ushort GetPort(TransportPortType type = TransportPortType.Both) {
            return this.sourceTransport.GetPort();
        }

        /// <summary>
        /// Sets the port number for the transport client.
        /// </summary>
        /// <param name="port">The port number to set.</param>
        /// <param name="type">The type of port to set (default is both).</param>
        public void SetPort(ushort port, TransportPortType type = TransportPortType.Both) {
            this.sourceTransport.SetPort(port);
        }

        /// <summary>
        /// Initializes the transport client, preparing it for operation.
        /// </summary>
        public void Initialize() {
            this.sourceTransport.Initialize();
        }

        /// <summary>
        /// Checks if the transport client is currently connected.
        /// </summary>
        /// <returns>True if connected, otherwise false.</returns>
        public bool IsConnected() {
            return this.sourceTransport.IsConnected();
        }

        /// <summary>
        /// Handles the event when a client connects to the transport.
        /// </summary>
        /// <param name="client">The connected client.</param>
        public void OnClientConnected(ITransportClient client) {
            this.sourceTransport.OnClientConnected(client);
        }

        /// <summary>
        /// Handles the event when a client disconnects from the transport.
        /// </summary>
        /// <param name="client">The disconnected client.</param>
        public void OnClientDisconnected(ITransportClient client) {
            this.sourceTransport.OnClientDisconnected(client);
        }

        /// <summary>
        /// Handles the event when a message is received from a client.
        /// </summary>
        /// <param name="client">The client that sent the message.</param>
        /// <param name="data">The received message data.</param>
        public void OnMessageReceived(ITransportClient client, byte[] data) {
            this.sourceTransport.OnMessageReceived(client, data);
        }

        /// <summary>
        /// Processes any pending operations for the transport client.
        /// </summary>
        public List<byte[]> Process() {
            return this.sourceTransport.Process();
        }

        /// <summary>
        /// Sends data to the connected transport using the specified delivery mode.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="mode">The delivery mode (default is unreliable).</param>
        public void Send(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable) {
            this.sourceTransport.Send(data, mode);
        }

        /// <summary>
        /// Sets the idle timeout for the transport client.
        /// </summary>
        /// <param name="time">The idle timeout in milliseconds.</param>
        public void SetIdleTimeout(int time) {
            this.sourceTransport.SetIdleTimeout(time);
        }

        /// <summary>
        /// Sets the IP address for the transport client.
        /// </summary>
        /// <param name="ip">The IP address to set.</param>
        public void SetIp(string ip) {
            this.sourceTransport.SetIp(ip);
        }

        /// <summary>
        /// Registers a peer with the transport client.
        /// </summary>
        /// <param name="peer">The peer to register.</param>
        public void RegisterPeer(ITransportPeer peer) {
            throw new System.Exception("Peer to Peer is not supported by this tranport system");
        }

        /// <summary>
        /// Unregisters the specified peer from the collection of peers.
        /// </summary>
        /// <param name="peer">The peer to unregister.</param>
        public void UnregisterPeer(ITransportPeer peer) {
            throw new System.Exception("Peer to Peer is not supported by this tranport system");
        }

        /// <summary>
        /// Unregisters the peer with the specified ID from the collection of peers.
        /// </summary>
        /// <param name="id">The ID of the peer to unregister.</param>
        public void UnregisterPeer(ushort id) {
            throw new System.Exception("Peer to Peer is not supported by this tranport system");
        }

        /// <summary>
        /// Retrieves the peer with the specified ID from the collection of peers.
        /// </summary>
        /// <param name="id">The ID of the peer to retrieve.</param>
        /// <returns>The peer with the specified ID.</returns>
        public ITransportPeer GetPeer(ushort id) {
            throw new System.Exception("Peer to Peer is not supported by this tranport system");
        }

        /// <summary>
        /// Return if this transport will disconnect client by timeout
        /// </summary>
        /// <returns>True if enabled, otherwise false</returns>
        public bool IsTimeoutEnabled() {
            return this.sourceTransport.IsTimeoutEnabled();
        }

        /// <summary>
        /// Set if this transport will disconnect client by timeout
        /// </summary>
        /// <param name="value">Time out enabled</param>
        public void SetTimeoutEnabled(bool value) {
            this.sourceTransport.SetTimeoutEnabled(value);
        }

        /// <summary>
        /// Return if this transport will disconnect
        /// </summary>
        /// <returns>True if enabled, otherwise false</returns>
        public bool IsDisconnectionEnabled() {
            return this.sourceTransport.IsTimeoutEnabled();
        }

        /// <summary>
        /// Set if this transport will disconnect
        /// </summary>
        /// <param name="value">Time out enabled</param>
        public void SetDisconnectionEnabled(bool value) {
            this.sourceTransport.SetTimeoutEnabled(value);
        }

        /// <summary>
        /// Initializes the peer to peer server
        /// </summary>
        /// <param name="peerToPeerPort">Port where peer to peer data will be received</param>
        public void InitializePeerToPeerServer(ushort peerToPeerPort) {
            throw new System.Exception("Peer to Peer is not supported by this tranport system");
        }
    }

}