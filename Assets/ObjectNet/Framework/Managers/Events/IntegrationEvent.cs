namespace com.onlineobject.objectnet {
    /// <summary>
    /// Static class containing definitions and methods related to 3 party tools integration like ( Steam, Dissonance, etc... ).
    /// 
    /// </summary>
    public class IntegrationEvent {
        /// <summary>
        /// Event used to Dissonance packets from server to client
        /// Dissonance on Asset Store : https://assetstore.unity.com/packages/tools/audio/dissonance-voice-chat-70078
        /// </summary>
        public static int VoiceToClient             = 40000;

        /// <summary>
        /// Event used to Dissonance packets from client to server
        /// Dissonance on Asset Store : https://assetstore.unity.com/packages/tools/audio/dissonance-voice-chat-70078
        /// </summary>
        public static int VoiceToServer             = 40001;

        /// <summary>
        /// Event used to Steam player index update to support host migration
        /// </summary>
        public static int UpdatePlayerIndexFactory  = 40002;

        /// <summary>
        /// Event used to request player id from client on Steam to support host migration
        /// </summary>
        public static int NotifySteamUserOnline     = 40003;

        /// <summary>
        /// Event used to tell to server that player is online
        /// </summary>
        public static int NotifySteamUserAlive      = 40004;

        /// <summary>
        /// Event used to send player data to all connected players
        /// </summary>
        public static int UpdatePlayerInfoData      = 40005;

        /// <summary>
        /// Request server status to check if server is online ( to be used in case of host migration )
        /// </summary>
        public static int RequestSteamServerStatus  = 40006;

        /// <summary>
        /// Notify clients that steam server is online ( to be used in case of host migration )
        /// </summary>
        public static int SteamServerOnline         = 40007;

        /// <summary>
        /// The ending boundary for user custom event codes.
        /// </summary>
        public static int EndBound                  = 49999;

        /// <summary>
        /// Determines if the given event code is within the range of user custom events.
        /// </summary>
        /// <param name="eventCode">The event code to check.</param>
        /// <returns>true if the event code is a user custom event; otherwise, false.</returns>
        public static bool IsIntegrationEvent(int eventCode) {
            // Check if the event code is within the start and end bounds for custom events
            return ((eventCode >= IntegrationEvent.VoiceToClient) && (eventCode <= IntegrationEvent.EndBound));
        }
    }
}