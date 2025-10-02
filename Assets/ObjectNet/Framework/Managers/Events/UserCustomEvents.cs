namespace com.onlineobject.objectnet {
    /// <summary>
    /// Static class containing definitions and methods related to user custom events.
    /// 
    /// This class itself doesn't contains any event,it's only has the boundaries of user events
    /// </summary>
    public static class UserCustomEvents {
        /// <summary>
        /// The starting boundary for user custom event codes.
        /// </summary>
        public static int StartBound            = 50000;

        /// <summary>
        /// The ending boundary for user custom event codes.
        /// </summary>
        public static int EndBound              = 59999;

        /// <summary>
        /// Determines if the given event code is within the range of user custom events.
        /// </summary>
        /// <param name="eventCode">The event code to check.</param>
        /// <returns>true if the event code is a user custom event; otherwise, false.</returns>
        public static bool IsUserCustomEvent(int eventCode) {
            // Check if the event code is within the start and end bounds for custom events
            return ((eventCode >= UserCustomEvents.StartBound) && (eventCode <= UserCustomEvents.EndBound));
        }
    }

}