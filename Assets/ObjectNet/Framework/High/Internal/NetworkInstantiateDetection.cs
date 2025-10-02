using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// A MonoBehaviour class that handles the instantiation and destruction of networked objects.
    /// </summary>
    public class NetworkInstantiateDetection : MonoBehaviour {

        // Serialized field to store if this object was already in scene when game starts
        [SerializeField]
        private bool staticSpawn;

        // Serialized object network id
        [SerializeField]
        private int staticId = 0;

        // Serialized field to store the unique signature of the instance prefab.
        [SerializeField]
        private string instancePrefabSignature;

        [SerializeField]
        private List<GameObject> childToDisable = new List<GameObject>();

        [SerializeField]
        private List<GameObject> childToSync = new List<GameObject>();

        private List<GameObject> cachedChildToDisable = new List<GameObject>();

        private List<GameObject> cachedChildToSync = new List<GameObject>();

        private bool alreadyRegistered = false;

        private float nextRegisterTime = 0f;

        const float DETECTION_INTERVAL = 1.0f;

        /// <summary>
        /// Debug attributes section
        /// 
        /// All attributes below are related with debug and diagnosis aspects only
        /// </summary>
        [NonSerialized]
        private NetworkObject networkControl = null;

        [NonSerialized]
        private Bounds renderBounds;

        [NonSerialized]
        private bool boundsCollected = false;

        [NonSerialized]
        private Color activeDrawColor = Color.blue;

        [NonSerialized]
        private Color passiveDrawColor = Color.yellow;

        [NonSerialized]
        private Action boundsRefresh;
        
        const float DELAY_TO_MOVE_BACK_TO_SCENE = 100;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// Registers the game object with the network manager if the application is playing.
        /// </summary>
        private void Awake() {
            if (Application.isPlaying) {
                if (NetworkManager.Instance() != null) {
                    NetworkManager.Instance().RegisterDetectedObject(this.gameObject, this.staticSpawn);
                }
            }
        }

        /// <summary>
        /// OnDestroy is called when the MonoBehaviour will be destroyed.
        /// Unregisters the network object from the network manager if the application is playing.
        /// </summary>
        void OnDestroy() {
            if (Application.isPlaying) {
                NetworkObject networkObject = this.gameObject.GetComponent<NetworkObject>();
                if (networkObject != null) {
                    if (NetworkManager.Instance() != null) {
                        NetworkManager.Instance().RegisterDestroyedObject(networkObject.GetNetworkId());
                        // Remove object from inScene objects
                        if (NetworkManager.Instance().InSceneObjectRegistered(this.gameObject)) {
                            NetworkManager.Instance().UnRegisterInSceneObject(this.gameObject, true);
                            NetworkManager.Container.UnRegister(networkObject.GetNetworkElement());
                        }
                    } else if (NetworkDebuggerManager.Instance()){
                        NetworkDebugger.LogWarning(String.Format("[ NetworkManager ] Requested before being initialized : Origin [ {0} ]", this.gameObject.name));
                    }
                }
            }
        }

        /// <summary>
        /// LateUpdate is called every frame, if the MonoBehaviour is enabled.
        /// In debug mode, destroys the component immediately if the application is not playing.
        /// </summary>
        private void LateUpdate() {
            if (this.alreadyRegistered == false) {
                if (this.nextRegisterTime < Time.time) {
                    if (NetworkManager.Instance() != null) {
                        if (NetworkManager.Instance().HasConnection()) {
                            if (NetworkManager.Instance().GetConnection().IsConnected()) {
                                if (NetworkManager.Container != null) {
                                    this.nextRegisterTime = Time.time + DETECTION_INTERVAL;
                                    if (NetworkManager.Instance().IsRunningLogic()) {
                                        this.alreadyRegistered = true;
                                        if (NetworkManager.Container.IsRegistered(this.gameObject) == false) {
                                            NetworkManager.Instance().RegisterDetectedObject(this.gameObject);
                                        } 
                                        if (NetworkManager.Instance().InSceneObjectRegistered(this.gameObject) == false) {
                                            NetworkManager.Instance().RegisterInSceneObject(this.gameObject); // Register InScene
                                        }
                                    } else if (NetworkManager.Instance().InSceneObjectRegistered(this.gameObject) == false) {
                                        if (NetworkManager.Instance().InSceneCollectedAllowed()) {
                                            NetworkManager.Instance().RegisterInSceneObject(this.gameObject);
                                            NetworkManager.Instance().FlagInSceneObjectsCollected();                                            
                                            this.alreadyRegistered = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Assigns a new unique signature to the instance if it doesn't already have one.
        /// </summary>
        public void Sign() {
            if (this.instancePrefabSignature == null) {
                this.instancePrefabSignature = Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Checks if the provided NetworkInstantiateDetection has the same prefab signature as this instance.
        /// </summary>
        /// <param name="compared">The NetworkInstantiateDetection to compare with.</param>
        /// <returns>True if both instances have the same prefab signature; otherwise, false.</returns>
        public bool IsSamePrefabOrigin(NetworkInstantiateDetection compared) {
            return (compared.instancePrefabSignature.Equals(this.instancePrefabSignature));
        }

        /// <summary>
        /// Gets the unique signature of the prefab instance.
        /// </summary>
        /// <returns>The unique signature as a string.</returns>
        public string GetPrefabSignature() {
            return this.instancePrefabSignature;
        }


        /// <summary>
        /// Return if this object is already in scene
        /// </summary>
        /// <returns>True is is static spawned</returns>
        public bool IsStaticSpawn() {
            return this.staticSpawn;
        }


        /// <summary>
        /// Return statid object ID
        /// </summary>
        /// <returns>Static id</returns>
        public int GetStaticId() {
            return this.staticId;
        }

        /// <summary>
        /// Register some child to be disabled when script starts
        /// </summary>
        /// <param name="child">Child to disable</param>
        public void RegisterChildToDisable(GameObject child) {
            if (this.childToDisable.Contains(child) == false) {
                this.childToDisable.Add(child);
            }
        }

        /// <summary>
        /// Unregister some child to be disabled when script starts
        /// </summary>
        /// <param name="child">Child to unregister</param>
        public void UnRegisterChildToDisable(GameObject child) {
            if (this.childToDisable.Contains(child) == true) {
                this.childToDisable.Remove(child);
            }
        }

        /// <summary>
        /// Clear all registered childs to disable
        /// </summary>
        public void ClearChildToDisable() {
            this.childToDisable.Clear();
        }


        /// <summary>
        /// Register some child to be sync when script starts
        /// </summary>
        /// <param name="child">Child to sync</param>
        public void RegisterChildToSync(GameObject child) {
            if (this.childToSync.Contains(child) == false) {
                this.childToSync.Add(child);
            }
        }

        /// <summary>
        /// Unregister some child to be sync when script starts
        /// </summary>
        /// <param name="child">Child to unregister</param>
        public void UnRegisterChildToSync(GameObject child) {
            if (this.childToSync.Contains(child) == true) {
                this.childToSync.Remove(child);
            }
        }

        /// <summary>
        /// Clear all registered childs to sync
        /// </summary>
        public void ClearChildToSync() {
            this.childToSync.Clear();
        }


        /// <summary>
        /// Return if this object if flagged to be disabled
        /// </summary>
        /// <param name="child">Child to check</param>
        /// <param name="removeIfExists">Remove from list if occurency exists</param>
        /// <returns>Trtue if exists, otherwise false</returns>
        public bool IsFlaggedToDisable(GameObject child, bool removeIfExists = false) {
            bool exists = (removeIfExists) ? (this.cachedChildToDisable.Contains(child) == true) : (this.childToDisable.Contains(child) == true);
            if (exists) {
                this.cachedChildToDisable.Remove(child);
            }
            return exists;
        }

        /// <summary>
        /// Return if this object if flagged to be sync
        /// </summary>
        /// <param name="child">Child to check</param>
        /// <param name="removeIfExists">Remove from list if occurency exists</param>
        /// <returns>Trtue if exists, otherwise false</returns>
        public bool IsFlaggedToSync(GameObject child, bool removeIfExists = false) {
            bool exists = (removeIfExists) ? (this.cachedChildToSync.Contains(child) == true) : (this.childToSync.Contains(child) == true);
            if (exists) {
                this.cachedChildToSync.Remove(child);
            }
            return exists;
        }
        
        /// <summary>
        /// Restore all elements tio cache
        /// </summary>
        public void ResetFlaggedCache() {
            this.cachedChildToDisable.Clear();
            this.cachedChildToSync.Clear();
            this.cachedChildToDisable.AddRange(this.childToDisable);
            this.cachedChildToSync.AddRange(this.childToSync);
        }

        void OnDrawGizmos() {
            if ((NetworkDebugger.Instance() == null) ||
                (NetworkDebugger.Instance().ShowGizmos == true)) { 
                if (this.networkControl == null) {
                    this.networkControl = this.GetComponent<NetworkObject>();
                }
                if (this.boundsCollected == false) {
                    CharacterController controller = this.GetComponent<CharacterController>();
                    if (controller != null) {
                        this.boundsRefresh = () => {
                            this.renderBounds = controller.bounds;
                        };                    
                        this.boundsCollected = true;
                    } else {
                        Renderer render = this.GetComponent<Renderer>();
                        if (render != null) {
                            this.boundsRefresh = () => {
                                this.renderBounds = render.bounds;
                            };
                            this.boundsCollected = true;
                        } else {
                            Renderer[] renders = this.GetComponentsInChildren<Renderer>();
                            if (renders.Length > 0) {
                                Renderer usedRender = null;
                                foreach (Renderer r in renders) {
                                    if (this.renderBounds.size.magnitude < r.bounds.size.magnitude) {
                                        this.renderBounds = r.bounds;
                                        usedRender = r;
                                    }
                                }
                                this.boundsRefresh = () => {
                                    this.renderBounds = usedRender.bounds;
                                };
                                this.boundsCollected = true;
                            } else {
                                Collider[] colliders = this.GetComponentsInChildren<Collider>();
                                if (colliders.Length > 0) {
                                    Collider usedCollider = null;
                                    foreach (Collider c in colliders) {
                                        if (this.renderBounds.size.magnitude < c.bounds.size.magnitude) {
                                            this.renderBounds = c.bounds;
                                            usedCollider = c;
                                        }
                                    }
                                    this.boundsRefresh = () => {
                                        this.renderBounds = usedCollider.bounds;
                                    };
                                    this.boundsCollected = true;
                                }
                            }
                        }
                    }
                }
                if ((this.networkControl != null) && (this.boundsCollected == true)) {
                    this.boundsRefresh.Invoke();
                    var bounds      = this.renderBounds;
                    Gizmos.matrix   = Matrix4x4.identity;
                    Gizmos.color    = (BehaviorMode.Active.Equals(this.networkControl.GetBehaviorMode()) ? this.activeDrawColor : this.passiveDrawColor);
                    Gizmos.DrawWireCube(bounds.center, bounds.extents * 2);
                }
            }
        }
    }

}