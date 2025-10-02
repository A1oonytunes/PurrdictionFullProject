using System.Runtime.InteropServices;
using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a data handler specifically for boolean streams.
    /// </summary>
    public class BooleanStream : DataHandler<bool> {
        // Currently, no additional members are defined. This class inherits all of its functionality from DataHandler<bool>.
        /// <summary>
        /// Writes the BOOLEAN data to the byte buffer at the specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The byte buffer to write to.</param>
        /// <param name="offset">The offset in the buffer to start writing at.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        public override int Write(bool data, ref byte[] buffer, ref int offset){
            int startOffset = offset;
            MemoryMarshal.Write<bool>(buffer.AsSpan(offset), ref data);
            offset += sizeof(bool) / sizeof(byte);
            return (offset - startOffset);
        }

        /// <summary>
        /// Reads BOOLEAN data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The BOOLEAN data read from the buffer.</returns>
        public override bool Read(byte[] buffer, ref int offset){
            bool value = MemoryMarshal.Read<bool>(buffer.AsSpan().Slice(offset));
            offset += sizeof(bool) / sizeof(byte);
            return value;
        }
    }

}