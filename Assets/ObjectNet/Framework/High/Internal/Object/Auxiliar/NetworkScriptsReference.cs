using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Manages the network-related scripts attached to a GameObject, providing functionality to enable/disable scripts
    /// based on network conditions, and to handle delayed script execution.
    /// </summary>
    public class NetworkScriptsReference : MonoBehaviour {

        // A list of script types that should be ignored when collecting and managing scripts.
        List<Type> IgnoredScripts = new List<Type>();

        // A dictionary mapping player scripts to their enabled state.
        Dictionary<MonoBehaviour, Boolean> PlayerScripts = new Dictionary<MonoBehaviour, Boolean>();

        // A dictionary mapping scripts to a delay time after which they should be disabled.
        Dictionary<MonoBehaviour, float> DelayedScripts = new Dictionary<MonoBehaviour, float>();

        // A dictionary to keep track of the original delay times for scripts.
        Dictionary<MonoBehaviour, float> OriginalDelayedScripts = new Dictionary<MonoBehaviour, float>();

        // A list of all MonoBehaviour scripts attached to the GameObject.
        List<MonoBehaviour> ComponentScripts = new List<MonoBehaviour>();

        // A dictionary mapping player input scripts to their enabled state.
        Dictionary<MonoBehaviour, Boolean> PlayerInputScripts = new Dictionary<MonoBehaviour, Boolean>();

        // Reference to the player interface.
        IPlayer player;

        // Network Object reference for this script container
        NetworkObject networkObjectReference;

        // Check for changes in this obejct authority
        bool wasActive = false;

        // Flags to check if disable/enable operations have been executed.
        bool disableExecuted = false;
        bool enableExecuted = false;

        // Time at which the scripts were disabled.
        float disabledExecutionTime = 0f;

        // A global list of script types that should be ignored across all instances.
        static List<Type> GlobalIgnoredScripts = new List<Type>();

        // Static constructor to initialize the global ignored script types.
        static NetworkScriptsReference() {
            NetworkScriptsReference.IgnoreType(typeof(NetworkObject));
            NetworkScriptsReference.IgnoreType(typeof(NetworkScriptsReference));
            NetworkScriptsReference.IgnoreType(typeof(NetworkInstantiateDetection));
            // NetworkScriptsReference.IgnoreType(typeof(NetworkInternalExecutor));
        }

        /// <summary>
        /// Constructor for the NetworkScriptsReference class.
        /// </summary>
        public NetworkScriptsReference() {
            if (this.IgnoredScripts.Count == 0) {
                // Add globally ignored script types to the instance's ignored list.
                foreach (Type ignored in NetworkScriptsReference.GlobalIgnoredScripts) {
                    this.IgnoredScripts.Add(ignored);
                }
            }
        }

        /// <summary>
        /// LateUpdate is called every frame, if the MonoBehaviour is enabled.
        /// It is used here to manage the delayed disabling of scripts.
        /// </summary>
        void LateUpdate() {
            if (this.disableExecuted == true) {
                // Disable all scripts flagged to be released after some amount of time.
                // Sometimes a script needs to execute its Awake or Start method only to configure something.
                if (this.DelayedScripts.Count > 0) {
                    List<MonoBehaviour> toRemove = new List<MonoBehaviour>();
                    foreach (var delayedScript in this.DelayedScripts) {
                        if ((this.disabledExecutionTime + delayedScript.Value) < NetworkClock.time) {
                            toRemove.Add(delayedScript.Key);
                            delayedScript.Key.enabled = false;
                        }
                    }
                    while (toRemove.Count > 0) {
                        MonoBehaviour removedScript = toRemove[0];
                        toRemove.RemoveAt(0);
                        this.DelayedScripts.Remove(removedScript);
                    }
                }
                if (this.enableExecuted == true) {
                    this.EnableComponents();
                    this.enableExecuted = false;
                }
            }
            // Associate the NetworkObject was in owner of this ScriptReferece
            if (this.networkObjectReference == null) {
                this.networkObjectReference = GetComponent<NetworkObject>();
                if (this.networkObjectReference != null) {
                    wasActive = this.networkObjectReference.IsActive();
                    // if the object is client only it will switch to active without triggering authority change
                    if (networkObjectReference.GetAccessLevel() == OwnerShipAccessLevel.ClientOnly) {
                        if (this.networkObjectReference.IsActive()) {
                            this.EnableComponents();
                        } else {
                            this.DisableComponents();
                        }
                    }
                }
                return;
            } else if (this.networkObjectReference.IsActive() != wasActive) {
                // Whenever the system detects any changes on the authority of this object I need to evaluate if I'm going to enable or disable components
                this.wasActive = this.networkObjectReference.IsActive();
                if (this.networkObjectReference.IsActive()) {
                    this.EnableComponents();
                } else {
                    this.DisableComponents();
                }
            }
        }

        /// <summary>
        /// Adds a script type to the global ignore list.
        /// </summary>
        /// <param name="scriptType">The type of the script to ignore.</param>
        public static void IgnoreType(Type scriptType) {
            NetworkScriptsReference.GlobalIgnoredScripts.Add(scriptType);
        }

        /// <summary>
        /// Adds a MonoBehaviour's type to the global ignore list.
        /// </summary>
        /// <param name="script">The MonoBehaviour instance whose type is to be ignored.</param>
        public static void IgnoreType(MonoBehaviour script) {
            NetworkScriptsReference.GlobalIgnoredScripts.Add(script.GetType());
        }

        /// <summary>
        /// Checks if a script type is in the global ignore list.
        /// </summary>
        /// <param name="scriptType">The type of the script to check.</param>
        /// <returns>True if the script type is ignored, false otherwise.</returns>
        public static bool IsIgnoreType(Type scriptType) {
            return NetworkScriptsReference.GlobalIgnoredScripts.Contains(scriptType);
        }

        /// <summary>
        /// Checks if a MonoBehaviour's type is in the global ignore list.
        /// </summary>
        /// <param name="script">The MonoBehaviour instance to check.</param>
        /// <returns>True if the MonoBehaviour's type is ignored, false otherwise.</returns>
        public static bool IsIgnoreType(MonoBehaviour script) {
            return NetworkScriptsReference.GlobalIgnoredScripts.Contains(script.GetType());
        }

        /// <summary>
        /// Registers a script type to be ignored by this instance.
        /// </summary>
        /// <param name="scriptType">The type of the script to ignore.</param>
        public void RegisterIgnored(Type scriptType) {
            if (!this.IgnoredScripts.Contains(scriptType)) {
                this.IgnoredScripts.Add(scriptType);
            }
        }

        /// <summary>
        /// Collects and categorizes scripts attached to the GameObject for network management.
        /// </summary>
        /// <param name="ignore">List of scripts to ignore.</param>
        /// <param name="delayed">List of scripts to delay with their respective delay times.</param>
        /// <param name="ignoredOnRemoteInput">List of scripts to ignore when remote input is considered.</param>
        public void Collect(List<MonoBehaviour> ignore = null, List<Tuple<MonoBehaviour, float>> delayed = null,
                            List<MonoBehaviour> ignoredOnRemoteInput = null) {
            MonoBehaviour[] playerScripts = this.gameObject.GetComponentsInChildren<MonoBehaviour>();
            // Normal scripts
            foreach (MonoBehaviour script in playerScripts) {
                if ( script != null ) { 
                    bool shallIgnore = false;
                    bool shallDelay = false;
                    float delay = 0f;
                    foreach (Type ignoredScript in this.IgnoredScripts) {
                        shallIgnore |= (script.GetType() == ignoredScript);
                    }
                    foreach (MonoBehaviour ignoredScript in ignore) {
                        shallIgnore |= (script.GetType() == ignoredScript.GetType());
                        if (shallIgnore) {
                            break;
                        }
                    }
                    if (!shallIgnore) {
                        foreach (Tuple<MonoBehaviour, float> delayedScript in delayed) {
                            if (script.GetType() == delayedScript.Item1.GetType()) {
                                shallDelay |= ((shallIgnore == false) && (delayedScript.Item2 > 0f));
                                if (shallDelay) {
                                    delay = delayedScript.Item2;
                                    break;
                                }
                            }
                        }
                    }
                    if (!shallIgnore) {
                        this.PlayerScripts.Add(script, script.enabled);
                        if ((shallDelay == true) && (script.enabled == true)) {
                            this.DelayedScripts.Add(script, delay);
                            this.OriginalDelayedScripts.Add(script, delay);
                        }
                    }
                } else {
                    NetworkDebugger.LogError("There are missing script on \"{0}\"", this.gameObject.name);
                }
            }
            // Normal scripts for remote input
            foreach (MonoBehaviour script in playerScripts) {
                if (script != null) {
                    bool shallIgnore = false;
                    foreach (Type ignoredScript in this.IgnoredScripts) {
                        if (ignoredScript == null) continue;
                        shallIgnore |= (script.GetType() == ignoredScript);
                    }
                    foreach (MonoBehaviour ignoredScript in ignoredOnRemoteInput) {
                        if (ignoredScript == null) continue;
                        shallIgnore |= (script.GetType() == ignoredScript.GetType());
                        if (shallIgnore) {
                            break;
                        }
                    }
                    if (!shallIgnore) {
                        this.PlayerInputScripts.Add(script, script.enabled);
                    }
                } else {
                    NetworkDebugger.LogError("There are missing script on \"{0}\"", this.gameObject.name);
                }
            }
        }

        /// <summary>
        /// Collects all MonoBehaviour scripts attached to the GameObject.
        /// </summary>
        public void CollectAll() {
            MonoBehaviour[] playerScripts = this.gameObject.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour script in playerScripts) {
                if (script != null) {
                    this.ComponentScripts.Add(script);
                } else {
                    NetworkDebugger.LogError("There are missing script on \"{0}\"", this.gameObject.name);
                }                
            }
        }

        /// <summary>
        /// Initializes network variables for synchronization.
        /// </summary>
        /// <param name="script">The script containing the fields to be synchronized.</param>
        /// <param name="synchronizedFields">Array of field names to be synchronized.</param>
        public void InitalizeNetworkVariables(MonoBehaviour script, VariableStatus[] synchronizedFields) {
            foreach (MonoBehaviour playerScript in this.ComponentScripts) {
                if ((playerScript != null) && (script != null)) {
                    if (script.GetType() == playerScript.GetType()) {
                        if (typeof(NetworkBehaviour).IsAssignableFrom(playerScript.GetType())) {
                            foreach (VariableStatus field in synchronizedFields) {
                                (playerScript as NetworkBehaviour).RegisterSynchonizedField(field.Variable, field.DeliveryMode, field.DetectionMode);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Disables all player scripts that are not set to be delayed.
        /// </summary>
        public void DisableComponents() {
            foreach (var script in this.PlayerScripts) {
                if ((script.Key != null) && (this.DelayedScripts.ContainsKey(script.Key) == false)) {
                    script.Key.enabled = false;
                }
            }
            this.disableExecuted = true;
            this.disabledExecutionTime = NetworkClock.time;
            if (this.DelayedScripts.Count == 0) {
                foreach (var entry in this.OriginalDelayedScripts) {
                    if (entry.Key != null) {
                        this.DelayedScripts.Add(entry.Key, entry.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Enables all player scripts that were previously enabled.
        /// </summary>
        public void EnableComponents() {
            foreach (var script in this.PlayerScripts) {
                if (script.Value == true) {
                    if (script.Key != null) {
                        script.Key.enabled = true;
                    }
                }
            }
            this.disableExecuted = false;
            this.disabledExecutionTime = 0;
        }

        /// <summary>
        /// Disables all input-related player scripts that are not set to be delayed.
        /// </summary>
        public void DisableInputComponents() {
            foreach (var script in this.PlayerScripts) {
                if (this.DelayedScripts.ContainsKey(script.Key) == false) {
                    if (script.Key != null) {
                        script.Key.enabled = false;
                    }
                }
            }
            this.disableExecuted = true;
            this.disabledExecutionTime = NetworkClock.time;
            if (this.DelayedScripts.Count == 0) {
                foreach (var entry in this.OriginalDelayedScripts) {
                    if (entry.Key != null) {
                        this.DelayedScripts.Add(entry.Key, entry.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Enables all input-related player scripts that were previously enabled.
        /// </summary>
        public void EnableInputComponents() {
            foreach (var script in this.PlayerInputScripts) {
                if (script.Value == true) {
                    if (script.Key != null) {
                        script.Key.enabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the player interface reference.
        /// </summary>
        /// <param name="player">The player interface to set.</param>
        public void SetPlayer(IPlayer player) {
            this.player = player;
        }

        /// <summary>
        /// Gets the player interface reference.
        /// </summary>
        /// <returns>The player interface.</returns>
        public IPlayer GetPlayer() {
            return this.player;
        }

    }

}