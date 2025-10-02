#if UNITY_EDITOR
#endif

using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a network entity that synchronizes particle system data across a network.
    /// </summary>
    public class ParticlesNetwork : NetworkEntity<ParticleInfoPacket, IDataStream> {

        // Holds the current particle system data to be synchronized.
        private ParticleInfoPacket particlesData;

        // Holds the previous particle system data to detect changes.
        private ParticleInfoPacket previousParticlesData;

        // Reference to the ParticleSystem component.
        private ParticleSystem particles;

        // Reference to the emission module of the ParticleSystem.
        private ParticleSystem.EmissionModule emissor;

        // Flag to check if the network entity has been initialized.
        private bool initialized = false;

        /// <summary>
        /// Default constructor for the ParticlesNetwork.
        /// </summary>
        public ParticlesNetwork() : base() {
        }

        /// <summary>
        /// Constructor for the ParticlesNetwork with a specified network object.
        /// </summary>
        /// <param name="networkObject">The network element associated with this network entity.</param>
        public ParticlesNetwork(INetworkElement networkObject) : base(networkObject) {
        }

        /// <summary>
        /// Computes the active state of the particle system and flags any changes.
        /// </summary>
        public override void ComputeActive() {
            if (!this.initialized) {
                InitializeParticleSystem();
            }

            // Flag if there was an update to send
            this.FlagUpdated(this.particlesData.IsPlaying != this.previousParticlesData.IsPlaying);
            this.FlagUpdated(this.particlesData.RateOverTime != this.previousParticlesData.RateOverTime);
            this.FlagUpdated(this.particlesData.RateOverDistance != this.previousParticlesData.RateOverDistance);

            // Update current particle data
            UpdateCurrentParticleData();

            // Store the current data as previous for the next update cycle
            StoreCurrentAsPrevious();
        }

        /// <summary>
        /// Computes the passive state of the particle system and applies any received changes.
        /// </summary>
        public override void ComputePassive() {
            if (!this.initialized) {
                InitializeParticleSystem();
            }

            // Store the current data as previous for the next update cycle
            StoreCurrentAsPrevious();

            // Apply the new particle data to the particle system
            ApplyParticleData();
        }

        /// <summary>
        /// Synchronizes the active state of the particle system by writing the data to the provided writer.
        /// </summary>
        /// <param name="writer">The data stream writer to write the particle data to.</param>
        public override void SynchonizeActive(IDataStream writer) {
            if (!this.initialized) {
                InitializeParticleSystem();
            }
            // Write if update is paused
            writer.Write(this.IsPaused());
            if (this.IsPaused() == false) {
                writer.Write(this.particlesData.IsPlaying);
                writer.Write(this.particlesData.RateOverTime);
                writer.Write(this.particlesData.RateOverDistance);
            }
        }

        /// <summary>
        /// Synchronizes the passive state of the particle system by applying the received data.
        /// </summary>
        /// <param name="data">The particle information packet containing the data to apply.</param>
        public override void SynchonizePassive(ParticleInfoPacket data) {
            this.particlesData = data;
        }

        /// <summary>
        /// Gets the current particle information packet to be used for passive synchronization.
        /// </summary>
        /// <returns>The current particle information packet.</returns>
        public override ParticleInfoPacket GetPassiveArguments() {
            return this.particlesData;
        }

        /// <summary>
        /// Extracts the particle system data from the provided data stream reader.
        /// </summary>
        /// <param name="reader">The data stream reader to read the particle data from.</param>
        public override void Extract(IDataStream reader) {
            // First extract if position is paused by other side
            bool isSenderPaused = reader.Read<bool>();
            if (isSenderPaused == false) {
                this.Resume();
                this.particlesData.IsPlaying        = reader.Read<bool>();
                this.particlesData.RateOverTime     = reader.Read<float>();
                this.particlesData.RateOverDistance = reader.Read<float>();
            } else {
                this.Pause();
            }
        }

        /// <summary>
        // Initializes the particle system by retrieving the necessary components.
        /// <summary>
        private void InitializeParticleSystem() {
            this.particles = this.GetNetworkObject().GetGameObject().GetComponent<ParticleSystem>();
            if (this.particles == null) {
                ParticleSystem[] particles = this.GetNetworkObject().GetGameObject().GetComponentsInChildren<ParticleSystem>();
                if (particles.Length > 0) {
                    this.particles = particles[0];
                }
            }
            this.emissor = this.particles.emission;
            this.initialized = true;
        }

        /// <summary>
        // Updates the current particle data with the latest values from the particle system.
        /// <summary>
        private void UpdateCurrentParticleData() {
            if (this.particles != null) {
                this.particlesData.IsPlaying        = this.particles.isPlaying;
                this.particlesData.RateOverTime     = this.emissor.rateOverTime.constant;
                this.particlesData.RateOverDistance = this.emissor.rateOverDistance.constant;
            }
        }

        /// <summary>
        // Stores the current particle data as the previous data for comparison in the next update cycle.
        /// <summary>
        private void StoreCurrentAsPrevious() {
            this.previousParticlesData = this.particlesData;
        }

        /// <summary>
        // Applies the particle data to the particle system, starting or stopping it as necessary.
        /// <summary>
        private void ApplyParticleData() {
            if (!this.particles.isPlaying && this.particlesData.IsPlaying) {
                this.particles.Play();
            } else if (this.particles.isPlaying && !this.particlesData.IsPlaying) {
                this.particles.Stop();
            }
            this.emissor.rateOverTime = this.particlesData.RateOverTime;
            this.emissor.rateOverDistance = this.particlesData.RateOverDistance;
        }
    }

}