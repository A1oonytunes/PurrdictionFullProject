using System.Collections.Generic;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a network player in a game or application.
    /// </summary>
    public class NetworkPlayer : IPlayer {

        /// <summary>
        /// The unique identifier for the player.
        /// </summary>
        ushort id;

        /// <summary>
        /// The name of the player.
        /// </summary>
        string name;

        /// <summary>
        /// Indicates whether the player is local (on the current machine).
        /// </summary>
        bool local = false;

        /// <summary>
        /// Indicates whether the player is the master (host or leader) of the game or lobby.
        /// </summary>
        bool master = false;

        /// <summary>
        /// The identifier of the lobby that the player is part of.
        /// </summary>
        ushort lobbyId = 0;

        /// <summary>
        /// Flag if this client has peer to peer avaiable
        /// </summary>
        bool peerToPeer = false;

        // Port number of the client for P2P connections
        private int peerToPeerPort = 0;

        /// <summary>
        /// The network client associated with the player.
        /// </summary>
        IClient networkClient;

        /// <summary>
        /// The network channel associated with the player ( used only when client os the server on embedded mode ).
        /// </summary>
        IChannel networkChannel;

        /// <summary>
        /// Constructs a new instance of a NetworkPlayer.
        /// </summary>
        /// <param name="id">The unique identifier for the player.</param>
        /// <param name="name">The name of the player.</param>
        /// <param name="client">The network client associated with the player (optional).</param>
        public NetworkPlayer(ushort id, string name, IClient client = null) {
            this.id = id;
            this.name = name;
            this.networkClient = client;
        }

        /// <summary>
        /// Constructs a new instance of a NetworkPlayer.
        /// </summary>
        /// <param name="id">The unique identifier for the player.</param>
        /// <param name="name">The name of the player.</param>
        /// <param name="channel">The network channel associated with the player ( used only when client os the server on embedded mode ).</param>
        public NetworkPlayer(ushort id, string name, IChannel channel = null) {
            this.id = id;
            this.name = name;
            this.networkChannel = channel;
        }

        /// <summary>
        /// Gets the player's unique identifier.
        /// </summary>
        /// <returns>The player's ID.</returns>
        public ushort GetPlayerId() {
            return this.id;
        }

        /// <summary>
        /// Gets the player's name.
        /// </summary>
        /// <returns>The player's name.</returns>
        public string GetPlayerName() {
            return this.name;
        }

        /// <summary>
        /// Determines if the player is local.
        /// </summary>
        /// <returns>True if the player is local; otherwise, false.</returns>
        public bool IsLocal() {
            return this.local;
        }

        /// <summary>
        /// Sets whether the player is local.
        /// </summary>
        /// <param name="value">True to set the player as local; otherwise, false.</param>
        public void SetLocal(bool value) {
            this.local = value;            
        }

        /// <summary>
        /// Determines if the player is the master.
        /// </summary>
        /// <returns>True if the player is the master; otherwise, false.</returns>
        public bool IsMaster() {
            return this.master;
        }

        /// <summary>
        /// Sets whether the player is the master.
        /// </summary>
        /// <param name="value">True to set the player as master; otherwise, false.</param>
        public void SetMaster(bool value) {
            this.master = value;
        }

        /// <summary>
        /// Sets the network client for the player.
        /// </summary>
        /// <param name="client">The network client to associate with the player.</param>
        public void SetClient(IClient client) {
            this.networkClient = client;
        }

        /// <summary>
        /// Gets the network client associated with the player.
        /// </summary>
        /// <returns>The associated network client.</returns>
        public IClient GetClient() {
            return this.networkClient;
        }

        /// <summary>
        /// Gets the network client associated with the player.
        /// </summary>
        /// <returns>The associated network client.</returns>
        public List<IClient> GetClients() {
            return this.networkChannel.GetConnectedClients();
        }

        /// <summary>
        /// Determines if the player has an associated network channel.
        /// </summary>
        /// <returns>True if the player has a network channel; otherwise, false.</returns>
        public bool HasChannel() {
            return (this.networkChannel != null);
        }

        /// <summary>
        /// Sets the network channel for the player.
        /// </summary>
        /// <param name="client">The network channel to associate with the player.</param>
        public void SetChannel(IChannel client) {
            this.networkChannel = client;
        }

        /// <summary>
        /// Gets the network channel associated with the player.
        /// </summary>
        /// <returns>The associated network channel.</returns>
        public IChannel GetChannel() {
            return this.networkChannel;
        }

        /// <summary>
        /// Gets the lobby ID that the player is part of.
        /// </summary>
        /// <returns>The lobby ID.</returns>
        public ushort GetLobbyId() {
            return this.lobbyId;
        }

        /// <summary>
        /// Sets the lobby ID for the player.
        /// </summary>
        /// <param name="lobbyId">The lobby ID to associate with the player.</param>
        public void SetLobbyId(ushort lobbyId) {
            this.lobbyId = lobbyId;
        }

        /// <summary>
        /// Return if this player is visible on internet
        /// </summary>
        /// <returns>true if is visible.</returns>
        public bool IsPeerAvaiable() {
            return this.peerToPeer;
        }

        /// <summary>
        /// Set if this player is visible over internet.
        /// </summary>
        /// <param name="value">Visible or not.</param>
        public void SetPeerAvaiable(bool value) {
            this.peerToPeer = value;
        }

        /// <summary>
        /// Gets the port number of the client on p2p connections.
        /// </summary>
        /// <returns>The port number.</returns>
        public int GetPeerToPeerPort() {
            return this.peerToPeerPort;
        }

        /// <summary>
        /// Set the port number of the client on p2p connections.
        /// </summary>
        /// <param name="value">Peer to Peer port number</param>
        public void SetPeerToPeerPort(ushort value) {
            this.peerToPeerPort = value;
        }
    }

}