namespace com.onlineobject.objectnet {
    /// <summary>
    /// Static class containing core game event codes and utility functions.
    /// </summary>
    public static class CoreGameEvents {
        /// <summary>
        /// Event code for when an object is spawned on the server and needs to be instantiated on the client.
        /// </summary>
        public static int ObjectInstantiate         = 5000;
        /// <summary>
        /// Event code for when an object is destroyed on the server and needs to be removed on the client.
        /// </summary>
        public static int ObjectDestroy             = 5001;
        /// <summary>
        /// Event code indicating that an object has been instantiated on the client and the server should be notified.
        /// </summary>
        public static int ObjectCreatedOnClient     = 5002;
        /// <summary>
        /// Event code for a basic update, typically used when a NetworkObject is updated.
        /// </summary>
        public static int ObjectUpdate              = 5003;
        /// <summary>
        /// Event code for player input updates when input handling is enabled.
        /// </summary>
        public static int ObjectInputUpdate         = 5004;
        /// <summary>
        /// Event code used to identify a player instance for targeted operations.
        /// </summary>
        public static int PlayerIdentify            = 5005;
        /// <summary>
        /// Event code sent as a response when a client has received its identification from the server.
        /// </summary>
        public static int PlayerIdentified          = 5006;
        /// <summary>
        /// Event code to inform the client that the player has been spawned on the server.
        /// </summary>
        public static int PlayerSpawnedOnServer     = 5007;
        /// <summary>
        /// Event code for requesting the server to send the client's ID.
        /// </summary>
        public static int PlayerRequestClientId     = 5008;
        /// <summary>
        /// Event code indicating that the player is ready on either the server or client.
        /// </summary>
        public static int PlayerReadyOnClient       = 5009;
        /// <summary>
        /// Event code for synchronizing the game tick and clock from the server to the players.
        /// </summary>
        public static int SynchronizeTick           = 5010;
        /// <summary>
        /// Event code for spawning an object over the network, typically from players to the server.
        /// </summary>
        public static int NetworkSpawn              = 5011;
        /// <summary>
        /// Event code to tell to client that object was already spawned on server.
        /// </summary>
        public static int NetworkSpawnResponse      = 5012;
        /// <summary>
        /// Event code for despawning an object over the network, typically from players to the server.
        /// </summary>
        public static int NetworkDestroy            = 5013;
                /// <summary>
        /// Event code to request server to send current static objects spawned
        /// </summary>
        public static int RequestStaticSpawnUpdate  = 5014;
        /// <summary>
        /// Event code to tell to all new client the number of current static objects spawned
        /// This is used remove object already removed from scene by the time when client connect
        /// </summary>
        public static int NetworkStaticSpawnUpdate  = 5015;
        /// <summary>
        /// Event code to tell to server to respawn a player object
        /// This event should be trigger only after player element has been destroyed and you need to respawn player object
        /// </summary>
        public static int PlayerRespawOnClient      = 5016;
        /// <summary>
        /// Event send from server to all clients to order to load some scene
        /// </summary>
        public static int RemoteLoadScene           = 5017;
        /// <summary>
        /// Event code to tell to server that some client was already loaded the scene
        /// </summary>
        public static int ClientSceneLoaded         = 5018;
        /// <summary>
        /// Event code to tell to server that all client's loaded the scene
        /// </summary>
        public static int RemoteSceneLoadFinished   = 5019;
        /// <summary>
        /// Event code to tell to client that his load operation was failed and server order hin tro disconnect ( or close the game )
        /// </summary>
        public static int RemoteSceneLoadFail       = 5020;
        /// <summary>
        /// Event code send from client to request server to start the remote scene to be loaded
        /// </summary>
        public static int RequestRemoteSceneLoad    = 5021;
        /// <summary>
        /// Event code send from server to client to enable/disable disconnection by timeout
        /// </summary>
        public static int DisconnectTimeoutChange   = 5022;
        /// <summary>
        /// Event code send from client to server to sopawn his player when spawn player is configured to manually
        /// </summary>
        public static int RequestPlayerSpawn        = 5023;
        /// <summary>
        /// Event send from server to all clients to order to load some scene
        /// </summary>
        public static int RemoteUnLoadScene         = 5024;
        /// <summary>
        /// Event code to tell to server that some client was already loaded the scene
        /// </summary>
        public static int ClientSceneUnLoaded       = 5025;
        /// <summary>
        /// Event code to tell to server that some client was already loaded the scene
        /// </summary>
        public static int RemoteSceneUnLoadFinished = 5026;
        /// <summary>
        /// Event code to tell to client that his load operation was failed and server order hin tro disconnect ( or close the game )
        /// </summary>
        public static int RemoteSceneUnLoadFail     = 5027;
        /// <summary>
        /// Event code send from client to request server to start the remote scene to be loaded
        /// </summary>
        public static int RequestRemoteSceneUnLoad  = 5028;
        /// <summary>
        /// Event code send from client to request server to spawn all objects in scene to me
        /// </summary>
        public static int RequestSceneObjects       = 5029;
        /// <summary>
        /// Event code indicating that the player is ready on either the server or client.
        /// </summary>
        public static int RequestPlayerRespawn      = 5030;
        /// <summary>
        /// Event code indicating that the player wish to receive an update of reliable variables.
        /// </summary>
        public static int RequestVariablesUpdate    = 5031;

        /// <summary>
        /// Event code used as a marker for the upper limit of core game event codes.
        /// </summary>
        public static int DummyEvent                = 5099;

        /// <summary>
        /// Determines if the given event code corresponds to a core game event.
        /// </summary>
        /// <param name="eventCode">The event code to check.</param>
        /// <returns>true if the event code is a core game event; otherwise, false.</returns>
        public static bool IsCoreEvent(int eventCode) {
            // Check if the event code is within the range of core game events.
            return ((eventCode >= CoreGameEvents.ObjectInstantiate) && (eventCode <= CoreGameEvents.DummyEvent));
        }
    }

}