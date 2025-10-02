namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the internal network behavior for a networked object.
    /// This interface extends INetworkBehavior to include internal lifecycle methods and network processing.
    /// </summary>
    public interface INetworkBehaviorInternal : INetworkBehavior {

        /// <summary>
        /// Executes the network process associated with this behavior.
        /// This method is intended to handle any network operations that need to be processed.
        /// </summary>
        void ExecuteNetworkProcess();

        /// <summary>
        /// Internal initialization method called when the network object is being set up.
        /// This method should prepare the object for network activity.
        /// </summary>
        void InternalNetworkAwake();

        /// <summary>
        /// Internal start method called when the network object is enabled network operations.
        /// This is similar to a OnEnable method but specific to network initialization.
        /// </summary>
        void InternalNetworkOnEnable();

        /// <summary>
        /// Internal start method called when the network object is disabled network operations.
        /// This is similar to a OnDisable method but specific to network initialization.
        /// </summary>
        void InternalNetworkOnDisable();

        /// <summary>
        /// Internal start method called when the network object is ready to start network operations.
        /// This is similar to a Start method but specific to network initialization.
        /// </summary>
        void InternalNetworkStart();

        /// <summary>
        /// Internal update method called every frame to handle network updates.
        /// This method is intended for regular updates such as checking network state or messages.
        /// </summary>
        void InternalNetworkUpdate();

        /// <summary>
        /// Internal late update method called after all InternalNetworkUpdate calls.
        /// This method is used for operations that need to be executed after the regular network updates.
        /// </summary>
        void InternalNetworkLateUpdate();

        /// <summary>
        /// Internal fixed update method called at fixed intervals to handle network physics updates.
        /// This method is intended for network operations that need to be synced with the physics engine.
        /// </summary>
        void InternalNetworkFixedUpdate();

        /// <summary>
        /// Sends data over the network to connected peers.
        /// </summary>
        /// <param name="eventCode">The event code that identifies the type of data being sent.</param>
        /// <param name="writer">The data stream containing the data to be sent.</param>
        /// <param name="mode">The delivery mode that specifies how the data should be sent. Defaults to Unreliable.</param>
        void Send(int eventCode, DataStream writer, DeliveryMode mode = DeliveryMode.Unreliable);

        /// <summary>
        /// Return the order that bvehaviour will be executed
        /// </summary>
        /// <returns>Network start events order</returns>
        NetworkStartOrder GetStartOrder();
    }

}
