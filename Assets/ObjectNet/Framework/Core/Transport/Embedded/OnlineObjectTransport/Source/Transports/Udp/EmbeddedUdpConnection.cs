using com.onlineobject.objectnet.embedded.Utils;
using System;
using System.Collections.Generic;
using System.Net;

namespace com.onlineobject.objectnet.embedded.Transports.Udp {
    /// <summary>Represents a connection to a <see cref="EmbeddedUdpServer"/> or <see cref="EmbeddedUdpClient"/>.</summary>
    public class EmbeddedUdpConnection : EmbeddedConnection, IEquatable<EmbeddedUdpConnection>
    {
        /// <summary>The endpoint representing the other end of the connection.</summary>
        public readonly IPEndPoint RemoteEndPoint;

        /// <summary>The local peer this connection is associated with.</summary>
        private readonly EmbeddedUdpPeer peer;

        /// <summary>Initializes the connection.</summary>
        /// <param name="remoteEndPoint">The endpoint representing the other end of the connection.</param>
        /// <param name="peer">The local peer this connection is associated with.</param>
        internal EmbeddedUdpConnection(IPEndPoint remoteEndPoint, EmbeddedUdpPeer peer)
        {
            RemoteEndPoint = remoteEndPoint;
            this.peer = peer;
        }

        /// <inheritdoc/>
        protected internal override void Send(byte[] dataBuffer, int amount)
        {
            peer.Send(dataBuffer, amount, RemoteEndPoint);
        }

        /// <inheritdoc/>
        public override string ToString() => RemoteEndPoint.ToStringBasedOnIPFormat();

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as EmbeddedUdpConnection);
        /// <inheritdoc/>
        public bool Equals(EmbeddedUdpConnection other)
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
        public static bool operator ==(EmbeddedUdpConnection left, EmbeddedUdpConnection right)
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
        public static bool operator !=(EmbeddedUdpConnection left, EmbeddedUdpConnection right) => !(left == right);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
