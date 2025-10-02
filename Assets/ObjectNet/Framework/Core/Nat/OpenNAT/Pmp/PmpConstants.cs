namespace com.onlineobject.objectnet {
    /// <summary>
    /// Static class containing constants for the Port Mapping Protocol (PMP).
    /// </summary>
    internal static class PmpConstants {
        /// <summary>
        /// The version of the PMP protocol being used.
        /// </summary>
        public const byte Version = 0;

        /// <summary>
        /// Operation code for external address requests.
        /// </summary>
        public const byte OperationExternalAddressRequest = 0;

        /// <summary>
        /// Operation code for UDP port mapping.
        /// </summary>
        public const byte OperationCodeUdp = 1;

        /// <summary>
        /// Operation code for TCP port mapping.
        /// </summary>
        public const byte OperationCodeTcp = 2;

        /// <summary>
        /// Server operation code for no-operation (NOOP).
        /// </summary>
        public const byte ServerNoop = 128;

        /// <summary>
        /// The client-side port used for PMP communications.
        /// </summary>
        public const int ClientPort = 5350;

        /// <summary>
        /// The server-side port used for PMP communications.
        /// </summary>
        public const int ServerPort = 5351;

        /// <summary>
        /// The delay in milliseconds between retry attempts for PMP operations.
        /// </summary>
        public const int RetryDelay = 250;

        /// <summary>
        /// The number of retry attempts for PMP operations before giving up.
        /// </summary>
        public const int RetryAttempts = 9;

        /// <summary>
        /// The recommended lease time in seconds for port mappings.
        /// </summary>
        public const int RecommendedLeaseTime = 60 * 60;

        /// <summary>
        /// The default lease time in seconds for port mappings, set to the recommended lease time.
        /// </summary>
        public const int DefaultLeaseTime = RecommendedLeaseTime;

        /// <summary>
        /// Result code indicating a successful operation.
        /// </summary>
        public const short ResultCodeSuccess = 0;

        /// <summary>
        /// Result code indicating the PMP version is not supported.
        /// </summary>
        public const short ResultCodeUnsupportedVersion = 1;

        /// <summary>
        /// Result code indicating the client is not authorized for the requested operation.
        /// </summary>
        public const short ResultCodeNotAuthorized = 2;

        /// <summary>
        /// Result code indicating a network failure prevented the operation from completing.
        /// </summary>
        public const short ResultCodeNetworkFailure = 3;

        /// <summary>
        /// Result code indicating the server is out of resources and cannot fulfill the request.
        /// </summary>
        public const short ResultCodeOutOfResources = 4;

        /// <summary>
        /// Result code indicating the operation code is not supported by the server.
        /// </summary>
        public const short ResultCodeUnsupportedOperationCode = 5;
    }

}