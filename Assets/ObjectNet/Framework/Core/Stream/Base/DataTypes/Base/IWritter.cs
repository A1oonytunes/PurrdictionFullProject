using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the interface for a writer that can serialize data into a byte buffer.
    /// </summary>
    public interface IWritter : IData {
        /// <summary>
        /// Writes the specified data into the provided byte buffer starting at the given offset.
        /// </summary>
        /// <param name="data">The data to be written.</param>
        /// <param name="buffer">The byte array to write the data to. This array will be modified by the method.</param>
        /// <param name="offset">The starting position within the buffer at which to begin writing. This value will be updated to reflect the new position after writing.</param>
        /// <param name="dataType">The type of the data to be written.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
         int Write(object data, ref byte[] buffer, ref int offset, Type dataType);
    }

}