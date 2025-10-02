using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a network element that can process local and remote network inputs and behaviors.
    /// </summary>
    public class NetworkElement : NetworkElementBase {

        // Indicates whether local device input should be computed.
        private bool computeLocalDevice = false;

        // Indicates whether remote device input should be computed.
        private bool computeRemoteDevice = false;

        // A list of behaviors associated with the device.
        private List<IEntity> deviceBehaviors = new List<IEntity>();

        /// <summary>
        /// Initializes a new instance of the NetworkElement class with a specified container and network ID.
        /// </summary>
        /// <param name="container">The network container associated with this element.</param>
        /// <param name="networkId">The network identifier for this element.</param>
        public NetworkElement(INetworkControl networkObject, INetworkContainer container, int networkId = 0) : base(networkObject, container, networkId) {
        }

        /// <summary>
        /// Initializes a new instance of the NetworkElement class with a specified container, behavior mode, and network ID.
        /// </summary>
        /// <param name="container">The network container associated with this element.</param>
        /// <param name="mode">The behavior mode for this element.</param>
        /// <param name="networkId">The network identifier for this element.</param>
        public NetworkElement(INetworkControl networkObject, INetworkContainer container, BehaviorMode mode = BehaviorMode.Both, int networkId = 0) : base(networkObject, container, mode, networkId) {
        }

        /// <summary>
        /// Initializes a new instance of the NetworkElement class with a GameObject.
        /// </summary>
        /// <param name="container">The network container associated with this element.</param>
        /// <param name="linkedObject">The GameObject linked to this network element.</param>
        /// <param name="networkId">The network identifier for this element (default is 0).</param>
        public NetworkElement(INetworkControl networkObject, INetworkContainer container, GameObject linkedObject, int networkId = 0) : base(networkObject, container, linkedObject, networkId) {
        }

        /// <summary>
        /// Initializes a new instance of the NetworkElement class with a GameObject and specified behavior mode.
        /// </summary>
        /// <param name="container">The network container associated with this element.</param>
        /// <param name="linkedObject">The GameObject linked to this network element.</param>
        /// <param name="mode">The behavior mode for this network element (default is Both).</param>
        /// <param name="networkId">The network identifier for this element (default is 0).</param>
        public NetworkElement(INetworkControl networkObject, INetworkContainer container, GameObject linkedObject, BehaviorMode mode = BehaviorMode.Both, int networkId = 0) : base(networkObject, container, linkedObject, mode, networkId) {
        }

        /// <summary>
        /// Initializes a new instance of the NetworkElement class with a MonoBehaviour.
        /// </summary>
        /// <param name="container">The network container associated with this element.</param>
        /// <param name="linkedObject">The MonoBehaviour linked to this network element.</param>
        /// <param name="networkId">The network identifier for this element (default is 0).</param>
        public NetworkElement(INetworkControl networkObject, INetworkContainer container, MonoBehaviour linkedObject, int networkId = 0) : base(networkObject, container, linkedObject, networkId) {
        }

        /// <summary>
        /// Initializes a new instance of the NetworkElement class with a MonoBehaviour and specified behavior mode.
        /// </summary>
        /// <param name="container">The network container associated with this element.</param>
        /// <param name="linkedObject">The MonoBehaviour linked to this network element.</param>
        /// <param name="mode">The behavior mode for this network element (default is Both).</param>
        /// <param name="networkId">The network identifier for this element (default is 0).</param>
        public NetworkElement(INetworkControl networkObject, INetworkContainer container, MonoBehaviour linkedObject, BehaviorMode mode = BehaviorMode.Both, int networkId = 0) : base(networkObject, container, linkedObject, mode, networkId) {
        }

        /// <summary>
        /// Processes the network element by first calling the base process method, then processing local and remote inputs.
        /// </summary>
        public override void Process() {
            base.Process();
            // Now i'm going to process all extra network behaviors
            this.NetworkLocalInputProcess();
            this.NetworkRemoteInputProcess();
        }

        /// <summary>
        /// Process network inputs locally, the ide is ( even if multiplayer is not enabled ) compute input
        /// doing this, developers dont need to process input into a differently way if player is running out of multiplayer system
        /// </summary>
        private void NetworkLocalInputProcess() {
            if (this.IsToComputeLocalDevice()) {
                if (this.HasDeviceBehavior<InputNetwork>()) {
                    InputNetwork inputNetwork = this.GetDeviceBehavior<InputNetwork>();
                    if (inputNetwork.GetInput().IsLocal()) {
                        this.GetDeviceBehavior<InputNetwork>().ComputeActive();
                    }
                }
            }
        }

        /// <summary>
        /// Processes remote network inputs, synchronizes behaviors, and sends updates over the network.
        /// </summary>
        private void NetworkRemoteInputProcess() {
            if (this.IsToComputeRemoteDevice()) {
                // Consume any pending packet received on network
                if (this.networkPackets.Count > 0) {
                    if (this.IsActive()) {
                        IDataStream packet = null;
                        if (this.networkPackets.TryDequeue(out packet)) {
                            foreach (IEntity behavior in this.deviceBehaviors) {
                                if (behavior.IsActive()) {
                                    behavior.Consume(packet);
                                }
                            }
                        }
                    }
                }
                // Synchonize all behaviors 
                // Passive element must be synchonized before compute, to ensure then element will be send after synchonize on local
                if (this.IsPassive()) {
                    // Reset all data buffer
                    this.GetWritterStream().Reset();
                    // Write network event ( ObjectUpdate if the basic update send into Object )
                    this.GetWritterStream().Write(CoreGameEvents.ObjectInputUpdate);
                    // Fill Object ID before send packet to network
                    this.GetWritterStream().Write(this.GetNetworkId());
                    // Now write each behavior separatedly
                    foreach (IEntity behavior in this.deviceBehaviors) {
                        if (behavior.IsActive()) {
                            behavior.Synchronize();
                        }
                    }
                    // Send packet
                    // Note: Here i haven't method to send ebvent code, so i need to mout packet
                    this.GetTransport().Send(this.GetWritterStream().GetBuffer(), this.GetDeliveryMode());
                }
                // Compute all behaviors
                foreach (IEntity behavior in this.deviceBehaviors) {
                    if (behavior.IsActive()) {
                        behavior.Compute();
                    }
                }
                // Synchonize all behaviors 
                // Passive element must be synchonized after compute, to ensure then element will be updated before synchonize on network
                if (this.IsActive()) {
                    foreach (IEntity behavior in this.deviceBehaviors) {
                        if (behavior.IsActive()) {
                            behavior.Synchronize();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines if local device computation is enabled.
        /// </summary>
        /// <returns>True if local device computation is to be performed, otherwise false.</returns>
        private bool IsToComputeLocalDevice() {
            return this.computeLocalDevice;
        }

        /// <summary>
        /// Sets the flag to determine whether to compute on the local device.
        /// </summary>
        /// <param name="value">True to enable local device computation, false to disable it.</param>
        public override void SetToComputeLocalDevice(bool value) {
            this.computeLocalDevice = value;
        }

        /// <summary>
        /// Determines if remote device computation is enabled.
        /// </summary>
        /// <returns>True if remote device computation is to be performed, otherwise false.</returns>
        private bool IsToComputeRemoteDevice() {
            return this.computeRemoteDevice;
        }

        /// <summary>
        /// Sets the flag to determine whether to compute on the remote device.
        /// </summary>
        /// <param name="value">True to enable remote device computation, false to disable it.</param>
        public override void SetToComputeRemoteDevice(bool value) {
            this.computeRemoteDevice = value;
        }

        /// <summary>
        /// Registers a device behavior to the network entity.
        /// </summary>
        /// <typeparam name="T">The type of the network entity.</typeparam>
        /// <typeparam name="E">The event type associated with the network entity.</typeparam>
        /// <param name="behavior">The behavior to register.</param>
        public override void RegisterDeviceBehavior<T, E>(INetworkEntity<T, E> behavior) {
            if (!this.deviceBehaviors.Contains(behavior)) {
                this.deviceBehaviors.Add(behavior);
                behavior.SetNetworkObject(this);
            }
        }

        /// <summary>
        /// Unregisters a device behavior of a specific type from the network entity.
        /// </summary>
        /// <typeparam name="T">The type of the behavior to unregister.</typeparam>
        public override void UnregisterDeviceBehavior<T>() {
            T unregisterComponent = default(T);
            bool found = false;
            foreach (T behavior in this.deviceBehaviors) {
                if (behavior.GetType().Equals(typeof(T))) {
                    unregisterComponent = behavior;
                    found = true;
                    break;
                }
            }
            if (found) {
                this.deviceBehaviors.Remove(unregisterComponent);
            }
        }

        /// <summary>
        /// Checks if a device behavior of a specific type is registered.
        /// </summary>
        /// <typeparam name="T">The type of the behavior to check for.</typeparam>
        /// <returns>True if the behavior is registered, otherwise false.</returns>
        public override bool HasDeviceBehavior<T>() {
            bool result = false;
            foreach (IEntity behavior in this.deviceBehaviors) {
                if (behavior.GetType().Equals(typeof(T))) {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves a device behavior of a specific type if it is registered.
        /// </summary>
        /// <typeparam name="T">The type of the behavior to retrieve.</typeparam>
        /// <returns>The behavior of the specified type if found, otherwise the default value for the type.</returns>
        public override T GetDeviceBehavior<T>() {
            T result = default(T);
            foreach (IEntity behavior in this.deviceBehaviors) {
                if (behavior.GetType().Equals(typeof(T))) {
                    result = (T)behavior;
                    break;
                }
            }
            return result;
        }

    }
}