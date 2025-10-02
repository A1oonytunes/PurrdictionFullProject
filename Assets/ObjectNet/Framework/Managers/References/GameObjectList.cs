using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// Represents a list of GameObjectStatus objects, providing methods to register,
    /// unregister, and query the presence of GameObjects.
    /// </summary>
    [Serializable]
    public class GameObjectList {

        // A list to hold the status of game objects.
        [SerializeField]
        private List<GameObjectStatus> Objects = new List<GameObjectStatus>();

        /// <summary>
        /// Checks if the specified GameObject is contained within the list.
        /// </summary>
        /// <param name="target">The GameObject to check for in the list.</param>
        /// <returns>True if the GameObject is present; otherwise, false.</returns>
        public bool ContainsObject(GameObject target) {
            bool result = false;
            foreach (GameObjectStatus objectEntry in this.Objects) {
                if (objectEntry.Target == target) {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Registers a GameObject by adding it to the list if it's not already present.
        /// </summary>
        /// <param name="target">The GameObject to register.</param>
        public void RegisterObject(GameObject target) {
            bool exists = false;
            foreach (GameObjectStatus objectEntry in this.Objects) {
                if (objectEntry.Target == target) {
                    exists = true;
                    break;
                }
            }
            if (!exists) {
                this.Objects.Add(new GameObjectStatus(target));
            }
        }

        /// <summary>
        /// Unregisters a GameObject by removing it from the list if it exists.
        /// </summary>
        /// <param name="target">The GameObject to unregister.</param>
        public void UnRegisterObject(GameObject target) {
            bool exists = false;
            int index = 0;
            foreach (GameObjectStatus scriptEntry in this.Objects) {
                if (scriptEntry.Target == target) {
                    exists = true;
                    break;
                }
                index++;
            }
            if (exists) {
                this.Objects.RemoveAt(index);
            }
        }

        /// <summary>
        /// Retrieves the list of GameObjectStatus objects.
        /// </summary>
        /// <returns>A list of GameObjectStatus objects.</returns>
        public List<GameObjectStatus> GetObjects() {
            return this.Objects;
        }

        /// <summary>
        /// Determines whether any GameObject in the list contains the specified MonoBehaviour script.
        /// </summary>
        /// <param name="script">The MonoBehaviour script to check for.</param>
        /// <returns>True if any GameObject contains the script; otherwise, false.</returns>
        public bool HasChildScript(MonoBehaviour script) {
            bool result = false;
            List<GameObjectStatus> childsToRemove = new List<GameObjectStatus>();
            foreach (GameObjectStatus obj in this.Objects) {
                if (obj.Target != null) {
                    result |= obj.Target.GetComponents<MonoBehaviour>().Contains(script);
                } else {
                    childsToRemove.Add(obj);
                }
            } 
            while (childsToRemove.Count > 0) {
                this.Objects.Remove(childsToRemove[0]);
                childsToRemove.RemoveAt(0);
            }
            return result;
        }
    }


}