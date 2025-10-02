using com.onlineobject.objectnet.embedded.Utils;
using System;
using System.Collections.Generic;

namespace com.onlineobject.objectnet.embedded {
    /// <summary>
    /// Abstract base class for embedded transport mechanisms.
    /// </summary>
    public abstract class ObjectNetTransport : ITransport {

        // IP address for the transport connection.
        string ip = "";

        // Port number for the transport connection.
        ushort port = 0;

        /// <summary>
        /// How long the system will wait before disconnecting in case of not receiving any packet.
        /// </summary>
        int idleTimeout = DEFAULT_ALIVE_TIMEOUT;

        // Action to be called when a client is connected.
        Action<ITransportClient> onClientConnected;

        // Action to be called when a client is disconnected.
        Action<ITransportClient> onClientDisconnected;

        // Action to be called when a message is received from a client.
        Action<ITransportClient, byte[]> onMessageReceived;

        // Queue for UDP messages waiting to be sent.
        Queue<byte[]> udpQueueToSend = new Queue<byte[]>();

        // Queue for TCP messages waiting to be sent.
        Queue<byte[]> tcpQueueToSend = new Queue<byte[]>();
        
        // Default timeout for idle connections, with different values for debug and release builds.
#if DEBUG
        const int DEFAULT_ALIVE_TIMEOUT = (60 * 1000); // 1 minute for debug mode
#else
        const int DEFAULT_ALIVE_TIMEOUT = (10 * 1000); // 10 seconds for release mode
#endif

        /// <summary>
        /// Return if this transport will disconnect client by timeout
        /// </summary>
        /// <returns>True if enabled, otherwise false</returns>
        public abstract bool IsTimeoutEnabled();

        /// <summary>
        /// Set if this transport will disconnect client by timeout
        /// </summary>
        /// <param name="value">Time out enabled</param>
        public abstract void SetTimeoutEnabled(bool value);

        /// <summary>
        /// Return if this transport will disconnect
        /// </summary>
        /// <returns>True if enabled, otherwise false</returns>
        public abstract bool IsDisconnectionEnabled();

        /// <summary>
        /// Set if this transport will disconnect
        /// </summary>
        /// <param name="value">Time out enabled</param>
        public abstract void SetDisconnectionEnabled(bool value);

        /// <summary>
        /// Sets the IP address for the transport connection.
        /// </summary>
        /// <param name="ip">The IP address to set.</param>
        public void SetIp(string ip) {
            this.ip = ip;
        }

        /// <summary>
        /// Gets the IP address for the transport connection.
        /// </summary>
        /// <returns>The IP address.</returns>
        public string GetIp() {
            return ip;
        }

        /// <summary>
        /// Gets the port number for the transport connection.
        /// </summary>
        /// <param name="type">The type of transport port to get (defaults to both).</param>
        /// <returns>The port number.</returns>
        public ushort GetPort(TransportPortType type = TransportPortType.Both) {
            return port;
        }

        /// <summary>
        /// Sets the port number for the transport connection.
        /// </summary>
        /// <param name="port">The port number to set.</param>
        /// <param name="type">The type of transport port to set (defaults to both).</param>
        public void SetPort(ushort port, TransportPortType type = TransportPortType.Both) {
            this.port = port;
        }

        /// <summary>
        /// Sets the idle timeout duration.
        /// </summary>
        /// <param name="time">The timeout duration in milliseconds.</param>
        public void SetIdleTimeout(int time) {
            this.idleTimeout = time;
        }

        /// <summary>
        /// Gets the idle timeout duration.
        /// </summary>
        /// <returns>The timeout duration in milliseconds.</returns>
        public int GetIdleTimeout() {
            return this.idleTimeout;
        }

        /// <summary>
        /// Invokes the message received action.
        /// </summary>
        /// <param name="client">The client from which the message was received.</param>
        /// <param name="data">The received message data.</param>
        public void OnMessageReceived(ITransportClient client, byte[] data) {
            onMessageReceived?.Invoke(client, data);
        }

        /// <summary>
        /// Invokes the client connected action.
        /// </summary>
        /// <param name="client">The client that has connected.</param>
        public void OnClientConnected(ITransportClient client) {
            onClientConnected?.Invoke(client);
        }

        /// <summary>
        /// Invokes the client disconnected action.
        /// </summary>
        /// <param name="client">The client that has disconnected.</param>
        public void OnClientDisconnected(ITransportClient client) {
            onClientDisconnected?.Invoke(client);
        }

        /// <summary>
        /// Configures the transport with the specified actions for client connection, disconnection, and message reception.
        /// </summary>
        /// <param name="onClientConnected">Action to call when a client connects.</param>
        /// <param name="onClientDisconnected">Action to call when a client disconnects.</param>
        /// <param name="onMessageReceived">Action to call when a message is received.</param>
        public void Configure(Action<ITransportClient> onClientConnected,
                              Action<ITransportClient> onClientDisconnected,
                              Action<ITransportClient, byte[]> onMessageReceived) {
            this.onClientConnected              = onClientConnected;
            this.onClientDisconnected           = onClientDisconnected;
            this.onMessageReceived              = onMessageReceived;
            // Initialize embedded log
            EmbeddedLogger.Initialize((string log) => {
                NetworkDebugger.Log(log);
            }, true);
        }

        /// <summary>
        /// Sends data using the specified delivery mode.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="mode">The delivery mode (defaults to Unreliable).</param>
        public virtual void Send(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable) {
            if (DeliveryMode.Unreliable == mode) {
                lock (this.udpQueueToSend) {
                    this.udpQueueToSend.Enqueue(data);
                    while ( this.udpQueueToSend.Count > TransportDefinitions.UnreliabeBufferSize ) {
                        this.udpQueueToSend.Dequeue();
                    }
                }
            } else if (DeliveryMode.Reliable == mode) {
                lock (this.tcpQueueToSend) {
                    this.tcpQueueToSend.Enqueue(data);
                }
            }
        }

        /// <summary>
        /// Dequeues a message from the appropriate queue based on the delivery mode.
        /// </summary>
        /// <param name="mode">The delivery mode.</param>
        /// <param name="outputMessage">The dequeued message.</param>
        /// <returns>True if a message was dequeued, false otherwise.</returns>
        protected bool DequeueMessage(DeliveryMode mode, out byte[] outputMessage) {
            bool result = false;
            outputMessage = null;
            Queue<byte[]> targetQueue = null;
            if (DeliveryMode.Unreliable == mode) {
                targetQueue = this.udpQueueToSend;
            } else if (DeliveryMode.Reliable == mode) {
                targetQueue = this.tcpQueueToSend;
            }
            result = (targetQueue.Count > 0);
            if (result) {
                lock (targetQueue) {
                    outputMessage = targetQueue.Dequeue();
                }
            }
            return result;
        }

        /// <summary>
        /// Sends a message to a specific client connection.
        /// </summary>
        /// <param name="targetConnection">The target client connection.</param>
        /// <param name="data">The data to send.</param>
        /// <param name="mode">The delivery mode (defaults to Unreliable).</param>
        protected void InternalSendMessageToClient(EmbeddedConnection targetConnection, byte[] data, DeliveryMode mode = DeliveryMode.Unreliable) {
            if ( this.IsConnected()) {
                EmbeddedMessage message = EmbeddedMessage.Create((DeliveryMode.Reliable == mode) ? MessageSendMode.Reliable : MessageSendMode.Unreliable, EmbeddedPeer.MESSAGE_PACKET); 
                message.AddInt(data.Length);
                message.AddBytes(data, false);
                targetConnection.Send(message);
            }
        }

        /// <summary>
        /// Initializes the transport mechanism.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Destroys the transport mechanism.
        /// </summary>
        public abstract void Destroy();

        /// <summary>
        /// Processes any pending transport tasks.
        /// </summary>
        public abstract List<byte[]> Process();

        /// <summary>
        /// Attempts to establish a connection using the transport mechanism.
        /// </summary>
        /// <returns>True if the connection was successful, false otherwise.</returns>
        public abstract bool Connect();

        /// <summary>
        /// Disconnect transport system
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Checks if the transport is currently connected.
        /// </summary>
        /// <returns>True if connected, false otherwise.</returns>
        public abstract bool IsConnected();

    }

}