using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// Represents a serializable list of script statuses, providing methods to manage the list.
    /// </summary>
    [Serializable]
    public sealed class ScriptList : System.Object {

        // Holds the list of script statuses.
        [SerializeField]
        private List<ScriptStatus> Scripts = new List<ScriptStatus>();

        /// <summary>
        /// Checks if the specified MonoBehaviour script is contained within the list.
        /// </summary>
        /// <param name="script">The MonoBehaviour script to check for.</param>
        /// <returns>True if the script is contained in the list; otherwise, false.</returns>
        public bool ContainsScript(MonoBehaviour script) {
            bool result = false;
            foreach (ScriptStatus scriptEntry in this.Scripts) {
                if (scriptEntry.Script == script) {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Registers a new MonoBehaviour script in the list if it is not already present.
        /// </summary>
        /// <param name="script">The MonoBehaviour script to register.</param>
        public void RegisterScript(MonoBehaviour script) {
            bool exists = false;
            foreach (ScriptStatus scriptEntry in this.Scripts) {
                if (scriptEntry.Script == script) {
                    exists = true;
                    break;
                }
            }
            if (!exists) {
                // Adds a new ScriptStatus to the list and refreshes its variables.
                this.Scripts.Add(new ScriptStatus(script, typeof(NetworkBehaviour).IsAssignableFrom(script.GetType())).RefreshVariables());
                // Sort script by game object order
                this.SortScriptsOrder();
            }
        }

        /// <summary>
        /// Unregisters a MonoBehaviour script from the list if it exists.
        /// </summary>
        /// <param name="script">The MonoBehaviour script to unregister.</param>
        public void UnRegisterScript(MonoBehaviour script) {
            bool exists = false;
            int index = 0;
            foreach (ScriptStatus scriptEntry in this.Scripts) {
                if (scriptEntry.Script == script) {
                    exists = true;
                    break;
                }
                index++;
            }
            if (exists) {
                // Removes the script at the found index.
                this.Scripts.RemoveAt(index);
                // Sort script by game object order
                this.SortScriptsOrder();
            }
        }

        /// <summary>
        /// Retrieves the list of registered scripts.
        /// </summary>
        /// <returns>A list of ScriptStatus objects.</returns>
        public List<ScriptStatus> GetScripts() {
            return this.Scripts;
        }

        /// <summary>
        /// To ensure that child object will keep ordered
        /// </summary>
        public void SortScriptsOrder() {
            try {
                this.Scripts.Sort(delegate (ScriptStatus scriptOne, ScriptStatus scriptTwo) { return scriptOne.Script.gameObject.GetInstanceID().CompareTo(scriptTwo.Script.gameObject.GetInstanceID()); });
            } catch(Exception err) {
                NetworkDebugger.LogDebug(err.Message);
            }
        }
        

        /// <summary>
        /// Clears all registered scripts from the list.
        /// </summary>
        public void ClearScripts() {
            this.Scripts.Clear();
        }
    }

}