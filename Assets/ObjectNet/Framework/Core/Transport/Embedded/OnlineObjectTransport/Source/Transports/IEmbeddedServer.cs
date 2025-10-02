using System;

namespace com.onlineobject.objectnet.embedded.Transports {
    /// <summary>Defines methods, properties, and events which every transport's server must implement.</summary>
    public interface IEmbeddedServer : IEmbeddedPeer
    {
        /// <summary>Invoked when a connection is established at the transport level.</summary>
        event EventHandler<ConnectedEventArgs> Connected;

        /// <inheritdoc cref="EmbeddedServer.Port"/>
        ushort Port { get; }
        
        /// <summary>Starts the transport and begins listening for incoming connections.</summary>
        /// <param name="port">The local port on which to listen for connections.</param>
        void Start(ushort port);

        /// <summary>
        /// Starts the transport and begins listening for incoming connections.
        /// </summary>
        /// <param name="address">Target address</param>
        /// <param name="port">The local port on which to listen for connections.</param>
        void Start(string address, ushort port);

        /// <summary>Closes an active connection.</summary>
        /// <param name="connection">The connection to close.</param>
        void Close(EmbeddedConnection connection);

        /// <summary>Closes all existing connections and stops listening for new connections.</summary>
        void Shutdown();
    }
}
