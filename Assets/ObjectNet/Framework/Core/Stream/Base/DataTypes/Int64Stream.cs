using System;
using System.Runtime.InteropServices;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a data handler specifically for Int64 streams.
    /// </summary>
    public class Int64Stream : DataHandler<Int64> {
        /// <summary>
        /// Writes data of type LONG to a buffer at a specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data to.</param>
        /// <param name="offset">The offset in the buffer at which to start writing.</param>
        /// <returns>The number of bytes written to the buffer.</returns>     
        public override int Write(long data, ref byte[] buffer, ref int offset){
            int startOffset = offset;
            MemoryMarshal.Write<long>(buffer.AsSpan(offset), ref data);
            offset += sizeof(long) / sizeof(byte);
            return (offset - startOffset);
        }

        /// <summary>
        /// Reads LONG data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The LONG data read from the buffer.</returns>
        public override long Read(byte[] buffer, ref int offset){
            long value = MemoryMarshal.Read<long>(buffer.AsSpan().Slice(offset));
            offset += sizeof(long) / sizeof(byte);
            return value;
        }
    }
}