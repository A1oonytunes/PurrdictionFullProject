using System.Runtime.InteropServices;
using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a data handler specifically for double streams.
    /// </summary>
    public class DoubleStream : DataHandler<double> {
        /// <summary>
        /// Writes data of type DOUBLE to a buffer at a specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data to.</param>
        /// <param name="offset">The offset in the buffer at which to start writing.</param>
        /// <returns>The number of bytes written to the buffer.</returns>     
        public override int Write(double data, ref byte[] buffer, ref int offset){
            int startOffset = offset;
            MemoryMarshal.Write<double>(buffer.AsSpan(offset), ref data);
            offset += sizeof(double) / sizeof(byte);
            return (offset - startOffset);
        }

        /// <summary>
        /// Reads DOUBLE data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The DOUBLE data read from the buffer.</returns>
        public override double Read(byte[] buffer, ref int offset){
            double value = MemoryMarshal.Read<double>(buffer.AsSpan().Slice(offset));
            offset += sizeof(double) / sizeof(byte);
            return value;
        }
    }
}