namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents an abstract base class for a network entity that can operate in active or passive mode.
    /// </summary>
    /// <typeparam name="T">The type of data used in passive mode operations.</typeparam>
    /// <typeparam name="E">The type of data stream used in active mode operations, must implement IDataStream.</typeparam>
    public abstract class NetworkEntity<T, E> : INetworkEntity<T, E> where E : IDataStream {

        /// <summary>
        // Reference to an object that represents a network element.
        /// <summary>
        private INetworkElement networkObject;

        /// <summary>
        // Indicates whether the network entity is currently active.
        /// <summary>
        private bool active = true;

        /// <summary>
        // Indicates whether the network entity is paused.
        /// <summary>
        private bool paused = false;

        /// <summary>
        // Determines if the logic for computing and synchronizing should be inverted.
        /// <summary>
        private bool invertLogic = false;

        /// <summary>
        // Tracks if the network entity has been updated.
        /// <summary>
        private bool updated = false;

        /// <summary>
        /// When update was flagged, this value will keep sending position for a certain amount of time
        /// to ensure that enought updates was sent in case of lost packets over UDP
        /// </summary>
        private float invalidateUpdateDelay = 0f;

        /// <summary>
        /// Is starting up initialized ?
        /// </summary
        private bool invalidateStarted = false;

        /// <summary>
        /// Delay to start invalidation
        /// </summary>
        private float invalidateStartupStart = 0f;

        /// <summary>
        /// During initialization i'm going to send everthing to ensure correclty movement start
        /// </summary>
        private float invalidateStartupTime = 0f;

        /// <summary>
        /// Timeout to detect the minimun of sending time ( to execute intergrity update pooling )
        /// </summary>
        private float nextMinimunSendRateTime = 0f;

        /// <summary>
        /// Amount of time to send updates
        /// </summary>
        private const float INVALIDATE_DELAY = 0.5f; // 1/2 second

        /// <summary>
        /// Amount of time to send updates during starting up
        /// </summary>
        private const float STARTUP_DURATION = 2.0f; // 2 second(s)

        /// <summary>
        /// Amount of time to send updates during starting up
        /// </summary>
        private const float STARTUP_DELAY = 0.005f; // 2 second(s)

        /// <summary>
        // Abstract method to compute the active state of the network entity.
        /// <summary>
        public abstract void ComputeActive();

        /// <summary>
        // Abstract method to compute the passive state of the network entity.
        /// <summary>
        public abstract void ComputePassive();

        /// <summary>
        /// Abstract method to synchronize the active state of the network entity with a data writer.
        /// </summary>
        /// <param name="writer">Writter object</param>
        public abstract void SynchonizeActive(E writer);

        /// <summary>
        /// Abstract method to synchronize the passive state of the network entity with given data.
        /// </summary>
        /// <param name="data">Data to be Synchonize</param>
        public abstract void SynchonizePassive(T data);

        /// <summary>
        /// Abstract method to retrieve arguments for passive mode operations.
        /// </summary>
        /// <returns>Alguments data</returns>
        public abstract T GetPassiveArguments();

        /// <summary>
        // Abstract method to extract data from a data stream.
        /// <summary>
        public abstract void Extract(E data);

        /// <summary>
        // Default constructor.
        /// <summary>
        public NetworkEntity() : base() {            
        }

        /// <summary>
        // Constructor that initializes the network entity with a network element.
        /// <summary>
        public NetworkEntity(INetworkElement networkObject) : base() {
            this.networkObject      = networkObject;
        }

        /// <summary>
        // Public method to compute the state of the network entity based on the inversion logic.
        /// <summary>
        public void Compute() {
            if (this.IsLogicInverted()) {
                this.InternalComputeInverted();
            } else {
                this.InternalCompute();
            }
        }

        /// <summary>
        // Internal method to compute the state when logic is not inverted.
        /// <summary>
        private void InternalCompute() {
            if ((this.GetNetworkObject().IsActive()) &&
                (this.GetNetworkObject().IsPassive())) {
                if (this.IsPaused() == false) {
                    this.ComputePassive();
                }
                this.ComputeActive();
            } else if (this.GetNetworkObject().IsActive()) {
                this.ComputeActive();
            } else if (this.GetNetworkObject().IsPassive()) {
                if (this.IsPaused() == false) {
                    this.ComputePassive();
                }
            }
        }

        /// <summary>
        // Internal method to compute the state when logic is inverted.
        /// <summary>
        private void InternalComputeInverted() {
            if ((this.GetNetworkObject().IsActive()) &&
                (this.GetNetworkObject().IsPassive())) {
                if (this.IsPaused() == false) {
                    this.ComputePassive();
                }
                this.ComputeActive();
            } else if (this.GetNetworkObject().IsActive()) {
                if (this.IsPaused() == false) {
                    this.ComputePassive();
                }
            } else if (this.GetNetworkObject().IsPassive()) {
                this.ComputeActive();                
            }
        }

        /// <summary>
        // Public method to synchronize the state of the network entity based on the inversion logic.
        /// <summary>
        public void Synchronize() {
            if (this.IsLogicInverted()) {
                this.InternalSynchonizeInverted();
            } else {
                this.InternalSynchonize();
            }
        }

        /// <summary>
        // Internal method to synchronize the state when logic is not inverted.
        /// <summary>
        private void InternalSynchonize() {
            if (this.GetNetworkObject().IsActive()) {
                this.SynchonizeActive(this.GetActiveArguments());
            } else if (this.GetNetworkObject().IsPassive()) {
                this.SynchonizePassive(this.GetPassiveArguments());
            }
        }

        /// <summary>
        // Internal method to synchronize the state when logic is inverted.
        /// <summary>
        private void InternalSynchonizeInverted() {
            if (this.GetNetworkObject().IsActive()) {
                this.SynchonizePassive(this.GetPassiveArguments());
            } else if (this.GetNetworkObject().IsPassive()) {
                this.SynchonizeActive(this.GetActiveArguments());                
            }
        }


        /// <summary>
        /// Consumes data from the provided IDataStream by extracting it using the Extract method.
        /// </summary>
        /// <param name="reader">The data stream to consume from.</param>
        public void Consume(IDataStream reader) {
            // Cast the reader to type E and pass it to the Extract method
            this.Extract((E)reader);
        }

        /// <summary>
        /// Retrieves the associated network object.
        /// </summary>
        /// <returns>The INetworkElement associated with this instance.</returns>
        public INetworkElement GetNetworkObject() {
            // Return the private field holding the network object
            return this.networkObject;
        }

        /// <summary>
        /// Sets the associated network object.
        /// </summary>
        /// <param name="networkObject">The network object to associate with this instance.</param>
        public void SetNetworkObject(INetworkElement networkObject) {
            // Assign the provided network object to the private field
            this.networkObject = networkObject;
        }

        /// <summary>
        /// Gets the active arguments from the network object's writer stream.
        /// </summary>
        /// <returns>The active arguments of type E.</returns>
        public E GetActiveArguments() {
            // Cast the writer stream from the network object to type E and return it
            return (E)this.GetNetworkObject().GetWritterStream();
        }

        /// <summary>
        /// Sends data with the specified event code and delivery mode.
        /// </summary>
        /// <param name="eventCode">The event code to send.</param>
        /// <param name="writer">The data stream to send.</param>
        /// <param name="mode">The delivery mode, defaulting to Unreliable if not specified.</param>
        public void Send(int eventCode, DataStream writer, DeliveryMode mode = DeliveryMode.Unreliable, IClient client = null) {
            // Use the network object to send the data with the provided event code and delivery mode
            this.GetNetworkObject().Send(eventCode, writer, mode, client);
        }

        /// <summary>
        /// Checks if the instance is currently active.
        /// </summary>
        /// <returns>True if active, otherwise false.</returns>
        public bool IsActive() {
            // Return the private field indicating if the instance is active
            return this.active;
        }

        /// <summary>
        /// Sets the active state of the instance.
        /// </summary>
        /// <param name="value">The active state to set.</param>
        public void SetActive(bool value) {
            // Set the private field to the provided active state value
            this.active = value;
        }

        /// <summary>
        /// Pause this behaviour
        /// </summary>
        public void Pause() {
            this.FlagUpdated(this.paused == false);
            this.paused = true;
        }

        /// <summary>
        /// Resume this behaviour
        /// </summary>
        public void Resume() {
            this.FlagUpdated(this.paused == true);
            this.paused = false;
        }

        /// <summary>
        /// Determine if synchonization is paused
        /// </summary>
        /// <returns>True if is paused; otherwise, false.</returns>
        public bool IsPaused() {
            return this.paused;
        }

        /// <summary>
        /// Checks if the logic of the instance is inverted.
        /// </summary>
        /// <returns>True if logic is inverted, otherwise false.</returns>
        public bool IsLogicInverted() {
            // Return the private field indicating if the logic is inverted
            return this.invertLogic;
        }

        /// <summary>
        /// Sets the logic inversion state of the instance.
        /// </summary>
        /// <param name="value">The logic inversion state to set.</param>
        public void SetLogicInverted(bool value) {
            // Set the private field to the provided logic inversion state value
            this.invertLogic = value;
        }

        /// <summary>
        /// Checks if the instance was updated.
        /// </summary>
        /// <returns>True if updated, otherwise false.</returns>
        public bool WasUpdated() {
            if (!this.invalidateStarted) {
                this.invalidateStarted = true;
                this.invalidateStartupTime = (NetworkClock.time + STARTUP_DURATION);
                this.invalidateStartupStart = (NetworkClock.time + STARTUP_DELAY);
            }
            this.updated |= ((this.invalidateStarted) && (this.invalidateStartupStart < NetworkClock.time) && (this.invalidateStartupTime > NetworkClock.time));
            if (this.invalidateStartupTime < NetworkClock.time) {
                if ((this.networkObject == null) || (this.networkObject.IsToEnableMinimunRate())) {
                    if (this.updated) {
                        this.nextMinimunSendRateTime = (NetworkClock.time + NetworkClock.Constants.UpdateRefreshRate);
                    } else if (this.nextMinimunSendRateTime < NetworkClock.time) {
                        this.updated = true;
                        this.nextMinimunSendRateTime = (NetworkClock.time + NetworkClock.Constants.UpdateRefreshRate);
                    }
                }
            }
            // Return the private field indicating if the instance was updated
            return ((this.updated) || (this.invalidateUpdateDelay > NetworkClock.time));            
        }

        /// <summary>
        /// Flags the instance as updated based on the provided value.
        /// </summary>
        /// <param name="updated">The updated state to set.</param>
        public void FlagUpdated(bool updated) {
            if ( updated ) {
                this.invalidateUpdateDelay = (NetworkClock.time + INVALIDATE_DELAY);
            }
            // Perform a bitwise OR on the updated field with the provided value
            this.updated |= updated;
        }

        /// <summary>
        /// Invalidates the updated state of the instance.
        /// </summary>
        public void Invalidate() {
            // Set the updated field to false, indicating it is no longer updated
            this.updated = false;
        }

    }

}