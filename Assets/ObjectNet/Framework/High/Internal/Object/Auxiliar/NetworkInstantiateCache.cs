using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Class for caching and registering network instantiated game objects.
    /// </summary>
    public class NetworkInstantiateCache {

        private List<Type> autoRegisterComponents = new List<Type>(); // List of component types to automatically register
        private Dictionary<int, GameObject> cache = new Dictionary<int, GameObject>(); // Cache for storing instantiated game objects

        /// <summary>
        /// Registers a game object in the cache and injects any required component types.
        /// </summary>
        /// <param name="obj">The game object to register.</param>
        public void Register(GameObject obj) {
            if (!this.cache.ContainsKey(obj.GetInstanceID())) {
                this.cache.Add(obj.GetInstanceID(), obj); // Add the game object to the cache
                this.InjectTypes(obj); // Inject any required component types
            }
        }

        /// <summary>
        /// Injects the specified component types into the given game object if they are not already present.
        /// </summary>
        /// <param name="obj">The game object to inject component types into.</param>
        private void InjectTypes(GameObject obj) {
            foreach (Type type in this.autoRegisterComponents) {
                if (obj.GetComponent(type) == null) {
                    // Register component if not already present
                    obj.AddComponent(type);
                }
            }
        }
    }

}