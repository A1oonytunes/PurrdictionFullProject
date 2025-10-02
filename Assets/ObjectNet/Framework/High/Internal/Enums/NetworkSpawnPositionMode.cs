namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the mode for spawning networked objects in the game world.
    /// </summary>
    public enum NetworkSpawnPositionMode {
        /// <summary>
        /// Indicates that networked objects should spawn at a fixed position.
        /// </summary>
        Fixed,

        /// <summary>
        /// Indicates that networked objects can spawn at multiple predefined positions.
        /// </summary>
        Multiple,

        /// <summary>
        /// Indicates that the spawn position for networked objects is determined dynamically at runtime.
        /// </summary>
        Dynamic
    }

}