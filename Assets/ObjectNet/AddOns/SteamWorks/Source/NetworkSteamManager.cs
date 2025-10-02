#pragma warning disable
#if STEAMWORKS_NET
using Steamworks;
using System.Linq;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;
using com.onlineobject.objectnet.steamworks;

namespace com.onlineobject.objectnet {
    public class NetworkSteamManager : MonoBehaviour, IInformationProvider {

        public class SteamClientInfo {
            public int      NetworkId;
            public ushort   PlayerId;
            public ushort   PlayerIndex;
            public ulong    SteamId;
            public bool     Alive;
            public float    TimeOut;
            public IClient  Connection;

            public SteamClientInfo(int networkId, ushort playerId, ushort playerIndex, ulong steamId) {
                this.NetworkId      = networkId;
                this.PlayerId       = playerId;
                this.PlayerIndex    = playerIndex;
                this.SteamId        = steamId;
                this.Alive          = true;
                this.TimeOut        = (Time.time + CLIENT_DEFAULT_KICK_TIME);
            }
        }

        // Flag is this object shall keep persistent between scenes
        [SerializeField]
        private bool dontDestroyOnLoad = true;

        [SerializeField]
        private bool debugLogger = false;

#if STEAMWORKS_NET
        [SerializeField]
        private ELobbyType lobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;

        [SerializeField]
        private ELobbyDistanceFilter lobbyDistance = ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide;
#endif

        [SerializeField]
        private bool autoRefresh = false;

        [SerializeField]
        private int refreshRate = 1;

        [SerializeField]
        private int disconnectDetection = 1;

        [SerializeField]
        private int maximumOfPlayers = 4;

        [SerializeField]
        private int reconnectionDelay = 4000;

        [SerializeField]
        private bool hostMigration = false;

        [SerializeField]
        private bool dontMigrateDuringAutoLoadPause = false;

        [SerializeField]
        private bool despawnDisconnectedServerPlayer = false;

        [SerializeField]
        private bool despawnDisconnectedClientsAfterMigration = false;

        [SerializeField]
        private int reconnectionClientToolerance = WAIT_CLIENT_RECONNECTION_TOOLERANCE;

        [SerializeField]
        private GameObject hostLeaveLobbyProviderObject;

        [SerializeField]
        private MonoBehaviour hostLeaveLobbyComponent;

        [SerializeField]
        private String hostLeaveLobbyMethod;

        [SerializeField]
        private SteamEventsEntry steamEvents;

        private string creationLobbyName;

        private ulong currentLobbyID;

        private SteamLobby currentLobby;

        private Action<bool> onPlayerJoinedOnLobby;

        private Action filterProcedure;

        // List containing all current lobbies
        private List<SteamLobby> currentLobbies = new List<SteamLobby>();

        private Dictionary<string, string> metadata = new Dictionary<string, string>();

        private float nextRefresh = 0f;

        /// ----------------------------------------------------------------------------------------------
        /// Host migration support attributes
        /// ----------------------------------------------------------------------------------------------

        private bool hostInstance = false; // Flag if this instance is a host of matche

        private bool clientReconnecting = false; // Flag if client is trying to reconnect

        private float nextDisconnectionCheck = 0f;

        private float nextConnectionRetry = 0f;

        private float nextDespawnCheck = 0f;

        private float nextAliveNotify = 0f;

        private bool connectedAtServer = false;

        private bool serverIsOnline = false;        

        private Dictionary<ulong, SteamClientInfo> steamIdMap = new Dictionary<ulong, SteamClientInfo>();

        private MD5 md5Hasher = MD5.Create();

        private int lastSendPlayerIndexFactory = 0;

        private bool serverNotified = false;

#if STEAMWORKS_NET
        private CSteamID currentLobbyOwner = default(CSteamID);

        protected Callback<GameLobbyJoinRequested_t>    JoinRequest;
        protected Callback<LobbyCreated_t>              LobbyCreated;
        protected Callback<LobbyEnter_t>                LobbyEntered;
        protected Callback<LobbyMatchList_t>            LobbyListRefresh;
        protected Callback<LobbyDataUpdate_t>           LobbyDataUpdated;
#endif

        const int   WAIT_CLIENT_RECONNECTION_TOOLERANCE = 10; // 10 Seconds

        const float CLIENT_RECONNECTION_RETRY           = 0.10f; // 100 ms

        const float CLIENT_DEFAULT_KICK_TIME            = 4.00f; // 4 Seconds

        const float DESPAWN_DISCONNECTION_CHECK_TIME    = 1.00f; // 1 Seconds

        const float ALIVE_INTERVAL_RATE                 = 3.00f; // 1 Seconds

        const float DISCONNECT_DETECTION_MULTIPLIER     = 0.100f; // 100 Milliseconds

        private static NetworkSteamManager instance;

        /// <summary>
        /// Provides access to the singleton instance of the NetworkManager.
        /// </summary>
        /// <returns>The singleton instance of the NetworkManager.</returns>
        public static NetworkSteamManager Instance() {
            // Check if an instance already exists
#if DEBUG
            if (!InstanceExists()) {
                // Warn the user if the application is running and no instance is found
                if (Application.isPlaying) {
                    NetworkDebugger.LogWarning("[ NetworkSteam ] Could not find the instance of object. Please ensure you have added the NetworkSteam Prefab to your scene.");
                }
            }
#endif
            // Return the singleton instance
            return NetworkSteamManager.instance;
        }

        /// <summary>
        /// Detect ig another NetworkSteam is already instantiated
        /// </summary>
        /// <returns>True if NetworkSteam already exists, otherwise false</returns>
        public bool DetectDuplicated() {
            return (NetworkSteamManager.instance != null);
        }

        /// <summary>
        /// Flag current instance of NetworkSteam as the in use instance
        /// </summary>
        private void SetInstance() {
            NetworkSteamManager.instance = this;
        }

        /// <summary>
        /// Checks if an instance of NetworkSteam already exists.
        /// </summary>
        /// <returns>True if an instance exists, false otherwise.</returns>
        private static bool InstanceExists() {
            return (NetworkSteamManager.instance != null); // Check if the instance is not null
        }

        /// <summary>
        /// Return if this instance is the host of the matched
        /// </summary>
        /// <returns>true is this is the host, otherwise false</returns>
        public bool IsHostInstance() {
            return this.hostInstance;
        }

        /// <summary>
        /// Return if this client is trying to reconnect with host
        /// </summary>
        /// <returns>true is trying to rewconnect, otherwise false</returns>
        public bool IsReconnecting() {
            return this.clientReconnecting;
        }
        
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake() {
            if ((this.dontDestroyOnLoad) &&
                (this.DetectDuplicated())) {
                DestroyImmediate(this.gameObject);
            } else {
                // Flag on base class
                if (this.dontDestroyOnLoad) {
                    DontDestroyOnLoad(this);
                }
                this.SetInstance(); // Flag instance to be the current object
            }
        }

        private void Start() {
#if STEAMWORKS_NET
            this.LobbyCreated       = Callback<LobbyCreated_t           >.Create(OnLobbyCreated);
            this.JoinRequest        = Callback<GameLobbyJoinRequested_t >.Create(OnJoinRequest);
            this.LobbyEntered       = Callback<LobbyEnter_t             >.Create(OnLobbyEntered);
            this.LobbyListRefresh   = Callback<LobbyMatchList_t         >.Create(OnLobbyRefreshResult);
            // If host migration is enabled i need to listen lobby changes
            if (this.hostMigration) {
                this.LobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdated);
            }
#endif
            // Register player index update event
            NetworkManager.RegisterEvent(IntegrationEvent.UpdatePlayerIndexFactory, this.OnPlayerIndexUpdated);
            NetworkManager.RegisterEvent(IntegrationEvent.UpdatePlayerInfoData,     this.OnPlayerInfoReceived);
            NetworkManager.RegisterEvent(IntegrationEvent.NotifySteamUserOnline,    this.OnSteamPlayerNotified);
            NetworkManager.RegisterEvent(IntegrationEvent.NotifySteamUserAlive,     this.OnSteamPlayerAlive);
            NetworkManager.RegisterEvent(IntegrationEvent.RequestSteamServerStatus, this.OnRequestSteamServerStatus);
            NetworkManager.RegisterEvent(IntegrationEvent.SteamServerOnline,        this.OnSteamServerOnline);
#if STEAMWORKS_NET
            // If Host Migration is enabled i need to set login values forced just in case of developper miss to do it
            if (this.hostMigration == true) {
                this.ForceToEnableLogin();
            }
#endif
        }
#if STEAMWORKS_NET
        private void LateUpdate() {
            if (this.autoRefresh) {
                if (this.nextRefresh < Time.time) {
                    this.nextRefresh = (Time.time + (float)(this.refreshRate * DISCONNECT_DETECTION_MULTIPLIER));
                    this.RequestLobbyList(this.filterProcedure);
                }
            }
            if ((this.hostMigration) && 
                ((this.dontMigrateDuringAutoLoadPause == false) && (NetworkManager.Instance().IsToAutoLoadSceneElements() == true))) {
                if (NetworkManager.Instance() != null) {
                    if (NetworkManager.Instance().IsClientConnection() == true) {
                        if (this.connectedAtServer == true) {
                            if (this.nextDisconnectionCheck < Time.time) {
                                this.nextDisconnectionCheck = (Time.time + (float)(this.disconnectDetection * DISCONNECT_DETECTION_MULTIPLIER));
                                if (NetworkManager.Instance().IsConnected() == false) {
                                    // First i need to close network connection
                                    this.connectedAtServer  = false;
                                    this.serverNotified     = false;
                                    this.serverIsOnline     = false;
                                    this.clientReconnecting = true; // Flag that this client will try to reconnect
                                }
                            }
                            // Notify server that this player is online
                            if (this.serverNotified == false) {
                                NetworkElement playerElement = NetworkManager.Instance().GetLocalPlayerElement<NetworkElement>();
                                if (playerElement != null) {
                                    this.serverNotified = true;
                                    using (DataStream writer = new DataStream()) {
                                        writer.Write(playerElement.GetNetworkId());
                                        writer.Write(SteamUser.GetSteamID().m_SteamID);
                                        NetworkManager.Instance().Send(IntegrationEvent.NotifySteamUserOnline, writer, DeliveryMode.Reliable);
                                    }
                                }
                            }
                            // Send to server that i'm alive to not kick me from server
                            if (this.nextAliveNotify < Time.time) {
                                this.nextAliveNotify = (Time.time + ((float)this.reconnectionClientToolerance / ALIVE_INTERVAL_RATE)); // Will send 3 tries during the tooleerance to avoid miss disconnection
                                using (DataStream writer = new DataStream()) {
                                    writer.Write(SteamUser.GetSteamID().m_SteamID);
                                    NetworkManager.Instance().Send(IntegrationEvent.NotifySteamUserAlive, writer, DeliveryMode.Reliable);
                                }
                                this.Log(string.Format("[NotifySteamUserAlive] Notifyed sent from client [{0}]", SteamUser.GetSteamID().m_SteamID));
                            }
                        } else if (NetworkManager.Instance().IsConnected() == true) {
                            if (this.connectedAtServer == false) {
                                this.connectedAtServer  = true;
                                // Re-Validate each player timeout to not disconnect player direclty
                                foreach (SteamClientInfo steamClientPlayer in this.steamIdMap.Values) {
                                    SteamClientInfo forSteamClient = this.steamIdMap[steamClientPlayer.SteamId];
                                    forSteamClient.Alive    = true;
                                    forSteamClient.TimeOut  = (Time.time + (float)this.reconnectionClientToolerance);
                                }

                                // Request server to check if he is online
                                using (DataStream writer = new DataStream()) {
                                    writer.Write(SteamUser.GetSteamID().m_SteamID); // Send myu steam id
                                    NetworkManager.Instance().Send(IntegrationEvent.RequestSteamServerStatus, writer, DeliveryMode.Reliable);
                                }

                            }
                            NetworkManager.Instance().EnableAutoReconnect();
                            // Despawn timeout players ( On Client Instance )
                            if (this.despawnDisconnectedClientsAfterMigration == true) {
                                if (this.serverIsOnline) {
                                    if (this.nextDespawnCheck < Time.time) {
                                        this.nextDespawnCheck = (Time.time + DESPAWN_DISCONNECTION_CHECK_TIME);
                                        foreach (SteamClientInfo steamClientPlayer in this.steamIdMap.Values) {
                                            if (steamClientPlayer.TimeOut < Time.time) {
                                                if (steamClientPlayer.SteamId != SteamUser.GetSteamID().m_SteamID) {
                                                    SteamClientInfo forSteamClient = this.steamIdMap[steamClientPlayer.SteamId];
                                                    forSteamClient.Alive = false;
                                                    if ((forSteamClient.Connection != null) && 
                                                        (forSteamClient.Connection.GetTransport() != null)) {
                                                        if (forSteamClient.Connection.GetTransport().IsConnected()) {
                                                            forSteamClient.Connection.GetTransport().Disconnect();
                                                        }
                                                        (forSteamClient.Connection.GetTransport() as SteamNetworkClient).FlagDisconnectedOnServer();
                                                    }                                                        
                                                    this.Log(string.Format("[DestroyOnClient] Sent from [{0}]", steamClientPlayer.SteamId));
                                                    NetworkManager.Instance().DestroyOnClient(steamClientPlayer.NetworkId); // Destroy player
                                                } else {
                                                    SteamClientInfo forSteamClient = this.steamIdMap[steamClientPlayer.SteamId];
                                                    forSteamClient.Alive = true;
                                                }
                                            }
                                        }
                                        foreach (ulong clientSteamId in this.steamIdMap.Keys.ToArray()) {
                                            if (this.steamIdMap[clientSteamId].Alive == false) {
                                                this.steamIdMap.Remove(clientSteamId);
                                            }
                                        }
                                    }
                                }
                                // Sen to server that i'm alive to not kick me from server
                                if (this.nextAliveNotify < Time.time) {
                                    this.nextAliveNotify = (Time.time + ((float)this.reconnectionClientToolerance / ALIVE_INTERVAL_RATE)); // Will send 3 tries during the tooleerance to avoid miss disconnection
                                    using (DataStream writer = new DataStream()) {
                                        writer.Write(SteamUser.GetSteamID().m_SteamID);
                                        NetworkManager.Instance().Send(IntegrationEvent.NotifySteamUserAlive, writer, DeliveryMode.Reliable);
                                    }
                                    this.Log(string.Format("[NotifySteamUserAlive] Notifyed sent from client [{0}]", SteamUser.GetSteamID().m_SteamID));
                                }
                            }
                        } else if (this.nextConnectionRetry < Time.time) {
                            this.nextConnectionRetry = (Time.time + CLIENT_RECONNECTION_RETRY);
                            // Need, i'm still client ? im going to reconnect to the new server
                            String lobbyAddressToJoin = SteamMatchmaking.GetLobbyData(new CSteamID(this.currentLobbyID), "HostAddress");
                            this.Log(string.Format("Trying to connect to HostAddress [{0}]", lobbyAddressToJoin));

                            this.connectedAtServer  = false;
                            this.hostInstance       = false; // Flag that this instance isn't the host
                            NetworkManager.Instance().DisableAutoReconnect();
                            NetworkManager.Instance().StopNetwork();
                            NetworkManager.Instance().ConfigureMode(NetworkConnectionType.Client);
                            NetworkManager.Instance().SetServerAddress(lobbyAddressToJoin);
                            NetworkManager.Instance().StartNetwork(true);                            
                        }                        
                    } else if (NetworkManager.Instance().IsServerConnection() == true) {
                        if (this.lastSendPlayerIndexFactory < NetworkManager.Instance().GetCurrentPlayerIndexFactory()) {
                            this.lastSendPlayerIndexFactory = NetworkManager.Instance().GetCurrentPlayerIndexFactory();
                            // Send message to all connected clients
                            using (DataStream writer = new DataStream()) {
                                writer.Write(this.lastSendPlayerIndexFactory);
                                NetworkManager.Instance().Send(IntegrationEvent.UpdatePlayerIndexFactory, writer, DeliveryMode.Reliable);
                            }
                        }
                        // Add itself on map to notify other players in case of host migration is enabled
                        if ((this.steamIdMap.Count == 0) || (this.steamIdMap.ContainsKey(SteamUser.GetSteamID().m_SteamID) == false)) {
                            // Find player to get updated data
                            NetworkElement playerElement = NetworkManager.Instance().GetLocalPlayerElement<NetworkElement>();
                            if (playerElement != null) {
                                this.steamIdMap.Add(SteamUser.GetSteamID().m_SteamID, new SteamClientInfo(playerElement.GetNetworkId(),
                                                                                                          playerElement.GetPlayerId(),
                                                                                                          playerElement.GetPlayerIndex(),
                                                                                                          SteamUser.GetSteamID().m_SteamID));
                                this.steamIdMap[SteamUser.GetSteamID().m_SteamID].Alive    = true;
                                this.steamIdMap[SteamUser.GetSteamID().m_SteamID].TimeOut  = (Time.time + this.reconnectionClientToolerance);
                            }
                        }
                        // Despawn timeout players ( On Server Instance )
                        if (this.despawnDisconnectedClientsAfterMigration == true) {
                            if (this.serverIsOnline) {
                                if (this.nextDespawnCheck < Time.time) {
                                    this.nextDespawnCheck = (Time.time + DESPAWN_DISCONNECTION_CHECK_TIME);
                                    foreach (SteamClientInfo steamClientPlayer in this.steamIdMap.Values) {
                                        if (steamClientPlayer.TimeOut < Time.time) {
                                            if (steamClientPlayer.SteamId != SteamUser.GetSteamID().m_SteamID) {
                                                SteamClientInfo forSteamClient = this.steamIdMap[steamClientPlayer.SteamId];
                                                forSteamClient.Alive = false;
                                                this.Log(string.Format("[DestroyOnClient] Sent from [{0}]", steamClientPlayer.SteamId));
                                                NetworkManager.Instance().DestroyOnClient(steamClientPlayer.NetworkId); // Destroy player
                                            } else {
                                                SteamClientInfo forSteamClient = this.steamIdMap[steamClientPlayer.SteamId];
                                                forSteamClient.Alive = true;
                                            }
                                        }
                                    }
                                    foreach (ulong clientSteamId in this.steamIdMap.Keys.ToArray()) {
                                        if (this.steamIdMap[clientSteamId].Alive == false) {
                                            this.steamIdMap.Remove(clientSteamId);
                                        }
                                    }
                                }
                            }
                            // Send all remaning player to all client to tells to all clients that they are Alive
                            if (this.nextAliveNotify < Time.time) {
                                this.nextAliveNotify = (Time.time + ((float)this.reconnectionClientToolerance / ALIVE_INTERVAL_RATE)); // Will send 3 tries during the tooleerance to avoid miss disconnection
                                foreach (SteamClientInfo steamClientPlayer in this.steamIdMap.Values) {
                                    using (DataStream writer = new DataStream()) {
                                        writer.Write(steamClientPlayer.SteamId);
                                        NetworkManager.Instance().Send(IntegrationEvent.NotifySteamUserAlive, writer, DeliveryMode.Reliable);
                                    }
                                    this.Log(string.Format("[NotifySteamUserAlive] Notifyed sent from server [{0}]", steamClientPlayer.SteamId));
                                }
                            }
                        }                        
                    }
                }
            }
        }
#endif
        public SteamEventsEntry GetSteamEvents() {
            return this.steamEvents;
        }

#if STEAMWORKS_NET
        private void ConvertClientIntoServer() {
            // Update the data to tell other players to connect to the new host
            SteamMatchmaking.SetLobbyData(new CSteamID(this.currentLobbyID), "HostMigrated", "true");
            SteamMatchmaking.SetLobbyData(new CSteamID(this.currentLobbyID), "HostAddress", SteamUser.GetSteamID().ToString());

            this.Log(string.Format("New HostAddress [{0}]", SteamUser.GetSteamID().ToString()));

            this.hostInstance       = true; // Flag that this instance is not the host
            // First i need to stop current connection before try to became host
            if (NetworkManager.Instance().IsConnected() == true) {
                NetworkManager.Instance().DisableAutoReconnect();
                NetworkManager.Instance().StopNetwork();                
            }
            // Now i'm going to became server
            NetworkManager.Instance().SetHostMigrated(true); // Flag that his instance became host by a migration
            NetworkManager.Instance().ConfigureMode(NetworkConnectionType.Server);
            NetworkManager.Instance().SetServerAddress(SteamUser.GetSteamID().ToString());
            NetworkManager.Instance().StartNetwork(true); // Start but said the is a reconnection to avoid to recreate the player
            NetworkDebugger.Log("Now you became the host : Server lobby started");

            // Register each player and set platerd ID's
            foreach (NetworkElement networkPlayerElement in NetworkManager.Instance().GetNetworkPlayersElements<NetworkElement>()) {
                // Add a tag to identify this GameObject as a player
                if (networkPlayerElement.IsOwner() == false) {
                    // Get SteamPlayerInformations component
                    SteamPlayerInformations steamInfo = networkPlayerElement.GetGameObject().GetComponent<SteamPlayerInformations>();
                    if (steamInfo != null) {
                        NetworkPlayerTag playerTag = networkPlayerElement.GetGameObject().GetComponent<NetworkPlayerTag>();
                        if (playerTag == null) {
                            playerTag = networkPlayerElement.GetGameObject().AddComponent<NetworkPlayerTag>();
                        }
                        playerTag.SetPlayerIndex(this.steamIdMap[steamInfo.SteamId].PlayerIndex);
                        networkPlayerElement.SetPlayerIndex(this.steamIdMap[steamInfo.SteamId].PlayerIndex);
                        networkPlayerElement.SetPlayerId(this.steamIdMap[steamInfo.SteamId].PlayerId);

                        // Configure remote player identification for this object
                        NetworkPlayerReference playerReference = networkPlayerElement.GetGameObject().GetComponent<NetworkPlayerReference>();
                        if (playerReference == null) {
                            playerReference = networkPlayerElement.GetGameObject().AddComponent<NetworkPlayerReference>();
                        }
                        playerReference.SetReconnection(true); // Flag reconnection to be filled when player connect
                    } else {
                        this.LogError(string.Format("Network id [{0}] not have SteamPlayerInformations component", networkPlayerElement.GetNetworkId()));
                    }
                } else {
                    // Get SteamPlayerInformations component
                    SteamPlayerInformations steamInfo = networkPlayerElement.GetGameObject().GetComponent<SteamPlayerInformations>();
                    if (steamInfo != null) {
                        // Add a tag to identify this GameObject as a player
                        networkPlayerElement.GetGameObject().AddComponent<NetworkPlayerTag>().SetPlayerIndex(steamInfo.PlayerIndex);
                        //
                        IPlayer player = NetworkManager.Instance().GetPlayer<IPlayer>(steamInfo.PlayerId);
                        // If a player is provided, configure the network player reference
                        if (player != null) {
                            if (networkPlayerElement.GetGameObject().GetComponent<NetworkPlayerReference>() == null) {
                                NetworkPlayer networkPlayer = (player as NetworkPlayer);
                                if (networkPlayer.GetClient() != null) {
                                    networkPlayerElement.GetGameObject()
                                                        .AddComponent<NetworkPlayerReference>()
                                                        .Configure(networkPlayer.GetClient(),
                                                                   (networkPlayer.GetClient().GetChannel() as Channel).GetConnectionID(),
                                                                   networkPlayer.GetPlayerId());
                                } else {
                                    networkPlayerElement.GetGameObject()
                                                        .AddComponent<NetworkPlayerReference>()
                                                        .Configure(networkPlayer.GetChannel(),
                                                                   (networkPlayer.GetChannel() as Channel).GetConnectionID(),
                                                                   networkPlayer.GetPlayerId());
                                }
                            }
                        }
                        // Add a component to identify this player as the master player
                        networkPlayerElement.GetGameObject().AddComponent<NetworkMasterPlayer>();
                    }
                }
            }
            // Update connected players time to avoid disconnection errors
            foreach (SteamClientInfo steamClientPlayer in this.steamIdMap.Values) {
                SteamClientInfo forSteamClient  = this.steamIdMap[steamClientPlayer.SteamId];
                // Update timeout into for this client
                forSteamClient.Alive            = true;
                forSteamClient.TimeOut          = (Time.time + (float)this.reconnectionClientToolerance);
            }
            // Flag that server isn't online.. 
            // Note: Server will became online when first player connect to it.. this means that Steam alrrady notified client's
            this.serverIsOnline = false;
            // Execute registered callback
            if (this.steamEvents != null) {
                this.steamEvents.ExecuteOnBecameHost(SteamUser.GetSteamID().m_SteamID);
            }
        }
#endif
        private void OnPlayerIndexUpdated(IDataStream reader) {
            IClient originClient = (reader as INetworkStream).GetClient();
            ushort  playerIndexId = reader.Read<ushort>();
            NetworkManager.Instance().UpdateCurrentPlayerIndexFactory(playerIndexId);
        }

        private void OnRequestSteamServerStatus(IDataStream reader) {
            IClient originClient = (reader as INetworkStream).GetClient();
#if STEAMWORKS_NET
            ulong clientSteamId = reader.Read<ulong>();
            // Answer to client that server is already online
            using (DataStream writer = new DataStream()) {
                writer.Write(SteamUser.GetSteamID().m_SteamID);
                writer.Write(Time.time); // Yeah, server is online
                originClient.Send(IntegrationEvent.SteamServerOnline, writer, DeliveryMode.Reliable);
            }
            // Update client timeout to avoid despawn
            if (this.steamIdMap.ContainsKey(clientSteamId)) {
                SteamClientInfo forSteamClient = this.steamIdMap[clientSteamId];
                forSteamClient.Alive    = true;
                forSteamClient.TimeOut  = (Time.time + (float)this.reconnectionClientToolerance);
            }
            if (this.serverIsOnline == false) {
                this.serverIsOnline = true;
                // Update connected playes time ot to avoid disconnection errors
                // Note: O do this here because is when i'm sure that clients already received that server was updated
                foreach (SteamClientInfo steamClientPlayer in this.steamIdMap.Values) {
                    SteamClientInfo forSteamClient = this.steamIdMap[steamClientPlayer.SteamId];
                    // Update timeout into for this client
                    forSteamClient.Alive    = true;
                    forSteamClient.TimeOut  = (Time.time + (float)this.reconnectionClientToolerance);
                }
            }

            this.Log(string.Format("[OnRequestSteamServerStatus] Steam server is now online on client : [{0}]", clientSteamId));
#endif
        }
        

        private void OnSteamServerOnline(IDataStream reader) {
#if STEAMWORKS_NET
            ulong   serverId    = reader.Read<ulong>();
            float   serverTime  = reader.Read<float>();
            // Flag that this server is online
            this.serverIsOnline     = true;
            this.clientReconnecting = false;
            // Update connected players time to avoid disconnection errors
            foreach (SteamClientInfo steamClientPlayer in this.steamIdMap.Values) {
                SteamClientInfo forSteamClient = this.steamIdMap[steamClientPlayer.SteamId];
                // Update timeout into for this client
                forSteamClient.Alive = true;
                forSteamClient.TimeOut = (Time.time + (float)this.reconnectionClientToolerance);
            }

            this.Log(string.Format("[OnSteamServerOnline] Steam server is now online server Id : [{0}] at [{1}]", serverId, serverTime));
#endif
        }

        private void OnSteamPlayerAlive(IDataStream reader) {
#if STEAMWORKS_NET
            ulong   playerSteamId   = reader.Read<ulong>();
            this.Log(string.Format("[OnSteamPlayerAlive] received from client [{0}] {1}", playerSteamId, this.steamIdMap.ContainsKey(playerSteamId) ? "exists" : "not exists"));
            if ( this.steamIdMap.ContainsKey(playerSteamId) ) {
                SteamClientInfo clientInfo  = this.steamIdMap[playerSteamId];
                // Update stea info of this player
                clientInfo.Alive        = true;
                clientInfo.TimeOut      = (Time.time + (float)this.reconnectionClientToolerance);
                clientInfo.Connection   = (reader as INetworkStream).GetClient();
            }
#endif
        }

        private void OnSteamPlayerNotified(IDataStream reader) {
            int     networkId       = reader.Read<int>();
            ulong   clientSteamId   = reader.Read<ulong>();
            this.Log(string.Format("Player Notified : {0} / {1}", networkId, clientSteamId));
            if (this.steamIdMap.ContainsKey(clientSteamId) == false) {
                // Find player to get updated data
                NetworkElement playerElement = NetworkManager.Instance().GetNetworkPlayerElement<NetworkElement>(networkId);
                this.steamIdMap.Add(clientSteamId, new SteamClientInfo(networkId,
                                                                       playerElement.GetPlayerId(),
                                                                       playerElement.GetPlayerIndex(),
                                                                       clientSteamId));
                this.steamIdMap[clientSteamId].Alive    = true;
                this.steamIdMap[clientSteamId].TimeOut  = (Time.time + this.reconnectionClientToolerance);
            }

            // Not send all existent player to this player
            IClient originClient = (reader as INetworkStream).GetClient();
            foreach (var registeredSteamClient in this.steamIdMap) {
                using (DataStream writer = new DataStream()) {
                    writer.Write(registeredSteamClient.Value.NetworkId);
                    writer.Write(registeredSteamClient.Value.PlayerId);
                    writer.Write(registeredSteamClient.Value.PlayerIndex);
                    writer.Write(registeredSteamClient.Value.SteamId);
                    originClient.Send(IntegrationEvent.UpdatePlayerInfoData, writer, DeliveryMode.Reliable);
                }
            }
            // Now send this player to all connected clients
            using (DataStream writer = new DataStream()) {
                writer.Write(this.steamIdMap[clientSteamId].NetworkId);
                writer.Write(this.steamIdMap[clientSteamId].PlayerId);
                writer.Write(this.steamIdMap[clientSteamId].PlayerIndex);
                writer.Write(this.steamIdMap[clientSteamId].SteamId);
                NetworkManager.Instance().GetConnection(ConnectionType.Server).Send(IntegrationEvent.UpdatePlayerInfoData, writer, DeliveryMode.Reliable);
            }
        }
        
        private void OnPlayerInfoReceived(IDataStream reader) {
            int     networkId       = reader.Read<int>();
            ushort  playerId        = reader.Read<ushort>();
            ushort  playerIndex     = reader.Read<ushort>();
            ulong   clientSteamId   = reader.Read<ulong>();
            if (this.steamIdMap.ContainsKey(clientSteamId) == false) {
                // Find network player
                this.steamIdMap.Add(clientSteamId, new SteamClientInfo(networkId, playerId, playerIndex, clientSteamId));
                this.steamIdMap[clientSteamId].Alive    = true;
                this.steamIdMap[clientSteamId].TimeOut  = (Time.time + this.reconnectionClientToolerance);

                // Register Steam component on this GameObject
                NetworkElement playerElement = NetworkManager.Instance().GetNetworkPlayerElement<NetworkElement>(networkId);
                if (playerElement != null) {
                    SteamPlayerInformations steamInfo = playerElement.GetGameObject().GetComponent<SteamPlayerInformations>();
                    if (steamInfo == null) {
                        steamInfo = playerElement.GetGameObject().AddComponent<SteamPlayerInformations>();
                    }
                    steamInfo.NetworkId     = networkId;
                    steamInfo.PlayerId      = playerId;
                    steamInfo.PlayerIndex   = playerIndex;
                    steamInfo.SteamId       = clientSteamId;
                } else {
                    this.LogError(string.Format("[OnPlayerInfoReceived] Network id not found [{0}]", networkId));
                }
            }
        }
        
        /// <summary>
        /// Create a new lobby instance
        /// </summary>
        /// <param name="lobbyName">The name of the new lobby</param>
        /// <param name="extraTags">Extra parameter added to this lobby (key/value pair)</param>
        /// <note type="tip">
        /// Use extra tags to flag any informations that you wish to be filtered on your game
        /// </note>
        public void CreateLobby(string lobbyName = "", params (string, string)[] extraTags) {
            this.creationLobbyName = lobbyName;
            this.metadata.Clear();
            foreach((string, string) data in extraTags) {
                this.metadata.Add(data.Item1, data.Item2);
            }
#if STEAMWORKS_NET
            SteamMatchmaking.CreateLobby(this.lobbyType, this.maximumOfPlayers);
#endif
        }

#if STEAMWORKS_NET
        public SteamLobby[] GetLobbies() {
            return this.currentLobbies.ToArray<SteamLobby>();
        }
#endif

        public void RequestLobbyList(Action filter = null) {
#if STEAMWORKS_NET
            this.filterProcedure = filter;
            if (this.filterProcedure != null) {
                this.filterProcedure.Invoke();
            } else {
                SteamMatchmaking.AddRequestLobbyListDistanceFilter(this.lobbyDistance);
            }
            SteamMatchmaking.RequestLobbyList();
#endif
        }

#if STEAMWORKS_NET
        private void OnLobbyCreated(LobbyCreated_t callback) {
            if (callback.m_eResult != EResult.k_EResultOK)
                return;
            currentLobbyID = callback.m_ulSteamIDLobby;

            SteamLobby lobbyData = new CSteamID(currentLobbyID);
            lobbyData.IsSession = true;
            lobbyData[SteamLobby.DataName] = this.creationLobbyName;
            SteamMatchmaking.SetLobbyData(lobbyData, "HostAddress",  SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(lobbyData, "LobbyName",    this.creationLobbyName);
            SteamMatchmaking.SetLobbyData(lobbyData, "HostMigrated", "false");
            SteamMatchmaking.SetLobbyData(lobbyData, "HostIsPlayer", "true");

            this.Log(string.Format("Server started with HostAddress [{0}]", SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyID), "HostAddress")));

            // Add custom metadata
            foreach (var data in this.metadata) {
                SteamMatchmaking.SetLobbyData(lobbyData, data.Key, data.Value);
            }
            this.currentLobby = lobbyData;
            this.hostInstance = true; // Flag that this instance is not the host
            NetworkManager.Instance().ConfigureMode(NetworkConnectionType.Server);
            NetworkManager.Instance().SetServerAddress(SteamUser.GetSteamID().ToString());
            NetworkDebugger.Log("Server Created Lobby: {0}", currentLobbyID);
        }
#endif

#if STEAMWORKS_NET
        private void OnJoinRequest(GameLobbyJoinRequested_t callback) {
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }
#endif

#if STEAMWORKS_NET
        private void OnLobbyEntered(LobbyEnter_t callback) {
            if (callback.m_EChatRoomEnterResponse != (int)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess) {
                this.Log("Invalid Lobby");
                return;
            }
            this.currentLobbyID      = callback.m_ulSteamIDLobby;
            this.currentLobbyOwner   = SteamMatchmaking.GetLobbyOwner(new CSteamID(currentLobbyID));
            if (NetworkManager.Instance().IsServerConnection()) {
                this.hostInstance = true; // Flag that this instance is not the host
                NetworkManager.Instance().StartNetwork();
                NetworkDebugger.Log("Server lobby started");
            } else {
                NetworkDebugger.Log("Client joined on lobby");
                try {
                    if (this.onPlayerJoinedOnLobby != null) {
                        this.onPlayerJoinedOnLobby.Invoke(true);
                    }
                } finally {
                    this.hostInstance = false; // Flag that this instance isn't the host
                    // First i need to go thought all members tio check has any host migration
                    String  lobbyAddressToJoin  = SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyID), "HostAddress");
                    this.Log(string.Format("Trying to connect to HostAddress [{0}]", lobbyAddressToJoin));
                    NetworkManager.Instance().ConfigureMode(NetworkConnectionType.Client);
                    NetworkManager.Instance().SetServerAddress(lobbyAddressToJoin);
                    NetworkManager.Instance().StartNetwork();
                }                
            }

        }
#endif

#if STEAMWORKS_NET
        private void OnLobbyRefreshResult(LobbyMatchList_t lobbyListResult) {
            this.currentLobbies.Clear();
            for (int i = 0; i < lobbyListResult.m_nLobbiesMatching; i++) {
                this.currentLobbies.Add( SteamMatchmaking.GetLobbyByIndex(i) );      
                if (this.currentLobby.SteamId == this.currentLobbies[this.currentLobbies.Count - 1].SteamId) {
                    NetworkDebugger.Log("Lobby on list: " + this.currentLobby.SteamId);
                }
            }
        }
#endif

#if STEAMWORKS_NET
        private void OnLobbyDataUpdated(LobbyDataUpdate_t lobbyData) {
            // Lobby was changed ?
            if (this.currentLobbyOwner.m_SteamID != SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyData.m_ulSteamIDLobby)).m_SteamID) {
                // Execute registered callback
                if (NetworkManager.Instance().IsClientConnection()) {
                    if (this.steamEvents != null) {
                        this.steamEvents.ExecuteOnHostLeaveLobby(SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyData.m_ulSteamIDLobby)).m_SteamID);
                    }
                }
                // I'm the owner ? I will became server
                if (SteamUser.GetSteamID().m_SteamID == SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyData.m_ulSteamIDLobby)).m_SteamID) {
                    this.ConvertClientIntoServer();
                } else  {
                    this.clientReconnecting = true; // Flag that this client will try to reconnect
                    // Check if need to destroy previous host player
                    if ( this.despawnDisconnectedServerPlayer == true ) {
                        // Get previous host player
                        if (this.steamIdMap.ContainsKey(this.currentLobbyOwner.m_SteamID)) {
                            this.Log(string.Format("[DestroyOnClient] Sent from lobby update [{0}]", this.currentLobbyOwner.m_SteamID));
                            NetworkManager.Instance().DestroyOnClient(this.steamIdMap[this.currentLobbyOwner.m_SteamID].NetworkId);
                            this.steamIdMap.Remove(this.currentLobbyOwner.m_SteamID);
                        }
                    }
                    // Need, i'm still client ? im going to reconnect to the new server
                    String lobbyAddressToJoin = SteamMatchmaking.GetLobbyData(new CSteamID(lobbyData.m_ulSteamIDLobby), "HostAddress");
                    this.Log(string.Format("Trying to connect to HostAddress [{0}]", lobbyAddressToJoin));

                    this.connectedAtServer      = false;
                    this.nextConnectionRetry    = (Time.time + CLIENT_RECONNECTION_RETRY);
                    // Flag that this instance if now the host
                    this.hostInstance           = false;
                    NetworkManager.Instance().DisableAutoReconnect();
                    NetworkManager.Instance().StopNetwork();
                    NetworkManager.Instance().ConfigureMode(NetworkConnectionType.Client);
                    NetworkManager.Instance().SetServerAddress(lobbyAddressToJoin);
                    NetworkManager.Instance().StartNetwork(true);
                }
                // Update lobby owner
                this.currentLobbyOwner  = SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyData.m_ulSteamIDLobby));
                this.currentLobbyID     = lobbyData.m_ulSteamIDLobby;
                // Execute registered callback
                if (NetworkManager.Instance().IsClientConnection()) {
                    if (this.steamEvents != null) {
                        this.steamEvents.ExecuteOnDetectNewHost(SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyData.m_ulSteamIDLobby)).m_SteamID);
                    }
                }
            }
        }
#endif

#if STEAMWORKS_NET
        public void RequestToJoin(CSteamID steamID, Action<bool> onLobbyJoined = null) {
            NetworkDebugger.Log("Attempting to join lobby with ID: {0}", steamID.m_SteamID.ToString());
            this.onPlayerJoinedOnLobby = onLobbyJoined;
            if (SteamMatchmaking.RequestLobbyData(steamID))
                SteamMatchmaking.JoinLobby(steamID);
            else
                NetworkDebugger.Log("Failed to join lobby with ID: {0}", steamID.m_SteamID.ToString());
        }
#endif

#if STEAMWORKS_NET
        public void LeaveLobby() {
            SteamMatchmaking.LeaveLobby(new CSteamID(currentLobbyID));
            currentLobbyID = 0;
            // Close network connection
            NetworkManager.Instance().DisableAutoReconnect();
            NetworkManager.Instance().ConfigureMode(NetworkConnectionType.Manual);
            NetworkManager.Instance().StopNetwork();
        }
#endif

#if STEAMWORKS_NET
        /// <summary>
        /// Transfer host ownership to another user on the same lobby
        /// </summary>
        /// <param name="steamUserID">Target user to be the new host</param>
        public void TransferHost(ulong steamUserID) {
            if (this.IsHostInstance()) {
                if (this.steamIdMap.ContainsKey(steamUserID)) {
                    if (SteamMatchmaking.SetLobbyOwner(new CSteamID(this.currentLobbyID),
                                                       new CSteamID(steamUserID)) == true) {
                        this.Log(string.Format("The current lobby was transferred to user [{0}].", steamUserID));
                    }
                } else {
                    this.LogError(string.Format("The user id [{0}] isn't in the current lobby, you can only tranfer lobby ownership to people who is in the current lobby.", steamUserID));
                }
            } else {
                this.LogError("You are not the host, only host player can tranfer host to another players");
            }
        }
#endif

#if STEAMWORKS_NET
        private void ForceToEnableLogin() {
            this.SetLoginValue("useNetworkLogin",               true);
            this.SetLoginValue("loginInfoProviderObject",       this.gameObject);
            this.SetLoginValue("loginInfoComponent",            this);
            this.SetLoginValue("loginInfoMethod",               "GetLoginInformations");
            this.SetLoginValue("loginTypesMethod",              "GetLoginInformationsTypes");

            this.SetLoginValue("enableLoginValidation",         true);
            this.SetLoginValue("loginValidationProviderObject", this.gameObject);
            this.SetLoginValue("loginValidationComponent",      this);
            this.SetLoginValue("loginValidationMethod",         "IsLoginValid");

            this.SetLoginValue("useCustomNetworkId",            true);
            this.SetLoginValue("useInternalNetworkId",          false);
            this.SetLoginValue("networkIdProviderObject",       this.gameObject);
            this.SetLoginValue("networkIdComponent",            this);
            this.SetLoginValue("networkIdMethod",               "GetNetworkId");
        }

        public void SetLoginValue(string property, object value) {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo propertyInfo = NetworkManager.Instance().GetType().GetField(property, bindFlags);
            propertyInfo.SetValue(NetworkManager.Instance(), value);
        }

        public object[] GetLoginInformations() {
            return new object[] {
                                 SteamUser.GetSteamID().m_SteamID,
                                 SteamUser.GetHSteamUser().m_HSteamUser
            };
            
        }

        public Type[] GetLoginInformationsTypes() {
            return new Type[] {
                                typeof(ulong),
                                typeof(int)
                               };

        }

        public bool IsLoginValid(object[] arguments) {
            return true;
        }

        public int GetNetworkId(params object[] arguments) {
            String stringId = arguments[0].ToString() + arguments[1].ToString();
            String stringIdReversed = this.ReverseString(stringId);
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(stringId + stringIdReversed));
            return BitConverter.ToInt32(hashed, 0);
        }

        public string ReverseString(string s) {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
#endif
        private void Log(string message) {
            if (this.debugLogger) {
                Debug.Log(message);
            }
        }

        private void LogError(string message) {
            Debug.LogError(message);
        }
    }
}
#pragma warning restore