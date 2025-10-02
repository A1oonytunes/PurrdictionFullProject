using System;
using System.Runtime.InteropServices;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a data handler specifically for uint16 streams.
    /// </summary>
    public class UInt16Stream : DataHandler<UInt16> {
        /// <summary>
        /// Writes data of type USHORT to a buffer at a specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data to.</param>
        /// <param name="offset">The offset in the buffer at which to start writing.</param>
        /// <returns>The number of bytes written to the buffer.</returns>     
        public override int Write(ushort data, ref byte[] buffer, ref int offset){
            int startOffset = offset;
            MemoryMarshal.Write<ushort>(buffer.AsSpan(offset), ref data);
            offset += sizeof(ushort) / sizeof(byte);
            return (offset - startOffset);
        }

        /// <summary>
        /// Reads USHORT data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The USHORT data read from the buffer.</returns>
        public override ushort Read(byte[] buffer, ref int offset){
            ushort value = MemoryMarshal.Read<ushort>(buffer.AsSpan().Slice(offset));
            offset += sizeof(ushort) / sizeof(byte);
            return value;
        }

    }
}