namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a floating-point input entry.
    /// </summary>
    public class FloatInput : InputEntry<float> {
        /// <summary>
        /// Initializes a new instance of the FloatInput class.
        /// </summary>
        /// <param name="name">The name of the input entry.</param>
        /// <param name="code">The unique code representing the input entry.</param>
        /// <param name="local">Indicates whether the input is local or not. Default is false.</param>
        public FloatInput(string name, byte code, bool local = false) : base(name, code, local) {
            // Calls the base class constructor with the provided name, code, and local flag.
        }

        /// <summary>
        /// Gets the current value of the float input.
        /// </summary>
        /// <returns>The current value as a float.</returns>
        public override float GetValue() {
            // Return the current value stored in the base class's value field.
            return this.value;
        }

        /// <summary>
        /// Sets the value of the float input.
        /// </summary>
        /// <param name="value">The new value to be set.</param>
        public override void SetValue(float value) {
            // Assign the new value to the base class's value field.
            this.value = value;
        }
    }

}