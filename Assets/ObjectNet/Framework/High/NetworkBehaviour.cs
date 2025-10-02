#pragma warning disable 0168
#pragma warning disable 0219
#pragma warning disable 0414

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Abstract class that defines the behavior of networked objects.
    /// </summary>
    public abstract class NetworkBehaviour : NetworkObject, INetworkBehavior {

        // Flag to show or hide network parameters in the inspector.
        [HideInInspector]
        [SerializeField]
        private bool ShowNetworkParameters = false;

        // Store the internal ID of this script into component, this is used to control NetworkVariables
        [HideInInspector]
        [SerializeField]
        private ushort BehaviorlId = 0;

        // Dictionary to hold method names and their corresponding MethodInfo objects.
        private Dictionary<string, MethodInfo> MethodsImplementation = new Dictionary<string, MethodInfo>();

        // List to keep track of synchronized fields.
        private Dictionary<string, Tuple<DeliveryMode, ReliableDetectionMode>> SynchronizedFields = new Dictionary<string, Tuple<DeliveryMode, ReliableDetectionMode>>();

        /// <summary>
        /// Virtual method to be overridden by derived classes to perform actions when the network starts.
        /// </summary>
        public virtual void OnNetworkStarted() {
        }

        /// <summary>
        /// Return associated behavior ID of this Behaviour
        /// </summary>
        /// <returns>Behaviour ID</returns>
        public ushort GetBehaviorId() {
            return this.BehaviorlId;
        }

        /// <summary>
        /// Retrieves the MethodInfo for a given method name within the current type.
        /// </summary>
        /// <param name="methodName">The name of the method to retrieve.</param>
        /// <returns>The MethodInfo object for the specified method.</returns>
        private MethodInfo GetInternalMethod(String methodName) {
            return this.GetType().GetMethod(methodName, BindingFlags.Public |
                                                        BindingFlags.NonPublic |
                                                        BindingFlags.Instance |
                                                        BindingFlags.DeclaredOnly);
        }

        /// <summary>
        /// Collects the MethodInfo for a given method name and adds it to the MethodsImplementation dictionary.
        /// </summary>
        /// <param name="methodName">The name of the method to collect.</param>
        private void CollectMethod(String methodName) {
            MethodInfo method = this.GetInternalMethod(methodName);
            if (method != null) {
                this.MethodsImplementation.Add(methodName, method);
            }
        }

        /// <summary>
        /// Return if methods was already collected
        /// </summary>
        /// <returns>True if methods was already collected, othrwise false</returns>
        internal bool WasMethodsCollected() {
            return (this.MethodsImplementation.Count > 0);
        }

        /// <summary>
        /// Collects the MethodInfo for all relevant network behavior methods.
        /// </summary>
        internal void CollectMethodsImplementations() {
            if (this.WasMethodsCollected() == false) {
                this.CollectMethod(NetworkBehaviorConstants.ActiveAwakeMethod);
                this.CollectMethod(NetworkBehaviorConstants.PassiveAwakeMethod);
                this.CollectMethod(NetworkBehaviorConstants.ActiveOnEnableMethod);
                this.CollectMethod(NetworkBehaviorConstants.PassiveOnEnableMethod);
                this.CollectMethod(NetworkBehaviorConstants.ActiveOnDisableMethod);
                this.CollectMethod(NetworkBehaviorConstants.PassiveOnDisableMethod);
                this.CollectMethod(NetworkBehaviorConstants.ActiveStartMethod);
                this.CollectMethod(NetworkBehaviorConstants.PassiveStartMethod);
                this.CollectMethod(NetworkBehaviorConstants.ActiveUpdateMethod);
                this.CollectMethod(NetworkBehaviorConstants.PassiveUpdateMethod);
                this.CollectMethod(NetworkBehaviorConstants.ActiveFixedUpdateMethod);
                this.CollectMethod(NetworkBehaviorConstants.PassiveFixedUpdateMethod);
                this.CollectMethod(NetworkBehaviorConstants.ActiveLateUpdateMethod);
                this.CollectMethod(NetworkBehaviorConstants.PassiveLateUpdateMethod);
            }
        }

        /// <summary>
        /// Starts the network behavior and collects method implementations.
        /// </summary>
        /// <param name="networkId">Optional network ID to start with.</param>
        public override void StartNetwork(int networkId = 0, Func<INetworkElement, INetworkElement> onNetworkStartedCallback = null) {
            base.StartNetwork(networkId, onNetworkStartedCallback);
            this.CollectMethodsImplementations();
            if (NetworkStartOrder.OnConnectionStablished.Equals(this.GetStartOrder())) {
                this.OnNetworkStarted();
            }            
        }

        /// </summary>
        // Executes the active awake method if it exists.
        /// </summary>
        private void ExecuteActiveAwake() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.ActiveAwakeMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.ActiveAwakeMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the active onenable method if it exists.
        /// </summary>
        private void ExecuteActiveOnEnable() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.ActiveOnEnableMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.ActiveOnEnableMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the active ondisable method if it exists.
        /// </summary>
        private void ExecuteActiveOnDisable() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.ActiveOnDisableMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.ActiveOnDisableMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the active start method if it exists.
        /// </summary>
        private void ExecuteActiveStart() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.ActiveStartMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.ActiveStartMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the active update method if it exists.
        /// </summary>
        private void ExecuteActiveUpdate() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.ActiveUpdateMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.ActiveUpdateMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the active fixed update method if it exists.
        /// </summary>
        private void ExecuteActiveFixedUpdate() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.ActiveFixedUpdateMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.ActiveFixedUpdateMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the active late update method if it exists.
        /// </summary>
        private void ExecuteActiveLateUpdate() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.ActiveLateUpdateMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.ActiveLateUpdateMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the passive awake method if it exists.
        /// </summary>
        private void ExecutePassiveAwake() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.PassiveAwakeMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.PassiveAwakeMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the passive onenable method if it exists.
        /// </summary>
        private void ExecutePassiveOnEnable() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.PassiveOnEnableMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.PassiveOnEnableMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the passive ondisable method if it exists.
        /// </summary>
        private void ExecutePassiveOnDisable() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.PassiveOnDisableMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.PassiveOnDisableMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the passive start method if it exists.
        /// </summary>
        private void ExecutePassiveStart() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.PassiveStartMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.PassiveStartMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the passive fixed update method if it exists.
        /// </summary>
        private void ExecutePassiveFixedUpdate() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.PassiveFixedUpdateMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.PassiveFixedUpdateMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the passive late update method if it exists.
        /// </summary>
        private void ExecutePassiveLateUpdate() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.PassiveLateUpdateMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.PassiveLateUpdateMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the passive update method if it exists.
        /// </summary>
        private void ExecutePassiveUpdate() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.PassiveUpdateMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.PassiveUpdateMethod].Invoke(this, null);
            }
        }

        /// <summary>
        /// Called when the network object is awakened. Executes different initialization
        /// methods based on the current behavior mode (Active or Passive).
        /// </summary>
        public override void InternalNetworkAwake() {
            if (BehaviorMode.Active == this.GetBehaviorMode()) {
                this.ExecuteActiveAwake();
            } else if (BehaviorMode.Passive == this.GetBehaviorMode()) {
                this.ExecutePassiveAwake();
            }
        }

        /// <summary>
        /// Internal OnEnable method called when the network object is enabled network operations.
        /// This is similar to a OnEnable method but specific to network initialization.
        /// </summary>
        public override void InternalNetworkOnEnable() {
            if (BehaviorMode.Active == this.GetBehaviorMode()) {
                this.ExecuteActiveOnEnable();
            } else if (BehaviorMode.Passive == this.GetBehaviorMode()) {
                this.ExecutePassiveOnEnable();
            }
        }

        /// <summary>
        /// Internal OnDisable method called when the network object is disabled network operations.
        /// This is similar to a OnDisable method but specific to network initialization.
        /// </summary>
        public override void InternalNetworkOnDisable() {
            if (BehaviorMode.Active == this.GetBehaviorMode()) {
                this.ExecuteActiveOnDisable();
            } else if (BehaviorMode.Passive == this.GetBehaviorMode()) {
                this.ExecutePassiveOnDisable();
            }
        }

        /// <summary>
        /// Called when the network object is started. Executes different start methods
        /// based on the current behavior mode (Active or Passive).
        /// </summary>
        public override void InternalNetworkStart() {
            if (BehaviorMode.Active == this.GetBehaviorMode()) {
                this.ExecuteActiveStart();
            } else if (BehaviorMode.Passive == this.GetBehaviorMode()) {
                this.ExecutePassiveStart();
            }
        }

        /// <summary>
        /// Called every frame to update the network object. Executes different update
        /// methods based on the current behavior mode (Active or Passive).
        /// </summary>
        public override void InternalNetworkUpdate() {
            if (BehaviorMode.Active == this.GetBehaviorMode()) {
                this.ExecuteActiveUpdate();
            } else if (BehaviorMode.Passive == this.GetBehaviorMode()) {
                this.ExecutePassiveUpdate();
            }
        }

        /// <summary>
        /// Called after all Update functions have been called. Executes different late
        /// update methods based on the current behavior mode (Active or Passive).
        /// </summary>
        public override void InternalNetworkLateUpdate() { 
            if (BehaviorMode.Active == this.GetBehaviorMode()) {
                this.ExecuteActiveLateUpdate();
            } else if (BehaviorMode.Passive == this.GetBehaviorMode()) {
                this.ExecutePassiveLateUpdate();
            }
        }

        /// <summary>
        /// Called every fixed framerate frame. Executes different fixed update methods
        /// based on the current behavior mode (Active or Passive).
        /// </summary>
        public override void InternalNetworkFixedUpdate() {
            if (BehaviorMode.Active == this.GetBehaviorMode()) {
                this.ExecuteActiveFixedUpdate();
            } else if (BehaviorMode.Passive == this.GetBehaviorMode()) {
                this.ExecutePassiveFixedUpdate();
            }
        }

        /// <summary>
        /// Initializes a synchronized field of the specified type.
        /// </summary>
        /// <param name="field">The field information to be synchronized.</param>
        /// <param name="deliveryMode">The delivery modde of network variable.</param>
        /// <typeparam name="T">The type of the field to be synchronized.</typeparam>
        private void InitializeSynchonizedField<T>(in FieldInfo field, DeliveryMode deliveryMode, ReliableDetectionMode detectionMode) {
            NetworkVariable<T> pairedSync = new NetworkVariable<T>(default(T), this);
            pairedSync.SetOrigin(NetworkVariableOrigin.Primitive); // This cames from a primitive variable registered on the PrefabsDatabaseWindow
            pairedSync.SetDeliveryMode(deliveryMode);
            pairedSync.SetDetectionMode(detectionMode);
            pairedSync.SetSynchonizedField(field, this);
            // Register variable to be Synchronized
            this.RegisterSynchronizedVariable(field.Name, pairedSync);
        }

        /// <summary>
        /// Registers a field to be synchronized if it is not already registered.
        /// </summary>
        /// <param name="field">The name of the field to register.</param>
        public void RegisterSynchonizedField(String field, DeliveryMode deliveryMode, ReliableDetectionMode detectionMode) {
            if (!this.SynchronizedFields.ContainsKey(field)) {
                this.SynchronizedFields.Add(field, new Tuple<DeliveryMode, ReliableDetectionMode>(deliveryMode, detectionMode));
            }
        }

        /// <summary>
        /// Return the delivery mode of network variable
        /// </summary>
        /// <param name="field">Field to check</param>
        /// <returns>The delivery mode of requqsted field</returns>
        public override DeliveryMode GetSynchonizedFieldDeliveryMode(String field) {
            if (this.SynchronizedFields.ContainsKey(field)) {
                return this.SynchronizedFields[field].Item1;
            } else {
                return DeliveryMode.Reliable;
            }
        }

        /// <summary>
        /// Return the detection mode of network variable
        /// </summary>
        /// <param name="field">Field to check</param>
        /// <returns>The detection mode of requqsted field</returns>
        public override ReliableDetectionMode GetSynchonizedFieldDetectionMode(String field) {
            if (this.SynchronizedFields.ContainsKey(field)) {
                return this.SynchronizedFields[field].Item2;
            } else {
                return ReliableDetectionMode.Automatic;
            }
        }

        /// <summary>
        /// Initializes all synchronized fields based on their types.
        /// </summary>
        public override void InitializeSynchonizedFields() {
            foreach (var synchonizedField in this.SynchronizedFields) {
                FieldInfo field = this.GetType().GetField(synchonizedField.Key, BindingFlags.Public    |
                                                                                BindingFlags.NonPublic |
                                                                                BindingFlags.Instance  |
                                                                                BindingFlags.DeclaredOnly);
                // Get from bae class
                if (field == null) {
                    Type baseClass = this.GetType().BaseType;
                    while ((baseClass != null) && (baseClass.Equals(typeof(NetworkBehaviour))) == false) {
                        field = baseClass.GetField(synchonizedField.Key, BindingFlags.Public       |
                                                                         BindingFlags.NonPublic    |
                                                                         BindingFlags.Instance     |
                                                                         BindingFlags.DeclaredOnly);
                        if (field == null) {
                            baseClass = baseClass.BaseType;
                        } else {
                            break;
                        }
                    }
                }

                if ( field != null ) {
                    Type fieldType = field.FieldType.IsEnum ? Enum.GetUnderlyingType(field.FieldType) : field.FieldType;
                    if (fieldType == typeof(int) )
                        this.InitializeSynchonizedField<int>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if (fieldType == typeof(uint) )
                        this.InitializeSynchonizedField<uint>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if (fieldType == typeof(long) )
                        this.InitializeSynchonizedField<long>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if ( fieldType == typeof(ulong) )
                        this.InitializeSynchonizedField<ulong>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if ( fieldType == typeof(short) )
                        this.InitializeSynchonizedField<short>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if ( fieldType == typeof(ushort) )
                        this.InitializeSynchonizedField<ushort>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if ( fieldType == typeof(float) )
                        this.InitializeSynchonizedField<float>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if ( fieldType == typeof(double) )
                        this.InitializeSynchonizedField<double>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if ( fieldType == typeof(byte) )
                        this.InitializeSynchonizedField<byte>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if (fieldType == typeof(sbyte))
                        this.InitializeSynchonizedField<sbyte>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if ( fieldType == typeof(byte[]) )
                        this.InitializeSynchonizedField<byte[]>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if ( fieldType == typeof(string) )
                        this.InitializeSynchonizedField<string>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if ( fieldType == typeof(char) )
                        this.InitializeSynchonizedField<char>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if ( fieldType == typeof(char[]) )
                        this.InitializeSynchonizedField<char[]>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if (fieldType == typeof(Vector2))
                        this.InitializeSynchonizedField<Vector2>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if ( fieldType == typeof(Vector3) )
                        this.InitializeSynchonizedField<Vector3>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if (fieldType == typeof(Vector4))
                        this.InitializeSynchonizedField<Vector4>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if ( fieldType == typeof(Color) )
                        this.InitializeSynchonizedField<Color>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if (fieldType == typeof(Quaternion))
                        this.InitializeSynchonizedField<Quaternion>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if (fieldType == typeof(Matrix4x4))
                        this.InitializeSynchonizedField<Matrix4x4>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);
                    else if ( fieldType == typeof(bool) )
                        this.InitializeSynchonizedField<bool>(field, synchonizedField.Value.Item1, synchonizedField.Value.Item2);

                }
            }
        }
    }
}