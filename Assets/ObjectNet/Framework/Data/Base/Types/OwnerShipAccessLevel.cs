namespace com.onlineobject.objectnet {
    /// <summary>
    /// Enumeration representing different owner ship access level.
    /// </summary>
    public enum OwnerShipAccessLevel {
        /// <summary>
        /// Allow to take or send object control from/to another player
        /// </summary>
        Full,

        /// <summary>
        /// Allow to take object control from another player or from the server
        /// </summary>
        TakeObject,

        /// <summary>
        /// Allow to transfer control to another player when a player is the owner of this object
        /// </summary>
        TransferObject,

        /// <summary>
        /// Ownership will be always of the client that create this object
        /// </summary>
        ClientOnly,

        /// <summary>
        /// Block any type of ownership change, only the server will be the owner of this object
        /// </summary>
        ServerOnly
    }

}