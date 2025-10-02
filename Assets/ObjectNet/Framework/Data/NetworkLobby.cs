using System.Collections.Generic;
using System.Linq;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a network lobby that can contain players.
    /// </summary>
    public class NetworkLobby : ILobby {

        // Unique identifier for the lobby.
        ushort id = 0;

        // Name of the lobby.
        string name = "";

        // Dictionary to hold players with their IDs as keys.
        Dictionary<ushort, IPlayer> players = new Dictionary<ushort, IPlayer>();

        /// <summary>
        /// Constructs a new NetworkLobby instance with a specified ID and optional name.
        /// </summary>
        /// <param name="id">The unique identifier for the lobby.</param>
        /// <param name="name">The name of the lobby. If null, a default name is generated.</param>
        public NetworkLobby(ushort id, string name = null) {
            this.id = id;
            this.name = (name != null) ? name : string.Format("Lobby [{0}]", this.id);
        }

        /// <summary>
        /// Retrieves the lobby's unique identifier.
        /// </summary>
        /// <returns>The lobby's ID.</returns>
        public ushort GetLobbyId() {
            return this.id;
        }

        /// <summary>
        /// Retrieves the lobby's name.
        /// </summary>
        /// <returns>The name of the lobby.</returns>
        public string GetLobbyName() {
            return this.name;
        }

        /// <summary>
        /// Registers a player to the lobby if they are not already registered.
        /// </summary>
        /// <param name="player">The player to register.</param>
        public void RegisterPlayer(IPlayer player) {
            if (!this.players.ContainsKey(player.GetPlayerId())) {
                this.players.Add(player.GetPlayerId(), player);
                player.SetLobbyId(this.GetLobbyId());
            }
        }

        /// <summary>
        /// Unregisters a player from the lobby if they are registered.
        /// </summary>
        /// <param name="player">The player to unregister.</param>
        public void UnregisterPlayer(IPlayer player) {
            if (this.players.ContainsKey(player.GetPlayerId())) {
                this.players.Remove(player.GetPlayerId());
                player.SetLobbyId(0);
            }
        }

        /// <summary>
        /// Checks if a player is registered in the lobby.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns>True if the player is registered, false otherwise.</returns>
        public bool IsPlayerRegistered(IPlayer player) {
            return this.players.ContainsKey(player.GetPlayerId());
        }

        /// <summary>
        /// Retrieves an array of all registered players in the lobby.
        /// </summary>
        /// <returns>An array of IPlayer objects representing the registered players.</returns>
        public IPlayer[] GetPlayers() {
            return this.players.Values.ToArray<IPlayer>();
        }

        /// <summary>
        /// Clears all players from the lobby.
        /// </summary>
        public void ClearPlayers() {
            this.players.Clear();
        }
    }

}