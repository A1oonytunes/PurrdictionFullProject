using System;
using UnityEngine;

namespace com.onlineobject.objectnet.integration {
    /// <summary>
    /// Handles the activation and deactivation of the lobby UI based on network events.
    /// </summary>
    public class UILobbyActivation : MonoBehaviour {

        /// <summary>
        /// The lobby GameObject that will be activated or deactivated.
        /// </summary>
        public GameObject Lobby;

        /// <summary>
        /// Called when a client has successfully connected to the relay server.
        /// Activates the Lobby GameObject if connected to the lobby server.
        /// </summary>
        /// <param name="client">The client that connected to the relay.</param>
        public void ClientConnectedOnRelay(IClient client) {
            // Activate the Lobby GameObject if the client is connected to the lobby server
            this.Lobby.SetActive(NetworkManager.Instance().IsConnectedOnLobbyServer());
        }

        /// <summary>
        /// Called when the lobby creation process is successful.
        /// Logs the lobby ID and deactivates the Lobby GameObject.
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby that was created.</param>
        public void LobbyCreationSucess(ushort lobbyId) {
            // Log the successful lobby creation with its ID
            NetworkDebugger.Log(string.Format("Connected at lobby [{0}]", lobbyId));
            // Deactivate the Lobby GameObject
            this.Lobby.SetActive(false);
        }

        /// <summary>
        /// Called when successfully joining a lobby.
        /// Logs the lobby ID and deactivates the Lobby GameObject.
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby that was joined.</param>
        public void LobbyJoinSucess(ushort lobbyId) {
            // Log the successful lobby join with its ID
            NetworkDebugger.Log(string.Format("Joined at lobby [{0}]", lobbyId));
            // Deactivate the Lobby GameObject
            this.Lobby.SetActive(false);
        }

        /// <summary>
        /// Called when there is an error during the lobby creation process.
        /// Throws an exception with the provided reason.
        /// </summary>
        /// <param name="reason">The reason for the lobby creation error.</param>
        public void LobbyCreationError(string reason) {
            // Throw an exception with the error reason
            throw new Exception(reason);
        }

    }

}