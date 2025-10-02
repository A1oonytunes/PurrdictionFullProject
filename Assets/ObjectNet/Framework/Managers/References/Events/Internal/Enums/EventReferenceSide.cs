namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the sides where an event can be referenced or triggered.
    /// </summary>
    public enum EventReferenceSide {
        /// <summary>
        /// Indicates that the event is referenced or triggered only on the server side.
        /// </summary>
        ServerSide,

        /// <summary>
        /// Indicates that the event is referenced or triggered only on the client side.
        /// </summary>
        ClientSide,

        /// <summary>
        /// Indicates that the event is referenced or triggered on both the server and client sides.
        /// </summary>
        BothSides
    }

}