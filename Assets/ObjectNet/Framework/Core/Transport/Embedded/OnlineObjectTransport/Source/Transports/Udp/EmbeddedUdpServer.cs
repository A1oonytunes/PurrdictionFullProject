using System;
using System.Collections.Generic;
using System.Net;

namespace com.onlineobject.objectnet.embedded.Transports.Udp {
    /// <summary>A server which can accept connections from <see cref="EmbeddedUdpClient"/>s.</summary>
    public class EmbeddedUdpServer : EmbeddedUdpPeer, IEmbeddedServer
    {
        /// <inheritdoc/>
        public event EventHandler<ConnectedEventArgs> Connected;
        /// <inheritdoc/>
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <inheritdoc/>
        public ushort Port { get; private set; }

        /// <summary>The currently open connections, accessible by their endpoints.</summary>
        private Dictionary<IPEndPoint, EmbeddedConnection> connections;
        /// <summary>The IP address to bind the socket to, if any.</summary>
        private IPAddress listenAddress;

        /// <inheritdoc/>
        public EmbeddedUdpServer(SocketMode mode = SocketMode.Both, int socketBufferSize = DefaultSocketBufferSize) : base(mode, socketBufferSize) { }

        /// <summary>Initializes the transport, binding the socket to a specific IP address.</summary>
        /// <param name="listenAddress">The IP address to bind the socket to.</param>
        /// <param name="socketBufferSize">How big the socket's send and receive buffers should be.</param>
        public EmbeddedUdpServer(IPAddress listenAddress, int socketBufferSize = DefaultSocketBufferSize) : base(SocketMode.Both, socketBufferSize)
        {
            this.listenAddress = listenAddress;
        }

        /// <inheritdoc/>
        public void Start(ushort port)
        {
            Port = port;
            connections = new Dictionary<IPEndPoint, EmbeddedConnection>();

            OpenSocket(listenAddress, port);
        }

        public void Start(string address, ushort port) {
            Port = port;
            listenAddress = IPAddress.Parse(address);
            connections = new Dictionary<IPEndPoint, EmbeddedConnection>();

            OpenSocket(listenAddress, port);
        }

        /// <summary>Decides what to do with a connection attempt.</summary>
        /// <param name="fromEndPoint">The endpoint the connection attempt is coming from.</param>
        /// <returns>Whether or not the connection attempt was from a new connection.</returns>
        private bool HandleConnectionAttempt(IPEndPoint fromEndPoint)
        {
            if (connections.ContainsKey(fromEndPoint))
                return false;

            EmbeddedUdpConnection connection = new EmbeddedUdpConnection(fromEndPoint, this);
            connections.Add(fromEndPoint, connection);
            OnConnected(connection);
            return true;
        }

        /// <inheritdoc/>
        public void Close(EmbeddedConnection connection)
        {
            if (connection is EmbeddedUdpConnection udpConnection)
                connections.Remove(udpConnection.RemoteEndPoint);
        }

        /// <inheritdoc/>
        public void Shutdown()
        {
            CloseSocket();
            connections.Clear();
        }

        /// <summary>Invokes the <see cref="Connected"/> event.</summary>
        /// <param name="connection">The successfully established connection.</param>
        protected virtual void OnConnected(EmbeddedConnection connection)
        {
            Connected?.Invoke(this, new ConnectedEventArgs(connection));
        }

        /// <inheritdoc/>
        protected override void OnDataReceived(byte[] dataBuffer, int amount, IPEndPoint fromEndPoint)
        {
            if ((MessageHeader)(dataBuffer[0] & EmbeddedMessage.HeaderBitmask) == MessageHeader.Connect && !HandleConnectionAttempt(fromEndPoint))
                return;

            if (connections.TryGetValue(fromEndPoint, out EmbeddedConnection connection) && !connection.IsNotConnected)
                DataReceived?.Invoke(this, new DataReceivedEventArgs(dataBuffer, amount, connection));
        }
    }
}
