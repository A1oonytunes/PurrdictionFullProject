using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A database for managing network prefabs in a Unity project.
    /// </summary>
    public class NetworkPrefabsDatabase : ScriptableObject {

        // Holds a list of network prefab entries.
        [HideInInspector]
        [SerializeField]
        private List<NetworkPrefabEntry> Prefabs = new List<NetworkPrefabEntry>();

        /// <summary>
        /// Retrieves all registered prefab entries.
        /// </summary>
        /// <returns>An array of NetworkPrefabEntry objects.</returns>
        public NetworkPrefabEntry[] GetPrefabs() {
            return this.Prefabs.ToArray();
        }

        /// <summary>
        /// Retrieves the GameObjects for all registered prefab entries.
        /// </summary>
        /// <returns>An array of GameObjects.</returns>
        public GameObject[] GetPrefabObjects() {
            List<GameObject> result = new List<GameObject>();
            foreach (NetworkPrefabEntry entry in this.Prefabs) {
                result.Add(entry.GetPrefab());
            }
            return result.ToArray();
        }

        /// <summary>
        /// Generates the next available network ID for a new prefab entry.
        /// </summary>
        /// <returns>The next available network ID as an integer.</returns>
        public int GetNextId() {
            int result = 0;
            foreach (NetworkPrefabEntry entry in this.Prefabs) {
                result = Mathf.Max(result, entry.GetId());
            }
            return ++result;
        }

        /// <summary>
        /// Checks if a prefab is already registered in the database.
        /// </summary>
        /// <param name="prefab">The GameObject to check.</param>
        /// <returns>True if the prefab exists, false otherwise.</returns>
        public bool PrefabExists(GameObject prefab) {
            bool result = false;
            foreach (NetworkPrefabEntry prefabEntry in this.Prefabs) {
                result |= (prefabEntry.GetPrefab().Equals(prefab));
                if (result) {
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves a prefab entry by its network ID.
        /// </summary>
        /// <param name="prefabId">The network ID of the prefab.</param>
        /// <returns>The corresponding NetworkPrefabEntry, or null if not found.</returns>
        public NetworkPrefabEntry GetPrefab(int prefabId) {
            NetworkPrefabEntry result = null;
            foreach (NetworkPrefabEntry prefabEntry in this.Prefabs) {
                if (prefabEntry.GetId().Equals(prefabId)) {
                    result = prefabEntry;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves a prefab entry by its GameObject.
        /// </summary>
        /// <param name="prefab">The GameObject of the prefab.</param>
        /// <returns>The corresponding NetworkPrefabEntry, or null if not found.</returns>
        public NetworkPrefabEntry GetPrefab(GameObject prefab) {
            NetworkPrefabEntry result = null;
            foreach (NetworkPrefabEntry prefabEntry in this.Prefabs) {
                if (prefabEntry.GetPrefab().Equals(prefab)) {
                    result = prefabEntry;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Registers a new prefab in the database.
        /// </summary>
        /// <param name="networkId">The network ID for the new prefab.</param>
        /// <param name="prefab">The GameObject of the new prefab.</param>
        /// <returns>The newly created or existing NetworkPrefabEntry.</returns>
        public NetworkPrefabEntry RegisterPrefab(int networkId, GameObject prefab) {
            NetworkPrefabEntry result = null;
            if (!this.PrefabExists(prefab)) {
                result = new NetworkPrefabEntry(networkId, prefab);
                this.Prefabs.Add(result);
                // Inject "NetworkInstantiateDetection" component into registered prefab
                NetworkObjectExtension.InjectNetwork(prefab);
            } else {
                result = this.GetPrefab(prefab);
            }
            return result;
        }

        /// <summary>
        /// Unregisters a prefab by its GameObject.
        /// </summary>
        /// <param name="prefab">The GameObject of the prefab to unregister.</param>
        public void UnregisterPrefab(GameObject prefab) {
            if (this.PrefabExists(prefab)) {
                this.Prefabs.Remove(this.GetPrefab(prefab));
            }
        }

        /// <summary>
        /// Unregisters a prefab by its NetworkPrefabEntry.
        /// </summary>
        /// <param name="prefabToRemove">The NetworkPrefabEntry of the prefab to unregister.</param>
        public void UnregisterPrefab(NetworkPrefabEntry prefabToRemove) {
            if (this.Prefabs.Contains(prefabToRemove)) {
                this.Prefabs.Remove(prefabToRemove);
            }
        }

    }

}