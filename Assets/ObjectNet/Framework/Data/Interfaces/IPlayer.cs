namespace com.onlineobject.objectnet {
    /// <summary>
    /// Interface representing a player in a game or application.
    /// </summary>
    public interface IPlayer {

        /// <summary>
        /// Retrieves the unique identifier for the player.
        /// </summary>
        /// <returns>A ushort representing the player's unique ID.</returns>
        ushort GetPlayerId();

        /// <summary>
        /// Retrieves the name of the player.
        /// </summary>
        /// <returns>A string representing the player's name.</returns>
        string GetPlayerName();

        /// <summary>
        /// Determines if the player is the local player (i.e., the player on this machine).
        /// </summary>
        /// <returns>A bool indicating whether the player is local.</returns>
        bool IsLocal();

        /// <summary>
        /// Sets the player's local status.
        /// </summary>
        /// <param name="value">A bool indicating the local status to set for the player.</param>
        void SetLocal(bool value);

        /// <summary>
        /// Determines if the player is the master player (e.g., the host or leader in a multiplayer setting).
        /// </summary>
        /// <returns>A bool indicating whether the player is the master.</returns>
        bool IsMaster();

        /// <summary>
        /// Sets the player's master status.
        /// </summary>
        /// <param name="value">A bool indicating the master status to set for the player.</param>
        void SetMaster(bool value);

        /// <summary>
        /// Retrieves the lobby identifier that the player is part of.
        /// </summary>
        /// <returns>A ushort representing the lobby's unique ID.</returns>
        ushort GetLobbyId();

        /// <summary>
        /// Sets the lobby identifier for the player.
        /// </summary>
        /// <param name="lobbyId">A ushort representing the lobby's unique ID to set for the player.</param>
        void SetLobbyId(ushort lobbyId);

        /// <summary>
        /// Return if peer to peer is avaiable to this player
        /// </summary>
        /// <returns>true if is avaiable.</returns>
        bool IsPeerAvaiable();

        /// <summary>
        /// Set if peer to peer is avaiable to this player
        /// </summary>
        /// <param name="value">Is avaiable or not.</param>
        void SetPeerAvaiable(bool value);

        /// <summary>
        /// Gets the port number of the client on p2p connections.
        /// </summary>
        /// <returns>The port number.</returns>
        int GetPeerToPeerPort();

        /// <summary>
        /// Set the port number of the client on p2p connections.
        /// </summary>
        /// <param name="value">Peer to Peer port number</param>
        void SetPeerToPeerPort(ushort value);
    }

}