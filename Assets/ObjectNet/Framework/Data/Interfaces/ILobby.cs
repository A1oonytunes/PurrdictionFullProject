namespace com.onlineobject.objectnet {

    /// <summary>
    /// Interface definition for a game lobby.
    /// </summary>
    public interface ILobby {

        /// <summary>
        /// Retrieves the unique identifier for the lobby.
        /// </summary>
        /// <returns>A ushort representing the unique lobby ID.</returns>
        ushort GetLobbyId();

        /// <summary>
        /// Retrieves the name of the lobby.
        /// </summary>
        /// <returns>A string representing the name of the lobby.</returns>
        string GetLobbyName();

        /// <summary>
        /// Registers a player to the lobby.
        /// </summary>
        /// <param name="player">The player to register.</param>
        void RegisterPlayer(IPlayer player);

        /// <summary>
        /// Unregisters a player from the lobby.
        /// </summary>
        /// <param name="player">The player to unregister.</param>
        void UnregisterPlayer(IPlayer player);

        /// <summary>
        /// Checks if a player is registered in the lobby.
        /// </summary>
        /// <param name="player">The player to check registration status for.</param>
        /// <returns>True if the player is registered, false otherwise.</returns>
        bool IsPlayerRegistered(IPlayer player);

        /// <summary>
        /// Retrieves all registered players in the lobby.
        /// </summary>
        /// <returns>An array of IPlayer objects representing the registered players.</returns>
        IPlayer[] GetPlayers();

        /// <summary>
        /// Clears all registered players from the lobby.
        /// </summary>
        void ClearPlayers();

    }

}