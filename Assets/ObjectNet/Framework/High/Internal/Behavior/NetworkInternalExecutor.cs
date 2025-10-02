using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A MonoBehaviour class that acts as a bridge for network-related behaviors.
    /// </summary>
    public class NetworkInternalExecutor : MonoBehaviour {

        // Reference to an object that implements INetworkBehaviorInternal interface.
        private List<INetworkBehaviorInternal> networkBehaviors = new List<INetworkBehaviorInternal>();

        private INetworkBehaviorInternal mainBehaviour;

        // Flag if executor was already initialized
        private bool executorInitialized = false;

        /// <summary>
        /// Registers a GameObject with a network behavior to this executor.
        /// </summary>
        /// <param name="target">The GameObject to which the network behavior is attached.</param>
        /// <param name="behavior">The network behavior to be registered with the executor.</param>
        public static NetworkInternalExecutor Register(GameObject target, 
                                                       INetworkBehaviorInternal behavior, 
                                                       Action onRegisterInternalExecutor,
                                                       Action<NetworkBehaviour> childStartAction) {
            NetworkInternalExecutor result = null;
            // Check if the target GameObject already has a NetworkInternalExecutor component.
            if (target.GetComponent<NetworkInternalExecutor>() == null) {
                // Add executor and set as result componet
                result = target.AddComponent<NetworkInternalExecutor>();
                // If not, add the NetworkInternalExecutor component and assign the network behavior.
                result.networkBehaviors.Add(behavior);
                // By objects lifecycle, when object start i haven't BehaviorMode defined yet, so i need to wait to be flagged before execute start
                result.enabled = false;
                // Store who is the main component
                result.mainBehaviour = behavior;
                // Will add on each NetworkBehaviour
                foreach (NetworkBehaviour child in target.GetComponentsInChildren<NetworkBehaviour>()) {
                    if (child.gameObject != target) {
                        result.networkBehaviors.Add(child);
                        if (childStartAction != null) {
                            childStartAction.Invoke(child);
                        }
                    } else if (!result.networkBehaviors.Contains(child)) {
                        result.networkBehaviors.Add(child);
                        child.CollectMethodsImplementations();
                        if (NetworkStartOrder.OnConnectionStablished.Equals(child.GetStartOrder())) {
                            child.OnNetworkStarted();                            
                        }
                    } else {
                        child.CollectMethodsImplementations();
                    }
                }
                // Call trigger after NetworkInternalExecutor has being assigned
                if (onRegisterInternalExecutor != null) {
                    onRegisterInternalExecutor.Invoke();
                }
                // Invoke onAwake
                if (NetworkStartOrder.OnConnectionStablished.Equals(behavior.GetStartOrder())) {
                    result.OnSetModeAwake();
                }
            } else {
                result = target.GetComponent<NetworkInternalExecutor>();
            }
            return result;
        }

        /// <summary>
        /// Return if this executor was alreaady initialized
        /// </summary>
        /// <returns>True is was initialized, otherwise false</returns>
        public bool WasInitialize() {
            return this.executorInitialized;
        }

        /// <summary>
        /// Initialize all executors registered on this executor
        /// </summary>
        public void InitializeExecutors() {
            if (this.executorInitialized == false) {
                this.executorInitialized    = true;
                this.enabled                = true;
                if (this.mainBehaviour != null) {
                    if (NetworkStartOrder.OnConnectionStablished.Equals(this.mainBehaviour.GetStartOrder())) {
                        (mainBehaviour as NetworkObject).InternalNetworkAwake();                        
                    }
                }
            }
        }

        /// <summary>
        /// Return the number of registered behaviours to execut
        /// </summary>
        /// <returns>Registered behaviours count</returns>
        public int GetBehaviourCounts() {
            return this.networkBehaviors.Count;
        }

        /// <summary>
        /// Unity's OnEnable method, called when the script instance is being loaded.
        /// </summary>
        void OnEnable() {
            // If a network behavior is assigned, call its InternalNetworkAwake method.
            /*
            if (this.awakeExecuted == false) {
                foreach (NetworkBehaviour behaviour in this.networkBehaviors) {
                    behaviour.SetBehaviorMode(this.mainBehaviour.GetBehaviorMode());
                }
                this.awakeExecuted = true;
            }
            */
            // If a network behavior is assigned, call its InternalNetworkOnEnable method.
            foreach (INetworkBehaviorInternal behaviour in this.networkBehaviors) { 
                behaviour.InternalNetworkOnEnable();
            }
        }

        /// <summary>
        /// Unity's OnDisable method, called when the script instance is being loaded.
        /// </summary>
        void OnDisable() {
            // If a network behavior is assigned, call its InternalNetworkOnEnable method.
            foreach (INetworkBehaviorInternal behaviour in this.networkBehaviors) { 
                behaviour.InternalNetworkOnDisable();
            }
        }

        /// <summary>
        /// Unity's Start method, called before the first frame update.
        /// </summary>
        void Awake() {
            // If a network behavior is assigned, call its InternalNetworkStart method.
            foreach (INetworkBehaviorInternal behaviour in this.networkBehaviors) {
                if (NetworkStartOrder.OnConnectionStablished.Equals(behaviour.GetStartOrder())) {
                    behaviour.InternalNetworkAwake();
                }
            }
        }

        /// <summary>
        /// Unity's Start method, called before the first frame update.
        /// </summary>
        public void OnSetModeAwake() {
            // If a network behavior is assigned, call its InternalNetworkStart method to be executed when mode was defined.
            foreach (INetworkBehaviorInternal behaviour in this.networkBehaviors) {
                if (NetworkStartOrder.OnModeAssigned.Equals(behaviour.GetStartOrder())) {
                    behaviour.InternalNetworkAwake();
                }
            }
        }

        /// <summary>
        /// Unity's Start method, called before the first frame update.
        /// </summary>
        void Start() {
            // If a network behavior is assigned, call its InternalNetworkStart method.
            foreach (INetworkBehaviorInternal behaviour in this.networkBehaviors) {
                if (NetworkStartOrder.OnConnectionStablished.Equals(behaviour.GetStartOrder())) {
                    behaviour.InternalNetworkStart();
                }
            }
        }

        /// <summary>
        /// Unity's Start method, called before the first frame update to be executed when mode was defined.
        /// </summary>
        public void OnSetModeStart() {
            // If a network behavior is assigned, call its InternalNetworkStart method.
            foreach (INetworkBehaviorInternal behaviour in this.networkBehaviors) {
                if (NetworkStartOrder.OnModeAssigned.Equals(behaviour.GetStartOrder())) {
                    behaviour.InternalNetworkStart();
                }
            }
        }

        /// <summary>
        /// Unity's Update method, called once per frame.
        /// </summary>
        void Update() {
            // If a network behavior is assigned, execute network process and call its InternalNetworkUpdate method.
            foreach (INetworkBehaviorInternal behaviour in this.networkBehaviors) {
                behaviour.UpdateSynchonizedVariables();
                behaviour.InternalNetworkUpdate();
            }
        }

        /// <summary>
        /// Unity's LateUpdate method, called after all Update methods have been called.
        /// </summary>
        void LateUpdate() {
            // If a network behavior is assigned, call its InternalNetworkLateUpdate method.
            foreach (INetworkBehaviorInternal behaviour in this.networkBehaviors) {
                behaviour.InternalNetworkLateUpdate();
            }
        }

        /// <summary>
        /// Unity's FixedUpdate method, called every fixed framerate frame.
        /// </summary>
        void FixedUpdate() {
            // If a network behavior is assigned, call its InternalNetworkFixedUpdate method.
            foreach (INetworkBehaviorInternal behaviour in this.networkBehaviors) {
                behaviour.ExecuteNetworkProcess();
                behaviour.InternalNetworkFixedUpdate();
            }
        }
    }

}