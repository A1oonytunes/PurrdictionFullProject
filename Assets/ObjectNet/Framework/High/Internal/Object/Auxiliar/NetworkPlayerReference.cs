namespace com.onlineobject.objectnet {
    using UnityEngine;

    /// <summary>
    /// Represents a reference to a network player, containing client information and identifiers.
    /// </summary>
    public class NetworkPlayerReference : MonoBehaviour {
        // Reference to the network client interface
        private IClient client;

        // Reference to the network channel interface
        private IChannel channel;

        // Unique identifier for the connection
        private int connectionId = 0;

        // Unique identifier for the player
        private ushort playerId = 0;

        // is a player reconnection
        private bool reconnection = false;

        /// <summary>
        /// Configures the network player reference with the specified client, connection ID, and player ID.
        /// </summary>
        /// <param name="client">The network client interface.</param>
        /// <param name="connectionId">The unique identifier for the connection.</param>
        /// <param name="playerId">The unique identifier for the player.</param>
        public void Configure(IClient client, int connectionId, ushort playerId) {
            this.client = client;
            this.connectionId = connectionId;
            this.playerId = playerId;
        }

        /// <summary>
        /// Configures the network player reference with the specified channel, connection ID, and player ID.
        /// </summary>
        /// <param name="channel">The network channel interface.</param>
        /// <param name="connectionId">The unique identifier for the connection.</param>
        /// <param name="playerId">The unique identifier for the player.</param>
        public void Configure(IChannel channel, int connectionId, ushort playerId) {
            this.channel = channel;
            this.connectionId = connectionId;
            this.playerId = playerId;
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
        /// Retrieves the network client associated with this player.
        /// </summary>
        /// <returns>The network client interface.</returns>
        public IChannel GetChannel() {
            return this.channel;
        }

        /// <summary>
        /// Sets the network channel for this player.
        /// </summary>
        /// <param name="channel">The network channel interface to set.</param>
        public void SetChannel(IChannel channel) {
            this.channel = channel;
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
        /// Retrieves the player ID.
        /// </summary>
        /// <returns>The player ID as an unsigned short.</returns>
        public ushort GetPlayerId() {
            return this.playerId;
        }

        /// <summary>
        /// Sets the player ID for this player.
        /// </summary>
        /// <param name="value">The new player ID.</param>
        public void SetPlayerId(ushort value) {
            this.playerId = value;
        }

        /// <summary>
        /// Return if this is a reconnection
        /// </summary>
        /// <returns>Tue is is a reconnection.</returns>
        public bool IsReconnection() {
            return this.reconnection;
        }

        /// <summary>
        /// Sets if is a reconnection.
        /// </summary>
        /// <param name="value">Reconnection.</param>
        public void SetReconnection(bool value) {
            this.reconnection = value;
        }
    }

}