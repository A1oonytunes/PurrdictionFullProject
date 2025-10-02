using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Static class containing constants for network behavior method names.
    /// </summary>
    public static class NetworkBehaviorConstants {
        // Decoded string for the active "Awake" method name.
        public static readonly string ActiveAwakeMethod         = NetworkBehaviorConstants.Decode("4163746976654177616b65");
        // Decoded string for the passive "Awake" method name.
        public static readonly string PassiveAwakeMethod        = NetworkBehaviorConstants.Decode("506173736976654177616b65");
        // Decoded string for the active "OnEnable" method name.
        public static readonly string ActiveOnEnableMethod      = NetworkBehaviorConstants.Decode("4163746976654f6e456e61626c65");
        // Decoded string for the passive "OnEnable" method name.
        public static readonly string PassiveOnEnableMethod     = NetworkBehaviorConstants.Decode("506173736976654f6e456e61626c65");
        // Decoded string for the active "OnDisable" method name.
        public static readonly string ActiveOnDisableMethod     = NetworkBehaviorConstants.Decode("4163746976654f6e44697361626c65");
        // Decoded string for the passive "OnDisable" method name.
        public static readonly string PassiveOnDisableMethod    = NetworkBehaviorConstants.Decode("506173736976654f6e44697361626c65");
        // Decoded string for the active "Start" method name.
        public static readonly string ActiveStartMethod         = NetworkBehaviorConstants.Decode("4163746976655374617274");
        // Decoded string for the passive "Start" method name.
        public static readonly string PassiveStartMethod        = NetworkBehaviorConstants.Decode("506173736976655374617274");
        // Decoded string for the active "Update" method name.
        public static readonly string ActiveUpdateMethod        = NetworkBehaviorConstants.Decode("416374697665557064617465");
        // Decoded string for the passive "Update" method name.
        public static readonly string PassiveUpdateMethod       = NetworkBehaviorConstants.Decode("50617373697665557064617465");
        // Decoded string for the active "FixedUpdate" method name.
        public static readonly string ActiveFixedUpdateMethod   = NetworkBehaviorConstants.Decode("4163746976654669786564557064617465");
        // Decoded string for the passive "FixedUpdate" method name.
        public static readonly string PassiveFixedUpdateMethod  = NetworkBehaviorConstants.Decode("506173736976654669786564557064617465");
        // Decoded string for the active "LateUpdate" method name.
        public static readonly string ActiveLateUpdateMethod    = NetworkBehaviorConstants.Decode("4163746976654c617465557064617465");
        // Decoded string for the passive "LateUpdate" method name.
        public static readonly string PassiveLateUpdateMethod   = NetworkBehaviorConstants.Decode("506173736976654c617465557064617465");

        /// <summary>
        /// Decodes a hexadecimal string to its ASCII string representation.
        /// </summary>
        /// <param name="hex">The hexadecimal string to decode.</param>
        /// <returns>The ASCII string representation of the hexadecimal input.</returns>
        private static string Decode(string hex) {
            // Remove any dashes from the hex string.
            hex = hex.Replace("-", "");
            // Initialize a byte array to hold the raw bytes of the hex string.
            byte[] raw = new byte[hex.Length / 2];
            // Convert each pair of characters (byte) in the hex string to its byte representation.
            for (int i = 0; i < raw.Length; i++) {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            // Return the ASCII encoded string of the byte array.
            return System.Text.Encoding.ASCII.GetString(raw);
        }
    }

}