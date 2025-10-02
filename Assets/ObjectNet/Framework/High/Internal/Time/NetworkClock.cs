using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a network clock that provides time-related information in a networked environment.
    /// </summary>
    public class NetworkClock : NetworkClockDefault {

        public sealed class TimeConstants {

            /// <summary>
            /// The minimun amount to sending update data
            /// Note : This is an integrity pooling to avoid any type of long lost messages and object will keep out of date
            /// </summary>
            public float UpdateRefreshRate = 0.5f;
        };

        /// <summary>
        /// 
        /// </summary>
        public static TimeConstants Constants = new TimeConstants();

        // Holds the singleton instance of the network clock.
        private static INetworkClock instance;

        /// <summary>
        /// Initializes the global network clock instance.
        /// </summary>
        /// <param name="clock">The INetworkClock instance to set as the global network clock.</param>
        public static void InitializeGlobal(INetworkClock clock) {
            // Ensure that only one instance of the network clock is initialized.
            if (NetworkClock.instance == null) {
                NetworkClock.instance = clock;
            } else {
                // Warn the user if there is an attempt to initialize multiple instances.
                NetworkDebugger.LogWarning("Multiple instances on network clocks is not allowed");
            }
        }

        /// <summary>
        /// Gets the time elapsed since the last frame update.
        /// </summary>
        public static float deltaTime { get { return NetworkClock.instance.DeltaTime; } }

        /// <summary>
        /// Gets the time elapsed since the last fixed frame update.
        /// </summary>
        public static float fixedDeltaTime { get { return NetworkClock.instance.FixedDeltaTime; } }

        /// <summary>
        /// Gets the total time since the start of the network clock.
        /// </summary>
        public static float time { get { return NetworkClock.instance.Time; } }

        /// <summary>
        /// Gets the number of frames that have passed since the last update.
        /// </summary>
        public static int deltaFrames { get { return NetworkClock.instance.DeltaFrames; } }

        /// <summary>
        /// Gets the current tick of the network clock.
        /// </summary>
        public static int tick { get { return NetworkClock.instance.Tick; } }

        /// <summary>
        /// Executes an action method for each frame that has passed since the last update.
        /// </summary>
        /// <param name="actionMethod">The action to execute for each passed frame.</param>
        public static void ProcessOnFrames(Action actionMethod) {
            // Determine the number of frames to process.
            int executionFrames = NetworkClock.deltaFrames;
            // Execute the action for each frame.
            while (executionFrames-- > 0) {
                actionMethod.Invoke();
            }
        }
    }

}