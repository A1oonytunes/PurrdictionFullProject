#if UNITY_TRANSPORT_ENABLED
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;

#endif

namespace com.onlineobject.objectnet {

    /// <summary>
    /// Abstract base class for a Unity network transport layer, providing common functionality for network communication.
    /// </summary>
    public abstract class UnityTransport
#if UNITY_TRANSPORT_ENABLED
        : ITransport {
#else
        {
#endif

#if UNITY_TRANSPORT_ENABLED

        // Network driver for low-level network operations
        NetworkDriver driver;
        

        // Network pipeline for reliable communication
        NetworkPipeline realiablePipeline;

        // Network settings for configuration
        NetworkSettings settings;

        // IP address for the network connection
        string ip;

        // Port number for the network connection
        ushort port;

        /// <summary>
        /// How long the system will wait before disconnecting due to inactivity (in milliseconds).
        /// </summary>
        int idleTimeout = IDLE_TIMEOUT;

        // Queue for UDP messages to be sent
        Queue<byte[]> udpQueueToSend = new Queue<byte[]>();

        // Queue for TCP messages to be sent
        Queue<byte[]> tcpQueueToSend = new Queue<byte[]>();

        // Event handler for when a client connects
        protected Action<ITransportClient> onClientConnected;

        // Event handler for when a client disconnects
        protected Action<ITransportClient> onClientDisconnected;

        // Event handler for when a message is received
        protected Action<ITransportClient, byte[]> onMessageReceived;

        // Queue for transitioning messages between queues
        Queue<byte[]> transitionQueue = new Queue<byte[]>();

        // Flag to enable reliable communication implemented in software
        bool enableReliableBySoftware = false;

        // Buffer for TCP messages waiting for acknowledgement
        byte[] tcpBufferWaitingAck = null;

        // ID of the current message waiting for acknowledgement
        int currentWaitingAck = 0;

        // ID of the last acknowledged message
        int currentReceivedAck = 0;

        // Timeout for message acknowledgement
        float acknoledgeTimeOut = 0;

        // Constant representing a normal packet
        const byte NORMAL_PACKET = 1;

        // Constant representing an acknowledgement packet
        const byte ACKNOLEDGE_PACKET = 2;

        // Default idle timeout in milliseconds
        const int IDLE_TIMEOUT = 10000;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Public variables to change Unity Transport connection settings without the need for
        /// creating methods and wrappers for this        
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable custom reliable UDP communication implemented in software.
        /// </summary>
        public static bool EnableCustomReliable = false;

        /// <summary>
        /// How long the system will wait to establish a connection before canceling it (in milliseconds).
        /// </summary>
        public static int ConnectStablishTimeout = 500;

        /// <summary>
        /// How many attempts the system will try to connect before canceling the connection procedure.
        /// </summary>
        public static int MaxConnectAttempts = 10;


        /// <summary>
        /// Sets the network driver for the current instance.
        /// </summary>
        /// <param name="driver">The NetworkDriver to be set.</param>
        public void SetDriver(NetworkDriver driver) {
            this.driver = driver;
        }

        /// <summary>
        /// Retrieves the network driver associated with the current instance.
        /// </summary>
        /// <returns>The NetworkDriver instance.</returns>
        public NetworkDriver GetDriver() {
            return driver;
        }

        /// <summary>
        /// Sets the reliable network pipeline for the current instance.
        /// </summary>
        /// <param name="pipeline">The NetworkPipeline to be set.</param>
        public void SetRealiablePipeline(NetworkPipeline pipeline) {
            this.realiablePipeline = pipeline;
        }

        /// <summary>
        /// Retrieves the reliable network pipeline associated with the current instance.
        /// </summary>
        /// <returns>The NetworkPipeline instance.</returns>
        public NetworkPipeline GetRealiablePipeline() {
            return this.realiablePipeline;
        }


        /// <summary>
        /// Sets the IP address for the current instance.
        /// </summary>
        /// <param name="ip">The IP address to be set as a string.</param>
        public void SetIp(string ip) {
            this.ip = ip;
        }

        /// <summary>
        /// Retrieves the IP address associated with the current instance.
        /// </summary>
        /// <returns>The IP address as a string.</returns>
        public string GetIp() {
            return ip;
        }

        /// <summary>
        /// Retrieves the network port associated with the current instance.
        /// </summary>
        /// <param name="type">The type of transport port to retrieve (default is Both).</param>
        /// <returns>The network port as an unsigned short.</returns>
        public ushort GetPort(TransportPortType type = TransportPortType.Both) {
            return port;
        }

        /// <summary>
        /// Sets the network port for the current instance.
        /// </summary>
        /// <param name="port">The port number to be set.</param>
        /// <param name="type">The type of transport port to set (default is Both).</param>
        public void SetPort(ushort port, TransportPortType type = TransportPortType.Both) {
            this.port = port;
        }

        /// <summary>
        /// Sets the idle timeout duration for the current instance.
        /// </summary>
        /// <param name="time">The idle timeout duration in seconds.</param>
        public void SetIdleTimeout(int time) {
            this.idleTimeout = time;
        }

        /// <summary>
        /// Retrieves the idle timeout duration associated with the current instance.
        /// </summary>
        /// <returns>The idle timeout duration in seconds.</returns>
        public int GetIdleTimeout() {
            return this.idleTimeout;
        }


        /// <summary>
        /// Configures the transport with event handlers for client connection, disconnection, and message reception.
        /// </summary>
        public virtual void Initialize() {
            // Check user overriden preference
            this.enableReliableBySoftware = UnityTransport.EnableCustomReliable;

            if (this.enableReliableBySoftware) {
                this.settings = new NetworkSettings();
                this.settings.WithNetworkConfigParameters(connectTimeoutMS: ConnectStablishTimeout, maxConnectAttempts: MaxConnectAttempts, disconnectTimeoutMS: this.idleTimeout);
#if UNITY_2022_1_OR_NEWER
#if UNITY_WEBGL && !UNITY_EDITOR
                this.driver = NetworkDriver.Create(new WebSocketNetworkInterface(), this.settings);
#else
                this.driver = NetworkDriver.Create(new UDPNetworkInterface(), this.settings);
#endif
#else
                this.driver = NetworkDriver.Create(this.settings);
#endif
            } else {
                this.settings = new NetworkSettings();
                // this.settings.WithReliableStageParameters(windowSize: 64);
                this.settings.WithNetworkConfigParameters(connectTimeoutMS: ConnectStablishTimeout, maxConnectAttempts: MaxConnectAttempts, disconnectTimeoutMS: this.idleTimeout);
                this.driver = NetworkDriver.Create(this.settings);
            }
            this.realiablePipeline = (this.enableReliableBySoftware) ? NetworkPipeline.Null : this.GetDriver().CreatePipeline(typeof(ReliableSequencedPipelineStage));            
        }

        /// <summary>
        /// Invokes the message received event with the provided client and data if the event has subscribers.
        /// </summary>
        /// <param name="client">The client from which the message was received.</param>
        /// <param name="data">The data received from the client.</param>
        public void OnMessageReceived(ITransportClient client, byte[] data) {
            // Check if the event has any subscribers before invoking
            if (this.onMessageReceived != null) {
                this.onMessageReceived.Invoke(client, data);
            }
        }

        /// <summary>
        /// Invokes the client connected event with the provided client if the event has subscribers.
        /// </summary>
        /// <param name="client">The client that has connected.</param>
        public void OnClientConnected(ITransportClient client) {
            // Check if the event has any subscribers before invoking
            if (this.onClientConnected != null) {
                this.onClientConnected.Invoke(client);
            }
        }

        /// <summary>
        /// Invokes the client disconnected event with the provided client if the event has subscribers.
        /// </summary>
        /// <param name="client">The client that has disconnected.</param>
        public void OnClientDisconnected(ITransportClient client) {
            // Check if the event has any subscribers before invoking
            if (this.onClientDisconnected != null) {
                this.onClientDisconnected.Invoke(client);
            }
        }

        /// <summary>
        /// Configures the event handlers for client connection, disconnection, and message reception.
        /// </summary>
        /// <param name="onClientConnected">The action to perform when a client connects.</param>
        /// <param name="onClientDisconnected">The action to perform when a client disconnects.</param>
        /// <param name="onMessageReceived">The action to perform when a message is received from a client.</param>
        public void Configure(Action<ITransportClient> onClientConnected,
                              Action<ITransportClient> onClientDisconnected,
                              Action<ITransportClient, byte[]> onMessageReceived) {
            this.onClientConnected = onClientConnected;
            this.onClientDisconnected = onClientDisconnected;
            this.onMessageReceived = onMessageReceived;
        }

        /// <summary>
        /// Sends data to the client using the specified delivery mode.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="mode">The delivery mode to use for sending the data.</param>
        public virtual void Send(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable) {
            // Choose the appropriate queue based on the delivery mode
            if (DeliveryMode.Unreliable.Equals(mode)) {
                lock (this.udpQueueToSend) {
                    this.udpQueueToSend.Enqueue(data);
                }
            } else if (DeliveryMode.Reliable.Equals(mode)) {
                lock (this.tcpQueueToSend) {
                    this.tcpQueueToSend.Enqueue(data);
                }
            }
        }

        /// <summary>
        /// Reallocates a message to the front of the queue based on the delivery mode.
        /// </summary>
        /// <param name="data">The data to reallocate.</param>
        /// <param name="mode">The delivery mode of the data.</param>
        private void ReallocateMessageOnQueue(byte[] data, DeliveryMode mode) {
            Queue<byte[]> targetQueue = null;
            // Select the appropriate queue based on the delivery mode
            if (DeliveryMode.Unreliable.Equals(mode)) {
                lock (this.udpQueueToSend) {
                    targetQueue = this.udpQueueToSend;
                }
            } else if (DeliveryMode.Reliable.Equals(mode)) {
                lock (this.tcpQueueToSend) {
                    targetQueue = this.tcpQueueToSend;
                }
            }
            // Move all messages to a transition queue, enqueue the new message, then move them back
            lock (targetQueue) {
                while (targetQueue.Count > 0) {
                    transitionQueue.Enqueue(targetQueue.Dequeue());
                }
                targetQueue.Enqueue(data);
                while (transitionQueue.Count > 0) {
                    targetQueue.Enqueue(transitionQueue.Dequeue());
                }
            }
        }

        /// <summary>
        /// Attempts to dequeue a message from the appropriate queue based on the delivery mode.
        /// </summary>
        /// <param name="mode">The delivery mode of the message to dequeue.</param>
        /// <param name="outputMessage">The dequeued message, if available.</param>
        /// <returns>True if a message was successfully dequeued; otherwise, false.</returns>
        protected bool DequeueMessage(DeliveryMode mode, out byte[] outputMessage) {
            bool result = false;
            outputMessage = null;
            // Check if software-based reliable delivery is disabled or if all messages have been acknowledged
            if ((!this.enableReliableBySoftware) || (this.currentWaitingAck == this.currentReceivedAck)) {
                Queue<byte[]> targetQueue = null;
                // Select the appropriate queue and trim the unreliable queue if necessary
                if (DeliveryMode.Unreliable.Equals(mode)) {
                    targetQueue = this.udpQueueToSend;
                    while (this.udpQueueToSend.Count > TransportDefinitions.UnreliabeBufferSize) {
                        this.udpQueueToSend.Dequeue();
                    }
                } else if (DeliveryMode.Reliable.Equals(mode)) {
                    targetQueue = this.tcpQueueToSend;
                }
                result = (targetQueue.Count > 0);
                if (result) {
                    lock (targetQueue) {
                        outputMessage = targetQueue.Dequeue();
                    }
                }
            } else {
                outputMessage = this.tcpBufferWaitingAck;
            }
            return result;
        }


        /// <summary>
        /// Sends a message to a client over the network.
        /// </summary>
        /// <param name="targetConnection">The target client connection to send the message to.</param>
        /// <param name="data">The byte array containing the message data.</param>
        /// <param name="mode">The delivery mode for the message (default is Unreliable).</param>
        /// <returns>True if the message was sent successfully, otherwise false.</returns>
        protected bool InternalSendMessageToClient(IUnityTransport targetConnection, byte[] data, DeliveryMode mode = DeliveryMode.Unreliable) {
            bool result = false;
            // Check if the message should be sent now based on the delivery mode and timeout
            if ((DeliveryMode.Unreliable.Equals(mode)) ||
                (this.acknoledgeTimeOut < UnityEngine.Time.time)) {
                if (targetConnection.IsConnected()) {
                    // Begin sending the message
                    Unity.Networking.Transport.Error.StatusCode beginResult = (Unity.Networking.Transport.Error.StatusCode)this.driver.BeginSend((DeliveryMode.Unreliable.Equals(mode)) ? NetworkPipeline.Null : this.realiablePipeline, targetConnection.GetConnection(), out var writer);
                    if (Unity.Networking.Transport.Error.StatusCode.Success.Equals(beginResult)) {
                        int totalOfBytesToSend = 0;
                        // If software reliable messaging is enabled, add packet type and mode to the message
                        if (this.enableReliableBySoftware) {
                            writer.WriteByte(NORMAL_PACKET);
                            totalOfBytesToSend += sizeof(byte);
                            writer.WriteByte((byte)mode);
                            totalOfBytesToSend += sizeof(byte);
                            // If the mode is reliable, add a transaction ID
                            if (DeliveryMode.Reliable.Equals(mode)) {
                                writer.WriteInt((this.acknoledgeTimeOut > UnityEngine.Time.time) ? this.currentWaitingAck : ++this.currentWaitingAck);
                                totalOfBytesToSend += sizeof(int);
                            }
                        }
                        // Write the message data length and the data itself
                        writer.WriteInt(data.Length);
                        totalOfBytesToSend += sizeof(int);
                        writer.WriteBytes(new NativeArray<byte>(data, Allocator.Temp));
                        totalOfBytesToSend += data.Length;
                        // End the send operation and check for errors
                        int endResult = this.driver.EndSend(writer);
                        if (endResult != totalOfBytesToSend) {
                            Unity.Networking.Transport.Error.StatusCode endStatus = (Unity.Networking.Transport.Error.StatusCode)endResult;
                            if (!Unity.Networking.Transport.Error.StatusCode.Success.Equals(endStatus)) {
                                // Disconnect client when any error occurs
                                targetConnection.GetConnection().Disconnect(this.driver);
                            }
                        } else {
                            result = true;
                            // If software reliable messaging is enabled and the mode is reliable, set a new timeout
                            if (this.enableReliableBySoftware) {
                                if (DeliveryMode.Reliable.Equals(mode)) {
                                    this.acknoledgeTimeOut = (UnityEngine.Time.time + 1000);
                                    this.tcpBufferWaitingAck = data;
                                }
                            }
                        }
                    } else {
                        // Handle failed send attempt for reliable messages
                        if (DeliveryMode.Reliable.Equals(mode)) {
                            if (this.enableReliableBySoftware) {
                                this.acknoledgeTimeOut = 0;
                                this.tcpBufferWaitingAck = null;
                            }
                            this.ReallocateMessageOnQueue(data, mode);
                        }
                        // Log errors if the connection is not in progress
                        if (!targetConnection.IsConnectionInProgress()) {
                            NetworkDebugger.LogError(string.Format("Error when trying to begin send message [{0}]", beginResult));
                        }
                    }
                } else if (DeliveryMode.Reliable.Equals(mode)) {
                    // Handle disconnected client for reliable messages
                    this.acknoledgeTimeOut = 0;
                    this.tcpBufferWaitingAck = null;
                    this.ReallocateMessageOnQueue(data, mode);
                }
            }
            return result;
        }

        /// <summary>
        /// Receives a message from a client over the network.
        /// </summary>
        /// <param name="clientConnection">The client connection to receive the message from.</param>
        /// <param name="data">The output byte array containing the received message data.</param>
        /// <returns>True if a message was received successfully, otherwise false.</returns>
        protected bool InternalReceiveMessageFromClient(IUnityTransport clientConnection, out byte[] data) {
            bool result = false;
            data = null;
            // Check if the client is connected
            if (clientConnection.IsConnected()) {
                DataStreamReader stream;
                Unity.Networking.Transport.NetworkEvent.Type cmd;
                // Process all network events for the connection
                while ((cmd = this.GetDriver().PopEventForConnection(clientConnection.GetConnection(), out stream)) != Unity.Networking.Transport.NetworkEvent.Type.Empty) {
                    if (cmd == Unity.Networking.Transport.NetworkEvent.Type.Connect) {
                        // Handle new client connection
                        this.OnClientConnected(clientConnection);
                    } else if (cmd == Unity.Networking.Transport.NetworkEvent.Type.Data) {
                        // Handle incoming data
                        if (this.enableReliableBySoftware) {
                            byte packetType = stream.ReadByte();
                            if (NORMAL_PACKET == packetType) {
                                DeliveryMode mode = (DeliveryMode)stream.ReadByte();
                                if (DeliveryMode.Reliable.Equals(mode)) {
                                    int currentReceivedTransaction = stream.ReadInt(); // Get transaction ID
                                                                                       // Send ACK back
                                    this.driver.BeginSend(this.realiablePipeline, clientConnection.GetConnection(), out var writer);
                                    writer.WriteByte(ACKNOLEDGE_PACKET);
                                    writer.WriteInt(currentReceivedTransaction);
                                    this.driver.EndSend(writer);
                                }
                                int totalOfBytes = stream.ReadInt();
                                NativeArray<byte> receivedData = new NativeArray<byte>(totalOfBytes, Allocator.Temp);
                                // Read the received data
                                stream.ReadBytes(receivedData);
                                data = receivedData.ToRawBytes<byte>();
                                result = true;
                                break; // Handle only this message
                            } else if (ACKNOLEDGE_PACKET == packetType) {
                                this.currentReceivedAck = stream.ReadInt(); // Get transaction ID
                                                                            // Reset timeout if the expected ACK is received
                                if (this.currentWaitingAck == this.currentReceivedAck) {
                                    this.acknoledgeTimeOut = 0f;
                                }
                                result = false;
                            }
                        } else {
                            int totalOfBytes = stream.ReadInt();
                            NativeArray<byte> receivedData = new NativeArray<byte>(totalOfBytes, Allocator.Temp);
                            // Read the received data
                            stream.ReadBytes(receivedData);
                            data = receivedData.ToRawBytes<byte>();
                            result = true;
                            break; // Handle only this message
                        }
                    } else if (cmd == Unity.Networking.Transport.NetworkEvent.Type.Disconnect) {
                        // Handle client disconnection
                        clientConnection.SetConnection(default(NetworkConnection));
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Destroys the network transport.
        /// </summary>
        public abstract void Destroy();

        /// <summary>
        /// Processes network events.
        /// </summary>
        public abstract List<byte[]> Process();

        /// <summary>
        /// Initiates a connection to the network.
        /// </summary>
        /// <returns>True if the connection attempt was started successfully, otherwise false.</returns>
        public abstract bool Connect();

        /// <summary>
        /// Disconnect transport system
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Checks if the network transport is currently connected.
        /// </summary>
        /// <returns>True if connected, otherwise false.</returns>
        public abstract bool IsConnected();

        /// <summary>
        /// Check if timeout is disabled
        /// </summary>
        /// <returns></returns>
        public bool IsTimeoutEnabled() {
            return true;
        }

        /// <summary>
        /// Disable timout
        /// </summary>
        /// <param name="value"></param>
        public void SetTimeoutEnabled(bool value) {
            UnityEngine.Debug.LogError("Unity Transport doesn't support timeout change");
        }

        /// <summary>
        /// Check if disconnction is disabled
        /// </summary>
        /// <returns></returns>
        public bool IsDisconnectionEnabled() {
            return true;
        }

        /// <summary>
        /// Disable disconnectin
        /// </summary>
        /// <param name="value"></param>
        public void SetDisconnectionEnabled(bool value) {
            UnityEngine.Debug.LogError("Unity Transport doesn't support disconnection disable");
        }
#endif
    }
}