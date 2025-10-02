namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the binding strategy for a transport server.
    /// </summary>
    public enum TransportServerBind {

        /// <summary>
        /// Indicates that the server should bind to any available address.
        /// </summary>
        UseAnyAddress,

        /// <summary>
        /// Indicates that the server should bind to a specific, fixed address.
        /// </summary>
        UseFixedAddress

    }
}