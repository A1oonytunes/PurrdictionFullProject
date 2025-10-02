#if STEAMWORKS_NET
using com.onlineobject.objectnet.embedded.Utils;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
#endif

namespace com.onlineobject.objectnet.steamworks {
    public abstract class SteamNetworkTransport
#if STEAMWORKS_NET
        : ITransport {
#else 
        {
#endif
#if STEAMWORKS_NET
        // IP address for the transport connection.
        string ip = "";

        // Port number for the transport connection.
        ushort port = 0;

        /// <summary>
        /// How long the system will wait before disconnecting in case of not receiving any packet.
        /// </summary>
        int idleTimeout = DEFAULT_ALIVE_TIMEOUT;

        /// <summary>
        /// Can this connectiom timeout
        /// </summary>
        bool canTimeout = true;

        /// <summary>
        /// Can this connectiom be disconnected
        /// </summary>
        bool canDisconnect = true;

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

        protected const int MAX_MESSAGES = 256;

        /// <summary>
        /// Sen data using steam socket
        /// </summary>
        /// <param name="conn">Connection to be used</param>
        /// <param name="data">Data to be send</param>
        /// <param name="deliveryMode">Mode to delivery</param>
        /// <returns></returns>
        protected EResult SendSocket(HSteamNetConnection conn, byte[] data, DeliveryMode deliveryMode = DeliveryMode.Unreliable) {
            Array.Resize(ref data, data.Length + 1);
            data[data.Length - 1] = (byte)deliveryMode;

            GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr pData = pinnedArray.AddrOfPinnedObject();
            int sendFlag = (DeliveryMode.Unreliable == deliveryMode) ? Steamworks.Constants.k_nSteamNetworkingSend_Unreliable : Steamworks.Constants.k_nSteamNetworkingSend_Reliable;
#if UNITY_SERVER
            EResult res = SteamGameServerNetworkingSockets.SendMessageToConnection(conn, pData, (uint)data.Length, sendFlag, out long _);
#else
            EResult res = SteamNetworkingSockets.SendMessageToConnection(conn, pData, (uint)data.Length, sendFlag, out long _);
#endif
            if (res != EResult.k_EResultOK) {
                Debug.LogWarning($"Send issue: {res}");
            }
            pinnedArray.Free();
            return res;
        }

        /// <summary>
        /// Process informing messages
        /// </summary>
        /// <param name="ptrs">POinter to received messsages</param>
        /// <returns></returns>
        protected (byte[], int) ProcessMessage(IntPtr ptrs) {
            SteamNetworkingMessage_t data = Marshal.PtrToStructure<SteamNetworkingMessage_t>(ptrs);
            byte[] managedArray = new byte[data.m_cbSize];
            Marshal.Copy(data.m_pData, managedArray, 0, data.m_cbSize);
            SteamNetworkingMessage_t.Release(ptrs);

            int channel = managedArray[managedArray.Length - 1];
            Array.Resize(ref managedArray, managedArray.Length - 1);
            return (managedArray, channel);
        }

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
        /// Return if this transport will disconnect client by timeout
        /// </summary>
        /// <returns>True if enabled, otherwise false</returns>
        public bool IsTimeoutEnabled() {
            return this.canTimeout;
        }

        /// <summary>
        /// Set if this transport will disconnect client by timeout
        /// </summary>
        /// <param name="value">Time out enabled</param>
        public void SetTimeoutEnabled(bool value) {
            this.canTimeout = value;
        }

        /// <summary>
        /// Return if this transport will disconnect
        /// </summary>
        /// <returns>True if enabled, otherwise false</returns>
        public bool IsDisconnectionEnabled() {
            return this.canDisconnect;
        }

        /// <summary>
        /// Set if this transport will disconnect
        /// </summary>
        /// <param name="value">Time out enabled</param>
        public void SetDisconnectionEnabled(bool value) {
            this.canDisconnect = value;
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
            this.onClientConnected = onClientConnected;
            this.onClientDisconnected = onClientDisconnected;
            this.onMessageReceived = onMessageReceived;
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
                    while (this.udpQueueToSend.Count > TransportDefinitions.UnreliabeBufferSize) {
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
        protected void InternalSendMessageToClient(SteamNetworkClient targetConnection, byte[] data, DeliveryMode mode = DeliveryMode.Unreliable) {
            if (this.IsConnected()) {
                targetConnection.Send(data, mode);
            }
        }

        /// <summary>
        /// Initializes the transport mechanism.
        /// </summary>
        public virtual void Initialize() {
#if UNITY_SERVER
            SteamGameServerNetworkingUtils.InitRelayNetworkAccess();
#else
            SteamNetworkingUtils.InitRelayNetworkAccess();
#endif
        }

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

#endif
    }
}