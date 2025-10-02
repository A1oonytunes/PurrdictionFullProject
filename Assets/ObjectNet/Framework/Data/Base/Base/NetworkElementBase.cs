using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Abstract base class for network elements that implements the INetworkElement interface.
    /// </summary>
    public abstract class NetworkElementBase : INetworkElement {

        // Unique identifier for the network element.
        private int networkId = 0;

        // Session identifier for the network connection.
        private string networkSessionId = null;

        // Network object associated with this element
        private INetworkControl networkObject;

        // Flag indicating whether this element represents a network player.
        private bool isNetworkPlayer = false;

        // Player identifier for networked games.
        private ushort playerId = 0;

        // Player index for networked games.
        private ushort playerIndex = 0;

        // Flag indicating whether this element is the owner of the network session.
        private bool owner = false;

        // Rate at which active network updates are sent.
        private int activeRate = SEND_RATE_AMOUNT;

        // Rate at which passive network updates are received.
        private int passiveRate = 0;

        // Enable to send at leat on message into the interval of time
        private bool enableMinimumRate = false;

        // Minimun interval of sending messages when minimum rate is enabled
        private int minimunRate = MINIMUM_SEND_RATE_AMOUNT;

        // Minimum rrequency of active network updates
        private float minimunFrequencyRate = (1.0f / MINIMUM_SEND_RATE_AMOUNT);

        // Time until the next network update execution according minimum rate.
        private float nextMinimumSendingRate = 0f;

        // Time until the next network update execution.
        private float nextExecution = 0f;

        private bool updateStarted = true;
        
        private float startupTime  = 0f;

        // The delivery mode for network messages (e.g., Reliable, Unreliable).
        private DeliveryMode deliveryMode = DeliveryMode.Unreliable;

        // Ownership access level
        private OwnerShipAccessLevel ownershipAccessLevel = OwnerShipAccessLevel.Full;

        // The channel used for network transport.
        private IChannel transport;

        // The container that holds this network element.
        private INetworkContainer container;

        // The data stream used for network communication.
        private IDataStream dataStream;

        // The behavior mode of the network element (e.g., Active, Passive, Both).
        private BehaviorMode behaviorMode = BehaviorMode.Active;

        // List of behaviors (entities) associated with this network element.
        private List<IEntity> behaviors = new List<IEntity>();

        // The GameObject associated with this network element.
        private GameObject gameObject;

        // Delegate method for sending network data.
        private Action<int, DataStream, DeliveryMode, IClient> sendMethod;

        // Dictionary of network events keyed by their identifiers.
        private Dictionary<int, INetworkEvent> events = new Dictionary<int, INetworkEvent>();

        // Thread-safe queue for outgoing network packets.
        protected ConcurrentQueue<IDataStream> networkPackets = new ConcurrentQueue<IDataStream>();

        // Thread-safe queue for incoming network packets.
        protected ConcurrentQueue<IDataStream> inputPackets = new ConcurrentQueue<IDataStream>();

        // Constant defining the size of the network buffer.
        const int NETWORK_BUFFER_SIZE = 10;

        // Constant defining the default send rate amount.
        const int SEND_RATE_AMOUNT = 30;


        // Time to send process events without frame interval
        const float DEFAULT_STARTUP_TIME = 2.0f;

        // Minimun size that message can have.
        const int PAYLOAD_PACKAGE_SIZE = 8;

        // Minimun send rate amount.
        const int MINIMUM_SEND_RATE_AMOUNT = 4; 

        /// <summary>
        /// Constructor for creating a network element with a specified container, behavior mode, and network ID.
        /// </summary>
        /// <param name="container">The container that will hold this network element.</param>
        /// <param name="mode">The behavior mode for this network element.</param>
        /// <param name="networkId">The unique identifier for this network element.</param>
        public NetworkElementBase(INetworkControl networkObject, INetworkContainer container, BehaviorMode mode = BehaviorMode.Both, int networkId = 0) {
            this.networkObject  = networkObject;
            this.container      = container;
            this.networkId      = networkId;
            this.behaviorMode   = mode;
            this.container.Register(this);
        }

        /// <summary>
        /// Constructor for creating a network element with a specified container and network ID.
        /// </summary>
        /// <param name="container">The container that will hold this network element.</param>
        /// <param name="networkId">The unique identifier for this network element.</param>
        public NetworkElementBase(INetworkControl networkObject, INetworkContainer container, int networkId = 0) {
            this.networkObject  = networkObject;
            this.container      = container;
            this.networkId      = networkId;
            this.container.Register(this);
        }

        /// <summary>
        /// Constructor for creating a network element with a specified container, linked GameObject, behavior mode, and network ID.
        /// </summary>
        /// <param name="container">The container that will hold this network element.</param>
        /// <param name="linkedObject">The GameObject linked to this network element.</param>
        /// <param name="mode">The behavior mode for this network element.</param>
        /// <param name="networkId">The unique identifier for this network element.</param>
        public NetworkElementBase(INetworkControl networkObject, INetworkContainer container, GameObject linkedObject, BehaviorMode mode = BehaviorMode.Both, int networkId = 0) {
            this.networkObject  = networkObject;
            this.container      = container;
            this.networkId      = networkId;
            this.gameObject     = linkedObject;
            this.behaviorMode   = mode;
            this.container.Register(this);
        }

        /// <summary>
        /// Constructor for creating a network element with a specified container, linked GameObject, and network ID.
        /// </summary>
        /// <param name="container">The container that will hold this network element.</param>
        /// <param name="linkedObject">The GameObject linked to this network element.</param>
        /// <param name="networkId">The unique identifier for this network element.</param>
        public NetworkElementBase(INetworkControl networkObject, INetworkContainer container, GameObject linkedObject, int networkId = 0) {
            this.networkObject  = networkObject;
            this.container      = container;
            this.networkId      = networkId;
            this.gameObject     = linkedObject;
            this.container.Register(this);
        }

        /// <summary>
        /// Constructor for creating a network element with a specified container, linked MonoBehaviour, behavior mode, and network ID.
        /// </summary>
        /// <param name="container">The container that will hold this network element.</param>
        /// <param name="linkedObject">The MonoBehaviour linked to this network element.</param>
        /// <param name="mode">The behavior mode for this network element.</param>
        /// <param name="networkId">The unique identifier for this network element.</param>
        public NetworkElementBase(INetworkControl networkObject, INetworkContainer container, MonoBehaviour linkedObject, BehaviorMode mode = BehaviorMode.Both, int networkId = 0) {
            this.networkObject  = networkObject;
            this.container      = container;
            this.networkId      = networkId;
            this.gameObject     = linkedObject.gameObject;
            this.behaviorMode   = mode;
            this.container.Register(this);
        }

        /// <summary>
        /// Constructor for creating a network element with a specified container, linked MonoBehaviour, and network ID.
        /// </summary>
        /// <param name="container">The container that will hold this network element.</param>
        /// <param name="linkedObject">The MonoBehaviour linked to this network element.</param>
        /// <param name="networkId">The unique identifier for this network element.</param>
        public NetworkElementBase(INetworkControl networkObject, INetworkContainer container, MonoBehaviour linkedObject, int networkId = 0) {
            this.networkObject  = networkObject;
            this.container      = container;
            this.networkId      = networkId;
            this.gameObject     = linkedObject.gameObject;
            this.container.Register(this);
        }

        /// <summary>
        /// Registers a device behavior with the network entity.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="E">The type of the data stream.</typeparam>
        /// <param name="behavior">The network entity behavior to register.</param>
        public abstract void RegisterDeviceBehavior<T, E>(INetworkEntity<T, E> behavior) where E : IDataStream;

        /// <summary>
        /// Unregisters a device behavior of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the entity to unregister.</typeparam>
        public abstract void UnregisterDeviceBehavior<T>() where T : IEntity;

        /// <summary>
        /// Checks if a device behavior of the specified type is registered.
        /// </summary>
        /// <typeparam name="T">The type of the entity to check.</typeparam>
        /// <returns>True if the behavior is registered; otherwise, false.</returns>
        public abstract bool HasDeviceBehavior<T>() where T : IEntity;

        /// <summary>
        /// Retrieves the device behavior of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The device behavior of the specified type.</returns>
        public abstract T GetDeviceBehavior<T>() where T : IEntity;

        /// <summary>
        /// Sets the device to compute locally.
        /// </summary>
        /// <param name="value">True to enable local computation; false otherwise.</param>
        public abstract void SetToComputeLocalDevice(bool value);

        /// <summary>
        /// Sets the device to compute remotely.
        /// </summary>
        /// <param name="value">True to enable remote computation; false otherwise.</param>
        public abstract void SetToComputeRemoteDevice(bool value);

        /// <summary>
        /// Configures the method used for sending data.
        /// </summary>
        /// <param name="method">The action to invoke when sending data.</param>
        public void ConfigureSendMethod(Action<int, DataStream, DeliveryMode, IClient> method) {
            this.sendMethod = method;
        }

        /// <summary>
        /// Sends data using the configured send method.
        /// </summary>
        /// <param name="eventCode">The event code associated with the data.</param>
        /// <param name="writer">The data stream to send.</param>
        /// <param name="mode">The delivery mode for the data.</param>
        public void Send(int eventCode, DataStream writer, DeliveryMode mode = DeliveryMode.Unreliable, IClient client = null) {
            this.sendMethod.Invoke(eventCode, writer, mode, client);
        }

        /// <summary>
        /// Processes network events and behaviors based on the network clock.
        /// </summary>
        public virtual void Process() {
            if ( !this.updateStarted ) {
                this.updateStarted = true;
                this.startupTime = (NetworkClock.time + DEFAULT_STARTUP_TIME);
            }

            // Active   : Needs a rate to send
            // Passive  : Must be as fast as it can to consume everything
            if (this.nextExecution < NetworkClock.time) {
                if (this.IsPassive()) {
                    if (this.startupTime < NetworkClock.time) {
                        this.nextExecution = (NetworkClock.time + this.GetPassiveFrequencyRate());
                    }
                } else if (this.IsActive()) {
                    if (this.startupTime < NetworkClock.time) {
                        this.nextExecution = (NetworkClock.time + this.GetActiveFrequencyRate());
                    }
                }
                this.InternalNetworkProcess();
            }
        }

        /// <summary>
        /// Internal processing of network events and behaviors.
        /// </summary>
        private void InternalNetworkProcess() {
            // Consume any pending packet received on the network
            if (this.networkPackets.Count > 0) {
                if (this.IsPassive()) {
                    IDataStream packet = null;
                    if (this.networkPackets.TryDequeue(out packet)) {
                        foreach (IEntity behavior in this.behaviors) {
                            if (behavior.IsActive()) {
                                behavior.Consume(packet);
                            }
                        }
                    }
                }
            }
            // Synchronize all behaviors
            // Passive elements must be synchronized before computing to ensure that elements will be sent after synchronization on local
            if (this.IsActive()) {
                if (this.GetTransport().IsTransportInitialized()) {
                    // Reset all data buffers
                    this.GetWritterStream().Reset();
                    // Write network event (ObjectUpdate is the basic update sent into Object)
                    this.GetWritterStream().Write(CoreGameEvents.ObjectUpdate);
                    // Fill Object ID before sending packet to network
                    this.GetWritterStream().Write(this.GetNetworkId());
                    // Now write each behavior separately
                    // Enable minumun and has any registered behaviour
                    bool wasUpdated = ((this.enableMinimumRate) && (this.nextMinimumSendingRate < NetworkClock.time) && (this.behaviors.Count > 0));
                    foreach (IEntity behavior in this.behaviors) {
                        if (behavior.IsActive()) {
                            behavior.Synchronize();
                            wasUpdated |= behavior.WasUpdated(); // Flag if something changed
                            behavior.Invalidate(); // Invalidate to test again in the next cycle
                        }
                    }
                    // Send packet only if something changed on any behavior and if has any registered behaviour
                    // The "PAYLOAD_PACKAGE_SIZE" protection is to ensure in case of someone create a custom behaviour without any data, on this case
                    // the package size will has only the "Event ID" and "Network Id"
                    if ((wasUpdated) && (this.GetWritterStream().GetBufferSize() > PAYLOAD_PACKAGE_SIZE)) {
                        this.nextMinimumSendingRate = (NetworkClock.time + this.minimunFrequencyRate);
                        this.GetTransport().Send(this.GetWritterStream().GetBuffer(), this.GetDeliveryMode());
                    }
                }
            }
            // Compute all behaviors
            foreach (IEntity behavior in this.behaviors) {
                if (behavior.IsActive()) {
                    behavior.Compute();
                }
            }
            // Synchronize all behaviors
            // Passive elements must be synchronized after computing to ensure that elements will be updated before synchronization on network
            if (this.IsPassive()) {
                foreach (IEntity behavior in this.behaviors) {
                    if (behavior.IsActive()) {
                        behavior.Synchronize();
                    }
                }
            }
        }


        /// <summary>
        /// Checks if an event with the specified event code is already registered.
        /// </summary>
        /// <param name="eventCode">The event code to check for existence.</param>
        /// <returns>True if the event is registered; otherwise, false.</returns>
        public bool HasEvent(int eventCode) {
            return this.events.ContainsKey(eventCode);
        }

        /// <summary>
        /// Registers a new event with a callback action if it is not already registered.
        /// </summary>
        /// <param name="eventCode">The unique code representing the event.</param>
        /// <param name="callBack">The callback action to be invoked when the event occurs.</param>
        /// <exception cref="Exception">Thrown when the event is already registered.</exception>
        public void RegisterEvent(int eventCode, Action<IDataStream> callBack) {
            if (!this.events.ContainsKey(eventCode)) {
                this.events.Add(eventCode, new NetworkEvent(eventCode, callBack));
            } else {
                throw new Exception(String.Format("Event \"{0}\" is already registered", eventCode.ToString()));
            }
        }

        /// <summary>
        /// Unregisters an event associated with the specified event code.
        /// </summary>
        /// <param name="eventCode">The event code of the event to unregister.</param>
        public void UnregisterEvent(int eventCode) {
            if (this.events.ContainsKey(eventCode)) {
                this.events.Remove(eventCode);
            }
        }

        /// <summary>
        /// Invokes the callback associated with the specified event code.
        /// </summary>
        /// <param name="eventCode">The event code of the event to invoke.</param>
        /// <param name="reader">The data stream to pass to the event's callback.</param>
        /// <exception cref="Exception">Thrown when the event is not registered.</exception>
        public void InvokeEvent(int eventCode, IDataStream reader) {
            if (this.events.ContainsKey(eventCode)) {
                this.events[eventCode].ExecuteEvent(reader);
            } else {
                if (this.gameObject != null) {
                    throw new Exception(String.Format("Event \"{0}\" is not registered for [ {1} : {2} ]", eventCode.ToString(), this.networkId, this.gameObject.name));
                } else {
                    throw new Exception(String.Format("Event \"{0}\" is not registered", eventCode.ToString()));
                }
            }
        }

        /// <summary>
        /// Gets the network ID.
        /// </summary>
        /// <returns>The network ID.</returns>
        public int GetNetworkId() {
            return this.networkId;
        }

        /// <summary>
        /// Sets the network ID.
        /// </summary>
        /// <param name="id">The new network ID to set.</param>
        /// <exception cref="System.Exception">Thrown when the network ID is already set to a different value.</exception>
        public void SetNetworkId(int id) {
            if ((this.networkId > 0) && (this.networkId != id)) {
                throw new System.Exception("Network object id is already defined");
            }
            this.networkId = id;
        }

        /// <summary>
        /// Gets the network session ID.
        /// </summary>
        /// <returns>The network session ID.</returns>
        public string GetNetworkSessionId() {
            return this.networkSessionId;
        }

        /// <summary>
        /// Sets the network session ID.
        /// </summary>
        /// <param name="id">The new network session ID to set.</param>
        public void SetNetworkSessionId(string id) {
            this.networkSessionId = id;
        }

        /// <summary>
        /// Determines if the current instance is the owner.
        /// </summary>
        /// <returns>True if the current instance is the owner; otherwise, false.</returns>
        public bool IsOwner() {
            return (this.isNetworkPlayer) ? this.owner : this.IsActive();
        }

        /// <summary>
        /// Detects if the current instance is the owner based on the provided network ID.
        /// </summary>
        /// <param name="networkId">The network ID to compare with the current instance's network ID.</param>
        public void DetectOwner(int networkId) {
            this.owner = (this.networkId == networkId);
        }

        /// <summary>
        /// Sets the ownership status of the current instance.
        /// </summary>
        /// <param name="value">The ownership status to set.</param>
        public void SetOwner(bool value) {
            this.owner = value;
        }

        /// <summary>
        /// Gets the transport channel.
        /// </summary>
        /// <returns>The transport channel.</returns>
        public IChannel GetTransport() {
            return this.transport;
        }

        /// <summary>
        /// Sets the transport channel.
        /// </summary>
        /// <param name="transport">The transport channel to set.</param>
        public void SetTransport(IChannel transport) {
            this.transport = transport;
        }

        /// <summary>
        /// Gets the network container.
        /// </summary>
        /// <returns>The network container.</returns>
        public INetworkContainer GetContainer() {
            return this.container;
        }

        /// <summary>
        /// Registers a network packet to the internal packet queue.
        /// </summary>
        /// <param name="packet">The network packet to register.</param>
        /// <remarks>
        /// This method ensures that the queue does not exceed a predefined buffer size.
        /// If the queue is full, it dequeues packets to make room for new ones.
        /// </remarks>
        public void RegisterNetworkPacket(IDataStream packet) {
            lock (this.networkPackets) {
                this.networkPackets.Enqueue(packet);
                // To not increase buffer and generate delays
                while (this.networkPackets.Count > NETWORK_BUFFER_SIZE) {
                    IDataStream dequeuedPacket = null;
                    this.networkPackets.TryDequeue(out dequeuedPacket);
                }
            }
        }


        /// <summary>
        /// Registers an input packet into the network object's input queue.
        /// </summary>
        /// <param name="packet">The data stream packet to register.</param>
        public void RegisterInputPacket(IDataStream packet) {
            lock (this.inputPackets) {
                this.inputPackets.Enqueue(packet);
                // Ensure the input packet queue does not exceed the network buffer size
                while (this.inputPackets.Count > NETWORK_BUFFER_SIZE) {
                    IDataStream dequeuedPacket = null;
                    this.inputPackets.TryDequeue(out dequeuedPacket);
                }
            }
        }

        /// <summary>
        /// Define network object associated with Network Element
        /// </summary>
        /// <param name="networkObject">Network object</param>
        public void SetNetworkObject(INetworkControl networkObject) {
            this.networkObject = networkObject;
        }

        /// <summary>
        /// Return network onbject associated with Network Element
        /// </summary>
        /// <returns>Network object</returns>
        public INetworkControl GetNetworkObject() {
            return this.networkObject;
        }

        /// <summary>
        /// Return network onbject associated with Network Element
        /// </summary>
        /// <typeparam name="T">Generic type to be returned</typeparam>
        /// <returns>Network object</returns>
        public T GetNetworkObject<T>() where T : INetworkControl { 
            return (T)this.networkObject;
        }
        

        /// <summary>
        /// Retrieves the writer data stream associated with this network object.
        /// </summary>
        /// <returns>The writer data stream.</returns>
        public IDataStream GetWritterStream() {
            return this.dataStream;
        }

        /// <summary>
        /// Sets the writer data stream for this network object.
        /// </summary>
        /// <param name="dataStream">The data stream to set as the writer.</param>
        public void SetWritterStream(IDataStream dataStream) {
            this.dataStream = dataStream;
        }

        /// <summary>
        /// Gets the associated GameObject.
        /// </summary>
        /// <returns>The GameObject associated with this network object.</returns>
        public GameObject GetGameObject() {
            return this.gameObject;
        }

        /// <summary>
        /// Sets the associated GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to associate with this network object.</param>
        public void SetGameObject(GameObject gameObject) {
            this.gameObject = gameObject;
        }

        /// <summary>
        /// Sets the behavior mode of the network object.
        /// </summary>
        /// <param name="mode">The behavior mode to set.</param>
        public void SetMode(BehaviorMode mode) {
            this.behaviorMode = mode;
        }

        /// <summary>
        /// Gets the current behavior mode of the network object.
        /// </summary>
        /// <returns>The current behavior mode.</returns>
        public BehaviorMode GetMode() {
            return this.behaviorMode;
        }

        /// <summary>
        /// Sets the delivery mode for network operations.
        /// </summary>
        /// <param name="deliveryMode">The delivery mode to set.</param>
        public void SetDeliveryMode(DeliveryMode deliveryMode) {
            this.deliveryMode = deliveryMode;
        }

        /// <summary>
        /// Gets the current delivery mode for network operations.
        /// </summary>
        /// <returns>The current delivery mode.</returns>
        public DeliveryMode GetDeliveryMode() {
            return this.deliveryMode;
        }

        /// <summary>
        /// Set the current ownership access level of an object
        /// </summary>
        /// <param name="accessLevel">Level of ownership access</param>
        public void SetOwnershipAccessLevel(OwnerShipAccessLevel accessLevel) {
            this.ownershipAccessLevel = accessLevel;
        }

        /// <summary>
        /// Gets the current ownership access level of an object
        /// </summary>
        /// <returns>The current current ownership access level.</returns>
        public OwnerShipAccessLevel GetOwnershipAccessLevel() {
            return this.ownershipAccessLevel;
        }

        /// <summary>
        /// Determines if the network object is in an active state.
        /// </summary>
        /// <returns>True if the object is active, false otherwise.</returns>
        public bool IsActive() {
            return (BehaviorMode.Active == this.behaviorMode) || (BehaviorMode.Both == this.behaviorMode);
        }

        /// <summary>
        /// Determines if the network object is in a passive state.
        /// </summary>
        /// <returns>True if the object is passive, false otherwise.</returns>
        public bool IsPassive() {
            return (BehaviorMode.Passive == this.behaviorMode) || (BehaviorMode.Both == this.behaviorMode);
        }

        /// <summary>
        /// Sets the network player status of the object.
        /// </summary>
        /// <param name="value">True if the object represents a network player, false otherwise.</param>
        public void SetIsPlayer(bool value) {
            this.isNetworkPlayer = value;
        }

        /// <summary>
        /// Sets the player ID for the network object.
        /// </summary>
        /// <param name="id">The player ID to set.</param>
        public void SetPlayerId(ushort id) {
            this.playerId = id;
        }

        /// <summary>
        /// Gets the player ID of the network object.
        /// </summary>
        /// <returns>The player ID.</returns>
        public ushort GetPlayerId() {
            return this.playerId;
        }

        /// <summary>
        /// Sets the player index for the network element.
        /// </summary>
        /// <param name="index">The player index to set.</param>
        public void SetPlayerIndex(ushort index) {
            this.playerIndex = index;
        }

        /// <summary>
        /// Gets the player index of the network element.
        /// </summary>
        /// <returns>The player index as a ushort.</returns>
        public ushort GetPlayerIndex() {
            return this.playerIndex;
        }

        /// <summary>
        /// Gets the active rate of the network object.
        /// </summary>
        /// <returns>The active rate.</returns>
        public int GetActiveRate(bool normalizeTimeScale = true) {
            return (normalizeTimeScale) ? Mathf.CeilToInt(this.activeRate / ((Time.timeScale > 0.0) ? Time.timeScale : 1.0f)) : this.activeRate;
        }

        /// <summary>
        /// Sets the active rate of the network object and calculates the active frequency rate.
        /// </summary>
        /// <param name="value">The active rate to set.</param>
        public void SetActiveRate(int value) {
            this.activeRate = value;
        }

        /// <summary>
        /// Return the active frequency rate
        /// </summary>
        /// <returns>Active frequency rate</returns>
        public float GetActiveFrequencyRate() {
            return (this.GetActiveRate() > 0) ? (1.0f / this.GetActiveRate()) : 0;
        }

        /// <summary>
        /// Gets the passive rate of the network object.
        /// </summary>
        /// <returns>The passive rate.</returns>
        public int GetPassiveRate(bool normalizeTimeScale = true) {
            return (normalizeTimeScale) ? Mathf.CeilToInt(this.passiveRate / ((Time.timeScale > 0.0) ? Time.timeScale : 1.0f)) : this.passiveRate;
        }

        /// <summary>
        /// Sets the passive rate of the network object and calculates the passive frequency rate.
        /// </summary>
        /// <param name="value">The passive rate to set.</param>
        public void SetPassiveRate(int value) {
            this.passiveRate = value;
        }

        /// <summary>
        /// Return the pssive frequency rate
        /// </summary>
        /// <returns>Pasive frequency rate</returns>
        public float GetPassiveFrequencyRate() {
            return (this.GetPassiveRate() > 0) ? (1.0f / this.GetPassiveRate()) : 0;
        }

        /// <summary>
        /// Enable the minimum send rate
        /// </summary>
        /// <param name="enabled">Minimun rate is enabled</param>
        public void SetEnableMinimunRate(bool enabled) {
            this.enableMinimumRate = enabled;
        }

        /// <summary>
        /// Return gi minimum rate is enabled
        /// </summary>
        /// <returns>True if is enabled, otherwise false</returns>
        public bool IsToEnableMinimunRate() {
            return this.enableMinimumRate;
        }

        /// <summary>
        /// Configure minimum rate os sending messages
        /// </summary>
        /// <param name="minRateValue">Minimum rate value</param>
        public void SetMinimunRateValue(int minRateValue) {
            this.minimunRate = minRateValue;
            this.minimunFrequencyRate = (1.0f / this.minimunRate);
        }

        /// <summary>
        /// Determines if the network object represents a player.
        /// </summary>
        /// <returns>True if the object represents a player, false otherwise.</returns>
        public bool IsPlayer() {
            return this.isNetworkPlayer;
        }

        /// <summary>
        /// Registers a behavior with the network object.
        /// </summary>
        /// <typeparam name="T">The type of the behavior entity.</typeparam>
        /// <typeparam name="E">The type of the data stream.</typeparam>
        /// <param name="behavior">The behavior to register.</param>
        public void RegisterBehavior<T, E>(INetworkEntity<T, E> behavior) where E : IDataStream {
            if (!this.behaviors.Contains(behavior)) {
                this.behaviors.Add(behavior);
                behavior.SetNetworkObject(this);
            }
        }

        /// <summary>
        /// Unregisters a behavior from the network object.
        /// </summary>
        /// <typeparam name="T">The type of the behavior entity to unregister.</typeparam>
        public void UnregisterBehavior<T>() where T : IEntity {
            T unregisterComponent = default(T);
            bool found = false;
            foreach (var behavior in this.behaviors) {
                if (behavior.GetType().Equals(typeof(T))) {
                    unregisterComponent = (T)behavior;
                    found = true;
                    break;
                }
            }
            if (found) {
                this.behaviors.Remove(unregisterComponent);
            }
        }

        /// <summary>
        /// Checks if a behavior of a specific type is registered with the network object.
        /// </summary>
        /// <typeparam name="T">The type of the behavior entity to check.</typeparam>
        /// <returns>True if the behavior is registered, false otherwise.</returns>
        public bool HasBehavior<T>() where T : IEntity {
            bool result = false;
            foreach (IEntity behavior in this.behaviors) {
                if (behavior.GetType().Equals(typeof(T))) {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves a behavior of a specific type from the network object.
        /// </summary>
        /// <typeparam name="T">The type of the behavior entity to retrieve.</typeparam>
        /// <returns>The behavior of the specified type if found, otherwise default value of T.</returns>
        public T GetBehavior<T>() where T : IEntity {
            T result = default(T);
            foreach (IEntity behavior in this.behaviors) {
                if (behavior.GetType().Equals(typeof(T))) {
                    result = (T)behavior;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves a behavior of a specific type from the network object.
        /// </summary>
        /// <typeparam name="T">The type of the behavior entity to retrieve.</typeparam>
        /// <returns>The behavior of the specified type if found, otherwise default value of T.</returns>
        public void UpdateBehaviors() {
            foreach (IEntity behavior in this.behaviors) {
                behavior.FlagUpdated(true);
            }            
        }
    }

}