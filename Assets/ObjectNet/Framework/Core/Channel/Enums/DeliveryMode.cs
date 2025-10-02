namespace com.onlineobject.objectnet {
    /// <summary>
    /// An enumeration to represent the delivery mode of a message or data packet.
    /// </summary>
    public enum DeliveryMode {
        /// <summary>
        /// Reliable delivery mode ensures that the message or data packet is guaranteed to reach its destination.
        /// This mode typically involves some form of acknowledgment and retransmission in case of failure.
        /// </summary>
        Reliable,

        /// <summary>
        /// Unreliable delivery mode does not guarantee that the message or data packet will reach its destination.
        /// This mode is usually faster than Reliable as it does not require acknowledgment or retransmission.
        /// It is suitable for scenarios where speed is critical and some data loss is acceptable.
        /// </summary>
        Unreliable
    }
}
