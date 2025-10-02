namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the types of prediction mechanisms for a process or operation.
    /// </summary>
    public enum PredictionType {
        /// <summary>
        /// Indicates that the prediction mechanism should be determined automatically.
        /// The system will select the most appropriate prediction type based on the context.
        /// </summary>
        Automatic,

        /// <summary>
        /// Indicates that the prediction should be based on a transform, such as a mathematical
        /// or geometrical transformation. This is typically used for non-physical or abstract
        /// prediction scenarios.
        /// </summary>
        UseTransform,

        /// <summary>
        /// Indicates that the prediction should be based on physics. This is suitable for
        /// scenarios where physical properties and behaviors (like velocity, acceleration, etc.)
        /// need to be taken into account for the prediction.
        /// </summary>
        UsePhysics
    }

}