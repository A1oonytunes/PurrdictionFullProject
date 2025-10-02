
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a network entity that synchronizes the scale of a game object across the network.
    /// </summary>
    public class ScaleNetwork : NetworkEntity<Vector3, IDataStream> {

        // Current scale of the network entity
        private Vector3 scale = Vector3.zero;

        // Flags to enable or disable synchronization for each axis
        private bool enableXAxis = true;
        private bool enableYAxis = true;
        private bool enableZAxis = true;

        // Flag to determine if the entity has been initialized
        private bool initialized = false;

        // Threshold for scale changes to be considered significant
        const float SCALE_THRESHOULD = 0.01f;

        /// <summary>
        /// Default constructor initializing the ScaleNetwork with all axes enabled.
        /// </summary>
        public ScaleNetwork() : base() {
        }

        /// <summary>
        /// Constructor that allows enabling or disabling synchronization for each axis.
        /// </summary>
        /// <param name="x">Enable synchronization on the X axis.</param>
        /// <param name="y">Enable synchronization on the Y axis.</param>
        /// <param name="z">Enable synchronization on the Z axis.</param>
        public ScaleNetwork(bool x, bool y, bool z) : base() {
            this.enableXAxis = x;
            this.enableYAxis = y;
            this.enableZAxis = z;
        }

        /// <summary>
        /// Constructor that initializes the ScaleNetwork with a network object.
        /// </summary>
        /// <param name="networkObject">The network object to be associated with this entity.</param>
        public ScaleNetwork(INetworkElement networkObject) : base(networkObject) {
        }

        /// <summary>
        /// Computes the active state of the network entity and flags it for update if the scale has changed significantly.
        /// </summary>
        public override void ComputeActive() {
            this.FlagUpdated(Vector3.Distance(this.scale, this.GetNetworkObject().GetGameObject().transform.localScale) > SCALE_THRESHOULD);
            this.scale = this.GetNetworkObject().GetGameObject().transform.localScale;
        }

        /// <summary>
        /// Computes the passive state of the network entity, initializing or updating the scale of the associated game object.
        /// </summary>
        public override void ComputePassive() {
            if (!this.initialized) {
                this.scale = this.GetNetworkObject().GetGameObject().transform.localScale;
            }
            this.GetNetworkObject().GetGameObject().transform.localScale = this.scale;
        }

        /// <summary>
        /// Gets the current scale arguments for passive synchronization.
        /// </summary>
        /// <returns>The current scale of the network entity.</returns>
        public override Vector3 GetPassiveArguments() {
            return this.scale;
        }

        /// <summary>
        /// Synchronizes the active state by writing the scale to the data stream.
        /// </summary>
        /// <param name="writer">The data stream to write the scale to.</param>
        public override void SynchonizeActive(IDataStream writer) {
            // Write if update is paused
            writer.Write(this.IsPaused());
            if (this.IsPaused() == false) {
                if (this.enableXAxis && this.enableYAxis && this.enableZAxis) {
                    writer.Write(this.scale);
                } else {
                    if (this.enableXAxis)
                        writer.Write(this.scale.x);
                    if (this.enableYAxis)
                        writer.Write(this.scale.y);
                    if (this.enableZAxis)
                        writer.Write(this.scale.z);
                }
            }
        }

        /// <summary>
        /// Synchronizes the passive state by updating the scale from the data.
        /// </summary>
        /// <param name="data">The new scale data to apply.</param>
        public override void SynchonizePassive(Vector3 data) {
            this.scale = data;
        }

        /// <summary>
        /// Extracts the scale from the data stream and applies it to the network entity.
        /// </summary>
        /// <param name="reader">The data stream to read the scale from.</param>
        public override void Extract(IDataStream reader) {
            this.initialized = true;
            // First extract if position is paused by other side
            bool isSenderPaused = reader.Read<bool>();
            if (isSenderPaused == false) {
                this.Resume();
                if (this.enableXAxis && this.enableYAxis && this.enableZAxis) {
                    this.scale = reader.Read<Vector3>();
                } else {
                    this.scale = new Vector3(
                        (this.enableXAxis) ? reader.Read<float>() : this.GetNetworkObject().GetGameObject().transform.localScale.x,
                        (this.enableYAxis) ? reader.Read<float>() : this.GetNetworkObject().GetGameObject().transform.localScale.y,
                        (this.enableZAxis) ? reader.Read<float>() : this.GetNetworkObject().GetGameObject().transform.localScale.z
                    );
                }
            } else {
                this.Pause();
            }
        }
    }
}