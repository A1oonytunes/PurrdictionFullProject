namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the types of network connections that can be established.
    /// </summary>
    public enum NetworkConnectionType {
        /// <summary>
        /// Represents a server connection type where the current instance acts as a server.
        /// </summary>
        Server,

        /// <summary>
        /// Represents a client connection type where the current instance acts as a client.
        /// </summary>
        Client,

        /// <summary>
        /// Represents a manual connection type where the connection parameters are set manually.
        /// </summary>
        Manual
    }

}