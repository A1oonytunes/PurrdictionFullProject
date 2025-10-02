using System.Runtime.InteropServices;
using UnityEngine;
using System;

namespace com.onlineobject.objectnet
{
    /// <summary>
    /// A data handler for handling Matrix4x4 data streams.
    /// </summary>
    public class Matrix4x4Stream : DataHandler<Matrix4x4>
    {

        /// <summary>
        /// Writes the Matrix4x4 data to the byte buffer at the specified offset.
        /// </summary>
        /// <param name="data">The Matrix4x4 data to write.</param>
        /// <param name="buffer">The byte buffer to write to.</param>
        /// <param name="offset">The offset in the buffer to start writing at.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        public override int Write(Matrix4x4 data, ref byte[] buffer, ref int offset)
        {
            int startOffset = offset;

            MemoryMarshal.Write(buffer.AsSpan(offset), ref data);
            offset += sizeof(float) * 16 / sizeof(byte);


            return (offset - startOffset);
        }

        /// <summary>
        /// Reads Matrix4x4 data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The Matrix4x4 data read from the buffer.</returns>
        public override Matrix4x4 Read(byte[] buffer, ref int offset)
        {
            // Read the the Matrix4x4 from the buffer and update the offset
            Matrix4x4 matrix = MemoryMarshal.Read<Matrix4x4>(buffer.AsSpan().Slice(offset));
            offset += sizeof(float) * 16 / sizeof(byte);
            return matrix;
        }

    }

}
