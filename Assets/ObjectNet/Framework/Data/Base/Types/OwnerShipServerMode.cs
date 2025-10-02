namespace com.onlineobject.objectnet {
    /// <summary>
    /// Enumeration representing how server will handle with ownership mode
    /// </summary>
    public enum OwnerShipServerMode {
        /// <summary>
        /// Allow to take or send object control from/to another player
        /// </summary>
        Allowed,

        /// <summary>
        /// Disable any type of ownership grant or transfer
        /// </summary>
        Disabled,

        /// <summary>
        /// Use access level defined by prefab
        /// </summary>
        PrefabDefinition
    }

}