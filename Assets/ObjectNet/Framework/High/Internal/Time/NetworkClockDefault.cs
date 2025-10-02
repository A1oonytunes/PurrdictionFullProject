using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Abstract class representing a default network clock, providing basic timekeeping functionality for networked applications.
    /// </summary>
    public abstract class NetworkClockDefault : NetworkClockBase {

        // The rate at which the clock updates its tick count.
        private float clockFramerateRate = DEFAULT_CLOCK_TICK_RATE;

        // The current tick count.
        private int currentTick = 0;

        // The frame number when the last tick update occurred.
        private int currentFrameForTick = 0;

        // The frame number when the last time update occurred.
        private int currentFrameForTime = 0;

        // The current time in seconds since the clock started.
        private float currentTime = 0.0f;

        // The frame number during the previous time update.
        private int previousFrameTime = 0;

        // The time in seconds during the previous time update.
        private float previousTime = 0.0f;

        // The time difference in seconds between the current and previous update.
        private float currentDeltaTime = 0.0f;

        // The number of frames between the current and previous update.
        private int currentDeltaFrames = 0;

        // The time in seconds between each tick.
        private float timeBetweenTicks = (1.0f / DEFAULT_CLOCK_TICK_RATE);

        // Indicates whether the clock has been initialized.
        private bool isInitialized = false;

        // The start time of the clock in Unix time milliseconds.
        private long startTime = 0;

        // The default rate at which the clock ticks per second.
        private const float DEFAULT_CLOCK_TICK_RATE = 50f;

        // The divisor used to convert milliseconds to seconds.
        private const float MILLISECONDS_DIVISOR = 1000f;

        /// <summary>
        /// Constructor for the NetworkClockDefault class.
        /// </summary>
        public NetworkClockDefault() : base() {
        }

        /// <summary>
        /// Initializes the network clock, setting up the necessary variables and starting the timekeeping process.
        /// </summary>
        public override void Initialize() {
            if (!this.isInitialized) {
                this.isInitialized = true;
                this.currentFrameForTick = this.GetFramesCount();
                this.currentFrameForTime = this.GetFramesCount();
                this.previousFrameTime = this.GetFramesCount();
                this.startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                this.currentTime = this.GetRealTime();
                this.timeBetweenTicks = (1.0f / this.clockFramerateRate);
                base.Initialize();
            }
        }

        /// <summary>
        /// Retrieves the current tick count, updating it based on the elapsed time since the last tick.
        /// </summary>
        /// <returns>The current tick count.</returns>
        public override int GetTick() {
            this.Initialize();
            int ellapsedFrames = (this.GetFramesCount() - this.currentFrameForTick);
            if (ellapsedFrames > 0) {
                double ellapsedTime = (this.GetRealTime() - this.currentTime);
                this.currentTime = this.GetRealTime();
                // Recalculate current tick
                while (ellapsedTime > 0f) {
                    this.currentTick++;
                    ellapsedTime -= this.timeBetweenTicks;
                }
                this.currentFrameForTick = this.GetFramesCount();
            }
            return this.currentTick;
        }

        /// <summary>
        /// Updates the current tick count to a specified value.
        /// </summary>
        /// <param name="overrideValue">The value to override the current tick count with.</param>
        public override void UpdateTick(int overrideValue = 0) {
            if (overrideValue > 0) {
                this.currentTick = overrideValue;
            }
        }

        /// <summary>
        /// Retrieves the current time in seconds since the clock started.
        /// </summary>
        /// <returns>The current time.</returns>
        public override float GetTime() {
            this.Initialize();
            int ellapsedFrames = (this.GetFramesCount() - this.currentFrameForTime);
            if (ellapsedFrames > 0) {
                this.currentTime            = this.GetRealTime();
                this.currentFrameForTime    = this.GetFramesCount();
            }
            return this.currentTime;
        }

        /// <summary>
        /// Retrieves the time difference in seconds between the current and previous update.
        /// </summary>
        /// <returns>The delta time.</returns>
        public override float GetDeltaTime() {
            this.CalculateDelta();
            return this.currentDeltaTime;
        }

        /// <summary>
        /// Retrieves the number of frames between the current and previous update.
        /// </summary>
        /// <returns>The delta frames count.</returns>
        public override int GetDeltaFrames() {
            this.CalculateDelta();
            return this.currentDeltaFrames;
        }

        /// <summary>
        /// Calculates the time and frame differences between the current and previous updates.
        /// </summary>
        private void CalculateDelta() {
            this.Initialize();
            if (this.GetFramesCount() > this.previousFrameTime) {
                this.currentDeltaFrames = (this.GetFramesCount() - this.previousFrameTime);
                this.previousFrameTime  = this.GetFramesCount();
                this.currentDeltaTime   = (this.GetRealTime() - this.previousTime);
                this.previousTime       = this.GetRealTime();
            }
        }

        /// <summary>
        /// Sets the target framerate for the clock, adjusting the time between ticks accordingly.
        /// </summary>
        /// <param name="frameRate">The target framerate in frames per second.</param>
        public void SetTargetFramerate(int frameRate) {
            this.clockFramerateRate = frameRate;
            // Re-Configure minimum time between ticks
            this.timeBetweenTicks = (1.0f / this.clockFramerateRate);
        }

        /// <summary>
        /// Retrieves the current real time in seconds since the clock started.
        /// </summary>
        /// <returns>The real time in seconds.</returns>
        private float GetRealTime() {
            return ((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this.startTime) / MILLISECONDS_DIVISOR);
        }
    }

}