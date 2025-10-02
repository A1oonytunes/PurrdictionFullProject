using Unity.Collections;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// This static class extends the functionality of NativeArray to handle raw byte conversions.
    /// </summary>
    public static class NativeArrayExtension {

        /// <summary>
        /// Converts a NativeArray of any struct type to a raw byte array.
        /// </summary>
        /// <param name="arr">The NativeArray to convert.</param>
        /// <typeparam name="T">The struct type of the elements in the NativeArray.</typeparam>
        /// <returns>A byte array containing the raw bytes of the NativeArray.</returns>
        public static byte[] ToRawBytes<T>(this NativeArray<T> arr) where T : struct {
            // Convert the NativeArray to a NativeSlice of bytes.
            var slice = new NativeSlice<T>(arr).SliceConvert<byte>();
            // Allocate a byte array to hold the raw bytes.
            var bytes = new byte[slice.Length];
            // Copy the bytes from the NativeSlice to the byte array.
            slice.CopyTo(bytes);
            // Return the byte array.
            return bytes;
        }

        /// <summary>
        /// Copies raw bytes into a NativeArray of any struct type.
        /// </summary>
        /// <param name="arr">The NativeArray to copy into.</param>
        /// <param name="bytes">The byte array containing the raw bytes to copy.</param>
        /// <typeparam name="T">The struct type of the elements in the NativeArray.</typeparam>
        public static void CopyFromRawBytes<T>(this NativeArray<T> arr, byte[] bytes) where T : struct {
            // Create a temporary NativeArray from the byte array.
            var byteArr = new NativeArray<byte>(bytes, Allocator.Temp);
            // Convert the byte NativeArray to a NativeSlice of the target struct type.
            var slice = new NativeSlice<byte>(byteArr).SliceConvert<T>();

            // Assert that the lengths of the NativeArray and the slice are equal.
            UnityEngine.Debug.Assert(arr.Length == slice.Length);
            // Copy the data from the slice into the NativeArray.
            slice.CopyTo(arr);
        }
    }

}