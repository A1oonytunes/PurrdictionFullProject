namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the types of transport protocols for network communication.
    /// </summary>
    public enum TransportPortType {
        /// <summary>
        /// Represents the Transmission Control Protocol (TCP),
        /// which is a connection-oriented protocol.
        /// </summary>
        Tcp,

        /// <summary>
        /// Represents the User Datagram Protocol (UDP),
        /// which is a connectionless protocol.
        /// </summary>
        Udp,

        /// <summary>
        /// Represents both TCP and UDP protocols.
        /// This may be used in contexts where either protocol can be used.
        /// </summary>
        Both
    }

}