using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Static class containing definitions and configurations for transport mechanisms.
    /// </summary>
    public static class TransportDefinitions {

        /// <summary>
        /// The maximun number of bytes on package
        /// </summary>
        public static int DatagramBufferSize = DEFAULT_BUFER_SIZE;

        /// <summary>
        /// The maximun number of bytes on package ( for dissonance )
        /// </summary>
        public static int DatagramDissonanceBufferSize = DEFAULT_DISSONANCE_BUFER_SIZE; 

        /// <summary>
        // Define the default address family for the transport layer.
        // Currently set to IPv4.
        /// <summary>
        public static TransportAddressFamily AddressFamily = TransportAddressFamily.Ipv4;

        /// <summary>
        // Define the default server binding type for the transport layer.
        // Currently set to use any available address for binding.
        /// <summary>
        public static TransportServerBind ServerBindingType = TransportServerBind.UseAnyAddress;

        /// <summary>
        /// The size of the buffer for unreliable messages. When the buffer size is reached,
        /// the oldest messages will be discarded.
        /// </summary>
        /// <remarks>
        /// TODO: Implement an automatic system to adjust the buffer size based on the number of objects in the scene.
        /// </remarks>
        public static int UnreliabeBufferSize = 10000000; // Initial buffer size for unreliable messages.

        /// <summary>
        /// The maximun of clients accepted by server
        /// </summary>
        public static int MaximunOfClients = -1; // Neqative means that has no limits

        /// <summary>
        /// The maximun number of attempts that a reliable package is sent before disconnect is detected
        /// </summary>
        public static int ReliableSendAttempts = DEFAULT_RETRY_ATTEMPTS;

        /// <summary>
        /// The maximun number of RESILIENT attempts that a reliable package is sent before disconnect is detected
        /// </summary>
        public static int AvgSendAttemptsResilience = DEFAULT_RESILIENCE_ATTEMPTS;

        /// <summary>
        /// Store if disconnectiuon was paused or resumed
        /// </summary>
        private static bool disconnectionWasPausedOrResumed = false;

        /// <summary>
        /// Store if disconnectiuon was paused
        /// </summary>
        private static bool disconnectionWasPaused = false;


        // Minimum queue size per object in the scene.
        private const int MIN_QUEUE_SIZE_PEER_OBJECT = 5;

        // Maximum queue size per object, calculated as a multiple of MIN_QUEUE_SIZE_PEER_OBJECT.
        private const int MAX_QUEUE_SIZE_PEER_OBJECT = (1000 * MIN_QUEUE_SIZE_PEER_OBJECT);

        // Default number of attempts that a reliable package is sent before disconnect is detected
        private const int DEFAULT_RETRY_ATTEMPTS = 30;

        // Default number of RESILIENCE attempts that a reliable package is sent before disconnect is detected
        private const int DEFAULT_RESILIENCE_ATTEMPTS = 64;

        // Default max buffer size used to send and receive data
        private const int DEFAULT_BUFER_SIZE = 1024;

        // Default max buffer size used to send and receive data ( for dissonance packages )
        private const int DEFAULT_DISSONANCE_BUFER_SIZE = 1024;

        /// <summary>
        /// This method the the values used to detect disconnection to infinity
        /// 
        /// WARNING: This method shall be avoided as much as possible since they disable the 
        ///          realiable messages not ensuring that those messages arrives on his destinations
        ///          we strongly don't recommend the use of this techinique. 
        ///          Use with you own responsability
        /// </summary>
        public static void DisableDisconnectDetections() {
            // Update global variables to not cause disconnection
            TransportDefinitions.disconnectionWasPausedOrResumed    = true;
            TransportDefinitions.disconnectionWasPaused             = true;


        }

        /// <summary>
        /// This method the the values used to detect disconnection to infinity
        /// 
        /// WARNING: This method shall be avoided as much as possible since they disable the 
        ///          realiable messages not ensuring that those messages arrives on his destinations
        ///          we strongly don't recommend the use of this technique. 
        ///          Use with you own responsability
        /// </summary>
        public static void ResumeDisconnectDetections() {
            // Update global variables to original values
            TransportDefinitions.disconnectionWasPausedOrResumed    = true;      
            TransportDefinitions.disconnectionWasPaused             = false;

        }

        /// <summary>
        /// Return if has changes on disconnection values
        /// </summary>
        /// <param name="reset">Reset value after return</param>
        /// <returns>True is has changes, otherwise false</returns>
        public static bool WasDisconnectionValuesChanged(bool reset = true) {
            bool currentValue = TransportDefinitions.disconnectionWasPausedOrResumed;
            if (reset) TransportDefinitions.disconnectionWasPausedOrResumed = false;
            return currentValue;
        }

        /// <summary>
        /// Return if disconnection is paused
        /// </summary>
        /// <returns>True if is paused, otherwise false</returns>
        public static bool IsDisconnectionValuesChanged() {
            return TransportDefinitions.disconnectionWasPaused;
        }
        
        /// <summary>
        /// Adjusts the buffer size for unreliable messages based on the number of objects in the scene.
        /// </summary>
        /// <param name="objectsCounter">The number of objects currently in the scene.</param>
        public static void AdjustBufferSize(int objectsCounter) {
            // Clamp the buffer size to ensure it's within the allowed range.
            // Note: Currently, this calculation does not change the buffer size as it clamps
            // the value to MAX_QUEUE_SIZE_PEER_OBJECT regardless of the objectsCounter.
            // This might be an area to review for correct functionality.
            TransportDefinitions.UnreliabeBufferSize = Math.Clamp(objectsCounter * MAX_QUEUE_SIZE_PEER_OBJECT, MAX_QUEUE_SIZE_PEER_OBJECT, MAX_QUEUE_SIZE_PEER_OBJECT);
        }
    }

}