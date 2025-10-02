namespace com.onlineobject.objectnet {
    /// <summary>
    /// Static class containing constants and methods related to lobby server events.
    /// </summary>
    public static class LobbyServerEvents {
        /// <summary>
        /// Request to create a lobby on server
        /// </summary>
        public static int LobbyCreateRequest        = 5300;


        /// <summary>
        /// Lobby successfully created on server
        /// </summary>
        public static int LobbyCreatedSucess        = 5301;


        /// <summary>
        /// Lobby creation failed on server
        /// </summary>
        public static int LobbyCreatedFailed        = 5302;


        /// <summary>
        /// Lobby finished or closed
        /// </summary>
        public static int LobbyFinish               = 5303;


        /// <summary>
        /// Request to join a lobby
        /// </summary>
        public static int LobbyJoinRequest          = 5304;


        /// <summary>
        /// Successfully joined a lobby
        /// </summary>
        public static int LobbyJoinSucess           = 5305;


        /// <summary>
        /// Failed to join a lobby
        /// </summary>
        public static int LobbyJoinFailed           = 5306;


        /// <summary>
        /// Request for the list of players in a lobby
        /// </summary>
        public static int LobbyPlayersListRequest   = 5307;


        /// <summary>
        /// Refresh the list of players in a lobby
        /// </summary>
        public static int LobbyPlayersListRefresh   = 5308;


        /// <summary>
        /// Request for the list of available lobbies
        /// </summary>
        public static int LobbyListRequest          = 5309;


        /// <summary>
        /// Refresh the list of available lobbies
        /// </summary>
        public static int LobbyListRefresh          = 5310; 

        /// <summary>
        // This event code is used as a marker for the upper bound of lobby event codes.
        /// </summary>
        public static int DummyEvent                = 5399;
    
        /// <summary>
        /// Determines if the given event code corresponds to a lobby event.
        /// </summary>
        /// <param name="eventCode">The event code to check.</param>
        /// <returns>true if the event code is within the range of lobby events; otherwise, false.</returns>
        public static bool IsLobbyEvent(int eventCode) {
            // Check if the event code is within the range of defined lobby event codes.
            return ((eventCode >= LobbyServerEvents.LobbyCreateRequest) && (eventCode <= LobbyServerEvents.DummyEvent));
        }
    }

}