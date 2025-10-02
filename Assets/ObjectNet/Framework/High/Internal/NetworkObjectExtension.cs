using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Provides extension methods for GameObjects to interact with network instantiation.
    /// </summary>
    public static class NetworkObjectExtension {

        /// <summary>
        /// Injects a network component into the GameObject if it does not already have one.
        /// </summary>
        /// <param name="gameObject">The GameObject to inject the network component into.</param>
        public static void InjectNetwork(this GameObject gameObject) {
            // Attempt to retrieve the NetworkInstantiateDetection component from the GameObject
            NetworkInstantiateDetection injector = gameObject.GetComponent<NetworkInstantiateDetection>();
            // If the component does not exist, add it to the GameObject
            if (injector == null) {
                injector = gameObject.AddComponent<NetworkInstantiateDetection>();
            }
            // Call the Sign method on the injector to complete the injection process
            injector.Sign();
        }

        /// <summary>
        /// Removes the network component from the GameObject if it exists.
        /// </summary>
        /// <param name="gameObject">The GameObject to remove the network component from.</param>
        public static void ReleaseNetwork(this GameObject gameObject) {
            // Retrieve the NetworkInstantiateDetection component from the GameObject
            NetworkInstantiateDetection injector = gameObject.GetComponent<NetworkInstantiateDetection>();
            // If the component exists, destroy it immediately
            if (injector != null) {
                GameObject.DestroyImmediate(injector);
            }
        }

        /// <summary>
        /// Checks if the GameObject has a network component attached.
        /// </summary>
        /// <param name="gameObject">The GameObject to check.</param>
        /// <returns>True if the GameObject has a NetworkInstantiateDetection component, false otherwise.</returns>
        public static bool IsNetworkComponent(this GameObject gameObject) {
            // Return true if the NetworkInstantiateDetection component is found, false otherwise
            return (gameObject.GetComponent<NetworkInstantiateDetection>() != null);
        }

        /// <summary>
        /// Checks if the GameObject is the same prefab as another GameObject by comparing their network components.
        /// </summary>
        /// <param name="gameObject">The GameObject to compare.</param>
        /// <param name="prefab">The other GameObject to compare against.</param>
        /// <returns>True if both GameObjects are the same prefab, false otherwise.</returns>
        public static bool IsSamePrefab(this GameObject gameObject, GameObject prefab) {
            // Retrieve the NetworkInstantiateDetection components from both GameObjects
            NetworkInstantiateDetection leftCompare = (gameObject != null) ? gameObject.GetComponent<NetworkInstantiateDetection>() : null;
            NetworkInstantiateDetection rightCompare = (prefab != null) ? prefab.GetComponent<NetworkInstantiateDetection>() : null;
            // Compare the components to determine if they are from the same prefab origin
            return ((leftCompare != null) &&
                    (rightCompare != null) &&
                    (leftCompare.IsSamePrefabOrigin(rightCompare)));
        }

        /// <summary>
        /// Checks if the GameObject is the same prefab as specified by a prefab signature.
        /// </summary>
        /// <param name="gameObject">The GameObject to compare.</param>
        /// <param name="prefabSignature">The signature of the prefab to compare against.</param>
        /// <returns>True if the GameObject's prefab signature matches the provided signature, false otherwise.</returns>
        public static bool IsSamePrefab(this GameObject gameObject, string prefabSignature) {
            // Retrieve the NetworkInstantiateDetection component from the GameObject
            NetworkInstantiateDetection instanceCompare = (gameObject != null) ? gameObject.GetComponent<NetworkInstantiateDetection>() : null;
            // Compare the component's prefab signature to the provided signature
            return ((instanceCompare != null) &&
                    (instanceCompare.GetPrefabSignature().Equals(prefabSignature)));
        }
    }

}