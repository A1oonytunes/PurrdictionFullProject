using System;
using UnityEngine;

namespace com.onlineobject.objectnet {

    // Define a serializable class to manage the status of a game object.
    [Serializable]
    public class GameObjectStatus {
        
        [SerializeField]
        public GameObject Target = null; // Reference to the target game object.

        [SerializeField]
        public Boolean Enabled = true; // Flag to determine if the game object is enabled.

        [SerializeField]
        public int ChildIndex = 0; // Index of child object to identify when disbale after spawn

        [SerializeField]
        public int Delay = 0; // Delay in seconds before the game object's status is applied.


        [SerializeField]
        public bool SyncPosition = false; // Flag indicating if this object must synchronize his position

        [SerializeField]
        public bool SyncRotation = false; // Flag indicating if this object must synchronize his rotation

        [SerializeField]
        public bool SyncScale = false; // Flag indicating if this object must synchronize his scale

        [NonSerialized]
        public GameObject TargetInstance = null;

        [NonSerialized]
        public bool Executed = false;

        /// <summary>
        /// Constructor for the GameObjectStatus class.
        /// </summary>
        private GameObjectStatus() {            
        }

        /// <summary>
        /// Constructor for the GameObjectStatus class.
        /// </summary>
        /// <param name="target">The game object this status will be associated with.</param>
        public GameObjectStatus(GameObject target) {
            this.Target = target; // Set the target game object.
        }

        public GameObjectStatus GenerateSafetyCopy() {
            GameObjectStatus result = new GameObjectStatus();
            result.Target           = this.Target;
            result.Enabled          = this.Enabled;
            result.Delay            = this.Delay;
            result.SyncPosition     = this.SyncPosition;
            result.SyncRotation     = this.SyncRotation;
            result.SyncScale        = this.SyncScale;
            result.TargetInstance   = null;
            return result;
        }
    }

}