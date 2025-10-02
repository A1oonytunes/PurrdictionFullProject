using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Interface defining the structure for input handling.
    /// </summary>
    public interface IInput {
        /// <summary>
        /// Retrieves the name of the input.
        /// </summary>
        /// <returns>A string representing the name of the input.</returns>
        string GetName();

        /// <summary>
        /// Retrieves the unique code associated with the input.
        /// </summary>
        /// <returns>A byte representing the unique code of the input.</returns>
        byte GetCode();

        /// <summary>
        /// Retrieves the type of the input.
        /// </summary>
        /// <returns>A Type representing the specific type of the input.</returns>
        Type GetInputType();

        /// <summary>
        /// Sets the input as local or not.
        /// </summary>
        /// <param name="value">A boolean value indicating whether the input is local (true) or not (false).</param>
        void SetLocalInput(bool value);

        /// <summary>
        /// Checks if the input is local.
        /// </summary>
        /// <returns>A boolean indicating whether the input is local (true) or not (false).</returns>
        bool IsLocalInput();
    }

}