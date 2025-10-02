namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the synchronization types for animations.
    /// </summary>
    /// <remarks>
    /// This enumeration is used to determine how an animation is synchronized.
    /// - UseController: The animation is synchronized based on the animation controller.
    /// - UseParameters: The animation is synchronized based on specific parameters.
    /// - ManualControl: The animation is manually controlled and synchronized by the user.
    /// </remarks>
    public enum AnimationSyncType {
        /// <summary>
        /// Synchronize the animation using the animation controller.
        /// </summary>
        UseController,

        /// <summary>
        /// Synchronize the animation based on specific parameters (e.g., speed, direction).
        /// </summary>
        UseParameters,

        /// <summary>
        /// The animation is manually controlled by the user, allowing for custom synchronization.
        /// </summary>
        ManualControl
    }

}