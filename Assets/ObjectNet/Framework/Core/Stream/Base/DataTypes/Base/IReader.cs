using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines an interface for objects that can read data from a byte array.
    /// </summary>
    public interface IReader : IData {
        /// <summary>
        /// Reads data from a byte array and converts it into the specified type.
        /// </summary>
        /// <typeparam name="E">The type of the data to be read.</typeparam>
        /// <param name="buffer">The byte array containing the data to read.</param>
        /// <param name="offset">The starting position within the buffer. This value is updated with the new position after the read operation.</param>
        /// <param name="dataType">The Type of the data to be read. This is used for type checking and conversion.</param>
        /// <returns>The data read from the buffer, converted to the specified type.</returns>
        E Read<E>(byte[] buffer, ref int offset, Type dataType);
    }

}