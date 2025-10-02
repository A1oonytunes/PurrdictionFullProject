using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines a generic data reader interface that extends from IReader.
    /// </summary>
    /// <typeparam name="T">The type of data to be read.</typeparam>
    public interface IDataReader<T> : IReader {
        /// <summary>
        /// Reads data from a buffer and updates the offset.
        /// </summary>
        /// <param name="buffer">The byte array to read from.</param>
        /// <param name="offset">The reference to the current position in the buffer, which will be updated after the read operation.</param>
        /// <returns>The data read from the buffer, of type T.</returns>
        T Read(byte[] buffer, ref int offset);

        /// <summary>
        /// Reads data of a specified type from a buffer and updates the offset.
        /// </summary>
        /// <param name="buffer">The byte array to read from.</param>
        /// <param name="offset">The reference to the current position in the buffer, which will be updated after the read operation.</param>
        /// <param name="dataType">The type of data to be read from the buffer.</param>
        /// <returns>The data read from the buffer, of the specified type.</returns>
        T Read(byte[] buffer, ref int offset, Type dataType);

        /// <summary>
        /// Generic method to read data of any type from a buffer and updates the offset.
        /// </summary>
        /// <typeparam name="E">The type of data to be read.</typeparam>
        /// <param name="buffer">The byte array to read from.</param>
        /// <param name="offset">The reference to the current position in the buffer, which will be updated after the read operation.</param>
        /// <returns>The data read from the buffer, of type E.</returns>
        E Read<E>(byte[] buffer, ref int offset);
    }

}