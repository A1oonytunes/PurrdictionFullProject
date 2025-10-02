namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the conditions under which a trigger should activate.
    /// </summary>
    public enum TriggerCondition {
        /// <summary>
        /// Indicates that the trigger should activate when all conditions are met.
        /// </summary>
        And,

        /// <summary>
        /// Indicates that the trigger should activate when any one of the conditions is met.
        /// </summary>
        Or
    }

}
