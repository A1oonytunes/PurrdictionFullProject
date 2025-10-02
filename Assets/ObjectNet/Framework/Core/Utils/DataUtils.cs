using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Utility class for data manipulation operations.
    /// </summary>
    public static class DataUtils {

        /// <summary>
        // A dictionary to cache the sizes of different types.
        /// <summary>
        static Dictionary<Type, Int32> sizesMap = new Dictionary<Type, int>();

        /// <summary>
        /// Store string sizeof
        /// </summary>
        const int STRING_SIZEOF = 1;

        /// <summary>
        /// Store sizeof of byte array
        /// </summary>
        const int BYTE_ARRAY_SIZEOF = 1;

        /// <summary>
        /// Display a friendly message to this user
        /// </summary>
        const string NOT_SUPPORTED_TYPE_EXCEPTION = "The type [{0}] is not natively supported, use \"DataUtils.RegisterSupportedType\" method to enable support for this type";
        /// <summary>
        /// Shifts the bits in an array of bytes to the left starting from a specified index.
        /// </summary>
        /// <param name="bytes">The byte array to shift.</param>
        /// <param name="start">The index from which to start shifting.</param>
        /// <param name="distance">The number of positions to shift the bits to the left.</param>
        public static void ShiftLeft(ref byte[] bytes, int start, int distance) {
            for (int index = start; index < bytes.Length; index++) {
                // Clamp the index to ensure it stays within the bounds of the array.
                bytes[Math.Clamp(index - distance, 0, bytes.Length - 1)] = bytes[Math.Clamp(index, 0, bytes.Length - 1)];
            }
        }

        /// <summary>
        /// Shifts the bits in an array of bytes to the right starting from a specified index.
        /// </summary>
        /// <param name="bytes">The byte array to shift.</param>
        /// <param name="start">The index from which to start shifting.</param>
        /// <param name="distance">The number of positions to shift the bits to the right.</param>
        public static void ShiftRight(ref byte[] bytes, int start, int distance) {
            for (int index = bytes.Length - (distance + 1); index >= start; index--) {
                // Clamp the index to ensure it stays within the bounds of the array.
                bytes[Math.Clamp(index + distance, 0, bytes.Length - 1)] = bytes[Math.Clamp(index, 0, bytes.Length - 1)];
            }
            // Set the remaining bytes to zero after the shift.
            for (int index = start; index < distance; index++) {
                bytes[Math.Clamp(index, 0, bytes.Length - 1)] = 0;
            }
        }

        /// <summary>
        /// Retrieves the size of a given type in bytes. If the size is not already cached, it computes and caches it.
        /// </summary>
        /// <param name="type">The type to determine the size of.</param>
        /// <returns>The size of the type in bytes.</returns>
        public static int SizeOfType(Type type) {
            lock (DataUtils.sizesMap) {
                if (DataUtils.sizesMap.Count == 0) {
                    DataUtils.PopulateSizeOfType();
                }
                // Check if the size is already cached.
                if (!DataUtils.sizesMap.ContainsKey(type)) {
                    try {
                        // Dynamically generate a method to compute the size of the type.
                        var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { });
                        ILGenerator il = dm.GetILGenerator();
                        il.Emit(OpCodes.Sizeof, type);
                        il.Emit(OpCodes.Ret);
                        // Cache the computed size.
                        DataUtils.sizesMap.Add(type, (int)dm.Invoke(null, null));                        
                    } catch {
                        throw new Exception(String.Format(NOT_SUPPORTED_TYPE_EXCEPTION, type.Name));
                    }
                }
            }
            // Return the cached size.
            return DataUtils.sizesMap[type];            
        }

        /// <summary>
        /// Populate types dictionaty
        /// </summary>
        private static void PopulateSizeOfType() {
            lock (DataUtils.sizesMap) {
                DataUtils.sizesMap.Add(typeof(string),      STRING_SIZEOF);
                DataUtils.sizesMap.Add(typeof(byte[]),      BYTE_ARRAY_SIZEOF);
                DataUtils.sizesMap.Add(typeof(bool),        sizeof(bool));
                DataUtils.sizesMap.Add(typeof(int),         sizeof(int));
                DataUtils.sizesMap.Add(typeof(uint),        sizeof(uint));
                DataUtils.sizesMap.Add(typeof(short),       sizeof(short));
                DataUtils.sizesMap.Add(typeof(ushort),      sizeof(ushort));
                DataUtils.sizesMap.Add(typeof(long),        sizeof(long));
                DataUtils.sizesMap.Add(typeof(ulong),       sizeof(ulong));
                DataUtils.sizesMap.Add(typeof(sbyte),       sizeof(sbyte));
                DataUtils.sizesMap.Add(typeof(byte),        sizeof(byte));
                DataUtils.sizesMap.Add(typeof(char),        sizeof(char));
                DataUtils.sizesMap.Add(typeof(float),       sizeof(float));
                DataUtils.sizesMap.Add(typeof(double),      sizeof(double));
                DataUtils.sizesMap.Add(typeof(Vector2),     sizeof(float) + sizeof(float));
                DataUtils.sizesMap.Add(typeof(Vector3),     sizeof(float) + sizeof(float) + sizeof(float));
                DataUtils.sizesMap.Add(typeof(Vector4),     sizeof(float) + sizeof(float) + sizeof(float) + sizeof(float));
                DataUtils.sizesMap.Add(typeof(Quaternion),  sizeof(float) + sizeof(float) + sizeof(float) + sizeof(float));
                DataUtils.sizesMap.Add(typeof(Color),       sizeof(float));
                DataUtils.sizesMap.Add(typeof(Matrix4x4),   sizeof(float) * 16);
            }
        }

        /// <summary>
        /// Register any type not nativelly supported
        /// 
        /// Note: Use "typeof(yourtype)" on type parameter
        /// </summary>
        /// <param name="type">Type to register</param>
        /// <param name="size">Size of type</param>        
        public static void RegisterSupportedType(Type type, int size) {
            lock (DataUtils.sizesMap) {
                DataUtils.sizesMap.Add(type, size);
            }
        }

        /// <summary>
        /// Return if this type is supported
        /// 
        /// Note: Use "DataUtils.RegisterSupportedType"to enable support to this type
        /// </summary>
        /// <param name="type">Type to check</param>
        public static bool IsTypeSupported(Type type) {
            lock (DataUtils.sizesMap) {
                return DataUtils.sizesMap.ContainsKey(type);
            }
        }
    }

}