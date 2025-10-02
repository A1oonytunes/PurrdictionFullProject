namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines an interface for a network clock that can provide various time-related data.
    /// </summary>
    public interface INetworkClock {
        /// <summary>
        /// Gets the time in seconds since the last frame update.
        /// </summary>
        public float DeltaTime { get { return this.GetDeltaTime(); } }

        /// <summary>
        /// Gets the fixed time interval in seconds at which physics and other fixed frame rate updates are performed.
        /// </summary>
        public float FixedDeltaTime { get { return this.GetFixedDeltaTime(); } }

        /// <summary>
        /// Gets the total time in seconds since the start of the game.
        /// </summary>
        public float Time { get { return this.GetTime(); } }

        /// <summary>
        /// Gets the number of frames that have passed since the last update.
        /// </summary>
        public int DeltaFrames { get { return this.GetDeltaFrames(); } }

        /// <summary>
        /// Gets or sets the network tick, which is a counter that can be used for synchronizing states across the network.
        /// </summary>
        public int Tick { get { return this.GetTick(); } set { this.UpdateTick(value); } }

        /// <summary>
        /// Initializes the network clock.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Retrieves the current network tick.
        /// </summary>
        /// <returns>The current tick count.</returns>
        int GetTick();

        /// <summary>
        /// Retrieves the total elapsed time.
        /// </summary>
        /// <returns>The total time in seconds since the start of the game.</returns>
        float GetTime();

        /// <summary>
        /// Retrieves the time elapsed since the last frame update.
        /// </summary>
        /// <returns>The delta time in seconds.</returns>
        float GetDeltaTime();

        /// <summary>
        /// Retrieves the fixed delta time for fixed frame rate updates.
        /// </summary>
        /// <returns>The fixed delta time in seconds.</returns>
        float GetFixedDeltaTime();

        /// <summary>
        /// Retrieves the number of frames that have passed since the last update.
        /// </summary>
        /// <returns>The number of delta frames.</returns>
        int GetDeltaFrames();

        /// <summary>
        /// Retrieves the total number of frames since the start of the game.
        /// </summary>
        /// <returns>The total frames count.</returns>
        int GetFramesCount();

        /// <summary>
        /// Updates the frames count to a new value.
        /// </summary>
        /// <param name="count">The new frames count.</param>
        void UpdateFramesCount(int count);

        /// <summary>
        /// Updates the count of fixed frames, typically used for fixed update loops.
        /// </summary>
        void UpdateFixedFramesCount();

        /// <summary>
        /// Updates the network tick with an optional override value.
        /// </summary>
        /// <param name="overrideValue">The value to override the current tick with. Defaults to 0.</param>
        void UpdateTick(int overrideValue = 0);
    }

}