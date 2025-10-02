using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines a static class for handling custom object events in a networked environment.
    ///
    /// Note: Custom user events mean any event raised by code without the use NetworkEventsManager controller
    ///       Those events are different since they can't be bounded automatically, In this case, the system will
    ///       *negative event to ensure that he will keep out of any internal system boundaries.
    ///
    ///       * This operation is controlled internally by engine and user can work seamsless
    /// </summary>
    public static class UserCustomObjectEvents {
        /// <summary>
        /// Constant representing the base event code for custom network object events.
        /// By convention, any negative event code is considered a custom network event.
        /// </summary>
        private const int CUSTOM_NETWORK_OBJECT_EVENTS = -1;

        /// <summary>
        /// Event code to take ownership of object
        /// </summary>
        public static int TakeControl           = -2;

        /// <summary>
        /// Event code to tell client that take was done
        /// </summary>
        public static int TakeControlSucess     = -3;

        /// <summary>
        /// Event code to release ownership of object
        /// </summary>
        public static int ReleaseControl        = -4;

        /// <summary>
        /// Event code to tell client that release was done
        /// </summary>
        public static int ReleaseControlSucess  = -5;

        /// <summary>
        /// Event code to transfer ownership of object to another player
        /// </summary>
        public static int TransferControl       = -6;

        /// <summary>
        /// Event code to tell client's that object ownership was already defined
        /// </summary>
        public static int OwnerShipDefined  = -7;

        /// <summary>
        /// Event code to teleport object to another position
        /// </summary>
        public static int ObjectTeleport        = -8;

        /// <summary>
        /// Constant representing a remote execution method invoke
        /// Note: Child objects will start from 33554431 to 4294967295 ( 0x01FFFFFF to 0xFFFFFFFF )
        /// </summary>
        public const int REMOTE_METHOD_EXECUTION = 0x01FFFFFF;

        /// <summary>
        /// Determines if the given event code corresponds to a custom object event.
        /// </summary>
        /// <param name="eventCode">The event code to check.</param>
        /// <returns>True if the event code is for a custom object event, false otherwise.</returns>
        public static bool IsCustomObjectEvent(int eventCode) {
            // Returns true if the event code is less than or equal to the custom network object events code.
            return ((eventCode <= CUSTOM_NETWORK_OBJECT_EVENTS) || (eventCode == REMOTE_METHOD_EXECUTION));
        }

        /// <summary>
        /// Determines if the given event code corresponds to a remote method execution.
        /// </summary>
        /// <param name="eventCode">The event code to check.</param>
        /// <returns>True if the event code is a remote method execution, false otherwise.</returns>
        public static bool IsRemoteMethod(int eventCode) {
            const uint REMOTE_METHOD_START_BOUNDS    = 0x01FFFFFF;
            const uint REMOTE_METHOD_END_BOUNDS      = 0xFFFFFFFF;
            return (eventCode >= REMOTE_METHOD_START_BOUNDS) && ((uint)eventCode < REMOTE_METHOD_END_BOUNDS);
        }

        /// <summary>
        /// Converts a given event code to a custom object event code.
        /// </summary>
        /// <param name="eventCode">The event code to convert.</param>
        /// <returns>An integer representing the custom object event code.</returns>
        public static int ToObjectEvent(int eventCode) {
            // Multiplies the absolute value of the event code by the custom network object events code
            // to generate a unique event code for custom object events.
            return (Mathf.Abs(eventCode) * CUSTOM_NETWORK_OBJECT_EVENTS);
        }
    }

}