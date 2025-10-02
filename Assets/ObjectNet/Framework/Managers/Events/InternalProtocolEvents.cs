namespace com.onlineobject.objectnet {
    /// <summary>
    /// Static class containing constants for internal protocol event codes and a method to check if an event code is internal.
    /// </summary>
    public static class InternalProtocolEvents {
        /// <summary>
        // Event code indicating a client has successfully connected.
        /// </summary>
        public static int ClientConnected       = 5200;

        /// <summary>
        // Event code used to measure round-trip time for a message between client and server.
        /// </summary>
        public static int UdpHostPing           = 5201;

        /// <summary>
        // Event code indicating a server-side error has occurred.
        /// </summary>
        public static int ServerError           = 5202;

        /// <summary>
        // Event code indicating an error occurred during the login process.
        /// </summary>
        public static int LoginError            = 5203;

        /// <summary>
        // Event code indicating a successful login.
        /// </summary>
        public static int LoginSucess           = 5204;

        /// <summary>
        // Event code used as a placeholder to indicate the upper bound of internal event codes.
        /// </summary>
        public static int DummyEvent            = 5299;

        /// <summary>
        /// Determines if the provided event code corresponds to an internal event.
        /// </summary>
        /// <param name="eventCode">The event code to check.</param>
        /// <returns>true if the event code is for an internal event; otherwise, false.</returns>
        public static bool IsInternalEvent(int eventCode) {
            // Check if the event code is within the range of defined internal event codes.
            return ((eventCode >= InternalProtocolEvents.ClientConnected) && (eventCode <= InternalProtocolEvents.DummyEvent));
        }
    }

}