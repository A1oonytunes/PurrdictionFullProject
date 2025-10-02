using System;
using System.Runtime.InteropServices;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a data handler specifically for uint64 streams.
    /// </summary>
    public class UInt64Stream : DataHandler<UInt64> {

        /// <summary>
        /// Writes data of type ULONG to a buffer at a specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data to.</param>
        /// <param name="offset">The offset in the buffer at which to start writing.</param>
        /// <returns>The number of bytes written to the buffer.</returns>     
        public override int Write(ulong data, ref byte[] buffer, ref int offset){
            int startOffset = offset;
            MemoryMarshal.Write<ulong>(buffer.AsSpan(offset), ref data);
            offset += sizeof(ulong) / sizeof(byte);
            return (offset - startOffset);
        }

        /// <summary>
        /// Reads ULONG data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The ULONG data read from the buffer.</returns>
        public override ulong Read(byte[] buffer, ref int offset){
            ulong value = MemoryMarshal.Read<ulong>(buffer.AsSpan().Slice(offset));
            offset += sizeof(ulong) / sizeof(byte);
            return value;
        }
    }
}