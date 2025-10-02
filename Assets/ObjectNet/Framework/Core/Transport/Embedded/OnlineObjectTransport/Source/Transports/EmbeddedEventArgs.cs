namespace com.onlineobject.objectnet.embedded.Transports {
    /// <summary>Contains event data for when a server's transport successfully establishes a connection to a client.</summary>
    public struct ConnectedEventArgs
    {
        /// <summary>The newly established connection.</summary>
        public readonly EmbeddedConnection Connection;

        /// <summary>Initializes event data.</summary>
        /// <param name="connection">The newly established connection.</param>
        public ConnectedEventArgs(EmbeddedConnection connection)
        {
            Connection = connection;
        }
    }

    /// <summary>Contains event data for when a server's or client's transport receives data.</summary>
    public struct DataReceivedEventArgs
    {
        /// <summary>An array containing the received data.</summary>
        public readonly byte[] DataBuffer;
        /// <summary>The number of bytes that were received.</summary>
        public readonly int Amount;
        /// <summary>The connection which the data was received from.</summary>
        public readonly EmbeddedConnection FromConnection;

        /// <summary>Initializes event data.</summary>
        /// <param name="dataBuffer">An array containing the received data.</param>
        /// <param name="amount">The number of bytes that were received.</param>
        /// <param name="fromConnection">The connection which the data was received from.</param>
        public DataReceivedEventArgs(byte[] dataBuffer, int amount, EmbeddedConnection fromConnection)
        {
            DataBuffer = dataBuffer;
            Amount = amount;
            FromConnection = fromConnection;
        }
    }

    /// <summary>Contains event data for when a server's or client's transport initiates or detects a disconnection.</summary>
    public struct DisconnectedEventArgs
    {
        /// <summary>The closed connection.</summary>
        public readonly EmbeddedConnection Connection;
        /// <summary>The reason for the disconnection.</summary>
        public readonly DisconnectReason Reason;

        /// <summary>Initializes event data.</summary>
        /// <param name="connection">The closed connection.</param>
        /// <param name="reason">The reason for the disconnection.</param>
        public DisconnectedEventArgs(EmbeddedConnection connection, DisconnectReason reason)
        {
            Connection = connection;
            Reason = reason;
        }
    }
}
