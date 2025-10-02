using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Manages the active state of child GameObjects for a networked player, with support for delayed disabling.
    /// </summary>
    public class NetworkChildsReference : MonoBehaviour {

        // Stores GameObjects that should be disabled after a delay.
        private List<GameObjectStatus> PendingObjects = new List<GameObjectStatus>();

        // Reference to the player interface.
        private IPlayer player;

        // Flag to check if the disable operation has been executed.
        private bool disableExecuted = false;

        // Flag to check if the enable operation has been executed.
        private bool enableExecuted = false;

        // Stores the time at which the disable operation was executed.
        private float disabledExecutionTime = 0f;

        const float MILLISECONDS_MULTIPLIER = 1000.0f;

        /// <summary>
        /// Initializes a new instance of the NetworkChildsReference class.
        /// </summary>
        public NetworkChildsReference() {
        }

        private void OnEnable() {
            foreach (var delayedObject in this.PendingObjects) {
                delayedObject.Executed = false;
            }
        }

        /// <summary>
        /// LateUpdate is called every frame, if the MonoBehaviour is enabled.
        /// It is used here to process delayed disabling of GameObjects.
        /// </summary>
        void LateUpdate() {
            if (this.disableExecuted) {
                this.enableExecuted = true;
                if (this.PendingObjects.Count > 0) {
                    foreach (var delayedObject in this.PendingObjects) {
                        if (!delayedObject.Executed) {
                            if ((delayedObject.Delay == 0) ||
                                ((this.disabledExecutionTime + (delayedObject.Delay / MILLISECONDS_MULTIPLIER)) < NetworkClock.time)) {
                                delayedObject.Executed = true;
                                delayedObject.TargetInstance.SetActive(false);
                            }
                        }
                        this.enableExecuted &= delayedObject.Executed;
                    }                    
                }
                if (this.enableExecuted) {
                    this.EnableChilds();
                    this.enableExecuted = false;
                    
                }
            }
        }

        /// <summary>
        /// Collects child GameObjects and their respective delay times for later processing.
        /// </summary>
        /// <param name="delayed">A list of Tuples containing GameObjects and their delay times.</param>
        public void Collect(List<GameObjectStatus> delayed = null) {
            foreach (GameObjectStatus delayedChild in delayed) {
                this.PendingObjects.Add(delayedChild);                
            }
        }

        /// <summary>
        /// Immediately disables all child GameObjects that are not set to be delayed.
        /// </summary>
        public void DisableChilds() {
            List<GameObjectStatus> toRemove = new List<GameObjectStatus>();
            foreach (GameObjectStatus pending in this.PendingObjects) {
                if (pending.TargetInstance != null) {
                    if (pending.Delay == 0) {
                        pending.Executed = true;
                        pending.TargetInstance.SetActive(false);
                    }
                } else {
                    toRemove.Add(pending);
                }
            }
            while (toRemove.Count > 0) {
                this.PendingObjects.Remove(toRemove[0]);
                toRemove.RemoveAt(0);
            }
            this.disableExecuted        = true;
            this.disabledExecutionTime  = NetworkClock.time;
        }

        /// <summary>
        /// Enables all child GameObjects based on their original active state before disabling.
        /// </summary>
        public void EnableChilds() {
            foreach (GameObjectStatus pending in this.PendingObjects) {
                if (pending.Executed) {
                    if (pending.Enabled) {
                        if (pending.Target.activeSelf && !pending.TargetInstance.activeSelf) {
                            pending.TargetInstance.SetActive(true);
                        }
                    }
                }
            }
            this.disableExecuted        = false;
            this.disabledExecutionTime  = 0;
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