using System;
using System.Runtime.InteropServices;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a data handler specifically for byte array streams.
    /// </summary>
    public class ByteArrayStream : DataHandler<byte[]> {

        /// <summary>
        /// Writes the Vector2 data into a byte buffer.
        /// </summary>
        /// <param name="data">The Vector2 data to write.</param>
        /// <param name="buffer">The byte array buffer to write the data to.</param>
        /// <param name="offset">The current offset in the buffer. Will be updated after the write operation.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        public override int Write(byte[] data, ref byte[] buffer, ref int offset) {
            int startOffset = offset;
            //send the array length first
            int ArrayLength = data.Length;
            MemoryMarshal.Write(buffer.AsSpan(offset), ref ArrayLength);
            offset += sizeof(int) / sizeof(byte);

            for (int bufferIndex = 0; bufferIndex < data.Length; bufferIndex++) {
                MemoryMarshal.Write<byte>(buffer.AsSpan(offset), ref data[bufferIndex]);
                offset++;
            }
            return (offset - startOffset);
        }

        /// <summary>
        /// Reads Vector3 data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The Vector3 data read from the buffer.</returns>
        public override byte[] Read(byte[] buffer, ref int offset) {
            // first i need to read size
            int ArrayLength = MemoryMarshal.Read<int>(buffer.AsSpan().Slice(offset));
            offset += sizeof(int) / sizeof(byte);
            // Now read total of bytes
            byte[] objectBytes = new byte[ArrayLength];
            for (int index = 0; index < objectBytes.Length; index++) {
                objectBytes[index] = MemoryMarshal.Read<byte>(buffer.AsSpan().Slice(offset));
                offset++;
            }
            return objectBytes;
        }
    }
}