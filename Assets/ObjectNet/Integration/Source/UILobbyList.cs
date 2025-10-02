using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.onlineobject.objectnet.integration {
    /// <summary>
    /// The UILobbyList class manages the lobby UI elements and interactions in a multiplayer game.
    /// </summary>
    public class UILobbyList : MonoBehaviour {

        // UI button to create a new lobby.
        public Button CreateLobbyButton;

        // UI button to refresh the list of available lobbies.
        public Button RefreshLobbyButton;

        // Input field for entering the name of a new lobby.
        public InputField LobbyName;

        // The root GameObject where lobby items will be instantiated.
        public GameObject LobbyItemsRoot;

        // The prefab for an individual lobby item in the list.
        public GameObject LobbyItem;

        // A dictionary to keep track of the current lobbies and their associated GameObjects.
        private Dictionary<ILobby, GameObject> Lobbies = new Dictionary<ILobby, GameObject>();

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// Here we are setting up listeners for the Create and Refresh lobby buttons.
        /// </summary>
        void Awake() {
            this.CreateLobbyButton.onClick.AddListener(CreateLobby);
            this.RefreshLobbyButton.onClick.AddListener(RefreshLobby);
        }

        /// <summary>
        /// Sends a request to create a lobby with the name specified in the LobbyName input field.
        /// </summary>
        private void CreateLobby() {
            NetworkManager.Instance().RequestLobbyCreate(this.LobbyName.text);
        }

        /// <summary>
        /// Sends a request to refresh the list of available lobbies.
        /// </summary>
        private void RefreshLobby() {
            NetworkManager.Instance().RequestLobbyRefresh();
        }

        /// <summary>
        /// LateUpdate is called every frame, if the MonoBehaviour is enabled.
        /// It is used here to update the lobby list UI based on the latest lobby information.
        /// </summary>
        private void LateUpdate() {
            // Only update the lobby list if not in relay mode.
            if (!NetworkManager.Instance().InRelayMode()) {
                // Add new lobbies to the UI.
                foreach (ILobby lobby in NetworkManager.Lobbies.GetLobbies()) {
                    if (!this.Lobbies.ContainsKey(lobby)) {
                        GameObject newItem = Instantiate(this.LobbyItem);
                        UILobbyItem lobbyItem = newItem.GetComponent<UILobbyItem>();
                        lobbyItem.label.text = lobby.GetLobbyName();
                        lobbyItem.button.onClick.AddListener(() => {
                            NetworkManager.Instance().RequestLobbyJoin(lobby.GetLobbyId()); // Send lobby join request.
                        });
                        newItem.transform.SetParent(this.LobbyItemsRoot.transform, false);
                        this.Lobbies.Add(lobby, newItem);
                    }
                }
                // Remove lobbies that are no longer available.
                List<ILobby> removedLobbies = new List<ILobby>();
                foreach (ILobby lobby in this.Lobbies.Keys) {
                    bool found = false;
                    foreach (ILobby lobbyData in NetworkManager.Lobbies.GetLobbies()) {
                        found |= (lobby.Equals(lobbyData));
                    }
                    if (!found) {
                        removedLobbies.Add(lobby);
                    }
                }
                while (removedLobbies.Count > 0) {
                    GameObject objToRemove = this.Lobbies[removedLobbies[0]];
                    this.Lobbies.Remove(removedLobbies[0]);
                    removedLobbies.RemoveAt(0);
                    objToRemove.transform.SetParent(null);
                    Destroy(objToRemove);
                }
            }
        }
    }

}