using System;
using System.Runtime.InteropServices;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a data handler specifically for int32 streams.
    /// </summary>
    public class Int32Stream : DataHandler<Int32> {

        /// <summary>
        /// Writes the INT data to the byte buffer at the specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The byte buffer to write to.</param>
        /// <param name="offset">The offset in the buffer to start writing at.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        public override int Write(int data, ref byte[] buffer, ref int offset){
            int startOffset = offset;
            MemoryMarshal.Write(buffer.AsSpan(offset), ref data);
            offset += sizeof(int) / sizeof(byte);

            return (offset - startOffset);
        }

        /// <summary>
        /// Reads INT data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The INT data read from the buffer.</returns>
        public override int Read(byte[] buffer, ref int offset){
            int value = MemoryMarshal.Read<int>(buffer.AsSpan().Slice(offset));
            offset += sizeof(int) / sizeof(byte);
            return value;
        }
    }
}