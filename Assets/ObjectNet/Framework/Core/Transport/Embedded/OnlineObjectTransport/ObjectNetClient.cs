using System;
using System.Collections.Generic;

namespace com.onlineobject.objectnet.embedded {
    /// <summary>
    /// Represents an embedded client that extends the functionality of TransportEmbedded and implements ITransportClient.
    /// </summary>
    public class ObjectNetClient : ObjectNetTransport, ITransportClient {
        // The embedded client instance.
        private EmbeddedClient client;

        /// <summary>
        /// The embedded server instance ( used for peer to per only ).
        /// </summary>
        private EmbeddedServer peerToPeerServer;

        // The connection associated with the embedded client.
        private EmbeddedConnection connection;

        // Netwrok peer to be used on p2p mode
        private Dictionary<ushort, ITransportPeer> peers = new Dictionary<ushort, ITransportPeer>();

        // Data to return to pooling
        private List<byte[]> dequeuedBuffers = new List<byte[]>();

        /// <summary>
        /// Default constructor for ClientEmbedded.
        /// </summary>
        public ObjectNetClient() {
        }

        /// <summary>
        /// Internal constructor used to create a ClientEmbedded instance with a given connection.
        /// </summary>
        /// <param name="connection">The connection to be used by the client.</param>
        protected internal ObjectNetClient(EmbeddedConnection connection) {
            this.connection = connection;
        }

        /// <summary>
        /// Retrieves the current connection.
        /// </summary>
        /// <returns>The EmbeddedConnection associated with this client.</returns>
        protected internal EmbeddedConnection GetConnection() {
            return this.connection;
        }

        /// <summary>
        /// Attempts to connect the client to the server using the IP and port provided by the base class.
        /// </summary>
        /// <returns>True if the connection is successful, false otherwise.</returns>
        public override bool Connect() {
            return this.client.Connect(string.Format("{0}:{1}", this.GetIp(), this.GetPort()));
        }

        /// <summary>
        /// Disconnect transport system
        /// </summary>
        public override void Disconnect() {
            if (this.client != null) {
                this.client.Disconnect();
            } else if (this.connection != null) {
                this.connection.LocalDisconnect();
            }
        }

        /// <summary>
        /// Return if this transport will disconnect client by timeout
        /// </summary>
        /// <returns>True if enabled, otherwise false</returns>
        public override bool IsTimeoutEnabled() {
            return this.client.Connection.CanTimeout;
        }

        /// <summary>
        /// Set if this transport will disconnect client by timeout
        /// </summary>
        /// <param name="value">Time out enabled</param>
        public override void SetTimeoutEnabled(bool value) {
            this.client.Connection.CanTimeout = value;
        }

        /// <summary>
        /// Return if this transport will disconnect
        /// </summary>
        /// <returns>True if enabled, otherwise false</returns>
        public override bool IsDisconnectionEnabled() {
            return this.client.Connection.CanDisconnect;
        }

        /// <summary>
        /// Set if this transport will disconnect
        /// </summary>
        /// <param name="value">Time out enabled</param>
        public override void SetDisconnectionEnabled(bool value) {
            this.client.Connection.CanDisconnect = value;
        }

        /// <summary>
        /// Registers a peer with the transport client.
        /// </summary>
        /// <param name="peer">The peer to register.</param>
        public void RegisterPeer(ITransportPeer peer) {
            if (!this.peers.ContainsKey(peer.GetId())) {
                this.peers.Add(peer.GetId(), peer);
                // Define peer connection
                peer.SetConnection<EmbeddedClient>(new EmbeddedClient());
                peer.GetConnection<EmbeddedClient>().Connected += (object sender, EventArgs e) => {
                    peer.SetConnected(peer.GetConnection<EmbeddedClient>().IsConnected);
                    // Execute peer to peer connect event
                    peer.OnClientConnectedOnPeer(this);                    
                };
                peer.GetConnection<EmbeddedClient>().ConnectionFailed += (object sender, ConnectionFailedEventArgs e) => {
                    peer.SetConnected(peer.GetConnection<EmbeddedClient>().IsConnected);
                    // Execute peer to peer connect event
                    peer.OnClientConnectedOnPeer(this);
                };
                peer.GetConnection<EmbeddedClient>().MessageReceived += (object sender, MessageReceivedEventArgs e) => {
                     int bufferSize = e.Message.GetInt();
                     byte[] dataBytes = e.Message.GetBytes(bufferSize);
                     this.OnMessageReceived(this, dataBytes);
                };
                peer.GetConnection<EmbeddedClient>().Connect(string.Format("{0}:{1}", peer.GetAddress(), peer.GetPort())); // Try to stablish connection with peer                                
            }
        }

        /// <summary>
        /// Unregisters the specified peer from the collection of peers.
        /// </summary>
        /// <param name="peer">The peer to unregister.</param>
        public void UnregisterPeer(ITransportPeer peer) {
            if (this.peers.ContainsKey(peer.GetId())) {
                this.peers.Remove(peer.GetId());
            }
        }

        /// <summary>
        /// Unregisters the peer with the specified ID from the collection of peers.
        /// </summary>
        /// <param name="id">The ID of the peer to unregister.</param>
        public void UnregisterPeer(ushort id) {
            if (this.peers.ContainsKey(id)) {
                this.peers.Remove(id);
            }
        }

        /// <summary>
        /// Retrieves the peer with the specified ID from the collection of peers.
        /// </summary>
        /// <param name="id">The ID of the peer to retrieve.</param>
        /// <returns>The peer with the specified ID.</returns>
        public ITransportPeer GetPeer(ushort id) {
            return this.peers[id];
        }


        /// <summary>
        /// Cleans up resources by disconnecting the client if it is not null.
        /// </summary>
        public override void Destroy() {
            if (this.client != null) {
                this.client.Disconnect();
            }
        }

        /// <summary>
        /// Initializes the client by setting up the connection timeout and event handlers for various client events.
        /// </summary>
        public override void Initialize() {
            this.client = new EmbeddedClient();
            this.client.ConnectTimeoutTime = this.GetIdleTimeout();
            // Configure events
            this.client.Connected += (object sender, EventArgs e) => {
                this.connection     = this.client.Connection;
                this.OnClientConnected(this);
            };
            this.client.Disconnected += (object sender, DisconnectedEventArgs e) => {
                this.connection     = null;
                this.OnClientDisconnected(this);
            };
            this.client.ConnectionFailed += (object sender, ConnectionFailedEventArgs e) => {
                this.connection     = null;
                this.OnClientDisconnected(this);
            };
            // For PeerToPeer connections
            this.client.ClientConnected += (object sender, ClientConnectedEventArgs e) => {
                NetworkDebugger.Log(string.Format("Client connected on server [{0}] host client [{1}] with [{2}] of disconnection timeout", e.Id, this.client.Id, this.client.ConnectTimeoutTime));
            };
            this.client.ClientDisconnected += (object sender, ClientDisconnectedEventArgs e) => {
                NetworkDebugger.Log(string.Format("Client disconnected from server [{0}] host client [{1}] with [{2}] of disconnection timeout", e.Id, this.client.Id, this.client.ConnectTimeoutTime));
            };
            this.client.MessageReceived += (object sender, MessageReceivedEventArgs e) => {
                int     bufferSize  = e.Message.GetInt();
                byte[]  dataBytes   = e.Message.GetBytes(bufferSize);
                this.OnMessageReceived(this, dataBytes);
            };
        }

        /// <summary>
        /// Initializes the peer to peer server
        /// </summary>
        /// <param name="peerToPeerPort">Port where peer to peer data will be received</param>
        public void InitializePeerToPeerServer(ushort peerToPeerPort) {
            this.peerToPeerServer = new EmbeddedServer();
            this.peerToPeerServer.TimeoutTime = this.GetIdleTimeout();
            this.peerToPeerServer.ClientConnected += (object sender, ServerConnectedEventArgs e) => {
                NetworkDebugger.Log(string.Format("New client connected [{0}] from [{1}:{2}] with [{3}] of disconnection timeout", e.Client.Id, (e.Client as Transports.Udp.EmbeddedUdpConnection).RemoteEndPoint.Address.MapToIPv4().ToString(), (e.Client as Transports.Udp.EmbeddedUdpConnection).RemoteEndPoint.Port, (e.Client as Transports.Udp.EmbeddedUdpConnection).TimeoutTime));
            };
            this.peerToPeerServer.ClientDisconnected += (object sender, ServerDisconnectedEventArgs e) => {
                
            };
            this.peerToPeerServer.MessageReceived += (object sender, MessageReceivedEventArgs e) => {
                int     bufferSize  = e.Message.GetInt();
                byte[]  dataBytes   = e.Message.GetBytes(bufferSize);
                this.OnMessageReceived(this, dataBytes);
            };
            if (TransportServerBind.UseFixedAddress.Equals(TransportDefinitions.ServerBindingType)) {
                this.peerToPeerServer.Start(this.GetIp(), peerToPeerPort, 100);
            } else {
                this.peerToPeerServer.Start(peerToPeerPort, 100);
            }
        }

        /// <summary>
        /// Checks if the client is currently connected.
        /// </summary>
        /// <returns>True if the client is connected, false otherwise.</returns>
        public override bool IsConnected() {
            return ((this.connection != null) && (this.connection.IsConnected));
        }
        
        /// <summary>
        /// Processes any pending operations such as updating the client state and sending queued messages.
        /// </summary>
        public override List<byte[]> Process() {
            if (this.client != null) {
                this.client.Update();
            }
            // Update peer to peer server to receive data
            if (this.peerToPeerServer != null) {
                if (this.peerToPeerServer.IsRunning) {
                    this.peerToPeerServer.Update();
                }
            }
            // Update peer to peer clients to receive all data incomign to peers
            foreach (ITransportPeer peer in this.peers.Values) {
                if ((!peer.GetConnection<EmbeddedClient>().IsConnected) &&
                    (!peer.GetConnection<EmbeddedClient>().IsConnecting)) {
                    NetworkDebugger.Log(string.Format("Peer trying to connect to [{0}:{1}]", peer.GetAddress(), peer.GetPort()));
                    peer.GetConnection<EmbeddedClient>().Connect(string.Format("{0}:{1}", peer.GetAddress(), peer.GetPort())); // Try to stablish connection with peer                                
                }
                peer.GetConnection<EmbeddedClient>().Update();
            }
            if (this.IsConnected()) {
                this.dequeuedBuffers.Clear();
                // Send pending messages (Reliable)
                while (this.DequeueMessage(DeliveryMode.Reliable, out var dataBytes)) {
                    this.InternalSendMessageToClient(this.connection, dataBytes, DeliveryMode.Reliable);
                    this.dequeuedBuffers.Add(dataBytes);
                }
                // Send pending messages (Unreliable)
                while (this.DequeueMessage(DeliveryMode.Unreliable, out var dataBytes)) {
                    this.InternalSendMessageToClient(this.connection, dataBytes, DeliveryMode.Unreliable);
                    // Send to al other peers ( is P2P is enabled "peers" map may have peers, otherwise will not )
                    foreach (ITransportPeer peer in this.peers.Values) {
                        if (peer.IsConnected()) {
                            this.InternalSendMessageToClient(peer.GetConnection<EmbeddedClient>().Connection, dataBytes, DeliveryMode.Unreliable);
                        }
                    }
                    this.dequeuedBuffers.Add(dataBytes);
                }
            }
            return this.dequeuedBuffers;
        }
    }
}