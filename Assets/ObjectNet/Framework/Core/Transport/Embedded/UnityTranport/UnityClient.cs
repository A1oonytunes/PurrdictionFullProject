#if UNITY_TRANSPORT_ENABLED
using System.Collections.Generic;
using Unity.Networking.Transport;
#endif


namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a client in a Unity networking context, extending the UnityTransport class and implementing the IUnityTransport interface.
    /// </summary>
    [TransportType(PeerToPeerSupport:false)]
    public class UnityClient
#if UNITY_TRANSPORT_ENABLED
            : UnityTransport, IUnityTransport {
#else 
        {
#endif
#if UNITY_TRANSPORT_ENABLED
        // Holds the network connection instance for this client.
        private NetworkConnection connection;

        // Indicates whether this client instance is created by the server.
        private bool isServerClientInstance = false;

        // Stores the previous state of the network connection to detect changes.
        private NetworkConnection.State previousConnectionState = NetworkConnection.State.Disconnected;

        // Data to return to pooling
        private List<byte[]> dequeuedBuffers = new List<byte[]>();

        /// <summary>
        /// Default constructor for UnityClient.
        /// </summary>
        public UnityClient() {
        }

        /// <summary>
        /// Internal constructor used when creating a UnityClient instance with a specific driver and connection.
        /// </summary>
        /// <param name="driver">The network driver to use for communication.</param>
        /// <param name="connection">The network connection associated with this client.</param>
        /// <param name="reliablePipeline">The network pipeline for reliable message delivery.</param>
        protected internal UnityClient(NetworkDriver driver, NetworkConnection connection, NetworkPipeline reliablePipeline) {
            this.SetDriver(driver);
            this.SetRealiablePipeline(reliablePipeline);
            this.connection = connection;
            this.isServerClientInstance = true;
        }

        /// <summary>
        /// Sets the network connection for this client.
        /// </summary>
        /// <param name="connection">The network connection to set.</param>
        public void SetConnection(NetworkConnection connection) {
            this.connection = connection;
        }

        /// <summary>
        /// Gets the current network connection for this client.
        /// </summary>
        /// <returns>The network connection instance.</returns>
        public NetworkConnection GetConnection() {
            return this.connection;
        }

        /// <summary>
        /// Initiates a connection to the server using the configured IP and port.
        /// </summary>
        /// <returns>True if the connection is successfully created, false otherwise.</returns>
        public override bool Connect() {
#if UNITY_2022_1_OR_NEWER
            var endpoint = NetworkEndpoint.Parse(this.GetIp(), this.GetPort());
#else
            var endpoint = NetworkEndPoint.Parse(this.GetIp(), this.GetPort());
#endif
            connection = default(NetworkConnection);
            connection = this.GetDriver().Connect(endpoint);
            return connection.IsCreated;
        }

        /// <summary>
        /// Disconnect transport system
        /// </summary>
        public override void Disconnect() {
            this.connection.Disconnect(this.GetDriver());
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
        /// Initializes the peer to peer server
        /// </summary>
        /// <param name="peerToPeerPort">Port where peer to peer data will be received</param>
        public void InitializePeerToPeerServer(ushort peerToPeerPort) {
            throw new System.Exception("Peer to Peer is not supported by this tranport system");
        }

        /// <summary>
        /// Disposes of the network driver if it has been created.
        /// </summary>
        public override void Destroy() {
            if (this.GetDriver().IsCreated) {
                this.GetDriver().Dispose();
            }
        }

        /// <summary>
        /// Initializes the client, calling the base class initialization method.
        /// </summary>
        public override void Initialize() {
            base.Initialize();
        }

        /// <summary>
        /// Checks if the client is currently connected to the server.
        /// </summary>
        /// <returns>True if the client is connected, false otherwise.</returns>
        public override bool IsConnected() {
            return ((this.connection != null) &&
                    (this.connection.IsCreated) &&
                    (this.GetDriver().IsCreated) &&
                    (!NetworkConnection.State.Disconnected.Equals(connection.GetState(this.GetDriver()))));
        }

        /// <summary>
        /// Determines if the connection was lost since the last check.
        /// </summary>
        /// <returns>True if the connection was previously connected and is now disconnected, false otherwise.</returns>
        public bool IsConnectionLost() {
            return NetworkConnection.State.Connected.Equals(this.previousConnectionState) &&
                   NetworkConnection.State.Disconnected.Equals(connection.GetState(this.GetDriver()));
        }

        /// <summary>
        /// Checks if the connection is currently in the process of connecting.
        /// </summary>
        /// <returns>True if the connection is in the process of connecting, false otherwise.</returns>
        public bool IsConnectionInProgress() {
            return NetworkConnection.State.Connecting.Equals(this.previousConnectionState);
        }

        /// <summary>
        /// Processes network events such as sending and receiving messages, and handles connection state changes.
        /// </summary>
        public override List<byte[]> Process() {
            if (!this.isServerClientInstance) {
                this.GetDriver().ScheduleUpdate().Complete();
            }
            if (!this.IsConnected()) {
                return new List<byte[]>().ToArray();
            } else if (this.IsConnectionLost()) {
                this.previousConnectionState = NetworkConnection.State.Disconnected;
                this.OnClientDisconnected(this);
            }
            this.previousConnectionState = this.connection.GetState(this.GetDriver());

            this.dequeuedBuffers.Clear();
            // Receive any incoming data (also handle with connection and disconnection)
            while (this.InternalReceiveMessageFromClient(this, out var receivedData)) {
                this.OnMessageReceived(this, receivedData);
            }
            // Send pending messages (Reliable)
            while (this.DequeueMessage(DeliveryMode.Reliable, out var dataBytes)) {
                if (!this.InternalSendMessageToClient(this, dataBytes, DeliveryMode.Reliable))
                    break;
                this.dequeuedBuffers.Add(dataBytes);
            }
            // Send pending messages (Unreliable)
            while (this.DequeueMessage(DeliveryMode.Unreliable, out var dataBytes)) {
                if (!this.InternalSendMessageToClient(this, dataBytes, DeliveryMode.Unreliable))
                    break;
                this.dequeuedBuffers.Add(dataBytes);
            }

            return this.dequeuedBuffers;
        }
#endif
    }
}