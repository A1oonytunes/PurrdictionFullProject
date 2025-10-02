namespace com.onlineobject.objectnet {

    /// <summary>
    /// Represents the transport protocols available for network communication channels.
    /// </summary>
    public enum ChannelTransport {
        // Represents the User Datagram Protocol (UDP) transport.
        Udp,

        // Represents the Transmission Control Protocol (TCP) transport.
        Tcp,

        // Represents a custom or hybrid transport protocol that uses both UDP and TCP.
        Bichannel
    }
}
