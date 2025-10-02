namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the modes for spawning network players in the game.
    /// </summary>
    public enum NetworkPlayerSpawnMode {
        /// <summary>
        /// Indicates that a single static spawn point is used for player spawning.
        /// </summary>
        SingleElement,

        /// <summary>
        /// Indicates that spawn points can be dynamically chosen or generated at runtime.
        /// </summary>
        DynamicElement
    }

}