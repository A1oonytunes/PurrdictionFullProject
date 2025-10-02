using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A specialized data handler for streaming Color objects to and from byte arrays.
    /// </summary>
    public class ColorStream : DataHandler<Color> {

        /// <summary>
        /// Writes a Color object to a byte array buffer.
        /// </summary>
        /// <param name="data">The Color object to write.</param>
        /// <param name="buffer">The byte array buffer to write to.</param>
        /// <param name="offset">The current offset in the buffer. Will be updated after write.</param>
        /// <returns>The number of bytes written.</returns>
        public override int Write(Color data, ref byte[] buffer, ref int offset) {
            byte r = (byte)(data.r * 255);
            byte g = (byte)(data.g * 255);
            byte b = (byte)(data.b * 255);
            byte a = (byte)(data.a * 255);
            int startOffset = offset;

            MemoryMarshal.Write(buffer.AsSpan(offset), ref r);
            offset += sizeof(byte) / sizeof(byte);
            MemoryMarshal.Write(buffer.AsSpan(offset), ref g);
            offset += sizeof(byte) / sizeof(byte);
            MemoryMarshal.Write(buffer.AsSpan(offset), ref b);
            offset += sizeof(byte) / sizeof(byte);
            MemoryMarshal.Write(buffer.AsSpan(offset), ref a);
            offset += sizeof(byte) / sizeof(byte);

            return (offset - startOffset);
        }

        /// <summary>
        /// Reads a Color object from a byte array buffer.
        /// </summary>
        /// <param name="buffer">The byte array buffer to read from.</param>
        /// <param name="offset">The current offset in the buffer. Will be updated after read.</param>
        /// <returns>The Color object read from the buffer.</returns>
        public override Color Read(byte[] buffer, ref int offset) {
            // Read the byte array representation of the Color object from the buffer.
        
            byte r = MemoryMarshal.Read<byte>(buffer.AsSpan().Slice(offset));
            offset += sizeof(byte) / sizeof(byte);
            byte g = MemoryMarshal.Read<byte>(buffer.AsSpan().Slice(offset));
            offset += sizeof(byte) / sizeof(byte);
            byte b = MemoryMarshal.Read<byte>(buffer.AsSpan().Slice(offset));
            offset += sizeof(byte) / sizeof(byte);
            byte a = MemoryMarshal.Read<byte>(buffer.AsSpan().Slice(offset));
            offset += sizeof(byte) / sizeof(byte);

            // Extract the RGBA components and create a new Color object.
            Color color = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
            // Return the Color object.
            return color;
        }
    }

}