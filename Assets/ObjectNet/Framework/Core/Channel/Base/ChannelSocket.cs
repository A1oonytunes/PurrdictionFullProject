namespace com.onlineobject.objectnet
{
    /// <summary>
    /// Represents a network channel that uses sockets for communication.
    /// </summary>
    /// <remarks>
    /// The ChannelSocket class extends the Channel class, providing specific implementations
    /// for socket-based communication channels.
    /// </remarks>
    public class ChannelSocket : Channel {
        
        /// <summary>
        /// Initializes a new instance of the ChannelSocket class with the specified direction and transport system.
        /// </summary>
        /// <param name="direction">The direction of the channel (input/output).</param>
        /// <param name="transportSystem">The transport system used by the channel (e.g., TCP, UDP).</param>
        public ChannelSocket(ChannelDirection direction, string transportSystem) : base(direction, transportSystem) {
            // The constructor is calling the base class constructor with the provided direction and transport system.
        }
    }
}