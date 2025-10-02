using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A data handler for handling Vector3 data streams.
    /// </summary>
    public class Vector3Stream : DataHandler<Vector3> {
        /// <summary>
        /// Writes the Vector3 data to the byte buffer at the specified offset.
        /// </summary>
        /// <param name="data">The Vector3 data to write.</param>
        /// <param name="buffer">The byte buffer to write to.</param>
        /// <param name="offset">The offset in the buffer to start writing at.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        public override int Write(Vector3 data, ref byte[] buffer, ref int offset) {
            int startOffset = offset;

            MemoryMarshal.Write(buffer.AsSpan(offset), ref data.x);
            offset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(buffer.AsSpan(offset), ref data.y);
            offset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(buffer.AsSpan(offset), ref data.z);
            offset += sizeof(float) / sizeof(byte);

            return (offset - startOffset);
        }

        /// <summary>
        /// Reads Vector3 data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The Vector3 data read from the buffer.</returns>
        public override Vector3 Read(byte[] buffer, ref int offset) {

            // Read the x component of the Vector2 from the buffer and update the offset
            float x = MemoryMarshal.Read<float>(buffer.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the y component of the Vector2 from the buffer and update the offset
            float y = MemoryMarshal.Read<float>(buffer.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the z component of the Vector2 from the buffer and update the offset
            float z = MemoryMarshal.Read<float>(buffer.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);

            return new Vector3(x,y,z);
        }
    }
}