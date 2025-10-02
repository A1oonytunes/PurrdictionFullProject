using System.Runtime.InteropServices;
using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a data handler specifically for float streams.
    /// </summary>
    public class FloatStream : DataHandler<float> {
        /// <summary>
        /// Writes data of type FLOAT to a buffer at a specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data to.</param>
        /// <param name="offset">The offset in the buffer at which to start writing.</param>
        /// <returns>The number of bytes written to the buffer.</returns>     
        public override int Write(float data, ref byte[] buffer, ref int offset){
            int startOffset = offset;
            MemoryMarshal.Write<float>(buffer.AsSpan(offset), ref data);
            offset += sizeof(float) / sizeof(byte);
            return (offset - startOffset);
        }

        /// <summary>
        /// Reads Float data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The Float data read from the buffer.</returns>
        public override float Read(byte[] buffer, ref int offset){
            float value = MemoryMarshal.Read<float>(buffer.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            return value;
        }
    }
}