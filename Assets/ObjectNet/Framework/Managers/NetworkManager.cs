#pragma warning disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Manages network operations, connections, and settings.
    /// </summary>
    public class NetworkManager : NetworkThreadDispatcher {

        /// <summary>
        /// Container where all network objects will be stores
        /// </summary>
        public static INetworkContainer Container { get { return NetworkManager.GetContainer(); } }

        /// <summary>
        /// Custom network events created by user
        /// </summary>
        public static INetworkEventsCore Events { get { return NetworkManager.GetEvents(); } }

        /// <summary>
        /// Lobby manager to allow and manage all lobby features
        /// </summary>
        public static NetworkLobbyManager Lobbies { get { return NetworkManager.GetLobbies(); } }

        /// <summary>
        /// Gets or sets the local tick count.
        /// </summary>
        public int LocalTicks {
            get => this.currentLocalTicks;
            set => this.currentLocalTicks = value;
        }

        /// <summary>
        /// Gets or sets the remote tick count and updates interpolation ticks accordingly.
        /// </summary>
        public int RemoteTicks {
            get => this.currentRemoteTicks;
            set {
                this.currentRemoteTicks     = value;
                this.interpolationTicks     = (value - this.TickBetweenUpdates);
            }
        }

        /// <summary>
        /// Gets or sets if client already received remote tick update
        /// </summary>
        public bool IsRemoteTicksInitialized {
            get => this.remoteTicksInitialized;
            set {
                this.remoteTicksInitialized = value;
            }
        }
        

        /// <summary>
        /// Gets or sets the number of ticks between updates and updates interpolation ticks accordingly.
        /// </summary>
        private int TickBetweenUpdates {
            get => this.tickBetweenUpdates;
            set {
                this.tickBetweenUpdates = value;
                this.interpolationTicks = (this.RemoteTicks - value);
            }
        }

        /// <summary>
        /// Gets or sets the number of ticks used for interpolation.
        /// </summary>
        public int InterpolationTicks {
            get => this.interpolationTicks;
            set => this.interpolationTicks = value;
        }

        /// <summary>
        /// Gets send update frequency
        /// </summary>
        public int SendRate {
            get => this.sendRateAmount;
        }

        /// <summary>
        /// Gets miimum send update frequency
        /// </summary>
        public int MinimumSendRate {
            get => this.minimumRateAmount;
        }

        // Flag is this object shall keep persistent between scenes
        [SerializeField]
        private bool dontDestroyOnLoad = true;

        // The default target database for the network
        [SerializeField]
        private string databaseTarget = GlobalResources.DEFAULT_DATABASE;

        // The working mode of the server (e.g., Embedded, Standalone)
        [SerializeField]
        private NetworkServerMode serverWorkingMode = NetworkServerMode.Embedded;

        // Flag to determine if Peer-to-Peer networking should be used
        [SerializeField]
        private bool usePeerToPeer = false;

        // Flag to determine if NAT traversal should be used for network connections
        [SerializeField]
        private bool useNatTraversal = false;

        // Flag to determine if a lobby manager should be used for organizing network games
        [SerializeField]
        private bool useLobbyManager = false;

        // The network transport layer used by the server
        [SerializeField]
        private NetworkTransport serverSocket;

        // The network transport layer used by the client
        [SerializeField]
        private NetworkTransport clientSocket;

        // The type of network connection to establish (e.g., Manual, Automatic)
        [SerializeField]
        private NetworkConnectionType connectionType = NetworkConnectionType.Manual;

        // Flag to determine if the network should automatically connect on start
        [SerializeField]
        private bool autoConnectOnStart = false;

        // The delay before initiating a connection (in seconds)
        [SerializeField]
        private int connectionDelay = 0;

        // Flag to determine if the connection should be established asynchronously
        [SerializeField]
        private bool asyncConnect = false;

        // Flag to determine if the network should automatically attempt to reconnect after a disconnect
        [SerializeField]
        private bool autoReconnect = false;

        // The delay before attempting to reconnect after a disconnect (in seconds)
        [SerializeField]
        private int reconnectionDelay = 5;

        // How many times client will try to reconnect on server
        [SerializeField]
        private int reconnectionAttemps = 5;

        // The timeout for detecting idle connections (in seconds)
        [SerializeField]
        private int idleTimeutDetection = 10;

        // The delay betwqeen client connection on server and notify clien (in milliseconds)
        [SerializeField]
        private int clientConnectDelay = 100;

        // Flag to determine if server restarts should be detected
        [SerializeField]
        private bool detectServerRestart = true;

        // Flag to determine if network login should be used
        [SerializeField]
        private bool useNetworkLogin = false;

        // The GameObject that provides login information
        [SerializeField]
        private GameObject loginInfoProviderObject;

        // The component that contains the login information method
        [SerializeField]
        private MonoBehaviour loginInfoComponent;

        // The method to call for obtaining login information
        [SerializeField]
        private String loginInfoMethod;

        // The method to call for obtaining login types
        [SerializeField]
        private String loginTypesMethod;

        // Flag to determine if login validation should be enabled
        [SerializeField]
        private bool enableLoginValidation = false;

        // The GameObject that provides login validation
        [SerializeField]
        private GameObject loginValidationProviderObject;

        // The component that contains the login validation method
        [SerializeField]
        private MonoBehaviour loginValidationComponent;

        // The method to call for validating login information
        [SerializeField]
        private String loginValidationMethod;

        // Flag to determine if the network clock should be used for synchronization
        [SerializeField]
        private bool useNetworkClock = true;

        // Flag to determine if movement prediction should be used to reduce lag
        [SerializeField]
        private bool useMovementPrediction = true;

        // Flag to determine if movement interpolation should be used to reduce lag
        [SerializeField]
        private bool useInterpolation = true;

        // How player movement will be made (e.g., Automatic, None)
        [SerializeField]
        private PredictionType movementType = PredictionType.Automatic;

        // The type of prediction to use for movement (e.g., Automatic, None)
        [SerializeField]
        private PredictionType predictionType = PredictionType.Automatic;

        // The size of the buffer for storing predicted movements
        [SerializeField]
        private int predictionBufferSize = 0;

        // The curve that defines how prediction is factored over time
        [SerializeField]
        private AnimationCurve predictionFactor = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        // The curve that defines how prediction speed is factored over time
        [SerializeField]
        private AnimationCurve predictionSpeedFactor = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        // Flag to determine if prediction should be overridden by custom logic
        [SerializeField]
        private bool overridePrediction = false;

        // The GameObject that provides custom prediction logic
        [SerializeField]
        private GameObject predictionProviderObject;

        // The component that contains the custom prediction method
        [SerializeField]
        private MonoBehaviour predictionComponent;

        // Flag to determine if remote input should be used for controlling characters
        [SerializeField]
        private bool useRemoteInput = true;

        // Flag to determine if input should be echoed back to the sender for verification
        [SerializeField]
        private bool useEchoInput = false;

        // The GameObject that provides input
        [SerializeField]
        private GameObject inputProviderObject;

        // The component that contains the input method
        [SerializeField]
        private MonoBehaviour inputComponent;

        // The list of managed inputs for the network
        [SerializeField]
        private List<ManagedInput> inputList = new List<ManagedInput>();

        // Flag to determine if internal ping measurements should be used
        [SerializeField]
        private bool useInternalPing = true;

        // Flag to determine if the network should be tolerant to latency variations
        [SerializeField]
        private bool latencyTolerance = true;

        // The value below which latency is considered good (in milliseconds)
        [SerializeField]
        private int goodLatencyValue = 80;

        // The value below which latency is considered acceptable (in milliseconds)
        [SerializeField]
        private int acceptableLatencyValue = 140;

        // Flag to determine if a single server should be used for the network
        [SerializeField]
        private bool useSingleServers = false;

        // The address of the server to connect to
        [SerializeField]
        private String serverAddress = "127.0.0.1";

        // The address used of the server when Bind
        [SerializeField]
        private String hostServerAddress = "127.0.0.1";

        // Accep any address as host when execute binding
        [SerializeField]
        private bool useAnyAddress = true;

        // Use IP Address defined by user
        [SerializeField]
        private bool useFixedAddress = false;

        // Use public IP Address when start listening
        [SerializeField]
        private bool usePublicAddress = false;

        // Use internal network IP Address when start listening
        [SerializeField]
        private bool useInternalAddress = false;

        // The port to use for TCP connections
        [SerializeField]
        private int tcpConnectionPort = DEFAULT_TCP_PORT;

        // The port to use for UDP connections
        [SerializeField]
        private int udpConnectionPort = DEFAULT_UDP_PORT;

        // The port to use for peer to peer connections
        [SerializeField]
        private int peerToPeerPort = DEFAULT_P2P_PORT;

        // System will use one port for each peer to peer client player
        [SerializeField]
        private bool peerToPeerPortPeerPlayer = true;

        // Flag to determine if multiple servers should be used for the network
        [SerializeField]
        private bool useMultipleServers = false;

        // The list of network servers available for connection
        [SerializeField]
        private List<String> networkServers = new List<String>();

        // The list indicating which network servers are enabled
        [SerializeField]
        private List<bool> networkServersEnabled = new List<bool>();

        // Flag to determine if dynamic server addresses should be used
        [SerializeField]
        private bool useDynamicAddress = false;

        // The GameObject that provides server address requests
        [SerializeField]
        private GameObject requestServersObject;

        // The component that contains the server address request method
        [SerializeField]
        private MonoBehaviour requestServerComponent;

        // The method to call for requesting server addresses
        [SerializeField]
        private String requestServerMethod;

        // The rate at which updates are sent over the network (in updates per second)
        [SerializeField]
        private int sendRateAmount = 30;

        // The minimum rate at which updates are sent over the network (in updates per second)
        // Note: This value will be used when no chnages are detected on the element
        [SerializeField]
        private int minimumRateAmount = 4;

        // The delivery mode for network updates (e.g., Reliable, Unreliable)
        [SerializeField]
        private DeliveryMode sendUpdateMode = DeliveryMode.Unreliable;

        // Flag to determine if the player spawner should be enabled
        [SerializeField]
        private bool enablePlayerSpawner = true;

        // Flag to determine if the network should use an internal network ID
        [SerializeField]
        private bool useInternalNetworkId = true;

        // Flag to determine if a custom network ID should be used
        [SerializeField]
        private bool useCustomNetworkId = false;

        // The GameObject that provides the network ID
        [SerializeField]
        private GameObject networkIdProviderObject;

        // The component that contains the network ID method
        [SerializeField]
        private MonoBehaviour networkIdComponent;

        // The method to call for obtaining the network ID
        [SerializeField]
        private String networkIdMethod;

        // Flag to determine if player cameras should be controlled by the network
        [SerializeField]
        private bool controlPlayerCameras = true;

        // Flag to determine if the player camera should be detached from the player
        [SerializeField]
        private bool detachPlayerCamera = false;

        // Flag to determine if the player should be despawned upon disconnect
        [SerializeField]
        private bool despawnPlayerOnDisconnect = true;

        // Flag to determine if the player should be despawned after a delay upon disconnect
        [SerializeField]
        private bool despawnPlayerAfterDelay = false;

        // The delay before removing a disconnected player (in seconds)
        [SerializeField]
        private int delayToRemoveDisconnected = 60;

        // The mode for spawning network players (e.g., SingleElement, Random)
        [SerializeField]
        private NetworkPlayerSpawnMode playerSpawnMode = NetworkPlayerSpawnMode.SingleElement;

        // The mode for spawning players after connected
        [SerializeField]
        private NetworkPlayerSpawnTime  playerSpawnTimeMode = NetworkPlayerSpawnTime.Automatic;

        // The prefab to use for spawning network players
        [SerializeField]
        private GameObject playerPrefabToSpawn = null;

        // The GameObject that provides player spawning logic
        [SerializeField]
        private GameObject playerSpawnerProviderObject;

        // The component that contains the player spawning method
        [SerializeField]
        private MonoBehaviour playerSpawnerComponent;

        // The method to call for spawning players
        [SerializeField]
        private String playerSpawnerMethod;

        // The mode for determining spawn positions (e.g., Fixed, Random)
        [SerializeField]
        private NetworkSpawnPositionMode spawnPositionMode = NetworkSpawnPositionMode.Fixed;

        // The fixed position to use for spawning players
        [SerializeField]
        private Transform fixedPositionToSpawn;

        // The list of multiple positions available for spawning players
        [SerializeField]
        private List<Transform> multiplePositionsToSpawn = new List<Transform>();

        // The GameObject that provides spawn position logic
        [SerializeField]
        private GameObject positionSpawnerProviderObject;

        // The component that contains the spawn position method
        [SerializeField]
        private MonoBehaviour positionSpawnerComponent;

        // The method to call for determining spawn positions
        [SerializeField]
        private String positionSpawnerMethod;

        // Determine if encryption is enabled or not
        [SerializeField]
        private bool encryptionEnabled = false;

        // The GameObject that provides encryption logic
        [SerializeField]
        private GameObject encryptionProviderObject;

        // The component that contains the encryption method
        [SerializeField]
        private MonoBehaviour encryptionComponent;

        // The method to call for when encryption was executed
        [SerializeField]
        private String encryptionMethod;

        // The method to call for when decription was executed
        [SerializeField]
        private String decryptionMethod;

        // The mode used to control ownership of objects
        [SerializeField]
        private OwnerShipServerMode ownerShipMode = OwnerShipServerMode.PrefabDefinition;

        // The maximun of connected clients at same time
        [SerializeField]
        private bool enableCCULimit = false;

        // The maximun of connected clients at same time
        [SerializeField]
        private uint maximunConnectedCount = uint.MaxValue;

        // Allow player to request scene lo be loaded
        [SerializeField]
        private bool allowPlayersRequestSceneLoad = false;

        // Pause the disconnect detection durign scene loading
        [SerializeField]
        private bool pauseDisconnectionOnSceneLoad = true;

        ////////////////////////////////////////////////////////////////////////////////////////
        /// 
        /// Network Prefabs Database
        /// 
        ////////////////////////////////////////////////////////////////////////////////////////
        NetworkPrefabsDatabase prefabsDatabase;

        ////////////////////////////////////////////////////////////////////////////////////////
        /// 
        /// Network Transport Database
        /// 
        ////////////////////////////////////////////////////////////////////////////////////////
        NetworkTransportsDatabase transportsDatabase;

        ////////////////////////////////////////////////////////////////////////////////////////
        ///
        /// Exposed events
        /// 
        /// Note: Those events are excposed to outside and can be overriden by users
        /// 
        ////////////////////////////////////////////////////////////////////////////////////////
        private Action<IClient> onClientConnected = null;

        private Action<IClient> onClientDisconnected = null;

        private Action<IClient> onDisconnected = null;

        ////////////////////////////////////////////////////////////////////////////////////////
        // Players manager ( Used on relay mode )
        ////////////////////////////////////////////////////////////////////////////////////////
        GameObject currentSpawnedPlayer = null;

        ////////////////////////////////////////////////////////////////////////////////////////
        // Players manager ( Used on relay mode )
        ////////////////////////////////////////////////////////////////////////////////////////
        private bool isConnectedOnRelayServer = false;

        private bool isConnectedOnLobbyServer = false;

        private ushort currentPlayerId = 0;

        private ushort playerIndexFactory = 0;

        private Dictionary<ushort, IPlayer> players = new Dictionary<ushort, IPlayer>();

        ////////////////////////////////////////////////////////////////////////////////////////
        // Lobby manager attributes
        ////////////////////////////////////////////////////////////////////////////////////////
        
        private NetworkLobbyManager lobbyManager;

        ////////////////////////////////////////////////////////////////////////////////////////
        // End
        ////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////
        // Host migration
        ////////////////////////////////////////////////////////////////////////////////////////
        private bool hostMigratedToHere = false;

        private INetworkContainer objectsContainer;

        private INetworkEventsCore eventsManager;

        private INatHelper natHelper;

        private INatHelper natHelperPeerToPeer;

        private String networkSessionId = "";

        private String adapterAddress = null;

        private bool disableReconnect = false;

        private List<NetworkServerData> servers = new List<NetworkServerData>();

        private bool remoteTicksInitialized     = false;

        private int currentLocalTicks           = 0;

        private int currentRemoteTicks          = 0;

        private int interpolationTicks          = 0;

        private int tickBetweenUpdates          = 2;

        private int tickTolerance               = 1;

        private int currentAttemps              = 0;

        private bool inSceneCollected           = false;

        private bool allowToSceneCollect        = false;

        private bool inSceneUpdatedRequested    = false;

        private bool remoteSceneLoading         = false;

        private bool spawnSceneElements         = true;

        private int manualExecutionTick         = 0;

        private int reliableExecutionTick       = 0;

        private Dictionary<IClient, int> clientsToUpdateVariables = new Dictionary<IClient, int>(); // List of new clients connected to a game

        private List<IClient> recicledClients = new List<IClient>();

        /// <summary>
        /// FPS Measure and Contol
        /// </summary>
        byte        currentFramePeerSecond  = 1;
        
        int         frameRateAveareageRange = 120;

        List<int>   collectedFrameRates     = new List<int>(); // The idea is collect some framerates and extract aveareage to avoid flicker

        /// <summary>
        /// Avaiable sockets
        /// </summary>
        private Dictionary<ConnectionType, NetworkTransport> connections = new Dictionary<ConnectionType, NetworkTransport>();

        /// <summary>
        /// Current list of all connected players
        /// </summary>
        private Dictionary<IClient, INetworkElement> clients = new Dictionary<IClient, INetworkElement>();

        private List<GameObject> detectedNetworkObjects = new List<GameObject>();

        private List<GameObject> detectedInSceneObjects = new List<GameObject>();

        private List<GameObject> currentInSceneObjects = new List<GameObject>();

        private Dictionary<string, List<GameObject>> garbageNetworkObjects = new Dictionary<string, List<GameObject>>();

        private List<DelayedRemove> delayedDespawnedObjects = new List<DelayedRemove>();

        private List<int> destroyedNetworkObjects = new List<int>();

        private Dictionary<int, Action> respawnedNetworkObjects = new Dictionary<int, Action>();

        private List<int> possibleSpawnedPositions = new List<int>();

        private List<DelayedRemove> objectsToDespawn = new List<DelayedRemove>();

        private List<KeyValuePair<IClient, int>> variablesToUpdateResultMap = new List<KeyValuePair<IClient, int>>();

        /// <summary>
        /// List all clients waiting for scne to be loaded and the tiem when this was requested
        /// </summary>
        private Dictionary<IClient, float> clientsLoadingScene = new Dictionary<IClient, float>();

        private RemoteSceneLoadMode remoteSceneLoadingMode = RemoteSceneLoadMode.LoadFirst;

        private RemoteSceneUnloadMode remoteSceneUnloadMode = RemoteSceneUnloadMode.UnloadFirst;

        private Action delayedServerLoadUnloadScene;

        private AsyncOperation asyncLoadingScene;

        private static NetworkManager instance;

        public const int INSTANTIATE_SPAWNER_DELAY = 100;

        public const int INITIALIZE_SPAWNER_DELAY = 150;

        public const int DEFAULT_TCP_PORT = 4550;

        public const int DEFAULT_UDP_PORT = 4550;

        public const int DEFAULT_P2P_PORT = 4560;

        const byte MAX_ALLOWED_FPS = 60;

        const float MILLISECONDS_MULTIPLIER = 1000.0f;

        const double WAIT_PING_TIME = 1.0;

        const int MIN_SERVERS_TO_PING = 1;

        const int DELAY_TO_REMOVE_RELIABLE_UPDATE = ( 30 * 10 ); // Will be remvoed after 30 seconds ( if game is running in 30 fps ) and 5 seconds ( if is running in 50 fps )

        public const string MAIN_CAMERA_TAG = "MainCamera";

        /// <summary>
        /// Provides access to the singleton instance of the NetworkManager.
        /// </summary>
        /// <returns>The singleton instance of the NetworkManager.</returns>
        public static NetworkManager Instance() {
            // Check if an instance already exists
#if DEBUG
            if (!InstanceExists()) {
                // Warn the user if the application is running and no instance is found
                if (Application.isPlaying) {
                    if (NetworkDebuggerManager.Instance()) {
                        NetworkDebugger.LogWarning("[ NetworkManager ] Could not find the instance of object. Please ensure you have added the NetworkManager Prefab to your scene.");
                    }
                }
            }
#endif
            // Return the singleton instance
            return NetworkManager.instance;
        }

        /// <summary>
        /// Detect ig another NetworkManager is already instantiated
        /// </summary>
        /// <returns>True if NetworkManager already exists, otherwise false</returns>
        public bool DetectDuplicated() {
            return (NetworkManager.instance != null);
        }

        /// <summary>
        /// Flag current instance of NetworkManager as the in use instance
        /// </summary>
        private void SetInstance() {
            NetworkManager.instance = this;
        }

        /// <summary>
        /// Checks if an instance of NetworkManager already exists.
        /// </summary>
        /// <returns>True if an instance exists, false otherwise.</returns>
        private static bool InstanceExists() {
            // Check if the instance is not null
            return (NetworkManager.instance != null);
        }

        /// <summary>
        /// Retrieves the network container associated with the NetworkManager instance.
        /// </summary>
        /// <returns>The INetworkContainer associated with the NetworkManager.</returns>
        private static INetworkContainer GetContainer() {
            // Return the objects container from the instance
            return NetworkManager.instance.objectsContainer;
        }

        /// <summary>
        /// Retrieves the network events core associated with the NetworkManager instance.
        /// </summary>
        /// <returns>The INetworkEventsCore associated with the NetworkManager.</returns>
        private static INetworkEventsCore GetEvents() {
            // Return the events manager from the instance
            return NetworkManager.instance.eventsManager;
        }

        /// <summary>
        /// Retrieves the network lobby manager associated with the NetworkManager instance.
        /// </summary>
        /// <returns>The NetworkLobbyManager associated with the NetworkManager.</returns>
        private static NetworkLobbyManager GetLobbies() {
            // Return the lobby manager from the instance
            return NetworkManager.instance.lobbyManager;
        }

        /// <summary>
        /// Registers a callback for a specific network event.
        /// </summary>
        /// <param name="eventCode">The event code to register the callback for.</param>
        /// <param name="callBack">The callback action to be invoked when the event occurs.</param>
        /// <param name="replace">Replace a previous existent event if event is already registeredexists.</param>
        /// <param name="broadcast">This event shall be broadcast to all players.</param>
        public static void RegisterEvent(int eventCode, Action<IDataStream> callBack, bool replace = false, bool broadcast = false) {
            // Register the event with the events manager
            NetworkManager.GetEvents().RegisterEvent(eventCode, callBack, replace, broadcast);
        }

        /// <summary>
        /// Registers a broadcast event with a specific event code.
        /// </summary>
        /// <param name="eventCode">The event code to register the broadcast event for.</param>
        public static void RegisterBroadcastEvent(int eventCode) {
            // Register the broadcast event with the events manager
            NetworkManager.GetEvents().RegisterBroadcastEvent(eventCode);
        }

        /// <summary>
        /// Unregisters a previously registered broadcast event.
        /// </summary>
        /// <param name="eventCode">The event code to unregister the broadcast event for.</param>
        public static void UnregisterBroadcastEvent(int eventCode) {
            // Unregister the broadcast event with the events manager
            NetworkManager.GetEvents().UnregisterBroadcastEvent(eventCode);
        }

        /// <summary>
        /// Checks if an event code corresponds to a broadcast event.
        /// </summary>
        /// <param name="eventCode">The event code to check.</param>
        /// <returns>True if the event code is for a broadcast event, false otherwise.</returns>
        public static bool IsBroadcastEvent(int eventCode) {
            // Check if the event is a broadcast event with the events manager
            return NetworkManager.GetEvents().IsBroadcastEvent(eventCode);
        }

        /// <summary>
        /// Retrieves the unique network session identifier.
        /// </summary>
        /// <returns>The network session identifier as a string.</returns>
        public static string GetInstanceId() {
            // Return the network session ID from the instance
            return NetworkManager.instance.networkSessionId;
        }

        /// <summary>
        /// Validates if the provided instance ID matches the current network session ID.
        /// </summary>
        /// <param name="instanceId">The instance ID to validate.</param>
        /// <returns>True if the instance ID matches, false otherwise.</returns>
        public static bool InstanceIsValid(String instanceId) {
            // Compare the provided instance ID with the current network session ID
            return (NetworkManager.instance.networkSessionId == instanceId);
        }

        /// <summary>
        /// Checks if the current network session has a valid instance ID.
        /// </summary>
        /// <returns>True if the instance ID is not null or empty, false otherwise.</returns>
        public static bool HasValidInstance() {
            // Check if the network session ID is neither null nor empty
            return !String.IsNullOrEmpty(NetworkManager.instance.networkSessionId);
        }

        /// <summary>
        /// Initializes the NetworkManager instance with necessary resources and configurations.
        /// </summary>
        public void Initialize() {
            // This option make communication run in background when unity is in editor mode,
            // otherwise messages are not arriving ( thank you Unity :S )
            Application.runInBackground = true;
            // Set the current instance to this object
            NetworkManager.instance = this;
            // Load resources and assign them to the instance properties
            this.prefabsDatabase = Resources.Load<NetworkPrefabsDatabase>(GlobalResources.GetPrefabsDatabase(this.databaseTarget));
            this.transportsDatabase = Resources.Load<NetworkTransportsDatabase>(GlobalResources.TRANSPORTS_DATABASE_PATH);
            // Generate a new unique session ID
            this.networkSessionId = Guid.NewGuid().ToString();
            // Initialize the container, events manager, and lobby manager
            this.objectsContainer   = new NetworkContainer();
            this.eventsManager      = new NetworkEventsCore();
            this.lobbyManager       = new NetworkLobbyManager();
            // Set the network clock based on the useNetworkClock flag
            this.NetworkClock = (this.useNetworkClock) ? new NetworkClock() : new UnityClock();
            // Configure lobby close event
            this.lobbyManager.SetOnLobbyClosed((ILobby closedLobby) => {
                if (this.InRelayMode()) {
                    // Inform to all clients regard to this lobby finish
                    using (DataStream writer = new DataStream()) {
                        writer.Write(closedLobby.GetLobbyId());
                        this.GetConnection(ConnectionType.Server).Send(LobbyServerEvents.LobbyFinish, writer, DeliveryMode.Reliable); // Send message                    
                    }
                }
            });
            // Set the number of remote ticks
            this.RemoteTicks = 2;
        }


        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// Initializes the network manager, sets up prefabs, servers, and NAT traversal if necessary.
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
                this.Initialize(); // Initialize the network manager

                // TODO: Find a way to add and remove components from prefab during play and stop game
                if (this.prefabsDatabase != null) {
                    // Iterate through each network prefab and inject network functionality
                    foreach (NetworkPrefabEntry networkPrefab in this.prefabsDatabase.GetPrefabs()) {
                        if (networkPrefab.GetPrefab() != null) {
                            networkPrefab.GetPrefab().InjectNetwork();
                        } else {
                            // Log an error if a network prefab is missing its associated GameObject
                            NetworkDebugger.LogError(String.Format("Some network prefab haven't associated gameobject [Network prefab id : {0}]. Check network prefabs into NetworkManager", networkPrefab.GetId()));
                        }
                    }
                }

                // If the transport database is null, create and fill it (Editor only)
    #if UNITY_EDITOR
                if ((this.transportsDatabase == null) || (this.transportsDatabase.GetTransports().Count() == 0)) {
                    NetworkTransportsDatabaseWindow.OpenNetworkDatabaseSource();
                    this.transportsDatabase = Resources.Load<NetworkTransportsDatabase>(GlobalResources.TRANSPORTS_DATABASE_PATH);
                    // Register embedded transport systems
                    this.transportsDatabase.RegisterTransport("ObjectNetTransport", "com.onlineobject.objectnet.embedded.ObjectNetServer",      "com.onlineobject.objectnet.embedded.ObjectNetClient",      false);                    
                    this.transportsDatabase.RegisterTransport("UnityTransport",     "com.onlineobject.objectnet.UnityServer",                   "com.onlineobject.objectnet.UnityClient",                   false);
                    this.transportsDatabase.RegisterTransport("SteamTransport",     "com.onlineobject.objectnet.steamworks.SteamNetworkServer", "com.onlineobject.objectnet.steamworks.SteamNetworkClient", false);
                    this.transportsDatabase.GetTransport("ObjectNetTransport").SetActive(true);
                } else if (this.transportsDatabase.HasTransport("UnityTransport") == false) {
                    this.transportsDatabase.RegisterTransport("UnityTransport", "com.onlineobject.objectnet.UnityServer", "com.onlineobject.objectnet.UnityClient", false);
                } else if (this.transportsDatabase.HasTransport("SteamTransport") == false) {
                    this.transportsDatabase.RegisterTransport("SteamTransport", "com.onlineobject.objectnet.steamworks.SteamNetworkServer", "com.onlineobject.objectnet.steamworks.SteamNetworkClient", false);
                }
    #endif

                // Create server instances based on configuration
                ServerAddressEntry[] serversEntryes = null;
                if (NetworkConnectionType.Server.Equals(this.connectionType)) {
                    // On ANY the value will be empy and system will use port inly when start bind
                    if (this.useAnyAddress) {
                        serversEntryes              = new ServerAddressEntry[1];
                        serversEntryes[0].Address   = "";
                    } else // If use fixed address i'm going to defien it here, for public and internal address i need to get before start server
                        if (this.useFixedAddress) {                    
                        serversEntryes = new ServerAddressEntry[1];
                        serversEntryes[0].Address   = this.hostServerAddress;
                    } else {
                        serversEntryes              = new ServerAddressEntry[1];
                        serversEntryes[0].Address   = this.hostServerAddress; // This value will be overriden before network start
                    }
                } else {
                    if (this.useDynamicAddress) {
                        serversEntryes = this.requestServerComponent.GetType()
                                                                    .GetMethod(this.requestServerMethod)
                                                                    .Invoke(this.requestServerComponent, new object[] { }) as ServerAddressEntry[];
                    } else if (this.useMultipleServers) {
                        serversEntryes = new ServerAddressEntry[this.networkServers.Count];
                        for (int index = 0; index < this.networkServers.Count; index++) {
                            serversEntryes[index].Address = this.networkServers[index];
                        }
                    } else if (this.useSingleServers) {
                        serversEntryes              = new ServerAddressEntry[1];
                        serversEntryes[0].Address   = this.serverAddress;
                    }                
                }
                // Add server information to the servers list
                foreach (ServerAddressEntry serverInfo in serversEntryes) {
                    this.servers.Add(new NetworkServerData(this, serverInfo));
                }
                // Initialize the network clock
                this.NetworkClock.Initialize();

                // Ping selected servers to get the closer possible
                if (this.servers.Count > 0) {
                    foreach (NetworkServerData server in this.servers) {
                        server.Ping();
                    }
                }

                // Set up NAT traversal if configured to do so
                if (this.useNatTraversal) {
                    this.natHelper = new NatHelper(this.tcpConnectionPort, this.udpConnectionPort);                    
                }
            }
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// Automatically connects to the network if configured to do so.
        /// </summary>
        private void Start() {
            // Automatically connect to the network if not in manual connection mode and autoConnectOnStart is true
            if (!NetworkConnectionType.Manual.Equals(this.connectionType) && this.autoConnectOnStart) {
                this.ConfigureMode(this.connectionType);
                this.StartNetwork(false, this.connectionDelay);
            }
        }

        /// <summary>
        /// OnDisable is called when the object becomes inactive.
        /// Stops any active server and client connections.
        /// </summary>
        private void OnDisable() {
            // Stop server connection if it exists
            if (this.HasConnection(ConnectionType.Server)) {
                this.GetConnection(ConnectionType.Server).GetSocket().Stop();
            }
            // Stop client connection if it exists
            if (this.HasConnection(ConnectionType.Client)) {
                this.GetConnection(ConnectionType.Client).GetSocket().Stop();
            }
        }

        /// <summary>
        /// Update is called once per frame.
        /// Performs base update operations, frame rate calculations, and object spawn/despawn detection.
        /// </summary>
        public override void Update() {
            if (this.IsMultiplayerAvaiable()) {
                base.Update(); // Call base Update to perform base executions
                this.currentFramePeerSecond = this.InternalCalculateFrameRate();
                if (this.IsRunningLogic()) {
                    this.DetectSpawnedObjects();
                    this.DetectDestroyedObjects();
                    this.RemoveDelayedDespawnedElements();
                    this.CheckRemoteSceneLoadTimeout();
                } else if (!this.IsLoadingRemoteScene()) {
                    this.InvalidateSpawnedObjects();
                    if (this.InSceneObjectsCollected()) {
                        this.InitializeInSceneObjectsOnClient();
                        if (!this.InSceneObjectsRequested()) {
                            this.RequestNetworkInSceneUpdate();
                            this.FlagInSceneObjectsRequested();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update is called once per frame.
        /// Performs base late update operations, frame rate calculations, and object spawn/despawn detection.
        /// </summary>
        public void LateUpdate() {
            if (this.IsMultiplayerAvaiable()) {
                if (this.IsRunningLogic()) {
                    this.RemoveUpdatedClientsToUpdateVariables();
                }
            }
        }

        /// <summary>
        /// Determines if the network logic is currently running.
        /// </summary>
        /// <returns>True if the network logic is running, otherwise false.</returns>
        public bool IsRunningLogic() {
            bool result = false;
            result |= (this.InEmbeddedMode() && this.HasConnection(ConnectionType.Server));
            result |= (this.InAuthoritativeMode() && this.HasConnection(ConnectionType.Server));
            result |= (this.IsConnectedOnRelayServer() && this.IsMasterPlayer());
            return result;
        }

        /// <summary>
        /// Return current manual execution tick
        /// </summary>
        /// <returns>Manual execution tick</returns>
        public int GetManualExecutionTick() {
            return this.manualExecutionTick;
        }

        /// <summary>
        /// Return current manual execution tick
        /// </summary>
        /// <returns>Manual execution tick</returns>
        public int GetReliableExecutionTick() {
            return this.reliableExecutionTick;
        }

        /// <summary>
        /// Flag system to check if raliable network variables was modified
        /// </summary>
        public void DetectVariablesChanges(bool includeAutomatic = false) {
            this.manualExecutionTick++;
            if (includeAutomatic) {
                this.reliableExecutionTick++;
            }
        }
        

        /// <summary>
        /// Determines if the network is currently processing inputs.
        /// </summary>
        /// <returns>True if inputs are being processed, otherwise false.</returns>
        public bool IsProcessingInputs() {
            bool result = false;
            result |= (this.IsRunningLogic() && this.IsRemoteInputEnabled());
            return result;
        }

        /// <summary>
        /// FixedUpdate is called every fixed framerate frame.
        /// Updates network ticks and performs fixed update operations.
        /// </summary>
        public override void FixedUpdate() {
            base.FixedUpdate();
            if (!this.IsRunningLogic()) {
                this.IsRemoteTicksInitialized   = true;
                this.RemoteTicks                = this.NetworkClock.Tick;
            }
            if (this.currentLocalTicks != this.NetworkClock.Tick) {
                this.currentLocalTicks = this.NetworkClock.Tick;
            }
        }

        /// <summary>
        /// Initiates a network connection, with optional reconnection and delay parameters.
        /// </summary>
        /// <param name="reconnection">Indicates whether this is a reconnection attempt.</param>
        /// <param name="delayToExecute">The delay in seconds before attempting the connection.</param>
        public void StartNetwork(bool reconnection = false, int delayToExecute = 0, int reconnectionAttemps = 0) {
            // When isn't a reconnection i need to reset current reconnection attemps
            if (reconnection == false) {
                this.currentAttemps = 0;
            }
            // If an asynchronous connection is already in progress or a reconnection is requested,
            // start the asynchronous connection coroutine with the specified delay.
            if ((this.asyncConnect  == true) ||
                (reconnection       == true)) {
                StartCoroutine(AsyncConnect(delayToExecute, reconnection));
            } else {
                // If NAT traversal is used and the router is not yet configured,
                // configure the router and then perform the network connection.
                if ((this.useNatTraversal) &&
                    (!this.natHelper.IsForwarded())) {
                    this.natHelper.ConfigureRouter(() => {
                        this.PerformNetworkConnection(reconnection);
                    }, () => {
                        this.PerformNetworkConnection(reconnection);
                    });
                } else {
                    // If NAT traversal is not used or the router is already configured,
                    // perform the network connection directly.
                    this.PerformNetworkConnection(reconnection);
                }
            }
        }

        /// <summary>
        /// Stops the network connection.
        /// </summary>
        public void StopNetwork() {
            // Perform network disconnection logic.
            this.PerformNetworkDisconnection();
        }
        
        /// <summary>
        /// Pause the disconnect detection
        /// </summary>
        /// <param name="hazardPause">Execute a hazard pause method
        /// 
        /// WARNING: This parameter shall be avoided to use as TRUE as much as possible since they disable the 
        ///          realiable messages not ensuring that those messages arrives on his destinations
        ///          we strongly don't recomment the use of this technique. 
        ///          Use with you own responsability
        ///          
        /// </param>
        public void PauseDisconnectDetection(bool hazardPause = false) {
            this.GetConnection().SetTimeoutEnabled(false);
            this.GetConnection().SetDisconnectionEnabled(false);
            if ( hazardPause ) {
                TransportDefinitions.DisableDisconnectDetections();
            }
            // When this is trigger on server i need to tell to all client's to don't disconnect either
            if (this.IsRunningLogic()) {
                // Send message to all cient's to stop timeout disconnection
                using (DataStream writer = new DataStream()) {
                    writer.Write(this.GetConnection(ConnectionType.Server).GetSocket().GetConnectionID());
                    writer.Write(false);
                    writer.Write(hazardPause);
                    this.GetConnection(ConnectionType.Server).Send(CoreGameEvents.DisconnectTimeoutChange, writer, DeliveryMode.Reliable); // Send message
                }
            }
        }

        /// <summary>
        /// Resume a paused disconnect detection
        /// </summary>
        public void ResumeDisconnectDetection() {
            this.GetConnection().SetTimeoutEnabled(true);
            this.GetConnection().SetDisconnectionEnabled(true);
            TransportDefinitions.ResumeDisconnectDetections(); // Bring values back
            // When this is trigger on server i need to tell to all client's to disconnect by timeout
            if (this.IsRunningLogic()) {
                // Send message to all cient's to resume timeout disconnection
                using (DataStream writer = new DataStream()) {
                    writer.Write(this.GetConnection(ConnectionType.Server).GetSocket().GetConnectionID());
                    writer.Write(true);
                    writer.Write(false);
                    this.GetConnection(ConnectionType.Server).Send(CoreGameEvents.DisconnectTimeoutChange, writer, DeliveryMode.Reliable); // Send message
                }
                // Flag to update all network variables ( just in case of someone was lost )
                foreach (NetworkPlayer player in this.GetPlayers<NetworkPlayer>()) {
                    NetworkManager.Instance().RegisterClientToUpdateVariables(player.GetClient());
                }
            }
        }

        /// <summary>
        /// Return if disconnect detection is enabled
        /// </summary>
        /// <returns>True is paused, otherwise false</returns>
        public bool IsTimeoutDetectionEnabled() {
            return this.GetConnection().IsTimeoutEnabled();
        }

        /// <summary>
        /// Return if this connection can be disconnected by disconnect detection
        /// </summary>
        /// <returns>True is can be disconnected, otherwise false</returns>
        public bool IsDisconnectionEnabled() {
            return this.GetConnection().IsDisconnectionEnabled();
        }

        /// <summary>
        /// Initialize all object in scene that need has network informations
        /// </summary>
        /// <param name="reader">The data stream containing the instantiation information.</param>
        public void InitializeInSceneObjectsOnClient() {
            // Start to extract parameters
            while (NetworkManager.Instance().HasInSceneObjects()) {
                GameObject inSceneObject = NetworkManager.Instance().GetNextInSceneObject(false);
                NetworkInstantiateDetection detectionScript = inSceneObject.GetComponent<NetworkInstantiateDetection>();
                if (detectionScript != null) {
                    NetworkPrefabEntry prefabEntry = NetworkManager.Instance().GetNetworkPrefabEntry(detectionScript.GetPrefabSignature());
                    if (prefabEntry != null) {
                        int objectNetworkId = detectionScript.GetStaticId();
                        int networkPrefabId = prefabEntry.GetId();
                        bool objectAutoSync = prefabEntry.GetAutoSync();
                        Vector3 prefabPosition = inSceneObject.transform.position;
                        Vector3 prefabRotation = inSceneObject.transform.eulerAngles;
                        Vector3 prefabScale = inSceneObject.transform.localScale;

                        // Then instantiate prefab on local
                        NetworkManager.Instance().InstantiateOnClient(networkPrefabId,
                                                                      prefabPosition,
                                                                      prefabRotation,
                                                                      prefabScale,
                                                                      objectNetworkId,
                                                                      objectAutoSync,
                                                                      false,
                                                                      false,
                                                                      this.GetConnection()?.GetSocket()?.GetLocalClient(),
                                                                      0,
                                                                      0,
                                                                      true);
                    } else {
                        NetworkManager.Instance().UnRegisterInSceneObject(inSceneObject);
                        NetworkDebugger.LogError("Network Spawn Object failed : Prefab is not assigend on network prefab \"{0}\"", inSceneObject.name);
                    }
                } else {
                    NetworkManager.Instance().UnRegisterInSceneObject(inSceneObject);
                }
            }            
        }

        /// <summary>
        /// Map network peer to peer port and execute callback after finished
        /// </summary>
        /// <param name="callBack">Callback to be executed</param>
        public void MapPeerToPeerPort(ushort targetPort, Action<bool> callBack) {
            if ((this.useNatTraversal) &&
                ((this.natHelperPeerToPeer == null) || (!this.natHelperPeerToPeer.IsForwarded()))) {
                if (this.natHelperPeerToPeer == null) {
                    this.natHelperPeerToPeer = new NatHelper(targetPort, targetPort);
                } else {
                    this.natHelperPeerToPeer.SetTcpPort(targetPort);
                    this.natHelperPeerToPeer.SetUdpPort(targetPort);
                }
                this.natHelperPeerToPeer.ConfigureRouter(() => {
                    callBack.Invoke(true);
                }, () => {
                    callBack.Invoke(false);
                });
            } else {
                callBack.Invoke(true);
            }
        }

        /// <summary>
        /// Sends data over the network to the server or client connection.
        /// </summary>
        /// <param name="eventCode">The event code associated with the message.</param>
        /// <param name="writer">The data stream containing the message to send.</param>
        /// <param name="mode">The delivery mode for the message (default is unreliable).</param>
        public void Send(int eventCode, DataStream writer, DeliveryMode mode = DeliveryMode.Unreliable) {
            // If there is a server connection, send the data to the server.
            if (this.HasConnection(ConnectionType.Server)) {
                this.GetConnection(ConnectionType.Server).Send(eventCode, writer, mode);
                // If there is a client connection, send the data to the client.
            } else if (this.HasConnection(ConnectionType.Client)) {
                this.GetConnection(ConnectionType.Client).Send(eventCode, writer, mode);
            }
        }

        /// <summary>
        /// Coroutine to handle asynchronous network connection with an optional delay.
        /// </summary>
        /// <param name="delayToExecute">The delay in seconds before attempting the connection.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        private IEnumerator AsyncConnect(int delayToExecute = 0, bool reconnection = false) {
            // Wait for the specified delay before proceeding with the connection.
            yield return new WaitForSeconds(delayToExecute);
            // If NAT traversal is used and the router is not yet configured,
            // configure the router and then perform the network connection.
            if ((this.useNatTraversal) &&
                (!this.natHelper.IsForwarded())) {
                this.natHelper.ConfigureRouter(() => {
                    this.PerformNetworkConnection(reconnection);
                }, () => {
                    this.PerformNetworkConnection(reconnection);
                });
            } else {
                // If NAT traversal is not used or the router is already configured,
                // perform the network connection directly.
                this.PerformNetworkConnection(reconnection);
            }
            yield return null;
        }

        /// <summary>
        /// Performs the actual network connection based on the current connection type.
        /// </summary>
        private void PerformNetworkConnection(bool reconnection = false) {
            if (!this.serverSocket.IsConnected()) {
                // Check the connection type and perform the appropriate connection logic.
                if (NetworkConnectionType.Server.Equals(this.connectionType)) {
                    // Server-specific connection logic, including event subscriptions and socket configuration.
                    this.serverSocket.OnServerStarted(this.OnServerStarted);
                    this.serverSocket.OnClientConnected(this.OnServerDetectClientConnection);
                    this.serverSocket.OnClientDisconnected(this.OnServerDetectClientDisconnection);
                    this.serverSocket.Configure(this.SelectServerToConnect().Address,
                                                (ushort)this.tcpConnectionPort,
                                                (ushort)this.udpConnectionPort,
                                                (ushort)(this.idleTimeutDetection * MILLISECONDS_MULTIPLIER));
                    if (this.currentAttemps < this.reconnectionAttemps) {
                        this.currentAttemps++;
                        this.serverSocket.Connect();
                    } else {
                        this.currentAttemps = 0;// Bring reconnection attermps to zero
                    }
                } else if (NetworkConnectionType.Client.Equals(this.connectionType)) {
                    // Client-specific connection logic, including event subscriptions and socket configuration.
                    this.clientSocket.OnConnected(this.ClientConnectionEvent);
                    this.clientSocket.OnConnectedRelayServer(this.ClientConnectedOnRelayServerEvent);
                    this.clientSocket.OnDisconnected(this.ClientDisconnectionEvent);
                    this.clientSocket.OnConnectionFailed(this.ClientConnectionFailedEvent);
                    this.clientSocket.OnLoginFailed(this.OnLoginFailedEvent);
                    this.clientSocket.OnLoginSucess(this.OnLoginSucessEvent);
                    this.clientSocket.Configure(this.SelectServerToConnect().Address,
                                                (ushort)this.tcpConnectionPort,
                                                (ushort)this.udpConnectionPort,
                                                (ushort)(this.idleTimeutDetection * MILLISECONDS_MULTIPLIER),
                                                this.clientConnectDelay);
                    if (this.currentAttemps < this.reconnectionAttemps) {
                        this.currentAttemps++;
                        this.clientSocket.Connect(reconnection);
                    } else {
                        this.currentAttemps = 0;// Bring reconnection attermps to zero
                    }
                } else if (NetworkConnectionType.Manual.Equals(this.connectionType)) {
                    // Manual mode requires the user to configure the transport manually.
                    throw new Exception("On manual mode you need to configure transport manually");
                }
            } else {
                throw new Exception("Socket is already connected. Disconnect before trying to connect");
            }
        }

        /// <summary>
        /// Performs the network disconnection based on the current connection type.
        /// </summary>
        private void PerformNetworkDisconnection() {
            NetworkManager.Instance().DisableAutoReconnect();
            // Check the connection type and perform the appropriate disconnection logic.
            if (NetworkConnectionType.Server.Equals(this.connectionType)) {
                this.serverSocket.Disconnect();
            } else if (NetworkConnectionType.Client.Equals(this.connectionType)) {
                this.clientSocket.Disconnect();
            }
        }


        /// <summary>
        /// Called when the server has started. This method handles the initialization of the server player,
        /// including login validation if necessary, and spawning the player's GameObject.
        /// </summary>
        private void OnServerStarted(IChannel channel) {
            // Check if the server is running in embedded mode
            if (this.InEmbeddedMode()) {
                // Check if login is required before spawning the player
                if (NetworkManager.Instance().IsLoginEnabled()) {
                    // Retrieve login parameters and their types
                    object[] loginParameters = NetworkManager.Instance().GetLoginInformations();
                    Type[] parametersTypes = NetworkManager.Instance().GetLoginInformationsTypes();

                    // Validate login information if required
                    if (NetworkManager.Instance().IsToValidateLogin()) {
                        if (!NetworkManager.Instance().IsValidLogin(loginParameters.ToArray())) {
                            // If validation fails, throw an exception
                            throw new Exception("Login failure: The username or Password doesn't match");
                        }
                    }
                }

                // Spawn the server player GameObject
                if (this.enablePlayerSpawner) {
                    IPlayer networkPlayer = NetworkManager.Instance().RegisterNetworkPlayer(channel);
                    networkPlayer.SetMaster(true); // Player who start the server will always be the master
                    networkPlayer.SetLocal(true);
                    GameObject spawnedPlayer = (this.isHostMigrated() == true) ? this.GetLocalPlayerObject() : this.SpawnServerPlayer(networkPlayer);

                    // If login is enabled, set the player's tag with login parameters
                    if (spawnedPlayer != null) {
                        if (NetworkManager.Instance().IsLoginEnabled()) {
                            NetworkPlayerTag playerTagControl = spawnedPlayer.GetComponent<NetworkPlayerTag>();
                            playerTagControl.SetAttributesValues(NetworkManager.Instance().GetLoginInformations().ToArray<object>());
                            playerTagControl.SetAttributesTypes(NetworkManager.Instance().GetLoginInformationsTypes());
                        }
                    }
                }
            }
            // Fires server starting up event
            this.ServerStartEvent(this.serverSocket.GetSocket());
        }

        /// <summary>
        /// Spawn player game opbject
        /// </summary>
        /// <param name="playerPrefab">Optional prefab to soawn</param>
        public void SpawnPlayer(GameObject playerPrefab = null) {
            this.InternalSpawnPlayer(playerPrefab, false, Vector3.zero, false, Quaternion.identity);
        }

        /// <summary>
        /// Spawn player game opbject
        /// </summary>
        /// <param name="playerPrefab">Prefab to spawn</param>
        /// <param name="position">Position to spawn</param>
        public void SpawnPlayer(GameObject playerPrefab, Vector3 position) {
            this.InternalSpawnPlayer(playerPrefab, true, position, false, Quaternion.identity);
        }

        /// <summary>
        /// Spawn player game opbject
        /// </summary>
        /// <param name="playerPrefab">Prefab to spawn</param>
        /// <param name="position">Position to spawn</param>
        /// <param name="rotation">Rotation to spawn</param>
        public void SpawnPlayer(GameObject playerPrefab, Vector3 position, Quaternion rotation) {
            this.InternalSpawnPlayer(playerPrefab, true, position, true, rotation);
        }


        /// <summary>
        /// Spawn client player game object on server side
        /// </summary>
        /// <param name="client">Client to respawn player</param>
        /// <param name="connectionId">Connection Id</param>
        /// <param name="playerId">Player Id</param>
        /// <param name="playerPrefab">Optional prefab to soawn</param>
        public GameObject SpawnClientPlayer(IClient client, int connectionId, ushort playerId, GameObject playerPrefab = null) {
            return this.SpawnClientPlayerOnServer(client, connectionId, false, Vector3.zero, false, Quaternion.identity, playerId, NetworkPlayerSpawnTime.Automatic, playerPrefab);            
        }

        /// <summary>
        /// Spawn client player game object on server side
        /// </summary>
        /// <param name="client">Client to respawn player</param>
        /// <param name="connectionId">Connection Id</param>
        /// <param name="playerId">Player Id</param>
        /// <param name="playerPrefab">Prefab to spawn</param>
        /// <param name="position">Position to spawn</param>
        public GameObject SpawnClientPlayer(IClient client, int connectionId, ushort playerId, GameObject playerPrefab, Vector3 position) {
            return this.SpawnClientPlayerOnServer(client, connectionId, true, position, false, Quaternion.identity, playerId, NetworkPlayerSpawnTime.Automatic, playerPrefab);            
        }

        /// <summary>
        /// Spawn client player game object on server side
        /// </summary>
        /// <param name="client">Client to respawn player</param>
        /// <param name="connectionId">Connection Id</param>
        /// <param name="playerId">Player Id</param>
        /// <param name="position">Position to spawn</param>
        public GameObject SpawnClientPlayer(IClient client, int connectionId, ushort playerId, Vector3 position) {
            return this.SpawnClientPlayerOnServer(client, connectionId, true, position, false, Quaternion.identity, playerId, NetworkPlayerSpawnTime.Automatic);
        }

        /// <summary>
        /// Spawn client player game object on server side
        /// </summary>
        /// <param name="client">Client to respawn player</param>
        /// <param name="connectionId">Connection Id</param>
        /// <param name="playerId">Player Id</param>
        /// <param name="playerPrefab">Prefab to spawn</param>
        /// <param name="position">Position to spawn</param>
        /// <param name="rotation">Rotation to spawn</param>
        public GameObject SpawnClientPlayer(IClient client, int connectionId, ushort playerId, GameObject playerPrefab, Vector3 position, Quaternion rotation) {
            return this.SpawnClientPlayerOnServer(client, connectionId, true, position, true, rotation, playerId, NetworkPlayerSpawnTime.Automatic, playerPrefab);
        }

        /// <summary>
        /// Spawn client player game object on server side
        /// </summary>
        /// <param name="client">Client to respawn player</param>
        /// <param name="connectionId">Connection Id</param>
        /// <param name="playerId">Player Id</param>
        /// <param name="position">Position to spawn</param>
        /// <param name="rotation">Rotation to spawn</param>
        public GameObject SpawnClientPlayer(IClient client, int connectionId, ushort playerId, Vector3 position, Quaternion rotation) {
            return this.SpawnClientPlayerOnServer(client, connectionId, true, position, true, rotation, playerId, NetworkPlayerSpawnTime.Automatic);
        }

        /// <summary>
        /// Request server to respawn a client player
        /// </summary>
        /// <param name="playerNetworkElement">Network element of this player to be respawned</param>
        public void RequestPlayerRespawn(INetworkElement playerNetworkElement) {
            this.GetConnection(ConnectionType.Client).RequestPlayerRespawn(playerNetworkElement);
        }

        /// <summary>
        /// Request server to respawn a client player
        /// </summary>
        /// <param name="playerNetworkElement">Network element of this player to be respawned</param>
        /// <param name="playerPrefab">Player prefab to spawn</param>
        public void RequestPlayerRespawn(INetworkElement playerNetworkElement, GameObject playerPrefab) {
            this.GetConnection(ConnectionType.Client).RequestPlayerRespawn(playerNetworkElement, playerPrefab);
        }

        /// <summary>
        /// Request server to respawn a client player
        /// </summary>
        /// <param name="playerNetworkElement">Network element of this player to be respawned</param>
        /// <param name="position">Position to spawn object</param>
        public void RequestPlayerRespawn(INetworkElement playerNetworkElement, Vector3 position) {
            this.GetConnection(ConnectionType.Client).RequestPlayerRespawn(playerNetworkElement, position);
        }

        /// <summary>
        /// Request server to respawn a client player
        /// </summary>
        /// <param name="playerNetworkElement">Network element of this player to be respawned</param>
        /// <param name="playerPrefab">Player prefab to spawn</param>
        /// <param name="position">Position to spawn object</param>
        public void RequestPlayerRespawn(INetworkElement playerNetworkElement, GameObject playerPrefab, Vector3 position) {
            this.GetConnection(ConnectionType.Client).RequestPlayerRespawn(playerNetworkElement, playerPrefab, position);
        }

        /// <summary>
        /// Request server to respawn a client player
        /// </summary>
        /// <param name="playerNetworkElement">Network element of this player to be respawned</param>
        /// <param name="position">Position to spawn object</param>
        /// <param name="rotation">Rotation to spawn object</param>
        public void RequestPlayerRespawn(INetworkElement playerNetworkElement, Vector3 position, Quaternion rotation) {
            this.GetConnection(ConnectionType.Client).RequestPlayerRespawn(playerNetworkElement, position, rotation);
        }

        /// <summary>
        /// Request server to respawn a client player
        /// </summary>
        /// <param name="playerNetworkElement">Network element of this player to be respawned</param>
        /// <param name="playerPrefab">Player prefab to spawn</param>
        /// <param name="position">Position to spawn object</param>
        /// <param name="rotation">Rotation to spawn object</param>
        public void RequestPlayerRespawn(INetworkElement playerNetworkElement, GameObject playerPrefab, Vector3 position, Quaternion rotation) {
            this.GetConnection(ConnectionType.Client).RequestPlayerRespawn(playerNetworkElement, playerPrefab, position, rotation);
        }

        /// <summary>
        /// Spawn player game opbject
        /// </summary>
        /// <returns></returns>
        private void InternalSpawnPlayer(GameObject playerPrefab, bool includePosition, Vector3 position, bool includeRotation, Quaternion rotation) {
            if ( this.IsRunningLogic() ) {
                GameObject playerObject = this.SpawnServerPlayer(NetworkManager.Instance().GetMasterPlayer<IPlayer>(), includePosition, position, includeRotation, rotation, NetworkPlayerSpawnTime.Manually, playerPrefab);
                // If login is enabled, set the player's tag with login parameters
                if (NetworkManager.Instance().IsLoginEnabled()) {
                    NetworkPlayerTag playerTagControl = playerObject.GetComponent<NetworkPlayerTag>();
                    playerTagControl.SetAttributesValues(NetworkManager.Instance().GetLoginInformations().ToArray<object>());
                    playerTagControl.SetAttributesTypes(NetworkManager.Instance().GetLoginInformationsTypes());
                }
            } else {
                // Request server to spawn player
                int     connectionId    = this.GetConnection(ConnectionType.Client).GetSocket().GetConnectionID();
                string  prefabSignature = "";
                if (playerPrefab != null) {
                    foreach (NetworkPrefabEntry networkPrefab in this.prefabsDatabase.GetPrefabs()) {
                        if (playerPrefab.IsSamePrefab(networkPrefab.GetPrefab())) {
                            prefabSignature = networkPrefab.GetPrefab().GetComponent<NetworkInstantiateDetection>().GetPrefabSignature();
                        }
                    }
                }
                // Send event to spawn client player on server side
                using (DataStream writer = new DataStream()) {
                    writer.Write(connectionId);
                    writer.Write(prefabSignature);
                    writer.Write(includePosition);
                    writer.Write(position);
                    writer.Write(includeRotation);
                    writer.Write(rotation);

                    // Include player data information if login is enabled.
                    if (NetworkManager.Instance().IsLoginEnabled()) {
                        object[]    parametersValues    = NetworkManager.Instance().GetLoginInformations();
                        Type[]      parametersTypes     = NetworkManager.Instance().GetLoginInformationsTypes();
                        // Write the login information to the data stream.
                        for (int paramIndex = 0; paramIndex < parametersValues.Length; paramIndex++) {
                            writer.Write(parametersValues[paramIndex], parametersTypes[paramIndex]);
                        }
                    }
                    // Send request to server
                    this.GetConnection(ConnectionType.Client).Send(CoreGameEvents.RequestPlayerSpawn, writer, DeliveryMode.Reliable); // Send message
                }
                // Check to warning developper
                if (playerPrefab != null) {
                    if (string.IsNullOrEmpty(prefabSignature)) {
                        Debug.Log(string.Format("The prefab [{0}] is not registered as network prefab on prefabs database. The default player will be spawned", playerPrefab.name));
                    }
                }
            }
        }

        /// <summary>
        /// Spawns a server player GameObject based on the configured spawn position mode.
        /// </summary>
        /// <param name="player">Optional parameter representing the player to spawn. If null, a default player is spawned.</param>
        /// <param name="includePosition">Shall to use position on spawned player</param>
        /// <param name="position">Position value</param>
        /// <param name="includeRotation">Shall to use rotation on spawned player</param>
        /// <param name="rotation">Rotation value</param>
        /// <param name="spawnTime">Moment when player will be spawned</param>
        /// <param name="playerPrefab">Optional prefab to use</param>
        /// <param name="destroyPrevious">Must destroy previous player before soawn this new one</param>
        /// <returns>The spawned player GameObject.</returns>
        private GameObject SpawnServerPlayer(IPlayer                player          = null, 
                                             bool                   includePosition = false, 
                                             Vector3                position        = default(Vector3), 
                                             bool                   includeRotation = false, 
                                             Quaternion             rotation        = default(Quaternion),  
                                             NetworkPlayerSpawnTime spawnTime       = NetworkPlayerSpawnTime.Automatic, 
                                             GameObject             playerPrefab    = null,
                                             bool                   destroyPrevious = true) {
            if (this.currentSpawnedPlayer != null) {
                if (destroyPrevious == true) {
                    GameObject.DestroyImmediate(this.currentSpawnedPlayer);
                    this.currentSpawnedPlayer = null;
                }
            }            
            if (this.currentSpawnedPlayer == null) {
                if ((NetworkPlayerSpawnTime.Automatic.Equals(this.playerSpawnTimeMode)) ||
                    (NetworkPlayerSpawnTime.Manually.Equals(spawnTime))) {
                    // Get the main camera for later use
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

                    // Get the prefab that should be spawned for the player
                    GameObject          prefabToSpawn   = (playerPrefab == null) ? this.GetPlayerPrefabToSpawn() : playerPrefab;
                    NetworkPrefabEntry  prefabEntry     = this.GetNetworkPrefabEntry(prefabToSpawn);
                    Quaternion          targetRotation  = Quaternion.identity;
                    if (prefabEntry != null) {
                        if (prefabEntry.IsToUsePrefabRotation()) {
                            targetRotation = prefabToSpawn.transform.rotation;
                        }
                    }
                    // Spawn the player based on the configured spawn position mode
                    if (NetworkSpawnPositionMode.Fixed.Equals(spawnPositionMode)) {
                        this.currentSpawnedPlayer = this.SpawnLocalPlayer(prefabToSpawn, (includePosition == true) ? position : this.fixedPositionToSpawn.position, (includeRotation == true) ? rotation : targetRotation, true);
                    } else if (NetworkSpawnPositionMode.Multiple.Equals(spawnPositionMode)) {
                        this.currentSpawnedPlayer = this.SpawnLocalPlayer(prefabToSpawn, (includePosition == true) ? position : this.GetSpawnedPositionFromMultiple(), (includeRotation == true) ? rotation : targetRotation, true);
                    } else if (NetworkSpawnPositionMode.Dynamic.Equals(spawnPositionMode)) {
                        this.currentSpawnedPlayer = this.SpawnLocalPlayer(prefabToSpawn, (includePosition == true) ? position : this.GetSpawnedPositionFromDynamic(), (includeRotation == true) ? rotation : targetRotation, true);
                    }

                    // Add a tag to identify this GameObject as a player
                    this.currentSpawnedPlayer.AddComponent<NetworkPlayerTag>().SetPlayerIndex(this.playerIndexFactory++);
                    // If a player is provided, configure the network player reference
                    if (player != null) {
                        if (this.currentSpawnedPlayer.GetComponent<NetworkPlayerReference>() == null) {
                            NetworkPlayer networkPlayer = (player as NetworkPlayer);
                            if (networkPlayer.GetClient() != null) {
                                this.currentSpawnedPlayer.AddComponent<NetworkPlayerReference>().Configure(networkPlayer.GetClient(),
                                                                                                           (networkPlayer.GetClient().GetChannel() as Channel).GetConnectionID(),
                                                                                                           networkPlayer.GetPlayerId());
                            } else {
                                this.currentSpawnedPlayer.AddComponent<NetworkPlayerReference>().Configure(networkPlayer.GetChannel(),
                                                                                                           (networkPlayer.GetChannel() as Channel).GetConnectionID(),
                                                                                                           networkPlayer.GetPlayerId());
                            }
                        }
                    }

                    // Add a component to identify this player as the master player
                    this.currentSpawnedPlayer.AddComponent<NetworkMasterPlayer>();

                    // Register and initialize network scripts on the spawned player
                    NetworkScriptsReference scriptsReference = this.currentSpawnedPlayer.GetComponent<NetworkScriptsReference>();
                    if (scriptsReference == null) {
                        foreach (NetworkPrefabEntry networkPrefab in this.prefabsDatabase.GetPrefabs()) {
                            if (prefabToSpawn == networkPrefab.GetPrefab()) {
                                // Initialize scripts that are marked to be synchronized over the network
                                ScriptList scriptList = networkPrefab.GetPrefabScripts();
                                scriptsReference = this.currentSpawnedPlayer.AddComponent<NetworkScriptsReference>();
                                scriptsReference.CollectAll();
                                foreach (var scriptStatus in scriptList.GetScripts()) {
                                    scriptsReference.InitalizeNetworkVariables(scriptStatus.Script, scriptStatus.GetSynchronizedVariables());
                                }
                                break;
                            }
                        }
                    }

                    // Disable the main camera if it is not attached to the player and controlPlayerCameras is true
                    if (NetworkManager.Instance().controlPlayerCameras) {
                        if (this.currentSpawnedPlayer != null && mainCamera != null) {
                            bool isMainCameraOnPlayer = false;
                            Camera[] cameras = this.currentSpawnedPlayer.GetComponentsInChildren<Camera>();
                            if (cameras.Length > 1) {
                                Debug.Log("====================================================================================================");
                                Debug.Log("[ATTENTION] Spawned player has more than one camera inside prefab, the first camera will be the main");
                                Debug.Log("====================================================================================================");
                            }
                            foreach (Camera cam in cameras) {
                                isMainCameraOnPlayer |= (mainCamera == cam);
                            }
                            if ((!isMainCameraOnPlayer) && (cameras.Length > 0)) {
                                mainCamera.enabled = false;
                                AudioListener audioControl = mainCamera.GetComponent<AudioListener>();
                                if (audioControl != null) {
                                    audioControl.enabled = false;
                                }
                                // Enable the first camera found on the player
                                cameras[0].enabled = true;
                                AudioListener playerCameraAudio = cameras[0].GetComponent<AudioListener>();
                                if (playerCameraAudio != null) {
                                    playerCameraAudio.enabled = true;
                                }
                                // Detach the camera from the player if required
                                if (this.detachPlayerCamera) {
                                    cameras[0].gameObject.transform.parent = null;
                                    // Add the detached camera to the garbage collector list
                                    if (!this.garbageNetworkObjects.ContainsKey(this.networkSessionId)) {
                                        this.garbageNetworkObjects.Add(this.networkSessionId, new List<GameObject>());
                                    }
                                    this.garbageNetworkObjects[this.networkSessionId].Add(cameras[0].gameObject);
                                }
                            } else if (isMainCameraOnPlayer) {
                                if (!mainCamera.enabled) {
                                    mainCamera.enabled = true;
                                }
                            }
                        } else if (this.currentSpawnedPlayer != null && mainCamera == null) {
                            // Need to activate player camera
                            Camera[] cameras = this.currentSpawnedPlayer.GetComponentsInChildren<Camera>();
                            if (cameras.Length > 0) {
                                cameras[0].enabled = true;
                                AudioListener audioControl = cameras[0].GetComponent<AudioListener>();
                                if (audioControl != null) {
                                    audioControl.enabled = true;
                                }
                                // Detach the camera from the player if required
                                if (this.detachPlayerCamera) {
                                    cameras[0].gameObject.transform.parent = null;
                                    // Add the detached camera to the garbage collector list
                                    if (!this.garbageNetworkObjects.ContainsKey(this.networkSessionId)) {
                                        this.garbageNetworkObjects.Add(this.networkSessionId, new List<GameObject>());
                                    }
                                    this.garbageNetworkObjects[this.networkSessionId].Add(cameras[0].gameObject);
                                }
                            }
                        }
                    }
                    // Trigger player spawn event
                    this.PlayerSpawnedEvent(this.currentSpawnedPlayer);                    
                }
            } else {
                Debug.Log("Player already spawned [ You can't spawn same player twice ]");                
            }

            return this.currentSpawnedPlayer;
        }

        /// <summary>
        /// Spawns a client player on the server.
        /// </summary>
        /// <param name="client">The client interface.</param>
        /// <param name="connectionId">The connection ID of the player.</param>
        /// <param name="playerId">The ID of the player (default is 0).</param>
        /// <param name="playerPrefab">Optional player prefab</param>
        /// <returns>The spawned player GameObject.</returns>
        private GameObject SpawnClientPlayerOnServer(IClient                client, 
                                                     int                    connectionId,
                                                     bool                   includePosition = false,
                                                     Vector3                position        = default(Vector3),
                                                     bool                   includeRotation = false,
                                                     Quaternion             rotation        = default(Quaternion),
                                                     ushort                 playerId        = 0, 
                                                     NetworkPlayerSpawnTime spawnTime       = NetworkPlayerSpawnTime.Automatic, 
                                                     GameObject             playerPrefab    = null) {
            GameObject playerSpawned = null;
            if ((NetworkPlayerSpawnTime.Automatic.Equals(this.playerSpawnTimeMode)) ||
                (NetworkPlayerSpawnTime.Manually.Equals(spawnTime))) {
                // Intantiate player prefab
                GameObject          prefabToSpawn   = (playerPrefab == null) ? this.GetPlayerPrefabToSpawn() : playerPrefab;
                NetworkPrefabEntry  prefabEntry     = this.GetNetworkPrefabEntry(prefabToSpawn);
                Quaternion          targetRotation  = Quaternion.identity;
                if (prefabEntry != null) {
                    if (prefabEntry.IsToUsePrefabRotation()) {
                        targetRotation = prefabToSpawn.transform.rotation;
                    }
                }
                Vector3     positionToSpawn = (includePosition == true) ? position : this.GetPlayerPositionToSpawn();
                Quaternion  rotationToSpawn = (includeRotation == true) ? rotation : targetRotation;
                // Spawn player on server ( locally )
                playerSpawned               = this.SpawnLocalPlayer(prefabToSpawn, positionToSpawn, rotationToSpawn, true);

                // Spawn player
                IPlayer networkPlayer       = (playerId == 0) ? NetworkManager.Instance().RegisterNetworkPlayer(client) : NetworkManager.Instance().GetPlayer<IPlayer>(playerId);
                if (networkPlayer == null) {
                    networkPlayer = NetworkManager.Instance().RegisterNetworkPlayer(client, playerId);
                }
                // Tag this object as player object ( no matter where )
                playerSpawned.AddComponent<NetworkPlayerTag>().SetPlayerIndex(this.playerIndexFactory++);
                // Configure remote player identification for this object
                playerSpawned.AddComponent<NetworkPlayerReference>().Configure(client, connectionId, networkPlayer.GetPlayerId());

                // Register script control
                NetworkScriptsReference scriptsReference = playerSpawned.GetComponent<NetworkScriptsReference>();
                if (scriptsReference == null) {
                    // Since developers can flag not disable some script i need to check those scripts to tell to "NetworkScriptsReference" to ignore those scripts
                    // First i need to find prefab into network prefab list
                    if (this.prefabsDatabase != null) {
                        foreach (NetworkPrefabEntry networkPrefab in this.prefabsDatabase.GetPrefabs()) {
                            if (prefabToSpawn == networkPrefab.GetPrefab()) {
                                // now when i already know the prefab i will check if has ognored scripts
                                ScriptList scriptList               = networkPrefab.GetPrefabScripts();
                                ScriptList scriptListOnRemoteInput  = networkPrefab.GetInputScripts();

                                List<MonoBehaviour> ignoredListFromPrefab = new List<MonoBehaviour>();
                                List<Tuple<MonoBehaviour, float>> delayedListFromPrefab = new List<Tuple<MonoBehaviour, float>>();

                                List<MonoBehaviour> ignoredListFromPrefabOnRemoteInput = new List<MonoBehaviour>();
                                if (scriptList != null) {
                                    // Ignored scripts
                                    foreach (ScriptStatus script in scriptList.GetScripts()) {
                                        if (script.Script != null) {
                                            if (script.Enabled) {
                                                ignoredListFromPrefab.Add(script.Script);
                                            } else if (script.Delay > 0f) {
                                                delayedListFromPrefab.Add(new Tuple<MonoBehaviour, float>(script.Script, (script.Delay / MILLISECONDS_MULTIPLIER)));
                                            }
                                        }
                                    }
                                    // Ignored scripts when remote input is enabled
                                    foreach (ScriptStatus script in scriptListOnRemoteInput.GetScripts()) {
                                        if (!script.Enabled) {
                                            if (script.Script != null) {
                                                ignoredListFromPrefabOnRemoteInput.Add(script.Script);
                                            }
                                        }
                                    }
                                }
                                // Add script on spawned element
                                scriptsReference = playerSpawned.AddComponent<NetworkScriptsReference>();
                                scriptsReference.Collect(ignoredListFromPrefab, delayedListFromPrefab, ignoredListFromPrefabOnRemoteInput);

                                // Now initialize all network variables
                                scriptsReference.CollectAll();
                                foreach (var scriptStatus in scriptList.GetScripts()) {
                                    if (scriptStatus.Script != null) {
                                        scriptsReference.InitalizeNetworkVariables(scriptStatus.Script, scriptStatus.GetSynchronizedVariables());
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                // Check if any child component shall be disabled
                NetworkChildsReference      childsReference = playerSpawned.GetComponent<NetworkChildsReference>();
                NetworkInstantiateDetection detectionScript = playerSpawned.GetComponent<NetworkInstantiateDetection>();
                if (childsReference == null) {
                    if (this.prefabsDatabase != null) {
                        foreach (NetworkPrefabEntry networkPrefab in this.prefabsDatabase.GetPrefabs()) {
                            if (prefabToSpawn == networkPrefab.GetPrefab()) {
                                // now when i already know the prefab i will check if has ognored scripts
                                GameObjectList childsList = networkPrefab.GetChildObjects();
                                List<GameObject> disabledChildsList = new List<GameObject>();
                                List<GameObjectStatus> delayedListFromPrefab = new List<GameObjectStatus>();
                                if (childsList != null) {
                                    foreach (GameObjectStatus childObject in childsList.GetObjects()) {
                                        if (!childObject.Enabled) {
                                            disabledChildsList.Add(childObject.Target);
                                            delayedListFromPrefab.Add(childObject.GenerateSafetyCopy());  
                                        }
                                    }
                                }
                                // Flag components to disable
                                Transform[] spawnedChilds = playerSpawned.GetComponentsInChildren<Transform>().Where(t => t.Equals(playerSpawned.transform) == false).ToArray<Transform>();
                                detectionScript.ResetFlaggedCache();
                                int disabledChildIndex = 0;
                                foreach (GameObject toDisable in disabledChildsList) {
                                    int childIndex = 0;
                                    foreach (Transform child in spawnedChilds) {
                                        if (detectionScript.IsFlaggedToDisable(child.gameObject, true)) { 
                                            delayedListFromPrefab[disabledChildIndex].TargetInstance = child.gameObject;
                                            break;
                                        }
                                        ++childIndex;
                                    }
                                    ++disabledChildIndex;
                                }
                                // Add script on spawned element
                                childsReference = playerSpawned.AddComponent<NetworkChildsReference>();
                                childsReference.Collect(delayedListFromPrefab);
                                break;
                            }
                        }
                        if ((!this.useRemoteInput) && (!NetworkManager.Instance().InAuthoritativeMode())) {
                            childsReference.DisableChilds();
                        } else {
                            childsReference.enabled = false; // Disable to not execute object disable
                        }
                    }
                } else {
                    if ((!this.useRemoteInput) && (!NetworkManager.Instance().InAuthoritativeMode())) {
                        childsReference.DisableChilds();
                    } else {
                        childsReference.enabled = false; // Disable to not execute object disable
                    }
                }

                if (( this.IsConnectedOnRelayServer() ) && ( this.IsMasterPlayer() )) {
                    NetworkClient   clientTransport = new NetworkClient(connectionId);
                    clientTransport.SetTransport(new RelayTransportClient(client.GetTransport()));
                    clientTransport.SetChannel(client.GetChannel());
                
                    IPlayer newPlayer = new NetworkPlayer(playerId, String.Format("Player_{0}", playerId), clientTransport);
                    newPlayer.SetLocal(false);
                    newPlayer.SetMaster(false);

                    // Register player
                    NetworkManager.Instance().RegisterNetworkPlayer(newPlayer);
                    // Register fake connection on channel
                    client.GetChannel().RegisterClient(clientTransport);
                
                    scriptsReference.SetPlayer(newPlayer);
                }
                if ((!this.useRemoteInput) && (!NetworkManager.Instance().InAuthoritativeMode())) {
                    scriptsReference.DisableComponents();
                }

                // Trigger player spawn event
                this.PlayerSpawnedEvent(playerSpawned);                
            }
            
            return playerSpawned;
        }

        /// <summary>
        /// Confirms that a player object has been created on the client side and performs initial setup.
        /// </summary>
        /// <param name="client">The client interface representing the connected client.</param>
        /// <param name="networkId">The network identifier for the player object.</param>
        public void ConfirmPlayerCreatedOnClient(IClient client, int networkId) {
            // Get current network object
            if (NetworkManager.GetContainer().IsRegistered(networkId)) {
                INetworkElement networkElement   = NetworkManager.GetContainer().GetElement(networkId);
                GameObject      playerObject     = networkElement.GetGameObject();
                if (playerObject != null) {
                    NetworkObject networkObject = playerObject.GetComponent<NetworkObject>();
                    if (networkObject != null) {
                        // Register player into connected player
                        if (this.clients.ContainsKey(client) == true) { 
                            this.clients[client] = networkObject.GetNetworkElement(); // If p-layer is alredy registered could be a respawn of same player, on this case i need to update NetworkObject
                            networkObject.FlagRespawned(); // Flag this object as respawned object
                        } else {
                            this.clients.Add(client, networkObject.GetNetworkElement()); // register NetworkObject into client players list
                        }
                        networkObject.SetTransport(this.GetConnection((this.IsConnectedOnRelayServer()) ? ConnectionType.Client : ConnectionType.Server));
                        networkObject.SetDeliveryMode(this.sendUpdateMode);
                        networkObject.SetBehaviorMode(this.IsRemoteInputEnabled() ? BehaviorMode.Active : BehaviorMode.Passive);
                        networkObject.SetRemoteControlsEnabled(this.useRemoteInput);
                        // Initialize internal InternalExecutor
                        networkObject.InitializeExecutor();
                        // If this is an player instance i need to check if he has camera
                        if (NetworkManager.Instance().controlPlayerCameras) {
                            // If player camera is not the main camera i need to disable it either
                            if ((networkObject != null) &&
                                (networkObject.gameObject != null)) {
                                Camera mainCamera = Camera.main;
                                if (mainCamera != null) {
                                    Camera[] cameras = networkObject.gameObject.GetComponentsInChildren<Camera>();
                                    if (cameras.Length > 1) {
                                        Debug.Log("====================================================================================================");
                                        Debug.Log("[ATTENTION] Spawned player has more than one camera inside prefab, the first camera will be the main");
                                        Debug.Log("====================================================================================================");
                                    }
                                    foreach (Camera cam in cameras) {
                                        if (mainCamera != cam) {
                                            cam.enabled = false;
                                            AudioListener audioControl = cam.GetComponent<AudioListener>();
                                            if (audioControl != null) {
                                                audioControl.enabled = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // Inform client that his object instance is already spawned on server
                        using (DataStream writer = new DataStream()) {
                            writer.Write(networkObject.GetNetworkId()); // Send network id back
                            client.Send(CoreGameEvents.PlayerSpawnedOnServer, writer, DeliveryMode.Reliable); // Send message
                        }
                        // Initialize executor services
                        networkObject.InitializeExecutor();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the event when a client disconnects from the server.
        /// </summary>
        /// <param name="client">The client interface representing the disconnected client.</param>
        public void FireOnClientDisconnectedFromServer(IClient client) {
            // Trigger the global event for client disconnection if available
            if (NetworkGlobalEvents.Instance() != null) {
                NetworkGlobalEvents.Instance().OnClientDisconnected(client);
            }
            // Despawn the player associated with the disconnected client
            this.DespawnClientPlayer(client);
        }

        /// <summary>
        /// Handles the event when a client connects to the server.
        /// </summary>
        /// <param name="client">The client interface representing the newly connected client.</param>
        private void OnServerDetectClientConnection(IClient client) {
            // Trigger the global event for client connection if available
            if (NetworkGlobalEvents.Instance() != null) {
                NetworkGlobalEvents.Instance().OnClientConnected(client);
            }            
        }

        /// <summary>
        /// Handles the event when a client is detected as disconnected from the server.
        /// </summary>
        /// <param name="client">The client interface representing the disconnected client.</param>
        private void OnServerDetectClientDisconnection(IClient client) {
            // Trigger the global event for client disconnection if available
            if (NetworkGlobalEvents.Instance() != null) {
                NetworkGlobalEvents.Instance().OnClientDisconnected(client);
            }
            // Despawn the player associated with the disconnected client
            this.DespawnClientPlayer(client);
        }

        /// <summary>
        /// Despawns a client player from the game. This includes unregistering the player from the network manager,
        /// notifying other players about the disconnection, and potentially selecting a new master player if necessary.
        /// </summary>
        /// <param name="client">The client interface representing the player to be despawned.</param>
        private void DespawnClientPlayer(IClient client) {
            if (this.InRelayMode()) {
                NetworkPlayer despawnedPlayer = NetworkManager.Instance().GetPlayer<NetworkPlayer>(client);
                // Is isn't master player that was disconnected
                if ((this.HasMasterPlayer(despawnedPlayer.GetLobbyId()) == true) && 
                    (client != this.GetMasterPlayer<NetworkPlayer>(despawnedPlayer.GetLobbyId()).GetClient())) {
                    NetworkPlayer masterPlayer = this.GetMasterPlayer<NetworkPlayer>(despawnedPlayer.GetLobbyId());
                    using (DataStream writer = new DataStream()) {
                        writer.Write((despawnedPlayer.GetClient() as NetworkClient).GetConnectionId()); // Send connection id of disconnected client
                        masterPlayer.GetClient().Send(RelayServerEvents.DisconnectedFromServer, writer, DeliveryMode.Reliable);
                    }
                    // Unregister from connected players
                    this.clients.Remove(client);
                    // Unregister player
                    this.UnregisterNetworkPlayer(despawnedPlayer);
                    if (NetworkManager.Instance().IsRunningLogic()) {
                        NetworkManager.Instance().GetMasterPlayer<NetworkPlayer>(despawnedPlayer.GetLobbyId()).GetClient().GetChannel().UnregisterClient(client);
                    }
                    // Remove from lobby
                    if (NetworkManager.Instance().IsLobbyControlEnabled()) {
                        NetworkManager.Lobbies.GetLobby(despawnedPlayer.GetLobbyId())?.UnregisterPlayer(despawnedPlayer);
                    }
                } else if ((client != null) &&
                           (this.HasMasterPlayer(despawnedPlayer.GetLobbyId()) == true) &&
                           (client.Equals(this.GetMasterPlayer<NetworkPlayer>(despawnedPlayer.GetLobbyId()).GetClient()) == true)) {
                    // Detect associated object 
                    NetworkClient networkClient         = (client as NetworkClient);
                    NetworkPlayer disconnectedPlayer    = this.GetPlayer<NetworkPlayer>(client);
                    // Unregister from connected players
                    this.UnregisterNetworkPlayer(disconnectedPlayer);                    
                    // Unregister player
                    if (NetworkManager.Instance().IsRunningLogic()) {
                        NetworkManager.Instance().GetMasterPlayer<NetworkPlayer>(disconnectedPlayer.GetLobbyId()).GetClient().GetChannel().UnregisterClient(client);
                    }
                    // Remove from lobby
                    if (NetworkManager.Instance().IsLobbyControlEnabled()) {
                        NetworkManager.Lobbies.GetLobby(despawnedPlayer.GetLobbyId())?.UnregisterPlayer(disconnectedPlayer);
                    }
                    // Send to despawn from all clients and must reassign master player
                    foreach (NetworkPlayer player in this.GetPlayers<NetworkPlayer>(disconnectedPlayer.GetLobbyId())) {
                        using (DataStream writer = new DataStream()) {
                            writer.Write(networkClient.GetNetworkObjectId()); // Send object to need be destroyed on client's
                            player.GetClient().Send(CoreGameEvents.ObjectDestroy, writer, DeliveryMode.Reliable);
                        }
                    }
                    // Now need to select a new MasterPlayer ( if haven't will close lobby if exists )
                    if ( this.HasPlayers(disconnectedPlayer.GetLobbyId()) ) {
                        NetworkPlayer newMasterPlayer = this.GetFirstNoMasterPlayers<NetworkPlayer>(disconnectedPlayer.GetLobbyId());
                        if ( newMasterPlayer != null ) {
                            // Flag this player to be master
                            newMasterPlayer.SetMaster(true);
                            // Send notification to the layer
                            using (DataStream writer = new DataStream()) {
                                writer.Write(newMasterPlayer.GetPlayerId()); // ID of new master player
                                // Must send the list of current connected client
                                if (NetworkManager.Instance().IsLobbyControlEnabled() == true) {
                                    ILobby clientLobby = NetworkManager.Lobbies.GetLobby(newMasterPlayer.GetLobbyId());
                                    IPlayer[] players = clientLobby.GetPlayers();
                                    writer.Write(players.Count());
                                    foreach (IPlayer connectedPlayer in players) {
                                        writer.Write(((connectedPlayer as NetworkPlayer).GetClient() as NetworkClient).GetConnectionId()); // Write connection ID
                                        writer.Write(connectedPlayer.GetPlayerId()); // Write player ID
                                    }
                                } else {
                                    List<IClient> clients = this.GetConnection(ConnectionType.Server).GetSocket().GetConnectedClients();
                                    writer.Write(clients.Count()); // Send object to need be destroyed on client's
                                    foreach (NetworkClient connectedClient in this.GetConnection(ConnectionType.Server).GetSocket().GetConnectedClients() ) {
                                        NetworkPlayer connectedPlayer = this.GetPlayer<NetworkPlayer>(connectedClient);
                                        writer.Write(connectedClient.GetConnectionId());
                                        writer.Write(connectedPlayer.GetPlayerId());
                                    }
                                }
                                newMasterPlayer.GetClient().Send(RelayServerEvents.UpdateMasterPlayer, writer, DeliveryMode.Reliable);                            
                            }                            
                        } else {
                            NetworkManager.Lobbies.CloseLobby(despawnedPlayer.GetLobbyId()); // Finish lobby
                        }                        
                    } else {
                        NetworkManager.Lobbies.CloseLobby(despawnedPlayer.GetLobbyId()); // Finish lobby
                    }                
                }

                // Send player creation to all network peers
                if (NetworkManager.Instance().IsPeerToPeerEnabled()) {
                    IClient playerClient = (despawnedPlayer as NetworkPlayer).GetClient();
                    using (DataStream writer = new DataStream()) {
                        writer.Write(despawnedPlayer.GetPlayerId()); // Player ID
                        // Send to all clients on same lobby ( if lobby is enabled )
                        foreach (NetworkPlayer playerTo in NetworkManager.Instance().GetPlayers<NetworkPlayer>(despawnedPlayer.GetLobbyId())) {
                            if (playerTo != despawnedPlayer) {
                                playerTo.GetClient().Send(RelayServerEvents.DestroyNetworkPeer, writer, DeliveryMode.Reliable);
                            }
                        }
                    }
                }
            } else {
                if (this.despawnPlayerOnDisconnect) {
                    if (this.clients.ContainsKey(client)) {
                        INetworkElement networkElement = this.clients[client];
                        // Unregister from connected players
                        this.clients.Remove(client);
                        // Unregister player
                        NetworkPlayer disconnectedPlayer = this.GetPlayer<NetworkPlayer>(client);
                        if (disconnectedPlayer != null) {
                            this.UnregisterNetworkPlayer(disconnectedPlayer);
                        }
                        if (NetworkManager.Instance().IsRunningLogic()) {
                            if (this.HasConnection(ConnectionType.Server)) {
                                this.GetConnection(ConnectionType.Server).GetSocket().UnregisterClient(client);
                            } else if (this.HasConnection(ConnectionType.Client)) {
                                this.GetConnection(ConnectionType.Client).GetSocket().UnregisterClient(client);
                            }
                        }
                        // Destroy object - Must be executed into internal unity thread
                        GameObject.Destroy(networkElement.GetGameObject());
                    }
                } else if (this.despawnPlayerAfterDelay) {
                    if (this.clients.ContainsKey(client)) {
                        // Delay remove
                        INetworkElement networkElement = this.clients[client];
                        this.RegisterDelayedDespawn(client, networkElement);
                    }
                }
            }
            // Call user event
            if (this.onClientDisconnected != null) {
                this.onClientDisconnected.Invoke(client);
            }
        }

        /// <summary>
        /// Spawns a local player GameObject at a specified position.
        /// </summary>
        /// <param name="playerPrefab">The player prefab to instantiate.</param>
        /// <param name="spawnPosition">The position in the world to spawn the player.</param>
        /// <param name="spawnRotation">The rotation in the world to spawn the player.</param>
        /// <param name="disableCamera">Optional parameter to disable the camera on the player GameObject.</param>
        /// <returns>The instantiated player GameObject.</returns>
        private GameObject SpawnLocalPlayer(GameObject playerPrefab, Vector3 spawnPosition, Quaternion spawnRotation, bool disableCamera = false) {
            GameObject result = GameObject.Instantiate(playerPrefab, spawnPosition, spawnRotation);               
            // Need to disable any camera attached to the player ( if exists )
            if (disableCamera == true) {
                Camera[] cameras = result.GetComponentsInChildren<Camera>();
                foreach (Camera cam in cameras) {
                    cam.enabled = false;
                }                
            }
            return result;
        }

        /// <summary>
        /// Registers an object for delayed despawn.
        /// </summary>
        /// <param name="client">The client associated with the object.</param>
        /// <param name="element">The network element to be despawned.</param>
        private void RegisterDelayedDespawn(IClient client, INetworkElement element) {
            if (!this.IsDelayedDespawnRegistered(element)) {
                this.delayedDespawnedObjects.Add(new DelayedRemove(client, element, NetworkClock.Time + (float)this.delayToRemoveDisconnected));
            }
        }

        /// <summary>
        /// Cancels the delayed despawn of an object if it was registered.
        /// </summary>
        /// <param name="element">The network element to cancel despawn for.</param>
        private void CancelDelayedDespawn(INetworkElement element) {
            if (this.IsDelayedDespawnRegistered(element)) {
                DelayedRemove despawnToCancel = null;
                foreach (DelayedRemove delayed in this.delayedDespawnedObjects) {
                    if (delayed.GetElement().Equals(element)) {
                        despawnToCancel = delayed;
                        break;
                    }
                }
                if ( despawnToCancel != null ) {
                    this.delayedDespawnedObjects.Remove(despawnToCancel);
                }
            }
        }

        /// <summary>
        /// Checks if an object is registered for delayed despawn.
        /// </summary>
        /// <param name="element">The network element to check.</param>
        /// <returns>True if the object is registered for delayed despawn, false otherwise.</returns>
        private bool IsDelayedDespawnRegistered(INetworkElement element) {
            bool result = false;
            foreach (DelayedRemove delayed in this.delayedDespawnedObjects) {
                if (delayed.GetElement().Equals(element)) {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Removes elements that have reached their delayed despawn time.
        /// </summary>
        private void RemoveDelayedDespawnedElements() {
            this.objectsToDespawn.Clear();

            foreach (DelayedRemove delayed in this.delayedDespawnedObjects) {
                if (delayed.IsTimedOut()) {
                    objectsToDespawn.Add(delayed);
                }
            }
            while (objectsToDespawn.Count > 0) {
                DelayedRemove toRemove = objectsToDespawn[0];
                objectsToDespawn.RemoveAt(0);
                this.delayedDespawnedObjects.Remove(toRemove);

                // Unregister from connected players
                INetworkElement networkElement = toRemove.GetElement();
                // Unregister from connected players
                this.clients.Remove(toRemove.GetClient());
                // Unregister player
                NetworkPlayer disconnectedPlayer = this.GetPlayer<NetworkPlayer>(toRemove.GetClient());
                if (disconnectedPlayer != null) {
                    this.UnregisterNetworkPlayer(disconnectedPlayer);
                }
                // Unregister client
                if (NetworkManager.Instance().IsRunningLogic()) {
                    NetworkTransport connection = null;
                    if (this.HasConnection(ConnectionType.Client)) {
                        connection = this.GetConnection(ConnectionType.Client);
                    } else if (this.HasConnection(ConnectionType.Server)) {
                        connection = this.GetConnection(ConnectionType.Server);
                    }
                    if (connection != null) {
                        connection.GetSocket().UnregisterClient(toRemove.GetClient());
                    }
                }
                // Remove from container
                NetworkManager.GetContainer().UnRegister(networkElement);
                // Destroy object
                GameObject.Destroy(networkElement.GetGameObject());
            }
        }

        /// <summary>
        /// Handles the event when a client lost his connection with server.
        /// </summary>
        /// <param name="exception">The exception that was thrown during the connection attempt.</param>
        private void ClientConnectionFailedEvent(Exception exception) {
            if ( NetworkManager.Instance().autoReconnect ) {
                if (!NetworkManager.Instance().disableReconnect) {
                    NetworkManager.Instance().StartNetwork(true, this.reconnectionDelay, this.reconnectionAttemps);
                }
                NetworkManager.Instance().disableReconnect = false;
            }
            // Call client event
            if ( NetworkGlobalEvents.Instance() != null ) {
                NetworkGlobalEvents.Instance().OnConnectionFailed(exception);
            }
        }

        /// <summary>
        /// Handles the event when server start to listen for incoming connections
        /// </summary>
        private void ServerStartEvent(IChannel channel) {
            if (NetworkGlobalEvents.Instance() != null) {
                NetworkGlobalEvents.Instance().OnServerStarted(channel);
            }
        }

        /// <summary>
        /// Handles the event when any player was spawned
        /// </summary>
        private void PlayerSpawnedEvent(GameObject player) {
            if (NetworkGlobalEvents.Instance() != null) {
                NetworkGlobalEvents.Instance().OnPlayerSpawned(player);
            }
        }

        /// <summary>
        /// Handles the event when player start his network services
        /// </summary>
        private void PlayerStartedEvent(INetworkElement networkElement) {
            if (NetworkGlobalEvents.Instance() != null) {
                NetworkGlobalEvents.Instance().OnPlayerStarted(networkElement);
            }
        }

        /// <summary>
        /// Handles the event when client became owner of his player 
        /// </summary>
        private void PlayerOwnerDetected(INetworkElement networkElement) {
            if (NetworkGlobalEvents.Instance() != null) {
                NetworkGlobalEvents.Instance().OnPlayerOwnerDetected(networkElement);
            }
        }
        
        /// <summary>
        /// Handles the event when client receive his connection id
        /// </summary>
        private void ConnectionIdAssigned(int connectonId) {
            if (NetworkGlobalEvents.Instance() != null) {
                NetworkGlobalEvents.Instance().OnConnectionIdAssigned(connectonId);
            }
        }
        
        /// <summary>
        /// Handles the event when remote scene load process was finished
        /// </summary>
        /// <param name="sceneName">Loaded scene name</param>
        private void RemoteSceneLoadFinishedEvent(string sceneName) {
            if (NetworkGlobalEvents.Instance() != null) {
                NetworkGlobalEvents.Instance().OnSceneLoadFinished(sceneName);
            }
        }

        /// <summary>
        /// Handles the event when remote scene unload process was finished
        /// </summary>
        /// <param name="sceneName">Loaded scene name</param>
        private void RemoteSceneUnLoadFinishedEvent(string sceneName) {
            if (NetworkGlobalEvents.Instance() != null) {
                NetworkGlobalEvents.Instance().OnSceneUnLoadFinished(sceneName);
            }
        }

        /// <summary>
        /// Handles the event when a client successfully connects.
        /// </summary>
        /// <param name="client">The client that connected.</param>
        private void ClientConnectionEvent(IClient client) {
            if ( NetworkGlobalEvents.Instance() != null ) {
                NetworkGlobalEvents.Instance().OnConnected(client);
            }
        }

        /// <summary>
        /// Handles the event when a player connects on a relay server.
        /// </summary>
        /// <param name="player">The player that connected on the relay server.</param>
        private void ClientConnectedOnRelayServerEvent(IPlayer player) {
            if ( NetworkGlobalEvents.Instance() != null ) {
                NetworkGlobalEvents.Instance().OnConnectedOnRelay((player as NetworkPlayer).GetClient());
            }
        }

        /// <summary>
        /// Handles the event when a client disconnects.
        /// </summary>
        /// <param name="client">The client that disconnected.</param>
        private void ClientDisconnectionEvent(IClient client) {
            if ((!this.InRelayMode()) || 
                (this.IsRunningLogic() && this.GetMasterPlayer<NetworkPlayer>().GetClient().Equals(client))) {
                if (NetworkManager.Instance().autoReconnect) {
                    if (!NetworkManager.Instance().disableReconnect) {
                        NetworkManager.Instance().StartNetwork(true, this.reconnectionDelay);
                    }
                    NetworkManager.Instance().disableReconnect = false;
                }
            }
            if ( this.onDisconnected != null ) {
                this.onDisconnected.Invoke(client);
            }
            if ( NetworkGlobalEvents.Instance() != null ) {
                NetworkGlobalEvents.Instance().OnDisconnected(client);
            }
        }

        /// <summary>
        /// Handles the event when a login attempt fails.
        /// </summary>
        /// <param name="exception">The exception that was thrown during the login attempt.</param>
        public void OnLoginFailedEvent(Exception exception) {            
            NetworkDebugger.LogError("Login attempt failed : {0}", exception.Message);
            // Call client event
            if ( NetworkGlobalEvents.Instance() != null ) {
                NetworkGlobalEvents.Instance().OnLoginFailed(exception);                    
            }
        }

        /// <summary>
        /// Handles the event when a login attempt is successful.
        /// </summary>
        /// <param name="client">The client that successfully logged in.</param>
        public void OnLoginSucessEvent(IClient client) {
            // Call client event
            if ( NetworkGlobalEvents.Instance() != null ) {
                NetworkGlobalEvents.Instance().OnLoginSucess(client);                    
            }
        }

        /// <summary>
        /// Invoked when a client login attempt fails.
        /// </summary>
        /// <param name="client">The client that failed to log in.</param>
        public void OnClientLoginFailedEvent(IClient client) {
            // Call client event
            if ( NetworkGlobalEvents.Instance() != null ) {
                NetworkGlobalEvents.Instance().OnClientLoginFailed(client);
            }
        }

        /// <summary>
        /// Invoked when a client login attempt is successful.
        /// </summary>
        /// <param name="client">The client that successfully logged in.</param>
        public void OnClientLoginSucessEvent(IClient client) {
            // Call client event
            if ( NetworkGlobalEvents.Instance() != null ) {
                NetworkGlobalEvents.Instance().OnClientLoginSucess(client);
            }
        }

        /// <summary>
        /// Invoked when a client successfully creates a lobby.
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby that was created.</param>
        public void OnClientLobbyCreationSucessEvent(ushort lobbyId) {
            // Call client event
            if ( NetworkGlobalEvents.Instance() != null ) {
                NetworkGlobalEvents.Instance().OnLobbyCreationSucess(lobbyId);
            }
        }

        /// <summary>
        /// Invoked when a client fails to create a lobby.
        /// </summary>
        /// <param name="reason">The reason for the lobby creation failure.</param>
        public void OnClientLobbyCreationFailedEvent(string reason) {
            // Call client event
            if ( NetworkGlobalEvents.Instance() != null ) {
                NetworkGlobalEvents.Instance().OnLobbyCreationFailed(reason);
            }
        }

        /// <summary>
        /// Invoked when a client successfully joins a lobby.
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby that was joined.</param>
        public void OnClientLobbyJoinSucessEvent(ushort lobbyId) {
            // Call client event
            if ( NetworkGlobalEvents.Instance() != null ) {
                NetworkGlobalEvents.Instance().OnLobbyJoinSuccess(lobbyId);
            }
        }

        /// <summary>
        /// Invoked when a client fails to join a lobby.
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby that the client attempted to join.</param>
        public void OnClientLobbyJoinFailedEvent(ushort lobbyId) {
            // Call client event
            if ( NetworkGlobalEvents.Instance() != null ) {
                NetworkGlobalEvents.Instance().OnLobbyJoinFailed(lobbyId);
            }
        }

        /// <summary>
        /// Called when list of players on lobby was received..
        /// </summary>
        /// <param name="players">Array with players names at lobby.</param>
        public void OnLobbyPlayersRefreshEvent(string[] players) {
            // Call client event
            if (NetworkGlobalEvents.Instance() != null) {
                NetworkGlobalEvents.Instance().OnLobbyPlayersRefresh(players);
            }
        }

        /// <summary>
        /// Invoked when a message is received from the network.
        /// </summary>
        /// <param name="reader">The data stream containing the message.</param>
        public void OnMessageReceivedEvent(IDataStream reader) {
            // Call client event
            if ( NetworkGlobalEvents.Instance() != null ) {
                NetworkGlobalEvents.Instance().OnMessageReceived(reader);
            }
        }

        /// <summary>
        /// Handles the event when player start his network services
        /// </summary>
        /// <param name="networkElement">Network element associated with this player.</param>
        public void OnPlayerStartedEvent(INetworkElement networkElement) {
            // Call client event
            if (NetworkGlobalEvents.Instance() != null) {
                if (networkElement.IsPlayer()) {
                    this.PlayerStartedEvent(networkElement);
                }
            }
        }

        /// <summary>
        /// Handles the event when client became owner of his player
        /// </summary>
        /// <param name="networkElement">Network element associated with this player.</param>
        public void OnPlayerOwnerDetected(INetworkElement networkElement) {
            // Call client event
            if (NetworkGlobalEvents.Instance() != null) {
                if (networkElement.IsPlayer()) {
                    this.PlayerOwnerDetected(networkElement);
                }
            }
        }
        
        /// <summary>
        /// Handles the event when client receive his connection id
        /// </summary>
        /// <param name="connectionId">Client connection id.</param>
        public void ConnectionIdAssignedEvent(int connectionId) {
            // Call client event
            if (NetworkGlobalEvents.Instance() != null) {
                this.ConnectionIdAssigned(connectionId);
            }
        }
        
        /// <summary>
        /// Event treigger when remote scene was loaded
        /// </summary>
        /// <param name="sceneName">Loaded scene name</param>
        public void OnRemoteSceneLoadFinishedEvent(string sceneName) {
            // Call client event
            if (NetworkGlobalEvents.Instance() != null) {
                this.RemoteSceneLoadFinishedEvent(sceneName);
            }
        }

        /// <summary>
        /// Event treigger when remote scene was unloaded
        /// </summary>
        /// <param name="sceneName">Loaded scene name</param>
        public void OnRemoteSceneUnLoadFinishedEvent(string sceneName) {
            // Call client event
            if (NetworkGlobalEvents.Instance() != null) {
                this.RemoteSceneUnLoadFinishedEvent(sceneName);
            }
        }

        /// <summary>
        /// Selects the server to connect to from a list of available servers.
        /// </summary>
        /// <returns>Returns the server address entry of the selected server.</returns>
        private ServerAddressEntry SelectServerToConnect() {
            if (NetworkConnectionType.Server.Equals(this.connectionType)) {
                if (this.usePublicAddress) {
                    this.SetServerAddress(NetworkManager.Instance().GetPublicIp()); // Update the PublicIp text with the public IP address
                } else if (this.useInternalAddress) {
                    this.SetServerAddress(NetworkManager.Instance().GetPrivateIp()); // Update the LocalIp text with the private IP address
                }
            }
            // Select server to connect ( select first as defualt and will check if need to select another later )
            NetworkServerData selectedServer = this.servers[0];
            if (this.servers.Count > MIN_SERVERS_TO_PING) {
                float pingTime = float.MaxValue;
                // Check witch server will connect
                foreach (NetworkServerData server in this.servers) {
                    if (server.IsExecutionPing()) {
                        double finishWait = ((DateTime.Now - DateTime.MinValue).TotalMilliseconds + WAIT_PING_TIME); // Wait for 1 second to finish each ping
                        while ((DateTime.Now - DateTime.MinValue).TotalMilliseconds < finishWait) {
                            if (server.IsExecutionPing()) {
                                Thread.Sleep(100);
                            } else {
                                break;
                            }
                        }
                    }
                    if ( !server.IsExecutionPing()) {
                        if (server.Online) {
                            if (server.Latency < pingTime) {
                                selectedServer = server;
                            }
                        }
                    }
                }
            }
            return selectedServer.GetServerInfo();
        }

        /// <summary>
        /// Sets the server address for the first server in the list.
        /// </summary>
        /// <param name="address">The new server address.</param>
        public void SetServerAddress(String address) {
            NetworkServerData   serverData  = this.servers[0];
            ServerAddressEntry  serverEntry = new ServerAddressEntry();
            serverEntry.Address   = address;
            serverData.SetServerInfo(serverEntry);
        }

        /// <summary>
        /// Sets the TCP port for the network connection.
        /// </summary>
        /// <param name="tcpPort">The TCP port number.</param>
        public void SetTcpPort(int tcpPort) {
            this.tcpConnectionPort = tcpPort;
        }

        /// <summary>
        /// Sets the UDP port for the network connection.
        /// </summary>
        /// <param name="udpPort">The UDP port number.</param>
        public void SetUdpPort(int tcpPort) { 
            this.udpConnectionPort = tcpPort;
        }

        /// <summary>
        /// Sets the port for P2P the network connection.
        /// </summary>
        /// <param name="peerToPeerPort">The P2P port number.</param>
        public void SetPeerToPeerPort(ushort peerToPeerPort) {
            this.peerToPeerPort = peerToPeerPort;
        }

        /// <summary>
        ///Return P2P port number
        /// </summary>
        /// <param name="udpPort">The P2P port number.</param>
        public int GetPeerToPeerPort() {
            return (ushort)this.peerToPeerPort;
        }

        /// <summary>
        /// Return P2P port number based peer to pper port with certain offset
        /// </summary>
        /// <param name="offset">The offset to be used in case of dynamic p2p ports.</param>
        /// <returns>Port to be used on peer to peer connection</returns>
        public ushort GeneratePeerToPeerPort(ushort offset = 0) {
            return (ushort)((this.peerToPeerPortPeerPlayer) ? (this.peerToPeerPort + offset) : this.peerToPeerPort);
        }

        /// <summary>
        /// Rsset the connection mode to his default status
        /// </summary>
        public void ResetConnectionMode() {
            this.connectionType = NetworkConnectionType.Manual;
            this.serverSocket.SetAutoConnect(false);
            this.serverSocket.gameObject.SetActive(false);
            this.clientSocket.SetAutoConnect(false);
            this.clientSocket.gameObject.SetActive(false);
        }

        /// <summary>
        /// Configures the network mode of operation (Server, Client, or Manual).
        /// </summary>
        /// <param name="mode">The network connection type to configure.</param>
        public void ConfigureMode(NetworkConnectionType mode) {
            if (NetworkConnectionType.Server.Equals(mode)) {
                this.connectionType = mode;
                this.ConfigureAsServer();
            } else if (NetworkConnectionType.Client.Equals(mode)) {
                this.connectionType = mode;
                this.ConfigureAsClient();
            } else if (NetworkConnectionType.Manual.Equals(mode)) {
                this.connectionType = mode;
                if (this.serverSocket != null) {
                    this.serverSocket.ConfigureSpawners(this.SpawnServerPlayer, this.SpawnClientPlayerOnServer);
                    this.serverSocket.SetAutoConnect(false);
                    this.serverSocket.gameObject.SetActive(true);
                }
                if (this.clientSocket != null) {
                    this.clientSocket.ConfigureSpawners(this.SpawnServerPlayer, this.SpawnClientPlayerOnServer);
                    this.clientSocket.SetAutoConnect(false);
                    this.clientSocket.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Configures the application as a server.
        /// </summary>
        private void ConfigureAsServer() {
            if (this.serverSocket != null) {
                if ( this.clientSocket != null ) {
                    this.clientSocket.ConfigureSpawners(this.SpawnServerPlayer, this.SpawnClientPlayerOnServer);
                    this.clientSocket.SetAutoConnect(false);
                    this.clientSocket.gameObject.SetActive(false);
                }
                this.serverSocket.ConfigureSpawners(this.SpawnServerPlayer, this.SpawnClientPlayerOnServer);
                this.serverSocket.SetAutoConnect(false);
                this.serverSocket.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Configures the application as a client.
        /// </summary>
        private void ConfigureAsClient() {
            if (this.clientSocket != null) {
                if ( this.serverSocket != null ) {
                    this.serverSocket.ConfigureSpawners(this.SpawnServerPlayer, this.SpawnClientPlayerOnServer);
                    this.serverSocket.SetAutoConnect(false);
                    this.serverSocket.gameObject.SetActive(false);
                }
                this.clientSocket.ConfigureSpawners(this.SpawnServerPlayer, this.SpawnClientPlayerOnServer);
                this.clientSocket.SetAutoConnect(false);
                this.clientSocket.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Renews the server instance by clearing out old network objects and resetting the instance ID.
        /// </summary>
        /// <param name="instanceId">The new instance ID to set for the server.</param>
        public void RenewServerInstance(String instanceId) {
            // First i'm going to remove all NetworkObjects
            INetworkElement[] networkObjects = NetworkManager.Container.GetElements();
            foreach (INetworkElement networkObject in networkObjects) {
                if ((networkObject.GetNetworkSessionId() == null) ||
                    (!networkObject.GetNetworkSessionId().Equals(instanceId))) {
                    NetworkManager.Container.UnRegister(networkObject);
                    GameObject.Destroy(networkObject.GetGameObject());
                }
            }
            // Destroy garbage
            List<string> sessionsToRemove = new List<string>();
            foreach(var garbageSession in this.garbageNetworkObjects) {
                if (!garbageSession.Key.Equals(instanceId)) {
                    while (garbageSession.Value.Count > 0) {
                        GameObject garbage = garbageSession.Value[0];
                        garbageSession.Value.RemoveAt(0);
                        GameObject.Destroy(garbage);
                    }
                }
            }
            // Remove finished instances
            foreach ( var garbageSession in this.garbageNetworkObjects.Where(g => g.Key != instanceId).ToList() ) {
                this.garbageNetworkObjects.Remove(garbageSession.Key);
            }
            // Remove elements 
            INetworkElement[] currentElements = this.objectsContainer.GetElements();
            foreach(INetworkElement element in currentElements) {
                this.objectsContainer.UnRegister(element);
                Destroy(element.GetGameObject());
            }
            // Then i'm going to renew instance ID
            this.networkSessionId = instanceId;
        }

        /// <summary>
        /// Sends a request to create a new lobby with the specified name.
        /// </summary>
        /// <param name="lobbyName">The name of the lobby to create.</param>
        /// <exception cref="Exception">Thrown when the lobby name is invalid.</exception>
        public void RequestLobbyCreate(String lobbyName) {
            if (!string.IsNullOrEmpty(lobbyName)) {
                if ((lobbyName.Length >= NetworkLobbyManager.MIN_LOBBY_LENGHT_NAME) &&
                    (lobbyName.Length <= NetworkLobbyManager.MAX_LOBBY_LENGHT_NAME)) {
                    using (DataStream writer = new DataStream()) {
                        writer.Write(lobbyName); // Lobby name
                        this.GetConnection(ConnectionType.Client).Send(LobbyServerEvents.LobbyCreateRequest, writer, DeliveryMode.Reliable); // Send message
                    }
                } else {
                    throw new Exception(String.Format("Lobby name must have at least {0} characters and less than", NetworkLobbyManager.MIN_LOBBY_LENGHT_NAME, NetworkLobbyManager.MAX_LOBBY_LENGHT_NAME));
                }
            } else {
                throw new Exception("A name must be provided to create a new lobby");
            }
        }

        /// <summary>
        /// Sends a request to refresh the list of available lobbies.
        /// </summary>
        public void RequestLobbyRefresh() {
            using (DataStream writer = new DataStream()) {
                this.GetConnection(ConnectionType.Client).Send(LobbyServerEvents.LobbyListRequest, writer, DeliveryMode.Reliable); // Send message
            }
        }

        /// <summary>
        /// Sends a request to join a lobby with the specified ID.
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby to join.</param>
        public void RequestLobbyJoin(ushort lobbyId) {
            using (DataStream writer = new DataStream()) {
                writer.Write(lobbyId); // Lobby id
                this.GetConnection(ConnectionType.Client).Send(LobbyServerEvents.LobbyJoinRequest, writer, DeliveryMode.Reliable); // Send message
            }
        }

        /// <summary>
        /// Sets the target database for the server instance.
        /// </summary>
        /// <param name="database">The name of the target database.</param>
        public void SetTargetDatabase(string database) {
            this.databaseTarget = database;
        }

        /// <summary>
        /// Initializes the server instance with the specified instance ID.
        /// </summary>
        /// <param name="instanceId">The instance ID to set for the server.</param>
        public void InitializeServerInstance(String instanceId) {
            this.networkSessionId = instanceId;
        }

        /// <summary>
        /// Checks if there is a connection associated with the specified network transport.
        /// </summary>
        /// <param name="connection">The network transport to check.</param>
        /// <returns>True if a connection exists, otherwise false.</returns>
        public bool HasConnection(NetworkTransport connection) {
            return this.connections.TryGetValue(connection.GetConnectionType(), out var outputValue);
        }

        /// <summary>
        /// Checks if there is a connection of the specified type.
        /// </summary>
        /// <param name="connectionType">The type of connection to check for.</param>
        /// <returns>True if a connection of the specified type exists, otherwise false.</returns>
        public override bool HasConnection(ConnectionType connectionType) {
            return this.connections.TryGetValue(connectionType, out var outputValue);
        }

        /// <summary>
        /// Retrieves the network transport associated with the specified connection type.
        /// </summary>
        /// <param name="connectionType">The type of connection to retrieve.</param>
        /// <returns>The network transport associated with the specified connection type.</returns>
        public override NetworkTransport GetConnection(ConnectionType connectionType) {
            return this.connections[connectionType];
        }

        /// <summary>
        /// Retrieves the network transport associated with the specified connection type.
        /// </summary>
        /// <returns>The network transport associated with the specified connection type.</returns>
        public override bool HasConnection() {
            ConnectionType connectionType = (this.IsServerConnection()) ? ConnectionType.Server : ConnectionType.Client;
            return this.connections.TryGetValue(connectionType, out var outputValue);
        }

        /// <summary>
        /// Retrieves the network transport associated with the specified connection type.
        /// </summary>
        /// <returns>The network transport associated with the specified connection type.</returns>
        public override NetworkTransport GetConnection() {
            return this.connections[(this.IsServerConnection()) ? ConnectionType.Server : ConnectionType.Client];
        }

        /// <summary>
        /// Gets the connection type of the current instance.
        /// </summary>
        /// <returns>The connection type.</returns>
        public NetworkConnectionType GetConnectionType() {
            return this.connectionType;
        }

        /// <summary>
        /// Determines if the current connection is a server connection.
        /// </summary>
        /// <returns>True if the connection type is Server, otherwise false.</returns>
        public bool IsServerConnection() {
            return (NetworkConnectionType.Server == this.connectionType);
        }

        /// <summary>
        /// Determines if the current connection is a client connection.
        /// </summary>
        /// <returns>True if the connection type is Client, otherwise false.</returns>
        public bool IsClientConnection() {
            return (NetworkConnectionType.Client == this.connectionType);
        }

        /// <summary>
        /// Checks if the client is currently connected to the server.
        /// </summary>
        /// <returns>True if connected, otherwise false.</returns>
        public bool IsConnected() {
            return this.HasConnection(ConnectionType.Client) && 
                   this.GetConnection(ConnectionType.Client).GetSocket().IsConnected();
        }

        /// <summary>
        /// Checks if the server is currently started
        /// </summary>
        /// <returns>True if connected, otherwise false.</returns>
        public bool IsServerStarted() {
            return this.HasConnection(ConnectionType.Server) &&
                   this.GetConnection(ConnectionType.Server).GetSocket().IsStarted();
        }

        /// <summary>
        /// Checks if Network is avaiable
        /// </summary>
        /// <returns>True if connected, otherwise false.</returns>
        public bool IsMultiplayerAvaiable() {
            return ((this.IsServerConnection() && this.IsServerStarted()) ||
                    (this.IsClientConnection() && this.IsConnected()));
        }

        /// <summary>
        /// Detects if a disconnection has occurred on the client side.
        /// </summary>
        /// <returns>True if a disconnection is detected, otherwise false.</returns>
        public bool IsDisconnectionDetected() {
            return this.GetConnection(ConnectionType.Client).GetSocket().IsConnectionLost();
        }

        /// <summary>
        /// Gets the send rate of the connection.
        /// </summary>
        /// <returns>The send rate amount.</returns>
        /// // Note
        public override int GetSendRate(bool normalizeTimeScale = true) {
            return (normalizeTimeScale) ? Mathf.CeilToInt(this.sendRateAmount / ((Time.timeScale > 0.0) ? Time.timeScale : 1.0f)) : this.sendRateAmount;
        }

        /// <summary>
        /// Gets the send rate of the connection.
        /// </summary>
        /// <returns>The send rate amount.</returns>
        /// // Note
        public override int GetMinimumSendRate(bool normalizeTimeScale = true) {
            return ((normalizeTimeScale == true) && (this.minimumRateAmount > 0)) ? Mathf.CeilToInt(this.minimumRateAmount / ((Time.timeScale > 0.0) ? Time.timeScale : 1.0f)) : this.minimumRateAmount;
        }

        /// <summary>
        /// Updates the remote tick count if the difference between the current remote tick count and the provided tick exceeds the tolerance.
        /// </summary>
        /// <param name="tick">The new tick count to update to.</param>
        public void UpdateRemoteTick(int tick) {            
            if ((Mathf.Abs(this.RemoteTicks - tick) > this.tickTolerance) || (this.IsRemoteTicksInitialized == false)) {
                this.IsRemoteTicksInitialized   = true;
                this.RemoteTicks                = tick;
                NetworkClock.Tick               = tick;
            }
        }

        /// <summary>
        /// Retrieves the current networking mode.
        /// </summary>
        /// <returns>The connection type of the current network connection.</returns>
        public override NetworkConnectionType GetNetworkingMode() {
            return this.connectionType;
        }

        /// <summary>
        /// Registers a new network connection and configures its latency measurement callback if it's not already registered.
        /// </summary>
        /// <param name="connection">The network transport to register.</param>
        public void RegisterConnection(NetworkTransport connection) {
            if (!this.connections.ContainsKey(connection.GetConnectionType())) {
                // If has a previous connection i need to release any registered connection
                if ( this.connections.Count > 0 ) {
                    ConnectionType[] connectionTypes = this.connections.Keys.ToArray();
                    NetworkTransport previousConnection = this.connections[connectionTypes[0]];
                    this.connections.Remove(connectionTypes[0]);
                    previousConnection.SetAutoConnect(false);
                    if (previousConnection.IsConnected()) {
                        previousConnection.Disconnect();
                    }
                    previousConnection.gameObject.SetActive(false);
                }
                this.connections.Add(connection.GetConnectionType(), connection);
                // Configure ping callback
                connection.SetLatencyMeasure(this.useInternalPing);
            }
        }

        /// <summary>
        /// Unregisters an existing network connection if it is currently registered.
        /// </summary>
        /// <param name="connection">The network transport to unregister.</param>
        public void UnregisterConnection(NetworkTransport connection) {
            if (this.connections.ContainsKey(connection.GetConnectionType())) {
                this.connections.Remove(connection.GetConnectionType());
            }
        }

        /// <summary>
        /// Clear spawned objects to not resend in case of this player became the master player 
        /// </summary>
        private void InvalidateSpawnedObjects() {
            while (this.detectedNetworkObjects.Count > 0) {
                this.detectedNetworkObjects.RemoveAt(0); // Then remove to not try to execute twice on same object
            }
            while (this.destroyedNetworkObjects.Count > 0) {
                this.destroyedNetworkObjects.RemoveAt(0); // Then remove to not try to execute twice on same object
            }           
        }

        /// <summary>
        /// Detects and processes newly spawned network objects.
        /// </summary>
        private void DetectSpawnedObjects() {
            while (this.detectedNetworkObjects.Count > 0) {
                bool inSceneObject      = false; // DO not spawn objct on lcients in case of this object is already in scene
                GameObject newObject    = this.detectedNetworkObjects[0]; // Get first object to check
                this.detectedNetworkObjects.RemoveAt(0); // Then remove to not try to execute twice on same object
                if (NetworkManager.Container.IsRegistered(newObject) == false) {
                    if (this.prefabsDatabase != null) {
                        foreach (NetworkPrefabEntry networkPrefab in this.prefabsDatabase.GetPrefabs()) {
                            if (newObject.IsSamePrefab(networkPrefab.GetPrefab())) {
                                // Auto Sync need to gas a NetworkObject script
                                int     networkObjectId = NetworkObject.DEFAULT_NETWORK_ID;
                                ushort  playerId        = NetworkObject.DEFAULT_NETWORK_ID;
                                ushort  playerIndex     = NetworkObject.DEFAULT_NETWORK_TAG;
                                bool    isNetworkPlayer = false;
                                NetworkMasterPlayer     masterPlayerReference   = newObject.GetComponent<NetworkMasterPlayer>();
                                NetworkObject           networkObject           = null;
                                NetworkPlayerReference  playerReference         = newObject.GetComponent<NetworkPlayerReference>();
                                if (networkPrefab.GetAutoSync()) {
                                    int networkId = NetworkObject.DEFAULT_NETWORK_ID;
                                    networkObject = newObject.GetComponent<NetworkObject>();
                                    if (networkObject == null) {
                                        networkObject = newObject.AddComponent<NetworkObject>();
                                        // If this object is "InScene" object i need to set his NetworkId
                                        NetworkInstantiateDetection instantiateDetection = newObject.GetComponent<NetworkInstantiateDetection>();
                                        if (instantiateDetection != null) {
                                            if (instantiateDetection.IsStaticSpawn()) {
                                                networkId     = instantiateDetection.GetStaticId();
                                                inSceneObject = true;
                                            }
                                        }
                                    } else if ((networkObject.HasNetworkElement()) &&
                                               (NetworkManager.Container.IsRegistered(networkObject.GetNetworkElement()) == true)) {
                                        break;
                                    } else {
                                        // If this object is "InScene" object i need to set his NetworkId
                                        NetworkInstantiateDetection instantiateDetection = newObject.GetComponent<NetworkInstantiateDetection>();
                                        if (instantiateDetection != null) {
                                            if (instantiateDetection.IsStaticSpawn()) {
                                                networkId = instantiateDetection.GetStaticId();
                                                inSceneObject = true;
                                            }
                                        }
                                    }
                                    // Flag is this object is network player ( because maybe was an ordinary network object )
                                    isNetworkPlayer     = ((networkObject.gameObject.GetComponent<NetworkPlayerTag>()         != null) ||
                                                           (networkObject.gameObject.GetComponent<NetworkPlayerReference>()   != null));
                                    if (isNetworkPlayer) {
                                        NetworkPlayerTag playerTag = networkObject.gameObject.GetComponent<NetworkPlayerTag>();
                                        if (playerTag != null) {
                                            if (this.IsToUseCustomNetworkId()) {
                                                if (NetworkObject.DEFAULT_NETWORK_ID.Equals(networkId) == true) {
                                                    networkId = this.GetCustomNetworkId(playerTag.GetAttributesValues());
                                                }
                                            }
                                            playerIndex = playerTag.GetPlayerIndex();
                                        }
                                    }
                                    // Add script references on spawned element
                                    this.InitializeNetworkScriptsReferences(newObject, networkPrefab);
                                    // Now set all networkObject values
                                    networkObject.SetTransport(this.GetConnection((this.IsConnectedOnRelayServer()) ? ConnectionType.Client : ConnectionType.Server));
                                    networkObject.SetClient(networkObject.GetTransport().GetSocket().GetLocalClient()); // When is a server socket i'm going to set a "fake" client to ensure that GetClient will return some value
                                    networkObject.SetDeliveryMode(this.sendUpdateMode);
                                    networkObject.SetRemoteControlsEnabled(this.useRemoteInput);
                                    networkObject.SetBehaviorMode(BehaviorMode.Active, false);
                                    networkObject.SetSyncParticles(networkPrefab.GetParticlesAutoSync());
                                    networkObject.SetSyncAnimation(networkPrefab.GetAnimationAutoSync());
                                    networkObject.SetSyncAnimationMode(networkPrefab.GetAnimationSyncMode());
                                    networkObject.SetAnimationLayerCount(networkPrefab.GetAnimationCount());
                                    networkObject.SetAnimationDefaultStatus(networkPrefab.GetAnimationDefaultStatus());
                                    networkObject.SetOwnershipAccessLevel(networkPrefab.GetOwnerShipAccessLevel());
                                    networkObject.SetMovementType(networkPrefab.GetMovementType());
                                    networkObject.SetDisableGravity(networkPrefab.IsToDisableGravity());
                                    networkObject.SetEnableKinematic(networkPrefab.IsToEnableKinematic());
                                    networkObject.SetEnablePhysicsInterpolation(networkPrefab.IsToEnableInterpolate());
                                    networkObject.SetToUsePrefabRotation(networkPrefab.IsToUsePrefabRotation());
                                    networkObject.SetSendRate((networkPrefab.IsToUseCustomRate()) ? networkPrefab.GetSendRateAmount() : this.sendRateAmount);
                                    networkObject.SetIsPlayer(isNetworkPlayer);
                                    // Configure ownership behaviors
                                    networkObject.OnSpawnPrefab((networkPrefab.GetOnSpawnPrefab() != null) ? networkPrefab.GetOnSpawnPrefab().ToEventReference() : null);
                                    networkObject.OnDespawnPrefab((networkPrefab.GetOnDespawnPrefab() != null) ? networkPrefab.GetOnDespawnPrefab().ToEventReference() : null);
                                    networkObject.OnAcceptOwnerShip((networkPrefab.GetOnAcceptObjectOwnerShip() != null) ? networkPrefab.GetOnAcceptObjectOwnerShip().ToEventReference() : null);
                                    networkObject.OnAcceptReleaseOwnerTakeShip((networkPrefab.GetOnAcceptReleaseObjectOwnerShip() != null) ? networkPrefab.GetOnAcceptReleaseObjectOwnerShip().ToEventReference() : null);
                                    networkObject.OnTakeObjectOwnerShip((networkPrefab.GetOnTakeObjectOwnership() != null) ? networkPrefab.GetOnTakeObjectOwnership().ToEventReference() : null);
                                    networkObject.OnReleaseObjectOwnerShip((networkPrefab.GetOnReleaseObjectOwnership() != null) ? networkPrefab.GetOnReleaseObjectOwnership().ToEventReference() : null);
                                    // Tranform update values
                                    networkObject.SetSyncPosition(networkPrefab.GetSyncPosition());
                                    networkObject.SetSyncRotation(networkPrefab.GetSyncRotation());
                                    networkObject.SetSyncScale(networkPrefab.GetSyncScale());                                    
                                    // Start network services
                                    networkObject.StartNetwork(networkId, (INetworkElement element) => {
                                        // Configure player ID
                                        NetworkPlayerReference playerReference = newObject.GetComponent<NetworkPlayerReference>();
                                        if (playerReference != null) {
                                            networkObject.GetNetworkElement().SetPlayerId(playerReference.GetPlayerId());
                                            playerId = networkObject.GetNetworkElement().GetPlayerId();
                                        }
                                        NetworkPlayerTag playerTag = networkObject.gameObject.GetComponent<NetworkPlayerTag>();
                                        if (playerTag != null) {
                                            networkObject.GetNetworkElement().SetPlayerIndex(playerTag.GetPlayerIndex()); // Update player index
                                        }
                                        return element;
                                    });
                                    // After start object is already identified
                                    networkObject.SetIdentified(true);
                                    // Now i'm going to collect the real network ID
                                    networkObjectId = networkObject.GetNetworkId();                                    
                                }
                                
                                // Detect spawned players when connected as embedded or authoritative
                                if (inSceneObject == false) { 
                                    if ((this.InEmbeddedMode() || this.InAuthoritativeMode()) && (!this.IsConnectedOnRelayServer())) {
                                        NetworkTransport transport = null;
                                        if (this.HasConnection(ConnectionType.Server)) {
                                            transport = this.GetConnection(ConnectionType.Server);
                                        } else if (this.HasConnection(ConnectionType.Client)) { 
                                            transport = this.GetConnection(ConnectionType.Client);
                                        }
                                        foreach (NetworkClient clientToSend in transport.GetSocket().GetConnectedClients()) {
                                            // if is sending player object to client he will ask for a feedback about creation
                                            bool sendFeedback = ((playerReference               != null) && 
                                                                 (playerReference.GetClient()   != null) && 
                                                                 (playerReference.GetClient().Equals(clientToSend)));
                                            // Create package to send
                                            using (DataStream writer = new DataStream()) {
                                                writer.Write(networkObjectId); // Send network id if appliable ( autosync is true )
                                                writer.Write(networkPrefab.GetId());
                                                writer.Write(networkPrefab.GetAutoSync());
                                                writer.Write(sendFeedback); // If is an local player instance shall send ack feedback when receive this message.
                                                writer.Write(isNetworkPlayer); // Is this object a network player ?
                                                writer.Write(playerId); // Player ID
                                                writer.Write(playerIndex); // player index
                                                writer.Write(newObject.transform.position); // Position
                                                writer.Write(newObject.transform.eulerAngles); // Rotation
                                                writer.Write(newObject.transform.localScale); // Scale
                                                // Send message
                                                clientToSend.Send(CoreGameEvents.ObjectInstantiate, writer, DeliveryMode.Reliable);
                                            }
                                        }
                                    } else {
                                        NetworkPlayer localPlayer   = this.GetLocalPlayer<NetworkPlayer>();
                                        NetworkPlayer masterPlayer  = this.GetMasterPlayer<NetworkPlayer>(localPlayer.GetLobbyId());
                                        if (( this.IsConnectedOnRelayServer() ) && (this.IsMasterPlayer())) {
                                            NetworkScriptsReference scriptReference = newObject.GetComponent<NetworkScriptsReference>();
                                            if ( scriptReference != null ) {
                                                if (scriptReference.GetPlayer() != null) {
                                                    localPlayer = (scriptReference.GetPlayer() as NetworkPlayer);
                                                }
                                            }
                                        }
                                        // Update player index
                                        networkObject.GetNetworkElement().SetPlayerIndex(playerIndex);

                                        // Create package to send
                                        foreach (NetworkClient clientToSend in this.GetConnection(ConnectionType.Client).GetSocket().GetConnectedClients()) {
                                            // if is sending player object to client he will ask for a feedback about creation
                                            bool sendFeedback = ((playerReference != null) && (localPlayer.GetClient().Equals(clientToSend)));
                                            if (masterPlayer.GetClient() != clientToSend) {
                                                // Create package to send
                                                using (DataStream writer = new DataStream()) {
                                                    writer.Write(networkObjectId); // Send network id if appliable ( autosync is true )
                                                    writer.Write(networkPrefab.GetId());
                                                    writer.Write(networkPrefab.GetAutoSync());
                                                    writer.Write(sendFeedback); // If is an local player instance shall send ack feedback when receive this message.
                                                    writer.Write(isNetworkPlayer); // Is this object a network player ?
                                                    writer.Write(playerId); // Player ID
                                                    writer.Write(playerIndex); // player index
                                                    writer.Write(newObject.transform.position); // Position
                                                    writer.Write(newObject.transform.eulerAngles); // Rotation
                                                    writer.Write(newObject.transform.localScale); // Scale
                                                    // Send message
                                                    clientToSend.Send(CoreGameEvents.ObjectInstantiate, writer, DeliveryMode.Reliable);
                                                }
                                            }
                                            // If is player instance i need to tell to relay server the network object id
                                            // This id will be used to destroy object when player disconnect from server
                                            if (( sendFeedback ) || ( localPlayer.IsMaster() )) {
                                                // Do not send to ordinary objects to not override correclty network player id
                                                if (isNetworkPlayer == true) {
                                                    // Create package to send
                                                    using (DataStream writer = new DataStream()) {
                                                        writer.Write((clientToSend as NetworkClient).GetConnectionId()); // Send connection id of disconnected client
                                                        writer.Write(networkObjectId);
                                                        clientToSend.Send(RelayServerEvents.UpdateNetworkObjectId, writer, DeliveryMode.Reliable);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (networkPrefab.GetAutoSync()) {
                                    // The last step will be advice client if i need to send the ownership to this client
                                    NetworkObjectReference objectReferenceScript = null;
                                    if (NetworkManager.Instance().IsOwnerShipUsingPrefab()) {
                                        if (OwnerShipAccessLevel.ClientOnly.Equals(networkObject.GetOwnershipAccessLevel())) {
                                            objectReferenceScript = networkObject.GetComponent<NetworkObjectReference>();
                                            if (objectReferenceScript != null) {
                                                networkObject.TransferControlToClient(objectReferenceScript.GetClient());
                                            } else {
                                                NetworkPlayer localPlayer   = this.GetLocalPlayer<NetworkPlayer>();
                                                NetworkPlayer masterPlayer  = this.GetMasterPlayer<NetworkPlayer>((localPlayer != null) ? localPlayer.GetLobbyId() : (ushort)0);
                                                NetworkTransport transport  = null;
                                                if (this.HasConnection(ConnectionType.Server)) {
                                                    transport = this.GetConnection(ConnectionType.Server);
                                                } else if (this.HasConnection(ConnectionType.Client)) {
                                                    transport = this.GetConnection(ConnectionType.Client);
                                                }
                                                foreach (NetworkClient clientToSend in transport.GetSocket().GetConnectedClients()) {
                                                    // tell to client's that ownership of this object was already defined
                                                    if (masterPlayer.GetClient() != clientToSend) {
                                                        using (DataStream writer = new DataStream()) {
                                                            writer.Write(networkObject.GetNetworkId());
                                                            // Send back to the client
                                                            clientToSend.Send(UserCustomObjectEvents.OwnerShipDefined, writer, DeliveryMode.Reliable);
                                                        }
                                                    }
                                                }
                                                // Flag as transferred
                                                networkObject.SetTranferReceived(true);
                                                // Force to update behaviour mode
                                                networkObject.SetBehaviorMode(networkObject.GetBehaviorMode());                                                
                                            }
                                        }
                                    }
                                    if (objectReferenceScript == null) {
                                        objectReferenceScript = networkObject.GetComponent<NetworkObjectReference>();
                                    }
                                    if (objectReferenceScript != null) {
                                        if (objectReferenceScript.GetTransactionId() > 0) {
                                            // Create package to send
                                            using (DataStream writer = new DataStream()) {
                                                writer.Write(objectReferenceScript.GetTransactionId()); // Transaction ID
                                                writer.Write(networkObjectId); // Object ID
                                                objectReferenceScript.GetClient().Send(CoreGameEvents.NetworkSpawnResponse, writer, DeliveryMode.Reliable);
                                            }
                                        }
                                    }
                                    // Initialize internal executor
                                    if ((isNetworkPlayer == false) || (networkObject.IsOwner())) {
                                        networkObject.InitializeExecutor();
                                    }
                                }
                                break;
                            }
                        }
                    }
                } else {
                    NetworkPrefabEntry networkPrefab = null;
                    foreach (NetworkPrefabEntry databasePrefab in this.prefabsDatabase.GetPrefabs()) {
                        if (newObject.IsSamePrefab(databasePrefab.GetPrefab())) {
                            networkPrefab = databasePrefab;
                            break;
                        }
                    }
                    // If this object is "InScene" object i need to set his NetworkId
                    NetworkInstantiateDetection instantiateDetection = newObject.GetComponent<NetworkInstantiateDetection>();
                    if (instantiateDetection != null) {
                        if (instantiateDetection.IsStaticSpawn()) {
                            inSceneObject = true;
                        }
                    }
                    // Add script references on spawned element
                    this.InitializeNetworkScriptsReferences(newObject, networkPrefab);

                    if (inSceneObject == false) { 
                        // First o need to get all references
                        NetworkPlayerReference  playerReference = newObject.GetComponent<NetworkPlayerReference>();
                        NetworkPlayerTag        playerTag       = newObject.GetComponent<NetworkPlayerTag>();
                        NetworkObject           networkObject   = newObject.GetComponent<NetworkObject>();
                        // Auto Sync need to gas a NetworkObject script
                        int     networkObjectId = networkObject.GetNetworkId();
                        ushort  playerId        = (playerReference  != null) ? playerReference.GetPlayerId() : (ushort)0;
                        ushort  playerIndex     = (playerTag        != null) ? playerTag.GetPlayerIndex()    : (ushort)0;
                        // Flag is this object is network player ( because maybe was an ordinary network object )
                        bool    isNetworkPlayer = ((networkObject.gameObject.GetComponent<NetworkPlayerTag>()         != null) ||
                                                   (networkObject.gameObject.GetComponent<NetworkPlayerReference>()   != null));

                        // Update player index
                        networkObject.GetNetworkElement().SetPlayerIndex(playerIndex);

                        // Cancel the schedule despawn
                        this.CancelDelayedDespawn(networkObject.GetNetworkElement());
                        // Detect spawned players when connected as embedded or authoritativa
                        if (this.InEmbeddedMode() || this.InAuthoritativeMode()) {
                            // Create package to send
                            using (DataStream writer = new DataStream()) {
                                writer.Write(networkObjectId); // Send network id if appliable ( autosync is true )
                                writer.Write(networkPrefab.GetId());
                                writer.Write(networkPrefab.GetAutoSync());
                                writer.Write(true); // If is an local player instance shall send ack feedback when receive this message.
                                writer.Write(isNetworkPlayer); // Is this object a network player ?
                                writer.Write(playerId); // Player ID
                                writer.Write(playerIndex); // player index
                                writer.Write(newObject.transform.position); // Position
                                writer.Write(newObject.transform.eulerAngles); // Rotation
                                writer.Write(newObject.transform.localScale); // Scale
                                // Send message
                                playerReference.GetClient().Send(CoreGameEvents.ObjectInstantiate, writer, DeliveryMode.Reliable);
                            } 
                        } else {
                            // Create package to send
                            using (DataStream writer = new DataStream()) {
                                writer.Write(networkObjectId); // Send network id if appliable ( autosync is true )
                                writer.Write(networkPrefab.GetId());
                                writer.Write(networkPrefab.GetAutoSync());
                                writer.Write(true); // If is an local player instance shall send ack feedback when receive this message.
                                writer.Write(isNetworkPlayer); // Is this object a network player ?
                                writer.Write(playerId); // Player ID
                                writer.Write(playerIndex); // player index
                                writer.Write(newObject.transform.position); // Position
                                writer.Write(newObject.transform.eulerAngles); // Rotation
                                writer.Write(newObject.transform.localScale); // Scale
                                // Send message
                                playerReference.GetClient().Send(CoreGameEvents.ObjectInstantiate, writer, DeliveryMode.Reliable);
                            }
                            // Do not send to ordinary objects to not override correclty network player id
                            if (isNetworkPlayer == true) {
                                // Create package to send
                                using (DataStream writer = new DataStream()) {
                                    writer.Write((playerReference.GetClient() as NetworkClient).GetConnectionId()); // Send connection id of disconnected client
                                    writer.Write(networkObjectId);
                                    playerReference.GetClient().Send(RelayServerEvents.UpdateNetworkObjectId, writer, DeliveryMode.Reliable);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialize all script and network variables references
        /// </summary>
        /// <param name="newObject">New created object</param>
        private void InitializeNetworkScriptsReferences(GameObject newObject, NetworkPrefabEntry networkPrefab) {
            NetworkScriptsReference scriptsReference = newObject.GetComponent<NetworkScriptsReference>();
            if (scriptsReference == null) {
                // Since developers can flag not disable some script i need to check those scripts to tell to "NetworkScriptsReference" to ignore those scripts
                ScriptList scriptList = networkPrefab.GetPrefabScripts();
                ScriptList scriptListOnRemoteInput = networkPrefab.GetInputScripts();

                List<MonoBehaviour> ignoredListFromPrefab = new List<MonoBehaviour>();
                List<Tuple<MonoBehaviour, float>> delayedListFromPrefab = new List<Tuple<MonoBehaviour, float>>();

                List<MonoBehaviour> ignoredListFromPrefabOnRemoteInput = new List<MonoBehaviour>();
                if (scriptList != null) {
                    // Ignored scripts
                    foreach (ScriptStatus script in scriptList.GetScripts()) {
                        if (script.Script != null) {
                            if (script.Enabled) {
                                ignoredListFromPrefab.Add(script.Script);
                            } else if (script.Delay > 0f) {
                                delayedListFromPrefab.Add(new Tuple<MonoBehaviour, float>(script.Script, (script.Delay / MILLISECONDS_MULTIPLIER)));
                            }
                        }
                    }
                    // Ignored scripts when remote input is enabled
                    foreach (ScriptStatus script in scriptListOnRemoteInput.GetScripts()) {
                        if (!script.Enabled) {
                            ignoredListFromPrefabOnRemoteInput.Add(script.Script);
                        }
                    }
                }
                // Add script on spawned element
                scriptsReference = newObject.AddComponent<NetworkScriptsReference>();
                scriptsReference.Collect(ignoredListFromPrefab, delayedListFromPrefab, ignoredListFromPrefabOnRemoteInput);
                // Now initialize all network variables
                scriptsReference.CollectAll();
                foreach (var scriptStatus in scriptList.GetScripts()) {
                    if (scriptStatus.Script != null) {
                        scriptsReference.InitalizeNetworkVariables(scriptStatus.Script, scriptStatus.GetSynchronizedVariables());
                    }
                }
            }
        }

        /// <summary>
        /// Initializes spawned objects for the client, excluding a specific network element if provided.
        /// </summary>
        /// <param name="client">The client to initialize spawned objects for.</param>
        /// <param name="ignoreElement">The network element to ignore during initialization.</param>
        public void InitializeSpawnedObjects(IClient client, INetworkElement ignoreElement = null) {
            foreach (INetworkElement currentObj in NetworkManager.Container.GetElements()) {
                if ((ignoreElement == null) ||
                    (!ignoreElement.Equals(currentObj))) {
                    bool isRespawnOfObject = (ignoreElement != null) ? ignoreElement.GetNetworkObject().IsRespawned() : false;
                    if ( isRespawnOfObject == false ) { 
                        if (this.prefabsDatabase != null) {
                            // If this object is "InScene" object i need to set his NetworkId
                            bool inSceneObject = false;
                            NetworkInstantiateDetection instantiateDetection = ((currentObj != null) && (currentObj.GetGameObject() != null)) ? currentObj.GetGameObject().GetComponent<NetworkInstantiateDetection>() : null;
                            if (instantiateDetection != null) {
                                if (instantiateDetection.IsStaticSpawn()) {
                                    inSceneObject = true;
                                }
                            }
                            if (inSceneObject == false) { 
                                foreach (NetworkPrefabEntry networkPrefab in this.prefabsDatabase.GetPrefabs()) {
                                    if ((currentObj != null) &&
                                        (currentObj.GetGameObject() != null) &&
                                        (networkPrefab.GetPrefab() != null)) {
                                        if (currentObj.GetGameObject().IsSamePrefab(networkPrefab.GetPrefab())) {
                                            bool isNetworkPlayer = false;
                                            ushort playerId = 0;
                                            ushort playerIndex = 0;
                                            NetworkObject networkObject = currentObj.GetGameObject().GetComponent<NetworkObject>();
                                            if ( networkObject != null ) {
                                                // Flag if is a player
                                                NetworkPlayerTag playerTag = networkObject.gameObject.GetComponent<NetworkPlayerTag>();
                                                isNetworkPlayer            = (networkObject.IsPlayer() || (playerTag != null));
                                                playerId                   = networkObject.GetNetworkElement().GetPlayerId();
                                                if (playerTag != null) {
                                                    playerIndex = playerTag.GetPlayerIndex();
                                                    networkObject.GetNetworkElement().SetPlayerIndex(playerIndex); // Update player index
                                                }
                                            }
                                            // Instantiate remotelly
                                            using (DataStream writer = new DataStream()) {
                                                writer.Write(currentObj.GetNetworkId());                        // Send network id of object if appliable ( autosync is true )
                                                writer.Write(networkPrefab.GetId());
                                                writer.Write(networkPrefab.GetAutoSync());                      // Auto Sync
                                                writer.Write(false);                                            // Isn't a player instance ( Don't need ack feedback )
                                                writer.Write(isNetworkPlayer);                                  // Is this object a network player ?
                                                writer.Write(playerId);                                         // Player ID
                                                writer.Write(playerIndex);                                      // player index
                                                writer.Write(currentObj.GetGameObject().transform.position);    // Position
                                                writer.Write(currentObj.GetGameObject().transform.eulerAngles); // Rotation
                                                writer.Write(currentObj.GetGameObject().transform.localScale);  // Scale
                                                // Send to this client
                                                client.Send(CoreGameEvents.ObjectInstantiate, writer, DeliveryMode.Reliable);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Detects and handles the destruction of network objects.
        /// </summary>
        private void DetectDestroyedObjects() {
            while (this.destroyedNetworkObjects.Count > 0) {
                int destroyedObjectId = this.destroyedNetworkObjects[0]; // Get first object to check
                this.destroyedNetworkObjects.RemoveAt(0); // Then remove to not try to execute twice on same object
                if ( Mathf.Abs(destroyedObjectId) > 0 ) {
                    // Destroy remotelly
                    using (DataStream writer = new DataStream()) {
                        writer.Write(destroyedObjectId); // Send object to need be destroyed on client's
                        // Send message
                        if ( this.IsConnectedOnRelayServer() && this.IsMasterPlayer() ) {
                            this.GetConnection(ConnectionType.Client).Send(CoreGameEvents.ObjectDestroy, writer, DeliveryMode.Reliable);
                        } else { 
                            this.GetConnection(ConnectionType.Server).Send(CoreGameEvents.ObjectDestroy, writer, DeliveryMode.Reliable);
                        }                        
                    }
                    if (this.respawnedNetworkObjects.ContainsKey(destroyedObjectId)) {
                        Action actionToExecute = this.respawnedNetworkObjects[destroyedObjectId];
                        this.respawnedNetworkObjects.Remove(destroyedObjectId);
                        // Execute delayed action
                        if (actionToExecute != null) {
                            actionToExecute.Invoke();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Instantiates a prefab on the client side with the given parameters and handles network synchronization.
        /// </summary>
        /// <param name="prefabId">The identifier for the prefab to instantiate.</param>
        /// <param name="position">The position to instantiate the prefab at.</param>
        /// <param name="rotation">The rotation to instantiate the prefab with.</param>
        /// <param name="scale">The scale to instantiate the prefab with.</param>
        /// <param name="networkId">The network identifier for the instantiated object (optional).</param>
        /// <param name="autoSync">Flag to determine if the object should automatically synchronize over the network (optional).</param>
        /// <param name="isLocalInstance">Flag to determine if this is a local instance (optional).</param>
        /// <param name="isPlayerInstance">Flag to determine if this is a player instance (optional).</param>
        /// <param name="client">The client that should handle the instantiation (optional).</param>
        public void InstantiateOnClient(int prefabId, Vector3 position, Vector3 rotation, Vector3 scale, int networkId = 0, bool autoSync = false, bool isLocalInstance = false, bool isPlayerInstance = false, IClient client = null, ushort playerId = 0, ushort playerIndex = 0, bool inSceneObject = false) {
            Camera mainCamera = Camera.main;            
            if (this.prefabsDatabase != null) {
                NetworkPrefabEntry prefabEntry = this.prefabsDatabase.GetPrefab(prefabId);
                if (prefabEntry != null) {
                    // If there's no MainCamera I nee to find a cameras on scene 
                    if (isPlayerInstance) {
                        if (mainCamera == null) {
                            Camera[] camerasOnScene = FindObjectsOfType<Camera>();
                            if (camerasOnScene.Length > 0) {
                                foreach (Camera cam in camerasOnScene) {
                                    camerasOnScene[0].tag = NetworkManager.MAIN_CAMERA_TAG;
                                }
                                mainCamera = Camera.main;
                            }
                        }
                    }
                    GameObject instantiatedObject = null;
                    if (inSceneObject) {
                        instantiatedObject = NetworkManager.Instance().GetInSceneObject(networkId);
                        // Remove from list to not duplicate it
                        NetworkManager.Instance().UnRegisterInSceneObject(instantiatedObject); 
                    } else if (NetworkManager.Container.HasElement(networkId)) {
                        return; // If element is already registered i'm going to only ignore
                    }
                    // If can't find i'm oging to spawn anyway
                    if (instantiatedObject == null) {
                        instantiatedObject = GameObject.Instantiate(prefabEntry.GetPrefab(), position, Quaternion.Euler(rotation));
                    }
                    instantiatedObject.transform.localScale = scale;
                    if (autoSync) {
                        BehaviorMode behaviorModeToUse = ((this.IsRemoteInputEnabled()) ? BehaviorMode.Passive : ((isLocalInstance) ? BehaviorMode.Active : BehaviorMode.Passive)); // Need to check to keep acording Active clients witch send his position to server
                        NetworkObject networkObject = instantiatedObject.GetComponent<NetworkObject>();
                        if (networkObject == null) {
                            networkObject = instantiatedObject.AddComponent<NetworkObject>();
                        }
                        NetworkScriptsReference scriptsReference = instantiatedObject.GetComponent<NetworkScriptsReference>();
                        if (scriptsReference == null) {
                            // Since developers can flag not disable some script i need to check those scripts to tell to "NetworkScriptsReference" to ignore those scripts
                            ScriptList scriptList = prefabEntry.GetPrefabScripts();
                            ScriptList scriptListOnRemoteInput = prefabEntry.GetInputScripts();

                            List<MonoBehaviour> ignoredListFromPrefab = new List<MonoBehaviour>();
                            List<Tuple<MonoBehaviour, float>> delayedListFromPrefab = new List<Tuple<MonoBehaviour, float>>();

                            List<MonoBehaviour> ignoredListFromPrefabOnRemoteInput = new List<MonoBehaviour>();
                            if (scriptList != null) {
                                // Ignored scripts
                                foreach (ScriptStatus script in scriptList.GetScripts()) {
                                    if (script.Script != null) {
                                        if (script.Enabled) {
                                            ignoredListFromPrefab.Add(script.Script);
                                        } else if (script.Delay > 0f) {
                                            delayedListFromPrefab.Add(new Tuple<MonoBehaviour, float>(script.Script, (script.Delay / MILLISECONDS_MULTIPLIER)));
                                        }
                                    }
                                }
                                // Ignored scripts when remote input is enabled
                                foreach (ScriptStatus script in scriptListOnRemoteInput.GetScripts()) {
                                    if (!script.Enabled) {
                                        if (script.Script != null) {
                                            ignoredListFromPrefabOnRemoteInput.Add(script.Script);
                                        }
                                    }
                                }
                            }
                            // Add script on spawned element
                            scriptsReference = instantiatedObject.AddComponent<NetworkScriptsReference>();
                            scriptsReference.Collect(ignoredListFromPrefab, delayedListFromPrefab, ignoredListFromPrefabOnRemoteInput);
                            // Now initialize all network variables
                            scriptsReference.CollectAll();
                            foreach (var scriptStatus in scriptList.GetScripts()) {
                                if (scriptStatus.Script != null) {
                                    scriptsReference.InitalizeNetworkVariables(scriptStatus.Script, scriptStatus.GetSynchronizedVariables());
                                }
                            }
                        }
                        scriptsReference.DisableComponents();
                        // Check if any child component shall be disabled
                        NetworkChildsReference      childsReference = instantiatedObject.GetComponent<NetworkChildsReference>();
                        NetworkInstantiateDetection detectionScript = instantiatedObject.GetComponent<NetworkInstantiateDetection>();
                        if (childsReference == null) {
                            // now when i already know the prefab i will check if has ognored scripts
                            GameObjectList childsList = prefabEntry.GetChildObjects();
                            List<GameObject> disabledChildsList = new List<GameObject>();
                            List<GameObjectStatus> delayedListFromPrefab = new List<GameObjectStatus>();
                            if (childsList != null) {
                                foreach (GameObjectStatus childObject in childsList.GetObjects()) {
                                    if (!childObject.Enabled) {
                                        disabledChildsList.Add(childObject.Target);
                                        delayedListFromPrefab.Add(childObject.GenerateSafetyCopy());
                                    }
                                }
                            }
                            // Flag components to disable
                            Transform[] spawnedChilds = instantiatedObject.GetComponentsInChildren<Transform>().Where(t => t.Equals(instantiatedObject.transform) == false).ToArray<Transform>();
                            detectionScript.ResetFlaggedCache();
                            int disabledChildIndex = 0;
                            foreach (GameObject toDisable in disabledChildsList) {
                                int childIndex = 0;
                                foreach (Transform child in spawnedChilds) {
                                    if (detectionScript.IsFlaggedToDisable(child.gameObject, true)) { 
                                        delayedListFromPrefab[disabledChildIndex].TargetInstance = child.gameObject;
                                        break;
                                    }
                                    ++childIndex;
                                }
                                ++disabledChildIndex;
                            }
                            // Add script on spawned element
                            childsReference = instantiatedObject.AddComponent<NetworkChildsReference>();
                            childsReference.Collect(delayedListFromPrefab);
                            if (BehaviorMode.Passive.Equals(behaviorModeToUse)) {
                                childsReference.DisableChilds();
                            } else {
                                childsReference.enabled = false; // Disable to not execute object disable
                            }
                        } else {
                            if (BehaviorMode.Passive.Equals(behaviorModeToUse)) {
                                childsReference.DisableChilds();
                            } else {
                                childsReference.enabled = false; // Disable to not execute object disable
                            }
                        }
                        // Configure network object
                        networkObject.SetTransport(this.GetConnection(ConnectionType.Client));
                        networkObject.SetClient(client);
                        networkObject.SetDeliveryMode(this.sendUpdateMode);
                        networkObject.SetRemoteControlsEnabled(this.useRemoteInput);
                        networkObject.SetIdentified(!isLocalInstance);
                        networkObject.SetBehaviorMode(behaviorModeToUse, BehaviorMode.Passive.Equals(behaviorModeToUse)); // Need to check to keep acording Active clients witch send his position to server
                        networkObject.SetSyncParticles(prefabEntry.GetParticlesAutoSync());
                        networkObject.SetSyncAnimation(prefabEntry.GetAnimationAutoSync());
                        networkObject.SetSyncAnimationMode(prefabEntry.GetAnimationSyncMode());
                        networkObject.SetAnimationLayerCount(prefabEntry.GetAnimationCount());
                        networkObject.SetAnimationDefaultStatus(prefabEntry.GetAnimationDefaultStatus());
                        networkObject.SetOwnershipAccessLevel(prefabEntry.GetOwnerShipAccessLevel());
                        networkObject.SetMovementType(prefabEntry.GetMovementType());
                        networkObject.SetDisableGravity(prefabEntry.IsToDisableGravity());
                        networkObject.SetEnableKinematic(prefabEntry.IsToEnableKinematic());
                        networkObject.SetSendRate((prefabEntry.IsToUseCustomRate()) ? prefabEntry.GetSendRateAmount() : this.sendRateAmount);
                        networkObject.SetIsPlayer(isPlayerInstance);
                        // Configure ownership behaviors
                        networkObject.OnSpawnPrefab((prefabEntry.GetOnSpawnPrefab() != null) ? prefabEntry.GetOnSpawnPrefab().ToEventReference() : null);
                        networkObject.OnDespawnPrefab((prefabEntry.GetOnDespawnPrefab() != null) ? prefabEntry.GetOnDespawnPrefab().ToEventReference() : null);
                        networkObject.OnAcceptOwnerShip((prefabEntry.GetOnAcceptObjectOwnerShip() != null) ? prefabEntry.GetOnAcceptObjectOwnerShip().ToEventReference() : null);
                        networkObject.OnAcceptReleaseOwnerTakeShip((prefabEntry.GetOnAcceptReleaseObjectOwnerShip() != null) ? prefabEntry.GetOnAcceptReleaseObjectOwnerShip().ToEventReference() : null);
                        networkObject.OnTakeObjectOwnerShip((prefabEntry.GetOnTakeObjectOwnership() != null) ? prefabEntry.GetOnTakeObjectOwnership().ToEventReference() : null);
                        networkObject.OnReleaseObjectOwnerShip((prefabEntry.GetOnReleaseObjectOwnership() != null) ? prefabEntry.GetOnReleaseObjectOwnership().ToEventReference() : null);
                        // Tranform update values
                        networkObject.SetSyncPosition(prefabEntry.GetSyncPosition());
                        networkObject.SetSyncRotation(prefabEntry.GetSyncRotation());
                        networkObject.SetSyncScale(prefabEntry.GetSyncScale());
                        // Start network services
                        networkObject.StartNetwork(networkId, (INetworkElement element) => {
                            // Update network element
                            INetworkElement networkElement = NetworkManager.Instance().GetObjectOnClient<INetworkElement>(networkId);
                            if (networkElement != null) {
                                networkElement.SetIsPlayer(isPlayerInstance);
                                networkElement.SetPlayerId(playerId);
                                networkElement.SetPlayerIndex(playerIndex);
                                networkElement.SetEnableMinimunRate(isPlayerInstance && (NetworkManager.Instance().GetMinimumSendRate() > 0));
                                networkElement.SetMinimunRateValue(NetworkManager.Instance().GetMinimumSendRate());
                            }
                            // Set Behaviour Mode
                            networkObject.SetBehaviorMode(behaviorModeToUse, BehaviorMode.Passive.Equals(behaviorModeToUse)); // Need to check to keep acording Active clients witch send his position to server
                            return element;
                        });
                        // If this is an player instance i need to check if he has camera
                        if (inSceneObject == false) {
                            if (NetworkManager.Instance().controlPlayerCameras) {
                                if ( isPlayerInstance ) {
                                    if (instantiatedObject != null) {
                                        if (mainCamera != null) {
                                            Camera[] cameras = instantiatedObject.GetComponentsInChildren<Camera>();
                                            if (cameras.Length > 1) {
                                                Debug.Log("====================================================================================================");
                                                Debug.Log("[ATTENTION] Spawned player has more than one camera inside prefab, the first camera will be the main");
                                                Debug.Log("====================================================================================================");
                                            }
                                            foreach (Camera cam in cameras) {
                                                if (mainCamera != cam) {
                                                    cam.enabled = false;
                                                    AudioListener audioControl = cam.GetComponent<AudioListener>();
                                                    if (audioControl != null) {
                                                        audioControl.enabled = false;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            // If this object is a instance of player i need to send feedbak to finish player identification process                        
                            if (isLocalInstance) {
                                if (client != null) {
                                    using (DataStream writer = new DataStream()) {
                                        writer.Write(networkId); // Send object created on client
                                        client.Send(CoreGameEvents.ObjectCreatedOnClient, writer, DeliveryMode.Reliable);
                                    }
                                }
                            }
                            // Execute network prefab callback
                            networkObject.ExecuteOnSpawnPrefab();
                        }
                        // Initialize internal InternalExecutor
                        networkObject.InitializeExecutor();                        
                    }
                    // Trigger player spawn event
                    if (isPlayerInstance) {
                        this.PlayerSpawnedEvent(instantiatedObject);                        
                    }                    
                }
            }
        }

        /// <summary>
        /// Return array containing all child GameObject information generated on Network Database Manager
        /// </summary>
        /// <param name="networkObject">Object to get child informations</param>
        /// <returns>Array of all child informations</returns>
        public GameObjectStatus[] GetChildsToSynchronizeTransform(GameObject networkObject) {
            List<GameObjectStatus>      resultList      = new List<GameObjectStatus>();
            NetworkInstantiateDetection detectionScript = networkObject.GetComponent<NetworkInstantiateDetection>();
            if (this.prefabsDatabase != null) {
                // First i'm going to find prefab reference on database
                foreach (NetworkPrefabEntry networkPrefab in this.prefabsDatabase.GetPrefabs()) {
                    if (networkObject.IsSamePrefab(networkPrefab.GetPrefab())) {
                        // now when i already know the prefab i will check if has ognored scripts
                        GameObjectList childsList = networkPrefab.GetChildObjects();
                        List<GameObject> synchronizeChildsList = new List<GameObject>();
                        if (childsList != null) {
                            foreach (GameObjectStatus childObject in childsList.GetObjects()) {
                                if (childObject.SyncPosition || 
                                    childObject.SyncRotation || 
                                    childObject.SyncScale   ) {
                                    synchronizeChildsList.Add(childObject.Target);
                                    resultList.Add(childObject.GenerateSafetyCopy());
                                }
                            }
                        }
                        // Flag components to disable
                        detectionScript.ResetFlaggedCache();
                        Transform[] spawnedChilds = networkObject.GetComponentsInChildren<Transform>(true);
                        int synchronizeChildIndex = 0;
                        foreach (GameObject tosynchronize in synchronizeChildsList) {
                            int childIndex = 0;
                            foreach (Transform child in spawnedChilds) {
                                if (detectionScript.IsFlaggedToSync(child.gameObject, true)) { 
                                    resultList[synchronizeChildIndex].TargetInstance = child.gameObject;
                                    break;
                                }
                                ++childIndex;
                            }
                            ++synchronizeChildIndex;
                        }
                        // Now i'm going to remove fosters
                        for( int childIndex = resultList.Count-1; childIndex >= 0; childIndex--) {
                            if (resultList[childIndex].TargetInstance == null) {
                                resultList.RemoveAt(childIndex);
                            }
                        }
                        break;
                    }
                }
            }
            return resultList.ToArray<GameObjectStatus>();
        }

        /// <summary>
        /// Retrieves a spawn position from a list of multiple possible positions.
        /// </summary>
        /// <returns>A Vector3 representing the chosen spawn position.</returns>
        private Vector3 GetSpawnedPositionFromMultiple() {
            Vector3 result = Vector3.zero;
            if (this.possibleSpawnedPositions.Count == 0) {
                for (int possibleIndex = 0; possibleIndex < this.multiplePositionsToSpawn.Count; possibleIndex++) {
                    this.possibleSpawnedPositions.Add(possibleIndex);
                }
            }
            if ( this.possibleSpawnedPositions.Count == 0 ) {
                result = this.fixedPositionToSpawn.position;            
            } else {
                int spawnedIndex = Mathf.Clamp(0, 
                                               UnityEngine.Random.Range(0, this.possibleSpawnedPositions.Count - 1), 
                                               Mathf.Clamp(this.possibleSpawnedPositions.Count-1, 0, Int32.MaxValue));
                result = this.multiplePositionsToSpawn[this.possibleSpawnedPositions[spawnedIndex] ].position;
                this.possibleSpawnedPositions.RemoveAt(spawnedIndex);
            }
            return result;
        }

        /// <summary>
        /// Retrieves a spawn position by invoking a method from a dynamic position spawner component.
        /// </summary>
        /// <returns>A Vector3 representing the chosen spawn position.</returns>
        private Vector3 GetSpawnedPositionFromDynamic() {
            Vector3 result = Vector3.zero;
            if ( this.positionSpawnerComponent != null ) {
                MethodInfo methodToExecute = this.positionSpawnerComponent.GetType().GetMethod(this.positionSpawnerMethod);
                result = (Vector3)methodToExecute.Invoke(this.positionSpawnerComponent, null);
            } else {
                result = this.fixedPositionToSpawn.position;
            }
            return result;
        }

        /// <summary>
        /// Retrieves the player prefab to spawn based on the current spawn mode.
        /// </summary>
        /// <returns>A GameObject representing the player prefab to spawn.</returns>
        private GameObject GetPlayerPrefabToSpawn() {
            GameObject result = null;
            if ( NetworkPlayerSpawnMode.SingleElement.Equals(this.playerSpawnMode) ) {
                result = this.playerPrefabToSpawn;
            } else if ( NetworkPlayerSpawnMode.DynamicElement.Equals(this.playerSpawnMode) ) {
                if ( this.playerSpawnerComponent != null ) {
                    MethodInfo methodToExecute = this.playerSpawnerComponent.GetType().GetMethod(this.playerSpawnerMethod);
                    result = (methodToExecute.Invoke(this.playerSpawnerComponent, null) as GameObject);
                } else {
                    result = this.playerPrefabToSpawn;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves the player position to spawn based on the current spawn position mode.
        /// </summary>
        /// <returns>A Vector3 representing the chosen player spawn position.</returns>
        private Vector3 GetPlayerPositionToSpawn() {
            Vector3 result = Vector3.zero;
            if (NetworkSpawnPositionMode.Fixed.Equals(spawnPositionMode)) {
                result = this.fixedPositionToSpawn.position;
            } else if (NetworkSpawnPositionMode.Multiple.Equals(spawnPositionMode)) {
                result = this.GetSpawnedPositionFromMultiple();
            } else if (NetworkSpawnPositionMode.Dynamic.Equals(spawnPositionMode)) {
                result = this.GetSpawnedPositionFromDynamic();
            }
            return result;
        }

        /// <summary>
        /// Destroys a network object on the client side.
        /// </summary>
        /// <param name="networkId">The network ID of the object to destroy.</param>
        public void DestroyOnClient(int networkId) {
            // Find object to destroy
            if (NetworkManager.GetContainer().IsRegistered(networkId)) {
                INetworkElement element = NetworkManager.GetContainer().GetElement(networkId);
                NetworkManager.GetContainer().UnRegister(element);
                GameObject.Destroy(element.GetGameObject());
            }
        }

        /// <summary>
        /// Retrieves a network object on the client side by its network ID.
        /// </summary>
        /// <typeparam name="T">The type of the network element to retrieve.</typeparam>
        /// <param name="networkId">The network ID of the object to retrieve.</param>
        /// <returns>The network object of type T, or default if not found.</returns>
        public T GetObjectOnClient<T>(int networkId) where T : INetworkElement {
            T result = default(T);
            if (NetworkManager.GetContainer().IsRegistered(networkId)) {
                result = ((T)NetworkManager.GetContainer().GetElement(networkId));
            }
            return result;
        }

        /// <summary>
        /// Retrieves a network object on the client side by its player ID.
        /// </summary>
        /// <typeparam name="T">The type of the network element to retrieve.</typeparam>
        /// <param name="playerId">The player ID of the object to retrieve.</param>
        /// <returns>The network object of type T, or default if not found.</returns>
        public T GetObjectOnClient<T>(ushort playerId) where T : INetworkElement {
            T result = default(T);
            if (NetworkManager.GetContainer().IsRegistered(playerId)) {
                result = ((T)NetworkManager.GetContainer().GetElement(playerId));
            }
            return result;
        }

        /// <summary>
        /// Registers a detected network object to keep track of it.
        /// </summary>
        /// <param name="networkObject">The GameObject to register.</param>
        public void RegisterDetectedObject(GameObject networkObject, bool staticSpawn = false) {
            if (this.detectedNetworkObjects.Contains(networkObject) == false) {
                this.detectedNetworkObjects.Add(networkObject);
            } else if (staticSpawn) {
                throw new Exception(string.Format("The object flagged as \"In Scene object\" has a duplicated Network Id", networkObject.name));
            }
        }

        /// <summary>
        /// Return if system already tried to collect inScene objects
        /// </summary>
        /// <returns>True is yesm, otherwise false</returns>
        public bool InSceneObjectsCollected() {
            return this.inSceneCollected;
        }

        /// <summary>
        /// Flag that system already tried to collect inScene objects
        /// </summary>
        public void FlagInSceneObjectsCollected() {
            this.inSceneCollected = true;
        }

        /// <summary>
        /// Return if remote scene is loading
        /// </summary>
        /// <returns>True is loading, otherwise false</returns>
        public bool IsLoadingRemoteScene() {
            return this.remoteSceneLoading;
        }

        /// <summary>
        /// Flag that remote scene loading started
        /// </summary>
        public void FlagLoadingSceneStarted() {
            this.remoteSceneLoading = true;
        }

        /// <summary>
        /// Flag that remote scene loading finished
        /// </summary>
        public void FlagLoadingSceneFinished() {
            this.remoteSceneLoading = false;
        }

        /// <summary>
        /// Flag async operation for remote scene loading started
        /// </summary>
        /// <param name="asyncLoading"></param>
        public void StartRemoteAsyncSceneLoding(AsyncOperation asyncLoading) {
            this.asyncLoadingScene = asyncLoading;
            this.asyncLoadingScene.allowSceneActivation = false;
        }

        /// <summary>
        /// Return tif remote scene loading was finished
        /// </summary>
        /// <returns>True if was finished, otherwise false</returns>
        public bool IsAsyncSceneLoadFinished() {
            return (this.asyncLoadingScene.isDone) || 
                   (this.asyncLoadingScene.progress >= 0.90f);
        }

        /// <summary>
        /// Activate a pre loaded remote scene
        /// </summary>
        public void ActivateRemoteScene() {
            this.asyncLoadingScene.allowSceneActivation = true;
            this.asyncLoadingScene = null;
        }
        

        /// <summary>
        /// Return if system already requested to updated inScene objects
        /// </summary>
        /// <returns>True is yesm, otherwise false</returns>
        public bool InSceneObjectsRequested() {
            return this.inSceneUpdatedRequested;
        }

        /// <summary>
        /// Flag that system already requested to update inScene objects
        /// </summary>
        public void FlagInSceneObjectsRequested() {
            this.inSceneUpdatedRequested = true;
        }

        /// <summary>
        /// Flag that system need to requeste inScene objects update
        /// </summary>
        public void FlagInSceneObjectsNotRequested() {
            this.inSceneUpdatedRequested = false;
        }

        /// <summary>
        /// Return if system already allowed to try to collect inScene objects
        /// </summary>
        /// <returns>True is yesm, otherwise false</returns>
        public bool InSceneCollectedAllowed() {
            return this.allowToSceneCollect;
        }

        /// <summary>
        /// Flag that system is allowed to try to collect inScene objects
        /// </summary>
        public void FlagInSceneObjectsAllowed() {
            this.allowToSceneCollect = true;
        }

        /// <summary>
        /// Registers in scene object to keep track of it.
        /// </summary>
        /// <param name="networkObject">The GameObject to register.</param>
        public void RegisterInSceneObject(GameObject networkObject) {
            if (this.detectedInSceneObjects.Contains(networkObject) == false) {
                this.detectedInSceneObjects.Add(networkObject);
            }
            if (this.currentInSceneObjects.Contains(networkObject) == false) {
                // First check if this id is already registered
                bool alreadyRegistered = false;
                NetworkInstantiateDetection currentObj = networkObject.GetComponent<NetworkInstantiateDetection>();
                if (currentObj != null) {
                    foreach (GameObject go in this.currentInSceneObjects.ToArray()) {
                        if (go == null) { this.currentInSceneObjects.Remove(go); continue; }
                        if (currentObj.GetStaticId() == go.GetComponent<NetworkInstantiateDetection>().GetStaticId()) {
                            Debug.LogError(string.Format("The object \"{0}\" is flagged as \"In Scene object\" and has a duplicated Network Id [{1}]", networkObject.name, currentObj.GetStaticId()));
                            alreadyRegistered = true;
                            break;
                        }
                    }
                }
                if (alreadyRegistered == false) {
                    this.currentInSceneObjects.Add(networkObject);
                }
            }            
        }

        /// <summary>
        /// Unregister in scene object.
        /// </summary>
        /// <param name="networkObject">The GameObject to unregister.</param>
        public void UnRegisterInSceneObject(GameObject networkObject, bool destroyed = false) {
            if (this.detectedInSceneObjects.Contains(networkObject) == true) {
                this.detectedInSceneObjects.Remove(networkObject);
            }
            if (destroyed) {
                if (this.currentInSceneObjects.Contains(networkObject) == true) {
                    this.currentInSceneObjects.Remove(networkObject);
                }
            }
        }

        /// <summary>
        /// Return in scene object based on his static id.
        /// </summary>
        /// <param name="staticNetworkId">Static network id.</param>
        public GameObject GetInSceneObject(int staticNetworkId, bool useCurrent = false) {
            GameObject result = null;
            foreach (GameObject inScene in ((useCurrent) ? this.currentInSceneObjects : this.detectedInSceneObjects)) {
                NetworkInstantiateDetection detectionScript = inScene.GetComponent<NetworkInstantiateDetection>();
                if (detectionScript != null) {
                    if (detectionScript.IsStaticSpawn()) {
                        if (detectionScript.GetStaticId().Equals(staticNetworkId)) {
                            result = inScene;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Return array containing all ID's of object inScene
        /// </summary>
        /// <returns>Array containing all ID's of object inScene</returns>
        public int[] GetInSceneObjectsIds() {
            List<int> result = new List<int>();
            foreach (GameObject inScene in this.currentInSceneObjects) {
                NetworkInstantiateDetection detectionScript = inScene.GetComponent<NetworkInstantiateDetection>();
                if (detectionScript != null) {
                    if (detectionScript.IsStaticSpawn()) {
                        result.Add(detectionScript.GetStaticId());
                    }
                }
            }
            return result.ToArray<int>();
        }

        /// <summary>
        /// Return if this object is registered.
        /// </summary>
        /// <param name="networkObject">The GameObject to register.</param>
        public bool InSceneObjectRegistered(GameObject networkObject) {
            NetworkInstantiateDetection detectionScript = networkObject.GetComponent<NetworkInstantiateDetection>();
            return (this.detectedInSceneObjects.Contains(networkObject) == true) ||
                   (detectionScript == null) ||
                   (detectionScript.IsStaticSpawn() == false) || 
                   (Mathf.Abs(detectionScript.GetStaticId()) == 0);
        }

        /// <summary>
        /// Return if has some InScene object registered
        /// </summary>
        /// <returns>True if has InScene objects</returns>
        public bool HasInSceneObjects() {
            return (this.detectedInSceneObjects.Count > 0);
        }

        /// <summary>
        /// Return the next in scene object.
        /// </summary>
        /// <returns>In scene object</returns>
        public GameObject GetNextInSceneObject(bool remove = true) {
            GameObject result = this.detectedInSceneObjects[0];
            if (remove) {
                this.detectedInSceneObjects.RemoveAt(0);
            }
            return result;
        }

        /// <summary>
        /// Request server to send InScene objects update
        /// </summary>
        /// <param name="connectedClient">Client to advice</param>
        public void RequestNetworkInSceneUpdate() {
            try {
                using (DataStream writer = new DataStream()) {
                    writer.Write(this.GetConnection().GetSocket().GetConnectionID()); // send origin connection id
                    this.GetConnection().Send(CoreGameEvents.RequestStaticSpawnUpdate, writer, DeliveryMode.Reliable);
                }
            } catch (Exception err) {
                NetworkDebugger.LogError(err.Message);
            }
        }

        /// <summary>
        /// Respawn network player 
        /// Note: This method shall be used when a player is destroyed and you need to respawn in some other place
        /// </summary>
        /// <param name="client">Client to be respawned ( Optional parameter to use on the server side only )</param>
        public void RespawnPlayer(IClient client = null) {
            if (NetworkManager.Instance().InAuthoritativeMode() ||
                (NetworkManager.Instance().InEmbeddedMode() && NetworkManager.Instance().IsMasterPlayer()) ||
                (NetworkManager.Instance().IsConnectedOnRelayServer() && NetworkManager.Instance().IsMasterPlayer())) {
                // When is null i'm going to log an exception to warning developper
                if (client == null) {
                    NetworkDebugger.LogError("To respawn player on server side you need to provide connected client by using \"client\" argument");
                } else {
                    this.RespawnPlayerOnServer(client);
                }
            } else {
                if (client != null) {
                    NetworkDebugger.LogWarning("When player is respawned on the client side the parameter \"client\" is ignored");
                }
                this.RespawnPlayerRemote();
            }
        }

        /// <summary>
        /// Respawn network player 
        /// Note: This method shall be used when a player is destroyed and you need to respawn in some other place
        /// </summary>
        /// <param name="client">Client to be respawned ( Optional parameter to use on the server side only )</param>
        public void RespawnServerPlayer() {
            if (NetworkManager.Instance().InAuthoritativeMode() ||
                (NetworkManager.Instance().InEmbeddedMode() && NetworkManager.Instance().IsMasterPlayer()) ||
                (NetworkManager.Instance().IsConnectedOnRelayServer() && NetworkManager.Instance().IsMasterPlayer())) {
                this.RespawnServerLocalPlayer();
            } else {
                NetworkDebugger.LogError("To respawn remote cliednt player you need to use the \"RespawnPlayer\" method instead of this");                
            }
        }

        /// <summary>
        /// Request server to respawn a player
        /// Note : Player will be automatically spawned o client when server spawn an notify object spawned
        /// </summary>
        private void RespawnPlayerRemote() {
            try {
                NetworkPlayer currentPlayer = NetworkManager.Instance().GetLocalPlayer<NetworkPlayer>();
                using (DataStream writer = new DataStream()) {
                    writer.Write(this.GetConnection().GetSocket().GetConnectionID()); // send origin connection id
                    writer.Write((ushort)(( currentPlayer != null ) ? currentPlayer.GetPlayerId() : 0)); // send player ID to respawn
                    this.GetConnection().Send(CoreGameEvents.PlayerRespawOnClient, writer, DeliveryMode.Reliable);
                }
            } catch (Exception err) {
                NetworkDebugger.LogError(err.Message);
            }
        }

        /// <summary>
        /// Respawn player into server
        /// Note: This method will respawn player on server side, this will make player to be spawned on client side when server 
        /// execute "DetectSpawnedObjects"
        /// </summary>
        private void RespawnPlayerOnServer(IClient connectionClient) {
            try {
                NetworkClient clientToRespawn   = (connectionClient as NetworkClient);
                NetworkPlayer currentPlayer     = NetworkManager.Instance().GetPlayer<NetworkPlayer>(connectionClient);
                if (NetworkManager.Instance().IsRunningLogic() == false) {
                    NetworkManager.Instance().SpawnClientPlayerOnServer(connectionClient,
                                                                        clientToRespawn.GetConnectionId(),
                                                                        false, 
                                                                        Vector3.zero, 
                                                                        false, 
                                                                        Quaternion.identity,
                                                                        ((ushort)((currentPlayer != null) ? currentPlayer.GetPlayerId() : 0)));
                } else {
                    NetworkDebugger.LogError("You can't execute this method on the client side, you need to execute the \"RespawnServerPlayer\" method instead");
                }
            } catch (Exception err) {
                NetworkDebugger.LogError(err.Message);
            }
        }

        /// <summary>
        /// Respawn master server player
        /// Note: This method shall be executed on the sertver side only
        /// </summary>
        private void RespawnServerLocalPlayer() {
            try {
                if (NetworkManager.Instance().IsRunningLogic() == true) {
                    NetworkManager.Instance().SpawnServerPlayer();
                } else {
                    NetworkDebugger.LogError("You can't execute this method on the server side, you need to execute the \"RespawnPlayerOnServer\" method instead");
                }
            } catch (Exception err) {
                NetworkDebugger.LogError(err.Message);
            }
        }

        /// <summary>
        /// Registers a destroyed network object to keep track of it.
        /// </summary>
        /// <param name="networkObject">The network ID of the destroyed object.</param>
        public void RegisterDestroyedObject(Int32 networkObject) {
            if (this.destroyedNetworkObjects.Contains(networkObject) == false) {
                this.destroyedNetworkObjects.Add(networkObject);
            }
        }

        /// <summary>
        /// Registers a callback to execute when an object was destroyed.
        /// </summary>
        /// <param name="networkObject">The network ID of the destroyed object.</param>
        /// <param name="callback">Callback to execute when object is destroyed.</param>
        /// <param name="replace">Replace an previous existent action.</param>
        public void RegisterOnDestroyAction(int networkObject, Action callback, bool replace = false) {
            if (this.respawnedNetworkObjects.ContainsKey(networkObject) == false) {
                this.respawnedNetworkObjects.Add(networkObject, callback);
            } else if (replace) {
                this.respawnedNetworkObjects[networkObject] = callback;
            }
        }
        

        /// <summary>
        /// Registers a garbage network object associated with the current network session.
        /// </summary>
        /// <param name="garbageObject">The GameObject to register as garbage.</param>
        public void RegisterGarbageObject(GameObject garbageObject) {
            if (!this.garbageNetworkObjects.ContainsKey(this.networkSessionId)) {
                this.garbageNetworkObjects.Add(this.networkSessionId, new List<GameObject>());
            }
            this.garbageNetworkObjects[this.networkSessionId].Add(garbageObject);            
        }

        /// <summary>
        /// Retrieves all network prefabs from the prefabs database.
        /// </summary>
        /// <returns>An array of GameObjects representing the network prefabs.</returns>
        public GameObject[] GetNetworkPrefabs() {
            return (this.prefabsDatabase != null) ? this.prefabsDatabase.GetPrefabObjects(): null;
        }

        /// <summary>
        /// Retrieves a specific network prefab by its signature.
        /// </summary>
        /// <param name="prefabSignature">The signature of the prefab to retrieve.</param>
        /// <returns>The prefab entry, or null if not found.</returns>
        public NetworkPrefabEntry GetNetworkPrefabEntry(string prefabSignature) {
            NetworkPrefabEntry result = null;
            foreach (NetworkPrefabEntry networkPrefab in this.prefabsDatabase.GetPrefabs()) {
                if (networkPrefab.GetPrefab().IsSamePrefab(prefabSignature)) {
                    result = networkPrefab;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves a specific network prefab by gameobject prefab.
        /// </summary>
        /// <param name="prefab">prefab to find.</param>
        /// <returns>The prefab entry, or null if not found.</returns>
        public NetworkPrefabEntry GetNetworkPrefabEntry(GameObject prefab) {
            NetworkInstantiateDetection detector = prefab.GetComponent<NetworkInstantiateDetection>();
            NetworkPrefabEntry result = null;
            if (detector != null) {
                foreach (NetworkPrefabEntry networkPrefab in this.prefabsDatabase.GetPrefabs()) {
                    if (networkPrefab.GetPrefab().IsSamePrefab(detector.GetPrefabSignature())) {
                        result = networkPrefab;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Determines if automatic player spanwer is enabled or note
        /// </summary>
        /// <returns>True if player spawner is enabled, otherwise false.</returns>
        public bool IsPlayerSpawnerEnabled() {
            return this.enablePlayerSpawner;
        }

        /// <summary>
        /// Determines if network login is enabled.
        /// </summary>
        /// <returns>True if network login is enabled, otherwise false.</returns>
        public bool IsLoginEnabled() {
            return this.useNetworkLogin;
        }

        /// <summary>
        /// Determines if login validation is enabled.
        /// </summary>
        /// <returns>True if login validation is enabled, otherwise false.</returns>
        public bool IsToValidateLogin() {
            return this.enableLoginValidation;
        }

        /// <summary>
        /// Retrieves login information as an array of objects.
        /// </summary>
        /// <returns>An array of objects containing login information.</returns>
        public object[] GetLoginInformations() {
            return this.loginInfoComponent.GetType().GetMethod(this.loginInfoMethod).Invoke(this.loginInfoComponent, new object[] { }) as object[];
        }

        /// <summary>
        /// Retrieves the types of the login information.
        /// </summary>
        /// <returns>An array of Type objects representing the types of the login information.</returns>
        public Type[] GetLoginInformationsTypes() {
            return this.loginInfoComponent.GetType().GetMethod(this.loginTypesMethod).Invoke(this.loginInfoComponent, new object[] { }) as Type[];
        }

        /// <summary>
        /// Validates the provided login arguments.
        /// </summary>
        /// <param name="arguments">An array of objects representing the login arguments.</param>
        /// <returns>True if the login is valid, otherwise false.</returns>
        public bool IsValidLogin(object[] arguments) {
            return ((bool) this.loginValidationComponent.GetType().GetMethod(this.loginValidationMethod).Invoke(this.loginValidationComponent, new object[] { arguments }));
        }        

        /// <summary>
        /// Retrieves a custom network identifier based on the provided arguments.
        /// </summary>
        /// <param name="arguments">An array of objects representing the arguments for retrieving the network ID.</param>
        /// <returns>An integer representing the custom network ID.</returns>
        public int GetCustomNetworkId(object[] arguments) {
            return ((int) this.networkIdComponent.GetType().GetMethod(this.networkIdMethod).Invoke(this.networkIdComponent, new object[] { arguments }));
        }

        /// <summary>
        /// Determines if the network clock should be used.
        /// </summary>
        /// <returns>True if the network clock is used, otherwise false.</returns>
        public bool UseNetworkClock() {
            return this.useNetworkClock;
        }

        /// <summary>
        /// Determines if movement prediction is enabled.
        /// </summary>
        /// <returns>True if movement prediction is enabled, otherwise false.</returns>
        public bool UseMovementPrediction() {
            return this.useMovementPrediction;
        }

        /// <summary>
        /// Determines if position intepolation is enabled.
        /// </summary>
        /// <returns>True if movement interpolation is enabled, otherwise false.</returns>
        public bool UseInterpolation() {
            return this.useInterpolation;
        }

        /// <summary>
        /// Determines if automatic prediction is enabled.
        /// </summary>
        /// <returns>True if automatic prediction is enabled, otherwise false.</returns>
        public bool UseAutomaticPrediction() {
            return (PredictionType.Automatic == this.predictionType);
        }

        /// <summary>
        /// Determines if transform-based prediction is enabled.
        /// </summary>
        /// <returns>True if transform-based prediction is enabled, otherwise false.</returns>
        public bool UseTransformPrediction() {
            return (PredictionType.UseTransform == this.predictionType);
        }

        /// <summary>
        /// Determines if physics-based prediction is enabled.
        /// </summary>
        /// <returns>True if physics-based prediction is enabled, otherwise false.</returns>
        public bool UsePhysicsPrediction() {
            return (PredictionType.UsePhysics == this.predictionType);
        }

        /// <summary>
        /// Retrieves the size of the prediction buffer.
        /// </summary>
        /// <returns>An integer representing the size of the prediction buffer.</returns>
        public PredictionType GetPredictionTechnique() {
            return this.predictionType;
        }

        /// <summary>
        /// Retrieves type of movement used by player
        /// </summary>
        /// <returns>Enum representing the type of movement used by player.</returns>
        public PredictionType GetMovementTechnique() {
            return this.movementType;
        }

        /// <summary>
        /// Determines if automatic movement is enabled.
        /// </summary>
        /// <returns>True if automatic movement is enabled, otherwise false.</returns>
        public bool UseAutomaticMovement() {
            return (PredictionType.Automatic == this.movementType);
        }

        /// <summary>
        /// Determines if transform-based movement is enabled.
        /// </summary>
        /// <returns>True if transform-based prediction is enabled, otherwise false.</returns>
        public bool UseTransformMovement() {
            return (PredictionType.UseTransform == this.movementType);
        }

        /// <summary>
        /// Determines if physics-based movement is enabled.
        /// </summary>
        /// <returns>True if physics-based movement is enabled, otherwise false.</returns>
        public bool UsePhysicsMovement() {
            return (PredictionType.UsePhysics == this.movementType);
        }




        /// <summary>
        /// Retrieves the prediction factor curve.
        /// </summary>
        /// <returns>An AnimationCurve representing the prediction factor over time.</returns>
        public int GetPredictionBufferSize() {
            return this.predictionBufferSize;
        }

        /// <summary>
        /// Retrieves the prediction speed factor curve.
        /// </summary>
        /// <returns>An AnimationCurve representing the prediction speed factor over time.</returns>
        public AnimationCurve GetPredictionFactor() {
            return this.predictionFactor;
        }

        /// <summary>
        /// Determines if there is an override for the prediction logic.
        /// </summary>
        /// <returns>True if there is a prediction override, otherwise false.</returns>
        public AnimationCurve GetPredictionSpeedFactor() {
            return this.predictionSpeedFactor;
        }

        /// <summary>
        /// Retrieves the prediction override component if available.
        /// </summary>
        /// <returns>An IPrediction instance representing the prediction override, or null if not available.</returns>
        public bool HasPredictionOverride() {
            return (this.predictionComponent != null);
        }

        /// <summary>
        /// Determines if remote input is used.
        /// </summary>
        /// <returns>True if remote input is used, otherwise false.</returns>
        public IPrediction GetPredictionOverride() {
            return (this.predictionComponent as IPrediction);
        }

        /// <summary>
        /// Determines if latency tolerance features are enabled.
        /// </summary>
        /// <returns>True if latency tolerance is enabled, otherwise false.</returns>
        public bool UseRemoteInput() {
            return this.useRemoteInput;
        }

        /// <summary>
        /// Retrieves the value considered as good latency.
        /// </summary>
        /// <returns>An integer representing the good latency value.</returns>
        public bool UseLatencyTolerance() {
            return this.latencyTolerance;
        }

        /// <summary>
        /// Retrieves the value considered as acceptable latency.
        /// </summary>
        /// <returns>An integer representing the acceptable latency value.</returns>
        public int GetGoodLatencyValue() {
            return this.goodLatencyValue;
        }

        /// <summary>
        /// Determines if the system should control player cameras.
        /// </summary>
        /// <returns>True if the system should control player cameras, otherwise false.</returns>
        public int GetAcceptableLatencyValue() {
            return this.acceptableLatencyValue;
        }

        /// <summary>
        /// Determines if the player camera should be detached.
        /// </summary>
        /// <returns>True if the player camera should be detached, otherwise false.</returns>
        public bool IsToControlCameras() {
            return this.controlPlayerCameras;
        }

        /// <summary>
        /// Retrieves the update mode for network communication.
        /// </summary>
        /// <returns>A DeliveryMode value representing the update mode.</returns>
        public bool IsToDetachPlayerCamera() {
            return this.detachPlayerCamera;
        }

        /// <summary>
        /// Disables the auto-reconnect feature.
        /// </summary>
        public DeliveryMode GetUpdateMode() {
            return this.sendUpdateMode;
        }

        /// <summary>
        /// Disable communication auto reconnect
        /// </summary>
        public void DisableAutoReconnect() {
            this.disableReconnect = true;
        }

        /// <summary>
        /// Enable auto reconnect of comunication
        /// </summary>
        public void EnableAutoReconnect() {
            this.disableReconnect = false;
        }

        /// <summary>
        /// Determines if a custom network ID should be used.
        /// </summary>
        /// <returns>True if a custom network ID is used, otherwise false.</returns>
        public bool IsToUseInternalNetworkId() {
            this.useInternalNetworkId &= (this.useCustomNetworkId == false);
            return this.useInternalNetworkId;
        }

        /// <summary>
        /// Determines if the system is in relay mode.
        /// </summary>
        /// <returns>True if the system is in relay mode, otherwise false.</returns>
        public bool IsToUseCustomNetworkId() {
            this.useCustomNetworkId &= (this.useInternalNetworkId == false);
            return this.useCustomNetworkId;
        }

        /// <summary>
        /// Determines if the system is in embedded mode.
        /// </summary>
        /// <returns>True if the system is in embedded mode, otherwise false.</returns>
        public bool InRelayMode() {
            return (NetworkServerMode.Relay == this.serverWorkingMode);
        }

        /// <summary>
        /// Return is peer to peer mode is enabled
        /// </summary>
        /// <returns>true if peer to peer is enabled, otherwise false.</returns>
        public bool IsPeerToPeerEnabled() {
            return ((this.usePeerToPeer) && (NetworkManager.Instance().IsPeerToPeerSupported()));
        }

        /// <summary>
        /// Determines if the system is in authoritative mode.
        /// </summary>
        /// <returns>True if the system is in authoritative mode, otherwise false.</returns>
        public bool InEmbeddedMode() {
            return (NetworkServerMode.Embedded == this.serverWorkingMode);
        }

        /// <summary>
        /// Determines if the server mode is disabled.
        /// </summary>
        /// <returns>True if the server mode is set to client only, otherwise false.</returns>
        public bool InAuthoritativeMode() {
            return (NetworkServerMode.Authoritative == this.serverWorkingMode);
        }

        /// <summary>
        /// Retrieves the current working mode of the server.
        /// </summary>
        /// <returns>A NetworkServerMode value representing the server's working mode.</returns>
        public bool IsServerModeDisabled() {
            return (NetworkServerMode.ClientOnly == this.serverWorkingMode);
        }

        /// <summary>
        /// Determines if there is a connection to the relay server.
        /// </summary>
        /// <returns>True if connected to the relay server, otherwise false.</returns>
        public NetworkServerMode GetServerWorkingMode() {
            return this.serverWorkingMode;
        }

        /// <summary>
        /// Determines if there is a connection to the lobby server.
        /// </summary>
        /// <returns>True if connected to the lobby server, otherwise false.</returns>
        public bool IsConnectedOnRelayServer() {
            return this.isConnectedOnRelayServer;
        }

        /// <summary>
        /// Checks if the current instance is connected to the lobby server.
        /// </summary>
        /// <returns>True if connected to the lobby server, otherwise false.</returns>
        public bool IsConnectedOnLobbyServer() {
            return this.isConnectedOnLobbyServer;
        }

        /// <summary>
        /// Sets the connection status for both relay and lobby servers.
        /// </summary>
        /// <param name="relayServer">The connection status for the relay server.</param>
        /// <param name="lobbyServer">The connection status for the lobby server.</param>
        public void SetConnectedOnRelayServer(bool relayServer, bool lobbyServer) {
            this.isConnectedOnRelayServer = relayServer;
            this.isConnectedOnLobbyServer = lobbyServer;
        }

        /// <summary>
        /// Return if encryption mode is enabled
        /// </summary>
        /// <returns>True is enabled, false otherwise</returns>
        public bool IsEncryptionEnabled() {
            return this.encryptionEnabled;
        }

        /// <summary>
        /// Return if ownership operations is allowed
        /// </summary>
        /// <returns>True if ownership is allowed</returns>
        public bool IsOwnerShipAllowed() {
            return (OwnerShipServerMode.Allowed == this.ownerShipMode);
        }

        /// <summary>
        /// Return if ownership operations is disabled
        /// </summary>
        /// <returns>True if ownership is disabled</returns>
        public bool IsOwnerShipDisabled() {
            return (OwnerShipServerMode.Disabled == this.ownerShipMode);
        }

        /// <summary>
        /// Return if ownership operations is disabled
        /// </summary>
        /// <returns>True if ownership is controlled by prefab definition</returns>
        public bool IsOwnerShipUsingPrefab() {
            return (OwnerShipServerMode.PrefabDefinition == this.ownerShipMode);
        }

        /// <summary>
        /// Return the maximum allowd connected clients ( CCU ) at same time
        /// </summary>
        /// <returns>True if ownership is controlled by prefab definition</returns>
        public uint GetMaximumConnectedClients() {
            return (this.enableCCULimit) ? this.maximunConnectedCount : uint.MaxValue;
        }

        /// <summary>
        /// Configure the CCU Behaviour on the server side
        /// </summary>
        /// <param name="ccuEnabled">enabled CCU</param>
        /// <param name="maximunOfPlayers">Maximum of the connected user simultaneously</param>
        public void ConfigureServerCCU(bool ccuEnabled, uint maximunOfPlayers) {
            this.enableCCULimit         = ccuEnabled;
            this.maximunConnectedCount  = maximunOfPlayers;
        }

        /// <summary>
        /// Register a new client that need receive an update of reliable variables
        /// </summary>
        /// <param name="client">Client o update</param>
        public void RegisterClientToUpdateVariables(IClient client) {
            if (client != null) {
                if (this.clientsToUpdateVariables.ContainsKey(client) == false) {
                    this.clientsToUpdateVariables.Add(client, Time.frameCount);
                } else {
                    this.clientsToUpdateVariables[client] = Time.frameCount;
                }
            }
        }

        /// <summary>
        /// Return clients that is out of date and  need to receive update of relianle variables
        /// </summary>
        /// <returns>Array containing pending clients to update</returns>
        public List<KeyValuePair<IClient, int>> GetClientsToUpdateVariables(int frameTick) {
            this.variablesToUpdateResultMap.Clear(); // Clear to ensure that no duplicated elements will be returned
            // Now check to populate list
            foreach (var element in this.clientsToUpdateVariables) {
                if (element.Value > frameTick) {
                    this.variablesToUpdateResultMap.Add(element);
                }
            }
            return this.variablesToUpdateResultMap;
        }
        
        /// <summary>
        /// Remove already updated clients that require variables update
        /// </summary>
        public void RemoveUpdatedClientsToUpdateVariables() {
            foreach (IClient client in this.clientsToUpdateVariables.Keys) {
                if ((client == null) ||
                    (client.GetTransport() == null) ||
                    ((client.GetTransport() != null) && (client.GetTransport().IsConnected() == false)) ||
                    (this.clientsToUpdateVariables[client] < (Time.frameCount - DELAY_TO_REMOVE_RELIABLE_UPDATE))) {
                    this.recicledClients.Add(client);
                }
            }
            // Removed recicled clients
            while (this.recicledClients.Count > 0) {
                this.clientsToUpdateVariables.Remove(this.recicledClients[0]);
                this.recicledClients.RemoveAt(0);
            }
        }

        /// <summary>
        /// Return method used to encript data 
        /// 
        /// Note: This method is configured by user on NetworkManager object
        /// </summary>
        /// <returns>Return the encryption method defined by user</returns>
        public Func<byte[], byte[]> GetEncryptionMethod() {
            Func<byte[], byte[]> result = null;
            if (this.encryptionComponent != null) {
                MethodInfo encryptionMethod = this.encryptionComponent.GetType().GetMethod(this.encryptionMethod);
                result = (byte[] inData) => {
                    return (encryptionMethod.Invoke(this.encryptionComponent, new object[] { inData }) as byte[]);
                };
            } else {
                result = null;
            }
            return result;
        }

        /// <summary>
        /// Return method used to decript data 
        /// 
        /// Note: This method is configured by user on NetworkManager object
        /// </summary>
        /// <returns>Return the decryption method defined by user</returns>
        public Func<byte[], byte[]> GetDecryptionMethod() {
            Func<byte[], byte[]> result = null;
            if (this.encryptionComponent != null) {
                MethodInfo decriptionMethod = this.encryptionComponent.GetType().GetMethod(this.decryptionMethod);
                result = (byte[] inData) => {
                    return (decriptionMethod.Invoke(this.encryptionComponent, new object[] { inData }) as byte[]);
                };
            } else {
                result = null;
            }
            return result;
        }

        /// <summary>
        /// Updates the master player status and potentially reactivates network elements.
        /// </summary>
        /// <param name="value">The new master status to set.</param>
        public void UpdateMasterPlayer(bool value) {
            if (this.HasLocalPlayer()) {
                this.GetLocalPlayer<IPlayer>().SetMaster(value);
                // If this instance start to be master, i need to updated elements
                if ( this.IsMasterPlayer() == true ) {
                    foreach(INetworkElement element in NetworkManager.Container.GetElements() ) {
                        if ( element.IsPassive() ) {
                            // Player shall not be active on master player, is only active in his own instance
                            NetworkObject networkObject = element.GetGameObject().GetComponent<NetworkObject>();
                            if ((element.IsPlayer() == false) && (networkObject.IsPlayer() == false)) {
                                networkObject.SetBehaviorMode(BehaviorMode.Active);
                                // Reactivate script according original prefab 
                                NetworkScriptsReference scriptsReference = element.GetGameObject().GetComponent<NetworkScriptsReference>();
                                if (scriptsReference != null) {
                                    scriptsReference.EnableComponents();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update the current transport system of alreadsy cached elements
        /// 
        /// Note: This is usefull when playter leave from being server to became client or vice-versa
        /// </summary>
        /// <param name="transport">New transport system</param>
        public void UpdateTransportOnElements(NetworkTransport transport) {
            foreach (INetworkElement element in NetworkManager.GetContainer().GetElements()) {
                if (element.GetNetworkObject() is NetworkObject) {
                    (element.GetNetworkObject() as NetworkObject).SetTransport(transport);
                }
            }
        }

        /// <summary>
        /// Retrieves an array of managed inputs.
        /// </summary>
        /// <returns>An array of ManagedInput objects.</returns>
        public ManagedInput[] GetManagedInputs() {
            return this.inputList.ToArray<ManagedInput>();
        }

        /// <summary>
        /// Gets the current value of an input by invoking the corresponding method.
        /// </summary>
        /// <typeparam name="T">The expected return type of the method.</typeparam>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <returns>The current value of the input.</returns>
        public T GetCurrentInputValue<T>(string methodName) {
            return (T)this.inputComponent.GetType().GetMethod(methodName).Invoke(this.inputComponent, null);
        }

        /// <summary>
        /// Determines if the local player is the master player.
        /// </summary>
        /// <returns>True if the local player is the master player, otherwise false.</returns>
        public bool IsMasterPlayer() {
            return (this.HasLocalPlayer()) ? this.GetLocalPlayer<IPlayer>().IsMaster() : 
                                            (this.HasPlayers() && this.HasMasterPlayer()) ? this.GetMasterPlayer<IPlayer>().IsLocal() : false;
        }

        /// <summary>
        /// Return if this instance became host
        /// </summary>
        /// <returns>True if bewcame host</returns>
        public bool isHostMigrated() {
            return this.hostMigratedToHere;
        }

        /// <summary>
        /// Set if this instance became host
        /// </summary>
        /// <param name="value">Host value</param>
        public void SetHostMigrated(bool value) {
            this.hostMigratedToHere = value;
        }

        /// <summary>
        /// Registers a network client with an associated network element.
        /// </summary>
        /// <param name="client">The client to register.</param>
        /// <param name="element">The network element associated with the client.</param>
        public void RegisterNetworkClient(IClient client, INetworkElement element) {
            if ( this.clients.ContainsKey(client) == false ) {
                this.clients.Add(client, element);
            } else {
                this.clients[client] = element;
            }
        }

        /// <summary>
        /// Registers a network player.
        /// </summary>
        /// <param name="player">The player to register.</param>
        public void RegisterNetworkPlayer(IPlayer player) {
            if ( !this.players.ContainsKey(player.GetPlayerId()) ) {
                this.players.Add(player.GetPlayerId(), player);
            }
        }

        /// <summary>
        /// Registers a network player with an optional client.
        /// </summary>
        /// <param name="id">The player's ID.</param>
        /// <param name="name">The player's name.</param>
        /// <param name="client">The optional client associated with the player.</param>
        public void RegisterNetworkPlayer(ushort id, string name, IClient client = null) {
            if ( !this.players.ContainsKey(id) ) {
                this.players.Add(id, new NetworkPlayer(id, name, client));
            }
        }

        /// <summary>
        /// Unregisters a network player.
        /// </summary>
        /// <param name="player">The player to unregister.</param>
        public void UnregisterNetworkPlayer(IPlayer player) {
            if ( this.players.ContainsKey(player.GetPlayerId()) ) {
                this.players.Remove(player.GetPlayerId());
            }
        }

        /// <summary>
        /// Registers a new network player for a given client and assigns a unique player ID.
        /// </summary>
        /// <param name="client">The client associated with the new player.</param>
        /// <returns>The newly registered player.</returns>
        public IPlayer RegisterNetworkPlayer(IClient client, ushort playerId = 0) {
            IPlayer newPlayer = new NetworkPlayer((playerId == 0) ? ++this.currentPlayerId : playerId, String.Format("Player_{0}", (playerId == 0) ? this.currentPlayerId : playerId), client);
            this.players.Add(newPlayer.GetPlayerId(), newPlayer);
            return newPlayer;
        }

        /// <summary>
        /// Registers a new network player for a given client and assigns a unique player ID.
        /// </summary>
        /// <param name="channel">The channel associated with the new player.</param>
        /// <returns>The newly registered player.</returns>
        public IPlayer RegisterNetworkPlayer(IChannel channel) {
            IPlayer newPlayer = new NetworkPlayer(++this.currentPlayerId, String.Format("Player_{0}", this.currentPlayerId), channel);
            this.players.Add(newPlayer.GetPlayerId(), newPlayer);
            return newPlayer;
        }

        /// <summary>
        /// Checks if a network player is associated with a given client.
        /// </summary>
        /// <param name="client">The client to check for an associated player.</param>
        /// <returns>True if a player is associated with the client, otherwise false.</returns>
        public bool HasNetworkPlayer(IClient client) {
            bool result = false;
            foreach (IPlayer player in this.players.Values) {
                if (client.Equals((player as NetworkPlayer).GetClient())) {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Determines if a local player exists.
        /// </summary>
        /// <returns>True if a local player exists, otherwise false.</returns>
        public bool HasLocalPlayer() {
            return (this.GetLocalPlayer<IPlayer>() != null);
        }

        /// <summary>
        /// Determines if has players.
        /// </summary>
        /// <returns>True if has any players, otherwise false.</returns>
        public bool HasPlayers() {
            return (this.GetPlayersCount() > 0);
        }

        /// <summary>
        /// Retrieves a player by their ID.
        /// </summary>
        /// <typeparam name="T">The type of player to retrieve.</typeparam>
        /// <param name="id">The player's ID.</param>
        /// <returns>The player with the specified ID.</returns>
        public T GetPlayer<T>(ushort id) where T : IPlayer {
            return (this.players.ContainsKey(id)) ? (T)this.players[id] : default(T);
        }

        /// <summary>
        /// Retrieves the local player.
        /// </summary>
        /// <typeparam name="T">The type of player to retrieve.</typeparam>
        /// <returns>The local player.</returns>
        public T GetLocalPlayer<T>() where T : IPlayer {
            IPlayer result = null;
            foreach ( IPlayer player in this.players.Values ) {
                if (player.IsLocal()) {
                    result = player;
                    break;
                }
            }
            return (T)result;
        }

        /// <summary>
        /// Retrieves a player associated with a given client.
        /// </summary>
        /// <typeparam name="T">The type of player to retrieve.</typeparam>
        /// <param name="client">The client associated with the player.</param>
        /// <returns>The player associated with the specified client.</returns>
        public T GetPlayer<T>(IClient client) where T : IPlayer {
            IPlayer result = null;
            foreach (IPlayer player in this.players.Values) {
                if (player != null) {
                    if ((player as NetworkPlayer).GetClient() != null) {
                        if ((player as NetworkPlayer).GetClient().Equals(client)) {
                            result = player;
                            break;
                        }
                    }
                }
            }
            return (T)result;
        }

        /// <summary>
        /// Retrieves the master player for a given lobby.
        /// </summary>
        /// <typeparam name="T">The type of player to retrieve.</typeparam>
        /// <param name="lobby">The lobby ID to search within.</param>
        /// <returns>The master player for the specified lobby.</returns>
        public T GetMasterPlayer<T>(ushort lobby = 0) where T : IPlayer {
            IPlayer result = null;
            foreach (IPlayer player in this.players.Values) {
                if (player.GetLobbyId().Equals(lobby)) {
                    if (player.IsMaster()) {
                        result = player;
                        break;
                    }
                }
            }
            return (T)result;
        }

        /// <summary>
        /// Retrieves a list of players for a given lobby.
        /// </summary>
        /// <typeparam name="T">The type of players to retrieve.</typeparam>
        /// <param name="lobby">The lobby ID to search within.</param>
        /// <returns>A list of players for the specified lobby.</returns>
        public List<T> GetPlayers<T>(ushort lobby = 0) where T : IPlayer {
            List<T> result = new List<T>();
            foreach (IPlayer player in this.players.Values) {
                if (player.GetLobbyId().Equals(lobby)) {
                    result.Add((T)player);
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves all nertwork players.
        /// </summary>
        /// <typeparam name="T">The type of player to retrieve.</typeparam>
        /// <returns>All registered network players.</returns>
        public T[] GetNetworkPlayersElements<T>() where T : INetworkElement {
            List<T> result = new List<T>();
            foreach (INetworkElement el in NetworkManager.Container.GetElements()) {
                if (el.IsPlayer()) {
                    result.Add((T)el);
                }
                
            }
            return (T[])result.ToArray();
        }

        /// <summary>
        /// Retrieves network players with a specific id.
        /// </summary>
        /// <typeparam name="T">The type of player to retrieve.</typeparam>
        /// <param name="networkId">Network id to find</param>
        /// <returns>Network element with this specific ID</returns>
        public T GetNetworkPlayerElement<T>(int networkId) where T : INetworkElement {
            T result = default(T);
            foreach (INetworkElement el in NetworkManager.Container.GetElements()) {
                if (el.IsPlayer()) {
                    if (el.GetNetworkId().Equals(networkId)) {
                        result = (T)el;
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// Retrieves the first non-master player from the specified lobby.
        /// </summary>
        /// <typeparam name="T">The type of player to return, must implement IPlayer.</typeparam>
        /// <param name="lobby">The lobby ID to search within, default is 0.</param>
        /// <returns>The first non-master player of type T in the specified lobby, or default(T) if none found.</returns>
        public T GetFirstNoMasterPlayers<T>(ushort lobby = 0) where T : IPlayer {
            T result = default(T);
            foreach (IPlayer player in this.players.Values) {
                if (player.GetLobbyId().Equals(lobby)) {
                    if (player.IsMaster() == false) {
                        result = (T)player;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if there is a master player in the specified lobby.
        /// </summary>
        /// <param name="lobby">The lobby ID to check, default is 0.</param>
        /// <returns>True if a master player exists in the lobby, otherwise false.</returns>
        public bool HasMasterPlayer(ushort lobby = 0) {
            return (this.GetMasterPlayer<IPlayer>(lobby) != null);
        }

        /// <summary>
        /// Determines if there are any players in the specified lobby.
        /// </summary>
        /// <param name="lobby">The lobby ID to check, default is 0.</param>
        /// <returns>True if there are players in the lobby, otherwise false.</returns>
        public bool HasPlayers(ushort lobby = 0) {
            return (this.GetPlayers<IPlayer>(lobby).Count > 0);
        }

        /// <summary>
        /// Gets the count of players in the specified lobby.
        /// </summary>
        /// <param name="lobby">The lobby ID to check, default is 0.</param>
        /// <returns>The number of players in the lobby.</returns>
        public int GetPlayersCount(ushort lobby = 0) {
            return this.GetPlayers<IPlayer>(lobby).Count;
        }

        /// <summary>
        /// Retrieves a local player element on this instance.
        /// </summary>
        /// <typeparam name="T">The type of player to retrieve.</typeparam>
        /// <returns>The network object associated with local player.</returns>
        public T GetLocalPlayerElement<T>() where T : INetworkElement {
            INetworkElement result = null;
            foreach (INetworkElement el in NetworkManager.Container.GetElements()) {
                if (el.IsPlayer() && el.IsOwner()) {
                    result = el;
                    break;
                }
            }
            return (T)result;
        }

        /// <summary>
        /// Retrieves a local GameObject associate with player.
        /// </summary>
        /// <returns>Local player GameObject.</returns>
        public GameObject GetLocalPlayerObject() {
            return this.GetLocalPlayerElement<INetworkElement>().GetGameObject();
        }

        /// <summary>
        /// Retrieves a local player element using is network id.
        /// </summary>
        /// <typeparam name="T">The type of player to retrieve.</typeparam>
        /// <param name="networkId">Network id to find</param>
        /// <returns>The network object associated with local player.</returns>
        public T GetPlayerElement<T>(int networkId) where T : INetworkElement {
            INetworkElement result = null;
            foreach (INetworkElement el in NetworkManager.Container.GetElements()) {
                if (el.IsPlayer()) {
                    if (el.GetNetworkId() == networkId) {
                        result = el;
                        break;
                    }
                }
            }
            return (T)result;
        }

        /// <summary>
        /// Return array containing all connected clients
        /// </summary>
        /// <typeparam name="T">Generic argument</typeparam>
        /// <returns></returns>
        public IClient[] GetConnectedClients<T>() where T : IClient {
            return this.clients.Keys.ToArray<IClient>();
        }

        /// <summary>
        /// Return Network element from a specified client
        /// </summary>
        /// <typeparam name="T">Generic argument</typeparam>
        /// <param name="client">Client for check</param>
        /// <returns></returns>
        public T GetClient<T>(IClient client) where T : IClient {
            return (T)this.clients[client];
        }

        /// <summary>
        /// RFeturn client from an specific element
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="element">Element to check</param>
        /// <returns></returns>
        public T GetClient<T>(INetworkElement element) where T : IClient {
            T result = default(T);
            foreach (var elementInList in this.clients) {
                if (elementInList.Value.Equals(element)) {
                    result = (T)elementInList.Key;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if auto-reconnect is disabled.
        /// </summary>
        /// <returns>True if auto-reconnect is disabled, otherwise false.</returns>
        public bool IsAutoReconnectDisabled() {
            return this.disableReconnect;
        }

        /// <summary>
        /// Determines if lobby control is enabled.
        /// </summary>
        /// <returns>True if lobby control is enabled and the server is in relay mode, otherwise false.</returns>
        public bool IsLobbyControlEnabled() {
            return  this.useLobbyManager && 
                    (NetworkServerMode.Relay == this.serverWorkingMode); // Lobby works only into relay mode
        }

        /// <summary>
        /// Checks if remote input is enabled.
        /// </summary>
        /// <returns>True if remote input is enabled, otherwise false.</returns>
        public bool IsRemoteInputEnabled() {
            return  this.useRemoteInput;
        }

        /// <summary>
        /// Enable automatic spawn scene elements player connect
        /// </summary>        
        public void EnableAutoLoadSceneElements() {
            this.spawnSceneElements = true;
        }

        /// <summary>
        /// Disable automatic spawn scene elements player connect
        /// </summary>        
        public void DisableAutoLoadSceneElements() {
            this.spawnSceneElements = false;
        }

        /// <summary>
        /// Return if automatic spawn scene elements player connect is enabled
        /// </summary>
        /// <returns>True if is enabled, otherwise false</returns>
        public bool IsToAutoLoadSceneElements() {
            return this.spawnSceneElements;
        }

        /// <summary>
        /// Disconnect a client
        /// 
        /// Note: Use this method to kick any player from game
        /// </summary>
        /// <param name="client">Client to disconnect</param>
        public void DisconnectClient(IClient client) {
            client.GetTransport().Disconnect();
        }


        /// <summary>
        /// Request server to send all network elements in scene to this player
        /// </summary>
        public void RequestRemoteSceneElements() {
            NetworkElement playerElement = this.GetLocalPlayerElement<NetworkElement>();
            // Request server to send all elements
            using (DataStream writer = new DataStream()) {
                writer.Write(playerElement.GetNetworkId());
                this.Send(CoreGameEvents.RequestSceneObjects, writer, DeliveryMode.Reliable);
            }
        }

        /// <summary>
        /// Request server to send all network elements in scene to this player
        /// </summary>
        public void RequestNetworkVariablesUpdate() {
            NetworkElement playerElement = this.GetLocalPlayerElement<NetworkElement>();
            // Request server to send all elements
            using (DataStream writer = new DataStream()) {
                writer.Write(playerElement.GetNetworkId());
                this.Send(CoreGameEvents.RequestVariablesUpdate, writer, DeliveryMode.Reliable);
            }
        }

        /// <summary>
        /// Make all connected client to load some scene
        /// Note: This will be used on server to execute RemoteLoadScene
        /// </summary>
        /// <param name="sceneName">Scene to load</param>
        /// <param name="serverLoadMode">When server will load scene ( Before request clients or after )</param>
        /// <param name="loadMode">Mode to load scene</param>
        /// <param name="loadTimeout">Timeout to load operation on the client side</param>
        public void LoadSceneRemote(string sceneName, RemoteSceneLoadMode serverLoadMode = RemoteSceneLoadMode.LoadAfter, LoadSceneMode loadMode = LoadSceneMode.Single, float loadTimeout = 10f) {
            Scene sceneToLoad = SceneManager.GetSceneByName(sceneName);
            if (sceneToLoad != null) {
                if (this.IsRunningLogic()) {
                    foreach (var client in this.clients.Keys) {
                        if (this.clientsLoadingScene.ContainsKey(client) == false) {
                            // Send load scene event
                            using (DataStream writer = new DataStream()) {
                                writer.Write(sceneName);
                                writer.Write((byte)loadMode);
                                // Send message
                                client.Send(CoreGameEvents.RemoteLoadScene, writer, DeliveryMode.Reliable);
                            }
                            // Register wait operation and the current time to check tmeout
                            this.clientsLoadingScene.Add(client, Time.time + loadTimeout);
                        }
                    }
                    // Store loading mode
                    this.remoteSceneLoadingMode = serverLoadMode;
                    // The will load scene
                    if (RemoteSceneLoadMode.LoadFirst == serverLoadMode) {
                        StartCoroutine(this.LoadAsyncScene(sceneName, loadMode));
                    } else if (this.clients.Count > 0) {
                        this.delayedServerLoadUnloadScene = () => {
                            StartCoroutine(this.LoadAsyncScene(sceneName, loadMode, true));
                        };
                    } else {
                        StartCoroutine(this.LoadAsyncScene(sceneName, loadMode, true));
                    }
                    // Need to pause disconnect detection during the loading scene phase to avoid timeout disconnections
                    this.PauseDisconnectDetection(this.pauseDisconnectionOnSceneLoad);
                } else if (this.allowPlayersRequestSceneLoad == true) {
                    // Send load scene event
                    using (DataStream writer = new DataStream()) {
                        writer.Write(sceneName);
                        writer.Write((byte)loadMode);
                        // Send message
                        this.Send(CoreGameEvents.RequestRemoteSceneLoad, writer, DeliveryMode.Reliable);
                    }
                    NetworkDebugger.LogDebug("Remote scene load requested [{0}]", sceneName);
                } else {
                    NetworkDebugger.LogError("You can't request to load a scene from a client connection, to enable this option, you must check \"Enable client scene load\" on the network manager", sceneName);
                }
            } else {
                NetworkDebugger.LogError("The scene [{0}] doesnt exists or isn't registered on the scene in build. Go to \"File > Build Settings\" and register this scene", sceneName);
            }
        }

        /// <summary>
        /// Make all connected client to unload some loaded scene
        /// Note: This will be use d on server to execute RemoteLoadScene
        /// </summary>
        /// <param name="sceneName">Scene to load</param>
        /// <param name="serverLoadMode">When server will load scene ( Before request clients or after )</param>
        /// <param name="unloadMode">Mode to load scene</param>
        /// <param name="loadTimeout">Timeout to load operation on the client side</param>
        public void UnloadSceneRemote(string sceneName, RemoteSceneUnloadMode serverUnloadMode = RemoteSceneUnloadMode.UnloadAfter, float loadTimeout = 10f) {
            Scene sceneToLoad = SceneManager.GetSceneByName(sceneName);
            if (sceneToLoad != null) {
                if (this.IsRunningLogic()) {
                    foreach (var client in this.clients.Keys) {
                        if (this.clientsLoadingScene.ContainsKey(client) == false) {
                            // Send load scene event
                            using (DataStream writer = new DataStream()) {
                                writer.Write(sceneName);
                                // Send message
                                client.Send(CoreGameEvents.RemoteUnLoadScene, writer, DeliveryMode.Reliable);
                            }
                            // Register wait operation and the current time to check tmeout
                            this.clientsLoadingScene.Add(client, Time.time + loadTimeout);
                        }
                    }
                    // Store loading mode
                    this.remoteSceneUnloadMode = serverUnloadMode;
                    // The will load scene
                    if (RemoteSceneUnloadMode.UnloadFirst == serverUnloadMode) {
                        StartCoroutine(this.UnLoadAsyncScene(sceneName));
                    } else if (this.clients.Count > 0) {
                        this.delayedServerLoadUnloadScene = () => {
                            StartCoroutine(this.UnLoadAsyncScene(sceneName, true));
                        };
                    } else {
                        StartCoroutine(this.UnLoadAsyncScene(sceneName, true));
                    }
                    // Need to pause disconnect detection during the loading scene phase to avoid timeout disconnections
                    this.PauseDisconnectDetection();
                } else if (this.allowPlayersRequestSceneLoad == true) {
                    // Send load scene event
                    using (DataStream writer = new DataStream()) {
                        writer.Write(sceneName);
                        // Send message
                        this.Send(CoreGameEvents.RequestRemoteSceneUnLoad, writer, DeliveryMode.Reliable);
                    }
                    NetworkDebugger.LogDebug("Remote scne load requested [{0}]", sceneName);
                } else {
                    NetworkDebugger.LogError("You can't request to unload a scene from a client connection, to enable this option, you must check \"Enable client scene load\" on the network manager", sceneName);
                }
            } else {
                NetworkDebugger.LogError("The scene [{0}] doesnt exists or isn't registered on the scene in build. Go to \"File > Build Settings\" and register this scene", sceneName);
            }
        }

        /// <summary>
        /// Unregister some pending client who is waiting for a scene to be loaded
        /// </summary>
        /// <param name="client">Client to unregister</param>
        /// <returns>True if all clients finished to load scene, if has any client pending to read will return false</returns>
        public bool UnregisterRemoteLoadingScene(IClient client) {
            // Remove from waiting list
            if (this.clientsLoadingScene.ContainsKey(client)) {
                this.clientsLoadingScene.Remove(client);
            }
            // If all clients loaded his scenes i'm going to load scene on server
            return (this.clientsLoadingScene.Count == 0);
        }

        /// <summary>
        /// Finis the remote scene loading operation
        /// </summary>
        public void FinishLoadingSceneOperation() {
            // The will load scene
            if (RemoteSceneLoadMode.LoadAfter == this.remoteSceneLoadingMode) {
                this.delayedServerLoadUnloadScene.Invoke();
            }
        }

        /// <summary>
        /// Check if has some scne pending to be loaded with timeout
        /// </summary>
        public void CheckRemoteSceneLoadTimeout() {
            if (this.clientsLoadingScene.Count > 0) {
                List<IClient> removedClients = new List<IClient>();
                IClient[] clients = this.clientsLoadingScene.Keys.ToArray();
                foreach(IClient client in clients) {
                    if (this.clientsLoadingScene[client] < Time.time) {
                        this.clientsLoadingScene.Remove(client);
                        removedClients.Add(client);
                    }
                }
                // All clients was removed i'm going to disconnect clients and load load scene
                if (this.clientsLoadingScene.Count == 0) { 
                    while (removedClients.Count > 0) {
                        IClient clientToDisconnect = removedClients[0];
                        removedClients.RemoveAt(0);
                        // Disconnect this client
                        clientToDisconnect.GetTransport().Disconnect();
                    }
                    // Load server scene
                    if ((RemoteSceneLoadMode.LoadAfter.Equals(this.remoteSceneLoadingMode)) ||
                        (RemoteSceneUnloadMode.UnloadAfter.Equals(this.remoteSceneUnloadMode))) {
                        if (this.delayedServerLoadUnloadScene != null) {
                            Action delayedAction = this.delayedServerLoadUnloadScene;
                            this.delayedServerLoadUnloadScene = null;
                            delayedAction.Invoke();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load scene on server side
        /// </summary>
        /// <param name="sceneIndex">Scene index</param>
        /// <param name="aditive">Load in aditive mode</param>
        /// <returns></returns>
        private IEnumerator LoadAsyncScene(string sceneName, LoadSceneMode loadMode, bool notifyClients = false) {
            // Flag scene is loading
            NetworkManager.Instance().FlagLoadingSceneStarted();
            // Need to repeat to ensure that objects is synchonized
            NetworkManager.Instance().FlagInSceneObjectsNotRequested();
            // Execute scene load
            this.StartRemoteAsyncSceneLoding(SceneManager.LoadSceneAsync(sceneName, loadMode));
            // Wait until the asynchronous scene fully loads
            while (!this.IsAsyncSceneLoadFinished()) {
                yield return null;
            }
            // Notify all clients that everybody has his scene loaded ( including server )
            if (notifyClients) {
                foreach (var client in this.clients.Keys) {
                    using (DataStream writer = new DataStream()) {
                        writer.Write(sceneName);
                        client.Send(CoreGameEvents.RemoteSceneLoadFinished, writer, DeliveryMode.Reliable);
                    }
                }
            }
            // Flag scene loading finihed
            NetworkManager.Instance().FlagLoadingSceneFinished();
            // Activate the previous loaded scene
            NetworkManager.Instance().ActivateRemoteScene();
            // Notify server either
            this.RemoteSceneLoadFinishedEvent(sceneName);
            // Resume disconnect detection due loadign scene finished
            this.ResumeDisconnectDetection();
        }

        /// <summary>
        /// Load scene on server side
        /// </summary>
        /// <param name="sceneIndex">Scene index</param>
        /// <param name="aditive">Load in aditive mode</param>
        /// <returns></returns>
        private IEnumerator UnLoadAsyncScene(string sceneName, bool notifyClients = false) {
            // Flag scene is loading
            NetworkManager.Instance().FlagLoadingSceneStarted();
            // Need to repeat to ensure that objects is synchonized
            NetworkManager.Instance().FlagInSceneObjectsNotRequested();
            // Execute scene load
            this.StartRemoteAsyncSceneLoding(SceneManager.UnloadSceneAsync(sceneName));
            // Wait until the asynchronous scene fully loads
            while (!this.IsAsyncSceneLoadFinished()) {
                yield return null;
            }
            // Notify all clients that everybody has his scene loaded ( including server )
            if (notifyClients) {
                foreach (var client in this.clients.Keys) {
                    using (DataStream writer = new DataStream()) {
                        writer.Write(sceneName);
                        client.Send(CoreGameEvents.RemoteSceneUnLoadFinished, writer, DeliveryMode.Reliable);
                    }
                }
            }
            // Flag scene loading finihed
            NetworkManager.Instance().FlagLoadingSceneFinished();
            // Notify server either
            this.RemoteSceneUnLoadFinishedEvent(sceneName);
            // Resume disconnect detection due loadign scene finished
            this.ResumeDisconnectDetection();
        }

        /// <summary>
        /// Return IP adress ( works on Windows and Android )
        /// </summary>
        /// <returns>Local IP Address</returns>
        public String GetLocalIPByDNS() {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
                try {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint.Address.ToString();
                } catch (SocketException) {
                    return IPAddress.Loopback.ToString();
                }
            }
        }

        /// <summary>
        /// Retrieves the private IP address.
        /// </summary>
        /// <returns>The private IP address if available, otherwise "unknown".</returns>
        public String GetPrivateIp() {
            String result = "unknow";
            if (( this.useNatTraversal  == true) &&
                ( this.natHelper        != null ) &&
                ( (this.natHelper as NatHelper).PortOpened == true )) {
                result = (this.natHelper as NatHelper).PrivateIp;
            } else {
                if ( this.adapterAddress == null ) {
                    this.adapterAddress = NetworkUtils.GetLocaIpAddress();
                }
                result = this.adapterAddress;
            }
            if (string.IsNullOrEmpty(result)) {
                result = this.GetLocalIPByDNS();
            }
            return result;
        }

        /// <summary>
        /// Retrieves the public IP address.
        /// </summary>
        /// <returns>The public IP address if available, otherwise "unknown".</returns>
        public String GetPublicIp() {
            String result = "unknow";
            if (( this.useNatTraversal  == true) &&
                ( this.natHelper        != null ) &&
                ( (this.natHelper as NatHelper).PortOpened == true )) {
                result = (this.natHelper as NatHelper).PublicIp;
            } else {
                if ( this.adapterAddress == null ) {
                    this.adapterAddress = NetworkUtils.GetLocaIpAddress();
                }
                result = this.adapterAddress;
            }
            if (string.IsNullOrEmpty(result)) {
                result = this.GetLocalIPByDNS();
            }
            return result;
        }

        /// <summary>
        /// Checks if the router port is mapped.
        /// </summary>
        /// <returns>True if the router port is mapped, otherwise false.</returns>
        public bool IsRouterPortMapped() {
            bool result = false;
            if (( this.useNatTraversal  == true) &&
                ( this.natHelper        != null )) {
                result = (this.natHelper as NatHelper).PortOpened;
            }
            return result;
        }

        /// <summary>
        /// Gets the average ping time in milliseconds.
        /// </summary>
        /// <returns>The average ping time if applicable, otherwise -1.</returns>
        public int GetPingAverage() {
            const float MILLISECONDS_PING_MULTIPLIER = 1000.0f;
            int result = -1;
            if (NetworkConnectionType.Server == this.connectionType) {
                result = 0; // No ping between server and logic
            } else if (NetworkConnectionType.Client == this.connectionType) { 
                result = (int)(this.clientSocket.GetPingAverage() * MILLISECONDS_PING_MULTIPLIER);
            }
            return result;
        }

        /// <summary>
        /// Gets the average ping time in seconds.
        /// </summary>
        /// <returns>The average ping time in seconds.</returns>
        public float GetPingAverageTime() {
            const float MILLISECONDS_PING_MULTIPLIER = 1000.0f;
            return (this.GetPingAverage() / MILLISECONDS_PING_MULTIPLIER);
        }

        /// <summary>
        /// Checks if server restart detection is enabled.
        /// </summary>
        /// <returns>True if server restart detection is enabled, otherwise false.</returns>
        public bool IsServerRestartDetectionEnabled() {
            return this.detectServerRestart;
        }

        /// <summary>
        /// Retrieves the active network transport entry.
        /// </summary>
        /// <returns>The active NetworkTransportEntry.</returns>
        public NetworkTransportEntry GetActiveTransport() {
            return this.transportsDatabase.GetActiveTransport();
        }

        /// <summary>
        /// Retrieves the current index factory value
        /// </summary>
        /// <returns>Current index.</returns>
        public ushort GetCurrentPlayerIndexFactory() {
            return this.playerIndexFactory;
        }

        /// <summary>
        /// Update the current player index id
        /// </summary>
        /// <param name="value">Index id</param>
        public void UpdateCurrentPlayerIndexFactory(ushort value) {
            this.playerIndexFactory = value;
        }

        // <summary>
        /// Determines whether [peer to peer is supported].
        /// </summary>
        /// <returns><c>true</c> if [peer to peer is supported]; otherwise, <c>false</c>.</returns>
        public bool IsPeerToPeerSupported() {
            Type transportClassType = Type.GetType(this.transportsDatabase.GetActiveTransport().GetClient());
            var attribute = transportClassType.GetCustomAttributes(typeof(TransportType), true).FirstOrDefault() as TransportType;
            // Return if double channel is configured
            return ((attribute == null) || (attribute.IsPeerToPeerSupported()));
        }
        
        /// <summary>
        /// Gets the current frames per second (FPS), clamped to the maximum allowed FPS.
        /// </summary>
        /// <returns>The current FPS, clamped to the allowed range.</returns>
        public int GetCurrentFPS() {
            return Mathf.Clamp(this.currentFramePeerSecond, 1, MAX_ALLOWED_FPS);
        }

        /// <summary>
        /// Calculates the average frame rate over a range of collected frame rates.
        /// </summary>
        /// <returns>The average frame rate as a byte.</returns>
        private byte InternalCalculateFrameRate() {
            int     sum         = 0;
            byte    avearage    = 0;
            float   frameDelay  = ( 1.0f / Time.unscaledDeltaTime );
            int     fps         = (byte)Mathf.RoundToInt(frameDelay);
            this.collectedFrameRates.Add(fps);
            while (this.collectedFrameRates.Count > this.frameRateAveareageRange) {
                this.collectedFrameRates.RemoveAt(0);
            }
            for (int i = 0; i < this.collectedFrameRates.Count; i++) {
                sum += this.collectedFrameRates[i];
            }
            avearage = (byte)Mathf.RoundToInt( sum / this.collectedFrameRates.Count );
            return avearage;
        }
    }
}
#pragma warning restore