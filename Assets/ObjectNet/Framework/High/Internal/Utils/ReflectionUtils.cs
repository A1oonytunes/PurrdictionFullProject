using System;
using System.Reflection;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Class with utilsa to be used on reflection operations
    /// </summary>
    public static class ReflectionUtils {

        /// <summary>
        /// Return if this class has this specific attribute annotation
        /// </summary>
        /// <param name="thisType">Origin class type</param>
        /// <param name="type">Attribute type to check</param>
        /// <returns>True if has this attribute</returns>
        public static bool HasAttribute(this Type thisType, Type type) {
            return (thisType.GetCustomAttribute(type) != null);
        }

        /// <summary>
        /// Return specific attribute annotation fo class
        /// </summary>
        /// <typeparam name="T">Generic attribute type to return</typeparam>
        /// <param name="type">Origin class type</param>
        /// <returns>Attribute object</returns>
        public static T GetAttribute<T>(this Type type) where T : Attribute {
            return (T)type.GetCustomAttribute(typeof(T));
        }

        /// <summary>
        /// Return if this field has this specific attribute annotation
        /// </summary>
        /// <param name="thisType">Origin class type</param>
        /// <param name="type">Attribute type to check</param>
        /// <returns>True if has this attribute</returns>
        public static bool HasAttribute(this FieldInfo thisType, Type type) {
            return (thisType.GetCustomAttribute(type) != null);
        }

        /// <summary>
        /// Return specific attribute annotation of field
        /// </summary>
        /// <typeparam name="T">Generic attribute type to return</typeparam>
        /// <param name="type">Origin class type</param>
        /// <returns>Attribute object</returns>
        public static T GetAttribute<T>(this FieldInfo type) where T : Attribute {
            return (T)type.GetCustomAttribute(typeof(T));
        }
    }
}