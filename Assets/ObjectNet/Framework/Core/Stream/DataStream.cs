using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a data stream that can handle various data types and network operations.
    /// </summary>
    public class DataStream : IDataStream, INetworkStream {

        // Buffer to hold the data stream.
        private byte[] dataBuffer;

        // Buffer to hold the data stream.
        private byte[] outputBuffer;

        // Current position in the buffer.
        private int bufferIndex = 0;

        // Size of the buffer.
        private int bufferSize = 0;

        // Flag to determine if buffer allocation should be automatic.
        private bool autoAllocate = true;

        // Client associated with the data stream.
        private IClient client;
        
        // Store the version of this data stream
        private uint streamVersion = START_STREAM_VERSION;

        // Local allowed stream types
        private static List<Type> allowedTypes = new List<Type>();

        // Mapping of data types to their respective read and write stream handlers.
        private static Dictionary<Type, Tuple<IData, IData>> streamMap = new Dictionary<Type, Tuple<IData, IData>>();
        
        // Global factory for creating stream handlers based on data types.
        private static Dictionary<Type, Type> streamFactory = new Dictionary<Type, Type>();

        // Global allowed stream types
        private static List<Type> streamAllowedTypes = new List<Type>();

        private static Dictionary<int, List<byte[]>> poolingBuffer = new Dictionary<int, List<byte[]>>();

        // Flag if stream was updated and need be remounted 
        private static volatile uint globalStreamVersion = START_STREAM_VERSION;

        // Default size for the buffer if not specified.
#if DISSONANCE_ENABLED
        readonly int DEFAULT_BUFFER_SIZE = TransportDefinitions.DatagramDissonanceBufferSize;
#else
        readonly int DEFAULT_BUFFER_SIZE = TransportDefinitions.DatagramBufferSize;
#endif        

        // Default stream version
        const int START_STREAM_VERSION = 1;
        
        /// <summary>
        /// Initializes a new instance of the DataStream class with a specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer to use for the data stream.</param>
        public DataStream(byte[] buffer) {
            this.Initialize();
            this.bufferSize = 0;
            this.bufferIndex = 0;
            this.dataBuffer = buffer;
        }

        /// <summary>
        /// Initializes a new instance of the DataStream class with a specified client, buffer, and buffer size.
        /// </summary>
        /// <param name="client">The client associated with the data stream.</param>
        /// <param name="buffer">The buffer to use for the data stream.</param>
        /// <param name="bufferSize">The size of the buffer.</param>
        public DataStream(IClient client, byte[] buffer, int bufferSize = 0) {
            this.Initialize();
            this.bufferSize = bufferSize;
            this.bufferIndex = 0;
            this.dataBuffer = buffer;
            this.client = client;
        }

        /// <summary>
        /// Initializes a new instance of the DataStream class with default settings.
        /// </summary>
        public DataStream() {
            this.bufferIndex = 0;
            this.Initialize();
        }

        /// <summary>
        /// Disposes of the resources used by the instance, clearing the stream map and suppressing finalization.
        /// </summary>
        public void Dispose() {
            // Return buffer to pooling
            if (this.dataBuffer     != null) this.EnqueueBuffer(this.dataBuffer);
            if (this.outputBuffer   != null) this.EnqueueBuffer(this.outputBuffer);

            this.dataBuffer     = null;
            this.outputBuffer   = null;
            // Suppress finalization to prevent the garbage collector from calling the finalizer
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Registers a global stream handler for a specific data type.
        /// </summary>
        /// <typeparam name="T">The type of the stream handler.</typeparam>
        /// <param name="dataType">The data type to associate with the stream handler.</param>
        /// <param name="streamType">The stream handler type.</param>
        public static void RegisterGlobalStream<T>(Type dataType, T streamType) where T : Type {
            bool updated = false;
            if (!DataStream.streamFactory.ContainsKey(typeof(T))) {
                DataStream.streamFactory.Add(dataType, streamType); // Register on factory
                updated = true;
            }
            if (!DataStream.streamAllowedTypes.Contains(typeof(T))) {
                DataStream.streamAllowedTypes.Add(dataType); // Register into valid types
                updated = true;
            }
            // Update global version of stream
            if (updated) {
                DataStream.globalStreamVersion++;
            }
        }

        /// <summary>
        /// Registers a global stream handler for a specific data type.
        /// </summary>
        /// <typeparam name="T">The type of the stream handler.</typeparam>
        /// <typeparam name="E">The stream handler type.</typeparam>
        public static void RegisterGlobalStream<T, E>() {
            bool updated = false;
            if (!DataStream.streamFactory.ContainsKey(typeof(T))) {
                DataStream.streamFactory.Add(typeof(T), typeof(E)); // Register on factory
                updated = true;
            }
            if (!DataStream.streamAllowedTypes.Contains(typeof(T))) {
                DataStream.streamAllowedTypes.Add(typeof(T)); // Register into valid types
                updated = true;
            }
            // Update global version of stream
            if (updated) {
                DataStream.globalStreamVersion++;
            }
        }

        /// <summary>
        /// Return fi has already a strem registered to desired type
        /// </summary>
        /// <typeparam name="T">The type of the stream handler.</typeparam>
        /// <typeparam name="E">The stream handler type.</typeparam>
        /// <returns>True if has already registered, otherwise false</returns>
        public static bool HasRegisteredStream<T, E>() {
            return DataStream.streamFactory.ContainsKey(typeof(T)) &&
                   DataStream.streamAllowedTypes.Contains(typeof(T));
        }

        /// <summary>
        /// Initializes the data stream by setting up default stream handlers for common data types.
        /// </summary>
        private void Initialize() {
            if ( DataStream.streamMap.Count == 0) {
                DataStream.streamMap.Add(typeof(Int16),       new Tuple<IData, IData>(new Int16Stream(),      new Int16Stream()));
                DataStream.streamMap.Add(typeof(UInt16),      new Tuple<IData, IData>(new UInt16Stream(),     new UInt16Stream()));
                DataStream.streamMap.Add(typeof(Int32),       new Tuple<IData, IData>(new Int32Stream(),      new Int32Stream()));
                DataStream.streamMap.Add(typeof(UInt32),      new Tuple<IData, IData>(new UInt32Stream(),     new UInt32Stream()));
                DataStream.streamMap.Add(typeof(Int64),       new Tuple<IData, IData>(new Int64Stream(),      new Int64Stream()));
                DataStream.streamMap.Add(typeof(UInt64),      new Tuple<IData, IData>(new UInt64Stream(),     new UInt64Stream()));
                DataStream.streamMap.Add(typeof(float),       new Tuple<IData, IData>(new FloatStream(),      new FloatStream()));
                DataStream.streamMap.Add(typeof(double),      new Tuple<IData, IData>(new DoubleStream(),     new DoubleStream()));
                DataStream.streamMap.Add(typeof(bool),        new Tuple<IData, IData>(new BooleanStream(),    new BooleanStream()));
                DataStream.streamMap.Add(typeof(string),      new Tuple<IData, IData>(new StringStream(),     new StringStream()));
                DataStream.streamMap.Add(typeof(char),        new Tuple<IData, IData>(new CharStream(),       new CharStream()));
                DataStream.streamMap.Add(typeof(Color),       new Tuple<IData, IData>(new ColorStream(),      new ColorStream()));
                DataStream.streamMap.Add(typeof(sbyte),       new Tuple<IData, IData>(new SByteStream(),      new SByteStream()));
                DataStream.streamMap.Add(typeof(byte),        new Tuple<IData, IData>(new ByteStream(),       new ByteStream()));
                DataStream.streamMap.Add(typeof(byte[]),      new Tuple<IData, IData>(new ByteArrayStream(),  new ByteArrayStream()));
                DataStream.streamMap.Add(typeof(Vector2),     new Tuple<IData, IData>(new Vector2Stream(),    new Vector2Stream()));
                DataStream.streamMap.Add(typeof(Vector3),     new Tuple<IData, IData>(new Vector3Stream(),    new Vector3Stream()));
                DataStream.streamMap.Add(typeof(Vector4),     new Tuple<IData, IData>(new Vector4Stream(),    new Vector4Stream()));
                DataStream.streamMap.Add(typeof(Matrix4x4),   new Tuple<IData, IData>(new Matrix4x4Stream(),  new Matrix4x4Stream()));
                DataStream.streamMap.Add(typeof(Quaternion),  new Tuple<IData, IData>(new QuaternionStream(), new QuaternionStream()));
                
                if (!DataStream.streamAllowedTypes.Contains(typeof(Int16)))      DataStream.streamAllowedTypes.Add(typeof(Int16));
                if (!DataStream.streamAllowedTypes.Contains(typeof(UInt16)))     DataStream.streamAllowedTypes.Add(typeof(UInt16));
                if (!DataStream.streamAllowedTypes.Contains(typeof(Int32)))      DataStream.streamAllowedTypes.Add(typeof(Int32));
                if (!DataStream.streamAllowedTypes.Contains(typeof(UInt32)))     DataStream.streamAllowedTypes.Add(typeof(UInt32));
                if (!DataStream.streamAllowedTypes.Contains(typeof(Int64)))      DataStream.streamAllowedTypes.Add(typeof(Int64));
                if (!DataStream.streamAllowedTypes.Contains(typeof(UInt64)))     DataStream.streamAllowedTypes.Add(typeof(UInt64));
                if (!DataStream.streamAllowedTypes.Contains(typeof(float)))      DataStream.streamAllowedTypes.Add(typeof(float));
                if (!DataStream.streamAllowedTypes.Contains(typeof(double)))     DataStream.streamAllowedTypes.Add(typeof(double));
                if (!DataStream.streamAllowedTypes.Contains(typeof(bool)))       DataStream.streamAllowedTypes.Add(typeof(bool));
                if (!DataStream.streamAllowedTypes.Contains(typeof(string)))     DataStream.streamAllowedTypes.Add(typeof(string));
                if (!DataStream.streamAllowedTypes.Contains(typeof(char)))       DataStream.streamAllowedTypes.Add(typeof(char));
                if (!DataStream.streamAllowedTypes.Contains(typeof(Color)))      DataStream.streamAllowedTypes.Add(typeof(Color));
                if (!DataStream.streamAllowedTypes.Contains(typeof(sbyte)))      DataStream.streamAllowedTypes.Add(typeof(sbyte));
                if (!DataStream.streamAllowedTypes.Contains(typeof(byte)))       DataStream.streamAllowedTypes.Add(typeof(byte));
                if (!DataStream.streamAllowedTypes.Contains(typeof(byte[])))     DataStream.streamAllowedTypes.Add(typeof(byte[]));
                if (!DataStream.streamAllowedTypes.Contains(typeof(Vector2)))    DataStream.streamAllowedTypes.Add(typeof(Vector2));
                if (!DataStream.streamAllowedTypes.Contains(typeof(Vector3)))    DataStream.streamAllowedTypes.Add(typeof(Vector3));
                if (!DataStream.streamAllowedTypes.Contains(typeof(Vector4)))    DataStream.streamAllowedTypes.Add(typeof(Vector4));
                if (!DataStream.streamAllowedTypes.Contains(typeof(Matrix4x4)))  DataStream.streamAllowedTypes.Add(typeof(Matrix4x4));
                if (!DataStream.streamAllowedTypes.Contains(typeof(Quaternion))) DataStream.streamAllowedTypes.Add(typeof(Quaternion));
                
                DataStream.allowedTypes.Add(typeof(Int16));
                DataStream.allowedTypes.Add(typeof(UInt16));
                DataStream.allowedTypes.Add(typeof(Int32));
                DataStream.allowedTypes.Add(typeof(UInt32));
                DataStream.allowedTypes.Add(typeof(Int64));
                DataStream.allowedTypes.Add(typeof(UInt64));
                DataStream.allowedTypes.Add(typeof(float));
                DataStream.allowedTypes.Add(typeof(double));
                DataStream.allowedTypes.Add(typeof(bool));
                DataStream.allowedTypes.Add(typeof(string));
                DataStream.allowedTypes.Add(typeof(char));
                DataStream.allowedTypes.Add(typeof(Color));
                DataStream.allowedTypes.Add(typeof(sbyte));
                DataStream.allowedTypes.Add(typeof(byte));
                DataStream.allowedTypes.Add(typeof(byte[]));
                DataStream.allowedTypes.Add(typeof(Vector2));
                DataStream.allowedTypes.Add(typeof(Vector3));
                DataStream.allowedTypes.Add(typeof(Vector4));
                DataStream.allowedTypes.Add(typeof(Matrix4x4));
                DataStream.allowedTypes.Add(typeof(Quaternion));

                // Register global stream classes
                foreach (var streamFactory in DataStream.streamFactory) {
                    if (!DataStream.streamMap.ContainsKey(streamFactory.Key)) {
                        DataStream.streamMap.Add(streamFactory.Key, 
                                           new Tuple<IData, IData>((IData)Activator.CreateInstance(streamFactory.Value), 
                                                                   (IData)Activator.CreateInstance(streamFactory.Value)));
                    }
                    // Register allowed type
                    if (!DataStream.allowedTypes.Contains(streamFactory.Key)) {
                        DataStream.allowedTypes.Add(streamFactory.Key);
                    }
                }
                this.streamVersion = DataStream.globalStreamVersion;
            } else if (this.streamVersion < DataStream.globalStreamVersion) {
                this.streamVersion = DataStream.globalStreamVersion;
                // Update stream
                foreach (var streamFactory in DataStream.streamFactory) {
                    if (!DataStream.streamMap.ContainsKey(streamFactory.Key)) {
                        DataStream.streamMap.Add(streamFactory.Key,
                            new Tuple<IData, IData>((IData)Activator.CreateInstance(streamFactory.Value),
                                (IData)Activator.CreateInstance(streamFactory.Value)));
                    }
                    // Register allowed type
                    if (!DataStream.allowedTypes.Contains(streamFactory.Key)) {
                        DataStream.allowedTypes.Add(streamFactory.Key);
                    }
                }
            }
        }


        /// <summary>
        /// Registers a stream handler for a specific data type.
        /// </summary>
        /// <typeparam name="T">The type of the stream handler.</typeparam>
        /// <param name="dataType">The data type to associate with the stream handler.</param>
        /// <param name="stream">The stream handler instance.</param>
        public void RegisterStream<T>(Type dataType, T stream) where T : IData {
            DataStream.streamMap.Add(dataType, new Tuple<IData, IData>(stream, stream));
        }

        /// <summary>
        /// Allocates a new buffer with a specified size.
        /// </summary>
        /// <param name="bufferSize">The size of the buffer to allocate.</param>
        public void Allocate(int bufferSize) {
            // Initialize a new byte array for the buffer with the given size
            this.dataBuffer     = this.DequeueBuffer(bufferSize);
            // Set the bufferSize field to the size of the new buffer
            this.bufferSize     = bufferSize;
            // Reset the bufferIndex to 0, indicating the start of the buffer
            this.bufferIndex    = 0;
        }

        /// <summary>
        /// Allocates a buffer using an existing byte array.
        /// </summary>
        /// <param name="buffer">The byte array to use as the buffer.</param>
        public void Allocate(byte[] buffer) {
            // Assign the provided byte array to the dataBuffer field
            this.dataBuffer     = buffer;
            // Set the bufferSize field to the length of the buffer, or 0 if the buffer is null
            this.bufferSize     = (buffer != null) ? buffer.Length : 0;
            // Reset the bufferIndex to 0, indicating the start of the buffer
            this.bufferIndex    = 0;
        }

        /// <summary>
        /// Retrieves the current buffer.
        /// </summary>
        /// <returns>A copy of the current buffer as a byte array.</returns>
        public byte[] GetBuffer() {
            byte[] previousBuffer = this.outputBuffer;
            // Create a new byte array to hold the result
            this.outputBuffer = this.DequeueBuffer(this.bufferSize);
            // Copy the contents of the dataBuffer into the result array
            System.Buffer.BlockCopy(this.dataBuffer, 0, this.outputBuffer, 0, this.bufferSize);
            // Return buffer to queue
            if (previousBuffer != null) {
                this.EnqueueBuffer(previousBuffer);
            }
            // Return the copied buffer
            return this.outputBuffer;            
        }

        /// <summary>
        /// Gets the current size of the buffer.
        /// </summary>
        /// <returns>The size of the buffer as an integer.</returns>
        public int GetBufferSize() {
            return this.bufferIndex; // Return the current index within the buffer, indicating the size of the used portion
        }

        /// <summary>
        /// Rewinds the buffer by a specified distance.
        /// </summary>
        /// <param name="distance">The distance to rewind within the buffer.</param>
        public void Rewind(int distance) {
            // Ensure the buffer is allocated before attempting to rewind
            this.AllocateBuffer();
            // Update the bufferIndex, clamping it to valid range to avoid out-of-bounds errors
            this.bufferIndex = Mathf.Clamp(this.bufferIndex - distance, 0, this.dataBuffer.Length - 1);
        }

        /// <summary>
        /// Rewinds the buffer by the size of a specified type.
        /// </summary>
        /// <typeparam name="T">The type used to determine the rewind distance.</typeparam>
        public void Rewind<T>() {
            // Ensure the buffer is allocated before attempting to rewind
            this.AllocateBuffer();
            Type datatype = typeof(T).IsEnum ? Enum.GetUnderlyingType(typeof(T)) : typeof(T);
            // Update the bufferIndex based on the size of the type T, clamping it to valid range
            this.bufferIndex = Mathf.Clamp(this.bufferIndex - DataUtils.SizeOfType(datatype), 0, this.dataBuffer.Length - 1);
        }

        /// <summary>
        /// Advances the buffer by a specified distance.
        /// </summary>
        /// <param name="distance">The distance to advance within the buffer.</param>
        public void Forward(int distance) {
            // Ensure the buffer is allocated before attempting to advance
            this.AllocateBuffer();
            // Update the bufferIndex, clamping it to valid range to avoid out-of-bounds errors
            this.bufferIndex = Mathf.Clamp(this.bufferIndex + distance, 0, this.dataBuffer.Length - 1);
        }

        /// <summary>
        /// Advances the buffer index forward by the size of the type T.
        /// </summary>
        /// <typeparam name="T">The type of data to calculate the size for advancing the buffer index.</typeparam>
        public void Forward<T>() {
            // Ensure the buffer is allocated before attempting to use it.
            this.AllocateBuffer();
            Type datatype = typeof(T).IsEnum ? Enum.GetUnderlyingType(typeof(T)) : typeof(T);
            // Advance the buffer index by the size of type T, clamping it to ensure it doesn't exceed the buffer bounds.
            this.bufferIndex = Mathf.Clamp(this.bufferIndex + DataUtils.SizeOfType(datatype), 0, this.dataBuffer.Length - 1);
        }

        /// <summary>
        /// Shifts the data in the buffer to the left starting from a specified index.
        /// </summary>
        /// <param name="start">The index from which to start shifting data.</param>
        /// <param name="distance">The number of positions to shift the data by.</param>
        public void ShiftLeft(int start, int distance) {
            // Ensure the buffer is allocated before attempting to use it.
            this.AllocateBuffer();
            // Shift the data in the buffer to the left by the specified distance.
            DataUtils.ShiftLeft(ref this.dataBuffer, start, distance);
            // Update the buffer index and size after the shift operation.
            this.bufferIndex -= distance;
            this.bufferSize = this.bufferIndex;
        }

        /// <summary>
        /// Shifts the data in the buffer to the right starting from a specified index.
        /// </summary>
        /// <param name="start">The index from which to start shifting data.</param>
        /// <param name="distance">The number of positions to shift the data by.</param>
        public void ShiftRight(int start, int distance) {
            // Ensure the buffer is allocated before attempting to use it.
            this.AllocateBuffer();
            // Shift the data in the buffer to the right by the specified distance.
            DataUtils.ShiftRight(ref this.dataBuffer, start, distance);
            // Update the buffer index and size after the shift operation.
            this.bufferIndex += distance;
            this.bufferSize = this.bufferIndex;
        }

        /// <summary>
        /// Reads data of type T from the buffer, with an option to rewind the buffer index before reading.
        /// </summary>
        /// <typeparam name="T">The type of data to read from the buffer.</typeparam>
        /// <param name="rewind">Whether to rewind the buffer index before reading.</param>
        /// <returns>The data read from the buffer.</returns>
        /// <exception cref="Exception">Thrown when the type T is not supported or does not have a reader.</exception>
        public T Read<T>(bool rewind = false) {
            // Check if some new stream was registered
            this.Initialize();
            // Initialize the result with the default value of type T.
            T result = default(T);
            // Check if there is a reader for type T in the stream map.
            IData reader = (DataStream.streamMap.ContainsKey(typeof(T))) ? DataStream.streamMap[typeof(T)].Item1 : null;
            //check if T is an enum type first
            if (typeof(T).IsEnum){                    
                // Optionally rewind the buffer index before reading.
                if (rewind){
                    this.Rewind<T>();
                }
                result = (T)Read(Enum.GetUnderlyingType(typeof(T)));
            }else if (reader != null){
                // Check if the reader is of the correct data reader interface for type T.
                if (reader is IDataReader<T>) {
                    // Optionally rewind the buffer index before reading.
                    if (rewind){
                        this.Rewind<T>();
                    }
                    // Read the data from the buffer using the reader.
                    result = (reader as IDataReader<T>).Read(this.dataBuffer, ref this.bufferIndex);
                } else {
                    // Throw an exception if the reader is not of the correct type.
                    throw new Exception(String.Format("[ {0} ] is not a reader", typeof(T)));
                }
            } else {
                // Throw an exception if there is no reader for type T.
                throw new Exception(String.Format("[ {0} ] is not supported", typeof(T)));
            }
            return result;
        }

        /// <summary>
        /// Reads data of type T from the buffer at a specific offset.
        /// </summary>
        /// <typeparam name="T">The type of data to read from the buffer.</typeparam>
        /// <param name="offset">The offset in the buffer from which to start reading.</param>
        /// <returns>The data read from the buffer.</returns>
        /// <exception cref="Exception">Thrown when the type T is not supported or does not have a reader.</exception>
        public T Read<T>(int offset) {
            // Check if some new stream was registered
            this.Initialize();
            // Initialize the result with the default value of type T.
            T result = default(T);

            Type dataType = typeof(T).IsEnum ? Enum.GetUnderlyingType(typeof(T)) : typeof(T);
            // Check if there is a reader for type T in the stream map.
            IData reader = (DataStream.streamMap.ContainsKey(dataType)) ? DataStream.streamMap[dataType].Item1 : null;
            if (reader != null) {
                //check if T is an enum type first 
                if (dataType.IsEnum){
                    if (reader is IReader) { 
                        result = (T)ReadByType(dataType, reader as IReader, ref offset);
                    }else{
                        // Throw an exception if the reader does not implement IReader
                        throw new Exception(String.Format("[ {0} ] is not a reader", dataType));
                    }
                }// Check if the reader is of the correct data reader interface for type T.
                else if(reader is IDataReader<T>) {
                    // Read the data from the buffer using the reader at the specified offset.
                    result = (reader as IDataReader<T>).Read(this.dataBuffer, ref offset);
                } else {
                    // Throw an exception if the reader is not of the correct type.
                    throw new Exception(String.Format("[ {0} ] is not a reader", typeof(T)));
                }
            } else {
                // Throw an exception if there is no reader for type T.
                throw new Exception(String.Format("[ {0} ] is not supported", typeof(T)));
            }
            return result;
        }


        /// <summary>
        /// Reads data of a specified type from the data buffer using a registered reader.
        /// </summary>
        /// <typeparam name="T">The type of data to read.</typeparam>
        /// <param name="dataType">The type of data to be read, used to find the appropriate reader.</param>
        /// <returns>The data read from the buffer.</returns>
        /// <exception cref="Exception">Thrown when the specified type is not supported or the reader is not found.</exception>
        public T Read<T>(Type dataType) {
            // Check if some new stream was registered
            this.Initialize();
            T result = default(T);

            // Attempt to retrieve the reader for the specified data type
            IData reader = (DataStream.streamMap.ContainsKey(dataType)) ? DataStream.streamMap[dataType].Item1 : null;
            //check if T is an enum type first
            if (typeof(T).IsEnum){
                result = (T)Read(Enum.GetUnderlyingType(typeof(T)));
            } else if (reader != null) {
                // Check if the reader implements the IReader interface
                if (reader is IReader) {
                    // Use the reader to read the data from the buffer
                    result = (reader as IReader).Read<T>(this.dataBuffer, ref this.bufferIndex, dataType);
                } else {
                    // Throw an exception if the reader does not implement IReader
                    throw new Exception(String.Format("[ {0} ] is not a reader", dataType));
            	}
            } else {
                // Throw an exception if no reader is registered for the data type
                throw new Exception(String.Format("[ {0} ] is not supported", dataType));
            }
            return result;
        }

        /// <summary>
        /// Reads data of a specified type from the stream.
        /// </summary>
        /// <param name="dataType">The type of the data to read.</param>
        /// <returns>The data read from the stream.</returns>
        public object Read(Type dataType) {
            // Check if some new stream was registered
            this.Initialize();
            object result = default(object);
            // Attempt to retrieve the reader for the specified data type
            IData reader = (DataStream.streamMap.ContainsKey(dataType)) ? DataStream.streamMap[dataType].Item1 : null;
            if (reader != null) {
                // Check if the reader implements the IReader interface
                if (reader is IReader) {
                    result = ReadByType(dataType, reader as IReader, ref this.bufferIndex);
                } else {
                    // Throw an exception if the reader does not implement IReader
                    throw new Exception(String.Format("[ {0} ] is not a reader", dataType));
                }
            } else {
                // Throw an exception if no reader is registered for the data type
                throw new Exception(String.Format("[ {0} ] is not supported", dataType));
            }
            return result;
        }

        private object ReadByType(Type dataType, IReader reader, ref int offset)
        {
            object result = default(object);
            // Use the reader to read the data from the buffer
            if (dataType == typeof(Int16))
                result = reader.Read<Int16>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(UInt16))
                result = reader.Read<UInt16>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(Int32))
                result = reader.Read<Int32>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(UInt32))
                result = reader.Read<UInt32>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(Int64))
                result = reader.Read<Int64>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(UInt64))
                result = reader.Read<UInt64>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(float))
                result = reader.Read<float>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(double))
                result = reader.Read<double>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(bool))
                result = reader.Read<bool>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(string))
                result = reader.Read<string>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(char))
                result = reader.Read<char>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(Color))
                result = reader.Read<Color>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(byte))
                result = reader.Read<byte>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(sbyte))
                result = reader.Read<sbyte>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(byte[]))
                result = reader.Read<byte[]>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(Vector2))
                result = reader.Read<Vector2>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(Vector3))
                result = reader.Read<Vector3>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(Vector4))
                result = reader.Read<Vector4>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(Matrix4x4))
                result = reader.Read<Matrix4x4>(this.dataBuffer, ref offset, dataType);
            else if (dataType == typeof(Quaternion))
                result = reader.Read<Quaternion>(this.dataBuffer, ref offset, dataType);
            else
                throw new Exception(String.Format("[ {0} ] is not a reader", dataType));

            return result;
        }

        /// <summary>
        /// Resets the internal buffer index and size, and clears the data buffer.
        /// </summary>
        public void Reset() {
            this.bufferIndex    = 0;
            this.bufferSize     = 0;
            // Clear the data buffer if it is not null
            /* It's not necessary since i'm reusinfg the buffer and all space was filled by the new process
            if (this.dataBuffer != null) {
                Array.Clear(this.dataBuffer, 0, this.dataBuffer.Length);
            }
            */
        }

        /// <summary>
        /// Writes data of a specified type to the data buffer using a registered writer.
        /// </summary>
        /// <typeparam name="T">The type of data to write.</typeparam>
        /// <param name="data">The data to be written to the buffer.</param>
        /// <exception cref="Exception">Thrown when the specified type is not supported or the writer is not found.</exception>
        public void Write<T>(T data) {
            // Check if some new stream was registered
            this.Initialize();
            // Ensure the buffer is allocated before writing
            this.AllocateBuffer();
            // Attempt to retrieve the writer for the specified data type
            // if (DataStream.streamMap.TryGetValue<T>(typeof(T))) ? DataStream.streamMap[typeof(T), out var catchedType))            
            // if (writter != null) {
            Type dataType = typeof(T).IsEnum ? Enum.GetUnderlyingType(typeof(T)) : typeof(T);
            if (DataStream.streamMap.TryGetValue(dataType, out var typeOnMap)) {
                IData writter = typeOnMap.Item2;
                
				if (typeof(T).IsEnum) {
                    Write<T>(data, dataType);
                } else if (writter is IDataWritter<T>){ // Check if the writer implements the IDataWritter<T> interface
                    // Use the writer to write the data to the buffer and update the buffer index and size
                	this.bufferIndex += (writter as IDataWritter<T>).Write(data, ref this.dataBuffer, ref this.bufferIndex);
                    this.bufferSize = this.bufferIndex;
				} else{
                	// Throw an exception if the writer does not implement IDataWritter<T>
                    throw new Exception(String.Format("[ {0} ] is not a writter", typeof(T)));
				}
            } else {
                // Throw an exception if no writer is registered for the data type
                throw new Exception(String.Format("[ {0} ] is not supported", typeof(T)));
            }
        }

        /// <summary>
        /// Writes data of a specified type to the data buffer at a specific offset using a registered writer.
        /// </summary>
        /// <typeparam name="T">The type of data to write.</typeparam>
        /// <param name="data">The data to be written to the buffer.</param>
        /// <param name="offset">The offset at which to start writing the data.</param>
        /// <exception cref="Exception">Thrown when the specified type is not supported or the writer is not found.</exception>
        public void Write<T>(T data, int offset) {
            // Check if some new stream was registered
            this.Initialize();
            // Ensure the buffer is allocated before writing
            this.AllocateBuffer();
            //check the data type of the data
            Type dataType = typeof(T).IsEnum ? Enum.GetUnderlyingType(typeof(T)) : typeof(T);

            // Attempt to retrieve the writer for the specified data type
            if (DataStream.streamMap.TryGetValue(dataType, out var typeOnMap)) {
                IData writter = typeOnMap.Item2;
                // Check if the writer implements the IDataWritter<T> interface
                if (typeof(T).IsEnum){
                    object dataToSend = Convert.ChangeType(data, dataType);
                    (writter as IWritter).Write(dataToSend, ref this.dataBuffer, ref offset, dataType);
                }else if (writter is IDataWritter<T>) {
                    // Use the writer to write the data to the buffer at the specified offset
                    (writter as IDataWritter<T>).Write(data, ref this.dataBuffer, ref offset);
                } else {
                    // Throw an exception if the writer does not implement IDataWritter<T>
                    throw new Exception(String.Format("[ {0} ] is not a writter", typeof(T)));
                }
            } else {
                // Throw an exception if no writer is registered for the data type
                throw new Exception(String.Format("[ {0} ] is not supported", typeof(T)));
            }
        }

        /// <summary>
        /// Writes data of a specified type to the data buffer using a registered writer.
        /// </summary>
        /// <typeparam name="T">The type of data to write.</typeparam>
        /// <param name="data">The data to be written to the buffer.</param>
        /// <param name="dataType">The type of data to be written, used to find the appropriate writer.</param>
        /// <exception cref="Exception">Thrown when the specified type is not supported or the writer is not found.</exception>
        public void Write<T>(T data, Type dataType) {
            // Check if some new stream was registered
            this.Initialize();
            // Ensure the buffer is allocated before writing
            this.AllocateBuffer();
            //the defual data to send
            object dataToSend = data;
            //check if T is an enum then convert it to its underlaying type
            if (typeof(T).IsEnum) {
                dataType 	= Enum.GetUnderlyingType(typeof(T));
                dataToSend 	= Convert.ChangeType(data, dataType); 
            }
            // Attempt to retrieve the writer for the specified data type
            if (DataStream.streamMap.TryGetValue(dataType, out var typeOnMap)) {
                IData writter = typeOnMap.Item2;
                // Check if the writer implements the IWritter interface
                if (writter is IWritter) {
                    // Use the writer to write the data to the buffer and update the buffer index and size
                    this.bufferIndex += (writter as IWritter).Write(dataToSend, ref this.dataBuffer, ref this.bufferIndex, dataType);
                    this.bufferSize = this.bufferIndex;
                } else {
                    // Throw an exception if the writer does not implement IWritter
                    throw new Exception(String.Format("[ {0} ] is not a writter", dataType));
                }
            } else {
                // Throw an exception if no writer is registered for the data type
                throw new Exception(String.Format("[ {0} ] is not supported", dataType));
            }
        }

        /// <summary>
        /// Return this this type is a valid Stream type
        /// </summary>
        /// <typeparam name="T">The type of data to check.</typeparam>
        /// <returns>true is a valid stream type, false otherwise</returns>
        public bool IsValidType<T>() {
            // Check if some new stream was registered
            this.Initialize();
            return DataStream.allowedTypes.Contains(typeof(T));
        }

        /// <summary>
        /// Return this this type is a valid Stream type
        /// </summary>
        /// <param name="dataType">The type of data to check.</param>
        /// <returns>true is a valid stream type, false otherwise</returns>
        public bool IsValidType(Type dataType) {
            // Check if some new stream was registered
            this.Initialize();
            return DataStream.allowedTypes.Contains(dataType);
        }

        /// <summary>
        /// Return associated type on index
        /// </summary>
        /// <param name="index">Index type</param>
        /// <returns>Type on specified index</returns>
        public Type GetType(ushort index) {
            // Check if some new stream was registered
            this.Initialize();
            return DataStream.allowedTypes[index];
        }

        /// <summary>
        /// Return index of type on factory map
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Index of specified type</returns>
        public ushort GetTypeIndex(Type type) {
            // Check if some new stream was registered
            this.Initialize();
            return (ushort)DataStream.allowedTypes.IndexOf(type);
        }

        /// <summary>
        /// Return index of type on factory map
        /// </summary>
        /// <typeparam name="T">Type to check</typeparam>
        /// <returns>Index of specified type</returns>
        public ushort GetTypeIndex<T>() {
            // Check if some new stream was registered
            this.Initialize();
            return (ushort)DataStream.allowedTypes.IndexOf(typeof(T));
        }

        /// <summary>
        /// Allocates the data buffer if automatic allocation is enabled and the buffer is not already allocated.
        /// </summary>
        private void AllocateBuffer() {
            if (this.autoAllocate) {
                if (this.dataBuffer == null) {
                    // Allocate the buffer with the default size
                    this.Allocate(DEFAULT_BUFFER_SIZE); // TODO: get buffer from a pooling and release on dispose
                }
            }
        }

        private byte[] DequeueBuffer(int bufferSize) {
            byte[] result = null;
            lock (DataStream.poolingBuffer) {
                if (DataStream.poolingBuffer.TryGetValue(bufferSize, out var tempData) == false) {
                    DataStream.poolingBuffer.Add(bufferSize, new List<byte[]>());
                    DataStream.poolingBuffer[bufferSize].Add(new byte[bufferSize]);
                } else if (DataStream.poolingBuffer[bufferSize].Count == 0) {
                    DataStream.poolingBuffer[bufferSize].Add(new byte[bufferSize]);
                }
                result = DataStream.poolingBuffer[bufferSize][0];
                DataStream.poolingBuffer[bufferSize].RemoveAt(0);
            }
            return result;
        }

        private void EnqueueBuffer(byte[] buffer) {
            if (buffer != null) {
                lock (DataStream.poolingBuffer) {
                    DataStream.poolingBuffer[buffer.Length].Add(buffer);
                }
            }
        }

        /// <summary>
        /// Retrieves the client associated with this instance.
        /// </summary>
        /// <returns>The client associated with this instance.</returns>
        public IClient GetClient() {
            return this.client;
        }

    }

}