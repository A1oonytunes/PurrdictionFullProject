using System.Runtime.InteropServices;
using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a data handler specifically for byte streams.
    /// </summary>
    public class ByteStream : DataHandler<byte> {
        /// <summary>
        /// Writes data of type BYTE to a buffer at a specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data to.</param>
        /// <param name="offset">The offset in the buffer at which to start writing.</param>
        /// <returns>The number of bytes written to the buffer.</returns>     
        public override int Write(byte data, ref byte[] buffer, ref int offset){
            int startOffset = offset;
            MemoryMarshal.Write<byte>(buffer.AsSpan(offset), ref data);
            offset += sizeof(byte) / sizeof(byte);
            return (offset - startOffset);
        }

        /// <summary>
        /// Reads BYTE data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The BYTE data read from the buffer.</returns>
        public override byte Read(byte[] buffer, ref int offset){
            byte value = MemoryMarshal.Read<byte>(buffer.AsSpan().Slice(offset));
            offset += sizeof(byte) / sizeof(byte);
            return value;
        }
    }
}