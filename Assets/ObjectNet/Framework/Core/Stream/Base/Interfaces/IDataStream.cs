using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines an interface for a data stream that can be used to read and write data in a variety of formats.
    /// </summary>
    public interface IDataStream : IDisposable {
        /// <summary>
        /// Allocates a buffer of the specified size for the data stream.
        /// </summary>
        /// <param name="bufferSize">The size of the buffer to allocate.</param>
        void Allocate(int bufferSize);

        /// <summary>
        /// Allocates a buffer using the provided byte array for the data stream.
        /// </summary>
        /// <param name="buffer">The byte array to use as the buffer.</param>
        void Allocate(byte[] buffer);

        /// <summary>
        /// Resets the data stream to its initial state.
        /// </summary>
        void Reset();

        /// <summary>
        /// Writes data of a generic type to the stream.
        /// </summary>
        /// <typeparam name="T">The type of data to write.</typeparam>
        /// <param name="data">The data to write to the stream.</param>
        void Write<T>(T data);

        /// <summary>
        /// Writes data of a generic type to the stream at the specified offset.
        /// </summary>
        /// <typeparam name="T">The type of data to write.</typeparam>
        /// <param name="data">The data to write to the stream.</param>
        /// <param name="offset">The offset at which to write the data.</param>
        void Write<T>(T data, int offset);

        /// <summary>
        /// Writes data of a specified type to the data buffer using a registered writer.
        /// </summary>
        /// <typeparam name="T">The type of data to write.</typeparam>
        /// <param name="data">The data to be written to the buffer.</param>
        /// <param name="dataType">The type of data to be written, used to find the appropriate writer.</param>
        /// <exception cref="Exception">Thrown when the specified type is not supported or the writer is not found.</exception>
        void Write<T>(T data, Type dataType);

        /// <summary>
        /// Reads data of a generic type from the stream.
        /// </summary>
        /// <typeparam name="T">The type of data to read.</typeparam>
        /// <param name="rewind">Whether to rewind the stream after reading.</param>
        /// <returns>The data read from the stream.</returns>
        T Read<T>(bool rewind = false);

        /// <summary>
        /// Reads data of a generic type from the stream at the specified offset.
        /// </summary>
        /// <typeparam name="T">The type of data to read.</typeparam>
        /// <param name="offset">The offset from which to read the data.</param>
        /// <returns>The data read from the stream.</returns>
        T Read<T>(int offset);

        /// <summary>
        /// Reads data of a specified type from the stream.
        /// </summary>
        /// <typeparam name="T">The type of data to read.</typeparam>
        /// <param name="dataType">The type of the data to read.</param>
        /// <returns>The data read from the stream.</returns>
        T Read<T>(Type dataType);

        /// <summary>
        /// Reads data of a specified type from the stream.
        /// </summary>
        /// <param name="dataType">The type of the data to read.</param>
        /// <returns>The data read from the stream.</returns>
        object Read(Type dataType);

        /// <summary>
        /// Retrieves the entire buffer from the data stream.
        /// </summary>
        /// <returns>The buffer as a byte array.</returns>
        byte[] GetBuffer();

        /// <summary>
        /// Retrieves the buffer size from the data stream.
        /// </summary>
        /// <returns>The buffer size.</returns>
        int GetBufferSize();

        /// <summary>
        /// Registers a stream with the data stream for a specific data type.
        /// </summary>
        /// <typeparam name="T">The type of the stream to register.</typeparam>
        /// <param name="dataType">The data type associated with the stream.</param>
        /// <param name="stream">The stream to register.</param>
        void RegisterStream<T>(Type dataType, T stream) where T : IData;

        /// <summary>
        /// Rewinds the stream by a specified distance.
        /// </summary>
        /// <param name="distance">The number of bytes to rewind.</param>
        void Rewind(int distance);

        /// <summary>
        /// Rewinds the stream to the beginning of the data of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of data to rewind to.</typeparam>
        void Rewind<T>();

        /// <summary>
        /// Moves the stream forward by a specified distance.
        /// </summary>
        /// <param name="distance">The number of bytes to move forward.</param>
        void Forward(int distance);

        /// <summary>
        /// Moves the stream forward to the end of the data of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of data to move forward to.</typeparam>
        void Forward<T>();

        /// <summary>
        /// Shifts the data in the stream to the left starting from a specified index.
        /// </summary>
        /// <param name="start">The index from which to start shifting.</param>
        /// <param name="distance">The number of bytes to shift.</param>
        void ShiftLeft(int start, int distance);

        /// <summary>
        /// Shifts the data in the stream to the right starting from a specified index.
        /// </summary>
        /// <param name="start">The index from which to start shifting.</param>
        /// <param name="distance">The number of bytes to shift.</param>
        void ShiftRight(int start, int distance);

        /// <summary>
        /// Return this this type is a valid Stream type
        /// </summary>
        /// <typeparam name="T">The type of data to check.</typeparam>
        /// <returns>true is a valid stream type, false otherwise</returns>
        bool IsValidType<T>();

        /// <summary>
        /// Return this this type is a valid Stream type
        /// </summary>
        /// <param name="dataType">The type of data to check.</param>
        /// <returns>true is a valid stream type, false otherwise</returns>
        bool IsValidType(Type dataType);

        /// <summary>
        /// Return associated type on index
        /// </summary>
        /// <param name="index">Index type</param>
        /// <returns>Type on specified index</returns>
        Type GetType(ushort index);

        /// <summary>
        /// Return index of type on factory map
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Index of specified type</returns>
        ushort GetTypeIndex(Type type);

        /// <summary>
        /// Return index of type on factory map
        /// </summary>
        /// <typeparam name="T">Type to check</typeparam>
        /// <returns>Index of specified type</returns>
        ushort GetTypeIndex<T>();

    }

}