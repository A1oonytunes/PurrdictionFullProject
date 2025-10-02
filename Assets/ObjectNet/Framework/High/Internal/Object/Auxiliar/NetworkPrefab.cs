using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a networked prefab object with synchronization settings.
    /// </summary>
    public class NetworkPrefab : Object {

        /// <summary>
        /// The prefab GameObject that will be used in the networked environment.
        /// </summary>
        public GameObject Prefab;

        /// <summary>
        /// Flag to determine if the prefab should automatically synchronize its state across the network.
        /// </summary>
        /// <remarks>
        /// When set to true, the prefab's state is automatically synchronized with all clients in the network.
        /// If false, manual synchronization methods must be implemented.
        /// </remarks>
        public bool AutoSync = true;

    }

}