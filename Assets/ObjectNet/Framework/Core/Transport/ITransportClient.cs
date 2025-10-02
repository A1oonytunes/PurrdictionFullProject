namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a transport client that can manage communication over a network or communication channel.
    /// </summary>
    /// <remarks>
    /// This interface extends the <see cref="ITransport"/> interface. It is a marker interface and does not
    /// define any additional members beyond those inherited from <see cref="ITransport"/>.
    /// Implementing classes should provide the specific details for how transport client functionality is handled.
    /// </remarks>
    public interface ITransportClient : ITransport {

        /// <summary>
        /// Registers a peer with the transport client.
        /// </summary>
        /// <param name="peer">The peer to register.</param>
        void RegisterPeer(ITransportPeer peer);

        /// <summary>
        /// Unregisters a peer from the transport client using the peer instance.
        /// </summary>
        /// <param name="peer">The peer to unregister.</param>
        void UnregisterPeer(ITransportPeer peer);

        /// <summary>
        /// Unregisters a peer from the transport client using the peer's unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the peer to unregister.</param>
        void UnregisterPeer(ushort id);

        /// <summary>
        /// Retrieves a peer from the transport client using the peer's unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the peer to retrieve.</param>
        /// <returns>The peer associated with the given identifier, or null if not found.</returns>
        ITransportPeer GetPeer(ushort id);

        /// <summary>
        /// Initializes a peer-to-peer server with the specified port.
        /// </summary>
        /// <param name="peerToPeerPort">The port to use for the peer-to-peer server.</param>
        void InitializePeerToPeerServer(ushort peerToPeerPort);

    }

}
