using System;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a manager for network global events, containing event references for various network-related actions.
    /// </summary>
    public class NetworkGlobalEvents : NetworkEventsManager {

        // Event triggered when the client successfully connects to the server.
        [EventInformations(EventName = "OnServerStarted", ExecutionSide = EventReferenceSide.ServerSide, ParametersType = new Type[] { typeof(IChannel) }, EventDescriptiom = "Trigger when server or host start to listen")]
        [SerializeField]
        public EventReference onServerStarted;

        // Event triggered when the client successfully connects to the server.
        [EventInformations(EventName = "OnConnected", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(IClient) }, EventDescriptiom = "Trigger when the connection with the server is established")]
        [SerializeField]
        public EventReference onConnected;

        // Event triggered when the client disconnects from the server.
        [EventInformations(EventName = "OnDisconnected", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(IClient) }, EventDescriptiom = "Trigger when the connection with the server is lost")]
        [SerializeField]
        public EventReference onDisconnected;

        // Event triggered when the client establishes a connection with a relay server.
        [EventInformations(EventName = "OnConnectedOnRelay", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(IClient) }, EventDescriptiom = "Trigger client establish a connection with relay server")]
        [SerializeField]
        public EventReference onConnectedOnRelay;

        // Event triggered when the client detects that the server has restarted.
        [EventInformations(EventName = "OnServerRestarted", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(IClient) }, EventDescriptiom = "Trigger when client detect that server was retarted")]
        [SerializeField]
        public EventReference onServerRestarted;

        // Event triggered when the client connection with server is lost.
        [EventInformations(EventName = "OnConnectionFailed", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(Exception) }, EventDescriptiom = "Trigger when the connection with the server is lost")]
        [SerializeField]
        public EventReference onConnectionFailed;

        // Event triggered when the client fails to log in to the server.
        [EventInformations(EventName = "OnLoginFailed", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(Exception) }, EventDescriptiom = "Trigger when login with server failed")]
        [SerializeField]
        public EventReference onLoginFailed;

        // Event triggered when the client successfully logs in to the server.
        [EventInformations(EventName = "OnLoginSucess", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(IClient) }, EventDescriptiom = "Trigger when login with the server is successfully")]
        [SerializeField]
        public EventReference onLoginSucess;

        // Event triggered when a new client connects to the server.
        [EventInformations(EventName = "OnClientConnected", ExecutionSide = EventReferenceSide.ServerSide, ParametersType = new Type[] { typeof(IClient) }, EventDescriptiom = "Trigger when a new client establishes a connection with the server")]
        [SerializeField]
        public EventReference onClientConnected;

        // Event triggered when a client disconnects from the server.
        [EventInformations(EventName = "OnClientDisconnected", ExecutionSide = EventReferenceSide.ServerSide, ParametersType = new Type[] { typeof(IClient) }, EventDescriptiom = "Trigger when client lost/close his connection with the server")]
        [SerializeField]
        public EventReference onClientDisconnected;

        // Event triggered when a client attempts to log in with invalid credentials.
        [EventInformations(EventName = "OnClientLoginFailed", ExecutionSide = EventReferenceSide.ServerSide, ParametersType = new Type[] { typeof(IClient) }, EventDescriptiom = "Trigger when client try to login with invalid credentials")]
        [SerializeField]
        public EventReference onClientLoginFailed;

        // Event triggered when a client successfully logs in to the server.
        [EventInformations(EventName = "OnClientLoginSucess", ExecutionSide = EventReferenceSide.ServerSide, ParametersType = new Type[] { typeof(IClient) }, EventDescriptiom = "Trigger when client logged with on server")]
        [SerializeField]
        public EventReference onClientLoginSucess;

        // Event triggered when a message is received by either the client or the server.
        [EventInformations(EventName = "OnMessageReceived", ExecutionSide = EventReferenceSide.BothSides, ParametersType = new Type[] { typeof(IDataStream) }, EventDescriptiom = "Trigger when any message was received")]
        [SerializeField]
        public EventReference onMessageReceived;

        // Event triggered when the client successfully creates a lobby.
        [EventInformations(EventName = "OnLobbyCreationSucess", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(ushort) }, EventDescriptiom = "Trigger when lobby creation was executed successfully")]
        [SerializeField]
        public EventReference onLobbyCreationSucess;

        // Event triggered when the client fails to create a lobby.
        [EventInformations(EventName = "OnLobbyCreationFailed", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(string) }, EventDescriptiom = "Trigger when lobby creation was failed")]
        [SerializeField]
        public EventReference onLobbyCreationFailed;

        // Event triggered when the client successfully joins a lobby.
        [EventInformations(EventName = "OnLobbyJoinSucess", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(ushort) }, EventDescriptiom = "Trigger when user was joined on lobby successfully")]
        [SerializeField]
        public EventReference onLobbyJoinedSucess;

        // Event triggered when the client fails to join a lobby.
        [EventInformations(EventName = "OnLobbyJoinFailed", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(ushort) }, EventDescriptiom = "Trigger when user was joined was failed")]
        [SerializeField]
        public EventReference onLobbyJoinFailed;

        // Event triggered when the client fails to join a lobby.
        [EventInformations(EventName = "OnLobbyPlayersRefresh", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(string[]) }, EventDescriptiom = "Trigger when received lobby players list")]
        [SerializeField]
        public EventReference onLobbyPlayersRefresh;

        // Event triggered when the client fails to join a lobby.
        [EventInformations(EventName = "OnPlayerSpawned", ExecutionSide = EventReferenceSide.BothSides, ParametersType = new Type[] { typeof(GameObject) }, EventDescriptiom = "Trigger when some player was spawned")]
        [SerializeField]
        public EventReference onPlayerSpawned;

        // Event triggered when the client fails to join a lobby.
        [EventInformations(EventName = "OnPlayerStarted", ExecutionSide = EventReferenceSide.BothSides, ParametersType = new Type[] { typeof(INetworkElement) }, EventDescriptiom = "Trigger when some player start his network instance")]
        [SerializeField]
        public EventReference onPlayerStarted;

        // Event triggered when the client fails to join a lobby.
        [EventInformations(EventName = "OnPlayerOwnerDetected", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(INetworkElement) }, EventDescriptiom = "Trigger when client became owner of his player")]
        [SerializeField]
        public EventReference onPlayerOwnerDetected;

        // Event triggered when the client fails to join a lobby.
        [EventInformations(EventName = "OnSceneLoadFinished", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(string) }, EventDescriptiom = "Trigger when all clients in the same match load some scene")]
        [SerializeField]
        public EventReference onSceneLoadFinished;

        // Event triggered when the client fails to join a lobby.
        [EventInformations(EventName = "OnSceneUnLoadFinished", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(string) }, EventDescriptiom = "Trigger when all clients in the same match load some scene")]
        [SerializeField]
        public EventReference onSceneUnLoadFinished;

        // Event triggered when the client fails to join a lobby.
        [EventInformations(EventName = "OnConnectionIdAssigned", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(int) }, EventDescriptiom = "Trigger just after client receive his connection id from server")]
        [SerializeField]
        public EventReference onConnectionIdAssigned;

        // Flag is this object shall keep persistent between scenes
        [SerializeField]
        private bool dontDestroyOnLoad = true;

        // Singleton instance of NetworkGlobalEvents.
        private static NetworkGlobalEvents instance;

        /// <summary>
        /// Gets the singleton instance of NetworkGlobalEvents.
        /// If the instance doesn't exist, logs a warning during play mode.
        /// </summary>
        /// <returns>The singleton instance of NetworkGlobalEvents.</returns>
        public static NetworkGlobalEvents Instance() {
#if DEBUG
            if (!InstanceExists()) {
                if (Application.isPlaying) {
#if LOG_OBJECTNET_WARNINGS
                    NetworkDebugger.LogWarning("Could not find the instance of object. Please ensure you have added the NetworkEventsManager Prefab to your scene.");
#endif
                }
            }
#endif
            return NetworkGlobalEvents.instance;
        }

        /// <summary>
        /// Detect ig another NetworkGlobalEvents is already instantiated
        /// </summary>
        /// <returns>True if NetworkGlobalEvents already exists, otherwise false</returns>
        public bool DetectDuplicated() {
            return (NetworkGlobalEvents.instance != null);
        }

        /// <summary>
        /// Flag current instance of NetworkGlobalEvents as the in use instance
        /// </summary>
        private void SetInstance() {
            NetworkGlobalEvents.instance = this;
        }

        /// <summary>
        /// Checks if the singleton instance of NetworkGlobalEvents exists.
        /// </summary>
        /// <returns>True if the instance exists, false otherwise.</returns>
        private static bool InstanceExists() {
		    return (NetworkGlobalEvents.instance != null);
	    }

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// Initializes the singleton instance.
        /// </summary>
        private void Awake() {
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

        /// <summary>
        /// Called when server start hist connection to listen incomming clients
        /// </summary>
        public void OnServerStarted(IChannel channel) {
            this.InvokeVoidMethod(this.onServerStarted, channel);
        }

        /// <summary>
        /// Called when any player was spawned
        /// </summary>
        public void OnPlayerSpawned(GameObject player) {
            this.InvokeVoidMethod(this.onPlayerSpawned, player);
        }

        /// <summary>
        /// Called when any player start his network services
        /// </summary>
        public void OnPlayerStarted(INetworkElement networkElement) {
            this.InvokeVoidMethod(this.onPlayerStarted, networkElement);
        }

        /// <summary>
        /// Called when any client became owner of his player
        /// </summary>
        public void OnPlayerOwnerDetected(INetworkElement networkElement) {
            this.InvokeVoidMethod(this.onPlayerOwnerDetected, networkElement);
        }
        
        /// <summary>
        /// Called when any client receive id connection ID
        /// </summary>
        public void OnConnectionIdAssigned(int connectionId) {
            this.InvokeVoidMethod(this.onConnectionIdAssigned, connectionId);
        }

        /// <summary>
        /// Called when remote scene load process was finished
        /// </summary>
        public void OnSceneLoadFinished(string sceneName) {
            this.InvokeVoidMethod(this.onSceneLoadFinished, sceneName);
        }

        /// <summary>
        /// Called when remote scene load process was finished
        /// </summary>
        public void OnSceneUnLoadFinished(string sceneName) {
            this.InvokeVoidMethod(this.onSceneUnLoadFinished, sceneName);
        }

        /// <summary>
        /// Called when a client successfully connects.
        /// </summary>
        /// <param name="client">The client that has connected.</param>
        public void OnConnected(IClient client) {
            this.InvokeVoidMethod(this.onConnected, client);
        }

        /// <summary>
        /// Called when a client disconnects.
        /// </summary>
        /// <param name="client">The client that has disconnected.</param>
        public void OnDisconnected(IClient client) {
            this.InvokeVoidMethod(this.onDisconnected, client);
        }

        /// <summary>
        /// Called when a client connects on a relay server.
        /// </summary>
        /// <param name="client">The client that has connected on the relay.</param>
        public void OnConnectedOnRelay(IClient client) {
            this.InvokeVoidMethod(this.onConnectedOnRelay, client);
        }

        /// <summary>
        /// Called when the server is restarted.
        /// </summary>
        /// <param name="client">The client that was connected when the server restarted.</param>
        public void OnServerRestarted(IClient client) {
            this.InvokeVoidMethod(this.onServerRestarted, client);
        }

        /// <summary>
        /// Called when client lost his connection with server.
        /// </summary>
        /// <param name="error">The exception that occurred client lost his connection with server.</param>
        public void OnConnectionFailed(Exception error) {
            this.InvokeVoidMethod(this.onConnectionFailed, error);
        }
                
        /// <summary>
        /// Called when a login attempt fails.
        /// </summary>
        /// <param name="error">The exception that occurred during the login attempt.</param>
        public void OnLoginFailed(Exception error) {
            this.InvokeVoidMethod(this.onLoginFailed, error);
        }

        /// <summary>
        /// Called when a login attempt is successful.
        /// </summary>
        /// <param name="client">The client that has successfully logged in.</param>
        public void OnLoginSucess(IClient client) {
            this.InvokeVoidMethod(this.onLoginSucess, client);
        }

        /// <summary>
        /// Called when a lobby is successfully created.
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby that was created.</param>
        public void OnLobbyCreationSucess(ushort lobbyId) {
            this.InvokeVoidMethod(this.onLobbyCreationSucess, lobbyId);
        }

        /// <summary>
        /// Called when a lobby creation attempt fails.
        /// </summary>
        /// <param name="reason">The reason for the lobby creation failure.</param>
        public void OnLobbyCreationFailed(string reason) {
            this.InvokeVoidMethod(this.onLobbyCreationFailed, reason);
        }

        /// <summary>
        /// Called when joining a lobby is successful.
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby that was joined.</param>
        public void OnLobbyJoinSuccess(ushort lobbyId) {
            this.InvokeVoidMethod(this.onLobbyJoinedSucess, lobbyId);
        }

        /// <summary>
        /// Called when an attempt to join a lobby fails.
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby that failed to be joined.</param>
        public void OnLobbyJoinFailed(ushort lobbyId) {
            this.InvokeVoidMethod(this.onLobbyJoinFailed, lobbyId);
        }

        /// <summary>
        /// Called when list of players was received.
        /// </summary>
        /// <param name="players">Array containing all players on the lobby.</param>
        public void OnLobbyPlayersRefresh(string[] players) {
            this.InvokeVoidMethod(this.onLobbyPlayersRefresh, players);
        }
        
        /// <summary>
        /// Called when a client successfully connects to the server.
        /// </summary>
        /// <param name="client">The client that has connected.</param>
        public void OnClientConnected(IClient client) {
            this.InvokeVoidMethod(this.onClientConnected, client);
        }

        /// <summary>
        /// Called when a client disconnects from the server.
        /// </summary>
        /// <param name="client">The client that has disconnected.</param>
        public void OnClientDisconnected(IClient client) {
            this.InvokeVoidMethod(this.onClientDisconnected, client);
        }

        /// <summary>
        /// Called when a client's login attempt fails.
        /// </summary>
        /// <param name="client">The client whose login attempt failed.</param>
        public void OnClientLoginFailed(IClient client) {
            this.InvokeVoidMethod(this.onClientLoginFailed, client);
        }

        /// <summary>
        /// Called when a client's login attempt is successful.
        /// </summary>
        /// <param name="client">The client whose login attempt was successful.</param>
        public void OnClientLoginSucess(IClient client) {
            this.InvokeVoidMethod(this.onClientLoginSucess, client);
        }

        /// <summary>
        /// Called when a message is received from a client.
        /// </summary>
        /// <param name="reader">The data stream containing the message.</param>
        public void OnMessageReceived(IDataStream reader) {
            this.InvokeVoidMethod(this.onMessageReceived, reader);
        }


    }
}