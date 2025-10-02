using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A generic data handler class that provides serialization and deserialization functionality for various data types.
    /// It implements both IDataWritter and IDataReader interfaces for writing and reading data respectively.
    /// </summary>
    /// <typeparam name="T">The type of data to handle.</typeparam>
    public class DataHandler<T> : IDataWritter<T>, IDataReader<T> {

        /// <summary>
        /// Writes data of type T to a buffer at a specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data to.</param>
        /// <param name="offset">The offset in the buffer at which to start writing.</param>
        /// <returns>The number of bytes written to the buffer.</returns>        
        public virtual int Write(T data, ref byte[] buffer, ref int offset) {
            return this.WriteData(data, ref buffer, ref offset, typeof(T));
        }

        /// <summary>
        /// Writes data of a specified type to a buffer at a specified offset.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="buffer">The buffer to write the data to.</param>
        /// <param name="offset">The offset in the buffer at which to start writing.</param>
        /// <param name="dataType">The type of the data to write.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        public virtual int Write(object data, ref byte[] buffer, ref int offset, Type dataType) {
            return this.WriteData(data, ref buffer, ref offset, dataType);
        }

     

        /// <summary>
        /// Reads data of type T from a buffer at a specified offset.
        /// </summary>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="offset">The offset in the buffer at which to start reading.</param>
        /// <returns>The data read from the buffer.</returns>
        public virtual T Read(byte[] buffer, ref int offset) {
            return this.ReadData(buffer, ref offset, typeof(T));
        }

        /// <summary>
        /// Reads data of a specified type from a buffer at a specified offset.
        /// </summary>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="offset">The offset in the buffer at which to start reading.</param>
        /// <param name="dataType">The type of the data to read.</param>
        /// <returns>The data read from the buffer.</returns>
        public virtual T Read(byte[] buffer, ref int offset, Type dataType) {
            return this.ReadData(buffer, ref offset, dataType);
        }

        /// <summary>
        /// Reads data of type E from a buffer at a specified offset.
        /// </summary>
        /// <typeparam name="E">The type of data to read.</typeparam>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="offset">The offset in the buffer at which to start reading.</param>
        /// <returns>The data read from the buffer.</returns>
        public virtual E Read<E>(byte[] buffer, ref int offset) {
            return (E)this.ReadData<E>(buffer, ref offset, typeof(E));
        }

        /// <summary>
        /// Reads data of type E from a buffer at a specified offset, using a specified data type.
        /// </summary>
        /// <typeparam name="E">The type of data to read.</typeparam>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="offset">The offset in the buffer at which to start reading.</param>
        /// <param name="dataType">The type of the data to read.</param>
        /// <returns>The data read from the buffer.</returns>
        public virtual E Read<E>(byte[] buffer, ref int offset, Type dataType) {
            return (E)this.ReadData<E>(buffer, ref offset, dataType);
        }

        /// <summary>
        /// Writes the given data into the buffer at the specified offset and updates the offset.
        /// </summary>
        /// <param name="data">The data to write into the buffer.</param>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The current offset in the buffer. This will be updated after writing.</param>
        /// <param name="dataType">The type of the data to be written.</param>
        /// <returns>The size of the data written to the buffer.</returns>
        /// <exception cref="Exception">Thrown when the buffer does not have enough space to write the data.</exception>
        private int WriteData<E>(E data, ref byte[] buffer, ref int offset, Type dataType) {
            int writeSize = 0;
            if ((offset + DataUtils.SizeOfType(dataType)) <= buffer.Length) {
                writeSize   += this.CopyDataToBuffer(ref buffer, offset + writeSize, dataType, data);
                offset      += writeSize;
            } else {
                throw new Exception(String.Format("Buffer has no avaiable space to write \"{0}\", required {1} avaiable {2}", dataType.Name, (offset + DataUtils.SizeOfType(dataType)), buffer.Length));
            }
            return writeSize;
        }

        /// <summary>
        /// Reads data of a specified type from the buffer at the given offset and updates the offset.
        /// </summary>
        /// <typeparam name="T">The type of the data to be read.</typeparam>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="offset">The current offset in the buffer. This will be updated after reading.</param>
        /// <param name="dataType">The type of the data to be read.</param>
        /// <returns>The data read from the buffer.</returns>
        /// <exception cref="Exception">Thrown when the buffer does not have enough space to read the data.</exception>
        private T ReadData(byte[] buffer, ref int offset, Type dataType) {
            T result = default(T);
            if ((offset + DataUtils.SizeOfType(dataType)) <= buffer.Length) {
                if (dataType == typeof(string)) {
                    // first i need to read size
                    int totalOfBytesOnString = this.ReadData<int>(buffer, ref offset, typeof(int));
                    // Now read total of bytes
                    byte[] objectBytes = new byte[totalOfBytesOnString];
                    for (int index = 0; index < objectBytes.Length; index++) {
                        objectBytes[index] = buffer[offset++];
                    }
                    result = (T)this.FromByteArray(objectBytes);
                } else if (dataType == typeof(byte[])) {
                    // first i need to read size
                    int totalOfBytesOnArray = this.ReadData<int>(buffer, ref offset, typeof(int));
                    // Now read total of bytes
                    byte[] objectBytes = new byte[totalOfBytesOnArray];
                    for (int index = 0; index < objectBytes.Length; index++) {
                        objectBytes[index] = buffer[offset++];
                    }
                    result = (T)this.FromByteArray(objectBytes);
                } else {
                    byte[] objectBytes = new byte[DataUtils.SizeOfType(dataType)];
                    for (int index = 0; index < objectBytes.Length; index++) {
                        objectBytes[index] = buffer[offset++];
                    }
                    result = (T)this.FromByteArray(objectBytes);
                }
            } else {
                throw new Exception(String.Format("Buffer has no avaiable space to read \"{0}\", required {1} avaiable {2}", dataType.Name, (offset + DataUtils.SizeOfType(dataType)), buffer.Length));
            }
            return result;
        }

        /// <summary>
        /// Reads data of a specified type from a buffer at a given offset.
        /// </summary>
        /// <typeparam name="T">The type of the data to read.</typeparam>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="offset">The offset at which to start reading.</param>
        /// <param name="dataType">The type of the data to read.</param>
        /// <returns>The data read from the buffer.</returns>
        /// <exception cref="Exception">Thrown when the buffer does not have enough space to read the data.</exception>
        private E ReadData<E>(byte[] buffer, ref int offset, Type dataType) {
            E result = default(E);
            if ((offset + DataUtils.SizeOfType(dataType)) <= buffer.Length) {
                if (dataType == typeof(string)) {
                    // first i need to read size
                    int totalOfBytesOnString = this.ReadData<int>(buffer, ref offset, typeof(int));
                    // Now read total of bytes
                    byte[] objectBytes = new byte[totalOfBytesOnString];
                    for (int index = 0; index < objectBytes.Length; index++) {
                        objectBytes[index] = buffer[offset++];
                    }
                    result = this.FromByteArray<E>(objectBytes, dataType);
                } else if (dataType == typeof(byte[])) {
                    // first i need to read size
                    int totalOfBytesOnArray = this.ReadData<int>(buffer, ref offset, typeof(int));
                    // Now read total of bytes
                    byte[] objectBytes = new byte[totalOfBytesOnArray];
                    for (int index = 0; index < objectBytes.Length; index++) {
                        objectBytes[index] = buffer[offset++];
                    }
                    result = this.FromByteArray<E>(objectBytes, dataType);
                } else {
                    byte[] objectBytes = new byte[DataUtils.SizeOfType(dataType)];
                    for ( int index = 0; index < objectBytes.Length; index++) {
                        objectBytes[index] = buffer[offset++];
                    }
                    result = this.FromByteArray<E>(objectBytes, dataType);
                }
            } else {
                throw new Exception(String.Format("Buffer has no avaiable space to read \"{0}\", required {1} avaiable {2}", dataType.Name, (offset + DataUtils.SizeOfType(dataType)), buffer.Length));
            }
            return result;
        }

        /// <summary>
        /// Converts an object to its byte array representation.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>A byte array representing the object.</returns>
        private int CopyDataToBuffer(ref byte[] buffer, int offset, Type dataType, object obj) {
           int result = 0;
           if (obj == null) {
               return 0;
           }
                if (dataType == typeof(int))        result = this.CopyToBuffer          (ref buffer, offset, (int)obj);
           else if (dataType == typeof(uint))       result = this.CopyToBuffer          (ref buffer, offset, (uint)obj);
           else if (dataType == typeof(ushort))     result = this.CopyToBuffer          (ref buffer, offset, (ushort)obj);
           else if (dataType == typeof(short))      result = this.CopyToBuffer          (ref buffer, offset, (short)obj);
           else if (dataType == typeof(float))      result = this.CopyToBuffer          (ref buffer, offset, (float)obj);
           else if (dataType == typeof(long))       result = this.CopyToBuffer          (ref buffer, offset, (long)obj);
           else if (dataType == typeof(ulong))      result = this.CopyToBuffer          (ref buffer, offset, (ulong)obj);
           else if (dataType == typeof(double))     result = this.CopyToBuffer          (ref buffer, offset, (double)obj);
           else if (dataType == typeof(char))       result = this.CopyToBuffer          (ref buffer, offset, (char)obj);
           else if (dataType == typeof(bool))       result = this.CopyToBuffer          (ref buffer, offset, (bool)obj);
           else if (dataType == typeof(string))     result = this.CopyToBuffer          (ref buffer, offset, Encoding.UTF8.GetBytes((obj as string)));
           else if (dataType == typeof(sbyte))      result = this.CopyToBuffer          (ref buffer, offset, (sbyte)obj);
           else if (dataType == typeof(byte))       result = this.CopyToBuffer          (ref buffer, offset, (byte)obj);
           else if (dataType == typeof(byte[]))     result = this.CopyToBuffer          (ref buffer, offset, (byte[])obj);
           else if (dataType == typeof(Vector2))    result = this.CopyVector2ToBuffer   (ref buffer, offset, (Vector2)obj);
           else if (dataType == typeof(Vector3))    result = this.CopyVector3ToBuffer   (ref buffer, offset, (Vector3)obj);
           else if (dataType == typeof(Vector4))    result = this.CopyVector4ToBuffer   (ref buffer, offset, (Vector4)obj);
           else if (dataType == typeof(Matrix4x4))  result = this.CopyMatrix4x4ToBuffer (ref buffer, offset, (Matrix4x4)obj);
           else if (dataType == typeof(Quaternion)) result = this.CopyQuaternionToBuffer(ref buffer, offset, (Quaternion)obj);
           else if (dataType == typeof(Color))      result = this.CopyColorToBuffer     (ref buffer, offset, (Color)obj);

           return result;
       }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bufer">Buffer to fill</param>
        /// <param name="offset">Buffer offset</param>
        /// <param name="value">Vallue to insert into the buffer</param>
        /// <returns>Amount of inserted data</returns>
        private int CopyToBuffer(ref byte[] bufer, int offset, uint value) {
            MemoryMarshal.Write<uint>(bufer.AsSpan(offset), ref value);
            return sizeof(uint) / sizeof(byte);
        }

        private int CopyToBuffer(ref byte[] bufer, int offset, int value) {
            MemoryMarshal.Write<int>(bufer.AsSpan(offset), ref value);
            return sizeof(int) / sizeof(byte);
        }

        private int CopyToBuffer(ref byte[] bufer, int offset, ushort value) {
            MemoryMarshal.Write<ushort>(bufer.AsSpan(offset), ref value);
            return sizeof(ushort) / sizeof(byte);
        }

        private int CopyToBuffer(ref byte[] bufer, int offset, short value) {
            MemoryMarshal.Write<short>(bufer.AsSpan(offset), ref value);
            return sizeof(short) / sizeof(byte);
        }


        private int CopyToBuffer(ref byte[] bufer, int offset, float value) {
            MemoryMarshal.Write<float>(bufer.AsSpan(offset), ref value);
            return sizeof(float) / sizeof(byte);
        }

        private int CopyToBuffer(ref byte[] bufer, int offset, long value) {
            MemoryMarshal.Write<long>(bufer.AsSpan(offset), ref value);
            return sizeof(long) / sizeof(byte);
        }

        private int CopyToBuffer(ref byte[] bufer, int offset, ulong value) {
            MemoryMarshal.Write<ulong>(bufer.AsSpan(offset), ref value);
            return sizeof(ulong) / sizeof(byte);
        }

        private int CopyToBuffer(ref byte[] bufer, int offset, double value) {
            MemoryMarshal.Write<double>(bufer.AsSpan(offset), ref value);
            return sizeof(double) / sizeof(byte);
        }

        private int CopyToBuffer(ref byte[] bufer, int offset, char value) {
            MemoryMarshal.Write<char>(bufer.AsSpan(offset), ref value);
            return sizeof(char) / sizeof(byte);
        }

        private int CopyToBuffer(ref byte[] bufer, int offset, bool value) {
            MemoryMarshal.Write<bool>(bufer.AsSpan(offset), ref value);
            return sizeof(bool) / sizeof(byte);
        }

        private int CopyToBuffer(ref byte[] bufer, int offset, byte value) {
            MemoryMarshal.Write<byte>(bufer.AsSpan(offset), ref value);
            return sizeof(byte) / sizeof(byte);
        }

        private int CopyToBuffer(ref byte[] bufer, int offset, sbyte value) {
            MemoryMarshal.Write<sbyte>(bufer.AsSpan(offset), ref value);
            return sizeof(sbyte) / sizeof(byte);
        }

        private int CopyToBuffer(ref byte[] bufer, int offset, byte[] value) {
            int realOffset = offset;
            // Write buffer size
           
            int bufferLength = value.Length;
            MemoryMarshal.Write<int>(bufer.AsSpan(realOffset), ref bufferLength);
            realOffset += sizeof(int) / sizeof(byte);

            // Write bytes
            for (int bufferIndex = 0; bufferIndex < value.Length; bufferIndex++) {
                MemoryMarshal.Write<byte>(bufer.AsSpan(realOffset), ref value[bufferIndex]);
                realOffset++;
            }
            return (realOffset - offset);
        }

        private int CopyVector2ToBuffer(ref byte[] bufer, int offset, Vector2 data) {
            int realOffset = offset;
            MemoryMarshal.Write(bufer.AsSpan(realOffset), ref data.x);
            realOffset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(bufer.AsSpan(realOffset), ref data.y);
            realOffset += sizeof(float) / sizeof(byte);
            return (realOffset - offset);
        }

        private int CopyVector3ToBuffer(ref byte[] bufer, int offset, Vector3 data) {
            int realOffset = offset;
            
            MemoryMarshal.Write(bufer.AsSpan(realOffset), ref data.x);
            realOffset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(bufer.AsSpan(realOffset), ref data.y);
            realOffset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(bufer.AsSpan(realOffset), ref data.z);
            realOffset += sizeof(float) / sizeof(byte);

            return (realOffset - offset);
        }
        private int CopyVector4ToBuffer(ref byte[] bufer, int offset, Vector4 data)
        {
            int realOffset = offset;

            MemoryMarshal.Write(bufer.AsSpan(realOffset), ref data.x);
            realOffset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(bufer.AsSpan(realOffset), ref data.y);
            realOffset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(bufer.AsSpan(realOffset), ref data.z);
            realOffset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(bufer.AsSpan(realOffset), ref data.w);
            realOffset += sizeof(float) / sizeof(byte);

            return (realOffset - offset);
        }

        private int CopyMatrix4x4ToBuffer(ref byte[] bufer, int offset, Matrix4x4 data)
        {
            int realOffset = offset;

            MemoryMarshal.Write(bufer.AsSpan(realOffset), ref data);
            realOffset += sizeof(float) * 16 / sizeof(byte);

            return (realOffset - offset);
        }

        private int CopyQuaternionToBuffer(ref byte[] bufer, int offset, Quaternion data) {
            int realOffset = offset;

            MemoryMarshal.Write(bufer.AsSpan(realOffset), ref data.x);
            realOffset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(bufer.AsSpan(realOffset), ref data.y);
            realOffset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(bufer.AsSpan(realOffset), ref data.z);
            realOffset += sizeof(float) / sizeof(byte);
            MemoryMarshal.Write(bufer.AsSpan(realOffset), ref data.w);
            realOffset += sizeof(float) / sizeof(byte);

            return (realOffset - offset);
        }
        private int CopyColorToBuffer(ref byte[] bufer, int offset, Color data) {
            byte r = (byte)(data.r * 255);
            byte g = (byte)(data.g * 255);
            byte b = (byte)(data.b * 255);
            byte a = (byte)(data.a * 255);
            int realOffset = offset;

            MemoryMarshal.Write<byte>(bufer.AsSpan(realOffset), ref r);
            realOffset += sizeof(byte) / sizeof(byte);
            MemoryMarshal.Write<byte>(bufer.AsSpan(realOffset), ref g);
            realOffset += sizeof(byte) / sizeof(byte);
            MemoryMarshal.Write<byte>(bufer.AsSpan(realOffset), ref b);
            realOffset += sizeof(byte) / sizeof(byte);
            MemoryMarshal.Write<byte>(bufer.AsSpan(realOffset), ref a);
            realOffset += sizeof(byte) / sizeof(byte);

            return (realOffset - offset);
        }


        /// <summary>
        /// Converts a byte array to an object of a specified type.
        /// </summary>
        /// <param name="data">The byte array to convert.</param>
        /// <returns>An object of type T represented by the byte array.</returns>
        private object FromByteArray(byte[] data) {
            object result = default(object);
            if (data == null) {
                return default(T);
            }            
                 if ((typeof(T)) == typeof(int))        result = BytesToInt(data);
            else if ((typeof(T)) == typeof(uint))       result = BytesToUInt(data);
            else if ((typeof(T)) == typeof(ushort))     result = BytesToUShort(data);
            else if ((typeof(T)) == typeof(float))      result = BytesToFloat(data);
            else if ((typeof(T)) == typeof(long))       result = BytesToLong(data);
            else if ((typeof(T)) == typeof(ulong))      result = BytesToULong(data);
            else if ((typeof(T)) == typeof(short))      result = BytesToShort(data);
            else if ((typeof(T)) == typeof(double))     result = BytesToDouble(data);
            else if ((typeof(T)) == typeof(char))       result = BytesToChar(data);
            else if ((typeof(T)) == typeof(bool))       result = BytesToBoolean(data);
            else if ((typeof(T)) == typeof(string))     result = Encoding.UTF8.GetString(data);
            else if ((typeof(T)) == typeof(sbyte))      result = (sbyte)data[0];
            else if ((typeof(T)) == typeof(byte))       result = data[0];
            else if ((typeof(T)) == typeof(byte[]))     result = data;
            else if ((typeof(T)) == typeof(Vector2))    result = this.BytesToVector2(data);
            else if ((typeof(T)) == typeof(Vector3))    result = this.BytesToVector3(data);
            else if ((typeof(T)) == typeof(Vector4))    result = this.BytesToVector4(data);
            else if ((typeof(T)) == typeof(Matrix4x4))  result = this.BytesToMatrix4x4(data);
            else if ((typeof(T)) == typeof(Quaternion)) result = this.BytesToQuaternion(data);
            else if ((typeof(T)) == typeof(Color))      result = this.BytesToColor(data);

            return result;
        }

        /// <summary>
        /// Converts a byte array to an object of a specified type.
        /// </summary>
        /// <typeparam name="E">The type of the object to convert to.</typeparam>
        /// <param name="data">The byte array to convert.</param>
        /// <returns>An object of type E represented by the byte array.</returns>
        private E FromByteArray<E>(byte[] data) {
            object result = default(object);
            if (data == null) {
                return default(E);
            }            
                 if ((typeof(E)) == typeof(int))        result = BytesToInt(data);
            else if ((typeof(E)) == typeof(uint))       result = BytesToUInt(data);
            else if ((typeof(E)) == typeof(ushort))     result = BytesToUShort(data);
            else if ((typeof(E)) == typeof(float))      result = BytesToFloat(data);
            else if ((typeof(E)) == typeof(long))       result = BytesToLong(data);
            else if ((typeof(E)) == typeof(ulong))      result = BytesToULong(data);
            else if ((typeof(E)) == typeof(short))      result = BytesToShort(data);
            else if ((typeof(E)) == typeof(double))     result = BytesToDouble(data);
            else if ((typeof(E)) == typeof(char))       result = BytesToChar(data);
            else if ((typeof(E)) == typeof(bool))       result = BytesToBoolean(data);
            else if ((typeof(E)) == typeof(string))     result = Encoding.UTF8.GetString(data);
            else if ((typeof(E)) == typeof(sbyte))      result = (sbyte)data[0];
            else if ((typeof(E)) == typeof(byte))       result = data[0];
            else if ((typeof(E)) == typeof(byte[]))     result = data;
            else if ((typeof(E)) == typeof(Vector2))    result = this.BytesToVector2(data);
            else if ((typeof(E)) == typeof(Vector3))    result = this.BytesToVector3(data);
            else if ((typeof(E)) == typeof(Vector4))    result = this.BytesToVector4(data);
            else if ((typeof(E)) == typeof(Matrix4x4))  result = this.BytesToMatrix4x4(data);
            else if ((typeof(E)) == typeof(Quaternion)) result = this.BytesToQuaternion(data);
            else if ((typeof(E)) == typeof(Color))      result = this.BytesToColor(data);

            return (E)result;
        }
        /// <summary>
        /// Converts a byte array to an object of a specified type.
        /// </summary>
        /// <typeparam name="E">The type of the object to convert to.</typeparam>
        /// <param name="data">The byte array to convert.</param>
        /// <param name="dataType">The type to convert the byte array to.</param>
        /// <returns>An object of type E represented by the byte array.</returns>
        private E FromByteArray<E>(byte[] data, Type dataType) {
            object result = default(object);
            if (data == null) {
                return default(E);
            }            
                 if (dataType == typeof(int))        result = BytesToInt(data);
            else if (dataType == typeof(uint))       result = BytesToUInt(data);
            else if (dataType == typeof(ushort))     result = BytesToUShort(data);
            else if (dataType == typeof(float))      result = BytesToFloat(data);
            else if (dataType == typeof(long))       result = BytesToLong(data);
            else if (dataType == typeof(ulong))      result = BytesToULong(data);
            else if (dataType == typeof(short))      result = BytesToShort(data);
            else if (dataType == typeof(double))     result = BytesToDouble(data);
            else if (dataType == typeof(char))       result = BytesToChar(data);
            else if (dataType == typeof(bool))       result = BytesToBoolean(data);
            else if (dataType == typeof(string))     result = Encoding.UTF8.GetString(data);
            else if (dataType == typeof(sbyte))      result = (sbyte)data[0];
            else if (dataType == typeof(byte))       result = data[0];
            else if (dataType == typeof(byte[]))     result = data;
            else if (dataType == typeof(Vector2))    result = this.BytesToVector2(data);
            else if (dataType == typeof(Vector3))    result = this.BytesToVector3(data);
            else if (dataType == typeof(Vector4))    result = this.BytesToVector4(data);
            else if (dataType == typeof(Matrix4x4))  result = this.BytesToMatrix4x4(data);
            else if (dataType == typeof(Quaternion)) result = this.BytesToQuaternion(data);
            else if (dataType == typeof(Color))      result = this.BytesToColor(data);

            return (E)result;
        }

        private int BytesToInt(byte[] data){
            int offset = 0;
            int value = MemoryMarshal.Read<int>(data.AsSpan().Slice(offset));
            return value;
        }

        private uint BytesToUInt(byte[] data){
            int offset = 0;
            uint value = MemoryMarshal.Read<uint>(data.AsSpan().Slice(offset));
            return value;
        }
        private short BytesToShort(byte[] data){
            int offset = 0;
            short value = MemoryMarshal.Read<short>(data.AsSpan().Slice(offset));
            return value;
        }

        private ushort BytesToUShort(byte[] data){
            int offset = 0;
            ushort value = MemoryMarshal.Read<ushort>(data.AsSpan().Slice(offset));
            return value;
        }

        private float BytesToFloat(byte[] data){
            int offset = 0;
            float value = MemoryMarshal.Read<float>(data.AsSpan().Slice(offset));
            return value;
        }

        private double BytesToDouble(byte[] data){
            int offset = 0;
            double value = MemoryMarshal.Read<double>(data.AsSpan().Slice(offset));
            return value;
        }
        
        private long BytesToLong(byte[] data){
            int offset = 0;
            long value = MemoryMarshal.Read<long>(data.AsSpan().Slice(offset));
            return value;
        }
        private ulong BytesToULong(byte[] data){
            int offset = 0;
            ulong value = MemoryMarshal.Read<ulong>(data.AsSpan().Slice(offset));
            return value;
        }   

        private char BytesToChar(byte[] data){
            int offset = 0;
            char value = MemoryMarshal.Read<char>(data.AsSpan().Slice(offset));
            return value;
        }

        private bool BytesToBoolean(byte[] data){
            int offset = 0;
            bool value = MemoryMarshal.Read<bool>(data.AsSpan().Slice(offset));
            return value;
        }
        private Vector2 BytesToVector2(byte[] data) {
            int offset = 0;
            // Read the x component of the Vector2 from the buffer and update the offset
            float x = MemoryMarshal.Read<float>(data.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the y component of the Vector2 from the buffer and update the offset
            float y = MemoryMarshal.Read<float>(data.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);

            return new Vector2(x, y);
        }



        private Vector3 BytesToVector3(byte[] data) {
            int offset = 0;
            // Read the x component of the Vector3 from the buffer and update the offset
            float x = MemoryMarshal.Read<float>(data.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the y component of the Vector3 from the buffer and update the offset
            float y = MemoryMarshal.Read<float>(data.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the z component of the Vector3 from the buffer and update the offset
            float z = MemoryMarshal.Read<float>(data.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);

            return new Vector3(x, y, z);
        }
        private Vector4 BytesToVector4(byte[] data)
        {
            int offset = 0;
            // Read the x component of the Vector4 from the buffer and update the offset
            float x = MemoryMarshal.Read<float>(data.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the y component of the Vector4 from the buffer and update the offset
            float y = MemoryMarshal.Read<float>(data.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the z component of the Vector4 from the buffer and update the offset
            float z = MemoryMarshal.Read<float>(data.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the w component of the Vector4 from the buffer and update the offset
            float w = MemoryMarshal.Read<float>(data.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);

            return new Vector4(x, y, z, w);
        }
        private Matrix4x4 BytesToMatrix4x4(byte[] data)
        {
            int offset = 0;
            // Read the x component of the Vector4 from the buffer and update the offset
            Matrix4x4 vector = MemoryMarshal.Read<Matrix4x4>(data.AsSpan().Slice(offset));
            return vector;
        }

        private Quaternion BytesToQuaternion(byte[] data) {
            int offset = 0;
            // Read the x component of the Vector2 from the buffer and update the offset
            float x = MemoryMarshal.Read<float>(data.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the y component of the Vector2 from the buffer and update the offset
            float y = MemoryMarshal.Read<float>(data.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the z component of the Vector2 from the buffer and update the offset
            float z = MemoryMarshal.Read<float>(data.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);
            // Read the z component of the Vector2 from the buffer and update the offset
            float w = MemoryMarshal.Read<float>(data.AsSpan().Slice(offset));
            offset += sizeof(float) / sizeof(byte);

            return new Quaternion(x, y, z, w);
        }

        private Color BytesToColor(byte[] data)
        {
            int offset = 0;
            byte r = MemoryMarshal.Read<byte>(data.AsSpan().Slice(offset));
            offset += sizeof(byte) / sizeof(byte);
            byte g = MemoryMarshal.Read<byte>(data.AsSpan().Slice(offset));
            offset += sizeof(byte) / sizeof(byte);
            byte b = MemoryMarshal.Read<byte>(data.AsSpan().Slice(offset));
            offset += sizeof(byte) / sizeof(byte);
            byte a = MemoryMarshal.Read<byte>(data.AsSpan().Slice(offset));
            offset += sizeof(byte) / sizeof(byte);

            // Extract the RGBA components and create a new Color object.
            Color color = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
            // Return the Color object.
            return color;
        }
    }
}