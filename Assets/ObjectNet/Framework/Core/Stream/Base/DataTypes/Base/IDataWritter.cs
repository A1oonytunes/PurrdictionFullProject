namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines a generic data writer interface that writes data of type <typeparamref name="T"/> into a buffer.
    /// </summary>
    /// <typeparam name="T">The type of data to be written.</typeparam>
    public interface IDataWritter<T> : IWritter {
        /// <summary>
        /// Writes the specified data of type <typeparamref name="T"/> into the provided buffer starting at the given offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data into. This buffer is passed by reference.</param>
        /// <param name="offset">The starting position within the buffer to begin writing. This offset is passed by reference and should be updated to reflect the new position after writing.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        int Write(T data, ref byte[] buffer, ref int offset);

    }
     
}