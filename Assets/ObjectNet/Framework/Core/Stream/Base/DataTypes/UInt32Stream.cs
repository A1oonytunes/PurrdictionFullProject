using System;
using System.Runtime.InteropServices;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a data handler specifically for uint32 streams.
    /// </summary>
    public class UInt32Stream : DataHandler<UInt32> {
        /// <summary>
        /// Writes data of type UINT to a buffer at a specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data to.</param>
        /// <param name="offset">The offset in the buffer at which to start writing.</param>
        /// <returns>The number of bytes written to the buffer.</returns>     
        public override int Write(uint data, ref byte[] buffer, ref int offset){
            int startOffset = offset;
            MemoryMarshal.Write<uint>(buffer.AsSpan(offset), ref data);
            offset += sizeof(uint) / sizeof(byte);
            return (offset - startOffset);
        }

        /// <summary>
        /// Reads UINT data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The UINT data read from the buffer.</returns>
        public override uint Read(byte[] buffer, ref int offset){
            uint value = MemoryMarshal.Read<uint>(buffer.AsSpan().Slice(offset));
            offset += sizeof(uint) / sizeof(byte);
            return value;
        }
    }
}