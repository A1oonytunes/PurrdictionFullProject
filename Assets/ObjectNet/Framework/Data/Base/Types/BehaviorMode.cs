namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the possible behavior modes for an entity.
    /// </summary>
    public enum BehaviorMode {
        /// <summary>
        /// Represents a mode where the entity is actively engaging or initiating actions.
        /// </summary>
        Active,

        /// <summary>
        /// Represents a mode where the entity is not initiating actions but may respond to external events.
        /// </summary>
        Passive,

        /// <summary>
        /// Represents a mode where the entity can both initiate actions and respond to external events.
        /// </summary>
        Both,

        /// <summary>
        /// Represents a unknow behaviour mode.
        /// </summary>
        Unknow
    }

}