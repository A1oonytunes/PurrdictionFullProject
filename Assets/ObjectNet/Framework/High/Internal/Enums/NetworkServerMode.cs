namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the modes in which a network server can operate.
    /// </summary>
    public enum NetworkServerMode {
        /// <summary>
        /// Relay mode where the server acts as a relay point between clients.
        /// </summary>
        Relay,

        /// <summary>
        /// Embedded mode where the server is embedded within the client application.
        /// </summary>
        Embedded,

        /// <summary>
        /// Authoritative mode where the server has the authority over game logic and state.
        /// </summary>
        Authoritative,

        /// <summary>
        /// ClientOnly mode where the application acts solely as a client without any server capabilities.
        /// </summary>
        ClientOnly
    }

}