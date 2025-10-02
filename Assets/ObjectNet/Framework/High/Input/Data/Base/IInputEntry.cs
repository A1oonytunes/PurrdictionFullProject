using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Interface defining the contract for an input entry that can handle values of type T.
    /// </summary>
    /// <typeparam name="T">The type of the value that the input entry will handle.</typeparam>
    public interface IInputEntry<T> : IInput {

        /// <summary>
        /// Retrieves the current value of the input entry.
        /// </summary>
        /// <returns>The value of type T currently held by the input entry.</returns>
        T GetValue();

        /// <summary>
        /// Sets the input entry to a specified value.
        /// </summary>
        /// <param name="value">The value of type T to set the input entry to.</param>
        void SetValue(T value);

        /// <summary>
        /// Registers a function that will be called to evaluate the input entry's value.
        /// </summary>
        /// <param name="function">A function that returns a value of type T when called.</param>
        void OnEvaluate(Func<T> function);

        /// <summary>
        /// Evaluates the current value of the input entry by invoking the registered evaluation function.
        /// </summary>
        /// <returns>The evaluated value of type T.</returns>
        T Evaluate();

    }

}