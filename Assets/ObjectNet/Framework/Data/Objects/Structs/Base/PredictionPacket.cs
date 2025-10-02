

using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a packet containing prediction data for an entity's position.
    /// </summary>
    public class PredictionPacket : IPredictionPacket {

        /// <summary>
        /// The predicted position of the entity in a 3D space.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// The latency of the client in seconds. This is used to adjust predictions
        /// to account for network delay.
        /// </summary>
        public float latency; // Client latency

        /// <summary>
        /// The time in seconds since the last update. This is used to calculate
        /// the new predicted position based on the last known velocity and direction.
        /// </summary>
        public float deltaTime;

    }

}