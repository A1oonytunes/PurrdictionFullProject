using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A specialized data handler class for serializing and deserializing Vector2 objects.
    /// Inherits from a generic DataHandler class that handles data of type Vector2.
    /// </summary>
    public class Vector2Stream : DataHandler<Vector2> {

        /// <summary>
        /// Writes the Vector2 data into a byte buffer.
        /// </summary>
        /// <param name="data">The Vector2 data to write.</param>
        /// <param name="buffer">The byte array buffer to write the data to.</param>
        /// <param name="offset">The current offset in the buffer. Will be updated after the write operation.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        public override int Write(Vector2 data, ref byte[] buffer, ref int offset) {
            int startOffset = offset;

            MemoryMarshal.Write(buffer.AsSpan(offset), ref data.x);
            offset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(buffer.AsSpan(offset), ref data.y);
            offset += sizeof(float) / sizeof(byte);

            return (offset - startOffset);
        }

        /// <summary>
        /// Reads Vector2 data from a byte buffer.
        /// </summary>
        /// <param name="buffer">The byte array buffer to read the data from.</param>
        /// <param name="offset">The current offset in the buffer. Will be updated after the read operation.</param>
        /// <returns>The Vector2 object read from the buffer.</returns>
        public override Vector2 Read(byte[] buffer, ref int offset) {
            // Read the x component of the Vector2 from the buffer and update the offset
            float x = MemoryMarshal.Read<float>(buffer.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the y component of the Vector2 from the buffer and update the offset
            float y = MemoryMarshal.Read<float>(buffer.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Return the reconstructed Vector2 object
            return new Vector2(x, y);
        }
    }
}