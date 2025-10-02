using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents an abstract class for input entry with a specified type.
    /// </summary>
    /// <typeparam name="T">The type of the input value.</typeparam>
    public abstract class InputEntry<T> : IInputEntry<T> {

        protected T value; // The value of the input entry.

        private byte Code; // The code associated with the input entry.

        private string Name; // The name of the input entry.

        private bool Local; // Indicates whether the input entry is local.

        private Func<T> evaluate; // The function used to evaluate the input entry.

        /// <summary>
        /// Initializes a new instance of the InputEntry class with the specified name, code, and local flag.
        /// </summary>
        /// <param name="name">The name of the input entry.</param>
        /// <param name="code">The code associated with the input entry.</param>
        /// <param name="local">Indicates whether the input entry is local (default is false).</param>
        public InputEntry(string name, byte code, bool local = false) {
            this.Name = name;
            this.Code = code;
            this.Local = local;
        }

        /// <summary>
        /// Gets the code associated with the input entry.
        /// </summary>
        /// <returns>The code associated with the input entry.</returns>
        public byte GetCode() {
            return this.Code;
        }

        /// <summary>
        /// Gets the name of the input entry.
        /// </summary>
        /// <returns>The name of the input entry.</returns>
        public string GetName() {
            return this.Name;
        }

        /// <summary>
        /// Gets the type of the input value.
        /// </summary>
        /// <returns>The type of the input value.</returns>
        public Type GetInputType() {
            return typeof(T);
        }

        /// <summary>
        /// Sets the local flag for the input entry.
        /// </summary>
        /// <param name="value">The value to set for the local flag.</param>
        public void SetLocalInput(bool value) {
            this.Local = value;
        }

        /// <summary>
        /// Indicates whether the input entry is local.
        /// </summary>
        /// <returns>True if the input entry is local; otherwise, false.</returns>
        public bool IsLocalInput() {
            return this.Local;
        }

        /// <summary>
        /// Sets the evaluation function for the input entry.
        /// </summary>
        /// <param name="function">The evaluation function to set.</param>
        public void OnEvaluate(Func<T> function) {
            this.evaluate = function;
        }

        /// <summary>
        /// Evaluates the input entry using the assigned evaluation function.
        /// </summary>
        /// <returns>The evaluated value of the input entry.</returns>
        public T Evaluate() {
            return this.evaluate.Invoke();
        }

        /// <summary>
        /// Gets the value of the input entry.
        /// </summary>
        /// <returns>The value of the input entry.</returns>
        public abstract T GetValue();

        /// <summary>
        /// Sets the value of the input entry.
        /// </summary>
        /// <param name="value">The value to set for the input entry.</param>
        public abstract void SetValue(T value);
    }

}