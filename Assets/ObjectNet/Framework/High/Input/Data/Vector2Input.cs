using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a 2D vector input entry, extending the generic InputEntry class with Vector2 type.
    /// </summary>
    public class Vector2Input : InputEntry<Vector2> {
        /// <summary>
        /// Initializes a new instance of the Vector2Input class with a specified name, code, and locality.
        /// </summary>
        /// <param name="name">The name of the input entry.</param>
        /// <param name="code">The unique byte code representing the input entry.</param>
        /// <param name="local">Optional boolean indicating whether the input is local. Default is false.</param>
        public Vector2Input(string name, byte code, bool local = false) : base(name, code, local) {
        }

        /// <summary>
        /// Gets the current value of the Vector2 input.
        /// </summary>
        /// <returns>The current Vector2 value.</returns>
        public override Vector2 GetValue() {
            return this.value;
        }

        /// <summary>
        /// Sets the value of the Vector2 input.
        /// </summary>
        /// <param name="value">The Vector2 value to set.</param>
        public override void SetValue(Vector2 value) {
            this.value = value;
        }
    }

}