#pragma warning disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// NetworkTransport is responsible for managing network connections, events, and pings.
    /// It allows for different configurations based on the connection type and channel.
    /// </summary>
    public class NetworkTransport : MonoBehaviour {

        // Serialized fields allow these private variables to be set in the Unity Editor.

        /// <summary>
        /// The type of connection to establish (Server or Client).
        /// </summary>
        [SerializeField]
        private ConnectionType connectionType = ConnectionType.Server;

        /// <summary>
        /// The transport channel to use (e.g., Bichannel, Unichannel).
        /// </summary>
        [SerializeField]
        private ChannelTransport channel = ChannelTransport.Bichannel;

        /// <summary>
        /// Whether to automatically connect on start.
        /// </summary>
        [SerializeField]
        private bool autoConnect = false;

        /// <summary>
        /// The socket used for the channel's network communication.
        /// </summary>
        [SerializeField]
        private ChannelSocket socket;

        /// <summary>
        /// The direction of the channel (Server or Client).
        /// </summary>
        [SerializeField]
        private ChannelDirection direction = ChannelDirection.Server;

        /// <summary>
        /// The frequency at which to send ping messages to measure latency.
        /// </summary>
        [SerializeField]
        private float pingFrequency = DEFAULT_PING_FREQUENCY;

        // Private fields for internal state management.

        /// <summary>
        /// Flag to enable or disable ping time measurements.
        /// </summary>
        private bool enablePingMeasure = true;

        /// <summary>
        /// The time taken for the last UDP ping.
        /// </summary>
        private float udpPingTime = 0f;

        /// <summary>
        /// The time at which the next UDP ping should be sent.
        /// </summary>
        private float nextUdpPing = 0f;

        /// <summary>
        /// The ID for the current ping transaction.
        /// </summary>
        private int pingTransactionId = 0;

        /// <summary>
        /// Determine if this is reconnection operation
        /// </summary>
        private bool reconnection = false;

        /// <summary>
        /// A dictionary to keep track of pending ping responses with their timestamps.
        /// </summary>
        private Dictionary<int, float> waitingPingResponse = new Dictionary<int, float>();

        /// <summary>
        /// A list to store samples of ping times for averaging.
        /// </summary>
        private List<float> pingAverageSamples = new List<float>();

        /// <summary>
        /// BUffer used during receive process
        /// </summary>
        List<byte[]> usedBuffers = null;

        // Event managers for different network events.
        private INetworkEventsCore coreEventsManager        = null;
        private INetworkEventsCore internalEventsManager    = null;
        private INetworkEventsCore relayEventsManager       = null;
        private INetworkEventsCore lobbyEventsManager       = null;

        // Actions for various network-related events.

        private Action<IChannel>    onServerStarted = null;
        private Action<IClient>     onClientConnected = null;
        private Action<IPlayer>     onClientConnectedOnRelayServer = null;
        private Action<IClient>     onClientDisconnected = null;
        private Action<IClient>     onConnected = null;
        private Action<IClient>     onDisconnected = null;
        private Action<Exception>   onConnectionFailed = null;
        private Action<Exception>   onLoginFailed = null;
        private Action<IClient>     onLoginSucess = null;
        private Action<IChannel>    onReceiveConnected = null;
        private Action<IChannel>    onReceiveDisconnected = null;
        private Action<IChannel>    onServerRestarted = null;
        private Action<string>      onRemoteSceneLoaded = null;
        private Action<string>      onRemoteSceneUnLoaded = null;
        private Action<DataStream>  onSendPlayerReadyOnClient;

        // Call back to make a bridge with network manage
        private Func<IPlayer,
                     bool,
                     Vector3, 
                     bool,
                     Quaternion,  
                     NetworkPlayerSpawnTime,
                     GameObject,
                     bool,
                     GameObject> spawnServerPlayer;

        private Func<IClient, 
                     int,
                     bool,
                     Vector3,
                     bool,
                     Quaternion,
                     ushort, 
                     NetworkPlayerSpawnTime,
                     GameObject,
                     GameObject> spawnClientPlayer;

        /// <summary>
        /// Delegate for the Awake event.
        /// </summary>
        public delegate void onAwake();

        /// <summary>
        /// Event triggered when the script awakes.
        /// </summary>
        private onAwake onScriptAwake;

        // Constants for default values and thresholds.

        /// <summary>
        /// The default frequency for sending pings.
        /// </summary>
        const float DEFAULT_PING_FREQUENCY = 0.100f;

        /// <summary>
        /// The maximum time before a ping is considered out of date.
        /// </summary>
        const float PING_OUTOFDATE_MAX = 1.0f;

        /// <summary>
        /// The number of ping samples to keep for averaging.
        /// </summary>
        const int PING_SAMPLES_AVERAGE = 100;

        /// <summary>
        /// The default value for an unknown ping time.
        /// </summary>
        const float UNKNOW_PING_TIME = 200f;

        /// <summary>
        /// Delay to respawn player after being destroyed when use respwn method
        /// </summary>
        const float DELAY_BEFORE_RESPAWN_AFTER_DESTROY = 1000f;

        /// <summary>
        /// Delay to wait before request server to sedn reliabel variables update
        /// </summary>
        const float DELAY_BEFORE_REQUEST_VARIABLES = 0.10f; // 100 ms

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// It initializes network connections, configures callbacks, and registers events.
        /// </summary>
        void Awake() {
            // Instantiate sockets
            this.socket = new ChannelSocket(this.direction,
                                            ChannelDirection.Server.Equals(this.direction) ? NetworkManager.Instance().GetActiveTransport().GetServer() :
                                                                                             NetworkManager.Instance().GetActiveTransport().GetClient());
            // Configure encription
            this.socket.SetEncryptionEnabled(NetworkManager.Instance().IsEncryptionEnabled());
            if (this.socket.IsEncryptionEnabled()) {
                this.socket.ConfigureEncryption(NetworkManager.Instance().GetEncryptionMethod(), NetworkManager.Instance().GetDecryptionMethod());
            }

            // Here i must define action when message received for each type of server mode
            this.socket.OnMessageReceived(( NetworkManager.Instance().InRelayMode() ) ? this.OnDataMessageReceivedOnRelay : this.OnDataMessageReceived);

            this.socket.OnException((Exception err) => {
                NetworkDebugger.LogError(err.Message);
            });
            this.socket.OnClientConnected((IClient client) => {
                NetworkDebugger.Log(String.Format("New client connected [{0}:{1}]", client.GetIp(), client.GetPort()));
                if (this.onClientConnected != null) {
                    this.onClientConnected.Invoke(client);
                }
            });
            this.socket.OnClientDisconnected((IClient client) => {
                NetworkDebugger.Log(String.Format("Client disconnected [{0}:{1}]", client.GetIp(), client.GetPort()));
                if (this.onClientDisconnected != null) {
                    this.onClientDisconnected.Invoke(client);
                }
                // Take controls of all objects under control of this client
                if (NetworkManager.Instance().InRelayMode()) {
                    // If is a relay server i need to tell to master player to take control of those objects
                    ushort lobbyId = 0;
                    NetworkPlayer originPlayer = NetworkManager.Instance().GetPlayer<NetworkPlayer>(client);
                    if (originPlayer != null) {
                        lobbyId = originPlayer.GetLobbyId();
                    }
                    NetworkPlayer masterPlayer = NetworkManager.Instance().GetMasterPlayer<NetworkPlayer>(lobbyId);
                    if (masterPlayer != null) {
                        foreach (INetworkControl elementControl in client.GetControls()) {
                            NetworkObject networkObj = (elementControl as NetworkObject);
                            if (networkObj.HasNetworkElement()) {
                                using (DataStream writer = new DataStream()) {
                                    writer.Write(networkObj.GetNetworkId());
                                    // Tell to server to take control of this object
                                    masterPlayer.GetClient().Send(RelayServerEvents.OwnerDisconnected, writer, DeliveryMode.Reliable);
                                }
                            }
                        }
                    }
                } else { 
                    foreach (INetworkControl elementControl in client.GetControls()) {
                        NetworkObject networkObj = (elementControl as NetworkObject);
                        if (networkObj.HasNetworkElement()) {
                            networkObj.SetBehaviorMode(BehaviorMode.Active);
                        }
                    }
                }
            });            
            this.socket.OnConnected((IClient client) => {
                NetworkDebugger.Log("Connected");
                if (this.onConnected != null) {
                    this.onConnected.Invoke(client);
                }
            });
            this.socket.OnDisconnected((IClient client) => {
                NetworkDebugger.Log("Disconnected");
                if (this.onDisconnected != null) {
                    this.onDisconnected.Invoke(client);
                }
                if (NetworkManager.Instance().IsClientConnection()) {
                    if (!this.socket.AlreadyConnected()) {
                        NetworkDebugger.Log("Connection attemp failed");
                        if (this.onConnectionFailed != null) {
                            this.onConnectionFailed.Invoke(new Exception("Connection attemp failed"));
                        }
                    }
                }
            });
            this.socket.OnConnectionFailed((Exception err) => {
                NetworkDebugger.Log(String.Format("Connection failed : {0}", err.Message));
                if (this.onConnectionFailed != null) {
                    this.onConnectionFailed.Invoke(err);
                }                
            });
            
            // Instantiate events manager for this component
            this.coreEventsManager      = new NetworkEventsCore();
            this.internalEventsManager  = new NetworkEventsCore();
            this.relayEventsManager     = new NetworkEventsCore();
            this.lobbyEventsManager     = new NetworkEventsCore();

            // Register all core events
            this.RegisterCoreEvent(CoreGameEvents.ObjectInstantiate,            this.OnObjectInstantiateReceived);
            this.RegisterCoreEvent(CoreGameEvents.ObjectDestroy,                this.OnObjectDestroyReceived);
            this.RegisterCoreEvent(CoreGameEvents.ObjectCreatedOnClient,        this.OnObjectCreatedOnClient);
            this.RegisterCoreEvent(CoreGameEvents.ObjectUpdate,                 this.OnObjectUpdateReceived);
            this.RegisterCoreEvent(CoreGameEvents.ObjectInputUpdate,            this.OnObjectInputUpdateReceived);
            this.RegisterCoreEvent(CoreGameEvents.PlayerIdentify,               this.OnLocalPlayerIdentify);
            this.RegisterCoreEvent(CoreGameEvents.PlayerIdentified,             this.OnLocalPlayerIdentified);
            this.RegisterCoreEvent(CoreGameEvents.PlayerSpawnedOnServer,        this.OnLocalPlayerSpawnedOnServer);
            this.RegisterCoreEvent(CoreGameEvents.PlayerRequestClientId,        this.OnLocalPlayerRequestIdOnServer);
            this.RegisterCoreEvent(CoreGameEvents.PlayerReadyOnClient,          this.OnPlayerIsReadyOnClient);
            this.RegisterCoreEvent(CoreGameEvents.SynchronizeTick,              this.OnClientSynchronizeTick); // [ Server to Client's ]
            this.RegisterCoreEvent(CoreGameEvents.NetworkSpawn,                 this.OnNetworkSpawnObject);
            this.RegisterCoreEvent(CoreGameEvents.NetworkSpawnResponse,         this.OnNetworkSpawnObjectResponse);
            this.RegisterCoreEvent(CoreGameEvents.NetworkDestroy,               this.OnNetworkDestroyObject);
            this.RegisterCoreEvent(CoreGameEvents.RequestStaticSpawnUpdate,     this.OnRequestStaticSpawnUpdate);
            this.RegisterCoreEvent(CoreGameEvents.NetworkStaticSpawnUpdate,     this.OnNetworkStaticSpawnUpdate);
            this.RegisterCoreEvent(CoreGameEvents.PlayerRespawOnClient,         this.OnPlayerRespawn);
            this.RegisterCoreEvent(CoreGameEvents.RemoteLoadScene,              this.OnRemoteLoadScene);
            this.RegisterCoreEvent(CoreGameEvents.ClientSceneLoaded,            this.OnClientSceneLoaded);
            this.RegisterCoreEvent(CoreGameEvents.RemoteSceneLoadFinished,      this.OnRemoteSceneLoadFinished);
            this.RegisterCoreEvent(CoreGameEvents.RemoteSceneLoadFail,          this.OnRemoteSceneLoadFailed);
            this.RegisterCoreEvent(CoreGameEvents.RequestRemoteSceneLoad,       this.OnRemoteLoadSceneRequested);
            this.RegisterCoreEvent(CoreGameEvents.DisconnectTimeoutChange,      this.OnDisconnectionTimeoutUpdate);
            this.RegisterCoreEvent(CoreGameEvents.RequestPlayerSpawn,           this.OnRequestPlayerSpawn);
            this.RegisterCoreEvent(CoreGameEvents.RemoteUnLoadScene,            this.OnRemoteUnLoadScene);
            this.RegisterCoreEvent(CoreGameEvents.ClientSceneUnLoaded,          this.OnClientSceneUnLoaded);
            this.RegisterCoreEvent(CoreGameEvents.RemoteSceneUnLoadFinished,    this.OnRemoteSceneUnLoadFinished);
            this.RegisterCoreEvent(CoreGameEvents.RemoteSceneUnLoadFail,        this.OnRemoteScenUnLoadFailed);
            this.RegisterCoreEvent(CoreGameEvents.RequestRemoteSceneUnLoad,     this.OnRemoteUnLoadSceneRequested);
            this.RegisterCoreEvent(CoreGameEvents.RequestSceneObjects,          this.OnLocalPlayerRequestSceneElements);
            this.RegisterCoreEvent(CoreGameEvents.RequestPlayerRespawn,         this.OnPlayerRespawnRequested);
            this.RegisterCoreEvent(CoreGameEvents.RequestVariablesUpdate,       this.OnPlayerVariablesRequested);

            // Register events [ for relay mode and client only ]
            this.RegisterRelayEvent(RelayServerEvents.ClientConnected,          this.OnConnectedOnRelayServerReceived);
            this.RegisterRelayEvent(RelayServerEvents.UpdateMasterPlayer,       this.OnUpdateMasterPlayerReceived);
            this.RegisterRelayEvent(RelayServerEvents.DisconnectedFromServer,   this.OnClientDisconnectedFromRelay);
            this.RegisterRelayEvent(RelayServerEvents.UpdateNetworkObjectId,    this.OnClientNetworkIdUpdated);
            this.RegisterRelayEvent(RelayServerEvents.ForceDisconnectClient,    this.OnForceToDisconnectClient);
            this.RegisterRelayEvent(RelayServerEvents.CreateNetworkPeer,        this.OnPeerCreatedOnRelay);
            this.RegisterRelayEvent(RelayServerEvents.DestroyNetworkPeer,       this.OnPeerDestroyedOnRelay);
            this.RegisterRelayEvent(RelayServerEvents.InitializePeerToPeer,     this.OnPeerToPeerInitialize);
            this.RegisterRelayEvent(RelayServerEvents.PlayerPeerAvaiable,       this.OnPlayerPeerAvaiable);
            this.RegisterRelayEvent(RelayServerEvents.PeerConnectionStatus,     this.OnPlayerPeerConnectionStatus);
            this.RegisterRelayEvent(RelayServerEvents.OwnerDisconnected,        this.OnOwnerDisconnected);

            // Register lobby events [ OnServerSide ]
            this.RegisterLobbyEvent(LobbyServerEvents.LobbyCreateRequest,       this.OnLobbyCreationRequest);
            this.RegisterLobbyEvent(LobbyServerEvents.LobbyFinish,              this.OnLobbyFinished);
            this.RegisterLobbyEvent(LobbyServerEvents.LobbyJoinRequest,         this.OnLobbyJoinRequest);
            this.RegisterLobbyEvent(LobbyServerEvents.LobbyPlayersListRequest,  this.OnLobbyPlayersRequest);
                
            // Register lobby events [ OnClientSide ]
            this.RegisterLobbyEvent(LobbyServerEvents.LobbyCreatedSucess,       this.OnLobbyCreationSucess);
            this.RegisterLobbyEvent(LobbyServerEvents.LobbyCreatedFailed,       this.OnLobbyCreationFailed);
            this.RegisterLobbyEvent(LobbyServerEvents.LobbyJoinSucess,          this.OnLobbyJoinSucess);
            this.RegisterLobbyEvent(LobbyServerEvents.LobbyJoinFailed,          this.OnLobbyJoinFailed);
            this.RegisterLobbyEvent(LobbyServerEvents.LobbyPlayersListRefresh,  this.OnLobbyPlayersRefresh);
            this.RegisterLobbyEvent(LobbyServerEvents.LobbyListRequest,         this.OnLobbyListRequest);
            this.RegisterLobbyEvent(LobbyServerEvents.LobbyListRefresh,         this.OnLobbyListRefresh);
            
            // Register all internal events
            this.RegisterInternalEvent(InternalProtocolEvents.ClientConnected,  this.OnConnectedReceived);
            this.RegisterInternalEvent(InternalProtocolEvents.UdpHostPing,      this.OnUdpPingReceiveReceived);
            this.RegisterInternalEvent(InternalProtocolEvents.LoginError,       this.OnLoginFailedReceived);
            this.RegisterInternalEvent(InternalProtocolEvents.LoginSucess,      this.OnLoginSucessReceived);
            
            // Register broadcast events
            NetworkManager.RegisterBroadcastEvent(CoreGameEvents.ObjectUpdate);         // Update will be send to evenryone on the same server ( or matche if exists )
            NetworkManager.RegisterBroadcastEvent(InternalGameEvents.AnimationPlay);    // Manual animation play must be executed on each instance
            NetworkManager.RegisterBroadcastEvent(InternalGameEvents.AnimationFade);    // Manual animation crossfade must be executed on each instance
            NetworkManager.RegisterBroadcastEvent(InternalGameEvents.AudioPlay);        // Audio must be executed on each instance

            // If has someone waiting for Awake i going to invoke it
            if (this.onScriptAwake != null) {
                this.onScriptAwake.Invoke();
            }

            if (this.autoConnect) {
                this.Connect();
            }
        }

        /// <summary>
        /// Execute when network transport is enabled
        /// </summary>
        private void OnEnable() {
            // Auto register
            NetworkManager.Instance().RegisterConnection(this);
            // Configure diagnostics callabacks
            NetworkDiagnostics.ConfigurePingCallback(this.GetPingAverage);
            // Need to update all existent objects to use thew new transport system
            NetworkManager.Instance().UpdateTransportOnElements(this);
        }

        /// <summary>
        /// Execute when network transport is disabled
        /// </summary>
        private void OnDisable() {
            // Unregister connection
            NetworkManager.Instance().UnregisterConnection(this);
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// It processes network socket operations and sends pings if necessary.
        /// </summary>
        public void Update() {
            if (!NetworkManager.Instance().IsRunningLogic()) {
                this.SendPingToHost(); // Send ping if applicable
            }
            // Process and return buyffer do channel pooling            
            this.usedBuffers = this.socket.Process();
            if (this.usedBuffers != null) {
                foreach (byte[] data in this.usedBuffers) {
                    this.socket.EnqueueBuffer(data);
                }
            }
        }

        /// <summary>
        /// Gets the network socket associated with this instance.
        /// </summary>
        /// <returns>The ChannelSocket instance.</returns>
        public ChannelSocket GetSocket() {
            return this.socket;
        }

        /// <summary>
        /// Checks if the network socket is connected.
        /// </summary>
        /// <returns>True if connected, false otherwise.</returns>
        public bool IsConnected() {
            return ((this.socket != null) && (this.socket.IsConnected()));
        }

        /// <summary>
        /// Sets the action to be invoked when the server is started.
        /// </summary>
        /// <param name="onServerConnected">The action to be invoked when the server is started.</param>
        public void OnServerStarted(Action<IChannel> onServerConnected) {
            this.onServerStarted = onServerConnected;
        }

        /// <summary>
        /// Sets the action to be invoked when a client is connected.
        /// </summary>
        /// <param name="onConnected">The action to be invoked when a client is connected.</param>
        public void OnConnected(Action<IClient> onConnected) {
            this.onConnected = onConnected;
        }

        /// <summary>
        /// Sets the action to be invoked when a client is disconnected.
        /// </summary>
        /// <param name="onDisconnected">The action to be invoked when a client is disconnected.</param>
        public void OnDisconnected(Action<IClient> onDisconnected) {
            this.onDisconnected = onDisconnected;
        }

        /// <summary>
        /// Sets the action to be invoked when a client is connected to the server.
        /// </summary>
        /// <param name="onConnected">The action to be invoked when a client is connected to the server.</param>
        public void OnClientConnected(Action<IClient> onConnected) {
            this.onClientConnected = onConnected;
        }

        /// <summary>
        /// Sets the action to be invoked when a client is connected to the relay server.
        /// </summary>
        /// <param name="onReplayConnected">The action to be invoked when a client is connected to the relay server.</param>
        public void OnConnectedRelayServer(Action<IPlayer> onReplayConnected) {
            this.onClientConnectedOnRelayServer = onReplayConnected;
        }

        /// <summary>
        /// Sets the action to be invoked when a client is disconnected from the server.
        /// </summary>
        /// <param name="onDisconnected">The action to be invoked when a client is disconnected from the server.</param>
        public void OnClientDisconnected(Action<IClient> onDisconnected) {
            this.onClientDisconnected = onDisconnected;
        }

        /// <summary>
        /// Sets the action to be invoked when client lost hist connection withs server.
        /// </summary>
        /// <param name="onFailed">The action to be invoked when a connection with server is lost.</param>
        public void OnConnectionFailed(Action<Exception> onFailed) {
            this.onConnectionFailed = onFailed;
        }

        
        /// <summary>
        /// Sets the action to be invoked when a login attempt fails.
        /// </summary>
        /// <param name="onLoginFailed">The action to be invoked when a login attempt fails.</param>
        public void OnLoginFailed(Action<Exception> onLoginFailed) {
            this.onLoginFailed = onLoginFailed;
        }

        /// <summary>
        /// Sets the action to be invoked when a login attempt is successful.
        /// </summary>
        /// <param name="onLoginSucess">The action to be invoked when a login attempt is successful.</param>
        public void OnLoginSucess(Action<IClient> onLoginSucess) {
            this.onLoginSucess = onLoginSucess;
        }

        /// <summary>
        /// Sets the action to be invoked when a server restart was detected by client.
        /// </summary>
        /// <param name="onServerRestarted">The action to be invoked when a server restart was detected.</param>
        public void OnServerRestarted(Action<IChannel> onServerRestarted) {
            this.onServerRestarted = onServerRestarted;
        }

        /// <summary>
        /// Sets the action to be invoked when a server tells to client that scene loading process was finished.
        /// </summary>
        /// <param name="onServerRestarted">The action to be invoked when a loadign scene was finished.</param>
        public void OnRemoteSceneLoaded(Action<string> onRemoteSceneLoaded) {
            this.onRemoteSceneLoaded = onRemoteSceneLoaded;
        }

        /// <summary>
        /// Configure methods used to spawn players
        /// </summary>
        /// <param name="spawnPlayerOnServer">Spawn host player on server side</param>
        /// <param name="spawnClientPlayer">Spawn client plater on server side</param>
        public void ConfigureSpawners(Func<IPlayer, bool, Vector3, bool, Quaternion, NetworkPlayerSpawnTime, GameObject, bool, GameObject>          spawnPlayerOnServer, 
                                      Func<IClient, int, bool, Vector3, bool, Quaternion, ushort, NetworkPlayerSpawnTime, GameObject, GameObject>   spawnClientPlayer) {
            this.spawnServerPlayer = spawnPlayerOnServer;
            this.spawnClientPlayer = spawnClientPlayer;
        }

        /// <summary>
        /// Establishes a connection with the socket if it is not already started or connected.
        /// If the direction is set to Server and the onServerStarted event is not null, it invokes the event.
        /// </summary>
        public void Connect(bool reconnection = false) {
            // Flag reconnection
            this.reconnection = reconnection;

            // Start the socket if it hasn't been started yet
            if (!this.socket.IsStarted()) {
                this.socket.Start();
            }
            // Connect the socket if it isn't already connected
            if (!this.socket.IsConnected()) {
                this.socket.Connect();
            }
            // If the channel direction is Server and the onServerStarted event is set, invoke the event
            if (ChannelDirection.Server.Equals(this.direction)) {
                this.onServerStarted?.Invoke(this.socket);
            }
        }

        /// <summary>
        /// Disconnects the socket if it is started or connected.
        /// </summary>
        public void Disconnect() {
            // Stop the socket if it has been started
            if (this.socket.IsStarted()) {
                this.socket.Stop();
            }
            // Disconnect the socket if it is connected
            if (this.socket.IsConnected()) {
                this.socket.Disconnect();
            }
        }

        /// <summary>
        /// Configures the socket with the provided IP address, TCP port, UDP port, and idle timeout.
        /// </summary>
        /// <param name="ip">The IP address to set for the socket.</param>
        /// <param name="tcpPort">The TCP port to set for the socket.</param>
        /// <param name="udpPort">The UDP port to set for the socket.</param>
        /// <param name="idleTimeout">The idle timeout to set for the socket.</param>
        /// <param name="connectionNotificationDelay">Connection notification delay.</param>
        public void Configure(String ip, ushort tcpPort, ushort udpPort, ushort idleTimeout, int connectionNotificationDelay = 100) {
            this.SetIp(ip);
            this.SetTcpPort(tcpPort);
            this.SetUdpPort(udpPort);
            this.SetIdleTimeout(idleTimeout);
            this.SetClientConnectedNotificationDelay(connectionNotificationDelay);
        }

        /// <summary>
        /// Sets the IP address for the socket.
        /// </summary>
        /// <param name="ip">The IP address to set.</param>
        public void SetIp(String ip) {
            this.socket.SetIp(ip);
        }

        /// <summary>
        /// Sets the TCP port for the socket.
        /// </summary>
        /// <param name="port">The TCP port to set.</param>
        public void SetTcpPort(ushort port) {
            this.socket.SetTcpPort(port);
        }

        /// <summary>
        /// Sets the UDP port for the socket.
        /// </summary>
        /// <param name="port">The UDP port to set.</param>
        public void SetUdpPort(ushort port) {
            this.socket.SetUdpPort(port);
        }

        /// <summary>
        /// Sets the idle timeout for the socket.
        /// </summary>
        /// <param name="timeout">The idle timeout in seconds.</param>
        public void SetIdleTimeout(ushort timeout) {
            this.socket.SetIdleTimeout(timeout);
        }

        /// <summary>
        /// Return curren idle timeout
        /// </summary>
        /// <returns>Curren condigured idle timeout</returns>
        public ushort GetIdleTimeout() {
            return this.socket.GetIdleTimeout();
        }

        /// <summary>
        /// Sets the idle timeout for the socket.
        /// </summary>
        /// <param name="timeout">The idle timeout in seconds.</param>
        public void SetTimeoutEnabled(bool timeout) {
            this.socket.SetTimeoutEnabled(timeout);
        }

        /// <summary>
        /// Return curren idle timeout
        /// </summary>
        /// <returns>Curren condigured idle timeout</returns>
        public bool IsTimeoutEnabled() {
            return this.socket.IsTimeoutEnabled();
        }

        /// <summary>
        /// Sets if this connection can be disconnected by other peer disconnected
        /// </summary>
        /// <param name="timeout">The idle timeout in seconds.</param>
        public void SetDisconnectionEnabled(bool timeout) {
            this.socket.SetTimeoutEnabled(timeout);
        }

        /// <summary>
        /// Return if this connection can be by other peer disconnected 
        /// </summary>
        /// <returns>Current disconnection enabled status</returns>
        public bool IsDisconnectionEnabled() {
            return this.socket.IsTimeoutEnabled();
        }

        /// <summary>
        /// Delay to notify client connection
        /// </summary>
        /// <param name="delay">The connection delay.</param>
        public void SetClientConnectedNotificationDelay(int delay) {
            this.socket.SetClientConnectedNotificationDelay(delay);
        }

        /// <summary>
        /// Retrieves the connection type of the socket.
        /// </summary>
        /// <returns>The current connection type as a ConnectionType enum.</returns>
        public ConnectionType GetConnectionType() {
            return this.connectionType;
        }


        /// <summary>
        /// Set the connection type for the network manager.
        /// </summary>
        /// <param name="connectionType">The type of connection to be set.</param>
        public void SetConnectionType(ConnectionType connectionType) {
            this.connectionType = connectionType;
        }

        /// <summary>
        /// Enable or disable latency measurement for the network manager.
        /// </summary>
        /// <param name="enabled">True to enable latency measurement, false to disable.</param>
        public void SetLatencyMeasure(bool enabled) {
            this.enablePingMeasure = enabled;
        }

        /// <summary>
        /// Set whether the network manager should automatically connect.
        /// </summary>
        /// <param name="autoConnect">True to enable auto-connect, false to disable.</param>
        public void SetAutoConnect(bool autoConnect) {
            this.autoConnect = autoConnect;
        }

        /// <summary>
        /// Send a method for global listener.
        /// This event will be caught by the network manager and handled globally.
        /// </summary>
        /// <param name="eventCode">The code of the event to be sent.</param>
        /// <param name="writer">The data stream containing the event information.</param>
        /// <param name="mode">The delivery mode for sending the event (default is DeliveryMode.Unreliable).</param>
        public void Send(int eventCode, DataStream writer, DeliveryMode mode = DeliveryMode.Unreliable) {
            // Note: Write on inverse order of parameters to put ID first ( event writing Event first )
            // Open space to write event code
            writer.ShiftRight(0, sizeof(int));
            writer.Write(eventCode, 0); // Write event code

            // Send buffer
            this.socket.Send(writer.GetBuffer(), mode);
        }

        /// <summary>
        /// Register a callback method to be invoked when the network manager awakes.
        /// </summary>
        /// <param name="callBack">The callback method to be registered.</param>
        public void RegisterAwake(onAwake callBack) {
            this.onScriptAwake += callBack;
        }

        /// <summary>
        /// Unregister a callback method from being invoked when the network manager awakes.
        /// </summary>
        /// <param name="callBack">The callback method to be unregistered.</param>
        public void UnRegisterAwake(onAwake callBack) {
            this.onScriptAwake -= callBack;
        }

        /// <summary>
        /// Get the average ping time for the network manager.
        /// </summary>
        /// <returns>The average ping time in milliseconds.</returns>
        public float GetPingAverage() {
            return this.udpPingTime;
        }

        /// <summary>
        /// Calculate the average ping time based on the collected samples.
        /// </summary>
        /// <returns>The calculated average ping time in milliseconds.</returns>
        private float CalculatePingTime() {
            float pingTime = 0f;
            foreach (float pingSample in this.pingAverageSamples) {
                pingTime += pingSample;
            }
            return (this.pingAverageSamples.Count > 0) ? (pingTime / this.pingAverageSamples.Count) : UNKNOW_PING_TIME;
        }

        /// <summary>
        /// Send a ping message to the host for latency measurement.
        /// </summary>
        private void SendPingToHost() {
            if (this.enablePingMeasure) {
                if (this.nextUdpPing < NetworkClock.time) {
                    if ((this.socket.IsConnected()) && (this.socket.IsStarted())) {
                        // Perform ping
                        this.nextUdpPing = (NetworkClock.time + DEFAULT_PING_FREQUENCY);
                        // Send ping message
                        using (DataStream writer = new DataStream()) {
                            writer.Write(++this.pingTransactionId);
                            this.Send(InternalProtocolEvents.UdpHostPing, writer);
                        }
                        // Store ping to wait echo and calculate travel time
                        this.waitingPingResponse.Add(this.pingTransactionId, NetworkClock.time);
                        // Remove older ping without answer
                        List<int> transactionToRemove = new List<int>();
                        foreach (var waitEntry in this.waitingPingResponse) {
                            if ((NetworkClock.time - waitEntry.Value) > PING_OUTOFDATE_MAX) {
                                transactionToRemove.Add(waitEntry.Key);
                            }
                        }
                        while (transactionToRemove.Count > 0) {
                            int removeId = transactionToRemove[0];
                            transactionToRemove.RemoveAt(0);
                            this.waitingPingResponse.Remove(removeId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if the network manager has a core event with the specified event code.
        /// </summary>
        /// <param name="eventCode">The code of the core event to check for.</param>
        /// <returns>True if the core event exists, otherwise false.</returns>
        private bool HasCoreEvent(int eventCode) {
            return this.coreEventsManager.HasEvent(eventCode);
        }

        /// <summary>
        /// Register a core event with the specified event code and callback method.
        /// </summary>
        /// <param name="eventCode">The code of the core event to be registered.</param>
        /// <param name="callBack">The callback method to be invoked for the core event.</param>
        private void RegisterCoreEvent(int eventCode, Action<IDataStream> callBack) {
            this.coreEventsManager.RegisterEvent(eventCode, callBack);
        }

        /// <summary>
        /// Invoke the callback method for the specified core event with the provided data stream.
        /// </summary>
        /// <param name="eventCode">The code of the core event to be invoked.</param>
        /// <param name="reader">The data stream containing the event information.</param>
        private void InvokeCoreEvent(int eventCode, IDataStream reader) {
            try {
                this.coreEventsManager.ExecuteEvent(eventCode, reader);
            } catch (Exception err) {
                NetworkDebugger.Log(String.Format("Error when try to execute core event [{0}]", eventCode));
                NetworkDebugger.LogError(err.Message);
            }
        }

        /// <summary>
        /// Checks if an internal event with the specified event code exists.
        /// </summary>
        /// <param name="eventCode">The event code to check for.</param>
        /// <returns>True if the event exists, otherwise false.</returns>
        private bool HasInternalEvent(int eventCode) {
            return this.internalEventsManager.HasEvent(eventCode);
        }

        /// <summary>
        /// Registers a callback action for an internal event with the specified event code.
        /// </summary>
        /// <param name="eventCode">The event code to register.</param>
        /// <param name="callBack">The callback action to execute when the event is triggered.</param>
        private void RegisterInternalEvent(int eventCode, Action<IDataStream> callBack) {
            this.internalEventsManager.RegisterEvent(eventCode, callBack);
        }

        /// <summary>
        /// Invokes the internal event with the specified event code.
        /// </summary>
        /// <param name="eventCode">The event code to invoke.</param>
        /// <param name="reader">The data stream to pass to the event's callback.</param>
        private void InvokeInternalEvent(int eventCode, IDataStream reader) {
            try {
                this.internalEventsManager.ExecuteEvent(eventCode, reader);
            } catch (Exception err) {
                NetworkDebugger.Log(String.Format("Error when try to execute internal event [{0}]", eventCode));
                NetworkDebugger.LogError(err.Message);
            }
        }

        /// <summary>
        /// Checks if a relay event with the specified event code exists.
        /// </summary>
        /// <param name="eventCode">The event code to check for.</param>
        /// <returns>True if the event exists, otherwise false.</returns>
        private bool HasRelayEvent(int eventCode) {
            return this.relayEventsManager.HasEvent(eventCode);
        }

        /// <summary>
        /// Registers a callback action for a relay event with the specified event code.
        /// </summary>
        /// <param name="eventCode">The event code to register.</param>
        /// <param name="callBack">The callback action to execute when the event is triggered.</param>
        private void RegisterRelayEvent(int eventCode, Action<IDataStream> callBack) {
            this.relayEventsManager.RegisterEvent(eventCode, callBack);
        }

        /// <summary>
        /// Invokes the relay event with the specified event code.
        /// </summary>
        /// <param name="eventCode">The event code to invoke.</param>
        /// <param name="reader">The data stream to pass to the event's callback.</param>
        private void InvokeRelayEvent(int eventCode, IDataStream reader) {
            try {
                this.relayEventsManager.ExecuteEvent(eventCode, reader);
            } catch (Exception err) {
                NetworkDebugger.Log(String.Format("Error when try to execute relay event [{0}]", eventCode));
                NetworkDebugger.LogError(err.Message);
            }
        }

        /// <summary>
        /// Checks if a lobby event with the specified event code exists.
        /// </summary>
        /// <param name="eventCode">The event code to check for.</param>
        /// <returns>True if the event exists, otherwise false.</returns>
        private bool HasLobbyEvent(int eventCode) {
            return this.lobbyEventsManager.HasEvent(eventCode);
        }

        /// <summary>
        /// Registers a callback action for a lobby event with the specified event code.
        /// </summary>
        /// <param name="eventCode">The event code to register.</param>
        /// <param name="callBack">The callback action to execute when the event is triggered.</param>
        private void RegisterLobbyEvent(int eventCode, Action<IDataStream> callBack) {
            this.lobbyEventsManager.RegisterEvent(eventCode, callBack);
        }

        /// <summary>
        /// Invokes the lobby event with the specified event code.
        /// </summary>
        /// <param name="eventCode">The event code to invoke.</param>
        /// <param name="reader">The data stream to pass to the event's callback.</param>
        private void InvokeLobbyEvent(int eventCode, IDataStream reader) {
            try {
                this.lobbyEventsManager.ExecuteEvent(eventCode, reader);
            } catch (Exception err) {
                NetworkDebugger.Log(String.Format("Error when try to execute lobby event [{0}]", eventCode));
                NetworkDebugger.LogError(err.Message);
            }
        }

        /// <summary>
        /// Checks if a user event with the specified event code exists.
        /// </summary>
        /// <param name="eventCode">The event code to check for.</param>
        /// <returns>True if the event exists, otherwise false.</returns>
        private bool HasUserEvent(int eventCode) {
            return ((NetworkManager.Instance() != null) && (NetworkManager.Events.HasEvent(eventCode)));
        }

        /// <summary>
        /// Registers a callback action for a user event with the specified event code.
        /// </summary>
        /// <param name="eventCode">The event code to register.</param>
        /// <param name="callBack">The callback action to execute when the event is triggered.</param>
        private void RegisterUserEvent(int eventCode, Action<IDataStream> callBack) {
            NetworkManager.Events.RegisterEvent(eventCode, callBack);
        }

        /// <summary>
        /// Invokes a user-defined event using the event code and data stream provided.
        /// </summary>
        /// <param name="eventCode">The code representing the specific event to invoke.</param>
        /// <param name="reader">The data stream containing the event data.</param>
        private void InvokeUserEvent(int eventCode, IDataStream reader) {
            try {
                NetworkManager.Events.ExecuteEvent(eventCode, reader);
            } catch (Exception err) {
                NetworkDebugger.Log(String.Format("Error when try to execute user event [{0}]", eventCode));
                NetworkDebugger.LogError(err.Message);
            }
        }

        /// <summary>
        /// Checks if a integration event with the specified event code exists.
        /// </summary>
        /// <param name="eventCode">The event code to check for.</param>
        /// <returns>True if the event exists, otherwise false.</returns>
        private bool HasIntegrationEvent(int eventCode) {
            return ((NetworkManager.Instance() != null) && (NetworkManager.Events.HasEvent(eventCode)));
        }

        /// <summary>
        /// Invokes a integration event using the event code and data stream provided.
        /// </summary>
        /// <param name="eventCode">The code representing the specific event to invoke.</param>
        /// <param name="reader">The data stream containing the event data.</param>
        private void InvokeIntegrationEvent(int eventCode, IDataStream reader) {
            try {
                NetworkManager.Events.ExecuteEvent(eventCode, reader);
            } catch (Exception err) {
                NetworkDebugger.Log(String.Format("Error when try to execute integration event [{0}]", eventCode));
                NetworkDebugger.LogError(err.Message);
            }
        }

        private void BroadcastEventToClients(int eventCode, IDataStream reader) {
            // Check if is a broadcast event, if yes, send to everyone on the same match ( unless itself and server )
            // Note, if has a custom event registered to execute broadcast, i'm going to check also
            if ((NetworkManager.IsBroadcastEvent(eventCode)) ||
                ((UserCustomObjectEvents.IsCustomObjectEvent(eventCode)) && (NetworkManager.IsBroadcastEvent(UserCustomObjectEvents.ToObjectEvent(eventCode))))) {
                if (ChannelDirection.Server == this.direction) {
                    if (reader is INetworkStream) {
                        // Using server socket i resend it in broadcast mode
                        IClient originClient = (reader as INetworkStream).GetClient();
                        foreach (NetworkClient client in this.GetSocket().GetConnectedClients()) {
                            if (originClient != client) {
                                client.Send(reader.GetBuffer(), NetworkManager.Instance().GetUpdateMode());
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the reception of a data message, processing it based on the event code and executing the corresponding event.
        /// </summary>
        /// <param name="reader">The data stream containing the message data.</param>
        private void OnDataMessageReceived(IDataStream reader) {
            try {
                int eventCode = reader.Read<int>(); // Extract event code
                // Check if is a broadcast event, if yes, send to everyone on the same match ( unless itself and server )
                // Note, if has a custom event registered to execute broadcast, i'm going to check also
                this.BroadcastEventToClients(eventCode, reader);

                if (InternalGameEvents.ObjectEvent == eventCode) {
                    // On this case i'm going to check if is some event to arrive on some NetworkObject                    
                    int objectEventCode = reader.Read<int>(); // Extract object ID
                    int networkObjectId = reader.Read<int>(); // Extract object internal code
                    if (NetworkManager.Container.IsRegistered(networkObjectId)) {
                        INetworkElement networkObject = NetworkManager.Container.GetElement(networkObjectId);
                        if (networkObject.HasEvent(objectEventCode)) {
                            networkObject.InvokeEvent(objectEventCode, reader);
                            // Check if is a broadcast event, if yes, send to everyone on the same match ( unless itself and server )
                            // Note, if has a custom event registered to execute broadcast, i'm going to check also
                            this.BroadcastEventToClients(objectEventCode, reader);
                        }
                    } else {
                        // Unknow event arrived
                        NetworkDebugger.LogWarning(String.Format("Event [{0}] arrived for unknow source [{0}]", objectEventCode, networkObjectId));
                    }
                } else if (CoreGameEvents.IsCoreEvent(eventCode)) {
                    if (this.HasCoreEvent(eventCode)) {
                        this.InvokeCoreEvent(eventCode, reader);
                    }
                } else if (InternalProtocolEvents.IsInternalEvent(eventCode)) {
                    if (this.HasInternalEvent(eventCode)) {
                        this.InvokeInternalEvent(eventCode, reader);
                    }
                } else if (RelayServerEvents.IsRelayEvent(eventCode)) {
                    if (this.HasRelayEvent(eventCode)) {
                        this.InvokeRelayEvent(eventCode, reader);
                    }
                } else if (LobbyServerEvents.IsLobbyEvent(eventCode)) {
                    if (this.HasLobbyEvent(eventCode)) {
                        this.InvokeLobbyEvent(eventCode, reader);
                    }
                } else if (UserCustomEvents.IsUserCustomEvent(eventCode)) {
                    if (this.HasUserEvent(eventCode)) {
                        this.InvokeUserEvent(eventCode, reader);
                    }
                } else if (IntegrationEvent.IsIntegrationEvent(eventCode)) {
                    if (this.HasIntegrationEvent(eventCode)) {
                        this.InvokeIntegrationEvent(eventCode, reader);
                    }
                } else if (NetworkManager.Container.HasEvent(eventCode)) {
                    NetworkManager.Container.InvokeEvent(eventCode, reader);
                } else if (UserCustomObjectEvents.IsCustomObjectEvent(eventCode)) {
                    // On this case i'm going to check if is some event to arrive on some NetworkObject                    
                    int networkObjectId = reader.Read<int>(); // Extract object internal code
                    if (NetworkManager.Container.IsRegistered(networkObjectId)) {
                        INetworkElement networkObject = NetworkManager.Container.GetElement(networkObjectId);
                        if (networkObject != null) {
                            networkObject.InvokeEvent(eventCode, reader);
                        }
                    } else if (UserCustomObjectEvents.IsRemoteMethod(eventCode)) {
                        NetworkDebugger.LogWarning(String.Format("Remote method was executed over an not existent network object [{0}]. This may happens when remote method is executed for a detroyed object.", eventCode, networkObjectId));
                    } else {
                        NetworkDebugger.LogWarning(String.Format("Event [{0}] arrived for unknow object [{1}]. This may happens when remote method is executed for a detroyed object.", eventCode, networkObjectId));
                    }
                } else if (InternalGameEvents.IsGameEvent(eventCode)) {
                    // On this case i'm going to check if is some event to arrive on some NetworkObject                    
                    int networkObjectId = reader.Read<int>(); // Extract object internal code
                    if (NetworkManager.Container.IsRegistered(networkObjectId)) {
                        INetworkElement networkObject = NetworkManager.Container.GetElement(networkObjectId);
                        if (networkObject != null) {
                            networkObject.InvokeEvent(eventCode, reader);
                        }
                    } else {
                        // Unknow event arrived
                        NetworkDebugger.LogError(String.Format("Event [{0}] arrived for unknow object [{1}]", eventCode, networkObjectId));
                    }
                } else {
                    // Unknow event arrived UserCustomEvents
                    NetworkDebugger.LogError(String.Format("Unknow event arrived [{0}]", eventCode));
                }
            } finally {
                // Trigger message received event
                NetworkManager.Instance().OnMessageReceivedEvent(reader);
            }
        }

        /// <summary>
        /// Handles the reception of a data message on a relay server, processing it based on the event code and executing the corresponding event.
        /// </summary>
        /// <param name="reader">The data stream containing the message data.</param>
        private void OnDataMessageReceivedOnRelay(IDataStream reader) {
            int connectionIdOnPacket = reader.Read<int>(); // Escape from connection ID
            int eventCode = reader.Read<int>(); // Extract event code
            if (eventCode == -2 ) {
                Debug.Log("");
            }
            // Check if is a broadcast event, if yes, send to everyone on the same match ( unless itself and server )
            if ((NetworkManager.IsBroadcastEvent(eventCode)) ||
                ((UserCustomObjectEvents.IsCustomObjectEvent(eventCode)) && (NetworkManager.IsBroadcastEvent(UserCustomObjectEvents.ToObjectEvent(eventCode))))) {
                if (ChannelDirection.Server == this.direction) {
                    if (reader is INetworkStream) {
                        // Using server socket i resend it in broadcast mode
                        IClient originClient = (reader as INetworkStream).GetClient();
                        if (NetworkManager.Instance().IsLobbyControlEnabled()) {
                            NetworkPlayer originPlayer = NetworkManager.Instance().GetPlayer<NetworkPlayer>(originClient);
                            if (originPlayer != null) {
                                if (originPlayer.GetLobbyId() > 0) {
                                    foreach (NetworkPlayer player in NetworkManager.Instance().GetPlayers<NetworkPlayer>(originPlayer.GetLobbyId())) {
                                        if (originClient != player.GetClient()) {
                                            if (NetworkManager.Instance().IsPeerToPeerEnabled()) {
                                                if (!player.IsPeerAvaiable()) {
                                                    player.GetClient().Transmit(reader.GetBuffer(), NetworkManager.Instance().GetUpdateMode());
                                                }
                                            } else {
                                                player.GetClient().Transmit(reader.GetBuffer(), NetworkManager.Instance().GetUpdateMode());
                                            }
                                        }
                                    }
                                }
                            } else {
                                // In this case a message from a unknow source was arrived, i need to remove it from my list
                                NetworkClient orphanClient = (originClient as NetworkClient);
                                // Send a message to disconnect this client
                                using (DataStream writer = new DataStream()) {
                                    writer.Write(orphanClient.GetConnectionId()); // Send network id back
                                    originClient.Send(RelayServerEvents.ForceDisconnectClient, writer, DeliveryMode.Reliable); // Send message
                                }
                            }
                        } else {
                            foreach (NetworkClient client in this.GetSocket().GetConnectedClients()) {
                                if (originClient != client) {
                                    if (NetworkManager.Instance().IsPeerToPeerEnabled()) {
                                        NetworkPlayer targetPlayer = NetworkManager.Instance().GetPlayer<NetworkPlayer>(client);
                                        if (!targetPlayer.IsPeerAvaiable()) {
                                            client.Transmit(reader.GetBuffer(), NetworkManager.Instance().GetUpdateMode());
                                        }
                                    } else {
                                        client.Transmit(reader.GetBuffer(), NetworkManager.Instance().GetUpdateMode());
                                    }
                                }
                            }
                        }
                    }
                }
            } else if (RelayServerEvents.IsRelayEvent(eventCode)) {
                if (this.HasRelayEvent(eventCode)) {
                    this.InvokeRelayEvent(eventCode, reader);
                }
            } else if (LobbyServerEvents.IsLobbyEvent(eventCode)) {
                if (this.HasLobbyEvent(eventCode)) {
                    this.InvokeLobbyEvent(eventCode, reader); // Then invoke event
                }
            } else {
                ushort lobbyId = 0;
                if (NetworkManager.Instance().IsLobbyControlEnabled()) {
                    IClient         originClient = (reader as INetworkStream).GetClient();
                    NetworkPlayer   originPlayer = NetworkManager.Instance().GetPlayer<NetworkPlayer>(originClient);
                    // Get lobby id to filter send
                    if (originPlayer != null) {
                        lobbyId = originPlayer.GetLobbyId();
                    } else {
                        // In this case a message from a unknow source was arrived, i need to remove it from my list
                        NetworkClient orphanClient = (originClient as NetworkClient);
                        // Send a message to disconnect this client
                        using (DataStream writer = new DataStream()) {
                            writer.Write(orphanClient.GetConnectionId()); // Send network id back
                            originClient.Send(RelayServerEvents.ForceDisconnectClient, writer, DeliveryMode.Reliable); // Send message
                        }
                        return;
                    }
                }
                if (NetworkManager.Instance().HasMasterPlayer(lobbyId)) {
                    IClient         messagesClientOrigin    = (reader as INetworkStream).GetClient();
                    NetworkPlayer   masterPlayer            = NetworkManager.Instance().GetMasterPlayer<NetworkPlayer>(lobbyId);
                    // If message came from master must be redirect to target
                    if (masterPlayer.GetClient() == messagesClientOrigin) {
                        // Extract target destination
                        int packetConnectionId = connectionIdOnPacket;
                        if (packetConnectionId > 0) {
                            NetworkClient targetClient = this.GetSocket().GetConnectedClient(packetConnectionId);
                            if (targetClient != null) {
                                if (messagesClientOrigin != targetClient) {
                                    targetClient.Transmit(reader.GetBuffer(), DeliveryMode.Reliable); // Send to target player
                                } else {
                                    if (NetworkManager.Instance().IsLobbyControlEnabled()) {
                                        NetworkPlayer originPlayer = NetworkManager.Instance().GetPlayer<NetworkPlayer>(messagesClientOrigin);
                                        if (originPlayer.GetLobbyId() > 0) {
                                            foreach (NetworkPlayer player in NetworkManager.Instance().GetPlayers<NetworkPlayer>(originPlayer.GetLobbyId())) {
                                                if (messagesClientOrigin.Equals(player.GetClient()) == false) {
                                                    player.GetClient().Transmit(reader.GetBuffer(), DeliveryMode.Reliable); // Send to target player
                                                }
                                            }
                                            // Send to other else players
                                            foreach (NetworkPlayer player in NetworkManager.Instance().GetPlayers<NetworkPlayer>(originPlayer.GetLobbyId())) { 
                                                if (messagesClientOrigin.Equals(player.GetClient()) == false) {
                                                    player.GetClient().Transmit(reader.GetBuffer(), DeliveryMode.Reliable); // Send to target player
                                                }
                                            }
                                        }
                                    } else {
                                        // Send to other else players
                                        foreach (NetworkClient sendClient in this.GetSocket().GetConnectedClients()) {
                                            if (messagesClientOrigin.Equals(sendClient) == false) {
                                                sendClient.Transmit(reader.GetBuffer(), DeliveryMode.Reliable); // Send to target player
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } else {
                        // Otherwise will be re-transmit to the master without putting client data
                        masterPlayer.GetClient().Transmit(reader.GetBuffer(), DeliveryMode.Reliable);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the instantiation of an object received over the network.
        /// </summary>
        /// <param name="reader">The data stream containing the instantiation information.</param>
        private void OnObjectInstantiateReceived(IDataStream reader) {
            // Extarct client
            IClient client              = (reader as INetworkStream).GetClient();
            // Start to extract parameters
            int objectNetworkId         = reader.Read<int>();       // Extract network ID of this object
            int networkPrefabId         = reader.Read<int>();       // Extract prefab ID
            bool objectAutoSync         = reader.Read<bool>();      // Extract if this object must be synched with server
            bool sendFeedback           = reader.Read<bool>();      // Need to send feedback ???
            bool isPlayer               = reader.Read<bool>();      // Is this object an instance of any player ?
            ushort objectPlayerId       = reader.Read<ushort>();    // player id
            ushort objectPlayerIndex    = reader.Read<ushort>();    // player index
            Vector3 prefabPosition      = reader.Read<Vector3>();   // Position
            Vector3 prefabRotation      = reader.Read<Vector3>();   // Rotation
            Vector3 prefabScale         = reader.Read<Vector3>();   // Scale
            // Then instantiate prefab on local
            NetworkManager.Instance().InstantiateOnClient(networkPrefabId, prefabPosition, prefabRotation, prefabScale, objectNetworkId, objectAutoSync, sendFeedback, isPlayer, client, objectPlayerId, objectPlayerIndex);
            // Must keep network ID updated in case of this player became master
            if (NetworkManager.Instance().IsToUseCustomNetworkId() == false) {
                NetworkManager.Container.UpdateNetworkId(objectNetworkId);
            }
        }

        /// <summary>
        /// Handles the spawning of an object on the network.
        /// </summary>
        /// <param name="reader">The data stream containing the spawn information.</param>
        private void OnNetworkSpawnObject(IDataStream reader) {
            // Extarct client
            IClient client          = (reader as INetworkStream).GetClient();
            // Start to extract parameters
            int     connectionID    = reader.Read<int>();
            uint    transactionId   = reader.Read<uint>();
            string  objectPrefabID  = reader.Read<string>();
            Vector3 prefabPosition  = reader.Read<Vector3>();   // Position
            Vector3 prefabRotation  = reader.Read<Vector3>();   // Rotation
            Vector3 prefabScale     = reader.Read<Vector3>();   // Scale
            // Locate prefab on refab database
            NetworkPrefabEntry   prefabEntry = NetworkManager.Instance().GetNetworkPrefabEntry(objectPrefabID);
            if (prefabEntry != null) {
                GameObject prefabToInstantiate = prefabEntry.GetPrefab();
                // Then instantiate prefab on local
                if (prefabToInstantiate != null) {
                    GameObject instance = GameObject.Instantiate(prefabToInstantiate, prefabPosition, Quaternion.Euler(prefabRotation));
                    instance.transform.localScale = prefabScale;
                    // Attach script detection to identify player how instantiate this object
                    NetworkObjectReference objectReferenceScript = null;
                    if (NetworkManager.Instance().IsOwnerShipUsingPrefab()) {
                        if (OwnerShipAccessLevel.ClientOnly == prefabEntry.GetOwnerShipAccessLevel()) {
                            objectReferenceScript = instance.AddComponent<NetworkObjectReference>();
                            objectReferenceScript.Configure(client, connectionID);
                        }
                    }
                    // Check if need to send spawn feedback
                    if (transactionId > 0) {
                        // Attach component if haven't
                        if (objectReferenceScript == null) {
                            objectReferenceScript = instance.AddComponent<NetworkObjectReference>();
                            objectReferenceScript.Configure(client, connectionID);
                        }
                        objectReferenceScript.SetTransactionId(transactionId);
                    }
                } else {
                    NetworkDebugger.LogError("Network Spawn Object failed : Prefab is not assigend on network prefab \"{0}\"", objectPrefabID);
                }
            } else {
                NetworkDebugger.LogError("Network Spawn Object failed : Prefab entry \"{0}\" doest not exists", objectPrefabID);
            }
        }

        /// <summary>
        /// Handles the spawning of an object on the network repsonse.
        /// </summary>
        /// <param name="reader">The data stream containing the spawn response information.</param>
        private void OnNetworkSpawnObjectResponse(IDataStream reader) {
            // Extarct client
            IClient client = (reader as INetworkStream).GetClient();
            // Start to extract parameters
            uint    transactionId       = reader.Read<uint>();
            int     elementNetworkId    = reader.Read<int>();
            if (NetworkGameObject.IsWaitingAsyncResultWait(transactionId)) {
                // Check if object exists in container
                if (NetworkManager.Container.HasElement(elementNetworkId)) {
                    // Tell to NetworkGameObject that this spawn was finished
                    NetworkGameObject.RegisterAsyncResult(transactionId, NetworkManager.Container.GetElement(elementNetworkId).GetGameObject());
                }
            }
        }

        /// <summary>
        /// Handles the despawning of an object on the network.
        /// </summary>
        /// <param name="reader">The data stream containing the spawn information.</param>
        private void OnNetworkDestroyObject(IDataStream reader) {
            // Extarct client
            IClient client = (reader as INetworkStream).GetClient();
            // Start to extract parameters
            int connectionID    = reader.Read<int>();
            int networkObjectID = reader.Read<int>();
            // Locate prefab on refab database
            INetworkElement networkElement = NetworkManager.Container.GetElement(networkObjectID);
            if (networkElement != null) {
                GameObject networkObject = networkElement.GetGameObject();
                // Then instantiate prefab on local
                if (networkObject != null) {
                    Destroy(networkObject);
                } else {
                    NetworkDebugger.LogError("Network Destroy Object failed : Object \"{0}\" has no game object to destroy", networkObjectID);
                }
            } else {
                NetworkDebugger.LogError("Network Destroy Object failed : Object \"{0}\" doesn't exists", networkObjectID);
            }
        }

        /// <summary>
        /// Request server to send status of all inscene object updated
        /// </summary>
        /// <param name="reader">The data stream containing the spawn response information.</param>
        private void OnRequestStaticSpawnUpdate(IDataStream reader) {
            this.NotifyNetworkInSceneUpdate((reader as INetworkStream).GetClient());
        }

        /// <summary>
        /// Handles the inscene objects spawning 
        /// </summary>
        /// <param name="reader">The data stream containing the spawn response information.</param>
        private void OnNetworkStaticSpawnUpdate(IDataStream reader) {
            // Extarct client
            IClient client = (reader as INetworkStream).GetClient();
            // Start to extract parameters
            int inScenePrefabsCount = reader.Read<int>();
            // Collect all objects ID's
            List<int> inSceneObjectsOnMaster = new List<int>();
            while (inScenePrefabsCount-- > 0) {
                inSceneObjectsOnMaster.Add(reader.Read<int>());
            }
            List<int> inSceneIds = new List<int>(NetworkManager.Instance().GetInSceneObjectsIds());
            // Now i'm going to destroy objects that isn't on list
            foreach (int inSceneNetworId in inSceneObjectsOnMaster) {
                if (inSceneIds.Contains(inSceneNetworId)) {
                    inSceneIds.Remove(inSceneNetworId);
                }
            }
            // Object remaning will be destroyed locally
            while (inSceneIds.Count > 0) {
                GameObject objToRemove = NetworkManager.Instance().GetInSceneObject(inSceneIds[0], true);
                inSceneIds.RemoveAt(0);
                if (objToRemove != null) {
                    NetworkManager.Instance().UnRegisterInSceneObject(objToRemove);
                    Destroy(objToRemove);
                }
            }

        }

        /// <summary>
        /// Handles object release when owner client was disconnected during relay mode
        /// </summary>
        /// <param name="reader">The data stream containing the disconnect information.</param>
        private void OnOwnerDisconnected(IDataStream reader) {
            // Extarct client
            IClient client = (reader as INetworkStream).GetClient();
            // Extract object network ID
            int objectNetworkID = reader.Read<int>();
            // Locate object on objects instance
            INetworkElement objectInstance = NetworkManager.Container.GetElement(objectNetworkID);
            if (objectInstance != null) {
                (objectInstance.GetNetworkObject() as NetworkObject).SetBehaviorMode(BehaviorMode.Active);
            }
        }

        /// <summary>
        /// Handles the destruction of an object received over the network.
        /// </summary>
        /// <param name="reader">The data stream containing the destruction information.</param>
        private void OnObjectDestroyReceived(IDataStream reader) {
            // Start to extract parameters
            int objectNetworkId = reader.Read<int>(); // Extract network ID of this object
            if (NetworkManager.Container.IsRegistered(objectNetworkId)) {
                INetworkElement element = NetworkManager.Instance().GetObjectOnClient<INetworkElement>(objectNetworkId);
                if ((element.IsPlayer() == true) && (NetworkManager.Instance().IsMasterPlayer())) {
                    // Get objects reference
                    NetworkObject objectReference = element.GetGameObject().GetComponent<NetworkObject>();
                    // Locate client and player object
                    IClient networkClient = objectReference.GetClient();
                    IPlayer networkPlayer = NetworkManager.Instance().GetPlayer<NetworkPlayer>(networkClient);
                    // Destroy object
                    NetworkManager.Instance().DestroyOnClient(objectNetworkId);
                    // Remove player
                    NetworkManager.Instance().UnregisterNetworkPlayer(networkPlayer);
                    // Remove client
                    if (NetworkManager.Instance().IsConnectedOnRelayServer()) {
                        if (NetworkManager.Instance().IsConnectedOnLobbyServer()) {
                            if (NetworkManager.Instance().IsRunningLogic()) {
                                NetworkManager.Instance().GetConnection(ConnectionType.Client).GetSocket().UnregisterClient(networkClient);
                            }
                        } else {
                            NetworkManager.Instance().GetConnection(ConnectionType.Client).GetSocket().UnregisterClient(networkClient);
                        }
                    } else if (NetworkManager.Instance().IsRunningLogic()) {
                        if (NetworkManager.Instance().HasConnection(ConnectionType.Server)) {
                            NetworkManager.Instance().GetConnection(ConnectionType.Server).GetSocket().UnregisterClient(networkClient);
                        } else if (NetworkManager.Instance().HasConnection(ConnectionType.Client)) {
                            NetworkManager.Instance().GetConnection(ConnectionType.Client).GetSocket().UnregisterClient(networkClient);
                        }
                    }
                    // Execute network prefab callback
                    objectReference.ExecuteOnDespawnPrefab();
                } else {
                    NetworkManager.Instance().DestroyOnClient(objectNetworkId);
                }
            }
        }

        /// <summary>
        /// Confirms the creation of an object on the client.
        /// </summary>
        /// <param name="reader">The data stream containing the confirmation information.</param>
        private void OnObjectCreatedOnClient(IDataStream reader) {
            int     networkId = reader.Read<int>(); // Extract network ID of this object
            IClient client    = (reader as INetworkStream).GetClient();
            NetworkManager.Instance().ConfirmPlayerCreatedOnClient(client, networkId);
        }

        /// <summary>
        /// Handles the reception of an object update over the network.
        /// </summary>
        /// <param name="reader">The data stream containing the update information.</param>
        private void OnObjectUpdateReceived(IDataStream reader) {
            // Then get network ID rewing into the buffer
            int networkId   = reader.Read<int>();
            if (NetworkManager.Container.IsRegistered(networkId)) {
                INetworkElement networkElement = NetworkManager.Container.GetElement(networkId);
                networkElement.RegisterNetworkPacket(reader);
            }
        }

        /// <summary>
        /// Handles the reception of an object input update over the network.
        /// </summary>
        /// <param name="reader">The data stream containing the input update information.</param>
        private void OnObjectInputUpdateReceived(IDataStream reader) {
            // Then get network ID rewing into the buffer
            int networkId   = reader.Read<int>();
            if (NetworkManager.Container.IsRegistered(networkId)) {
                INetworkElement networkElement = NetworkManager.Container.GetElement(networkId);
                networkElement.RegisterNetworkPacket(reader);
            }
        }

        /// <summary>
        /// Handles the identification of the local player by reading the network ID from the data stream
        /// and configuring the corresponding network object.
        /// </summary>
        /// <param name="reader">The data stream containing the network ID.</param>
        private void OnLocalPlayerIdentify(IDataStream reader) {
            // Read the network ID from the data stream.
            int networkId = reader.Read<int>();
            // Check if the network ID is registered with the NetworkManager.
            if (NetworkManager.Container.IsRegistered(networkId)) {
                // Retrieve the network element associated with the network ID.
                INetworkElement networkElement = NetworkManager.Container.GetElement(networkId);
                // Detect the owner of the network element.
                networkElement.DetectOwner(networkId);
                if (networkElement.IsOwner()) {
                    NetworkManager.Instance().OnPlayerOwnerDetected(networkElement);
                }
                // If the local player is the owner of the network element.
                if (networkElement.IsOwner()) {
                    // Retrieve the NetworkObject component from the network element's GameObject.
                    NetworkObject networkObject = networkElement.GetGameObject().GetComponent<NetworkObject>();
                    // Configure the network object with the appropriate transport, delivery mode, and input settings.
                    networkObject.SetTransport(NetworkManager.Instance().GetConnection(ConnectionType.Client));
                    networkObject.SetDeliveryMode(NetworkManager.Instance().GetUpdateMode());
                    networkObject.SetRemoteControlsEnabled(NetworkManager.Instance().UseRemoteInput());
                    networkObject.SetBehaviorMode(NetworkManager.Instance().IsRemoteInputEnabled() ? BehaviorMode.Passive : BehaviorMode.Active);
                    // Mark the network object as identified.
                    networkObject.SetIdentified(true);
                    // Initialize network input if applicable.
                    networkObject.ConfigureNetworkInput();

                    // Reactivate scripts according to the original prefab.
                    NetworkScriptsReference scriptsReference = networkElement.GetGameObject().GetComponent<NetworkScriptsReference>();
                    if (scriptsReference != null) {
                        // Enable input components or all components based on remote input settings.
                        if (NetworkManager.Instance().UseRemoteInput()) {
                            scriptsReference.EnableInputComponents();
                        } else {
                            scriptsReference.EnableComponents();
                        }
                    }

                    // Handle camera control logic if required by the engine.
                    if (NetworkManager.Instance().IsToControlCameras()) {
                        // Disable the main camera if it is not on the player and enable the player's camera.
                        Camera mainCamera = Camera.main;
                        // If there's no MainCamera I nee to find a cameras on scene 
                        if (mainCamera == null) {
                            Camera[] camerasOnScene = FindObjectsOfType<Camera>();
                            if (camerasOnScene.Length > 0) {
                                foreach (Camera cam in camerasOnScene) {
                                    camerasOnScene[0].tag = NetworkManager.MAIN_CAMERA_TAG;
                                }
                                mainCamera = Camera.main;
                            }
                        }
                        if (networkElement.GetGameObject() != null && mainCamera != null) {
                            bool isMainCameraOnPlayer = false;
                            Camera[] cameras = networkElement.GetGameObject().GetComponentsInChildren<Camera>();
                            if (cameras.Length > 1) {
                                Debug.Log("====================================================================================================");
                                Debug.Log("[ATTENTION] Spawned player has more than one camera inside prefab, the first camera will be the main");
                                Debug.Log("====================================================================================================");
                            }
                            foreach (Camera cam in cameras) {
                                isMainCameraOnPlayer |= (mainCamera == cam);
                            }
                            if (!isMainCameraOnPlayer && cameras.Length > 0) {
                                mainCamera.enabled = false;
                                AudioListener audioControl = mainCamera.GetComponent<AudioListener>();
                                if (audioControl != null) {
                                    audioControl.enabled = false;
                                }
                                // Enable the first camera found on the player.
                                cameras[0].enabled = true;
                                // Enable audio founbd on camera
                                AudioListener audioPlayerControl = cameras[0].GetComponent<AudioListener>();
                                if (audioPlayerControl != null) {
                                    audioPlayerControl.enabled = true;
                                }
                                // Detach the camera from the player prefab if required.
                                if (NetworkManager.Instance().IsToDetachPlayerCamera()) {
                                    cameras[0].gameObject.transform.parent = null;
                                    cameras[0].gameObject.name = string.Format("{0}.{1}", cameras[0].gameObject.name, networkId);
                                    // Register the camera for later cleanup.
                                    NetworkManager.Instance().RegisterGarbageObject(cameras[0].gameObject);
                                }
                            } else if (isMainCameraOnPlayer) {
                                // Detach the main camera from the player prefab if required.
                                if (NetworkManager.Instance().IsToDetachPlayerCamera()) {
                                    mainCamera.gameObject.transform.parent = null;
                                    cameras[0].gameObject.name = string.Format("{0}.{1}", cameras[0].gameObject.name, networkId);
                                    // Register the main camera for later cleanup.
                                    NetworkManager.Instance().RegisterGarbageObject(mainCamera.gameObject);
                                }
                                // Enable the first camera found on the player.
                                cameras[0].enabled = true;
                                // Enable audio founbd on camera
                                AudioListener audioPlayerControl = cameras[0].GetComponent<AudioListener>();
                                if (audioPlayerControl != null) {
                                    audioPlayerControl.enabled = true;
                                }
                            }
                        }
                    }
                    if (NetworkManager.Instance().IsRunningLogic() == false) {
                        NetworkManager.Instance().FlagInSceneObjectsAllowed();
                    }                    
                }
                // Send an identifier response back to the server.
                using (DataStream writer = new DataStream()) {
                    writer.Write(networkId);
                    writer.Write(this.reconnection); // Is a reconnection ?
                    writer.Write(NetworkManager.Instance().IsToAutoLoadSceneElements()); // Shall server spawn scene elements to me ?
                    (reader as INetworkStream).GetClient().Send(CoreGameEvents.PlayerIdentified, writer, DeliveryMode.Reliable);
                }
                // Request any variable to be updated
                NetworkManager.Instance().Enqueue(() => {
                    NetworkManager.Instance().RequestNetworkVariablesUpdate();
                }, DELAY_BEFORE_REQUEST_VARIABLES);
            }
        }

        /// <summary>
        /// Handles the event when a local player is identified.
        /// </summary>
        /// <param name="reader">The data stream containing the network ID.</param>
        private void OnLocalPlayerIdentified(IDataStream reader) {
            // Read the network ID from the data stream.
            int     networkId       = reader.Read<int>();
            bool    reconnection    = reader.Read<bool>();
            bool    spawnElements   = reader.Read<bool>();
            // Retrieve the client from the network stream.
            IClient client = (reader as INetworkStream).GetClient();
            // Retrieve the network element associated with the network ID.
            INetworkElement networkElement = NetworkManager.Container.GetElement(networkId);
            // If was a reconnection i will not to spawn object, otherwise thi might duplicate objects during host migration
            if ((reconnection == false) && (spawnElements == true)) {
                // Initialize spawned objects for the identified player.
                NetworkManager.Instance().InitializeSpawnedObjects(client, networkElement);
            }
            // Initialize internal behavior execution
            networkElement.GetNetworkObject().InitializeExecutor();            
        }

        /// <summary>
        /// Handles the event when a local player is identified.
        /// </summary>
        /// <param name="reader">The data stream containing the network ID.</param>
        private void OnLocalPlayerRequestSceneElements(IDataStream reader) {
            // Read the network ID from the data stream.
            int networkId = reader.Read<int>();
            // Retrieve the client from the network stream.
            IClient client = (reader as INetworkStream).GetClient();
            // Initialize spawned objects for the identified player.
            NetworkManager.Instance().InitializeSpawnedObjects(client);
            // Flag to send variables update to this client
            NetworkManager.Instance().RegisterClientToUpdateVariables(client);
        }
        

        /// <summary>
        /// Handles the event when a local player is spawned on the server.
        /// </summary>
        /// <param name="reader">The data stream containing the network ID.</param>
        private void OnLocalPlayerSpawnedOnServer(IDataStream reader) {
            // Read the network ID from the data stream.
            int networkId = reader.Read<int>();
            // Retrieve the client from the network stream.
            IClient client = (reader as INetworkStream).GetClient();
            // Send an identifier request to the server.
            using (DataStream writer = new DataStream()) {
                writer.Write(networkId);
                client.Send(CoreGameEvents.PlayerRequestClientId, writer, DeliveryMode.Reliable);
            }
        }

        /// <summary>
        /// Handles the event when a local player requests an ID on the server.
        /// </summary>
        /// <param name="reader">The data stream containing the network ID.</param>
        private void OnLocalPlayerRequestIdOnServer(IDataStream reader) {
            // Read the network ID from the data stream.
            int networkId = reader.Read<int>();
            // Retrieve the client from the network stream.
            IClient client = (reader as INetworkStream).GetClient();
            // Send client identification to inform the client that they are the owner of the object.
            using (DataStream writer = new DataStream()) {
                writer.Write(networkId);
                client.Send(CoreGameEvents.PlayerIdentify, writer, DeliveryMode.Reliable);
            }
        }

        /// <summary>
        /// Callback to be executed when player is ready on client
        /// </summary>
        /// <param name="callback">Callback to be executed when player is ready</param>
        public void OnSendPlayerReadyOnClient(Action<DataStream> callback) {
            this.onSendPlayerReadyOnClient = callback;
        }

        /// <summary>
        /// Tells to the server that player is about to Respawn.
        /// 
        /// Note : This is a internal method to be used to configure server during respawn operation
        /// </summary>
        /// <param name="playerNetworkElement">Network element of this player</param>
        public void RequestPlayerRespawn(INetworkElement playerNetworkElement) {
            this.InternalRequestPlayerRespawn(playerNetworkElement, null, false, Vector3.zero, false, Quaternion.identity);
        }

        /// <summary>
        /// Tells to the server that player is about to Respawn.
        /// 
        /// Note : This is a internal method to be used to configure server during respawn operation
        /// </summary>
        /// <param name="playerNetworkElement">Network element of this player</param>
        /// <param name="position">Position to spawn object</param>
        public void RequestPlayerRespawn(INetworkElement playerNetworkElement, Vector3 position) {
            this.InternalRequestPlayerRespawn(playerNetworkElement, null, true, position, false, Quaternion.identity);
        }

        /// <summary>
        /// Tells to the server that player is about to Respawn.
        /// 
        /// Note : This is a internal method to be used to configure server during respawn operation
        /// </summary>
        /// <param name="playerNetworkElement">Network element of this player</param>
        /// <param name="position">Position to spawn object</param>
        /// <param name="rotation">Rotation to spawn object</param>
        public void RequestPlayerRespawn(INetworkElement playerNetworkElement, Vector3 position, Quaternion rotation) {
            this.InternalRequestPlayerRespawn(playerNetworkElement, null, true, position, true, rotation);
        }
        
        /// <summary>
        /// Tells to the server that player is about to Respawn.
        /// 
        /// Note : This is a internal method to be used to configure server during respawn operation
        /// </summary>
        /// <param name="playerNetworkElement">Network element of this player</param>
        /// <param name="playerPrefab">Player prefab to spawn</param>
        public void RequestPlayerRespawn(INetworkElement playerNetworkElement, GameObject playerPrefab) {
            this.InternalRequestPlayerRespawn(playerNetworkElement, playerPrefab, false, Vector3.zero, false, Quaternion.identity);
        }

        /// <summary>
        /// Tells to the server that player is about to Respawn.
        /// 
        /// Note : This is a internal method to be used to configure server during respawn operation
        /// </summary>
        /// <param name="playerNetworkElement">Network element of this player</param>
        /// <param name="playerPrefab">Player prefab to spawn</param>
        /// <param name="position">Position to spawn object</param>
        public void RequestPlayerRespawn(INetworkElement playerNetworkElement, GameObject playerPrefab, Vector3 position) {
            this.InternalRequestPlayerRespawn(playerNetworkElement, playerPrefab, true, position, false, Quaternion.identity);
        }

        /// <summary>
        /// Tells to the server that player is about to Respawn.
        /// 
        /// Note : This is a internal method to be used to configure server during respawn operation
        /// </summary>
        /// <param name="playerNetworkElement">Network element of this player</param>
        /// <param name="playerPrefab">Player prefab to spawn</param>
        /// <param name="position">Position to spawn object</param>
        /// <param name="rotation">Rotation to spawn object</param>
        public void RequestPlayerRespawn(INetworkElement playerNetworkElement, GameObject playerPrefab, Vector3 position, Quaternion rotation) {
            this.InternalRequestPlayerRespawn(playerNetworkElement, playerPrefab, true, position, true, rotation);
        }

        /// <summary>
        /// Tells to the server that player is about to Respawn.
        /// 
        /// Note : This is a internal method to be used to configure server during respawn operation
        /// </summary>
        /// <param name="playerNetworkElement">Network element of this player</param>
        private void InternalRequestPlayerRespawn(INetworkElement playerNetworkElement, GameObject playerPrefab, bool usePosition, Vector3 startPosition, bool useRotation, Quaternion startRotation) {
            IClient client = this.GetSocket().GetLocalClient();
            if ((playerPrefab == null) ||
                (playerPrefab.GetComponent<NetworkInstantiateDetection>() != null)) {
                // Send ready to master server
                using (DataStream writer = new DataStream()) {
                    writer.Write((client as NetworkClient).GetConnectionId());
                    writer.Write(playerNetworkElement.GetNetworkId()); // Network ID to respawn
                    writer.Write(playerNetworkElement.GetPlayerId()); // Player ID
                    writer.Write(playerPrefab != null); // Prefab signature
                    writer.Write((playerPrefab != null) ? playerPrefab.GetComponent<NetworkInstantiateDetection>().GetPrefabSignature() : ""); // Prefab signature
                    writer.Write(usePosition); // Shall to use position ?
                    writer.Write(startPosition); // Start position
                    writer.Write(useRotation); // Shall to use rotation ?
                    writer.Write(startRotation); // Start rotation
                    // Include player data information if login is enabled.
                    if (NetworkManager.Instance().IsLoginEnabled()) {
                        object[] parametersValues = NetworkManager.Instance().GetLoginInformations();
                        Type[] parametersTypes = NetworkManager.Instance().GetLoginInformationsTypes();
                        // Write the login information to the data stream.
                        for (int paramIndex = 0; paramIndex < parametersValues.Length; paramIndex++) {
                            writer.Write(parametersValues[paramIndex], parametersTypes[paramIndex]);
                        }
                    }
                    client.Send(CoreGameEvents.RequestPlayerRespawn, writer, DeliveryMode.Reliable);
                }
            } else {
                throw new Exception(string.Format("The prefab \"{0}\" isn't a networi prefab and can't be spawned over network", playerPrefab.name));
            }
        }

        /// <summary>
        /// Notifies the server that the player is ready on the client side.
        /// </summary>
        /// <param name="client">The client to notify.</param>
        /// <param name="connectionId">The connection ID of the client.</param>
        /// <param name="playerId">The player ID (optional).</param>
        /// <param name="respawn">Is this a respawn operation (optional).</param>
        /// <param name="respawnedNetworkId">The network id of respawned player (optional).</param>
        private void NotifyPlayerReadyOnClient(IClient client, int connectionId, ushort playerId = 0, bool respawn = false, int respawnedNetworkId = 0) {
            // Send a message to the server indicating that the player is ready.
            using (DataStream writer = new DataStream()) {
                writer.Write(connectionId);
                writer.Write(playerId);
                writer.Write(this.reconnection); // Is this a reconnection ?
                writer.Write(respawn); // If this a respawn ?
                writer.Write(respawnedNetworkId); // Network ID to respawn
                // Include player data information if login is enabled.
                if (NetworkManager.Instance().IsLoginEnabled()) {
                    object[] parametersValues = NetworkManager.Instance().GetLoginInformations();
                    Type[] parametersTypes = NetworkManager.Instance().GetLoginInformationsTypes();
                    // Write the login information to the data stream.
                    for (int paramIndex = 0; paramIndex < parametersValues.Length; paramIndex++) {
                        writer.Write(parametersValues[paramIndex], parametersTypes[paramIndex]);
                    }
                }
                // Inject any extra data to send to server avount this player
                if (this.onSendPlayerReadyOnClient != null) {
                    this.onSendPlayerReadyOnClient.Invoke(writer);
                }
                client.Send(CoreGameEvents.PlayerReadyOnClient, writer, DeliveryMode.Reliable);
            }
        }

        /// <summary>
        /// Handles the event when a player is ready on the client side.
        /// </summary>
        /// <param name="reader">The data stream containing the player's connection and login information.</param>
        private void OnPlayerRespawnRequested(IDataStream reader) {
            int             connectionId    = reader.Read<int>();
            int             networkId       = reader.Read<int>();
            ushort          playerId        = reader.Read<ushort>();
            bool            usePrefab       = reader.Read<bool>();
            string          prefabSignature = reader.Read<string>();
            bool            usePosition     = reader.Read<bool>();
            Vector3         startPosition   = reader.Read<Vector3>();
            bool            useRotation     = reader.Read<bool>();
            Quaternion      startRotation   = reader.Read<Quaternion>();

            List<object>    loginParameters         = (NetworkManager.Instance().IsLoginEnabled()) ? new List<object>() : null;
            Type[]          loginParametersTypes    = (NetworkManager.Instance().IsLoginEnabled()) ? NetworkManager.Instance().GetLoginInformationsTypes() : null;
            // Read the login information from the data stream.
            if (NetworkManager.Instance().IsLoginEnabled()) {
                foreach (Type parameterType in loginParametersTypes) {
                    loginParameters.Add(reader.Read<object>(parameterType));
                }
            }
            // Now i'm going to start to respawn object
            IClient targetClient    = (reader as INetworkStream).GetClient();
            // First i'm going to register a destruction callback
            NetworkManager.Instance().RegisterOnDestroyAction(networkId, () => {
                NetworkManager.Instance().Enqueue(() => {
                    NetworkPrefabEntry  prefabEntry     = (usePrefab) ? NetworkManager.Instance().GetNetworkPrefabEntry(prefabSignature) : null;
                    GameObject          spawnedPlayer   = null;
                    if ((usePosition == true) && (useRotation == false)) {
                        if (usePrefab) {
                            if (prefabEntry != null) {
                                spawnedPlayer = NetworkManager.Instance().SpawnClientPlayer(targetClient, connectionId, playerId, prefabEntry.GetPrefab(), startPosition);
                            } else {
                                throw new Exception(string.Format("The prefab \"{0}\" signature was not found on prefabs database", prefabSignature));
                            }
                        } else {
                            spawnedPlayer = NetworkManager.Instance().SpawnClientPlayer(targetClient, connectionId, playerId, startPosition);
                        }
                    } else if ((usePosition == true) && (useRotation == true)) {
                        if (usePrefab) {
                            if (prefabEntry != null) {
                                spawnedPlayer = NetworkManager.Instance().SpawnClientPlayer(targetClient, connectionId, playerId, prefabEntry.GetPrefab(), startPosition, startRotation);
                            } else {
                                throw new Exception(string.Format("The prefab \"{0}\" signature was not found on prefabs database", prefabSignature));
                            }
                        } else {
                            spawnedPlayer = NetworkManager.Instance().SpawnClientPlayer(targetClient, connectionId, playerId, startPosition, startRotation);
                        }
                    } else {
                        if (usePrefab) {
                            if (prefabEntry != null) {
                                spawnedPlayer = NetworkManager.Instance().SpawnClientPlayer(targetClient, connectionId, playerId, prefabEntry.GetPrefab());
                            } else {
                                throw new Exception(string.Format("The prefab \"{0}\" signature was not found on prefabs database", prefabSignature));
                            }
                        } else {
                            spawnedPlayer = NetworkManager.Instance().SpawnClientPlayer(targetClient, connectionId, playerId);
                        }
                    }
                    // Set player tag attributes to match the login parameters.
                    if (NetworkManager.Instance().IsLoginEnabled()) {
                        NetworkPlayerTag playerTagControl = spawnedPlayer.GetComponent<NetworkPlayerTag>();
                        playerTagControl.SetAttributesValues(loginParameters.ToArray<object>());
                        playerTagControl.SetAttributesTypes(loginParametersTypes);
                    }
                }, DELAY_BEFORE_RESPAWN_AFTER_DESTROY);
            });
            // Then i'm going to destroy game object
            NetworkManager.Instance().DestroyOnClient(networkId);            
        }

        /// <summary>
        /// Handles the event when a player request realiable variables update.
        /// </summary>
        /// <param name="reader">The data stream containing the player's request information.</param>
        private void OnPlayerVariablesRequested(IDataStream reader) {
            ushort  playerId = reader.Read<ushort>();            
            // Invalidate variables to update the new player
            NetworkManager.Instance().RegisterClientToUpdateVariables((reader as INetworkStream).GetClient());
        }        

        /// <summary>
        /// Handles the event when a player is ready on the client side.
        /// </summary>
        /// <param name="reader">The data stream containing the player's connection and login information.</param>
        private void OnPlayerIsReadyOnClient(IDataStream reader) {
            // Read the connection ID and player ID from the data stream.
            int     connectionId    = reader.Read<int>();
            ushort  playerId        = reader.Read<ushort>();
            bool    reconnection    = reader.Read<bool>();
            bool    respawn         = reader.Read<bool>();
            int     networkId       = reader.Read<int>();
            // Process login information if login is enabled.
            if (NetworkManager.Instance().IsPlayerSpawnerEnabled() ) {
                Action  actionToExecute = null;
                if (NetworkManager.Instance().IsLoginEnabled()) {
                    actionToExecute = () => {
                        try {
                            List<object> loginParameters = new List<object>();
                            Type[] loginParametersTypes = NetworkManager.Instance().GetLoginInformationsTypes();
                            // Read the login information from the data stream.
                            foreach (Type parameterType in loginParametersTypes) {
                                loginParameters.Add(reader.Read<object>(parameterType));
                            }
                            // Validate the login information if required.
                            if (NetworkManager.Instance().IsToValidateLogin()) {
                                if (!NetworkManager.Instance().IsValidLogin(loginParameters.ToArray())) {
                                    throw new Exception("Login failure: It was not possible to authenticate the user with the provided arguments");
                                }
                            }
                            GameObject spawnedPlayer = null;
                            // Use a custom network ID if configured.
                            if (NetworkManager.Instance().IsToUseCustomNetworkId()) {
                                int objectNetworkId = NetworkManager.Instance().GetCustomNetworkId(loginParameters.ToArray<object>());
                                Debug.Log(string.Format("Custom Network Id [{0}]", objectNetworkId));
                                if (NetworkManager.Container.IsRegistered(objectNetworkId)) {
                                    INetworkElement networkPlayerelement = NetworkManager.Container.GetElement(objectNetworkId);
                                    spawnedPlayer = networkPlayerelement.GetGameObject();
                                    // Update the client connection on the player object and network object.
                                    NetworkPlayerReference playerReference = spawnedPlayer.GetComponent<NetworkPlayerReference>();
                                    playerReference.SetClient((reader as INetworkStream).GetClient());
                                    if (playerReference.IsReconnection()) {
                                        playerReference.SetConnectionId(connectionId);
                                        playerReference.SetPlayerId(playerId);
                                    }
                                    NetworkObject networkObject = spawnedPlayer.GetComponent<NetworkObject>();
                                    networkObject.SetClient((reader as INetworkStream).GetClient());
                                    // If is reconnection will not register to detect to not spawn object twice
                                    if (reconnection == false) {
                                        // Register the player object for detection and notification purposes.
                                        NetworkManager.Instance().RegisterDetectedObject(spawnedPlayer);
                                    }
                                }
                            }
                            // Spawn the player on the server after login validation.
                            if (spawnedPlayer == null) {
                                spawnedPlayer = this.spawnClientPlayer((reader as INetworkStream).GetClient(), connectionId, false, Vector3.zero, false, Quaternion.identity, 0, NetworkPlayerSpawnTime.Automatic, null);
                            }
                            // Trigger the login success event.
                            NetworkManager.Instance().OnClientLoginSucessEvent((reader as INetworkStream).GetClient());
                            // Set player tag attributes to match the login parameters.
                            NetworkPlayerTag playerTagControl = spawnedPlayer.GetComponent<NetworkPlayerTag>();
                            playerTagControl.SetAttributesValues(loginParameters.ToArray<object>());
                            playerTagControl.SetAttributesTypes(loginParametersTypes);
                            // Send a login success message to the client.
                            IClient networkClient = (reader as INetworkStream).GetClient();
                            if (networkClient != null) {
                                using (DataStream writer = new DataStream()) {
                                    writer.Write((int)NetworkErrorCodes.LoginSucess);
                                    networkClient.Send(InternalProtocolEvents.LoginSucess, writer, DeliveryMode.Reliable);
                                }
                            }
                        } catch (Exception err) {
                            // Handle login failure by logging the error and notifying the client.
                            try {
                                NetworkDebugger.LogError("Login attempt failed: {0}", err.Message);
                                IClient networkClient = (reader as INetworkStream).GetClient();
                                if (networkClient != null) {
                                    using (DataStream writer = new DataStream()) {
                                        writer.Write((int)NetworkErrorCodes.LoginFailure);
                                        networkClient.Send(InternalProtocolEvents.LoginError, writer, DeliveryMode.Reliable);
                                    }
                                }
                                if (this.onLoginFailed != null) {
                                    this.onLoginFailed.Invoke(err);
                                }
                                // Trigger the login failed event.
                                NetworkManager.Instance().OnClientLoginFailedEvent(networkClient);
                            } catch (Exception errThrow) {
                                if (this.onLoginFailed != null) {
                                    this.onLoginFailed.Invoke(errThrow);
                                } else {
                                    throw errThrow;
                                }
                            }
                        }
                    };
                } else if (NetworkManager.Instance().InEmbeddedMode() ||
                           NetworkManager.Instance().InAuthoritativeMode() ||
                           (NetworkManager.Instance().IsConnectedOnRelayServer() && NetworkManager.Instance().IsMasterPlayer())) {
                    // Spawn the player on the server in other modes.
                    IClient targetClient = (reader as INetworkStream).GetClient();
                    actionToExecute = () => {
                        this.spawnClientPlayer(targetClient, connectionId, false, Vector3.zero, false, Quaternion.identity, playerId, NetworkPlayerSpawnTime.Automatic, null);
                    };                    
                }
                if ((respawn) && (Mathf.Abs(networkId) > 0)) {
                    // First i'm going to register a destruction callback
                    NetworkManager.Instance().RegisterOnDestroyAction(networkId, actionToExecute);
                    // Then i'm going to destroy any previous player object
                    NetworkManager.Instance().DestroyOnClient(networkId); 
                } else {
                    actionToExecute.Invoke();
                }                
            }
        }

        /// <summary>
        /// Handles the event when a player need to be respawned after his destruction
        /// </summary>
        /// <param name="reader">The data stream containing the player's connection and login information.</param>
        private void OnPlayerRespawn(IDataStream reader) {
            // Read the connection ID and player ID from the data stream.
            int connectionId        = reader.Read<int>();
            ushort playerId         = reader.Read<ushort>();
            if (NetworkManager.Instance().IsPlayerSpawnerEnabled()) {
                if (NetworkManager.Instance().InEmbeddedMode() ||
                    NetworkManager.Instance().InAuthoritativeMode() ||
                    (NetworkManager.Instance().IsConnectedOnRelayServer() && NetworkManager.Instance().IsMasterPlayer())) {
                    // Spawn the player on the server in other modes.
                    IClient targetClient = (reader as INetworkStream).GetClient();
                    this.spawnClientPlayer(targetClient, connectionId, false, Vector3.zero, false, Quaternion.identity, playerId, NetworkPlayerSpawnTime.Automatic, null);
                }
            }
        }

        /// <summary>
        /// Handles the event when a player need to load some scene
        /// </summary>
        /// <param name="reader">The data stream containing loading scene information.</param>
        private void OnRemoteLoadSceneRequested(IDataStream reader) {
            string          sceneName = reader.Read<string>();
            LoadSceneMode   loadeMode = (LoadSceneMode)reader.Read<byte>();
            // The will load scene on server side
            NetworkManager.Instance().LoadSceneRemote(sceneName, RemoteSceneLoadMode.LoadAfter, loadeMode);
        }

        /// <summary>
        /// Handles the event when a player need to unload some scene
        /// </summary>
        /// <param name="reader">The data stream containing loading scene information.</param>
        private void OnRemoteUnLoadSceneRequested(IDataStream reader) {
            string sceneName = reader.Read<string>();
            // The will load scene on server side
            NetworkManager.Instance().UnloadSceneRemote(sceneName, RemoteSceneUnloadMode.UnloadAfter);
        }
        

        /// <summary>
        /// Handles the event when a player need to load some scene
        /// </summary>
        /// <param name="reader">The data stream containing loading scene information.</param>
        private void OnRemoteLoadScene(IDataStream reader) {
            string          sceneName   = reader.Read<string>();
            LoadSceneMode   loadeMode   = (LoadSceneMode)reader.Read<byte>();
            // The will load scene
            StartCoroutine(this.LoadAsyncScene(sceneName, loadeMode, (reader as INetworkStream).GetClient()));
        }

        /// <summary>
        /// Handles the event when a player need to load some scene
        /// </summary>
        /// <param name="reader">The data stream containing loading scene information.</param>
        private void OnRemoteUnLoadScene(IDataStream reader) {
            string sceneName = reader.Read<string>();
            // The will load scene
            StartCoroutine(this.UnLoadAsyncScene(sceneName, (reader as INetworkStream).GetClient()));
        }
        

        private IEnumerator LoadAsyncScene(string sceneName, LoadSceneMode loadMode, IClient client) {
            // Execute scene load
            NetworkManager.Instance().StartRemoteAsyncSceneLoding(SceneManager.LoadSceneAsync(sceneName, loadMode));
            // Wait until the asynchronous scene fully loads
            while (!NetworkManager.Instance().IsAsyncSceneLoadFinished()) {
                yield return null;
            }
            // The send the result event that scene was already loaded
            using (DataStream writer = new DataStream()) {
                writer.Write(this.GetSocket().GetConnectionID()); // Return scene index
                writer.Write(sceneName);
                client.Send(CoreGameEvents.ClientSceneLoaded, writer, DeliveryMode.Reliable);
            }
        }

        private IEnumerator UnLoadAsyncScene(string sceneName, IClient client) {
            // Execute scene load
            NetworkManager.Instance().StartRemoteAsyncSceneLoding(SceneManager.UnloadSceneAsync(sceneName));
            // Wait until the asynchronous scene fully loads
            while (!NetworkManager.Instance().IsAsyncSceneLoadFinished()) {
                yield return null;
            }
            // The send the result event that scene was already loaded
            using (DataStream writer = new DataStream()) {
                writer.Write(this.GetSocket().GetConnectionID()); // Return scene index
                writer.Write(sceneName);
                client.Send(CoreGameEvents.ClientSceneUnLoaded, writer, DeliveryMode.Reliable);
            }
        }
        

        /// <summary>
        /// Handles the event when a player tell that the scene was already loaded
        /// </summary>
        /// <param name="reader">The data stream containing loading scene information.</param>
        private void OnClientSceneLoaded(IDataStream reader) {
            int             connectionId    = reader.Read<int>();
            string          sceneName       = reader.Read<string>();
            // Register as client already loaded the scene ( if return true means that load process was finished )
            if ( NetworkManager.Instance().UnregisterRemoteLoadingScene((reader as INetworkStream).GetClient()) ) {
                // Finish any pending load operation
                NetworkManager.Instance().FinishLoadingSceneOperation();
            }
        }

        /// <summary>
        /// Handles the event when a player tell that the scene was already loaded
        /// </summary>
        /// <param name="reader">The data stream containing loading scene information.</param>
        private void OnClientSceneUnLoaded(IDataStream reader) {
            int connectionId = reader.Read<int>();
            string sceneName = reader.Read<string>();
            // Register as client already loaded the scene ( if return true means that load process was finished )
            if (NetworkManager.Instance().UnregisterRemoteLoadingScene((reader as INetworkStream).GetClient())) {
                // Finish any pending load operation
                NetworkManager.Instance().FinishLoadingSceneOperation();
            }
        }
        
        /// <summary>
        /// Handles the event when a all players loaded his scenes
        /// </summary>
        /// <param name="reader">The data stream containing loading scene information.</param>
        private void OnRemoteSceneLoadFinished(IDataStream reader) {
            string sceneName = reader.Read<string>();
            if ( this.onRemoteSceneLoaded != null ) {
                this.onRemoteSceneLoaded.Invoke(sceneName);
            }
            // Flag scene loading finihed
            NetworkManager.Instance().FlagLoadingSceneFinished();
            // Now active remote laoded scene
            NetworkManager.Instance().ActivateRemoteScene();
            // Call global event
            NetworkManager.Instance().OnRemoteSceneLoadFinishedEvent(sceneName);
            // Resume disconnect detection due loadign scene finished
            NetworkManager.Instance().ResumeDisconnectDetection();
        }

        /// <summary>
        /// Handles the event when a all players unloaded his scenes
        /// </summary>
        /// <param name="reader">The data stream containing unloading scene information.</param>
        private void OnRemoteSceneUnLoadFinished(IDataStream reader) {
            string sceneName = reader.Read<string>();
            if (this.onRemoteSceneUnLoaded != null) {
                this.onRemoteSceneUnLoaded.Invoke(sceneName);
            }
            // Flag scene loading finihed
            NetworkManager.Instance().FlagLoadingSceneFinished();
            // Call global event
            NetworkManager.Instance().OnRemoteSceneUnLoadFinishedEvent(sceneName);
            // Resume disconnect detection due loadign scene finished
            NetworkManager.Instance().ResumeDisconnectDetection();
        }

        /// <summary>
        /// Handles the event when server detect that load operation was failed on client
        /// </summary>
        /// <param name="reader">The data stream containing loading scene fail information.</param>
        private void OnRemoteSceneLoadFailed(IDataStream reader) {
            NetworkDebugger.Log("Remote load scene failed, client will be disconnected");
            NetworkManager.Instance().StopNetwork(); // Stop network operation in a controlled way
        }

        /// <summary>
        /// Handles the event when server detect that unload operation was failed on client
        /// </summary>
        /// <param name="reader">The data stream containing loading scene fail information.</param>
        private void OnRemoteScenUnLoadFailed(IDataStream reader) {
            NetworkDebugger.Log("Remote unload scene failed, client will be disconnected");
            NetworkManager.Instance().StopNetwork(); // Stop network operation in a controlled way
        }
        
        /// <summary>
        /// Handle with event code send from server to client to enable/disable disconnection by timeout
        /// </summary>
        /// <param name="reader">The data stream containing loading disconnection timeout information.</param>
        private void OnDisconnectionTimeoutUpdate(IDataStream reader) {
            int     connectionId    = reader.Read<int>();
            bool    value           = reader.Read<bool>();
            bool    hazardPause     = reader.Read<bool>();
            if (value) {
                NetworkManager.Instance().ResumeDisconnectDetection();
            } else {
                NetworkManager.Instance().PauseDisconnectDetection(hazardPause);
            }
            NetworkDebugger.Log("Disconnecton detection changed to [{0}] with hazard as [{1}]", value, hazardPause);
        }

        /// <summary>
        /// Handle with event code send from client to server to spawn player
        /// </summary>
        /// <param name="reader">The data stream containing player spawn information.</param>
        private void OnRequestPlayerSpawn(IDataStream reader) {
            int     connectionId    = reader.Read<int>();
            string  prefabSignature = reader.Read<string>();
            bool    includePosition = reader.Read<bool>();
            Vector3 position        = reader.Read<Vector3>();
            bool    includeRotation = reader.Read<bool>();
            Vector3 rotation        = reader.Read<Vector3>();

            List<object>    loginParameters         = null;
            Type[]          loginParametersTypes    = null;
            if (NetworkManager.Instance().IsLoginEnabled()) {
                loginParameters         = new List<object>();
                loginParametersTypes    = NetworkManager.Instance().GetLoginInformationsTypes();
                // Read the login information from the data stream.
                foreach (Type parameterType in loginParametersTypes) {
                    loginParameters.Add(reader.Read<object>(parameterType));
                }
            }
            GameObject playerObject = null;
            // Find prefab with the recedived signature
            if (string.IsNullOrEmpty(prefabSignature) == false) {
                NetworkPrefabEntry prefabEntry = NetworkManager.Instance().GetNetworkPrefabEntry(prefabSignature);
                if (prefabEntry != null) {
                    playerObject = this.spawnClientPlayer((reader as INetworkStream).GetClient(), connectionId, includePosition, position, includeRotation, Quaternion.Euler(rotation), 0, NetworkPlayerSpawnTime.Manually, prefabEntry.GetPrefab());
                } else {
                    throw new Exception(String.Format("The prefab signature [{0}] doesn't exists in prefabs database", prefabSignature));
                }
            } else {
                playerObject = this.spawnClientPlayer((reader as INetworkStream).GetClient(), connectionId, includePosition, position, includeRotation, Quaternion.Euler(rotation), 0, NetworkPlayerSpawnTime.Manually, null);
            }
            // If login is enabled, set the player's tag with login parameters
            if (NetworkManager.Instance().IsLoginEnabled()) {
                NetworkPlayerTag playerTagControl = playerObject.GetComponent<NetworkPlayerTag>();
                playerTagControl.SetAttributesTypes(loginParametersTypes);
                playerTagControl.SetAttributesValues(loginParameters.ToArray<object>());
            }
        }

        /// <summary>
        /// Handles the synchronization of the client's tick with the server.
        /// </summary>
        /// <param name="reader">The data stream containing the tick information.</param>
        private void OnClientSynchronizeTick(IDataStream reader) {
            // Read the tick from the data stream.
            int receivedTick = reader.Read<int>();
            // Update the remote tick in the NetworkManager.
            NetworkManager.Instance().UpdateRemoteTick(receivedTick);
        }

        /// <summary>
        /// Handles the event when a connection is successfully established.
        /// </summary>
        /// <param name="reader">The data stream used to read incoming data.</param>
        private void OnConnectedReceived(IDataStream reader) {
            int                 connectionId    = reader.Read<int>();
            string              instanceId      = reader.Read<string>();            
            try {
                if (NetworkManager.Instance().IsServerRestartDetectionEnabled()) {
                    if (!NetworkManager.InstanceIsValid(instanceId)) {
                        if (NetworkManager.HasValidInstance()) {
                            NetworkManager.Instance().RenewServerInstance(instanceId);
                            if ( this.onServerRestarted != null ) {
                                this.onServerRestarted.Invoke(this.GetSocket());
                            }                        
                            if (NetworkGlobalEvents.Instance() != null) {
                                NetworkGlobalEvents.Instance().OnServerRestarted((reader as INetworkStream).GetClient());
                            }
                        } else {
                            NetworkManager.Instance().InitializeServerInstance(instanceId);
                        }
                    }
                }
                // Then notify that player is ready on client side
                this.NotifyPlayerReadyOnClient((reader as INetworkStream).GetClient(), connectionId);
                // Call onConnect event if configured
                if (this.onReceiveConnected != null) {
                    this.onReceiveConnected.Invoke(this.GetSocket());
                }
                // Execute the connection id assigned event
                NetworkManager.Instance().ConnectionIdAssignedEvent(connectionId);
            } finally {
                if (ChannelDirection.Client == this.direction) { 
                    if ( this.GetSocket().GetConnectedClients().Count() > 0) {
                        this.GetSocket().GetConnectedClient(this.GetSocket().GetConnectionID()).SetConnectionId(connectionId);
                    }
                }
                this.GetSocket().SetConnectionID(connectionId);
            }
        }

        /// <summary>
        /// Handles the event when a UDP ping message is received.
        /// </summary>
        /// <param name="reader">The data stream used to read incoming data.</param>
        private void OnUdpPingReceiveReceived(IDataStream reader) {
            int pingTransactionId = reader.Read<int>();
            if (this.waitingPingResponse.ContainsKey(pingTransactionId)) {
                float sendPingTime = this.waitingPingResponse[pingTransactionId];
                this.waitingPingResponse.Remove(pingTransactionId);
                this.pingAverageSamples.Add(NetworkClock.time - sendPingTime);
                // To not store too many samples
                if (this.pingAverageSamples.Count > PING_SAMPLES_AVERAGE) {
                    this.pingAverageSamples.RemoveAt(0);
                }
                // Update current ping time
                this.udpPingTime = this.CalculatePingTime();
            } else if ((reader as INetworkStream).GetClient() != null) {
                // Send ping answer message
                if ( ChannelDirection.Server == this.direction ) {
                    using (DataStream writer = new DataStream()) {
                        writer.Write(pingTransactionId);                        
                        (reader as INetworkStream).GetClient().Send(InternalProtocolEvents.UdpHostPing, writer);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the event when a login attempt has failed.
        /// </summary>
        /// <param name="reader">The data stream used to read incoming data.</param>
        private void OnLoginFailedReceived(IDataStream reader) {
            int errorCode = reader.Read<int>();             
            NetworkManager.Instance().DisableAutoReconnect();
            NetworkManager.Instance().StopNetwork();
            NetworkManager.Instance().OnLoginFailedEvent(new Exception(String.Format("[{0}] Login error : The username or Password doesn't match", errorCode)));            
        }

        /// <summary>
        /// Handles the event when a login attempt is successful.
        /// </summary>
        /// <param name="reader">The data stream used to read incoming data.</param>
        private void OnLoginSucessReceived(IDataStream reader) {
            NetworkManager.Instance().OnLoginSucessEvent((reader as INetworkStream).GetClient());            
        }

        /// <summary>
        /// Handles the event when a connection to a relay server is established.
        /// </summary>
        /// <param name="reader">The data stream used to read incoming data.</param>
        private void OnConnectedOnRelayServerReceived(IDataStream reader) {
            NetworkDebugger.Log("Connected on relay server");
            int                 connectionId    = reader.Read<int>();
            string              instanceId      = reader.Read<string>();
            ushort              playerId        = reader.Read<ushort>();
            string              playerName      = reader.Read<string>();
            bool                isMasterPlayer  = reader.Read<bool>();
            bool                isLobbyServer   = reader.Read<bool>();
            NetworkServerMode   serverMode      = (NetworkServerMode)reader.Read<byte>();
            try {
                if (NetworkManager.Instance().IsServerRestartDetectionEnabled()) {
                    if (!NetworkManager.InstanceIsValid(instanceId)) {
                        if (NetworkManager.HasValidInstance()) {
                            NetworkManager.Instance().RenewServerInstance(instanceId);
                            if ( this.onServerRestarted != null ) {
                                this.onServerRestarted.Invoke(this.GetSocket());
                            }                        
                            if (NetworkGlobalEvents.Instance() != null) {
                                NetworkGlobalEvents.Instance().OnServerRestarted((reader as INetworkStream).GetClient());
                            }
                        } else {
                            NetworkManager.Instance().InitializeServerInstance(instanceId);
                        }
                    }
                }
                // Must flag that this client is connected at a relay server
                NetworkManager.Instance().SetConnectedOnRelayServer(true, isLobbyServer);

                // Register player on client side
                IPlayer newPlayer = new NetworkPlayer(playerId, playerName, (reader as INetworkStream).GetClient());
                newPlayer.SetLocal(true);
                newPlayer.SetMaster(isMasterPlayer);
                NetworkManager.Instance().RegisterNetworkPlayer(newPlayer);
                // Then notify that player is ready on client side
                if (NetworkManager.Instance().IsRunningLogic()) {
                    this.spawnServerPlayer(newPlayer, false, Vector3.zero, false, Quaternion.identity, NetworkPlayerSpawnTime.Automatic, null, true); // Spawn master player server            
                    // Spawn master player on master player
                    if ( newPlayer.IsMaster() ) {
                        if ( this.onClientConnectedOnRelayServer != null ) {
                            this.onClientConnectedOnRelayServer.Invoke(newPlayer);
                        }
                    }
                } else if ( isLobbyServer == false ) {
                    this.NotifyPlayerReadyOnClient((reader as INetworkStream).GetClient(), connectionId, playerId);
                    if ( this.onClientConnectedOnRelayServer != null ) {
                        this.onClientConnectedOnRelayServer.Invoke(newPlayer);
                    }
                } else {
                    if ( this.onClientConnectedOnRelayServer != null ) {
                        this.onClientConnectedOnRelayServer.Invoke(newPlayer);
                    }                    
                }
                // Call onConnect event if configured
                if (this.onReceiveConnected != null) {
                    this.onReceiveConnected.Invoke(this.GetSocket());
                }
            } finally {
                if (ChannelDirection.Client == this.direction) { 
                    if ( this.GetSocket().GetConnectedClients().Count() > 0) {
                        this.GetSocket().GetConnectedClient(this.GetSocket().GetConnectionID()).SetConnectionId(connectionId);
                    }
                }
                this.GetSocket().SetConnectionID(connectionId);
            }
        }

        /// <summary>
        /// Handles the event when the master player is updated.
        /// </summary>
        /// <param name="reader">The data stream containing the update information.</param>
        private void OnUpdateMasterPlayerReceived(IDataStream reader) {
            ushort masterPlayerId = reader.Read<ushort>();
            if (NetworkManager.Instance().HasLocalPlayer()) {
                IPlayer newMasterPlayer = NetworkManager.Instance().GetLocalPlayer<IPlayer>();
                NetworkManager.Instance().UpdateMasterPlayer(newMasterPlayer.GetPlayerId().Equals(masterPlayerId));
                // Get client
                NetworkClient currentClient = ((newMasterPlayer as NetworkPlayer).GetClient() as NetworkClient);
                // Get clients
                int clientsCount = reader.Read<int>();
                while (clientsCount > 0) {
                    clientsCount--;
                    int connectionId = reader.Read<int>();
                    ushort playerId  = reader.Read<ushort>();
                    // Create new client connection
                    if (currentClient.GetConnectionId() != connectionId) {
                        // Create a fake transport system
                        NetworkClient clientTransport = new NetworkClient(connectionId);
                        clientTransport.SetTransport(new RelayTransportClient(currentClient.GetTransport()));
                        clientTransport.SetChannel(currentClient.GetChannel());
                        // Create player
                        IPlayer newPlayer = new NetworkPlayer(playerId, String.Format("Player_{0}", playerId), clientTransport);
                        newPlayer.SetLocal(false);                        
                        newPlayer.SetMaster(false);                        
                        if (NetworkManager.Instance().IsLobbyControlEnabled()) {
                            newPlayer.SetLobbyId(newMasterPlayer.GetLobbyId());
                        }
                        // Register player
                        NetworkManager.Instance().RegisterNetworkPlayer(newPlayer);                        
                        // Register fake connection on channel
                        currentClient.GetChannel().RegisterClient(clientTransport);
                        /// Register client for this new MasterPlayer
                        INetworkElement networkElement = NetworkManager.Instance().GetObjectOnClient<INetworkElement>(playerId);
                        if (networkElement != null) {
                            NetworkManager.Instance().RegisterNetworkClient(clientTransport, networkElement);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the event when a client is disconnected from the relay.
        /// </summary>
        /// <param name="reader">The data stream containing the disconnect information.</param>
        private void OnClientDisconnectedFromRelay(IDataStream reader) { 
            int             disconnectedClientId    = reader.Read<int>();
            IClient         disconnectedClient      = this.GetSocket().GetConnectedClient(disconnectedClientId);
            if (disconnectedClient != null) {
                NetworkDebugger.Log("OnClientDisconnectedFromRelay [1]");
                NetworkManager.Instance().FireOnClientDisconnectedFromServer(disconnectedClient);
            } else {
                NetworkDebugger.Log("OnClientDisconnectedFromRelay [2]");
                NetworkManager.Instance().FireOnClientDisconnectedFromServer((reader as INetworkStream).GetClient());
            }
        }

        /// <summary>
        /// Handles the event when a client's network ID is updated.
        /// </summary>
        /// <param name="reader">The data stream containing the update information.</param>
        private void OnClientNetworkIdUpdated(IDataStream reader) { 
            int         clientNetworkId     = reader.Read<int>();
            int         networkObjectId     = reader.Read<int>();
            IClient     targetClient        = ((reader as INetworkStream).GetClient().GetChannel() as Channel).GetConnectedClient(clientNetworkId);
            if (targetClient != null) {
                (targetClient as NetworkClient).SetNetworkObjectId(networkObjectId);
            } else { 
                ((reader as INetworkStream).GetClient() as NetworkClient).SetNetworkObjectId(networkObjectId);
            }            
        }

        /// <summary>
        /// Handles the event when a client is forced to disconnect.
        /// </summary>
        /// <param name="reader">The data stream containing the disconnect information.</param>
        private void OnForceToDisconnectClient(IDataStream reader) { 
            IClient         targetClient    = (reader as INetworkStream).GetClient();
            NetworkPlayer   originPlayer    = NetworkManager.Instance().GetPlayer<NetworkPlayer>(targetClient);
            if (targetClient != null) {
                (targetClient as NetworkClient).GetChannel().UnregisterClient(targetClient);
                if ( originPlayer != null ) {
                    NetworkManager.Instance().UnregisterNetworkPlayer(originPlayer);
                }
            }
        }

        /// <summary>
        /// Handles the player created on relay server message
        /// </summary>
        /// <param name="reader">The data structure containing player creation</param>
        private void OnPeerCreatedOnRelay(IDataStream reader) {
            ushort  playerId    = reader.Read<ushort>();
            string  playerIp    = reader.Read<string>();
            ushort  playerPort  = reader.Read<ushort>();
            bool    isAvaiable  = reader.Read<bool>();

            NetworkDebugger.LogDebug(String.Format("Player Created On Relay {0} -> {1}:{2} is {3}", playerId, playerIp, playerPort, (isAvaiable ? "Avaiable" : "Unavaiable")));

            IClient             targetClient    = (reader as INetworkStream).GetClient();
            ITransportClient    transport       = (this.GetSocket().GetTransport() as ITransportClient);
            transport.RegisterPeer(new TransportPeer(playerId, playerIp, playerPort, (ITransportClient client) => {
                transport.GetPeer(playerId).SetConnected(client.IsConnected());
                // Tell to the server if this peer is connected or not
                using (DataStream writer = new DataStream()) {
                    writer.Write(playerId);                                  // Player ID
                    writer.Write(transport.GetPeer(playerId).IsConnected()); // Connection was stablished ?
                    // Send message
                    targetClient.Send(RelayServerEvents.PeerConnectionStatus, writer, DeliveryMode.Reliable);
                }
                NetworkDebugger.LogDebug(String.Format("Network Peer \"{0}\" {1} on port {2}", playerId,
                                                                                               (transport.GetPeer(playerId).IsConnected() ? "Connected" : "Disconnected"),
                                                                                               playerPort));
            }));
        }

        /// <summary>
        /// Handles the player destroyed from relay server message
        /// </summary>
        /// <param name="reader">The data structure containing player destroy</param>
        private void OnPeerDestroyedOnRelay(IDataStream reader) {
            ushort playerId = reader.Read<ushort>();

            NetworkDebugger.LogDebug(String.Format("Player Destroyed On Relay {0}", playerId));
            ITransportClient transport = (this.GetSocket().GetTransport() as ITransportClient);
            transport.UnregisterPeer(playerId);
        }

        /// <summary>
        /// Handles with peer to peer initialize message
        /// </summary>
        /// <param name="reader">The data structure containing peer to peer initialization</param>
        private void OnPeerToPeerInitialize(IDataStream reader) {
            IClient         targetClient    = (reader as INetworkStream).GetClient();
            ITransportClient transport      = (this.GetSocket().GetTransport() as ITransportClient);

            ushort playerId         = reader.Read<ushort>();
            ushort peerToPeerPort   = (ushort)reader.Read<int>();

            // Now i'm going to MAP a new port to be used by peer to peer messages
            NetworkManager.Instance().MapPeerToPeerPort(peerToPeerPort , (bool mapped) => {
                // Start server to listen peer to peer client's connection
                transport.InitializePeerToPeerServer(peerToPeerPort);

                // Send back to server if player is acessible or not ( ir peer to peer can send direct messages to this client )
                using (DataStream writer = new DataStream()) {
                    writer.Write(playerId);         // Player ID
                    writer.Write(peerToPeerPort);   // Peer to Peer port
                    writer.Write(mapped);           // Port was mapped ?
                    // Send message
                    targetClient.Send(RelayServerEvents.PlayerPeerAvaiable, writer, DeliveryMode.Reliable);
                }
                NetworkDebugger.LogDebug(String.Format("Network Peer \"{0}\" {1} on port {2}", playerId, 
                                                                                               (mapped ? "Initialized" : "Failed"),
                                                                                               peerToPeerPort));
            });
        }   

        /// <summary>
        /// Event handler for when a player's peer becomes available or not.
        /// </summary>
        /// <param name="reader">The data stream containing the player's information.</param>
        private void OnPlayerPeerAvaiable(IDataStream reader) {
            ushort  playerId        = reader.Read<ushort>();
            ushort  peerToPeerPort  = reader.Read<ushort>();
            bool    avaiable        = reader.Read<bool>();
            NetworkPlayer originPlayer = NetworkManager.Instance().GetPlayer<NetworkPlayer>(playerId);
            if (originPlayer != null) {
                ITransportClient    transport       = (this.GetSocket().GetTransport() as ITransportClient);
                NetworkPlayer       playerPeer      = (originPlayer as NetworkPlayer);
                IClient             playerClient    = playerPeer.GetClient();
                playerPeer.SetPeerToPeerPort(peerToPeerPort);
                playerPeer.SetPeerAvaiable(avaiable);
                originPlayer.SetPeerAvaiable(avaiable);                
                
                // Send to all clients on same lobby
                using (DataStream writer = new DataStream()) {
                    writer.Write(playerPeer.GetPlayerId());         // Player ID
                    writer.Write(playerClient.GetIp());             // Network client IP
                    writer.Write(playerPeer.GetPeerToPeerPort());   // Network client port
                    writer.Write(playerPeer.IsPeerAvaiable());      // Peer is avaiable to receive direct messages ?
                    // Send to all clients on same lobby ( if lobby is enabled )
                    foreach (NetworkPlayer playerTo in NetworkManager.Instance().GetPlayers<NetworkPlayer>((originPlayer != null) ? originPlayer.GetLobbyId() : (ushort)0)) {
                        if (playerTo != originPlayer) {
                            playerTo.GetClient().Send(RelayServerEvents.CreateNetworkPeer, writer, DeliveryMode.Reliable);
                        }
                    }
                }
                NetworkDebugger.LogDebug(String.Format("[PlayerPeerAvaiable] Player [{0}] Peer {1}", playerId, avaiable ? "Avaiable" : "Unavaiable"));
            } else {
                NetworkDebugger.LogDebug(String.Format("[PlayerPeerAvaiable] Player [{0}] not exists", playerId));
            }
        }

        /// <summary>
        /// Event handler for when a player's peer becomes available or not.
        /// </summary>
        /// <param name="reader">The data stream containing the player's information.</param>
        private void OnPlayerPeerConnectionStatus(IDataStream reader) {
            ushort  playerId            = reader.Read<ushort>();
            bool    connectionStatus    = reader.Read<bool>();
            NetworkPlayer originPlayer  = NetworkManager.Instance().GetPlayer<NetworkPlayer>(playerId);
            if (originPlayer != null) {
                originPlayer.SetPeerAvaiable(connectionStatus);
                IClient playerClient = (originPlayer as NetworkPlayer).GetClient();                
                NetworkDebugger.LogDebug(String.Format("[PlayerPeerConnectionStatus] Player [{0}] Peer {1}", playerId, connectionStatus ? "Connected" : "Disconnected"));
            } else {
                NetworkDebugger.LogDebug(String.Format("[PlayerPeerConnectionStatus] Player [{0}] not exists", playerId));
            }
        }

        /// <summary>
        /// Handles the request to create a new lobby.
        /// </summary>
        /// <param name="reader">The data stream containing the lobby creation information.</param>
        private void OnLobbyCreationRequest(IDataStream reader) { 
            string lobbyName = reader.Read<string>();
            try {
                if (!string.IsNullOrEmpty(lobbyName)) {
                    if ((lobbyName.Length >= NetworkLobbyManager.MIN_LOBBY_LENGHT_NAME) &&
                        (lobbyName.Length <= NetworkLobbyManager.MAX_LOBBY_LENGHT_NAME)) {
                        IClient         originClient    = (reader as INetworkStream).GetClient();
                        NetworkPlayer   originPlayer    = NetworkManager.Instance().GetPlayer<NetworkPlayer>(originClient);
                        // Create lobby
                        ILobby          newLobby        = NetworkManager.Lobbies.CreateLobby(lobbyName);
                        // Send lobby creation sucess
                        using (DataStream writer = new DataStream()) {
                            writer.Write(newLobby.GetLobbyId());
                            (reader as INetworkStream).GetClient().Send(LobbyServerEvents.LobbyCreatedSucess, writer, DeliveryMode.Reliable);
                        }
                        originPlayer.SetLobbyId(newLobby.GetLobbyId());
                        originPlayer.SetMaster(true); // Creator will always be the master of lobby ( by defsault )
                        // Send lobby refresh to all connected players
                        this.SendLobbyRefresh();
                    } else {
                        throw new Exception(String.Format("Lobby name must have at least {0} characters and less than", NetworkLobbyManager.MIN_LOBBY_LENGHT_NAME, NetworkLobbyManager.MAX_LOBBY_LENGHT_NAME));
                    }
                } else {
                    throw new Exception("A name must be provided to create a new lobby");
                }
            } catch (Exception err) {
                // Send lobby creation failed
                using (DataStream writer = new DataStream()) {
                    writer.Write(err.Message);
                    (reader as INetworkStream).GetClient().Send(LobbyServerEvents.LobbyCreatedFailed, writer, DeliveryMode.Reliable);
                }
            }
        }

        /// <summary>
        /// Handles the finish lobby event message
        /// </summary>
        /// <param name="reader">The data stream containing the lobby creation information.</param>
        private void OnLobbyFinished(IDataStream reader) {
            ushort lobbyId = reader.Read<ushort>();
            if ( NetworkManager.Lobbies.HasLobby(lobbyId) ) {
                NetworkManager.Lobbies.UnregisterLobby(lobbyId); // Unregister lobby from current lobbies
            }
        }

        /// <summary>
        /// Handles the request to join a lobby.
        /// </summary>
        /// <param name="reader">The data stream containing the lobby ID.</param>
        private void OnLobbyJoinRequest(IDataStream reader) { 
            ushort lobbyId = reader.Read<ushort>();
            try {
                IClient         originClient    = (reader as INetworkStream).GetClient();
                NetworkPlayer   originPlayer    = NetworkManager.Instance().GetPlayer<NetworkPlayer>(originClient);
                ILobby          joinedLobby     = NetworkManager.Lobbies.GetLobby(lobbyId);
                if ( joinedLobby != null ) {
                    using (DataStream writer = new DataStream()) {
                        writer.Write(lobbyId);
                        originClient.Send(LobbyServerEvents.LobbyJoinSucess, writer, DeliveryMode.Reliable);
                    }
                    // Send lobby refresh to all connected players
                    joinedLobby.RegisterPlayer(originPlayer);
                }                
            } catch (Exception err) {
                // Send lobby creation failed
                using (DataStream writer = new DataStream()) {
                    writer.Write(lobbyId);
                    (reader as INetworkStream).GetClient().Send(LobbyServerEvents.LobbyJoinFailed, writer, DeliveryMode.Reliable);
                }
                NetworkDebugger.LogDebug(String.Format("[OnLobbyJoinRequest] [{0}]", err.Message));
            }
        }

        /// <summary>
        /// Handles the request of all player into a lobbies.
        /// </summary>
        /// <param name="reader">The data stream containing the lobby ID.</param>
        private void OnLobbyPlayersRequest(IDataStream reader) {
            ushort lobbyId = reader.Read<ushort>();
            IClient originClient = (reader as INetworkStream).GetClient();
            NetworkPlayer originPlayer = NetworkManager.Instance().GetPlayer<NetworkPlayer>(originClient);
            ILobby playersLobby = NetworkManager.Lobbies.GetLobby(lobbyId);
            if (playersLobby != null) {
                using (DataStream writer = new DataStream()) {
                    writer.Write(lobbyId);
                    playersLobby.GetPlayers().Count();
                    foreach(IPlayer player in playersLobby.GetPlayers()) {
                        writer.Write(player.GetPlayerId());
                        writer.Write(player.GetPlayerName());
                    }
                    originClient.Send(LobbyServerEvents.LobbyPlayersListRefresh, writer, DeliveryMode.Reliable);
                }                    
            }            
        }

        /// <summary>
        /// Handles the request of all player into a lobbies.
        /// </summary>
        /// <param name="reader">The data stream containing the lobby ID.</param>
        private void OnLobbyPlayersRefresh(IDataStream reader) {
            ushort          lobbyId         = reader.Read<ushort>();
            ILobby          targetLobby     = NetworkManager.Lobbies.GetLobby(lobbyId);
            if (targetLobby != null) {
                targetLobby.ClearPlayers(); // Remove all players from lobby
                ushort playersCount = reader.Read<ushort>();
                string[] players = new string[playersCount];
                int playersIndex = 0;
                while (playersCount > 0) {
                    ushort playerId = reader.Read<ushort>();
                    players[playersIndex++] = reader.Read<string>();
                    // Put into lobby
                    IPlayer playerOnLobby = NetworkManager.Instance().GetPlayer<IPlayer>(playerId);
                    if (playerOnLobby != null) {
                        targetLobby.RegisterPlayer(playerOnLobby);
                    }
                    playersCount--;
                }
                NetworkManager.Instance().OnLobbyPlayersRefreshEvent(players);
            }
        }

        /// <summary>
        /// Handles the successful creation of a lobby.
        /// </summary>
        /// <param name="reader">The data stream containing the lobby ID.</param>
        private void OnLobbyCreationSucess(IDataStream reader) { 
            ushort          lobbyId         = reader.Read<ushort>();
            IClient         originClient    = (reader as INetworkStream).GetClient();
            NetworkPlayer   originPlayer    = NetworkManager.Instance().GetPlayer<NetworkPlayer>(originClient);
            // Set lobby
            originPlayer.SetLobbyId(lobbyId);
            // Send player ready to be spawned and complete the process
            originPlayer.SetMaster(true); // Mask this player as master of his own lobby
            this.spawnServerPlayer(originPlayer, false, Vector3.zero, false, Quaternion.identity, NetworkPlayerSpawnTime.Automatic, null, true); // Spawn master player server
            // Trigger final user event
            NetworkManager.Instance().OnClientLobbyCreationSucessEvent(lobbyId);
        }

        /// <summary>
        /// Handles the failure of lobby creation.
        /// </summary>
        /// <param name="reader">The data stream containing the error message.</param>
        private void OnLobbyCreationFailed(IDataStream reader) { 
            string  errorMessage = reader.Read<string>();
            // Invoke user lobby failed event
            NetworkManager.Instance().OnClientLobbyCreationFailedEvent(errorMessage);
        }

        /// <summary>
        /// Handles the successful joining of a lobby.
        /// </summary>
        /// <param name="reader">The data stream containing the lobby ID.</param>
        private void OnLobbyJoinFailed(IDataStream reader) {
            ushort  lobbyId         = reader.Read<ushort>();
            // Trigger final user event
            NetworkManager.Instance().OnClientLobbyJoinFailedEvent(lobbyId);
        }

        /// <summary>
        /// Handles the failed joining of a lobby.
        /// </summary>
        /// <param name="reader">The data stream containing the lobby ID.</param>
        private void OnLobbyJoinSucess(IDataStream reader) {
            ushort lobbyId = reader.Read<ushort>();
            ILobby joinedLobby = NetworkManager.Lobbies.GetLobby(lobbyId);
            IClient originClient = (reader as INetworkStream).GetClient();
            NetworkPlayer originPlayer = NetworkManager.Instance().GetPlayer<NetworkPlayer>(originClient);
            originPlayer.SetLobbyId(lobbyId);
            joinedLobby.RegisterPlayer(originPlayer);
            // Send ready to master server
            this.NotifyPlayerReadyOnClient((reader as INetworkStream).GetClient(), (originClient as NetworkClient).GetConnectionId(), originPlayer.GetPlayerId());
            // Trigger final user event
            NetworkManager.Instance().OnClientLobbyJoinSucessEvent(lobbyId);
        }

        /// <summary>
        /// Handles the request for the lobby list.
        /// </summary>
        /// <param name="reader">The data stream containing the request.</param>
        private void OnLobbyListRequest(IDataStream reader) {
            this.SendLobbyRefresh((reader as INetworkStream).GetClient()); // Send lobby list to client
        }

        /// <summary>
        /// Handles the refresh of the lobby list.
        /// </summary>
        /// <param name="reader">The data stream containing the lobby list.</param>
        private void OnLobbyListRefresh(IDataStream reader) {
            int lobbiesCount = reader.Read<int>();
            List<ushort> lobbies = new List<ushort>();
            List<ushort> lobbiesToRemove = new List<ushort>();
            while (lobbiesCount > 0) {
                lobbiesCount--;
                ushort lobbyId      = reader.Read<ushort>();
                string lobbyName    = reader.Read<string>();
                if ( !NetworkManager.Lobbies.HasLobby(lobbyId) ) {
                    NetworkManager.Lobbies.CreateLobby(lobbyId, lobbyName);
                }
                lobbies.Add(lobbyId);
            }
            foreach (ILobby lobby in NetworkManager.Lobbies.GetLobbies()) {
                if ( !lobbies.Contains(lobby.GetLobbyId()) ) {
                    lobbiesToRemove.Add(lobby.GetLobbyId());
                }
            }
            while (lobbiesToRemove.Count > 0) {
                NetworkManager.Lobbies.CloseLobby(lobbiesToRemove[0]);
                lobbiesToRemove.RemoveAt(0);
            }
        }

        /// <summary>
        /// Sends a lobby refresh to the specified client or all connected clients.
        /// </summary>
        /// <param name="targetClient">The target client to send the lobby refresh to. If null, sends to all connected clients.</param>

        private void SendLobbyRefresh(IClient targetClient = null) {
            List<IClient> targetClients = (targetClient == null) ? this.GetSocket().GetConnectedClients() : null;
            if (targetClients != null) {
                foreach (NetworkClient client in targetClients) {
                    // Send lobby creation failed
                    using (DataStream writer = new DataStream()) {
                        writer.Write(NetworkManager.Lobbies.GetLobbiesCount()); // Lobby count
                        foreach (ILobby lobby in NetworkManager.Lobbies.GetLobbies()) {
                            writer.Write(lobby.GetLobbyId());
                            writer.Write(lobby.GetLobbyName());
                        }
                        client.Send(LobbyServerEvents.LobbyListRefresh, writer);
                    }
                }
            }
        }
        
        /// <summary>
        /// Notify client regard to InScene objects
        /// </summary>
        /// <param name="connectedClient">Client to advice</param>
        public void NotifyNetworkInSceneUpdate(IClient connectedClient) {
            try {
                using (DataStream writer = new DataStream()) {
                    int[] inSceneIds = NetworkManager.Instance().GetInSceneObjectsIds();
                    writer.Write(inSceneIds.Length); // Objects count
                    foreach (int inSceneObjectId in inSceneIds) {
                        writer.Write(inSceneObjectId);
                    }
                    connectedClient.Send(CoreGameEvents.NetworkStaticSpawnUpdate, writer, DeliveryMode.Reliable);
                }
            } catch(Exception err) {
                NetworkDebugger.LogError(err.Message);
            }
        }

        /// <summary>
        /// Handles the application quitting by stopping the socket.
        /// </summary>
        void OnApplicationQuit() {
            this.socket.Stop();
        }
    }
}

#pragma warning restore