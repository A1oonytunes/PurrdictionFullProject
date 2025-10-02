namespace com.onlineobject.objectnet {

    /// <summary>
    /// Define Network behaviour configurations
    /// </summary>
    public enum NetworkStartOrder  {
        
        /// <summary>
        /// Occurs when network connection was stablished
        /// </summary>
        OnConnectionStablished,

        /// <summary>
        /// Occurs when player ( active/passive ) mode was assigned
        /// </summary>
        OnModeAssigned

    }

}