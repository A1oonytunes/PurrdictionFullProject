namespace com.onlineobject.objectnet {
    /// <summary>
    /// Interface defining the contract for network input handling.
    /// </summary>
    public interface INetworkInput {
        /// <summary>
        /// Registers a new input with the specified name.
        /// </summary>
        /// <typeparam name="T">The type of the input to register.</typeparam>
        /// <param name="inputName">The name of the input to register.</param>
        /// <returns>An IInputEntry instance representing the registered input.</returns>
        IInputEntry<T> RegisterInput<T>(string inputName);

        /// <summary>
        /// Sets the state of an input identified by name.
        /// </summary>
        /// <typeparam name="T">The type of the input state.</typeparam>
        /// <param name="inputName">The name of the input to set.</param>
        /// <param name="state">The new state of the input.</param>
        void SetInput<T>(string inputName, T state);

        /// <summary>
        /// Sets the state of an input identified by a code.
        /// </summary>
        /// <typeparam name="T">The type of the input state.</typeparam>
        /// <param name="inputCode">The code of the input to set.</param>
        /// <param name="state">The new state of the input.</param>
        void SetInput<T>(byte inputCode, T state);

        /// <summary>
        /// Retrieves the state of an input identified by a code.
        /// </summary>
        /// <typeparam name="T">The type of the input state.</typeparam>
        /// <param name="inputCode">The code of the input to get.</param>
        /// <returns>The state of the input.</returns>
        T GetInput<T>(byte inputCode);

        /// <summary>
        /// Retrieves the state of an input identified by name.
        /// </summary>
        /// <typeparam name="T">The type of the input state.</typeparam>
        /// <param name="inputName">The name of the input to get.</param>
        /// <returns>The state of the input.</returns>
        T GetInput<T>(string inputName);

        /// <summary>
        /// Retrieves all registered inputs.
        /// </summary>
        /// <returns>An array of IInput representing all registered inputs.</returns>
        IInput[] GetInputs();

        /// <summary>
        /// Determines whether the network input is local.
        /// </summary>
        /// <returns>True if the input is local; otherwise, false.</returns>
        bool IsLocal();

        /// <summary>
        /// Determines whether the network input is currently active.
        /// </summary>
        /// <returns>True if the input is active; otherwise, false.</returns>
        bool IsActive();
    }

}