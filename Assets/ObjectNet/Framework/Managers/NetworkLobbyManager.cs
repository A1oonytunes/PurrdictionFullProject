using System;
using System.Collections.Generic;
using System.Linq;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Manages network lobbies, including creation, retrieval, and player registration.
    /// </summary>
    public class NetworkLobbyManager {

        // Holds the ID of the last created lobby
        private ushort currentLobbyId = 0;

        // Dictionary to store lobbies with their corresponding IDs
        private Dictionary<ushort, ILobby> lobbyes = new Dictionary<ushort, ILobby>();

        private Action<ILobby> onLobbyFinished;

        // Minimum length for a lobby name
        public const int MIN_LOBBY_LENGHT_NAME = 6;

        // Maximum length for a lobby name
        public const int MAX_LOBBY_LENGHT_NAME = 10;

        /// <summary>
        /// Creates a new lobby with an optional name.
        /// </summary>
        /// <param name="name">The name of the lobby to create.</param>
        /// <returns>The created lobby instance.</returns>
        public ILobby CreateLobby(string name = null) {
            this.lobbyes.Add(++this.currentLobbyId, new NetworkLobby(this.currentLobbyId, name));
            return this.lobbyes[this.currentLobbyId];
        }

        /// <summary>
        /// Creates a new lobby with a specified ID and name.
        /// </summary>
        /// <param name="id">The ID for the new lobby.</param>
        /// <param name="name">The name of the lobby to create.</param>
        /// <returns>The created lobby instance.</returns>
        public ILobby CreateLobby(ushort id, string name) {
            this.lobbyes.Add(id, new NetworkLobby(id, name));
            return this.lobbyes[id];
        }

        /// <summary>
        /// Retrieves a lobby by its ID.
        /// </summary>
        /// <param name="id">The ID of the lobby to retrieve.</param>
        /// <returns>The lobby instance with the specified ID.</returns>
        public ILobby GetLobby(ushort id) {
            return (this.lobbyes.ContainsKey(id)) ? this.lobbyes[id] : null;
        }

        /// <summary>
        /// Retrieves all lobbies.
        /// </summary>
        /// <returns>An array of all lobby instances.</returns>
        public ILobby[] GetLobbies() {
            return this.lobbyes.Values.ToArray<ILobby>();
        }

        /// <summary>
        /// Gets the count of current lobbies.
        /// </summary>
        /// <returns>The number of lobbies.</returns>
        public int GetLobbiesCount() {
            return this.lobbyes.Count;
        }

        /// <summary>
        /// Checks if a lobby with the specified ID exists.
        /// </summary>
        /// <param name="id">The ID of the lobby to check.</param>
        /// <returns>True if the lobby exists, false otherwise.</returns>
        public bool HasLobby(ushort id) {
            return this.lobbyes.ContainsKey(id);
        }

        /// <summary>
        /// Registers a player to a lobby.
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby to register the player to.</param>
        /// <param name="player">The player to register.</param>
        public void RegisterPlayer(ushort lobbyId, IPlayer player) {
            if (!this.GetLobby(lobbyId).IsPlayerRegistered(player)) {
                this.UnregisterPlayer(player); // Remove from any other lobby first
                this.GetLobby(lobbyId).RegisterPlayer(player);
            }
        }

        /// <summary>
        /// Unregisters a player from any lobby they are currently registered in.
        /// </summary>
        /// <param name="player">The player to unregister.</param>
        public void UnregisterPlayer(IPlayer player) {
            foreach (ILobby lobby in this.lobbyes.Values) {
                if (lobby.IsPlayerRegistered(player)) {
                    lobby.RegisterPlayer(player);
                    break;
                }
            }
        }

        /// <summary>
        /// Closes a lobby and returns the players that were in it.
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby to close.</param>
        /// <returns>An array of players that were in the closed lobby.</returns>
        public IPlayer[] CloseLobby(ushort lobbyId) {
            IPlayer[] result = null;
            ILobby closedLobby = null;
            if (this.HasLobby(lobbyId)) {
                closedLobby = this.GetLobby(lobbyId);
                result = closedLobby.GetPlayers();
                closedLobby.ClearPlayers();
                this.lobbyes.Remove(lobbyId);
            }
            if (this.onLobbyFinished != null ) {
                if (closedLobby != null) {
                    this.onLobbyFinished.Invoke(closedLobby);
                }
            }            
            return result;
        }

        /// <summary>
        /// Unregister lobby from current lobbies
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby to unregister.</param>
        public void UnregisterLobby(ushort lobbyId) {
            if (this.lobbyes.ContainsKey(lobbyId)) {
                this.lobbyes.Remove(lobbyId);
            }
        }

        /// <summary>
        /// Determines if a player is in a lobby and outputs the lobby.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <param name="playerLobby">The lobby the player is in, if any.</param>
        /// <returns>True if the player is in a lobby, false otherwise.</returns>
        public bool InLobby(IPlayer player, out ILobby playerLobby) {
            bool result = false;
            playerLobby = null;
            foreach (ILobby lobby in this.lobbyes.Values) {
                result = lobby.IsPlayerRegistered(player);
                if (result) {
                    playerLobby = lobby;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves the lobby a player is currently in.
        /// </summary>
        /// <param name="player">The player whose lobby is to be retrieved.</param>
        /// <returns>The lobby the player is in, or null if the player is not in any lobby.</returns>
        public ILobby GetLobby(IPlayer player) {
            ILobby result = null;
            foreach (ILobby lobby in this.lobbyes.Values) {
                if (lobby.IsPlayerRegistered(player)) {
                    result = lobby;
                    break;
                }
            }
            return result;
        }


        /// <summary>
        /// Configure event to be executed when lobby was closed
        /// </summary>
        /// <param name="callback">Callback event</param>
        public void SetOnLobbyClosed(Action<ILobby> callback) {
            this.onLobbyFinished = callback;
        }
    }

}