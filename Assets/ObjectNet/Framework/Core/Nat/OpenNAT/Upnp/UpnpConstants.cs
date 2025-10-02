namespace com.onlineobject.objectnet {
    /// <summary>
    /// Contains UPnP (Universal Plug and Play) constants for error and status codes.
    /// </summary>
    internal static class UpnpConstants {
        /// <summary>
        /// Error code for invalid arguments.
        /// </summary>
        public const int InvalidArguments = 402;

        /// <summary>
        /// Error code indicating that an action has failed.
        /// </summary>
        public const int ActionFailed = 501;

        /// <summary>
        /// Error code for unauthorized access.
        /// </summary>
        public const int Unathorized = 606;

        /// <summary>
        /// Error code for specifying an invalid index in an array.
        /// </summary>
        public const int SpecifiedArrayIndexInvalid = 713;

        /// <summary>
        /// Error code when there is no entry in the array corresponding to the specified index.
        /// </summary>
        public const int NoSuchEntryInArray = 714;

        /// <summary>
        /// Error code indicating that wildcards are not permitted in the source IP.
        /// </summary>
        public const int WildCardNotPermittedInSourceIp = 715;

        /// <summary>
        /// Error code indicating that wildcards are not permitted in the external port.
        /// </summary>
        public const int WildCardNotPermittedInExternalPort = 716;

        /// <summary>
        /// Error code for a conflict in the mapping entry.
        /// </summary>
        public const int ConflictInMappingEntry = 718;

        /// <summary>
        /// Error code indicating that the same port values are required.
        /// </summary>
        public const int SamePortValuesRequired = 724;

        /// <summary>
        /// Error code indicating that only permanent leases are supported.
        /// </summary>
        public const int OnlyPermanentLeasesSupported = 725;

        /// <summary>
        /// Error code indicating that the remote host only supports wildcard.
        /// </summary>
        public const int RemoteHostOnlySupportsWildcard = 726;

        /// <summary>
        /// Error code indicating that the external port only supports wildcard.
        /// </summary>
        public const int ExternalPortOnlySupportsWildcard = 727;

        /// <summary>
        /// Error code indicating that no port maps are available.
        /// </summary>
        public const int NoPortMapsAvailable = 728;

        /// <summary>
        /// Error code indicating a conflict with other mechanisms.
        /// </summary>
        public const int ConflictWithOtherMechanisms = 729;

        /// <summary>
        /// Error code indicating that wildcards are not permitted in the internal port.
        /// </summary>
        public const int WildCardNotPermittedInIntPort = 732;
    }

}