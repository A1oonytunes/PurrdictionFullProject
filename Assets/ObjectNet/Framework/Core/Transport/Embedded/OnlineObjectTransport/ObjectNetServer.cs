using com.onlineobject.objectnet.embedded.Transports.Udp;
using System.Collections.Generic;

namespace com.onlineobject.objectnet.embedded {
    /// <summary>
    /// Represents a server that uses an embedded transport mechanism.
    /// </summary>
    public sealed class ObjectNetServer : ObjectNetTransport, ITransportServer {

        /// <summary>
        /// The embedded server instance.
        /// </summary>
        private EmbeddedServer server;

        /// <summary>
        /// A dictionary mapping embedded connections to their corresponding client objects.
        /// </summary>
        private Dictionary<EmbeddedConnection, ObjectNetClient> clients = new Dictionary<EmbeddedConnection, ObjectNetClient>();

        // Data to return to pooling
        private List<byte[]> dequeuedBuffers = new List<byte[]>();

        /// <summary>
        /// Starts the server and begins listening for incoming connections.
        /// </summary>
        /// <returns>True if the server successfully starts, false otherwise.</returns>
        public override bool Connect() {
            if (TransportServerBind.UseFixedAddress.Equals(TransportDefinitions.ServerBindingType)) {
                this.server.Start(this.GetIp(), this.GetPort(), 100);
            } else {
                this.server.Start(this.GetPort(), 100);
            }
            return this.server.IsRunning;
        }

        /// <summary>
        /// Disconnect transport system
        /// </summary>
        public override void Disconnect() {
            this.server.Stop();
        }

        /// <summary>
        /// Return if this transport will disconnect client by timeout
        /// </summary>
        /// <returns>True if enabled, otherwise false</returns>
        public override bool IsTimeoutEnabled() {
            return this.server.CanTimeout;
        }

        /// <summary>
        /// Set if this transport will disconnect client by timeout
        /// </summary>
        /// <param name="value">Time out enabled</param>
        public override void SetTimeoutEnabled(bool value) {
            this.server.CanTimeout = value;
        }


        /// <summary>
        /// Return if this transport will disconnect
        /// </summary>
        /// <returns>True if enabled, otherwise false</returns>
        public override bool IsDisconnectionEnabled() {
            return this.server.CanTimeout;
        }

        /// <summary>
        /// Set if this transport will disconnect
        /// </summary>
        /// <param name="value">Time out enabled</param>
        public override void SetDisconnectionEnabled(bool value) {
            this.server.CanTimeout = value;
        }

        /// <summary>
        /// Checks if the server is currently connected and running.
        /// </summary>
        /// <returns>True if the server is running, false otherwise.</returns>
        public override bool IsConnected() {
            return ((this.server != null) && (this.server.IsRunning));
        }

        /// <summary>
        /// Initializes the server with default settings and event handlers for client connections and messages.
        /// </summary>
        public override void Initialize() {
            this.server = new EmbeddedServer();
            this.server.TimeoutTime = this.GetIdleTimeout();
            this.server.ClientConnected += (object sender, ServerConnectedEventArgs e) => {
                this.clients.Add(e.Client, new ObjectNetClient(e.Client));
                this.clients[e.Client].SetIp((e.Client as EmbeddedUdpConnection).RemoteEndPoint.Address.MapToIPv4().ToString());
                this.clients[e.Client].SetPort((ushort)(e.Client as EmbeddedUdpConnection).RemoteEndPoint.Port);
                this.OnClientConnected(this.clients[e.Client]);
            };
            this.server.ClientDisconnected += (object sender, ServerDisconnectedEventArgs e) => {
                if (this.clients.ContainsKey(e.Client)) {
                    try {
                        this.OnClientDisconnected(this.clients[e.Client]);
                    } finally {
                        this.clients.Remove(e.Client);
                    }
                }
            };
            this.server.MessageReceived += (object sender, MessageReceivedEventArgs e) => {
                if (this.clients.ContainsKey(e.FromConnection)) {
                    int bufferSize = e.Message.GetInt();
                    byte[] dataBytes = e.Message.GetBytes(bufferSize);
                    this.OnMessageReceived(this.clients[e.FromConnection], dataBytes);
                }
            };
        }

        /// <summary>
        /// Stops the server if it is currently running.
        /// </summary>
        public override void Destroy() {
            if (this.IsConnected()) {
                this.server.Stop();
            }
        }

        /// <summary>
        /// Processes any pending operations such as sending messages and updating client states.
        /// </summary>
        public override List<byte[]> Process() {
            if (this.server != null) {
                this.server.Update();
            }
            if (this.IsConnected()) {
                try {
                    this.dequeuedBuffers.Clear();
                    // Send pending messages (Reliable)
                    while (this.DequeueMessage(DeliveryMode.Reliable, out var dataBytes)) {
                        this.InternalSendMessage(dataBytes, DeliveryMode.Reliable);
                        this.dequeuedBuffers.Add(dataBytes);
                    }
                    // Send pending messages (Unreliable)
                    while (this.DequeueMessage(DeliveryMode.Unreliable, out var dataBytes)) {
                        this.InternalSendMessage(dataBytes, DeliveryMode.Unreliable);
                        this.dequeuedBuffers.Add(dataBytes);
                    }
                } finally {
                    // Process all clients
                    foreach (ObjectNetClient connection in this.clients.Values) {
                        this.dequeuedBuffers.AddRange(connection.Process());
                    }
                }
            }
            return this.dequeuedBuffers;
        }

        /// <summary>
        /// Sends data to all connected clients.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="mode">The delivery mode (reliable or unreliable).</param>
        public override void Send(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable) {
            base.Send(data, mode);
        }

        /// <summary>
        /// Sends a message to all connected clients.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="mode">The delivery mode (reliable or unreliable).</param>
        private void InternalSendMessage(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable) {
            foreach (ObjectNetClient connection in clients.Values) {
                this.InternalSendMessageToClient(connection.GetConnection(), data, mode);
            }
        }
    }

}