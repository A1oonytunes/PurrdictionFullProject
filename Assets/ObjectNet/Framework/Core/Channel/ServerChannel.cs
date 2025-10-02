namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a server channel for communication with a specific transport system.
    /// </summary>
    public class ServerChannel : ChannelSocket {
        /// <summary>
        /// Initializes a new instance of the ServerChannel class with the specified transport system.
        /// </summary>
        /// <param name="transportSystem">The transport system used for communication.</param>
        public ServerChannel(string transportSystem) : base(ChannelDirection.Server, transportSystem) {
        }
    }

}