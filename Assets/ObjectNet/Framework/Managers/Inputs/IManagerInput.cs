using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Interface defining the structure for manager input handling.
    /// </summary>
    public interface IManagerInput {
        /// <summary>
        /// Retrieves the unique code associated with the input.
        /// </summary>
        /// <returns>A byte representing the unique input code.</returns>
        byte GetInputCode();

        /// <summary>
        /// Retrieves the name of the input.
        /// </summary>
        /// <returns>A string representing the name of the input.</returns>
        string GetInputName();

        /// <summary>
        /// Retrieves the method associated with the input processing.
        /// </summary>
        /// <returns>A string representing the method used for input processing.</returns>
        string GetMethod();

        /// <summary>
        /// Retrieves the type of manager handling the input.
        /// </summary>
        /// <returns>A string representing the type of manager.</returns>
        string GetManagerType();

        /// <summary>
        /// Retrieves the Type of the input.
        /// </summary>
        /// <returns>A Type representing the specific input type.</returns>
        Type GetInputType();
    }

}