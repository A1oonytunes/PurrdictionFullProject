namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a packet of information for a particle system.
    /// </summary>
    public struct ParticleInfoPacket {
        /// <summary>
        /// Gets or sets a value indicating whether the particle system is currently playing.
        /// </summary>
        /// <value>
        /// <c>true</c> if the particle system is playing; otherwise, <c>false</c>.
        /// </value>
        public bool IsPlaying { get; set; }

        /// <summary>
        /// Gets or sets the emission rate of particles over time.
        /// </summary>
        /// <value>
        /// The number of particles to emit per unit of time.
        /// </value>
        public float RateOverTime { get; set; }

        /// <summary>
        /// Gets or sets the emission rate of particles over distance.
        /// </summary>
        /// <value>
        /// The number of particles to emit per unit of distance.
        /// </value>
        public float RateOverDistance { get; set; }
    }

}