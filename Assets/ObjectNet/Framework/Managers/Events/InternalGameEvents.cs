namespace com.onlineobject.objectnet {
    /// <summary>
    /// Static class containing constants for internal game event codes and a method to check if a code is a game event.
    /// </summary>
    public static class InternalGameEvents {
        /// <summary>
        /// Special event code used to redirect event to NetworkObject directly.
        /// </summary>
        public static int ObjectEvent           = 5100;

        /// <summary>
        /// Event code for playing animations.
        /// </summary>
        public static int AnimationPlay         = 5102;

        /// <summary>
        /// Event code for playing audio.
        /// </summary>
        public static int AudioPlay             = 5103;

        /// <summary>
        /// Event code for fade fade animations.
        /// </summary>
        public static int AnimationFade         = 5104;

        /// <summary>
        /// Event code for reliable variables update.
        /// </summary>
        public static int ReliableVariable      = 5105;

        /// <summary>
        /// Dummy event code used only to flag the final bound of event codes.
        /// This should not be used for actual events.
        /// </summary>
        public static int DummyEvent            = 5199;

        /// <summary>
        /// Determines if the provided event code corresponds to a defined game event.
        /// </summary>
        /// <param name="eventCode">The event code to check.</param>
        /// <returns>true if the event code is within the range of defined game events; otherwise, false.</returns>
        public static bool IsGameEvent(int eventCode) {
            return ((eventCode >= InternalGameEvents.ObjectEvent) && (eventCode <= InternalGameEvents.DummyEvent));
        }
    }

}