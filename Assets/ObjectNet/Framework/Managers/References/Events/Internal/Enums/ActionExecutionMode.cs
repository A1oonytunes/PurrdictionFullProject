namespace com.onlineobject.objectnet {
    /// <summary>
    /// Specifies the mode of execution for an action based on certain conditions.
    /// </summary>
    public enum ActionExecutionMode {
        /// <summary>
        /// Executes the action when the condition becomes true.
        /// </summary>
        OnBecameTrue,
        /// <summary>
        /// Executes the action when the condition becomes false.
        /// </summary>
        OnBecameFalse,
        /// <summary>
        /// Executes the action when the condition transitions from true to false or vice versa.
        /// </summary>
        OnTransition,
        /// <summary>
        /// Executes the action continuously when condition is true
        /// </summary>
        Continuous
    }
}
