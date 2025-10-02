using System.Runtime.InteropServices;
using System;

namespace com.onlineobject.objectnet
{
    /// <summary>
    /// Represents a data handler specifically for signalized bytes on streams.
    /// </summary>
    public class SByteStream : DataHandler<sbyte>
    {

        /// <summary>
        /// Writes data of type SBYTE to a buffer at a specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data to.</param>
        /// <param name="offset">The offset in the buffer at which to start writing.</param>
        /// <returns>The number of bytes written to the buffer.</returns>     
        public override int Write(sbyte data, ref byte[] buffer, ref int offset){
            int startOffset = offset;
            MemoryMarshal.Write<sbyte>(buffer.AsSpan(offset), ref data);
            offset += sizeof(sbyte) / sizeof(byte);
            return (offset - startOffset);
        }

        /// <summary>
        /// Reads SBYTE data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The SBYTE data read from the buffer.</returns>
        public override sbyte Read(byte[] buffer, ref int offset){
            sbyte value = MemoryMarshal.Read<sbyte>(buffer.AsSpan().Slice(offset));
            offset += sizeof(sbyte) / sizeof(byte);
            return value;
        }
    }
}