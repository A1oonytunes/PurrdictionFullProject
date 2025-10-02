using com.onlineobject.objectnet;
using System;
using System.Runtime.InteropServices;


namespace com.onlineobject.objectnet
{    /// <summary>
     /// Represents a data handler specifically for CHAR streams.
     /// </summary>
    public class CharStream : DataHandler<char>
    {
        /// <summary>
        /// Writes data of type CHAR to a buffer at a specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data to.</param>
        /// <param name="offset">The offset in the buffer at which to start writing.</param>
        /// <returns>The number of bytes written to the buffer.</returns>     
        public override int Write(char data, ref byte[] buffer, ref int offset){
            int startOffset = offset;
            MemoryMarshal.Write<char>(buffer.AsSpan(offset), ref data);
            offset += sizeof(char) / sizeof(byte);
            return (offset - startOffset);
        }

        /// <summary>
        /// Reads CHAR data from the byte buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The CHAR data read from the buffer.</returns>
        public override char Read(byte[] buffer, ref int offset)
        {
            char value = MemoryMarshal.Read<char>(buffer.AsSpan().Slice(offset));
            offset += sizeof(char) / sizeof(byte);
            return value;
        }

    }
}
