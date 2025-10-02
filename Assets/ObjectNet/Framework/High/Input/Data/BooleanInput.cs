namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a boolean input entry that can be used to store and manipulate a boolean value.
    /// </summary>
    public class BooleanInput : InputEntry<bool> {

        /// <summary>
        /// Initializes a new instance of the BooleanInput class with the specified name, code, and local flag.
        /// </summary>
        /// <param name="name">The name of the input entry.</param>
        /// <param name="code">The unique byte code representing the input entry.</param>
        /// <param name="local">Optional. A boolean flag indicating whether the input is local. Default is false.</param>
        public BooleanInput(string name, byte code, bool local = false) : base(name, code, local) {
        }

        /// <summary>
        /// Gets the current value of the boolean input and resets it to false after retrieval.
        /// </summary>
        /// <returns>The current value of the boolean input before it is reset.</returns>
        public override bool GetValue() {
            try {
                // Return the current value
                return this.value;
            } finally {
                // Ensure the value is reset to false after it's been retrieved
                this.value = false;
            }
        }

        /// <summary>
        /// Sets the value of the boolean input using a logical OR operation with the existing value.
        /// </summary>
        /// <param name="value">The boolean value to set.</param>
        public override void SetValue(bool value) {
            // Set the value using a logical OR to combine with the existing value
            this.value |= value;
        }

    }

}