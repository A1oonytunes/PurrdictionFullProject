using System;
using System.Runtime.InteropServices;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a data handler specifically for int16 streams.
    /// </summary>
    public class Int16Stream : DataHandler<Int16> {
        /// <summary>
        /// Writes data of type SHORT to a buffer at a specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data to.</param>
        /// <param name="offset">The offset in the buffer at which to start writing.</param>
        /// <returns>The number of bytes written to the buffer.</returns>     
        public override int Write(short data, ref byte[] buffer, ref int offset){
            int startOffset = offset;
            MemoryMarshal.Write<short>(buffer.AsSpan(offset), ref data);
            offset += sizeof(short) / sizeof(byte);
            return (offset - startOffset);
        }

        /// <summary>
        /// Reads SHORT data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The SHORT data read from the buffer.</returns>
        public override short Read(byte[] buffer, ref int offset){
            short value = MemoryMarshal.Read<short>(buffer.AsSpan().Slice(offset));
            offset += sizeof(short) / sizeof(byte);
            return value;
        }
    }
}