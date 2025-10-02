using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a packet of data containing information about an object's position.
    /// </summary>
    public class PositionPacket : IPositionPacket {

        /// <summary>
        /// Gets the simulation tick at which the position is recorded.
        /// </summary>
        public int Tick { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the position change was a result of a teleport.
        /// </summary>
        public bool IsTeleport { get; private set; }

        /// <summary>
        /// Gets the position of the object in 3D space.
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Gets the time elapsed since the last position update.
        /// </summary>
        public float DeltaTime { get; private set; }

        /// <summary>
        /// Gets the velocity of the object.
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        /// Object speed based on the difference between positions on time
        /// </summary>
        public float Speed { get; private set; }

        /// <summary>
        /// Time where event occurs
        /// </summary>
        public float Time { get; private set; }

        /// <summary>
        /// Initializes a new instance of the PositionPacket class with specified parameters.
        /// </summary>
        /// <param name="tick">The simulation tick at which the position is recorded.</param>
        /// <param name="teleport">Whether the position change was a result of a teleport.</param>
        /// <param name="position">The position of the object in 3D space.</param>
        public PositionPacket(int tick, bool teleport, Vector3 position) {
            this.Tick = tick;
            this.IsTeleport = teleport;
            this.Position = position;
            this.DeltaTime = 0f;
            this.Velocity = Vector3.zero;
        }

        /// <summary>
        /// Initializes a new instance of the PositionPacket class with specified parameters, including delta time and linear velocity.
        /// </summary>
        /// <param name="tick">The simulation tick at which the position is recorded.</param>
        /// <param name="teleport">Whether the position change was a result of a teleport.</param>
        /// <param name="position">The position of the object in 3D space.</param>
        /// <param name="deltaTime">The time elapsed since the last position update.</param>
        /// <param name="velocity">The velocity of the object.</param>
        public PositionPacket(int tick, bool teleport, Vector3 position, float deltaTime, Vector3 velocity, float speed) {
            this.Tick = tick;
            this.IsTeleport = teleport;
            this.Position = position;
            this.DeltaTime = deltaTime;
            this.Velocity = velocity;
            this.Speed = speed;
        }

    }

}