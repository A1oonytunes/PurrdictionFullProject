using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    /// <summary>
    /// Class NetworkManagerEditor.
    /// Implements the <see cref="Editor" />
    /// Implements the <see cref="com.onlineobject.objectnet.IDatabaseTargetEditor" />
    /// </summary>
    /// <seealso cref="Editor" />
    /// <seealso cref="com.onlineobject.objectnet.IDatabaseTargetEditor" />
    [CustomEditor(typeof(NetworkManager))]
    [CanEditMultipleObjects]
    public class NetworkManagerEditor : Editor, IDatabaseTargetEditor {

        /// <summary>
        /// The network manager
        /// </summary>
        NetworkManager networkManager;

        /// <summary>
        /// Flag if this object will not be destroyed when scene changes
        /// </summary>
        SerializedProperty dontDestroyOnLoad;

        /// <summary>
        /// The database target
        /// </summary>
        SerializedProperty databaseTarget;

        /// <summary>
        /// The server working mode
        /// </summary>
        SerializedProperty serverWorkingMode;

        /// <summary>
        /// The use peer to peer
        /// </summary>
        SerializedProperty usePeerToPeer;

        /// <summary>
        /// Port used on peer to peer connections
        /// </summary>
        SerializedProperty peerToPeerPort;

        /// <summary>
        /// System will use one port for each peer to peer client player
        /// </summary>
        SerializedProperty peerToPeerPortPeerPlayer;

        /// <summary>
        /// The use nat traversal
        /// </summary>
        SerializedProperty useNatTraversal;

        /// <summary>
        /// The use lobby manager
        /// </summary>
        SerializedProperty useLobbyManager;

        /// <summary>
        /// The server socket
        /// </summary>
        SerializedProperty serverSocket;

        /// <summary>
        /// The client socket
        /// </summary>
        SerializedProperty clientSocket;

        /// <summary>
        /// The connection type
        /// </summary>
        SerializedProperty connectionType;

        /// <summary>
        /// The automatic connect on start
        /// </summary>
        SerializedProperty autoConnectOnStart;

        /// <summary>
        /// The connection delay
        /// </summary>
        SerializedProperty connectionDelay;

        /// <summary>
        /// The asynchronous connect
        /// </summary>
        SerializedProperty asyncConnect;

        /// <summary>
        /// The automatic reconnect
        /// </summary>
        SerializedProperty autoReconnect;

        /// <summary>
        /// The reconnection delay
        /// </summary>
        SerializedProperty reconnectionDelay;

        /// <summary>
        /// The reconnection attemps
        /// </summary>
        SerializedProperty reconnectionAttemps;

        /// <summary>
        /// The idle timeut detection
        /// </summary>
        SerializedProperty idleTimeutDetection;

        /// <summary>
        /// The idle timeut detection
        /// </summary>
        SerializedProperty clientConnectDelay;

        /// <summary>
        /// The detect server restart
        /// </summary>
        SerializedProperty detectServerRestart;

        /// <summary>
        /// The use network login
        /// </summary>
        SerializedProperty useNetworkLogin;

        /// <summary>
        /// The login information provider object
        /// </summary>
        SerializedProperty loginInfoProviderObject;

        /// <summary>
        /// The login information component
        /// </summary>
        SerializedProperty loginInfoComponent;

        /// <summary>
        /// The login information method
        /// </summary>
        SerializedProperty loginInfoMethod;

        /// <summary>
        /// The login types method
        /// </summary>
        SerializedProperty loginTypesMethod;

        /// <summary>
        /// The enable login validation
        /// </summary>
        SerializedProperty enableLoginValidation;

        /// <summary>
        /// The login validation provider object
        /// </summary>
        SerializedProperty loginValidationProviderObject;

        /// <summary>
        /// The login validation component
        /// </summary>
        SerializedProperty loginValidationComponent;

        /// <summary>
        /// The login validation method
        /// </summary>
        SerializedProperty loginValidationMethod;

        /// <summary>
        /// The use network clock
        /// </summary>
        SerializedProperty useNetworkClock;

        /// <summary>
        /// The use movement interpolation
        /// </summary>
        SerializedProperty useInterpolation;

        /// <summary>
        /// The type of movement
        /// </summary>
        SerializedProperty movementType;

        /// <summary>
        /// The use movement prediction
        /// </summary>
        SerializedProperty useMovementPrediction;

        /// <summary>
        /// The prediction type
        /// </summary>
        SerializedProperty predictionType;

        /// <summary>
        /// The prediction buffer size
        /// </summary>
        SerializedProperty predictionBufferSize;

        /// <summary>
        /// The prediction factor
        /// </summary>
        SerializedProperty predictionFactor;

        /// <summary>
        /// The prediction speed factor
        /// </summary>
        SerializedProperty predictionSpeedFactor;

        /// <summary>
        /// The override prediction
        /// </summary>
        SerializedProperty overridePrediction;

        /// <summary>
        /// The prediction provider object
        /// </summary>
        SerializedProperty predictionProviderObject;

        /// <summary>
        /// The prediction component
        /// </summary>
        SerializedProperty predictionComponent;

        /// <summary>
        /// The use remote input
        /// </summary>
        SerializedProperty useRemoteInput;

        /// <summary>
        /// The use echo input
        /// </summary>
        SerializedProperty useEchoInput;

        /// <summary>
        /// The input provider object
        /// </summary>
        SerializedProperty inputProviderObject;

        /// <summary>
        /// The input component
        /// </summary>
        SerializedProperty inputComponent;

        /// <summary>
        /// The input list
        /// </summary>
        SerializedProperty inputList;

        /// <summary>
        /// The use internal ping
        /// </summary>
        SerializedProperty useInternalPing;

        /// <summary>
        /// The latency toolerante
        /// </summary>
        SerializedProperty latencyTolerance;

        /// <summary>
        /// The good latency value
        /// </summary>
        SerializedProperty goodLatencyValue;

        /// <summary>
        /// The acceptable latency value
        /// </summary>
        SerializedProperty acceptableLatencyValue;

        /// <summary>
        /// The use single servers
        /// </summary>
        SerializedProperty useSingleServers;

        /// <summary>
        /// The server address
        /// </summary>
        SerializedProperty serverAddress;

        /// <summary>
        // Use any binding as bindign service
        /// <summary>
        SerializedProperty useAnyAddress;

        /// <summary>
        // Use public IP Address defined by user
        /// <summary>
        SerializedProperty useFixedAddress;

        /// <summary>
        // Use public IP Address when start listening
        /// <summary>
        SerializedProperty usePublicAddress;

        /// <summary>
        // Use internal network IP Address when start listening
        /// <summary>
        SerializedProperty useInternalAddress;

        /// <summary>
        /// The server address
        /// </summary>
        SerializedProperty hostServerAddress;

        /// <summary>
        /// The TCP connection port
        /// </summary>
        SerializedProperty tcpConnectionPort;

        /// <summary>
        /// The UDP connection port
        /// </summary>
        SerializedProperty udpConnectionPort;

        /// <summary>
        /// The use multiple servers
        /// </summary>
        SerializedProperty useMultipleServers;

        /// <summary>
        /// The network servers
        /// </summary>
        SerializedProperty networkServers;

        /// <summary>
        /// The network servers enabled
        /// </summary>
        SerializedProperty networkServersEnabled;

        /// <summary>
        /// The use dynamic address
        /// </summary>
        SerializedProperty useDynamicAddress;

        /// <summary>
        /// The request servers object
        /// </summary>
        SerializedProperty requestServersObject;

        /// <summary>
        /// The request server component
        /// </summary>
        SerializedProperty requestServerComponent;

        /// <summary>
        /// The request server method
        /// </summary>
        SerializedProperty requestServerMethod;

        /// <summary>
        /// The enable player spawner
        /// </summary>
        SerializedProperty enablePlayerSpawner;

        /// <summary>
        /// The use internal network identifier
        /// </summary>
        SerializedProperty useInternalNetworkId;

        /// <summary>
        /// The use custom network identifier
        /// </summary>
        SerializedProperty useCustomNetworkId;

        /// <summary>
        /// The network identifier provider object
        /// </summary>
        SerializedProperty networkIdProviderObject;

        /// <summary>
        /// The network identifier component
        /// </summary>
        SerializedProperty networkIdComponent;

        /// <summary>
        /// The network identifier method
        /// </summary>
        SerializedProperty networkIdMethod;

        /// <summary>
        /// The send rate amount
        /// </summary>
        SerializedProperty sendRateAmount;

        /// <summary>
        /// The send rate amount
        /// </summary>
        SerializedProperty minimumRateAmount;

        /// <summary>
        /// The send update mode
        /// </summary>
        SerializedProperty sendUpdateMode;

        /// <summary>
        /// The control player cameras
        /// </summary>
        SerializedProperty controlPlayerCameras;

        /// <summary>
        /// The detach player camera
        /// </summary>
        SerializedProperty detachPlayerCamera;

        /// <summary>
        /// The despawn player on disconnect
        /// </summary>
        SerializedProperty despawnPlayerOnDisconnect;

        /// <summary>
        /// The despawn player after delay
        /// </summary>
        SerializedProperty despawnPlayerAfterDelay;

        /// <summary>
        /// The delay to remove disconnected
        /// </summary>
        SerializedProperty delayToRemoveDisconnected;

        /// <summary>
        /// The player spawn mode
        /// </summary>
        SerializedProperty playerSpawnMode;

        /// <summary>
        /// The player time spawn mode
        /// </summary>
        SerializedProperty playerSpawnTimeMode;

        /// <summary>
        /// The player prefab to spawn
        /// </summary>
        SerializedProperty playerPrefabToSpawn;

        /// <summary>
        /// The player spawner provider object
        /// </summary>
        SerializedProperty playerSpawnerProviderObject;

        /// <summary>
        /// The player spawner component
        /// </summary>
        SerializedProperty playerSpawnerComponent;

        /// <summary>
        /// The player spawner method
        /// </summary>
        SerializedProperty playerSpawnerMethod;

        /// <summary>
        /// The spawn position mode
        /// </summary>
        SerializedProperty spawnPositionMode;

        /// <summary>
        /// The fixed position to spawn
        /// </summary>
        SerializedProperty fixedPositionToSpawn;

        /// <summary>
        /// The multiple positions to spawn
        /// </summary>
        SerializedProperty multiplePositionsToSpawn;

        /// <summary>
        /// The position spawner provider object
        /// </summary>
        SerializedProperty positionSpawnerProviderObject;

        /// <summary>
        /// The position spawner component
        /// </summary>
        SerializedProperty positionSpawnerComponent;

        /// <summary>
        /// The position spawner method
        /// </summary>
        SerializedProperty positionSpawnerMethod;

        /// <summary>
        /// Determine if encryption is enabled or not
        /// </summary>
        SerializedProperty encryptionEnabled;

        /// <summary>
        /// The encryption provider object
        /// </summary>
        SerializedProperty encryptionProviderObject;

        /// <summary>
        /// The encryption component
        /// </summary>
        SerializedProperty encryptionComponent;

        /// <summary>
        /// The encryption method
        /// </summary>
        SerializedProperty encryptionMethod;

        /// <summary>
        /// The decryption method
        /// </summary>
        SerializedProperty decryptionMethod;

        /// <summary>
        /// The mode used to control ownership of objects
        /// </summary>
        SerializedProperty ownerShipMode;

        /// <summary>
        /// CCU Limit activated
        /// </summary>
        SerializedProperty enableCCULimit;

        /// <summary>
        /// Maximun of CCU ( when active )
        /// </summary>
        SerializedProperty maximunConnectedCount;

        /// <summary>
        /// Allow to load scene from client connection
        /// </summary>
        SerializedProperty allowPlayersRequestSceneLoad;

        /// <summary>
        /// Allow to load scene from client connection
        /// </summary>
        SerializedProperty pauseDisconnectionOnSceneLoad;

        /// <summary>
        /// The use automatic movement
        /// </summary>
        bool useAutomaticMovement = false;

        /// <summary>
        /// The use transform movement
        /// </summary>
        bool useTransformMovement = false;

        /// <summary>
        /// The use physics movement
        /// </summary>
        bool usePhysicsMovement = false;

        /// <summary>
        /// The use automatic prediction
        /// </summary>
        bool useAutomaticPrediction = false;

        /// <summary>
        /// The use transform prediction
        /// </summary>
        bool useTransformPrediction = false;

        /// <summary>
        /// The use physics prediction
        /// </summary>
        bool usePhysicsPrediction = false;

        /// <summary>
        /// The use network as server
        /// </summary>
        bool useNetworkAsServer = false;

        /// <summary>
        /// The use network as client
        /// </summary>
        bool useNetworkAsClient = false;

        /// <summary>
        /// The use network as manual
        /// </summary>
        bool useNetworkAsManual = false;

        /// <summary>
        /// The use authoritative server
        /// </summary>
        bool useAuthoritativeServer = false;

        /// <summary>
        /// The use relay server
        /// </summary>
        bool useRelayServer = false;

        /// <summary>
        /// The use embedded server
        /// </summary>
        bool useEmbeddedServer = false;

        /// <summary>
        /// The use disabled server
        /// </summary>
        bool useDisabledServer = false;

        /// <summary>
        /// The static player spawn
        /// </summary>
        bool staticPlayerSpawn = false;

        /// <summary>
        /// The dynamic player spawn
        /// </summary>
        bool dynamicPlayerSpawn = false;

        /// <summary>
        /// Spawn player automatially
        /// </summary>
        bool automaticPlayerSpawn = false;

        /// <summary>
        /// Spawn player manually
        /// </summary>
        bool manualPlayerSpawn = false;

        /// <summary>
        /// To use internal identifier
        /// </summary>
        bool useInternalNetworkIdentifier = false;

        /// <summary>
        /// To use customidentifier
        /// </summary>
        bool useCustomNetworkIdentifier = false;

        /// <summary>
        /// The fixed player spawn position
        /// </summary>
        bool fixedPlayerSpawnPosition = false;

        /// <summary>
        /// The multiple player spawn position
        /// </summary>
        bool multiplePlayerSpawnPosition = false;

        /// <summary>
        /// The dynamic player spawn position
        /// </summary>
        bool dynamicPlayerSpawnPosition = false;

        /// <summary>
        /// The use unreliable send
        /// </summary>
        bool useUnreliableSend = false;

        /// <summary>
        /// The use reliable send
        /// </summary>
        bool useReliableSend = false;

        /// <summary>
        /// Allow to take or send object control from/to another player
        /// </summary>
        bool useOwnerShipAllowedMode = false;

        /// <summary>
        /// Disable any type of ownership grant or transfer
        /// </summary>
        bool useOwnerShipDisabledMode = false;

        /// <summary>
        /// Use access level defined by prefab
        /// </summary>
        bool useOwnerShipPrefabMode = true;

        /// <summary>
        /// The prefabs database
        /// </summary>
        NetworkPrefabsDatabase prefabsDatabase;

        /// <summary>
        /// The detail background opacity
        /// </summary>
        const float DETAIL_BACKGROUND_OPACITY = 0.05f;

        /// <summary>
        /// The transport background alpha
        /// </summary>
        const float TRANSPORT_BACKGROUND_ALPHA = 0.25f;

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public void OnEnable() {
            this.networkManager = (this.target as NetworkManager);
            // Get all serializable objects
            this.dontDestroyOnLoad              = serializedObject.FindProperty("dontDestroyOnLoad");
            this.databaseTarget                 = serializedObject.FindProperty("databaseTarget");
            this.serverWorkingMode              = serializedObject.FindProperty("serverWorkingMode");
            this.usePeerToPeer                  = serializedObject.FindProperty("usePeerToPeer");
            this.peerToPeerPort                 = serializedObject.FindProperty("peerToPeerPort");
            this.peerToPeerPortPeerPlayer       = serializedObject.FindProperty("peerToPeerPortPeerPlayer");
            this.useNatTraversal                = serializedObject.FindProperty("useNatTraversal");
            this.useLobbyManager                = serializedObject.FindProperty("useLobbyManager");
            this.serverSocket                   = serializedObject.FindProperty("serverSocket");
            this.clientSocket                   = serializedObject.FindProperty("clientSocket");
            this.connectionType                 = serializedObject.FindProperty("connectionType");
            this.autoConnectOnStart             = serializedObject.FindProperty("autoConnectOnStart");
            this.connectionDelay                = serializedObject.FindProperty("connectionDelay");
            this.asyncConnect                   = serializedObject.FindProperty("asyncConnect");
            this.autoReconnect                  = serializedObject.FindProperty("autoReconnect");
            this.reconnectionDelay              = serializedObject.FindProperty("reconnectionDelay");
            this.reconnectionAttemps            = serializedObject.FindProperty("reconnectionAttemps");
            this.idleTimeutDetection            = serializedObject.FindProperty("idleTimeutDetection");
            this.clientConnectDelay             = serializedObject.FindProperty("clientConnectDelay");
            this.detectServerRestart            = serializedObject.FindProperty("detectServerRestart");
            this.useNetworkLogin                = serializedObject.FindProperty("useNetworkLogin");
            this.loginInfoProviderObject        = serializedObject.FindProperty("loginInfoProviderObject");
            this.loginInfoComponent             = serializedObject.FindProperty("loginInfoComponent");
            this.loginInfoMethod                = serializedObject.FindProperty("loginInfoMethod");
            this.loginTypesMethod               = serializedObject.FindProperty("loginTypesMethod");
            this.enableLoginValidation          = serializedObject.FindProperty("enableLoginValidation");
            this.loginValidationProviderObject  = serializedObject.FindProperty("loginValidationProviderObject");
            this.loginValidationComponent       = serializedObject.FindProperty("loginValidationComponent");
            this.loginValidationMethod          = serializedObject.FindProperty("loginValidationMethod");
            this.useNetworkClock                = serializedObject.FindProperty("useNetworkClock");
            this.useInterpolation               = serializedObject.FindProperty("useInterpolation");
            this.movementType                   = serializedObject.FindProperty("movementType");
            this.useMovementPrediction          = serializedObject.FindProperty("useMovementPrediction");
            this.predictionType                 = serializedObject.FindProperty("predictionType");
            this.useRemoteInput                 = serializedObject.FindProperty("useRemoteInput");
            this.useEchoInput                   = serializedObject.FindProperty("useEchoInput");
            this.inputList                      = serializedObject.FindProperty("inputList");
            this.inputProviderObject            = serializedObject.FindProperty("inputProviderObject");
            this.inputComponent                 = serializedObject.FindProperty("inputComponent");
            this.useInternalPing                = serializedObject.FindProperty("useInternalPing");
            this.sendRateAmount                 = serializedObject.FindProperty("sendRateAmount");
            this.minimumRateAmount              = serializedObject.FindProperty("minimumRateAmount");
            this.sendUpdateMode                 = serializedObject.FindProperty("sendUpdateMode");
            this.latencyTolerance               = serializedObject.FindProperty("latencyTolerance");
            this.goodLatencyValue               = serializedObject.FindProperty("goodLatencyValue");
            this.acceptableLatencyValue         = serializedObject.FindProperty("acceptableLatencyValue");
            this.predictionBufferSize           = serializedObject.FindProperty("predictionBufferSize");
            this.predictionFactor               = serializedObject.FindProperty("predictionFactor");
            this.predictionSpeedFactor          = serializedObject.FindProperty("predictionSpeedFactor");
            this.overridePrediction             = serializedObject.FindProperty("overridePrediction");
            this.predictionProviderObject       = serializedObject.FindProperty("predictionProviderObject");
            this.predictionComponent            = serializedObject.FindProperty("predictionComponent");
            this.useSingleServers               = serializedObject.FindProperty("useSingleServers");
            this.serverAddress                  = serializedObject.FindProperty("serverAddress");
            this.useAnyAddress                  = serializedObject.FindProperty("useAnyAddress");
            this.useFixedAddress                = serializedObject.FindProperty("useFixedAddress");
            this.usePublicAddress               = serializedObject.FindProperty("usePublicAddress");
            this.useInternalAddress             = serializedObject.FindProperty("useInternalAddress");
            this.hostServerAddress              = serializedObject.FindProperty("hostServerAddress");
            this.tcpConnectionPort              = serializedObject.FindProperty("tcpConnectionPort");
            this.udpConnectionPort              = serializedObject.FindProperty("udpConnectionPort");
            this.useMultipleServers             = serializedObject.FindProperty("useMultipleServers");
            this.networkServers                 = serializedObject.FindProperty("networkServers");
            this.networkServersEnabled          = serializedObject.FindProperty("networkServersEnabled");
            this.useDynamicAddress              = serializedObject.FindProperty("useDynamicAddress");
            this.requestServersObject           = serializedObject.FindProperty("requestServersObject");
            this.requestServerComponent         = serializedObject.FindProperty("requestServerComponent");
            this.requestServerMethod            = serializedObject.FindProperty("requestServerMethod");
            this.enablePlayerSpawner            = serializedObject.FindProperty("enablePlayerSpawner");
            this.useInternalNetworkId           = serializedObject.FindProperty("useInternalNetworkId");
            this.useCustomNetworkId             = serializedObject.FindProperty("useCustomNetworkId");
            this.networkIdProviderObject        = serializedObject.FindProperty("networkIdProviderObject");
            this.networkIdComponent             = serializedObject.FindProperty("networkIdComponent");
            this.networkIdMethod                = serializedObject.FindProperty("networkIdMethod");
            this.controlPlayerCameras           = serializedObject.FindProperty("controlPlayerCameras");
            this.detachPlayerCamera             = serializedObject.FindProperty("detachPlayerCamera");
            this.despawnPlayerOnDisconnect      = serializedObject.FindProperty("despawnPlayerOnDisconnect");
            this.despawnPlayerAfterDelay        = serializedObject.FindProperty("despawnPlayerAfterDelay");
            this.delayToRemoveDisconnected      = serializedObject.FindProperty("delayToRemoveDisconnected");
            this.playerSpawnMode                = serializedObject.FindProperty("playerSpawnMode");
            this.playerSpawnTimeMode            = serializedObject.FindProperty("playerSpawnTimeMode");
            this.playerPrefabToSpawn            = serializedObject.FindProperty("playerPrefabToSpawn");
            this.playerSpawnerProviderObject    = serializedObject.FindProperty("playerSpawnerProviderObject");
            this.playerSpawnerComponent         = serializedObject.FindProperty("playerSpawnerComponent");
            this.playerSpawnerMethod            = serializedObject.FindProperty("playerSpawnerMethod");
            this.spawnPositionMode              = serializedObject.FindProperty("spawnPositionMode");
            this.fixedPositionToSpawn           = serializedObject.FindProperty("fixedPositionToSpawn");
            this.multiplePositionsToSpawn       = serializedObject.FindProperty("multiplePositionsToSpawn");
            this.positionSpawnerProviderObject  = serializedObject.FindProperty("positionSpawnerProviderObject");
            this.positionSpawnerComponent       = serializedObject.FindProperty("positionSpawnerComponent");
            this.positionSpawnerMethod          = serializedObject.FindProperty("positionSpawnerMethod");
            this.encryptionEnabled              = serializedObject.FindProperty("encryptionEnabled");
            this.encryptionProviderObject       = serializedObject.FindProperty("encryptionProviderObject");
            this.encryptionComponent            = serializedObject.FindProperty("encryptionComponent");
            this.encryptionMethod               = serializedObject.FindProperty("encryptionMethod");
            this.decryptionMethod               = serializedObject.FindProperty("decryptionMethod");
            this.ownerShipMode                  = serializedObject.FindProperty("ownerShipMode");
            this.enableCCULimit                 = serializedObject.FindProperty("enableCCULimit");
            this.maximunConnectedCount          = serializedObject.FindProperty("maximunConnectedCount");
            this.allowPlayersRequestSceneLoad   = serializedObject.FindProperty("allowPlayersRequestSceneLoad");
            this.pauseDisconnectionOnSceneLoad  = serializedObject.FindProperty("pauseDisconnectionOnSceneLoad");


            this.useAutomaticMovement           = (this.movementType.enumValueFlag == (int)PredictionType.Automatic);
            this.useTransformMovement           = (this.movementType.enumValueFlag == (int)PredictionType.UseTransform);
            this.usePhysicsMovement             = (this.movementType.enumValueFlag == (int)PredictionType.UsePhysics);

            this.useAutomaticPrediction         = (this.predictionType.enumValueFlag == (int)PredictionType.Automatic);
            this.useTransformPrediction         = (this.predictionType.enumValueFlag == (int)PredictionType.UseTransform);
            this.usePhysicsPrediction           = (this.predictionType.enumValueFlag == (int)PredictionType.UsePhysics);

            this.useNetworkAsServer             = (this.connectionType.enumValueFlag == (int)NetworkConnectionType.Server);
            this.useNetworkAsClient             = (this.connectionType.enumValueFlag == (int)NetworkConnectionType.Client);
            this.useNetworkAsManual             = (this.connectionType.enumValueFlag == (int)NetworkConnectionType.Manual);

            this.useOwnerShipAllowedMode        = (this.ownerShipMode.enumValueFlag == (int)OwnerShipServerMode.Allowed);
            this.useOwnerShipDisabledMode       = (this.ownerShipMode.enumValueFlag == (int)OwnerShipServerMode.Disabled);
            this.useOwnerShipPrefabMode         = (this.ownerShipMode.enumValueFlag == (int)OwnerShipServerMode.PrefabDefinition);

            // Initialize database
            this.RefreshDatabase();
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorUtils.PrintImage("objectnet_logo", Color.blue, 0, 25);

            GUILayout.Space(5.0f);

            if (Application.isPlaying) {
                EditorUtils.PrintImageButton("Changes are disabled during PlayMode", "oo_info", Color.red.WithAlpha(0.15f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                });
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorUtils.PrintImageButton("Documentation", "oo_document", EditorUtils.IMAGE_BUTTON_COLOR, EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                Help.BrowseURL("https://onlineobject.net/objectnet/docs/manual/ObjectNet.html");
            });
            EditorUtils.PrintImageButton("Tutorials", "oo_youtube", EditorUtils.IMAGE_BUTTON_COLOR, EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                Help.BrowseURL("https://www.youtube.com/@TheObjectNet");
            });
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5.0f);

            // Prefabs Database
            EditorUtils.PrintImageButton("Network Prefabs", "oo_prefab", EditorUtils.IMAGE_BUTTON_COLOR, EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                NetworkPrefabsDatabaseWindow.OpenNetworkPrefabsDatabaseWindow(this.databaseTarget.stringValue);
            });

            GUILayout.Space(5.0f);

            // Target Database
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.SIMPLE_DANGER_FONT_COLOR.WithAlpha(TRANSPORT_BACKGROUND_ALPHA)));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5.0f);

            EditorGUILayout.BeginHorizontal();
            EditorUtils.PrintExplanationLabel(String.Format("Selected Database \"{0}\"", this.databaseTarget.stringValue.ToUpper()),
                                              "oo_cache",
                                              EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR,
                                              5,
                                              13,
                                              0,
                                              false);
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(-5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5);
            EditorUtils.PrintSimpleExplanation("Click to modify ", Color.white, 13, false);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(2);
            if (GUILayout.Button(Resources.Load("oo_eye_clickable") as Texture, GUIStyle.none, GUILayout.Width(24), GUILayout.Height(24))) {
                NetworkDatabaseWindow.OpenNetworkDatabaseWindow(this.networkManager, this, this.databaseTarget.stringValue);
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(10.0f);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            // Target Database - END

            GUILayout.Space(5.0f);

            EditorUtils.PrintHeader("Network Manager", Color.blue, Color.white, 16, "oo_network", true, () => {
                if (this.dontDestroyOnLoad != null) {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(3.0f);
                    EditorGUILayout.BeginHorizontal();
                    EditorUtils.PrintSimpleExplanation("Don't destroy");
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(3.0f);
                    EditorUtils.PrintBooleanSquaredByRef(ref this.dontDestroyOnLoad, "", null, 14, 12, false);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            });
            EditorUtils.PrintHeader("Network Transport", EditorUtils.SUB_DETAIL_PANEL_COLOR, Color.white, 12, "oo_send");
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.DETAIL_INFO_COLOR.WithAlpha(TRANSPORT_BACKGROUND_ALPHA)));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
            EditorUtils.PrintExplanationLabel(String.Format("Active [ {0} ]", NetworkTransportsDatabaseWindow.GetActiveTransportName()),
                                              "oo_socket",
                                              EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR,
                                              5,
                                              13,
                                              0,
                                              false);
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(-5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5);
            EditorUtils.PrintSimpleExplanation("Click to modify ", Color.white, 13, false);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(2);
            if (GUILayout.Button(Resources.Load("oo_eye_clickable") as Texture, GUIStyle.none, GUILayout.Width(24), GUILayout.Height(24))) {
                NetworkTransportsDatabaseWindow.OpenNetworkTransportsDatabaseWindow();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2.0f);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorUtils.PrintExplanationLabel("Network transport is the way how data is transfered between each network node.", "oo_info");

            EditorUtils.PrintHeader("Server Mode", EditorUtils.SUB_DETAIL_PANEL_COLOR, Color.white, 12, "oo_server_mode");
            // Print checkboxes
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            this.useRelayServer = (this.serverWorkingMode.enumValueIndex == (int)NetworkServerMode.Relay);
            this.useEmbeddedServer = (this.serverWorkingMode.enumValueIndex == (int)NetworkServerMode.Embedded);
            this.useAuthoritativeServer = (this.serverWorkingMode.enumValueIndex == (int)NetworkServerMode.Authoritative);
            this.useDisabledServer = (this.serverWorkingMode.enumValueIndex == (int)NetworkServerMode.ClientOnly);
            if (EditorUtils.PrintBoolean(ref this.useEmbeddedServer, "Embedded")) {
                this.useAuthoritativeServer = !this.useEmbeddedServer;
                this.useRelayServer = !this.useEmbeddedServer;
                this.useDisabledServer = !this.useEmbeddedServer;
            }
            if (EditorUtils.PrintBoolean(ref this.useRelayServer, "Relay")) {
                this.useAuthoritativeServer = !this.useRelayServer;
                this.useEmbeddedServer = !this.useRelayServer;
                this.useDisabledServer = !this.useRelayServer;
            }
            if (EditorUtils.PrintBoolean(ref this.useAuthoritativeServer, "Authoritative")) {
                this.useRelayServer = !this.useAuthoritativeServer;
                this.useEmbeddedServer = !this.useAuthoritativeServer;
                this.useDisabledServer = !this.useAuthoritativeServer;
            }
            if (EditorUtils.PrintBoolean(ref this.useDisabledServer, "Client Only")) {
                this.useAuthoritativeServer = !this.useDisabledServer;
                this.useRelayServer = !this.useDisabledServer;
                this.useEmbeddedServer = !this.useDisabledServer;
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            if (this.useEmbeddedServer) {
                this.serverWorkingMode.enumValueIndex = (int)NetworkServerMode.Embedded;
                GUILayout.Space(10);
                EditorUtils.PrintExplanationLabel("On embedded mode one player will be the host, run all the login and redirect communication between players.", "oo_info");
                GUILayout.Space(5);
                // Check if nat traversal is enabled
                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                GUILayout.Space(15.0f);
                EditorGUILayout.BeginVertical();
                EditorUtils.PrintBooleanSquaredByRef(ref this.useNatTraversal, "Enable NAT Traversal", "oo_peer_to_peer", 16, 12);
                if (this.useNatTraversal.boolValue) {
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("Try to redirect router port to machine where server or game is running", "oo_info");
                    GUILayout.Space(5.0f);
                    EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("This option will allow to accept incomming connections from tjhe outside networks when you is into a closed LAN", "oo_network", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintSimpleExplanation("This functionality depend that router allow PnP or UPnP connections", EditorUtils.SIMPLE_DANGER_FONT_COLOR);
                    GUILayout.Space(10.0f);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            } else if (this.useRelayServer) {
                this.serverWorkingMode.enumValueIndex = (int)NetworkServerMode.Relay;
                GUILayout.Space(10);
                EditorUtils.PrintSimpleExplanation("Atention.. This option must generate an separated instance only to work as server host and without playing the game.");
                GUILayout.Space(5);
                EditorUtils.PrintExplanationLabel("On Relay mode a server will redirect communication between players. An certain level of validation logic can be implemented", "oo_info");
                GUILayout.Space(10);

                // Check if lobby suppport is enabled
                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                GUILayout.Space(15.0f);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(10.0f);
                EditorUtils.PrintBooleanSquaredByRef(ref this.useLobbyManager, "Enable Lobby support", "oo_lobby", 16, 12);
                if (this.useLobbyManager.boolValue) {
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("Allow players to create lobbies", "oo_info");
                    GUILayout.Space(5.0f);
                    EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("Players must enter into lobbies before being spawned and play a match", "oo_lobby", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                    GUILayout.Space(10.0f);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                // Check if peer to peer is enabled
                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                GUILayout.Space(15.0f);
                EditorGUILayout.BeginVertical();
                EditorUtils.PrintBooleanSquaredByRef(ref this.usePeerToPeer, "Enable Peer To Peer", "oo_latency", 16, 12);
                if (this.usePeerToPeer.boolValue) {
                    if (NetworkTransportsDatabaseWindow.IsPeerToPeerSupported()) {
                        GUILayout.Space(5.0f);
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                        EditorUtils.PrintSizedLabel("Peer to Peer Port", 12, EditorUtils.DEFAULT_HEADER_TITLE_COLOR);
                        GUILayout.Space(5.0f);
                        this.peerToPeerPort.intValue = Convert.ToInt32(EditorGUILayout.TextField(this.peerToPeerPort.intValue.ToString(), GUILayout.Width(80)));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.BeginVertical();
                        GUILayout.Space(3.0f);
                        EditorUtils.PrintBooleanSquaredByRef(ref this.peerToPeerPortPeerPlayer, "", null, 14, 12, false);
                        EditorGUILayout.EndVertical();
                        EditorUtils.PrintSizedLabel("Use one port peer player", 12, EditorUtils.DEFAULT_HEADER_TITLE_COLOR);
                        GUILayout.Space(10.0f);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(5.0f);
                        EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                        GUILayout.Space(5.0f);
                        EditorUtils.PrintExplanationLabel("Allow player to connect directly always when possible", "oo_info");
                        GUILayout.Space(5.0f);
                        EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                        GUILayout.Space(5.0f);
                        EditorUtils.PrintExplanationLabel("Peer to Peer model may drastically reduce costs with network bandwidth and latency between players, since that players communicate directly with host", "oo_peer_to_peer", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                        GUILayout.Space(5.0f);
                        EditorUtils.PrintSimpleExplanation("Peer to Peer model may left game more susceptible to cheating, it's recomended implement any technique to prevent it", EditorUtils.SIMPLE_DANGER_FONT_COLOR);
                        GUILayout.Space(10.0f);
                    } else {
                        EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, Vector2.zero);
                        GUILayout.Space(5.0f);
                        EditorUtils.PrintExplanationLabel(string.Format("The selected transport system [ {0} ] does not support peer to peer communication model", NetworkTransportsDatabaseWindow.GetActiveTransportName()), "oo_error");
                        GUILayout.Space(5.0f);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(48.0f);
                        EditorUtils.PrintSizedLabel("Peer to Peer will be disabled", 12, Color.red.WithAlpha(0.5f));
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(5.0f);
                        EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, Vector2.zero);
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                // Check if nat traversal is enabled
                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                GUILayout.Space(15.0f);
                EditorGUILayout.BeginVertical();
                EditorUtils.PrintBooleanSquaredByRef(ref this.useNatTraversal, "Enable NAT Traversal", "oo_peer_to_peer", 16, 12);
                if (this.useNatTraversal.boolValue) {
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("Try to redirect router port to machine where server or game is running", "oo_info");
                    GUILayout.Space(5.0f);
                    EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("This option will increase the possibility to accept incomming connections and peer to peer network", "oo_network", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintSimpleExplanation("This functionality depend that router allow PnP or UPnP connections", EditorUtils.SIMPLE_DANGER_FONT_COLOR);
                    GUILayout.Space(10.0f);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            } else if (this.useAuthoritativeServer) {
                this.serverWorkingMode.enumValueIndex = (int)NetworkServerMode.Authoritative;
                GUILayout.Space(10);
                EditorUtils.PrintSimpleExplanation("Atention.. This option must generate an separated instance only to work as server host and without playing the game.");
                GUILayout.Space(5);
                EditorUtils.PrintExplanationLabel("In authoritative mode a game instance will run on the server and players will only send their own data.\nAll logic will be executed on the server", "oo_info");
                GUILayout.Space(5);
            } else if (this.useDisabledServer) {
                this.serverWorkingMode.enumValueIndex = (int)NetworkServerMode.ClientOnly;
                GUILayout.Space(10);
                GUILayout.Space(5);
                EditorUtils.PrintExplanationLabel("Using this option you need a Relay or Authoritative server running to work as server of game and you will need to connect at host where server is running", "oo_info");
                GUILayout.Space(5);
            }
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();


            /// Ownership mode
            EditorUtils.PrintHeader("OwnerShip working mode", EditorUtils.SUB_DETAIL_PANEL_COLOR, Color.white, 12, "oo_ownership_coloured");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();
            if (EditorUtils.PrintBoolean(ref this.useOwnerShipAllowedMode, "Allowed", "oo_ownership")) {
                this.useOwnerShipDisabledMode   = !this.useOwnerShipAllowedMode;
                this.useOwnerShipPrefabMode     = !this.useOwnerShipAllowedMode;
            }
            GUILayout.Space(10);
            if (EditorUtils.PrintBoolean(ref this.useOwnerShipDisabledMode, "Disabled", "oo_lock_colored")) {
                this.useOwnerShipAllowedMode    = !this.useOwnerShipDisabledMode;
                this.useOwnerShipPrefabMode     = !this.useOwnerShipDisabledMode;
            }
            GUILayout.Space(10);
            if (EditorUtils.PrintBoolean(ref this.useOwnerShipPrefabMode, "Use Prefab Definition", "oo_prefab")) {
                this.useOwnerShipAllowedMode    = !this.useOwnerShipPrefabMode;
                this.useOwnerShipDisabledMode   = !this.useOwnerShipPrefabMode;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10.0f);
            bool showAuthoritativeWarning = false;
            if (this.useOwnerShipAllowedMode) {
                this.ownerShipMode.enumValueFlag = (int)OwnerShipServerMode.Allowed;
                EditorUtils.PrintExplanationLabel("All ownership operations will be allowed", "oo_info");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(25.0f);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(5.0f);
                EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.gray.WithAlpha(0.15f)));
                GUILayout.Space(10.0f);
                EditorUtils.PrintSimpleExplanation(" - Take ownership is allowed", Color.green.WithAlpha(0.75f));
                EditorUtils.PrintSimpleExplanation(" - Tranfer ownership is allowed", Color.green.WithAlpha(0.75f));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                showAuthoritativeWarning = (this.useAuthoritativeServer);                
            } else if (this.useOwnerShipDisabledMode) {
                this.ownerShipMode.enumValueFlag = (int)OwnerShipServerMode.Disabled;
                EditorUtils.PrintExplanationLabel("All ownership operations will be disabled", "oo_info");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(25.0f);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(5.0f);
                EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.gray.WithAlpha(0.15f)));
                GUILayout.Space(10.0f);
                EditorUtils.PrintSimpleExplanation(" - Take ownership is blocked", Color.yellow.WithAlpha(0.75f));
                EditorUtils.PrintSimpleExplanation(" - Tranfer ownership is blocked", Color.yellow.WithAlpha(0.75f));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            } else if (this.useOwnerShipPrefabMode) {
                this.ownerShipMode.enumValueFlag = (int)OwnerShipServerMode.PrefabDefinition;
                EditorUtils.PrintExplanationLabel("Ownership operations will respect definitions existent into network prefabs definitions", "oo_info");
                showAuthoritativeWarning = (this.useAuthoritativeServer);
            }

            if (showAuthoritativeWarning) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(25.0f);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(5.0f);
                EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.gray.WithAlpha(0.15f)));
                GUILayout.Space(10.0f);
                EditorUtils.PrintExplanationLabel("WARNING", "oo_info");
                GUILayout.Space(5.0f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20.0f);
                EditorUtils.PrintSimpleExplanation("On authoritative mode the use of ownership tranfer to player may open a door to cheating");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(15.0f);
            /// End - Ownership mode

            EditorUtils.PrintHeader("Connections", EditorUtils.SUB_DETAIL_PANEL_COLOR, Color.white, 12, "oo_socket");
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            GUILayout.Label("Server Connection");
            this.serverSocket.objectReferenceValue = (EditorGUILayout.ObjectField(this.serverSocket.objectReferenceValue, typeof(NetworkTransport), true) as NetworkTransport);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            GUILayout.Label("Client Connection");
            this.clientSocket.objectReferenceValue = (EditorGUILayout.ObjectField(this.clientSocket.objectReferenceValue, typeof(NetworkTransport), true) as NetworkTransport);
            EditorGUILayout.EndHorizontal();
            // Print checkboxes
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            if (EditorUtils.PrintBoolean(ref this.useNetworkAsServer, "Server")) {
                this.useNetworkAsClient = !this.useNetworkAsServer;
                this.useNetworkAsManual = !this.useNetworkAsServer & !this.useNetworkAsClient;
            }
            if (EditorUtils.PrintBoolean(ref this.useNetworkAsClient, "Client")) {
                this.useNetworkAsServer = !this.useNetworkAsClient;
                this.useNetworkAsManual = !this.useNetworkAsClient & !this.useNetworkAsServer;
            }
            if (EditorUtils.PrintBoolean(ref this.useNetworkAsManual, "Manual")) {
                this.useNetworkAsServer = !this.useNetworkAsManual;
                this.useNetworkAsClient = !this.useNetworkAsManual & !this.useNetworkAsServer;
            }
            EditorGUILayout.EndHorizontal();
            if (this.useNetworkAsServer) {
                this.connectionType.enumValueFlag = (int)NetworkConnectionType.Server;
                GUILayout.Space(10);
                EditorUtils.PrintExplanationLabel("Clients will connected on this instance of the game, and this instance will run all game logic", "oo_info");
                GUILayout.Space(5);
            } else if (this.useNetworkAsClient) {
                this.connectionType.enumValueFlag = (int)NetworkConnectionType.Client;
                GUILayout.Space(10);
                EditorUtils.PrintExplanationLabel("This instance need to connect with server and will execute total or partially only his own logic", "oo_info");
                GUILayout.Space(5);
            } else if (this.useNetworkAsManual) {
                this.connectionType.enumValueFlag = (int)NetworkConnectionType.Manual;
                GUILayout.Space(10);
                EditorUtils.PrintExplanationLabel("You will need to implement by code ( or UI ) how your game will select instance is Server or Client", "oo_info");
                GUILayout.Space(5);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();

            if (!this.useNetworkAsManual) {
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorUtils.PrintBooleanSquaredByRef(ref this.autoConnectOnStart, "Connect on Start", null, 14, 12);
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical();
                GUILayout.Space(5);
                EditorUtils.PrintSimpleExplanation("Auto connect when aplication/game start");
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                if (this.autoConnectOnStart.boolValue == true) {
                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                    EditorUtils.DrawHorizontalIntBar(ref this.connectionDelay, "Delay to connect (sec)", 0, 60, 12);
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.Space(5);
            } else {
                GUILayout.Space(10);
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            GUILayout.Space(5);
            EditorUtils.DrawHorizontalIntBar(ref this.idleTimeutDetection, "Disconnect Detection (sec)", 1, 60, 12);
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorUtils.PrintExplanationLabel("How long system will take to disconnect socket when connection is idle", "oo_timeout");
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorUtils.PrintBooleanSquaredByRef(ref this.autoReconnect, "Auto Reconnect", null, 14, 12);
            EditorUtils.PrintBooleanSquaredByRef(ref this.asyncConnect, "Don't block on connect", null, 14, 12);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            EditorUtils.DrawHorizontalIntBar(ref this.reconnectionDelay, "Reconnection Interval (seg)", 0, 200, 12);
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorUtils.PrintSimpleExplanation("Amount of time between connection failure and next reconnection try");
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            EditorUtils.DrawHorizontalIntBar(ref this.reconnectionAttemps, "Reconnection attemps", 0, 200, 12);
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorUtils.PrintSimpleExplanation("How many times system will try to reconnect the client when disconnection is detected");
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();

            if (this.connectionType.enumValueFlag == (int)NetworkConnectionType.Server) {
                if (NetworkTransportsDatabaseWindow.IsConnectionDelaySupported()) {
                    this.clientConnectDelay.intValue = Mathf.Clamp(this.clientConnectDelay.intValue, 
                                                                   (this.clientConnectDelay.intValue == 0) ? NetworkTransportsDatabaseWindow.GetDefaulConnectionDelay() : 
                                                                                                             NetworkTransportsDatabaseWindow.GetMinimumConnectionDelay(), 
                                                                   NetworkTransportsDatabaseWindow.GetMaximumConnectionDelay());
                    GUILayout.Space(5);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.blue.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                    GUILayout.Space(5);
                    EditorUtils.DrawHorizontalIntBar(ref this.clientConnectDelay, 
                                                     "Client connection notification delay (ms)", 
                                                     NetworkTransportsDatabaseWindow.GetMinimumConnectionDelay(), 
                                                     NetworkTransportsDatabaseWindow.GetMaximumConnectionDelay(), 
                                                     12);
                    GUILayout.Space(5);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(5);
                    EditorUtils.PrintSimpleExplanation("Time to wait before notifying the client regarding his connection after connecting at the server");
                    GUILayout.Space(10);
                    EditorUtils.PrintExplanationLabel("On the Steam Transport system, this value is useful to ensure client connection, increase if you experience issues during connection", "oo_steam");
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(10);
                } else {
                    this.clientConnectDelay.intValue = 0;
                }
            }

            GUILayout.Space(5);
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            EditorGUILayout.BeginHorizontal();
            EditorUtils.PrintBooleanSquaredByRef(ref this.detectServerRestart, "Detect server restart", null, 14, 12);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorUtils.PrintExplanationLabel("In case of server restart, all players will be respawned in his actual position", "oo_info");
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();


            GUILayout.Space(5);
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            EditorGUILayout.BeginHorizontal();
            EditorUtils.PrintBooleanSquaredByRef(ref this.enableCCULimit, "Enable CCU Limit", null, 14, 12);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (this.enableCCULimit.boolValue == false) {
                EditorUtils.PrintExplanationLabel("This option will limit the maximum of connected users simultaneously in the server.", "oo_info");
            } else {
                GUILayout.Space(5);
                this.maximunConnectedCount.intValue = (int)Math.Clamp(this.maximunConnectedCount.intValue, 1, uint.MaxValue);
                EditorUtils.DrawHorizontalIntBar(ref this.maximunConnectedCount, "Maximum of CCU", 1, 100, 12);
                GUILayout.Space(5);
                EditorUtils.PrintExplanationLabel("CCU ( concurrent users ) is the amount of users that can be simultaneously connected at the server.", "oo_info");
                GUILayout.Space(5);
            }
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();


            GUILayout.Space(5);
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            EditorGUILayout.BeginHorizontal();
            EditorUtils.PrintBooleanSquaredByRef(ref this.allowPlayersRequestSceneLoad, "Enable client scene load", null, 14, 12);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorUtils.PrintExplanationLabel("This option allows any client to request the server to start the remote scene-loading process.", "oo_info");
            if (this.allowPlayersRequestSceneLoad.boolValue == true) {
                GUILayout.Space(5);
                EditorUtils.PrintExplanationLabelColored("Use this resource carefully because players can request to load different scenes at the same time causing instability on your game", "oo_error", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                GUILayout.Space(5);
            }
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            EditorGUILayout.BeginHorizontal();
            EditorUtils.PrintBooleanSquaredByRef(ref this.pauseDisconnectionOnSceneLoad, "Pause disconnection during scene loading", null, 14, 12);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorUtils.PrintExplanationLabel("This option will pause any disconnect detection during the scene-loading process.", "oo_info");
            if (this.pauseDisconnectionOnSceneLoad.boolValue == true) {
                GUILayout.Space(5);
                EditorUtils.PrintExplanationLabelColored("Any disconnection will be resumed only after all clients finish his loading process.", "oo_error", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                GUILayout.Space(5);
            }
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            EditorUtils.PrintHeader("Connection Parameters", EditorUtils.SUB_DETAIL_PANEL_COLOR, Color.white, 12, "oo_parameters");

            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);
            EditorUtils.PrintExplanationLabel("Define Tcp and Udp port to be used by client and servers", "oo_info");
            GUILayout.Space(10);

            if (NetworkTransportsDatabaseWindow.IsDoubleChannelTransport()) {
                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                GUILayout.Space(30);
                EditorGUILayout.BeginVertical();
                EditorUtils.PrintSizedLabel("Tcp Port", 12, EditorUtils.DEFAULT_HEADER_TITLE_COLOR);
                this.tcpConnectionPort.intValue = Convert.ToInt32(EditorGUILayout.TextField(this.tcpConnectionPort.intValue.ToString(), GUILayout.Width(80)));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                GUILayout.Space(20);
                EditorUtils.PrintSimpleExplanation("TCP is used to send reliable messages");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                GUILayout.Space(30);
                EditorGUILayout.BeginVertical();
                EditorUtils.PrintSizedLabel("Udp Port", 12, EditorUtils.DEFAULT_HEADER_TITLE_COLOR);
                this.udpConnectionPort.intValue = Convert.ToInt32(EditorGUILayout.TextField(this.udpConnectionPort.intValue.ToString(), GUILayout.Width(80)));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                GUILayout.Space(20);
                EditorUtils.PrintSimpleExplanation("UDP is used to send unreliable messages");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            } else {
                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                GUILayout.Space(30);
                EditorGUILayout.BeginVertical();
                EditorUtils.PrintSizedLabel("Transport Port", 12, EditorUtils.DEFAULT_HEADER_TITLE_COLOR);
                this.tcpConnectionPort.intValue = Convert.ToInt32(EditorGUILayout.TextField(this.tcpConnectionPort.intValue.ToString(), GUILayout.Width(80)));
                // Update UDP to be the same as TCP
                this.udpConnectionPort.intValue = this.tcpConnectionPort.intValue;
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                GUILayout.Space(20);
                EditorUtils.PrintSimpleExplanation("TCP and UDP will both use the same port");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            if (this.connectionType.enumValueFlag == (int)NetworkConnectionType.Server) {
                if (EditorUtils.PrintBoolean(ref this.useAnyAddress, "Default Address")) {
                    if (this.useAnyAddress.boolValue) {
                        this.useFixedAddress.boolValue = false;
                        this.useInternalAddress.boolValue = false;
                        this.usePublicAddress.boolValue = false;
                    }
                }
                if (this.useAnyAddress.boolValue) {
                    GUILayout.Space(10);
                    EditorUtils.PrintExplanationLabel("The bind will be over port only and system will select the network interface to start listening", "oo_info");
                    GUILayout.Space(10);
                }

                if (EditorUtils.PrintBoolean(ref this.useFixedAddress, "Use Fixed Address")) {
                    if (this.useFixedAddress.boolValue) {
                        this.useAnyAddress.boolValue = false;
                        this.useInternalAddress.boolValue = false;
                        this.usePublicAddress.boolValue = false;
                    }
                }
                if (this.useFixedAddress.boolValue) {
                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                    GUILayout.Space(20.0f);
                    EditorGUILayout.BeginVertical();
                    EditorUtils.PrintSizedLabel("Server Address", 12, EditorUtils.EXPLANATION_FONT_COLOR);
                    GUILayout.Space(2.0f);
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.BeginVertical();
                    this.hostServerAddress.stringValue = EditorGUILayout.TextField(this.hostServerAddress.stringValue);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(10);
                    EditorUtils.PrintExplanationLabel("System will use address defined on \"Server Address\" field", "oo_info");
                    GUILayout.Space(10);
                }
                if (EditorUtils.PrintBoolean(ref this.usePublicAddress, "Use Public Address")) {
                    if (this.usePublicAddress.boolValue) {
                        this.useAnyAddress.boolValue = false;
                        this.useFixedAddress.boolValue = false;
                        this.useInternalAddress.boolValue = false;
                    }
                }
                if (this.usePublicAddress.boolValue) {
                    GUILayout.Space(10);
                    EditorUtils.PrintExplanationLabel("System will use the public ip address, ( Your router IP if you are into a LAN )", "oo_info");
                    GUILayout.Space(10);
                }
                if (EditorUtils.PrintBoolean(ref this.useInternalAddress, "Use Internal Address")) {
                    if (this.useInternalAddress.boolValue) {
                        this.useAnyAddress.boolValue = false;
                        this.useFixedAddress.boolValue = false;
                        this.usePublicAddress.boolValue = false;
                    }
                }
                if (this.useInternalAddress.boolValue) {
                    GUILayout.Space(10);
                    EditorUtils.PrintExplanationLabel("System will use the address associated with your internal network ( if you are into a LAN )", "oo_info");
                    GUILayout.Space(10);
                }
            } else {
                if (EditorUtils.PrintBoolean(ref this.useSingleServers, "Single Server")) {
                    if (this.useSingleServers.boolValue) {
                        this.useMultipleServers.boolValue = false;
                        this.useDynamicAddress.boolValue = false;
                    }
                }
                if (this.useSingleServers.boolValue) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15.0f);
                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                    GUILayout.Space(20.0f);
                    EditorGUILayout.BeginVertical();
                    EditorUtils.PrintSizedLabel("Server Address", 12, EditorUtils.EXPLANATION_FONT_COLOR);
                    GUILayout.Space(2.0f);
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.BeginVertical();
                    this.serverAddress.stringValue = EditorGUILayout.TextField(this.serverAddress.stringValue);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                if (EditorUtils.PrintBoolean(ref this.useMultipleServers, "Multiple Servers")) {
                    if (this.useMultipleServers.boolValue) {
                        this.useSingleServers.boolValue = false;
                        this.useDynamicAddress.boolValue = false;
                    }
                }
                if (this.useMultipleServers.boolValue) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15.0f);
                    EditorGUILayout.BeginVertical();

                    EditorUtils.PrintImageButton("Register Server", "oo_add", EditorUtils.IMAGE_BUTTON_COLOR, EditorUtils.IMAGE_BUTTON_FONT_COLOR, 12, 14f, 14f, () => {
                        this.networkServers.InsertArrayElementAtIndex(this.networkServers.arraySize);
                        this.networkServersEnabled.InsertArrayElementAtIndex(this.networkServersEnabled.arraySize);
                        // Set initial values
                        this.networkServers.GetArrayElementAtIndex(this.networkServers.arraySize - 1).stringValue = "127.0.0.1";
                        this.networkServersEnabled.GetArrayElementAtIndex(this.networkServersEnabled.arraySize - 1).boolValue = true;
                    });

                    EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                    int removeIndex = -1;
                    if (this.networkServers.arraySize > 0) {
                        GUILayout.Space(10.0f);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(30.0f);
                        EditorUtils.PrintSizedLabel("Avaiable servers [ Ip Address ]", 12, EditorUtils.EXPLANATION_FONT_COLOR);
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(2.0f);
                    }
                    try {
                        for (int serverIndex = 0; serverIndex < this.networkServers.arraySize; serverIndex++) {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(40.0f);

                            if (GUILayout.Button(Resources.Load("oo_remove") as Texture, GUIStyle.none, GUILayout.Width(14), GUILayout.Height(14))) {
                                removeIndex = serverIndex;
                            }
                            GUILayout.Space(5.0f);
                            this.networkServers.GetArrayElementAtIndex(serverIndex).stringValue = EditorGUILayout.TextField(this.networkServers.GetArrayElementAtIndex(serverIndex).stringValue);

                            bool isEnabled = this.networkServersEnabled.GetArrayElementAtIndex(serverIndex).boolValue;
                            GUILayout.Space(13.0f);
                            EditorUtils.PrintBooleanSquared(ref isEnabled, "Enabled", null, 14, 10, false);
                            this.networkServersEnabled.GetArrayElementAtIndex(serverIndex).boolValue = isEnabled;
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(5.0f);
                        }
                    } catch {
                        this.networkServers.ClearArray();
                        this.networkServersEnabled.ClearArray();
                    }
                    if (removeIndex > -1) {
                        this.networkServers.DeleteArrayElementAtIndex(removeIndex);
                        this.networkServersEnabled.DeleteArrayElementAtIndex(removeIndex);
                    }
                    GUILayout.Space(10);
                    EditorUtils.PrintExplanationLabel("System will try to connect to the server with low latency possible", "oo_info");
                    GUILayout.Space(10);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                if (EditorUtils.PrintBoolean(ref this.useDynamicAddress, "Dynamic Server(s)")) {
                    if (this.useDynamicAddress.boolValue) {
                        this.useMultipleServers.boolValue = false;
                        this.useSingleServers.boolValue = false;
                    }
                }
                if (this.useDynamicAddress.boolValue) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15.0f);
                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                    GUILayout.Space(20.0f);
                    EditorGUILayout.BeginVertical();
                    EditorUtils.PrintSizedLabel("Custom Server List", 12, EditorUtils.EXPLANATION_FONT_COLOR);
                    GUILayout.Space(2.0f);
                    this.DrawServerAddressProvider();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorUtils.PrintHeader("Send configurations", EditorUtils.SUB_DETAIL_PANEL_COLOR, Color.white, 12, "oo_send");
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            EditorUtils.DrawHorizontalIntBar(ref this.sendRateAmount, "Send update frequency", 1, 60);
            GUILayout.Space(15);
            EditorUtils.PrintExplanationLabel("Define how many update messages will be send peer second", "oo_info");
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(45);
            EditorUtils.PrintSimpleExplanation("This value will be used only for object update status messages");
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.EndVertical();

            // Fix the minimum to not overpass maximum
            this.minimumRateAmount.intValue = Mathf.Clamp(this.minimumRateAmount.intValue, 0, this.sendRateAmount.intValue);

            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            EditorUtils.DrawHorizontalIntBar(ref this.minimumRateAmount, "Minimum update frequency", 0, 60);
            GUILayout.Space(15);
            EditorUtils.PrintExplanationLabel("Define the minimum update messages will be send peer second", "oo_info");
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(45);
            EditorUtils.PrintSimpleExplanation("This is the interval that system will keep sending updated even without any changes on object to mitigate that any lost message");
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.EndVertical();
            

            GUILayout.Space(5);
            EditorUtils.PrintHeader("Delivery method", EditorUtils.SUB_DETAIL_PANEL_COLOR, Color.white, 12, "oo_delivery");
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            EditorGUILayout.BeginHorizontal();
            this.useReliableSend = (this.sendUpdateMode.enumValueIndex == (int)DeliveryMode.Reliable);
            this.useUnreliableSend = (this.sendUpdateMode.enumValueIndex == (int)DeliveryMode.Unreliable);
            // Print options
            if (EditorUtils.PrintBoolean(ref this.useReliableSend, "Reliable", null, 14, 12)) {
                this.useUnreliableSend = !this.useReliableSend;
            }
            if (EditorUtils.PrintBoolean(ref this.useUnreliableSend, "Unreliable", null, 14, 12)) {
                this.useReliableSend = !this.useUnreliableSend;
            }
            EditorGUILayout.EndHorizontal();
            if (this.useReliableSend) {
                this.sendUpdateMode.enumValueIndex = (int)DeliveryMode.Reliable;
                GUILayout.Space(10);
                EditorUtils.PrintExplanationLabel("Reliable messages have a delivery warrant but are considerably slow when compared with Unreliable and consume much more bandwidth", "oo_info");
                GUILayout.Space(5);
            } else if (this.useUnreliableSend) {
                this.sendUpdateMode.enumValueIndex = (int)DeliveryMode.Unreliable;
                GUILayout.Space(10);
                EditorUtils.PrintExplanationLabel("Unreliable messages are fast and has less bandwidth consumed, but you need to send with a high frequency to mitigate any lost", "oo_info");
                GUILayout.Space(5);
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(45);
            EditorUtils.PrintSimpleExplanation("This option will be used only for object update status messages");
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.EndVertical();

            EditorUtils.PrintBoolean(ref this.useNetworkLogin, "Enable Login", "oo_login");
            if (this.useNetworkLogin.boolValue) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15.0f);
                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                EditorGUILayout.BeginVertical();
                GUILayout.Space(10);
                this.DrawLoginInformationProvider();
                GUILayout.Space(10);
                EditorUtils.PrintExplanationLabel("This option will send from client's to server all data needed to perform login on server", "oo_info");
                GUILayout.Space(10.0f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20.0f);
                EditorUtils.PrintSimpleExplanation("Note: This option will only send data and use for internal purposes, any login validation must be implemented by developper");
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10.0f);
                EditorUtils.PrintBooleanSquaredByRef(ref this.enableLoginValidation, "Enable Validation", null, 14, 12);
                if (this.enableLoginValidation.boolValue) {
                    this.DrawLoginValidationProvider();
                    GUILayout.Space(10);
                    EditorUtils.PrintExplanationLabel("Validation must ensure that information provided by login information are valid and correct", "oo_info");
                }
                GUILayout.Space(10.0f);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorUtils.PrintBoolean(ref this.encryptionEnabled, "Enable encryption", "oo_encryption");
            if (this.encryptionEnabled.boolValue) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15.0f);
                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                EditorGUILayout.BeginVertical();
                GUILayout.Space(10);
                this.DrawencryptionProvider();
                GUILayout.Space(10);
                EditorUtils.PrintExplanationLabel("This option allows to encript data when send and receive messages", "oo_info");
                GUILayout.Space(10.0f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20.0f);
                EditorUtils.PrintSimpleExplanation("Note: Encryption can reduce performance since it adds an extra step of computation to encrypt and decrypt each transmitted data");
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10.0f);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorUtils.PrintBoolean(ref this.useNetworkClock, "Network Clock", "oo_clock");

            EditorUtils.PrintBoolean(ref this.latencyTolerance, "Latency Tolerance", "oo_latency");
            if (this.latencyTolerance.boolValue) {
                EditorUtils.DrawHorizontalIntBar(ref this.goodLatencyValue, "Good (ms)", 0, 200, 12);
                EditorUtils.DrawHorizontalIntBar(ref this.acceptableLatencyValue, "Acceptable (ms)", 0, 200, 12);
                this.acceptableLatencyValue.intValue = Mathf.Clamp(this.acceptableLatencyValue.intValue, this.goodLatencyValue.intValue, 200);

                // Print explanation
#if ENABLE_ECHO_INPUT
                GUILayout.Space(15.0f);
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                GUILayout.Space(5.0f);
                EditorUtils.PrintExplanationLabel("How latency system affect controls [ Avoid input lag ]", "oo_info");
                GUILayout.Space(5.0f);
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                GUILayout.Space(5.0f);                       
                EditorUtils.PrintExplanationLabel("Player with good latency will use his input echo only", "oo_low_latency", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                GUILayout.Space(5.0f);
                EditorUtils.PrintExplanationLabel("Player with acceptable latency will start using his local input and then will use his input echo after first arrive", "oo_mediun_latency", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                GUILayout.Space(5.0f);
                EditorUtils.PrintExplanationLabel("Player with high latency will use his local input always", "oo_high_latency", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                GUILayout.Space(10.0f);
#else
                EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                GUILayout.Space(15.0f);
                EditorUtils.PrintExplanationLabel("Latency toolerance will directly affect movement prediction calculation.", "oo_info");
                GUILayout.Space(5.0f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(50.0f);
                EditorGUILayout.BeginVertical();
                EditorUtils.PrintSimpleExplanation("Not all games behave or need movement prediction in the same way. Adjust tolerance values according to your game purposes to get better results");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(15.0f);
                EditorGUILayout.EndVertical();
                GUILayout.Space(10.0f);
#endif
            }

            EditorUtils.PrintBoolean(ref this.useInterpolation, "Movement Interpolation", "oo_kinematic");
            if (this.useInterpolation.boolValue) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10.0f);
                EditorUtils.PrintExplanationLabel("Interpolation will try to move objects smoothly from one place to another when some movement occurs ", "oo_info");
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10.0f);
            }

            EditorUtils.PrintBoolean(ref this.useMovementPrediction, "Movement Prediction", "oo_prediction");
            if (this.useMovementPrediction.boolValue) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15.0f);
                EditorGUILayout.BeginVertical();

                Color previousColor = GUI.backgroundColor;
                Color previousFontColor = GUI.contentColor;
                GUI.color = EditorUtils.SUB_DETAIL_BACKGROUND_COLOR;
                GUI.contentColor = EditorUtils.SUB_DETAIL_FONT_COLOR;
                EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                EditorUtils.PrintHeader("Prediction Parameters", EditorUtils.SUB_DETAIL_PANEL_COLOR, Color.white, 12, "oo_parameters");
                EditorUtils.DrawHorizontalIntBar(ref this.predictionBufferSize, "Prediction Buffer Size", 10, 120);
                EditorUtils.PrintExplanationLabel("How many positions system will keep in cache to perform prediction correctly", "oo_info");

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(15);
                if (EditorUtils.PrintBoolean(ref this.useAutomaticPrediction, "Automatic", "oo_search")) {
                    this.usePhysicsPrediction = !this.useAutomaticPrediction;
                    this.useTransformPrediction = !this.useAutomaticPrediction;
                }
                GUILayout.Space(10);
                if (EditorUtils.PrintBoolean(ref this.useTransformPrediction, "By Transform", "oo_gizmo")) {
                    this.usePhysicsPrediction = !this.useTransformPrediction;
                    this.useAutomaticPrediction = !this.useTransformPrediction;
                }
                GUILayout.Space(10);
                if (EditorUtils.PrintBoolean(ref this.usePhysicsPrediction, "By Physics", "oo_rigidbody")) {
                    this.useTransformPrediction = !this.usePhysicsPrediction;
                    this.useAutomaticPrediction = !this.usePhysicsPrediction;
                }
                GUILayout.Space(15.0f);

                if ( this.useAutomaticPrediction ) this.predictionType.enumValueFlag = (int)PredictionType.Automatic;
                else if (this.useTransformPrediction) this.predictionType.enumValueFlag = (int)PredictionType.UseTransform;
                else if (this.usePhysicsPrediction) this.predictionType.enumValueFlag = (int)PredictionType.UsePhysics;

                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, Vector2.zero);
                GUILayout.Space(5.0f);

                EditorGUILayout.BeginHorizontal();
                EditorUtils.PrintExplanationLabel("Latency Factor", "oo_latency", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                GUILayout.FlexibleSpace();
                this.predictionFactor.animationCurveValue = EditorGUILayout.CurveField(this.predictionFactor.animationCurveValue, GUILayout.Width(250));
                GUILayout.Space(5);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
                EditorUtils.PrintExplanationLabel("Latency will affect prediction directly, an ascendant curve means that the bigger the latency, the more in-front moving objects shall appear", "oo_info");
                GUILayout.Space(10.0f);

                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, Vector2.zero);
                GUILayout.Space(5.0f);
                EditorGUILayout.BeginHorizontal();
                EditorUtils.PrintExplanationLabel("Speed Factor", "oo_delivery", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR, 15, 16);
                GUILayout.FlexibleSpace();
                this.predictionSpeedFactor.animationCurveValue = EditorGUILayout.CurveField(this.predictionSpeedFactor.animationCurveValue, GUILayout.Width(250));
                GUILayout.Space(5);
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);
                EditorUtils.PrintExplanationLabel("According to velocity, objects and players can be less or more in front of their real position. Use an ascendant curve to represent that.", "oo_info");
                GUILayout.Space(10.0f);

                if (!this.latencyTolerance.boolValue) {
                    EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, Vector2.zero);
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("To use movement prediction the latency toolerante must be enabled", "oo_error");
                    GUILayout.Space(5.0f);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();

                // Custom prediction system
                EditorGUILayout.BeginVertical();
                EditorUtils.PrintBooleanSquaredByRef(ref this.overridePrediction, "Use custom prediction", "oo_gizmo", 14, 12);
                if (this.overridePrediction.boolValue) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20.0f);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(5.0f);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label("Object with component");
                    this.predictionProviderObject.objectReferenceValue = (EditorGUILayout.ObjectField(this.predictionProviderObject.objectReferenceValue, typeof(GameObject), false) as GameObject);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    // To select prediction component
                    if (this.predictionProviderObject.objectReferenceValue != null) {
                        List<MonoBehaviour> components = new List<MonoBehaviour>();
                        List<String> componentsName = new List<String>();
                        if (this.predictionProviderObject.objectReferenceValue != null) {
                            foreach (MonoBehaviour component in (this.predictionProviderObject.objectReferenceValue as GameObject).GetComponents<MonoBehaviour>()) {
                                if (typeof(IPrediction).IsAssignableFrom(component.GetType())) {
                                    components.Add(component);
                                    componentsName.Add(component.GetType().Name);
                                }
                            }
                        }

                        if (this.predictionProviderObject.objectReferenceValue != null) {
                            GUILayout.Space(5.0f);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.BeginVertical();
                            GUILayout.Label("Prediction Component");
                            int selectedObjectIndex = (this.predictionComponent.objectReferenceValue != null) ? Array.IndexOf(componentsName.ToArray<string>(), this.predictionComponent.objectReferenceValue.GetType().Name) : -1;
                            int selectedObject = EditorGUILayout.Popup(selectedObjectIndex, componentsName.ToArray<string>(), GUILayout.Width(250));
                            this.predictionComponent.objectReferenceValue = (((selectedObject < components.Count) && (selectedObject > -1)) ? components[selectedObject] : null);
                            EditorGUILayout.EndVertical();
                            GUILayout.Space(5.0f);
                            EditorGUILayout.EndHorizontal();
                        } else {
                            this.predictionComponent.objectReferenceValue = null;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(10.0f);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10.0f);
                    EditorUtils.PrintExplanationLabel("System will use this component to calculate the predicted position of moving objects", "oo_info");
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(10.0f);
                } else {
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("This option allows the use of some custom prediction instead of an embedded prediction system. Using this you can implement your prediction algorithm according to your game needs", "oo_info");
                    GUILayout.Space(5.0f);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                GUI.color = previousColor;
                GUI.contentColor = previousFontColor;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorUtils.PrintBoolean(ref this.useRemoteInput, "Remote Controls", "oo_gamepad");
            if (this.useRemoteInput.boolValue) {
                // Draw each managed input
                this.DrawManagedInputs();

                // Shall use echo input?
#if ENABLE_ECHO_INPUT
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15.0f);
                EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                EditorUtils.PrintBooleanSquaredByRef(ref this.useEchoInput, "Use echo input", null, 14, 12);
                GUILayout.Space(10.0f);
                EditorUtils.PrintExplanationLabel("On this mode input will be sent to the server and players will use the echo of his input according to his latency latency", "oo_info");
                GUILayout.Space(15.0f);
                if (!this.latencyToolerante.boolValue) { 
                    EditorUtils.PrintExplanationLabel("To use remote controls the latency toolerante must be enabled", "oo_error");
                }
                GUILayout.Space(10.0f);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
#else
                EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                GUILayout.Space(10.0f);
                EditorUtils.PrintExplanationLabel("This option allow to send input over network instead use local input to play.", "oo_info");
                GUILayout.Space(5.0f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(60.0f);
                EditorGUILayout.BeginVertical();
                EditorUtils.PrintSimpleExplanation("* bool 	    : To check if button is down or pressed");
                EditorUtils.PrintSimpleExplanation("* float	    : To check some pressing value or stick tilt");
                EditorUtils.PrintSimpleExplanation("* Vector2  : To provide stick, dpad or any 2d axys");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.Space(15.0f);
#endif
            }

            EditorUtils.PrintBoolean(ref this.useInternalPing, "Measure Latency", "oo_ping");
            if (this.useInternalPing.boolValue) {
                GUILayout.Space(10.0f);
                EditorUtils.PrintExplanationLabel("This option enable an internal ping to measure a more accurated latency used internally.", "oo_info");
                GUILayout.Space(10.0f);
            }

            EditorUtils.PrintBoolean(ref this.enablePlayerSpawner, "Player Prefab", "oo_player");
            if (this.enablePlayerSpawner.boolValue) {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15.0f);
                this.dynamicPlayerSpawn = (this.playerSpawnMode.enumValueIndex == (int)NetworkPlayerSpawnMode.DynamicElement);
                this.staticPlayerSpawn = (this.playerSpawnMode.enumValueIndex == (int)NetworkPlayerSpawnMode.SingleElement);
                if (EditorUtils.PrintBooleanSquared(ref this.staticPlayerSpawn, "Use Single Prefab", null, 14, 12)) {
                    this.dynamicPlayerSpawn = !this.staticPlayerSpawn;
                }
                if (EditorUtils.PrintBooleanSquared(ref this.dynamicPlayerSpawn, "Use Dynamic Spawner", null, 14, 12)) {
                    this.staticPlayerSpawn = !this.dynamicPlayerSpawn;
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);
                if (this.dynamicPlayerSpawn) {
                    this.playerSpawnMode.enumValueIndex = (int)NetworkPlayerSpawnMode.DynamicElement;
                } else if (this.staticPlayerSpawn) {
                    this.playerSpawnMode.enumValueIndex = (int)NetworkPlayerSpawnMode.SingleElement;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20.0f);
                EditorGUILayout.BeginVertical();
                if (this.playerSpawnMode.enumValueIndex == (int)NetworkPlayerSpawnMode.DynamicElement) {
                    this.DrawPlayerSpawnerProvider();
                    GUILayout.Space(10.0f);
                    EditorUtils.PrintExplanationLabel("This option allows to dynamically select prefab that will be spawned when player enter into the game", "oo_info");
                } else if (this.playerSpawnMode.enumValueIndex == (int)NetworkPlayerSpawnMode.SingleElement) {
                    this.playerPrefabToSpawn.objectReferenceValue = (EditorGUILayout.ObjectField(this.playerPrefabToSpawn.objectReferenceValue, typeof(GameObject), false) as GameObject);
                    GUILayout.Space(10.0f);
                    if (this.automaticPlayerSpawn) {
                        EditorUtils.PrintExplanationLabel("This prefab will be spawned when player connects to the server", "oo_info");
                    } else if (this.manualPlayerSpawn) {
                        EditorUtils.PrintExplanationLabel("This prefab will be spawned when \"SpawnPlayer\" method was called", "oo_info");
                    }                    
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5.0f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20.0f);
                EditorGUILayout.BeginVertical();
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, Vector2.zero);
                EditorUtils.PrintSimpleExplanation("Spawn mode", EditorUtils.EXPLANATION_FONT_COLOR);
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR_SUB.WithAlpha(0.10f), 1.0f, Vector2.zero);
                GUILayout.Space(5.0f);
                EditorGUILayout.BeginHorizontal();
                this.automaticPlayerSpawn   = (this.playerSpawnTimeMode.enumValueIndex == (int)NetworkPlayerSpawnTime.Automatic);
                this.manualPlayerSpawn      = (this.playerSpawnTimeMode.enumValueIndex == (int)NetworkPlayerSpawnTime.Manually);                
                if (EditorUtils.PrintBoolean(ref this.automaticPlayerSpawn, "Automatic Spawn", null, 14, 12)) {
                    this.manualPlayerSpawn = !this.automaticPlayerSpawn;
                }
                if (EditorUtils.PrintBoolean(ref this.manualPlayerSpawn, "Manually Spawn", null, 14, 12)) {
                    this.automaticPlayerSpawn = !this.manualPlayerSpawn;
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10.0f);
                if (this.automaticPlayerSpawn) {
                    EditorUtils.PrintExplanationLabel("On \"Automatic Spawn\", player will be spawned right after connection was stablished", "oo_info");
                } else if (this.manualPlayerSpawn) {
                    EditorUtils.PrintExplanationLabel("On \"Manually Spawn\", you need to manually call \"SpawnPlayer\" method", "oo_info");
                }
                GUILayout.Space(10.0f);
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, Vector2.zero);

                if (this.automaticPlayerSpawn) {
                    this.playerSpawnTimeMode.enumValueIndex = (int)NetworkPlayerSpawnTime.Automatic;
                } else if (this.manualPlayerSpawn) {
                    this.playerSpawnTimeMode.enumValueIndex = (int)NetworkPlayerSpawnTime.Manually;
                }

                EditorUtils.PrintSimpleExplanation("Player Network Identification", EditorUtils.EXPLANATION_FONT_COLOR);
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR_SUB.WithAlpha(0.10f), 1.0f, Vector2.zero);
                GUILayout.Space(5.0f);
                EditorGUILayout.BeginHorizontal();
                this.useInternalNetworkIdentifier   = this.useInternalNetworkId.boolValue;
                this.useCustomNetworkIdentifier     = this.useCustomNetworkId.boolValue;
                if (EditorUtils.PrintBoolean(ref this.useInternalNetworkIdentifier, "Internal Network ID", null, 14, 12)) {
                    this.useCustomNetworkIdentifier = !this.useInternalNetworkIdentifier;
                }
                if (EditorUtils.PrintBoolean(ref this.useCustomNetworkIdentifier, "Custom Network ID", null, 14, 12)) {
                    this.useInternalNetworkIdentifier = !this.useCustomNetworkIdentifier;
                }
                this.useInternalNetworkId.boolValue = this.useInternalNetworkIdentifier;
                this.useCustomNetworkId.boolValue   = this.useCustomNetworkIdentifier;
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10.0f);
                EditorUtils.PrintExplanationLabel("Internal ID is how system identify objects over network. Use a custom netwok ID when you need to keep player on scene after disconnection, on all other cases internal ID should resolve", "oo_info");
                if (this.useCustomNetworkId.boolValue) {
                    GUILayout.Space(5.0f);
                    this.DrawNetworkIdProvider();
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintSimpleExplanation("Note: Custom network ID requires a custom code to generate a unique ID for user all the time when player connects or reconnect into a game or match");
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(5);
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, Vector2.zero);
                EditorUtils.PrintSimpleExplanation("Players Movement Type", EditorUtils.EXPLANATION_FONT_COLOR);
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR_SUB.WithAlpha(0.10f), 1.0f, Vector2.zero);
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15);
                if (EditorUtils.PrintBoolean(ref this.useAutomaticMovement, "Automatic", "oo_search")) {
                    this.usePhysicsMovement = !this.useAutomaticMovement;
                    this.useTransformMovement = !this.useAutomaticMovement;
                }
                GUILayout.Space(10);
                if (EditorUtils.PrintBoolean(ref this.useTransformMovement, "By Transform", "oo_gizmo")) {
                    this.usePhysicsMovement = !this.useTransformMovement;
                    this.useAutomaticMovement = !this.useTransformMovement;
                }
                GUILayout.Space(10);
                if (EditorUtils.PrintBoolean(ref this.usePhysicsMovement, "By Physics", "oo_rigidbody")) {
                    this.useTransformMovement = !this.usePhysicsMovement;
                    this.useAutomaticMovement = !this.usePhysicsMovement;
                }
                if (this.useAutomaticMovement)
                    this.movementType.enumValueFlag = (int)PredictionType.Automatic;
                else if (this.useTransformMovement)
                    this.movementType.enumValueFlag = (int)PredictionType.UseTransform;
                else if (this.usePhysicsMovement)
                    this.movementType.enumValueFlag = (int)PredictionType.UsePhysics;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();


                // Draw position mode
                this.fixedPlayerSpawnPosition = (this.spawnPositionMode.enumValueIndex == (int)NetworkSpawnPositionMode.Fixed);
                this.multiplePlayerSpawnPosition = (this.spawnPositionMode.enumValueIndex == (int)NetworkSpawnPositionMode.Multiple);
                this.dynamicPlayerSpawnPosition = (this.spawnPositionMode.enumValueIndex == (int)NetworkSpawnPositionMode.Dynamic);
                GUILayout.Space(5.0f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20.0f);
                EditorGUILayout.BeginVertical();
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, Vector2.zero);
                EditorUtils.PrintSimpleExplanation("Position to spawn players", EditorUtils.EXPLANATION_FONT_COLOR);
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR_SUB.WithAlpha(0.10f), 1.0f, Vector2.zero);
                GUILayout.Space(5.0f);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5.0f);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15.0f);
                if (EditorUtils.PrintBooleanSquared(ref this.fixedPlayerSpawnPosition, "Fixed Position", null, 14, 12)) {
                    this.multiplePlayerSpawnPosition = !this.fixedPlayerSpawnPosition;
                    this.dynamicPlayerSpawnPosition = !this.fixedPlayerSpawnPosition;
                }
                if (EditorUtils.PrintBooleanSquared(ref this.multiplePlayerSpawnPosition, "Multiple Positions", null, 14, 12)) {
                    this.fixedPlayerSpawnPosition = !this.multiplePlayerSpawnPosition;
                    this.dynamicPlayerSpawnPosition = !this.multiplePlayerSpawnPosition;
                }
                if (EditorUtils.PrintBooleanSquared(ref this.dynamicPlayerSpawnPosition, "Dynamic Position", null, 14, 12)) {
                    this.fixedPlayerSpawnPosition = !this.dynamicPlayerSpawnPosition;
                    this.multiplePlayerSpawnPosition = !this.dynamicPlayerSpawnPosition;
                }
                EditorGUILayout.EndHorizontal();

                if (this.fixedPlayerSpawnPosition) {
                    this.spawnPositionMode.enumValueIndex = (int)NetworkSpawnPositionMode.Fixed;
                } else if (this.multiplePlayerSpawnPosition) {
                    this.spawnPositionMode.enumValueIndex = (int)NetworkSpawnPositionMode.Multiple;
                } else if (this.dynamicPlayerSpawnPosition) {
                    this.spawnPositionMode.enumValueIndex = (int)NetworkSpawnPositionMode.Dynamic;
                }

                if (this.fixedPlayerSpawnPosition) {
                    GUILayout.Space(10.0f);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20.0f);
                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                    GUILayout.Space(10.0f);
                    GUILayout.Label("Position To Spawn");
                    this.fixedPositionToSpawn.objectReferenceValue = (EditorGUILayout.ObjectField(this.fixedPositionToSpawn.objectReferenceValue, typeof(Transform), true) as Transform);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(15.0f);
                    EditorGUILayout.EndHorizontal();
                } else if (this.multiplePlayerSpawnPosition) {
                    this.DrawPlayerSpawnPositions();
                } else if (this.dynamicPlayerSpawnPosition) {
                    this.DrawPositionSpawnerProvider();
                }
                GUILayout.Space(10.0f);
                // Shall control camera ?
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20.0f);
                EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                EditorUtils.PrintBooleanSquaredByRef(ref this.controlPlayerCameras, "Control player cameras", null, 14, 12);
                GUILayout.Space(10.0f);
                EditorUtils.PrintExplanationLabel("Control camera will ensure that only one camera will exists on scene", "oo_info");
                GUILayout.Space(5.0f);
                EditorUtils.PrintBooleanSquaredByRef(ref this.detachPlayerCamera, "Detach camera from player", null, 14, 12);
                GUILayout.Space(10.0f);
                EditorUtils.PrintExplanationLabel("Detach GameObject with camera control from player after spawn", "oo_info");
                GUILayout.Space(10.0f);
                EditorUtils.PrintBooleanSquaredByRef(ref this.despawnPlayerOnDisconnect, "Remove player on disconnect", null, 14, 12);
                GUILayout.Space(10.0f);
                EditorUtils.PrintExplanationLabel("Remove player from scene when player disconnect", "oo_info");
                GUILayout.Space(10.0f);
                if (!this.despawnPlayerOnDisconnect.boolValue) {
                    EditorUtils.PrintBooleanSquaredByRef(ref this.despawnPlayerAfterDelay, "Remove disconnect after time", null, 14, 12);
                    GUILayout.Space(10.0f);
                    string currentTime = "";
                    if (this.delayToRemoveDisconnected.intValue < 60) {
                        currentTime = String.Format("{0:D2} sec", this.delayToRemoveDisconnected.intValue);
                    } else {
                        currentTime = String.Format("{0:D2}:{1:D2} min", Mathf.FloorToInt(this.delayToRemoveDisconnected.intValue / 60), (this.delayToRemoveDisconnected.intValue % 60));
                    }
                    EditorUtils.DrawHorizontalIntBar(ref this.delayToRemoveDisconnected, "Delay to remove", 1, 60 * 5, EditorUtils.DEFAULT_SLIDER_FONT_SIZE, currentTime);
                    GUILayout.Space(10.0f);
                    EditorUtils.PrintExplanationLabel("Wait a certain amount of time before remove disconnected player from scene", "oo_info");
                    GUILayout.Space(10.0f);
                    if (!this.useCustomNetworkId.boolValue) {
                        EditorUtils.PrintSimpleExplanation("Note: To reconnect player and keep his informations on the same game/match you may also to define a custom network ID generator");
                        GUILayout.Space(5.0f);
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                GUILayout.Space(10.0f);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the server address provider.
        /// </summary>
        private void DrawServerAddressProvider() {
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintSimpleExplanation("Select object that containt the script with method to provide server address list");
            EditorUtils.HorizontalLine(Color.gray, 1.0f, Vector2.zero);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Object");
            this.requestServersObject.objectReferenceValue = (EditorGUILayout.ObjectField(this.requestServersObject.objectReferenceValue, typeof(GameObject), true, GUILayout.Width(250)) as GameObject);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            List<MonoBehaviour> components = new List<MonoBehaviour>();
            List<String> componentsName = new List<String>();
            if (this.requestServersObject.objectReferenceValue != null) {
                foreach (MonoBehaviour component in (this.requestServersObject.objectReferenceValue as GameObject).GetComponents<MonoBehaviour>()) {
                    if (typeof(IInformationProvider).IsAssignableFrom(component.GetType())) {
                        components.Add(component);
                        componentsName.Add(component.GetType().Name);
                    }
                }
            }

            if (this.requestServersObject.objectReferenceValue != null) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Component");
                int selectedObjectIndex = (this.requestServerComponent.objectReferenceValue != null) ? Array.IndexOf(componentsName.ToArray<string>(), this.requestServerComponent.objectReferenceValue.GetType().Name) : -1;
                int selectedObject = EditorGUILayout.Popup(selectedObjectIndex, componentsName.ToArray<string>(), GUILayout.Width(250));
                this.requestServerComponent.objectReferenceValue = (((selectedObject < components.Count) && (selectedObject > -1)) ? components[selectedObject] : null);
                EditorGUILayout.EndHorizontal();
                if (selectedObject > -1) {
                    List<MethodInfo> methods = new List<MethodInfo>();
                    List<String> methodsName = new List<String>();
                    if (this.requestServerComponent.objectReferenceValue != null) {
                        foreach (MethodInfo method in this.requestServerComponent.objectReferenceValue.GetType().GetMethods(BindingFlags.DeclaredOnly |
                                                                                                                            BindingFlags.NonPublic |
                                                                                                                            BindingFlags.Instance |
                                                                                                                            BindingFlags.Public)) {
                            if (method.ReturnParameter.ParameterType == typeof(ServerAddressEntry[])) {
                                methods.Add(method);
                                methodsName.Add(method.Name);
                            }
                        }
                    }
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Method");
                    int selectedMethodIndex = ((this.requestServerMethod != null) && (this.requestServerMethod.stringValue != null)) ? Array.IndexOf(methodsName.ToArray<string>(), this.requestServerMethod.stringValue) : -1;
                    int selectedMethod = EditorGUILayout.Popup(selectedMethodIndex, methodsName.ToArray<string>(), GUILayout.Width(250));
                    EditorGUILayout.EndHorizontal();
                    if ((selectedMethod > -1) && (selectedMethod < methods.Count)) {
                        this.requestServerMethod.stringValue = methods[selectedMethod].Name;
                    } else if (this.requestServerMethod != null) {
                        this.requestServerMethod.stringValue = null;
                    }
                }
            } else if (this.requestServerMethod != null) {
                this.requestServerMethod.stringValue = null;
            }
        }

        /// <summary>
        /// Draws the player spawner provider.
        /// </summary>
        private void DrawPlayerSpawnerProvider() {
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintSimpleExplanation("Select object that containt script with method to provide the prefab to be spawned");
            EditorUtils.HorizontalLine(Color.gray, 1.0f, Vector2.zero);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Object Source");
            this.playerSpawnerProviderObject.objectReferenceValue = (EditorGUILayout.ObjectField(this.playerSpawnerProviderObject.objectReferenceValue, typeof(GameObject), true, GUILayout.Width(250)) as GameObject);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            List<MonoBehaviour> components = new List<MonoBehaviour>();
            List<String> componentsName = new List<String>();
            if (this.playerSpawnerProviderObject.objectReferenceValue != null) {
                foreach (MonoBehaviour component in (this.playerSpawnerProviderObject.objectReferenceValue as GameObject).GetComponents<MonoBehaviour>()) {
                    if (typeof(IInformationProvider).IsAssignableFrom(component.GetType())) {
                        components.Add(component);
                        componentsName.Add(component.GetType().Name);
                    }
                }
            }

            if (this.playerSpawnerProviderObject.objectReferenceValue != null) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Component Source");
                int selectedObjectIndex = (this.playerSpawnerComponent.objectReferenceValue != null) ? Array.IndexOf(componentsName.ToArray<string>(), this.playerSpawnerComponent.objectReferenceValue.GetType().Name) : -1;
                int selectedObject = EditorGUILayout.Popup(selectedObjectIndex, componentsName.ToArray<string>(), GUILayout.Width(250));
                this.playerSpawnerComponent.objectReferenceValue = (((selectedObject < components.Count) && (selectedObject > -1)) ? components[selectedObject] : null);
                EditorGUILayout.EndHorizontal();
                if (selectedObject > -1) {
                    List<MethodInfo> methods = new List<MethodInfo>();
                    List<String> methodsName = new List<String>();
                    if (this.playerSpawnerComponent.objectReferenceValue != null) {
                        foreach (MethodInfo method in this.playerSpawnerComponent.objectReferenceValue.GetType().GetMethods(BindingFlags.DeclaredOnly |
                                                                                                                            BindingFlags.NonPublic |
                                                                                                                            BindingFlags.Instance |
                                                                                                                            BindingFlags.Public)) {
                            if (method.ReturnParameter.ParameterType == typeof(GameObject)) {
                                methods.Add(method);
                                methodsName.Add(method.Name);
                            }
                        }
                    }
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Method Source");
                    int selectedMethodIndex = ((this.playerSpawnerMethod != null) && (this.playerSpawnerMethod.stringValue != null)) ? Array.IndexOf(methodsName.ToArray<string>(), this.playerSpawnerMethod.stringValue) : -1;
                    int selectedMethod = EditorGUILayout.Popup(selectedMethodIndex, methodsName.ToArray<string>(), GUILayout.Width(250));
                    EditorGUILayout.EndHorizontal();
                    if ((selectedMethod > -1) && (selectedMethod < methods.Count)) {
                        this.playerSpawnerMethod.stringValue = methods[selectedMethod].Name;
                    } else if (this.playerSpawnerMethod != null) {
                        this.playerSpawnerMethod.stringValue = null;
                    }
                }
            } else if (this.playerSpawnerMethod != null) {
                this.playerSpawnerMethod.stringValue = null;
            }
        }

        /// <summary>
        /// Draws the position spawner provider.
        /// </summary>
        private void DrawPositionSpawnerProvider() {
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintSimpleExplanation("Select object that containt script with method to provide position spawner");
            EditorUtils.HorizontalLine(Color.gray, 1.0f, Vector2.zero);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Object Source");
            this.positionSpawnerProviderObject.objectReferenceValue = (EditorGUILayout.ObjectField(this.positionSpawnerProviderObject.objectReferenceValue, typeof(GameObject), true, GUILayout.Width(250)) as GameObject);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            List<MonoBehaviour> components = new List<MonoBehaviour>();
            List<String> componentsName = new List<String>();
            if (this.positionSpawnerProviderObject.objectReferenceValue != null) {
                foreach (MonoBehaviour component in (this.positionSpawnerProviderObject.objectReferenceValue as GameObject).GetComponents<MonoBehaviour>()) {
                    if (typeof(IInformationProvider).IsAssignableFrom(component.GetType())) {
                        components.Add(component);
                        componentsName.Add(component.GetType().Name);
                    }
                }
            }

            if (this.positionSpawnerProviderObject.objectReferenceValue != null) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Component Source");
                int selectedObjectIndex = (this.positionSpawnerComponent.objectReferenceValue != null) ? Array.IndexOf(componentsName.ToArray<string>(), this.positionSpawnerComponent.objectReferenceValue.GetType().Name) : -1;
                int selectedObject = EditorGUILayout.Popup(selectedObjectIndex, componentsName.ToArray<string>(), GUILayout.Width(250));
                this.positionSpawnerComponent.objectReferenceValue = (((selectedObject < components.Count) && (selectedObject > -1)) ? components[selectedObject] : null);
                EditorGUILayout.EndHorizontal();
                if (selectedObject > -1) {
                    List<MethodInfo> methods = new List<MethodInfo>();
                    List<String> methodsName = new List<String>();
                    if (this.positionSpawnerComponent.objectReferenceValue != null) {
                        foreach (MethodInfo method in this.positionSpawnerComponent.objectReferenceValue.GetType().GetMethods(BindingFlags.DeclaredOnly |
                                                                                                                              BindingFlags.NonPublic |
                                                                                                                              BindingFlags.Instance |
                                                                                                                              BindingFlags.Public)) {
                            if (method.ReturnParameter.ParameterType == typeof(Vector3)) {
                                methods.Add(method);
                                methodsName.Add(method.Name);
                            }
                        }
                    }
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Method Source");
                    int selectedMethodIndex = ((this.positionSpawnerMethod != null) && (this.positionSpawnerMethod.stringValue != null)) ? Array.IndexOf(methodsName.ToArray<string>(), this.positionSpawnerMethod.stringValue) : -1;
                    int selectedMethod = EditorGUILayout.Popup(selectedMethodIndex, methodsName.ToArray<string>(), GUILayout.Width(250));
                    EditorGUILayout.EndHorizontal();
                    if ((selectedMethod > -1) && (selectedMethod < methods.Count)) {
                        this.positionSpawnerMethod.stringValue = methods[selectedMethod].Name;
                    } else if (this.positionSpawnerMethod != null) {
                        this.positionSpawnerMethod.stringValue = null;
                    }
                }
            } else if (this.positionSpawnerMethod != null) {
                this.positionSpawnerMethod.stringValue = null;
            }
        }

        /// <summary>
        /// Draws the network identifier provider.
        /// </summary>
        private void DrawNetworkIdProvider() {
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintSimpleExplanation("Select object that containt script with method to provide the network id");
            EditorUtils.HorizontalLine(Color.gray, 1.0f, Vector2.zero);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Object Source");
            this.networkIdProviderObject.objectReferenceValue = (EditorGUILayout.ObjectField(this.networkIdProviderObject.objectReferenceValue, typeof(GameObject), true, GUILayout.Width(250)) as GameObject);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            List<MonoBehaviour> components = new List<MonoBehaviour>();
            List<String> componentsName = new List<String>();
            if (this.networkIdProviderObject.objectReferenceValue != null) {
                foreach (MonoBehaviour component in (this.networkIdProviderObject.objectReferenceValue as GameObject).GetComponents<MonoBehaviour>()) {
                    if (typeof(IInformationProvider).IsAssignableFrom(component.GetType())) {
                        components.Add(component);
                        componentsName.Add(component.GetType().Name);
                    }
                }
            }

            if (this.networkIdProviderObject.objectReferenceValue != null) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Component Source");
                int selectedObjectIndex = (this.networkIdComponent.objectReferenceValue != null) ? Array.IndexOf(componentsName.ToArray<string>(), this.networkIdComponent.objectReferenceValue.GetType().Name) : -1;
                int selectedObject = EditorGUILayout.Popup(selectedObjectIndex, componentsName.ToArray<string>(), GUILayout.Width(250));
                this.networkIdComponent.objectReferenceValue = (((selectedObject < components.Count) && (selectedObject > -1)) ? components[selectedObject] : null);
                EditorGUILayout.EndHorizontal();
                if (selectedObject > -1) {
                    List<MethodInfo> methods = new List<MethodInfo>();
                    List<String> methodsName = new List<String>();
                    if (this.networkIdComponent.objectReferenceValue != null) {
                        foreach (MethodInfo method in this.networkIdComponent.objectReferenceValue.GetType().GetMethods(BindingFlags.DeclaredOnly |
                                                                                                                        BindingFlags.NonPublic |
                                                                                                                        BindingFlags.Instance |
                                                                                                                        BindingFlags.Public)) {
                            if (method.ReturnParameter.ParameterType == typeof(int)) {
                                ParameterInfo[] arguments = method.GetParameters();
                                if ((arguments != null) &&
                                    (arguments.Count() == 1) &&
                                    (arguments[0].ParameterType.IsArray) &&
                                    (typeof(object).IsAssignableFrom(arguments[0].ParameterType.GetElementType()))) {
                                    methods.Add(method);
                                    methodsName.Add(method.Name);
                                }
                            }
                        }
                    }
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Method Source");
                    int selectedMethodIndex = ((this.networkIdMethod != null) && (this.networkIdMethod.stringValue != null)) ? Array.IndexOf(methodsName.ToArray<string>(), this.networkIdMethod.stringValue) : -1;
                    int selectedMethod = EditorGUILayout.Popup(selectedMethodIndex, methodsName.ToArray<string>(), GUILayout.Width(250));
                    EditorGUILayout.EndHorizontal();
                    if ((selectedMethod > -1) && (selectedMethod < methods.Count)) {
                        this.networkIdMethod.stringValue = methods[selectedMethod].Name;
                    } else if (this.networkIdMethod != null) {
                        this.networkIdMethod.stringValue = null;
                    }
                }
            } else if (this.networkIdMethod != null) {
                this.networkIdMethod.stringValue = null;
            }
        }

        /// <summary>
        /// Draws the login information provider.
        /// </summary>
        private void DrawLoginInformationProvider() {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintSimpleExplanation("Select object that containt script with method to provide login informations");
            GUILayout.Space(5.0f);
            EditorUtils.HorizontalLine(Color.gray, 1.0f, Vector2.zero);
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Object Source");
            this.loginInfoProviderObject.objectReferenceValue = (EditorGUILayout.ObjectField(this.loginInfoProviderObject.objectReferenceValue, typeof(GameObject), true, GUILayout.Width(250)) as GameObject);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            List<MonoBehaviour> components = new List<MonoBehaviour>();
            List<String> componentsName = new List<String>();
            if (this.loginInfoProviderObject.objectReferenceValue != null) {
                foreach (MonoBehaviour component in (this.loginInfoProviderObject.objectReferenceValue as GameObject).GetComponents<MonoBehaviour>()) {
                    if (typeof(IInformationProvider).IsAssignableFrom(component.GetType())) {
                        components.Add(component);
                        componentsName.Add(component.GetType().Name);
                    }
                }
            }

            if (this.loginInfoProviderObject.objectReferenceValue != null) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Component Source");
                int selectedObjectIndex = (this.loginInfoComponent.objectReferenceValue != null) ? Array.IndexOf(componentsName.ToArray<string>(), this.loginInfoComponent.objectReferenceValue.GetType().Name) : -1;
                int selectedObject = EditorGUILayout.Popup(selectedObjectIndex, componentsName.ToArray<string>(), GUILayout.Width(250));
                this.loginInfoComponent.objectReferenceValue = (((selectedObject < components.Count) && (selectedObject > -1)) ? components[selectedObject] : null);
                EditorGUILayout.EndHorizontal();
                if (selectedObject > -1) {
                    List<MethodInfo> methods = new List<MethodInfo>();
                    List<MethodInfo> methodsTypes = new List<MethodInfo>();
                    List<String> methodsName = new List<String>();
                    List<String> methodsTypesName = new List<String>();
                    if (this.loginInfoComponent.objectReferenceValue != null) {
                        foreach (MethodInfo method in this.loginInfoComponent.objectReferenceValue.GetType().GetMethods(BindingFlags.Public
                                                                                                                        | BindingFlags.NonPublic
                                                                                                                        | BindingFlags.Instance
                                                                                                                        | BindingFlags.DeclaredOnly)) {
                            if ((method.ReturnParameter.ParameterType.IsArray) &&
                                (typeof(object).IsAssignableFrom(method.ReturnParameter.ParameterType.GetElementType()))) {
                                methods.Add(method);
                                methodsName.Add(method.Name);
                            }
                        }
                    }
                    if (this.loginInfoComponent.objectReferenceValue != null) {
                        foreach (MethodInfo method in this.loginInfoComponent.objectReferenceValue.GetType().GetMethods(BindingFlags.Public
                                                                                                                        | BindingFlags.NonPublic
                                                                                                                        | BindingFlags.Instance
                                                                                                                        | BindingFlags.DeclaredOnly)) {
                            if ((method.ReturnParameter.ParameterType.IsArray) &&
                                (typeof(Type).IsAssignableFrom(method.ReturnParameter.ParameterType.GetElementType()))) {
                                methodsTypes.Add(method);
                                methodsTypesName.Add(method.Name);
                            }
                        }
                    }
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Method [ For Values ]");
                    int selectedMethodIndex = ((this.loginInfoMethod != null) && (this.loginInfoMethod.stringValue != null)) ? Array.IndexOf(methodsName.ToArray<string>(), this.loginInfoMethod.stringValue) : -1;
                    int selectedMethod = EditorGUILayout.Popup(selectedMethodIndex, methodsName.ToArray<string>(), GUILayout.Width(250));
                    EditorGUILayout.EndHorizontal();
                    if ((selectedMethod > -1) && (selectedMethod < methods.Count)) {
                        this.loginInfoMethod.stringValue = methods[selectedMethod].Name;
                    } else if (this.loginInfoMethod != null) {
                        this.loginInfoMethod.stringValue = null;
                    }

                    // loginTypesMethod
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Method [ For Types ]");
                    int selectedTypesMethodIndex = ((this.loginInfoMethod != null) && (this.loginInfoMethod.stringValue != null)) ? Array.IndexOf(methodsTypesName.ToArray<string>(), this.loginTypesMethod.stringValue) : -1;
                    int selectedTypesMethod = EditorGUILayout.Popup(selectedTypesMethodIndex, methodsTypesName.ToArray<string>(), GUILayout.Width(250));
                    EditorGUILayout.EndHorizontal();
                    if ((selectedTypesMethod > -1) && (selectedTypesMethod < methodsTypes.Count)) {
                        this.loginTypesMethod.stringValue = methodsTypes[selectedTypesMethod].Name;
                    } else if (this.loginTypesMethod != null) {
                        this.loginTypesMethod.stringValue = null;
                    }

                }
            } else if (this.loginInfoMethod != null) {
                this.loginInfoMethod.stringValue = null;
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(10.0f);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the login validation provider.
        /// </summary>
        private void DrawLoginValidationProvider() {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintSimpleExplanation("Select object that containt script with method to execute login validation");
            GUILayout.Space(5.0f);
            EditorUtils.HorizontalLine(Color.gray, 1.0f, Vector2.zero);
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Object Source");
            this.loginValidationProviderObject.objectReferenceValue = (EditorGUILayout.ObjectField(this.loginValidationProviderObject.objectReferenceValue, typeof(GameObject), true, GUILayout.Width(250)) as GameObject);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            List<MonoBehaviour> components = new List<MonoBehaviour>();
            List<String> componentsName = new List<String>();
            if (this.loginValidationProviderObject.objectReferenceValue != null) {
                foreach (MonoBehaviour component in (this.loginValidationProviderObject.objectReferenceValue as GameObject).GetComponents<MonoBehaviour>()) {
                    if (typeof(IInformationProvider).IsAssignableFrom(component.GetType())) {
                        components.Add(component);
                        componentsName.Add(component.GetType().Name);
                    }
                }
            }

            if (this.loginValidationProviderObject.objectReferenceValue != null) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Component Source");
                int selectedObjectIndex = (this.loginValidationComponent.objectReferenceValue != null) ? Array.IndexOf(componentsName.ToArray<string>(), this.loginValidationComponent.objectReferenceValue.GetType().Name) : -1;
                int selectedObject = EditorGUILayout.Popup(selectedObjectIndex, componentsName.ToArray<string>(), GUILayout.Width(250));
                this.loginValidationComponent.objectReferenceValue = (((selectedObject < components.Count) && (selectedObject > -1)) ? components[selectedObject] : null);
                EditorGUILayout.EndHorizontal();
                if (selectedObject > -1) {
                    List<MethodInfo> methods = new List<MethodInfo>();
                    List<String> methodsName = new List<String>();
                    if (this.loginValidationComponent.objectReferenceValue != null) {
                        foreach (MethodInfo method in this.loginValidationComponent.objectReferenceValue.GetType().GetMethods(BindingFlags.Public
                                                                                                                              | BindingFlags.NonPublic
                                                                                                                              | BindingFlags.Instance
                                                                                                                              | BindingFlags.DeclaredOnly)) {
                            if (method.ReturnParameter.ParameterType == typeof(bool)) {
                                ParameterInfo[] arguments = method.GetParameters();
                                if ((arguments != null) &&
                                    (arguments.Count() == 1) &&
                                    (arguments[0].ParameterType.IsArray) &&
                                    (typeof(object).IsAssignableFrom(arguments[0].ParameterType.GetElementType()))) {
                                    methods.Add(method);
                                    methodsName.Add(method.Name);
                                }
                            }
                        }
                    }
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Method Source");
                    int selectedMethodIndex = ((this.loginValidationMethod != null) && (this.loginValidationMethod.stringValue != null)) ? Array.IndexOf(methodsName.ToArray<string>(), this.loginValidationMethod.stringValue) : -1;
                    int selectedMethod = EditorGUILayout.Popup(selectedMethodIndex, methodsName.ToArray<string>(), GUILayout.Width(250));
                    EditorGUILayout.EndHorizontal();
                    if ((selectedMethod > -1) && (selectedMethod < methods.Count)) {
                        this.loginValidationMethod.stringValue = methods[selectedMethod].Name;
                    } else if (this.loginValidationMethod != null) {
                        this.loginValidationMethod.stringValue = null;
                    }
                }
            } else if (this.loginValidationMethod != null) {
                this.loginValidationMethod.stringValue = null;
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(10.0f);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the encryption provider.
        /// </summary>
        private void DrawencryptionProvider() {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintSimpleExplanation("Select object that containt script with method to execute encryption/decryption of messages");
            GUILayout.Space(5.0f);
            EditorUtils.HorizontalLine(Color.gray, 1.0f, Vector2.zero);
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Object Source");
            this.encryptionProviderObject.objectReferenceValue = (EditorGUILayout.ObjectField(this.encryptionProviderObject.objectReferenceValue, typeof(GameObject), true, GUILayout.Width(250)) as GameObject);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            List<MonoBehaviour> components = new List<MonoBehaviour>();
            List<String> componentsName = new List<String>();
            if (this.encryptionProviderObject.objectReferenceValue != null) {
                foreach (MonoBehaviour component in (this.encryptionProviderObject.objectReferenceValue as GameObject).GetComponents<MonoBehaviour>()) {
                    if (typeof(IInformationProvider).IsAssignableFrom(component.GetType())) {
                        components.Add(component);
                        componentsName.Add(component.GetType().Name);
                    }
                }
            }

            if (this.encryptionProviderObject.objectReferenceValue != null) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Component Source");
                int selectedObjectIndex = (this.encryptionComponent.objectReferenceValue != null) ? Array.IndexOf(componentsName.ToArray<string>(), this.encryptionComponent.objectReferenceValue.GetType().Name) : -1;
                int selectedObject = EditorGUILayout.Popup(selectedObjectIndex, componentsName.ToArray<string>(), GUILayout.Width(250));
                this.encryptionComponent.objectReferenceValue = (((selectedObject < components.Count) && (selectedObject > -1)) ? components[selectedObject] : null);
                EditorGUILayout.EndHorizontal();
                if (selectedObject > -1) {
                    List<MethodInfo> methods = new List<MethodInfo>();
                    List<String> methodsName = new List<String>();
                    if (this.encryptionComponent.objectReferenceValue != null) {
                        foreach (MethodInfo method in this.encryptionComponent.objectReferenceValue.GetType().GetMethods(BindingFlags.Public
                                                                                                                         | BindingFlags.NonPublic
                                                                                                                         | BindingFlags.Instance
                                                                                                                         | BindingFlags.DeclaredOnly)) {
                            if (method.ReturnParameter.ParameterType == typeof(byte[])) {
                                ParameterInfo[] arguments = method.GetParameters();
                                if ((arguments != null) &&
                                    (arguments.Count() == 1) &&
                                    (arguments[0].ParameterType.IsArray) &&
                                    (typeof(byte).IsAssignableFrom(arguments[0].ParameterType.GetElementType()))) {
                                    methods.Add(method);
                                    methodsName.Add(method.Name);
                                }
                            }
                        }
                    }
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Encryption method");
                    int selectedencryptionMethodIndex = ((this.encryptionMethod != null) && (this.encryptionMethod.stringValue != null)) ? Array.IndexOf(methodsName.ToArray<string>(), this.encryptionMethod.stringValue) : -1;
                    int selectedencryptionMethod = EditorGUILayout.Popup(selectedencryptionMethodIndex, methodsName.ToArray<string>(), GUILayout.Width(250));
                    EditorGUILayout.EndHorizontal();
                    if ((selectedencryptionMethod > -1) && (selectedencryptionMethod < methods.Count)) {
                        this.encryptionMethod.stringValue = methods[selectedencryptionMethod].Name;
                    } else if (this.encryptionMethod != null) {
                        this.encryptionMethod.stringValue = null;
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Decryption method");
                    int selecteddecryptionMethodIndex = ((this.decryptionMethod != null) && (this.decryptionMethod.stringValue != null)) ? Array.IndexOf(methodsName.ToArray<string>(), this.decryptionMethod.stringValue) : -1;
                    int selecteddecryptionMethod = EditorGUILayout.Popup(selecteddecryptionMethodIndex, methodsName.ToArray<string>(), GUILayout.Width(250));
                    EditorGUILayout.EndHorizontal();
                    if ((selecteddecryptionMethod > -1) && (selecteddecryptionMethod < methods.Count)) {
                        this.decryptionMethod.stringValue = methods[selecteddecryptionMethod].Name;
                    } else if (this.decryptionMethod != null) {
                        this.decryptionMethod.stringValue = null;
                    }
                }
            } else {
                if (this.encryptionMethod != null) {
                    this.encryptionMethod.stringValue = null;
                }
                if (this.decryptionMethod != null) {
                    this.decryptionMethod.stringValue = null;
                }
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(10.0f);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the animation modes.
        /// </summary>
        /// <param name="selectedIndex">Index of the selected.</param>
        /// <returns>AnimationSyncType.</returns>
        private AnimationSyncType DrawAnimationModes(AnimationSyncType selectedIndex) {
            AnimationSyncType result = AnimationSyncType.UseController;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);
            int selectedAnimationTypeIndex = (int)selectedIndex;
            List<string> animationTypesName = new List<string>();
            animationTypesName.Add(AnimationSyncType.UseController.ToString());
            animationTypesName.Add(AnimationSyncType.UseParameters.ToString());
            int previousSize = GUI.skin.label.fontSize;
            int selectedMethod = EditorGUILayout.Popup(selectedAnimationTypeIndex, animationTypesName.ToArray<string>(), GUILayout.Width(150));
            if (selectedMethod == (int)AnimationSyncType.UseController) {
                result = AnimationSyncType.UseController;
            } else if (selectedMethod == (int)AnimationSyncType.UseParameters) {
                result = AnimationSyncType.UseParameters;
            }
            EditorGUILayout.EndHorizontal();
            return result;
        }

        /// <summary>
        /// Draws the player spawn positions.
        /// </summary>
        private void DrawPlayerSpawnPositions() {
            GUILayout.Space(10.0f);
            EditorUtils.PrintSimpleExplanation("Players will be random spawned into one of those positions");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15.0f);
            if (GUILayout.Button(Resources.Load("oo_add") as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18))) {
                this.multiplePositionsToSpawn.InsertArrayElementAtIndex(this.multiplePositionsToSpawn.arraySize);
            }
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 14;
            GUILayout.Label("Register Position");
            GUI.skin.label.fontSize = previousSize;
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10.0f);

            EditorGUILayout.BeginVertical();
            int removeIndex = -1;
            for (int prefabIndex = 0; prefabIndex < this.multiplePositionsToSpawn.arraySize; prefabIndex++) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15.0f);
                if (GUILayout.Button(Resources.Load("oo_remove") as Texture, GUIStyle.none, GUILayout.Width(14), GUILayout.Height(14))) {
                    removeIndex = prefabIndex;
                }
                GUILayout.Space(5.0f);
                this.multiplePositionsToSpawn.GetArrayElementAtIndex(prefabIndex).objectReferenceValue = (EditorGUILayout.ObjectField(this.multiplePositionsToSpawn.GetArrayElementAtIndex(prefabIndex).objectReferenceValue, typeof(Transform), true) as Transform);
                GUILayout.Space(5.0f);
                EditorGUILayout.EndHorizontal();
            }
            if (removeIndex > -1) {
                this.multiplePositionsToSpawn.DeleteArrayElementAtIndex(removeIndex);
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(15.0f);
        }

        /// <summary>
        /// Draws the managed inputs.
        /// </summary>
        private void DrawManagedInputs() {
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20.0f);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorUtils.PrintImageButton("Register Input", "oo_add", EditorUtils.IMAGE_BUTTON_COLOR, EditorUtils.IMAGE_BUTTON_FONT_COLOR, 12, 14f, 14f, () => {
                this.inputList.InsertArrayElementAtIndex(this.inputList.arraySize);
                this.inputList.GetArrayElementAtIndex(this.inputList.arraySize - 1).objectReferenceValue = new ManagedInput((byte)this.inputList.arraySize, string.Format("Input_{0}", this.inputList.arraySize));
            });
            EditorGUILayout.EndHorizontal();

            List<MethodInfo> methods = new List<MethodInfo>();
            List<String> methodsName = new List<String>();
            if (this.inputList.arraySize > 0) {
                GUILayout.Space(5.0f);
                EditorUtils.PrintExplanationLabel("Provide object that contains script with input methods.", "oo_info");
                GUILayout.Space(10.0f);

                EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                GUILayout.Space(5.0f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5.0f);
                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Input Source");
                this.inputProviderObject.objectReferenceValue = (EditorGUILayout.ObjectField(this.inputProviderObject.objectReferenceValue, typeof(GameObject), true, GUILayout.Width(250)) as GameObject);
                EditorGUILayout.EndHorizontal();

                List<MonoBehaviour> components = new List<MonoBehaviour>();
                List<String> componentsName = new List<String>();
                if (this.inputProviderObject.objectReferenceValue != null) {
                    foreach (MonoBehaviour component in (this.inputProviderObject.objectReferenceValue as GameObject).GetComponents<MonoBehaviour>()) {
                        if (typeof(IInputProvider).IsAssignableFrom(component.GetType())) {
                            components.Add(component);
                            componentsName.Add(component.GetType().Name);
                        }
                    }
                }
                // Listar os monobehaviros
                if (this.inputProviderObject.objectReferenceValue != null) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Component");
                    int selectedObjectIndex = (this.inputComponent.objectReferenceValue != null) ? Array.IndexOf(componentsName.ToArray<string>(), this.inputComponent.objectReferenceValue.GetType().Name) : -1;
                    int selectedObject = EditorGUILayout.Popup(selectedObjectIndex, componentsName.ToArray<string>(), GUILayout.Width(250));
                    this.inputComponent.objectReferenceValue = (((selectedObject < components.Count) && (selectedObject > -1)) ? components[selectedObject] : null);
                    EditorGUILayout.EndHorizontal();
                    if (selectedObject > -1) {
                        if (this.inputComponent.objectReferenceValue != null) {
                            foreach (MethodInfo method in this.inputComponent.objectReferenceValue.GetType().GetMethods(BindingFlags.DeclaredOnly |
                                                                                                                        BindingFlags.NonPublic |
                                                                                                                        BindingFlags.Instance |
                                                                                                                        BindingFlags.Public)) {
                                if ((method.ReturnParameter.ParameterType == typeof(bool)) ||
                                    (method.ReturnParameter.ParameterType == typeof(float)) ||
                                    (method.ReturnParameter.ParameterType == typeof(Vector2))) {
                                    methods.Add(method);
                                    methodsName.Add(method.Name);
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(5.0f);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5.0f);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            int removeIndex = -1;
            float endOffset = 5f;
            if (this.inputList.arraySize > 0) {
                GUILayout.Space(10f);
            }
            for (int inputIndex = 0; inputIndex < this.inputList.arraySize; inputIndex++) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15.0f);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(3.0f);
                if (GUILayout.Button(Resources.Load("oo_delete") as Texture, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15))) {
                    removeIndex = inputIndex;
                }
                EditorGUILayout.EndVertical();

                GUILayout.Space(5.0f);
                ManagedInput inputEntry = (this.inputList.GetArrayElementAtIndex(inputIndex).objectReferenceValue as ManagedInput);
                inputEntry.SetInputName(EditorGUILayout.TextField(inputEntry.GetInputName(), GUILayout.Width(140)));
                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                int selectedMethodIndex = ((inputEntry != null) && (inputEntry.GetMethod() != null)) ? Array.IndexOf(methodsName.ToArray<string>(), inputEntry.GetMethod()) : -1;
                int selectedMethod = EditorGUILayout.Popup(selectedMethodIndex, methodsName.ToArray<string>(), GUILayout.Width(250));
                EditorGUILayout.EndHorizontal();
                if ((selectedMethod > -1) && (selectedMethod < methods.Count)) {
                    inputEntry.SetMethod(methods[selectedMethod].Name);
                    inputEntry.SetManagedType(methods[selectedMethod].ReturnType.FullName);
                } else if (this.requestServerMethod == null) {
                    inputEntry.SetMethod(null);
                    inputEntry.SetManagedType(null);
                }

                GUILayout.Space(5.0f);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10.0f);
                endOffset = 25f;
            }
            if (removeIndex > -1) {
                this.inputList.DeleteArrayElementAtIndex(removeIndex);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(endOffset);
        }

        /// <summary>
        /// Refreshes the database.
        /// </summary>
        public void RefreshDatabase() {
            // Load prefabs database
            serializedObject.Update();
            this.prefabsDatabase = Resources.Load<NetworkPrefabsDatabase>(GlobalResources.GetPrefabsDatabase(this.databaseTarget.stringValue));
            if (this.prefabsDatabase == null) {
                NetworkPrefabsDatabaseWindow.CreateNetworkDatabaseSource(this.databaseTarget.stringValue);
                this.prefabsDatabase = Resources.Load<NetworkPrefabsDatabase>(GlobalResources.GetPrefabsDatabase(this.databaseTarget.stringValue));
            }
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Refreshes the window.
        /// </summary>
        public void RefreshWindow() {
            this.OnInspectorGUI();
        }

    }
#endif
}
