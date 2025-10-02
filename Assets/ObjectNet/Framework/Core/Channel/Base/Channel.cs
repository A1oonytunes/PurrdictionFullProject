#pragma warning disable 0168
#pragma warning disable 0219
#pragma warning disable 0414

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a communication channel, handling network connections, message transmission, and client management.
    /// </summary>
    public class Channel : ChannelBase {

        // Private fields for internal state management
        private int connectionId = 0;
        private bool started = false;
        private bool terminated = false;
        private bool connected = false;
        private bool autoReconnect = false;
        private bool encryptionEnabled = false;
        private ChannelDirection socketDirection = ChannelDirection.Server;
        private String transportClass;
        private String ip = "127.0.0.1";
        private ushort tcpPort = DEFAULT_TCP_PORT;
        private ushort udpPort = DEFAULT_UDP_PORT;
        private ushort idleTimeout = IDLE_TIMEOUT;
        private bool canTimeout = true;
        private int clientConnectedNotificationDelay = CONNECTED_NOTIFICATION_DELAY;
        private ITransport transportLayer;
        private bool alreadyConnected;
        private IClient localClient;
        private volatile int clientConnectionId = 0;
        private Dictionary<ITransportClient, NetworkClient> connectedClients = new Dictionary<ITransportClient, NetworkClient>();
        private List<IClient> connectedResultClients = new List<IClient>();

        // Callbacks for various network events
        private Action<IDataStream> onMessageReceived;
        private Action<IClient> onClientConnected;
        private Action<IClient> onClientDisconnected;
        private Action<IClient> onConnected;
        private Action<IClient> onDisconnected;
        private Action<Exception> onConnectionFailed;
        private Action<Exception> onError;
        private Func<byte[], byte[]> onEncrypt;
        private Func<byte[], byte[]> onDecrypt;

        // To enable pooling buffer
        private ArrayPool<byte> poolingBuffer = ArrayPool<byte>.Shared;

        // Constants for default values and timeouts
        const ushort    DEFAULT_TCP_PORT = 4550;
        const ushort    DEFAULT_UDP_PORT = 4550;
        const ushort    IDLE_TIMEOUT = 10000;
        const int       CONNECTED_NOTIFICATION_DELAY = 100;

        /// <summary>
        /// Initializes a new instance of the Channel class with the specified direction and transport system.
        /// </summary>
        /// <param name="direction">The direction of the channel (server or client).</param>
        /// <param name="transportSystem">The class name of the transport system to be used.</param>
        public Channel(ChannelDirection direction, string transportSystem) : base() {
            this.socketDirection    = direction;
            this.transportClass     = transportSystem;
        }

        /// <summary>
        /// Destructor to clean up any remaining threads on object finalization.
        /// </summary>
        ~Channel(){
            this.FinishPendingThreads();
        }

        /// <summary>
        /// Stops any pending threads related to the channel.
        /// </summary>
        private void FinishPendingThreads() {
            this.Stop();
        }

        /// <summary>
        /// Retrieves the connection ID of the channel.
        /// </summary>
        /// <returns>The connection ID.</returns>
        public int GetConnectionID() {
            return this.connectionId;
        }

        /// <summary>
        /// Sets the connection ID for the channel.
        /// </summary>
        /// <param name="connectionId">The connection ID to set.</param>
        public void SetConnectionID(int connectionId) {
            this.connectionId = connectionId;
            if ( this.localClient != null ) {
                (this.localClient as NetworkClient).SetConnectionId(this.connectionId);
            }
        }

        /// <summary>
        /// Retrieves the connection ID of this specific client.
        /// </summary>
        /// <returns>The client connection ID.</returns>
        public int GetClientConnectionId() {
            return this.clientConnectionId;
        }        

        /// <summary>
        /// Checks if the channel has been started.
        /// </summary>
        /// <returns>True if the channel is started, otherwise false.</returns>
        public bool IsStarted() {
            return this.started;
        }

        /// <summary>
        /// Checks if the channel has been terminated.
        /// </summary>
        /// <returns>True if the channel is terminated, otherwise false.</returns>
        public bool IsTerminated() {
            return this.terminated;
        }

        /// <summary>
        /// Sets the transport layer by specifying the class name as a string.
        /// </summary>
        /// <param name="transportClass">The class name of the transport layer to be used.</param>
        public override void SetTransport(string transportClass) {
            this.transportClass = transportClass;
        }

        /// <summary>
        /// Return the configured transport system namespace
        /// </summary>
        /// <returns>Configured transport system</returns>
        public override string GetConfiguredTransport() {
            return this.transportClass;
        }

        /// <summary>
        /// Sets the transport layer by providing an instance of an object that implements ITransport.
        /// </summary>
        /// <param name="transport">The transport layer object to be used.</param>
        public override void SetTransport(ITransport transport) {
            this.transportLayer = transport;
        }

        /// <summary>
        /// Retrieves the current transport layer object.
        /// </summary>
        /// <returns>The transport layer object that is currently in use.</returns>
        public override ITransport GetTransport() {
            return this.transportLayer;
        }

        /// <summary>
        /// Return if transport system is already initialized
        /// </summary>
        /// <returns>true if initialized.</returns>
        public override bool IsTransportInitialized() {
            return (this.transportLayer != null);
        }

        /// <summary>
        /// Determine if encryption is enabled or not
        /// </summary>
        /// <param name="value">Ture if encryption is enabled, false otherwise</param>
        public void SetEncryptionEnabled(bool value) {
            this.encryptionEnabled = value;
        }
        /// <summary>
        /// Return if encryption is enabled
        /// </summary>
        /// <returns>True is is enabled, toherwise false</returns>
        public bool IsEncryptionEnabled() {
            return this.encryptionEnabled;
        }

        /// <summary>
        /// Configure how encryption wil works
        /// 
        /// Note: This methods allow to define any encryption alghorith
        /// </summary>
        /// <param name="onEncryptData">Method used to encrypt data before sent</param>
        /// <param name="onDecryptData">Method used to decrypt data before sent</param>
        public void ConfigureEncryption(Func<byte[], byte[]> onEncryptData, Func<byte[], byte[]> onDecryptData) {
            this.onEncrypt  = onEncryptData;
            this.onDecrypt  = onDecryptData;
        }

        /// <summary>
        /// Starts the channel and initializes the transport layer.
        /// </summary>
        public override void Start() {
            // Initialize transport
            this.terminated         = false;
            this.started            = true;
            // Trace log
            NetworkDebugger.Log(String.Format("[{0}] Starting network using [{1}] transport system [{2}{3}{4}]",
                                    ((ChannelDirection.Server.Equals(this.socketDirection)) ? "Server" : "Client"),
                                    this.transportClass,
                                    this.ip,
                                    (string.IsNullOrEmpty(this.ip) ? "" : ":"),
                                    this.tcpPort));

            // Instantiate type according selected transport layer
            Type instantiatedType   = Type.GetType(this.transportClass);

            /**
             * The following piece of code shall be used in case of the transport class name can't be found
             * When this happens, the developper shall to use one of the following compilation directives
             * 
             * FORCE_OBJECTNET_TRANSPORT    ( To use ObjectNet transport system )
             * FORCE_UNITY_TRANSPORT        ( To use Unity transport system )
             * FORCE_STEAM_TRANSPORT        ( To use Steam transport system )
             */
            if (instantiatedType == null) {            
            
#if FORCE_OBJECTNET_TRANSPORT
                instantiatedType = (ChannelDirection.Server.Equals(this.socketDirection) ? 
                                            typeof(com.onlineobject.objectnet.embedded.ObjectNetServer) :
                                            typeof(com.onlineobject.objectnet.embedded.ObjectNetClient));
#endif
#if FORCE_UNITY_TRANSPORT
                instantiatedType = (ChannelDirection.Server.Equals(this.socketDirection) ?
                                            typeof(com.onlineobject.objectnet.UnityServer) :
                                            typeof(com.onlineobject.objectnet.UnityClient));
#endif
#if FORCE_STEAM_TRANSPORT
                instantiatedType = (ChannelDirection.Server.Equals(this.socketDirection) ?
                                            typeof(com.onlineobject.objectnet.steamworks.SteamNetworkServer) :
                                            typeof(com.onlineobject.objectnet.steamworks.SteamNetworkClient));
#endif
            }
            if (instantiatedType == null) {
                NetworkDebugger.LogError(String.Format("[{0}] Could no locate the selected transport type [{1}]. Try to add \"LOAD_TYPE_L2CPP\" on player definitions",
                                        ((ChannelDirection.Server.Equals(this.socketDirection)) ? "Server" : "Client"),
                                        this.transportClass));
                NetworkDebugger.LogError("Go to player preferences and add one of the following directives FORCE_OBJECTNET_TRANSPORT, FORCE_UNITY_TRANSPORT or FORCE_STEAM_TRANSPORT according to the preffered transport system");
                return;
            }
            this.transportLayer     = (Activator.CreateInstance(instantiatedType) as ITransport);
            // Confgure target
            this.transportLayer.SetIp(this.ip);
            this.transportLayer.SetPort(this.tcpPort, TransportPortType.Tcp);
            this.transportLayer.SetPort(this.udpPort, TransportPortType.Udp);
            this.transportLayer.SetIdleTimeout(this.idleTimeout);
            // Configure callbacks
            this.transportLayer.Configure((ChannelDirection.Server.Equals(this.socketDirection)) ? this.OnClientConnectedOnServerCallback       : this.OnClientConnectedCallback,
                                          (ChannelDirection.Server.Equals(this.socketDirection)) ? this.OnClientDisconnectedFromServerCallback  : this.OnClientDisconnectedCallback,
                                          this.OnMessageReceivedCallback);
            // Initialize
            this.transportLayer.Initialize();

            // I need to create a fake client to avoid NullReference exception when execute a GetClient on some objects
            this.localClient = new NetworkClient(this.connectionId);
            this.localClient.SetTransport(this.transportLayer);
            this.localClient.SetChannel(this);
            
            // Trace log
            NetworkDebugger.Log(String.Format("[{0}] Started using [{1}] transport system [{2}{3}{4}] with [{5}] of disconnection timeout",
                                    ((ChannelDirection.Server.Equals(this.socketDirection)) ? "Server" : "Client"),
                                    this.transportClass,
                                    this.ip,
                                    (string.IsNullOrEmpty(this.ip) ? "" : ":"),
                                    this.tcpPort,
                                    this.idleTimeout));
        }

        /// <summary>
        /// Stops the network server, terminates the server and disconnects all clients.
        /// </summary>
        public override void Stop() {
            this.terminated = true;
            this.started = false;
            this.Disconnect();
        }

        /// <summary>
        /// Processes the network server by calling the Process method of the transport layer if it is not null.
        /// </summary>
        public override List<byte[]> Process() {
            if (this.transportLayer != null) {
                return this.transportLayer.Process();
            } else {
                return null;
            }
        }

        /// <summary>
        /// Return a fake client for the server connection
        /// </summary>
        /// <returns>Instance of ICLient</returns>
        public override IClient GetLocalClient() {
            return this.localClient;
        }

        /// <summary>
        /// Return a list of connected client's if this player is the master on embedded mode
        /// </summary>
        /// <typeparam name="T">Type pf client that menhtod will return</typeparam>
        /// <returns>Connected clients</returns>
        public override List<IClient> GetConnectedClients() {
            this.connectedResultClients.Clear();
            foreach (IClient client in this.connectedClients.Values) {
                this.connectedResultClients.Add(client);
            }
            return this.connectedResultClients;
        }


        /// <summary>
        /// Gets the connected client with the specified connection ID.
        /// </summary>
        /// <param name="connectionId">The connection ID of the client to retrieve.</param>
        /// <returns>The NetworkClient object with the specified connection ID, or null if not found.</returns>
        public override NetworkClient GetConnectedClient(int connectionId) {
            NetworkClient result = null;
            foreach (NetworkClient client in this.connectedClients.Values) {
                if (client.GetConnectionId().Equals(connectionId)) {
                    result = client;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Registers a client with the network server.
        /// </summary>
        /// <param name="client">The client to register.</param>
        public override void RegisterClient(IClient client) {
            this.connectedClients.Add((client.GetTransport() as ITransportClient), (client as NetworkClient));
        }

        /// <summary>
        /// Unregisters a client from the network server.
        /// </summary>
        /// <param name="client">The client to unregister.</param>
        public override void UnregisterClient(IClient client) {
            if (this.connectedClients.ContainsKey((client.GetTransport() as ITransportClient))) {
                this.connectedClients.Remove((client.GetTransport() as ITransportClient));
            }
        }

        /// <summary>
        /// Checks if the network server is connected.
        /// </summary>
        /// <returns>True if the server is connected, otherwise false.</returns>
        public override bool IsConnected() {
            return (this.transportLayer != null) ? this.transportLayer.IsConnected() : false;
        }

        /// <summary>
        /// Checks if the network server has lost connection.
        /// </summary>
        /// <returns>True if the server has lost connection, otherwise false.</returns>
        public override bool IsConnectionLost() {
            this.alreadyConnected |= this.IsConnected();
            return this.alreadyConnected && !this.IsConnected();
        }

        /// <summary>
        /// Initiates a connection to the network server.
        /// </summary>
        /// <returns>True if the connection is successful, otherwise false.</returns>
        public override bool Connect() {
            this.connected         = this.ConnectTransport(this.transportLayer);
            this.alreadyConnected |= this.IsConnected();
            return this.connected;
        }

        /// <summary>
        /// Return if this connection was connected at some point into the past
        /// </summary>
        /// <returns>True if connection is stablished, false otherwise.</returns>
        public override bool AlreadyConnected() {
            return this.alreadyConnected;
        }

        /// <summary>
        /// Disconnects the network server.
        /// </summary>
        public override void Disconnect() {
            try {
                this.transportLayer.Disconnect();                
            } catch (Exception err) {
                if (this.onError != null) {
                    try {
                        this.onError.Invoke(err);
                    } catch {
                    }
                }                
            }
        }

        /// <summary>
        /// Attempts to connect the transport and handles any connection errors.
        /// </summary>
        /// <param name="transport">The transport to connect.</param>
        /// <returns>True if the connection is successful, otherwise false.</returns>
        private bool ConnectTransport(ITransport transport) {
            bool result         = false;
            try {
                result          = transport.Connect();
            } catch (Exception err) {
                if (this.onError != null) {
                    try {
                        this.onError.Invoke(err);
                    } catch {
                    }
                }
                if (this.onConnectionFailed != null) {
                    this.onConnectionFailed.Invoke(err);
                }
            }
            return result;
        }

        /// <summary>
        /// Sets the IP address for the network server.
        /// </summary>
        /// <param name="ip">The IP address to set.</param>
        public void SetIp(String ip) {
            this.ip = ip;
        }

        /// <summary>
        /// Sets the TCP port for the network server.
        /// </summary>
        /// <param name="port">The TCP port to set.</param>
        public void SetTcpPort(ushort port) {
            this.tcpPort = port;
        }

        /// <summary>
        /// Sets the UDP port for the network server.
        /// </summary>
        /// <param name="port">The UDP port to set.</param>
        public void SetUdpPort(ushort port) {
            this.udpPort = port;
        }

        /// <summary>
        /// Sets the idle timeout for the network server.
        /// </summary>
        /// <param name="timeout">The idle timeout to set.</param>
        public void SetIdleTimeout(ushort timeout) {
            this.idleTimeout = timeout;
            // Update idle timeout
            if (this.transportLayer != null) {
                this.transportLayer.SetIdleTimeout(this.idleTimeout);
            }
        }

        /// <summary>
        /// Return current idle timeout
        /// </summary>
        /// <returns>Idle timoeut</returns>
        public ushort GetIdleTimeout() {
            return this.idleTimeout;
        }
        
        /// <summary>
        /// Define if this connection will disconnect clients by time out
        /// </summary>
        /// <param name="value">Timeout value</param>
        public void SetTimeoutEnabled(bool timeout) {
            this.canTimeout = timeout;
            // Update idle timeout
            if (this.transportLayer != null) {
                this.transportLayer.SetTimeoutEnabled(this.canTimeout);
            }
        }

        /// <summary>
        /// Return if timeout is or not enabled 
        /// </summary>
        /// <returns></returns>
        public bool IsTimeoutEnabled() {
            return this.canTimeout;
        }

        /// <summary>
        /// Delay to notify client connection
        /// </summary>
        /// <param name="delay">The connection delay.</param>
        public void SetClientConnectedNotificationDelay(int delay) {
            this.clientConnectedNotificationDelay = delay;
        }


        /// <summary>
        /// Attempts to establish a connection using the provided IP address and ports.
        /// </summary>
        /// <param name="ip">The IP address to connect to.</param>
        /// <param name="tcpPort">The TCP port to use for the connection.</param>
        /// <param name="udpPort">The UDP port to use for the connection.</param>
        /// <param name="timeout">The timeout duration in seconds.</param>
        /// <returns>True if the connection was successful, otherwise false.</returns>
        public override bool Connect(string ip, ushort tcpPort, ushort udpPort, ushort timeout) {
            this.ip = ip;
            this.tcpPort = tcpPort;
            this.udpPort = udpPort;
            this.idleTimeout = timeout;
            return this.Connect();
        }

        /// <summary>
        /// Sends data to the connected client or server.
        /// </summary>
        /// <param name="data">The byte array containing the data to send.</param>
        /// <param name="mode">The delivery mode for the data transmission.</param>
        /// <param name="transport">The transport layer to use for sending data. If null, the default transport layer is used.</param>
        public override void Send(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable, ITransport transport = null) {
            ITransport targetTransport = ((transport == null) ? this.transportLayer : transport);
            int connectionId = this.clientConnectionId;
            if (targetTransport is ITransportClient) {
                ITransportClient clientTransport = (targetTransport as ITransportClient);
                if (this.connectedClients.ContainsKey(clientTransport)) {
                    connectionId = this.connectedClients[clientTransport].GetConnectionId();
                }
            }
            if (targetTransport != null) {
                targetTransport.Send(this.AnaliseSentData(this.ComposePacket(data, connectionId)), mode);
            } else {
                throw new Exception("The transport system is not initialized yet, which may mean that the channel isn't connected. You can't send data while the connection isn't established yet");
            }
        }

        /// <summary>
        /// Transmits data using the specified transport layer or the default one if none is specified.
        /// </summary>
        /// <param name="data">The byte array containing the data to transmit.</param>
        /// <param name="mode">The delivery mode for the data transmission.</param>
        /// <param name="transport">The transport layer to use for transmission. If null, the default transport layer is used.</param>
        public override void Transmit(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable, ITransport transport = null) {
            ((transport == null) ? this.transportLayer : transport).Send(this.AnaliseSentData(data), mode);
        }

        /// <summary>
        /// Registers a callback to be invoked when a message is received.
        /// </summary>
        /// <param name="onReceive">The callback to invoke with the received data stream.</param>
        public void OnMessageReceived(Action<IDataStream> onReceive) {
            this.onMessageReceived = onReceive;
        }

        /// <summary>
        /// Registers a callback to be invoked when a client connects.
        /// </summary>
        /// <param name="onConnected">The callback to invoke with the connected client.</param>
        public void OnClientConnected(Action<IClient> onConnected) {
            this.onClientConnected = onConnected;
        }

        /// <summary>
        /// Registers a callback to be invoked when a client disconnects.
        /// </summary>
        /// <param name="onDisconnect">The callback to invoke with the disconnected client.</param>
        public void OnClientDisconnected(Action<IClient> onDisconnect) {
            this.onClientDisconnected = onDisconnect;
        }

        /// <summary>
        /// Registers a callback to be invoked when the connection is successfully established.
        /// </summary>
        /// <param name="onConnected">The callback to invoke when connected.</param>
        public void OnConnected(Action<IClient> onConnected) {
            this.onConnected = onConnected;
        }

        /// <summary>
        /// Registers a callback to be invoked when the connection is lost.
        /// </summary>
        /// <param name="onDisconnect">The callback to invoke when disconnected.</param>
        public void OnDisconnected(Action<IClient> onDisconnect) {
            this.onDisconnected = onDisconnect;
        }

        /// <summary>
        /// Registers a callback to be invoked when client lost his connecton with server.
        /// </summary>
        /// <param name="onFailed">The callback to invoke with the exception that client lost connection with server.</param>
        public void OnConnectionFailed(Action<Exception> onFailed) {
            this.onConnectionFailed = onFailed;
        }

        /// <summary>
        /// Registers a callback to be invoked when an exception occurs.
        /// </summary>
        /// <param name="onError">The callback to invoke with the exception that occurred.</param>
        public void OnException(Action<Exception> onError) {
            this.onError = onError;
        }

        private byte[] DequeueBuffer(int bufferSize) {
            byte[] result = null;
            lock (this.poolingBuffer) {
                result = this.poolingBuffer.Rent(minimumLength: bufferSize);                
            }
            return result;
        }

        /// <summary>
        /// Enqueue a buffer cache into buffer pooling system
        /// </summary>
        /// <param name="buffer">Buffer to enqueue</param>
        public void EnqueueBuffer(byte[] buffer) {
            if (buffer != null) {
                lock (this.poolingBuffer) {
                    this.poolingBuffer.Return(buffer, clearArray: false);                    
                }
            }
        }

        /// <summary>
        /// Analise the output data before send
        /// </summary>
        /// <param name="data">The data to be sent in the packet.</param>
        /// <returns>The packet as a byte array.</returns>
        private byte[] AnaliseSentData(byte[] data) {
            if ( NetworkDebugger.Instance() != null ) {
                if (NetworkDebugger.Instance().IsToMonitorNetwork() == true) {
                    NetworkDebugger.LogNetworkTraffic(null, data); // Analise only output data
                }
            }
            return data;
        }

        /// <summary>
        /// Analise the input data before send
        /// </summary>
        /// <param name="data">The received data.</param>
        /// <returns>The packet as a byte array.</returns>
        private byte[] AnaliseReceivedData(byte[] data) {
            if (NetworkDebugger.Instance() != null) {
                if (NetworkDebugger.Instance().IsToMonitorNetwork() == true) {
                    NetworkDebugger.LogNetworkTraffic(data, null); // Analise only input data
                }
            }
            return data;
        }

        /// <summary>
        /// Composes a packet by prepending the connection ID to the data.
        /// </summary>
        /// <param name="data">The data to be sent in the packet.</param>
        /// <param name="targetConnectionId">The target connection ID. If 0, the default connection ID is used.</param>
        /// <returns>The composed packet as a byte array.</returns>
        private byte[] ComposePacket(byte[] data, int targetConnectionId = 0) {
            byte[] packetData = data;
            // Encript data if encription is enabled
            if (this.encryptionEnabled) {
                if (this.onEncrypt != null) {
                    packetData = this.onEncrypt.Invoke(data);
                } else {
                    throw new Exception("Encription mode is enabled but there's no encription method defined");
                }
            }
            int bufferSize              = (packetData.Length + (sizeof(int) / sizeof(byte)));
            byte[] dataToSend           = this.DequeueBuffer(bufferSize);
            int positionIndexOnBuffer   = 0;
            int connectionIdToCopy      = (targetConnectionId > 0) ? targetConnectionId : this.connectionId;
            // Copy the connection ID to the output buffer
            MemoryMarshal.Write<int>(dataToSend.AsSpan(positionIndexOnBuffer), ref connectionIdToCopy);
            positionIndexOnBuffer      += sizeof(int) / sizeof(byte);

            // Then copy the actual data to the output buffer
            foreach (byte bt in packetData) {
                dataToSend[positionIndexOnBuffer++] = bt;
            }

            return dataToSend;
        }

        /// <summary>
        /// Decomposes a packet into its constituent parts, extracting the connection ID and the data payload.
        /// </summary>
        /// <param name="client">The transport client from which the packet originated.</param>
        /// <param name="data">The raw packet data received.</param>
        /// <param name="result">Outputs whether the packet is valid based on the client's existence.</param>
        /// <param name="connectionId">Outputs the connection ID extracted from the packet.</param>
        /// <returns>The data payload of the packet without the connection ID.</returns>
        private byte[] DecomposePacket(ITransportClient client, byte[] data, out bool result, out int connectionId) {
            byte[]  packetData          = data;
            int     bufferSize          = (packetData.Length - (sizeof(int) / sizeof(byte)));
            // First i need to get connection id
            byte[]  bufferData          = this.DequeueBuffer(bufferSize);

            // Copy data buffer
            System.Buffer.BlockCopy(packetData, (sizeof(int) / sizeof(byte)), bufferData, 0, bufferSize);

            // Encript data if encription is enabled
            if (this.encryptionEnabled) {
                if (this.onDecrypt != null) {
                    bufferData = this.onDecrypt.Invoke(bufferData);
                } else {
                    throw new Exception("Encryption mode is enabled but there's no decription method defined");
                }
            }

            // Find connection id on clients
            int receivedConnectionId        = MemoryMarshal.Read<int>(packetData.AsSpan(0));
            NetworkClient receivedClient    = this.connectedClients.ContainsKey(client) ? this.connectedClients[client] : null;            
            bool isValidMessage             = (receivedClient != null);
            result                          = isValidMessage;
            connectionId                    = receivedConnectionId;
            return bufferData;
        }

        /// <summary>
        /// Callback for when a client connects to the server.
        /// </summary>
        /// <param name="client">The transport client that has connected.</param>
        private async void OnClientConnectedOnServerCallback(ITransportClient client) {
            if (this.connectedClients.Count >= NetworkManager.Instance().GetMaximumConnectedClients() ) {
                NetworkDebugger.Log(String.Format("Client disconnected due CCU Limit {0}:{1}", client.GetIp(), client.GetPort()));
                client.Disconnect();
            } else { 
                this.connectedClients.Add(client, new NetworkClient(client.GetIp(), client.GetPort(), ++this.clientConnectionId));
                NetworkDebugger.Log(String.Format("Client created [{0}] {1}:{2}", this.clientConnectionId, client.GetIp(), client.GetPort()));
                this.connectedClients[client].SetChannel(this);
                this.connectedClients[client].SetTransport(client);
                // Generate a new player for this connection
                if (NetworkManager.Instance().InRelayMode()) {
                    NetworkClient   targetClient = this.connectedClients[client];
                    IPlayer         targetPlayer = NetworkManager.Instance().GetPlayer<NetworkPlayer>(targetClient);
                    bool isMasterPlayer = ((!NetworkManager.Instance().IsLobbyControlEnabled()) && 
                                           (!NetworkManager.Instance().HasMasterPlayer((targetPlayer != null) ? targetPlayer.GetLobbyId() : (ushort)0)));
                    IPlayer player = NetworkManager.Instance().RegisterNetworkPlayer(this.connectedClients[client]);
                    player.SetMaster(isMasterPlayer); // On relay first player will be master ( On lobby, player must create a lobby first )
                    // Send current connected ID to the client
                    using (DataStream writer = new DataStream()) {
                        writer.Write(this.connectedClients[client].GetConnectionId());          // Connection ID on this client
                        writer.Write(NetworkManager.GetInstanceId());                           // Send instance ID of this game session
                        writer.Write(player.GetPlayerId());                                     // Player ID on this client
                        writer.Write(player.GetPlayerName());                                   // Player name
                        writer.Write(player.IsMaster());                                        // Is this a master player
                        writer.Write(NetworkManager.Instance().IsLobbyControlEnabled());        // Send if server is working lobby mode
                        writer.Write((byte)NetworkManager.Instance().GetServerWorkingMode());   // To chek if is relay, Authoritative or Embedded
                        this.connectedClients[client].Send(RelayServerEvents.ClientConnected, writer, DeliveryMode.Reliable);
                    }
                    // Send player creation to all network peers
                    if (NetworkManager.Instance().IsPeerToPeerEnabled()) {
                        // Send already existent players to the new player
                        foreach (NetworkPlayer playerTo in NetworkManager.Instance().GetPlayers<NetworkPlayer>((targetPlayer != null) ? targetPlayer.GetLobbyId() : (ushort)0)) {
                            if (playerTo != player) {
                                if (playerTo.IsPeerAvaiable()) {
                                    using (DataStream writer = new DataStream()) {
                                        writer.Write(playerTo.GetPlayerId());       // Player ID
                                        writer.Write(playerTo.GetClient().GetIp()); // Network client IP
                                        writer.Write(playerTo.GetPeerToPeerPort()); // Network client port
                                        writer.Write(playerTo.IsPeerAvaiable());    // Peer is avaiable to receive direct messages ?
                                        // Send message
                                        this.connectedClients[client].Send(RelayServerEvents.CreateNetworkPeer, writer, DeliveryMode.Reliable);
                                    }
                                }
                            }
                        }
                        // Define P2P port to this new player
                        player.SetPeerToPeerPort(NetworkManager.Instance().GeneratePeerToPeerPort((ushort)this.connectedClients[client].GetConnectionId()));
                        // Now send peer to peer initialization to new player
                        using (DataStream writer = new DataStream()) {
                            writer.Write(player.GetPlayerId());         // Player ID
                            writer.Write(player.GetPeerToPeerPort());   // Network client port
                            // Send message
                            this.connectedClients[client].Send(RelayServerEvents.InitializePeerToPeer, writer, DeliveryMode.Reliable);
                        }
                    
                    }
                } else {
                    await Task.Delay(this.clientConnectedNotificationDelay);
                    // Send current connected ID to the client
                    using (DataStream writer = new DataStream()) {
                        writer.Write(this.connectedClients[client].GetConnectionId());          // Connection ID on this client
                        writer.Write(NetworkManager.GetInstanceId());                           // Send instance ID of this game session
                        this.connectedClients[client].Send(InternalProtocolEvents.ClientConnected, writer, DeliveryMode.Reliable);
                    }
                }
                NetworkDebugger.Log(String.Format("Connected sent [{0}]", this.clientConnectionId));
                if ( this.onClientConnected != null ) {                
                    this.onClientConnected.Invoke(this.connectedClients[client]);
                }
            }
        }

        /// <summary>
        /// Callback for when a client connects.
        /// </summary>
        /// <param name="client">The transport client that has connected.</param>
        private void OnClientConnectedCallback(ITransportClient client) {
            this.connectedClients.Add(client, new NetworkClient(client.GetIp(), client.GetPort(), this.GetConnectionID()));
            NetworkDebugger.Log(String.Format("Client created {0}:{1}", client.GetIp(), client.GetPort()));
            this.connectedClients[client].SetChannel(this);
            this.connectedClients[client].SetTransport(client);
            if ( this.onConnected != null ) {                
                this.onConnected.Invoke(this.connectedClients[client]);
            }
        }

        /// <summary>
        /// Callback for when a client disconnects from the server.
        /// </summary>
        /// <param name="client">The transport client that has disconnected.</param>
        private void OnClientDisconnectedFromServerCallback(ITransportClient client) {
            IClient disconnectedClient = null;
            if (this.connectedClients.ContainsKey(client)) {
                disconnectedClient = this.connectedClients[client];
                this.connectedClients.Remove(client);
            }
            if ( this.onClientDisconnected != null ) {
                this.onClientDisconnected.Invoke(disconnectedClient);
            }
        }

        /// <summary>
        /// Callback for when a client disconnects.
        /// </summary>
        /// <param name="client">The transport client that has disconnected.</param>
        private void OnClientDisconnectedCallback(ITransportClient client) {
            IClient disconnectedClient = null;
            if (this.connectedClients.ContainsKey(client)) {
                disconnectedClient = this.connectedClients[client];
                this.connectedClients.Remove(client);
            }
            if ( this.onDisconnected != null ) {
                this.onDisconnected.Invoke(disconnectedClient);
            }
        }

        /// <summary>
        /// Callback for when a message is received from a client.
        /// </summary>
        /// <param name="client">The transport client from which the message was received.</param>
        /// <param name="data">The raw message data received.</param>
        private void OnMessageReceivedCallback(ITransportClient client, byte[] data) {
            if (this.onMessageReceived != null) {
                byte[]  decomposedData = this.DecomposePacket(client, this.AnaliseReceivedData(data), out var isValidPacket, out var originConnectionId);
                if (isValidPacket) {
                    IClient clientOrigin = null;
                    if ((NetworkManager.Instance().IsConnectedOnRelayServer()) && (NetworkManager.Instance().IsMasterPlayer())) { 
                        foreach (NetworkClient  clientNetwork in this.connectedClients.Values) {
                            if (clientNetwork.GetConnectionId() == originConnectionId) {
                                clientOrigin = clientNetwork;
                                break;
                            }
                        }
                        if (clientOrigin == null) {
                            if (this.connectedClients.ContainsKey(client)) {
                                clientOrigin = this.connectedClients[client];
                            }
                        }
                    } else if (this.connectedClients.ContainsKey(client)) {
                        clientOrigin = this.connectedClients[client];
                    }
                    // First find cklient
                    this.onMessageReceived(new DataStream(clientOrigin, 
                                                          NetworkManager.Instance().InRelayMode() ? data        : decomposedData, 
                                                          NetworkManager.Instance().InRelayMode() ? data.Length : decomposedData.Length));
                }
            }
        }

    }
}