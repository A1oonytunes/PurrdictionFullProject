namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a client-side communication channel that extends ChannelSocket.
    /// </summary>
    public class ClientChannel : ChannelSocket {

        /// <summary>
        /// Initializes a new instance of the ClientChannel class with the specified transport system.
        /// </summary>
        /// <param name="transportSystem">The transport system to be used by the channel (e.g., TCP, UDP).</param>
        public ClientChannel(string transportSystem) : base(ChannelDirection.Client, transportSystem) {
            // The constructor passes the channel direction and transport system to the base class constructor.
        }
    }
}