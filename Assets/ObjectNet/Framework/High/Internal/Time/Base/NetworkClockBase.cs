using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// Abstract base class for network clocks, providing a common interface and basic functionality.
    /// </summary>
    public abstract class NetworkClockBase : INetworkClock {

        /// <summary>Current frame count.</summary>
        protected int currentFrameCount = 0;

        /// <summary>Current fixed update frame count.</summary>
        protected int currentFixedFrameCount = 0;

        /// <summary>Current fixed delta time, initialized to Unity's default fixed delta time.</summary>
        protected float currentFixedDeltaTime = Time.fixedDeltaTime; // To keep according to Unity's default

        /// <summary>Time when the clock started.</summary>
        protected float startedTimeOfClock = 0f;

        /// <summary>Current tick count.</summary>
        private int currentTickCount = 0;

        /// <summary>
        /// Constructor that initializes the global network clock.
        /// </summary>
        public NetworkClockBase() {
            NetworkClock.InitializeGlobal(this);
        }

        /// <summary>
        /// Gets the delta frames since the last update.
        /// </summary>
        /// <returns>The number of delta frames.</returns>
        public abstract int GetDeltaFrames();

        /// <summary>
        /// Gets the delta time since the last update.
        /// </summary>
        /// <returns>The delta time in seconds.</returns>
        public abstract float GetDeltaTime();

        /// <summary>
        /// Gets the current time according to the network clock.
        /// </summary>
        /// <returns>The current time in seconds.</returns>
        public abstract float GetTime();

        /// <summary>
        /// Initializes the network clock, setting the start time.
        /// </summary>
        public virtual void Initialize() {
            this.startedTimeOfClock = this.GetTime();
        }

        /// <summary>
        /// Gets the fixed delta time for fixed updates.
        /// </summary>
        /// <returns>The fixed delta time in seconds.</returns>
        public virtual float GetFixedDeltaTime() {
            return this.currentFixedDeltaTime;
        }

        /// <summary>
        /// Gets the total number of frames since the clock started.
        /// </summary>
        /// <returns>The total frame count.</returns>
        public int GetFramesCount() {
            return this.currentFrameCount;
        }

        /// <summary>
        /// Gets the current tick count.
        /// </summary>
        /// <returns>The current tick count.</returns>
        public virtual int GetTick() {
            return this.currentTickCount;
        }

        /// <summary>
        /// Updates the tick count, incrementing it or setting it to an override value.
        /// </summary>
        /// <param name="overrideValue">The value to override the tick count with. If zero, the count is incremented.</param>
        public virtual void UpdateTick(int overrideValue = 0) {
            if (overrideValue == 0) {
                this.currentTickCount++;
            } else {
                this.currentTickCount = overrideValue;
            }
        }

        /// <summary>
        /// Updates the frame count to the specified count.
        /// </summary>
        /// <param name="count">The new frame count.</param>
        public void UpdateFramesCount(int count) {
            this.currentFrameCount = count;
        }

        /// <summary>
        /// Increments the fixed frame count and recalculates the fixed delta time based on the elapsed time.
        /// </summary>
        public void UpdateFixedFramesCount() {
            this.currentFixedFrameCount++;
            // Calculate fixed frame count on average
            float deltaTime = (this.GetTime() - this.startedTimeOfClock);
            // Update fixed delta time
            if (deltaTime > 0f) {
                this.currentFixedDeltaTime = (deltaTime / (float)this.currentFixedFrameCount);
            }
        }
    }

}