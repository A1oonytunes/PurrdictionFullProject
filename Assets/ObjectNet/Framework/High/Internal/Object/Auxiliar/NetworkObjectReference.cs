using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a client reference to a network object, containing client information and identifiers reagerd player that request to create this object.
    /// </summary>
    public class NetworkObjectReference : MonoBehaviour {
        // Reference to the network client interface
        private IClient client;

        // Unique identifier for the connection
        private int connectionId = 0;

        // Unique identifier for the transaction
        private uint spawnTransactionId = 0;

        /// <summary>
        /// Configures the network object reference with the specified client, connection ID, and player ID.
        /// </summary>
        /// <param name="client">The network client interface.</param>
        /// <param name="connectionId">The unique identifier for the connection.</param>
        public void Configure(IClient client, int connectionId) {
            this.client = client;
            this.connectionId = connectionId;
        }

        /// <summary>
        /// Retrieves the network client associated with this player.
        /// </summary>
        /// <returns>The network client interface.</returns>
        public IClient GetClient() {
            return this.client;
        }

        /// <summary>
        /// Sets the network client for this player.
        /// </summary>
        /// <param name="client">The network client interface to set.</param>
        public void SetClient(IClient client) {
            this.client = client;
        }

        /// <summary>
        /// Retrieves the connection ID for this player.
        /// </summary>
        /// <returns>The connection ID as an integer.</returns>
        public int GetConnectionId() {
            return this.connectionId;
        }

        /// <summary>
        /// Sets the connection ID for this player.
        /// </summary>
        /// <param name="value">The new connection ID.</param>
        public void SetConnectionId(int value) {
            this.connectionId = value;
        }

        /// <summary>
        /// Retrieves spawn transaction id
        /// </summary>
        /// <returns>The spawn transaction id.</returns>
        public uint GetTransactionId() {
            return this.spawnTransactionId;
        }

        /// <summary>
        /// Sets spawn transaction id.
        /// </summary>
        /// <param name="value">The spawn transaction id.</param>
        public void SetTransactionId(uint value) {
            this.spawnTransactionId = value;
        }
    }

}