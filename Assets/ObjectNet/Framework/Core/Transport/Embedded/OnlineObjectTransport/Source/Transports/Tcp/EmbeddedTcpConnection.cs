using com.onlineobject.objectnet.embedded.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace com.onlineobject.objectnet.embedded.Transports.Tcp {
    /// <summary>Represents a connection to a <see cref="EmbeddedTcpServer"/> or <see cref="EmbeddedTcpClient"/>.</summary>
    public class EmbeddedTcpConnection : EmbeddedConnection, IEquatable<EmbeddedTcpConnection>
    {
        /// <summary>The endpoint representing the other end of the connection.</summary>
        public readonly IPEndPoint RemoteEndPoint;

        /// <summary>Whether or not the server has received a connection attempt from this connection.</summary>
        internal bool DidReceiveConnect;

        /// <summary>The socket to use for sending and receiving.</summary>
        private readonly Socket socket;
        /// <summary>The local peer this connection is associated with.</summary>
        private readonly EmbeddedTcpPeer peer;
        /// <summary>An array to receive message size values into.</summary>
        private readonly byte[] sizeBytes = new byte[sizeof(int)];
        /// <summary>The size of the next message to be received.</summary>
        private int nextMessageSize;

        /// <summary>Initializes the connection.</summary>
        /// <param name="socket">The socket to use for sending and receiving.</param>
        /// <param name="remoteEndPoint">The endpoint representing the other end of the connection.</param>
        /// <param name="peer">The local peer this connection is associated with.</param>
        internal EmbeddedTcpConnection(Socket socket, IPEndPoint remoteEndPoint, EmbeddedTcpPeer peer)
        {
            RemoteEndPoint = remoteEndPoint;
            this.socket = socket;
            this.peer = peer;
        }

        /// <inheritdoc/>
        protected internal override void Send(byte[] dataBuffer, int amount)
        {
            if (amount == 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Sending 0 bytes is not allowed!");

            try
            {
                if (socket.Connected)
                {
                    EmbeddedConverter.FromInt(amount, peer.SendBuffer, 0);
                    Array.Copy(dataBuffer, 0, peer.SendBuffer, sizeof(int), amount); // TODO: consider sending length separately with an extra socket.Send call instead of copying the data an extra time
                    socket.Send(peer.SendBuffer, amount + sizeof(int), SocketFlags.None);
                }
            }
            catch (SocketException)
            {
                // May want to consider triggering a disconnect here (perhaps depending on the type
                // of SocketException)? Timeout should catch disconnections, but disconnecting
                // explicitly might be better...
            }
        }

        /// <summary>Polls the socket and checks if any data was received.</summary>
        internal void Receive()
        {
            bool tryReceiveMore = true;
            while (tryReceiveMore)
            {
                int byteCount = 0;
                try
                {
                    if (nextMessageSize > 0)
                    {
                        // We already have a size value
                        tryReceiveMore = TryReceiveMessage(out byteCount);
                    }
                    else if (socket.Available >= sizeof(int))
                    {
                        // We have enough bytes for a complete size value
                        socket.Receive(sizeBytes, sizeof(int), SocketFlags.None);
                        nextMessageSize = EmbeddedConverter.ToInt(sizeBytes, 0);
                        
                        if (nextMessageSize > 0)
                            tryReceiveMore = TryReceiveMessage(out byteCount);
                    }
                    else
                        tryReceiveMore = false;
                }
                catch (SocketException ex)
                {
                    tryReceiveMore = false;
                    switch (ex.SocketErrorCode)
                    {
                        case SocketError.Interrupted:
                        case SocketError.NotSocket:
                            peer.OnDisconnected(this, DisconnectReason.TransportError);
                            break;
                        case SocketError.ConnectionReset:
                            peer.OnDisconnected(this, DisconnectReason.Disconnected);
                            break;
                        case SocketError.TimedOut:
                            peer.OnDisconnected(this, DisconnectReason.TimedOut);
                            break;
                        case SocketError.MessageSize:
                            break;
                        default:
                            break;
                    }
                }
                catch (ObjectDisposedException)
                {
                    tryReceiveMore = false;
                    peer.OnDisconnected(this, DisconnectReason.TransportError);
                }
                catch (NullReferenceException)
                {
                    tryReceiveMore = false;
                    peer.OnDisconnected(this, DisconnectReason.TransportError);
                }

                if (byteCount > 0)
                    peer.OnDataReceived(byteCount, this);
            }
        }

        /// <summary>Receives a message, if all of its data is ready to be received.</summary>
        /// <param name="receivedByteCount">How many bytes were received.</param>
        /// <returns>Whether or not all of the message's data was ready to be received.</returns>
        private bool TryReceiveMessage(out int receivedByteCount)
        {
            if (socket.Available >= nextMessageSize)
            {
                // We have enough bytes to read the complete message
                receivedByteCount = socket.Receive(peer.ReceiveBuffer, nextMessageSize, SocketFlags.None);
                nextMessageSize = 0;
                return true;
            }

            receivedByteCount = 0;
            return false;
        }

        /// <summary>Closes the connection.</summary>
        internal void Close()
        {
            socket.Close();
        }

        /// <inheritdoc/>
        public override string ToString() => RemoteEndPoint.ToStringBasedOnIPFormat();

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as EmbeddedTcpConnection);
        /// <inheritdoc/>
        public bool Equals(EmbeddedTcpConnection other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return RemoteEndPoint.Equals(other.RemoteEndPoint);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return -288961498 + EqualityComparer<IPEndPoint>.Default.GetHashCode(RemoteEndPoint);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool operator ==(EmbeddedTcpConnection left, EmbeddedTcpConnection right)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (left is null)
            {
                if (right is null)
                    return true;

                return false; // Only the left side is null
            }

            // Equals handles case of null on right side
            return left.Equals(right);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool operator !=(EmbeddedTcpConnection left, EmbeddedTcpConnection right) => !(left == right);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
