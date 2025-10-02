#pragma warning disable 0168
#pragma warning disable 0219
#pragma warning disable 0414

using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a clock implementation for Unity game engine.
    /// </summary>
    public class UnityClock : NetworkClockBase {

        private int previousFrameTime = 0; // Stores the frame count of the previous frame.

        private float currentDeltaTime = 0.0f; // Stores the time between the current and previous frame.

        private int currentDeltaFrames = 0; // Stores the number of frames elapsed since the previous frame.

        private bool isInitialized = false; // Indicates whether the clock has been initialized.

        /// <summary>
        /// Initializes a new instance of the UnityClock class.
        /// </summary>
        public UnityClock() : base() {
        }

        /// <summary>
        /// Initializes the clock if it has not been initialized already.
        /// </summary>
        public override void Initialize() {
            if (!this.isInitialized) {
                this.previousFrameTime = Time.frameCount;
                this.isInitialized = true;
            }
        }

        /// <summary>
        /// Gets the time between the current and previous frame.
        /// </summary>
        /// <returns>The time between the current and previous frame.</returns>
        public override float GetDeltaTime() {
            return Time.deltaTime;
        }

        /// <summary>
        /// Gets the fixed time interval between physics updates.
        /// </summary>
        /// <returns>The fixed time interval between physics updates.</returns>
        public override float GetFixedDeltaTime() {
            return Time.fixedDeltaTime;
        }

        /// <summary>
        /// Gets the time since the start of the game.
        /// </summary>
        /// <returns>The time since the start of the game.</returns>
        public override float GetTime() {
            return Time.time;
        }

        /// <summary>
        /// Gets the number of frames elapsed since the previous frame.
        /// </summary>
        /// <returns>The number of frames elapsed since the previous frame.</returns>
        public override int GetDeltaFrames() {
            this.CalculateDelta();
            return this.currentDeltaFrames;
        }

        /// <summary>
        /// Calculates the number of frames elapsed since the previous frame.
        /// </summary>
        private void CalculateDelta() {
            this.Initialize();
            if (Time.frameCount > this.previousFrameTime) {
                this.currentDeltaFrames = (Time.frameCount - this.previousFrameTime);
                this.previousFrameTime = Time.frameCount;
            }
        }
    }

}