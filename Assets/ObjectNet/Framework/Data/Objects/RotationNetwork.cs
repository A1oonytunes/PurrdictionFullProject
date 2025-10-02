
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// The RotationNetwork class is responsible for synchronizing and interpolating the rotation of a networked object.
    /// It extends from the generic NetworkEntity class, which handles the networking aspect.
    /// </summary>
    public class RotationNetwork : NetworkEntity<Vector3, IDataStream> {

        // Current rotation of the object in Euler angles.
        private Vector3 rotation = Vector3.zero;

        // Current rotation of the object as a Quaternion.
        private Quaternion objectQuaternion = Quaternion.identity;

        // Flags to enable or disable synchronization for each axis.
        private bool enableXAxis = true;
        private bool enableYAxis = true;
        private bool enableZAxis = true;

        // Flag to determine if the object has been initialized.
        private bool initialized = false;

        // Flag to determine if interpolation should be used when applying rotations.
        private bool interpolate = true;

        // Timestamp of the last received rotation data.
        private float receivedTime = 0f;

        // Timestamp of the previous rotation data.
        private float previousTime = 0f;

        // Threshold to determine if the rotation has changed significantly.
        const float ROTATION_THRESHOULD = 0.01f;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RotationNetwork() : base() {
        }

        /// <summary>
        /// Constructor with axis enable flags.
        /// </summary>
        /// <param name="x">Enable rotation synchronization on the X axis.</param>
        /// <param name="y">Enable rotation synchronization on the Y axis.</param>
        /// <param name="z">Enable rotation synchronization on the Z axis.</param>
        public RotationNetwork(bool x, bool y, bool z) : base() {
            this.enableXAxis = x;
            this.enableYAxis = y;
            this.enableZAxis = z;
        }

        /// <summary>
        /// Constructor with a reference to the network object.
        /// </summary>
        /// <param name="networkObject">The network object to be synchronized.</param>
        public RotationNetwork(INetworkElement networkObject) : base(networkObject) {
        }

        /// <summary>
        /// Computes the active state of the rotation, determining if the rotation has changed significantly.
        /// </summary>
        public override void ComputeActive() {
            // Check if the rotation has changed beyond the threshold and flag the object as updated.
            this.FlagUpdated(Quaternion.Angle(this.objectQuaternion, this.GetNetworkObject().GetGameObject().transform.rotation) > ROTATION_THRESHOULD);
            // Update the current rotation and quaternion from the GameObject.
            this.rotation = this.GetNetworkObject().GetGameObject().transform.eulerAngles;
            this.objectQuaternion = this.GetNetworkObject().GetGameObject().transform.rotation;
        }

        /// <summary>
        /// Computes the passive state of the rotation, applying interpolated or direct rotation to the GameObject.
        /// </summary>
        public override void ComputePassive() {
            // Initialize the rotation on the first pass.
            if (!this.initialized) {
                this.rotation = this.GetNetworkObject().GetGameObject().transform.eulerAngles;
            }
            // Apply interpolated or direct rotation based on the 'interpolate' flag.
            if (this.interpolate) {
                // Interpolate the rotation over time.
                this.GetNetworkObject().GetGameObject().transform.eulerAngles = Quaternion.Slerp(
                    this.GetNetworkObject().GetGameObject().transform.rotation,
                    Quaternion.Euler(this.rotation),
                    NetworkClock.deltaTime * (1.0f / ((Mathf.Abs(this.previousTime - this.receivedTime) > 0f) ? Mathf.Abs(this.previousTime - this.receivedTime) : 1f))
                ).eulerAngles;
            } else {
                // Apply the rotation directly.
                this.GetNetworkObject().GetGameObject().transform.eulerAngles = this.rotation;
            }
        }

        /// <summary>
        /// Synchronizes the passive state with the received rotation data.
        /// </summary>
        /// <param name="data">The received rotation data in Euler angles.</param>
        public override void SynchonizePassive(Vector3 data) {
            this.rotation = data;
        }

        /// <summary>
        /// Synchronizes the active state by writing the rotation data to the writer stream.
        /// </summary>
        /// <param name="writer">The data stream to write the rotation data to.</param>
        public override void SynchonizeActive(IDataStream writer) {
            // Write if update is paused
            writer.Write(this.IsPaused());
            if (this.IsPaused() == false) {
                // Write the full rotation or individual axes based on the enabled flags.
                if (this.enableXAxis && this.enableYAxis && this.enableZAxis) {
                    writer.Write(this.rotation);
                } else {
                    if (this.enableXAxis)
                        writer.Write(this.rotation.x);
                    if (this.enableYAxis)
                        writer.Write(this.rotation.y);
                    if (this.enableZAxis)
                        writer.Write(this.rotation.z);
                }
            }
        }

        /// <summary>
        /// Gets the current rotation arguments for passive synchronization.
        /// </summary>
        /// <returns>The current rotation in Euler angles.</returns>
        public override Vector3 GetPassiveArguments() {
            return this.rotation;
        }

        /// <summary>
        /// Extracts the rotation data from the reader stream and applies it to the object.
        /// </summary>
        /// <param name="reader">The data stream to read the rotation data from.</param>
        public override void Extract(IDataStream reader) {
            // Mark as initialized and update the time stamps.
            this.initialized = true;
            this.previousTime = (this.receivedTime > 0) ? this.receivedTime : NetworkClock.time;
            this.receivedTime = NetworkClock.time;
            // First extract if position is paused by other side
            bool isSenderPaused = reader.Read<bool>();
            if (isSenderPaused == false) {
                this.Resume();
                // Read the full rotation or individual axes based on the enabled flags.
                if (this.enableXAxis && this.enableYAxis && this.enableZAxis) {
                    this.rotation = reader.Read<Vector3>();
                } else {
                    this.rotation = new Vector3(
                        (this.enableXAxis) ? reader.Read<float>() : this.GetNetworkObject().GetGameObject().transform.rotation.x,
                        (this.enableYAxis) ? reader.Read<float>() : this.GetNetworkObject().GetGameObject().transform.rotation.y,
                        (this.enableZAxis) ? reader.Read<float>() : this.GetNetworkObject().GetGameObject().transform.rotation.z
                    );
                }
            } else {
                this.Pause();
            }
        }
    }


}