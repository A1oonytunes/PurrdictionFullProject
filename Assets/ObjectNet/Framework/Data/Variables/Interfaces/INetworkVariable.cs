namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines an interface for a network variable.
    /// </summary>
    /// <remarks>
    /// A network variable is a special type of variable that can be synchronized across a network.
    /// It provides methods to get and set its value.
    /// </remarks>
    /// <typeparam name="T">The type of the value stored in the network variable.</typeparam>
    public interface INetworkVariable<T> : IVariable {
        /// <summary>
        /// Retrieves the current value of the network variable.
        /// </summary>
        /// <returns>The current value of type T.</returns>
        T GetValue();

        /// <summary>
        /// Sets the value of the network variable.
        /// </summary>
        /// <param name="value">The new value to set of type T.</param>
        void SetValue(T value);
    }

}