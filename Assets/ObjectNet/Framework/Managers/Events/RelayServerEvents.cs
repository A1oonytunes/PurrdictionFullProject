namespace com.onlineobject.objectnet {
    /// <summary>
    /// Static class containing constants for relay server event codes and a method to check if a code is a relay event.
    /// </summary>
    public static class RelayServerEvents {
        /// <summary>
        // Event code for when a client is connected to the relay server.
        /// </summary>
        public static int ClientConnected = 5400;

        /// <summary>
        // Event code to update which player is the master player of the match.
        /// </summary>
        public static int UpdateMasterPlayer        = 5401;

        /// <summary>
        // Event code to notify the master player that a client has disconnected.
        /// </summary>
        public static int DisconnectedFromServer    = 5402;

        /// <summary>
        // Event code to update the network object ID in the relay structure.
        /// <summary>
        public static int UpdateNetworkObjectId     = 5403;

        /// <summary>
        // Event code to force a client to disconnect.
        /// </summary>
        public static int ForceDisconnectClient     = 5404;

        /// <summary>
        // Event code to tells to client to create a NetworkPlayer instance
        /// </summary>
        public static int CreateNetworkPeer         = 5405;

        /// <summary>
        // Event code to tells to client to destory network player
        /// </summary>
        public static int DestroyNetworkPeer        = 5406;

        /// <summary>
        // Event code to tells to the client to initialize peer to peer network
        /// </summary>
        public static int InitializePeerToPeer      = 5407;

        /// <summary>
        // Event code to tells if network player is visible or not
        /// </summary>
        public static int PlayerPeerAvaiable        = 5408;

        /// <summary>
        // Event code to tells connection status of peer
        /// </summary>
        public static int PeerConnectionStatus      = 5409;

        /// <summary>
        /// Event send to servee when is runnign in relay mode and owner disconnect
        /// </summary>
        public static int OwnerDisconnected         = 5410;

        /// <summary>
        // Dummy event code used as an upper bound for relay event codes.
        /// </summary>
        public static int DummyEvent                = 5499;

        /// <summary>
        /// Checks if the provided event code is within the range of defined relay event codes.
        /// </summary>
        /// <param name="eventCode">The event code to check.</param>
        /// <returns>True if the event code is a relay event, false otherwise.</returns>
        public static bool IsRelayEvent(int eventCode) {
            return ((eventCode >= RelayServerEvents.ClientConnected) && (eventCode <= RelayServerEvents.DummyEvent));
        }
    }

}