using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A data handler for handling Quaternion data streams.
    /// </summary>
    public class QuaternionStream : DataHandler<Quaternion> {
        /// <summary>
        /// Writes the Quaternion data to the byte buffer at the specified offset.
        /// </summary>
        /// <param name="data">The Quaternion data to write.</param>
        /// <param name="buffer">The byte buffer to write to.</param>
        /// <param name="offset">The offset in the buffer to start writing at.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        public override int Write(Quaternion data, ref byte[] buffer, ref int offset) {
            int startOffset = offset;

            MemoryMarshal.Write(buffer.AsSpan(offset), ref data.x);
            offset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(buffer.AsSpan(offset), ref data.y);
            offset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(buffer.AsSpan(offset), ref data.z);
            offset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(buffer.AsSpan(offset), ref data.w);
            offset += sizeof(float) / sizeof(byte);

            return (offset - startOffset);
        }

        /// <summary>
        /// Reads Quaternion data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The Quaternion data read from the buffer.</returns>
        public override Quaternion Read(byte[] buffer, ref int offset) {

            // Read the x component of the Quaternion from the buffer and update the offset
            float x = MemoryMarshal.Read<float>(buffer.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the y component of the Quaternion from the buffer and update the offset
            float y = MemoryMarshal.Read<float>(buffer.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the z component of the Quaternion from the buffer and update the offset
            float z = MemoryMarshal.Read<float>(buffer.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the w component of the Quaternion from the buffer and update the offset
            float w = MemoryMarshal.Read<float>(buffer.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);

            return new Quaternion(x, y, z, w);
        }
    }
}