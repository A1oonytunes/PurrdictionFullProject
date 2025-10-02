using System;
using System.Text;
using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// Represents an expression that holds a parameter with a specific data type and value.
    /// This class is used to serialize and deserialize parameter data for use in scripts.
    /// </summary>
    [Serializable]
    public class AttributeExpression : ScriptableObject {

        /// <summary>
        /// Enumerates the source types for the attribute.
        /// </summary>
        [SerializeField]
        public AttributeSourceType SourceType = AttributeSourceType.Attribute;

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        [SerializeField]
        public string Parameter;

        /// <summary>
        /// A buffer to store the full name of the data type as a string.
        /// </summary>
        [SerializeField]
        private string TypeBuffer;

        /// <summary>
        /// The raw binary data representing the parameter's value.
        /// </summary>
        [SerializeField]
        private byte[] Data;

        /// <summary>
        /// Gets or sets the data type of the parameter.
        /// </summary>
        public Type DataType { get { return this.GetParameterType(); } set { this.SetParameterType(value); } }

        /// <summary>
        /// Gets or sets the value of the parameter, automatically handling serialization and deserialization.
        /// </summary>
        public object ParameterValue { get { return this.GetData(); } set { this.SetData(value); } }

        /// <summary>
        /// Retrieves the data type of the parameter from the type buffer.
        /// </summary>
        /// <returns>The data type as a Type object, or null if the type buffer is not set.</returns>
        private Type GetParameterType() {
            return (this.TypeBuffer != null) ? Type.GetType(this.TypeBuffer) : null;
        }

        /// <summary>
        /// Sets the data type of the parameter, storing its full name in the type buffer.
        /// </summary>
        /// <param name="type">The Type of the parameter.</param>
        private void SetParameterType(Type type) {
            this.TypeBuffer = (type != null) ? type.FullName : null;
        }

        /// <summary>
        /// Deserializes and returns the parameter's value from the raw binary data.
        /// </summary>
        /// <returns>The deserialized parameter value.</returns>
        private object GetData() {
            // Deserialize the data based on the DataType and return the appropriate value.
            if (this.DataType == typeof(int)) {
                return ((this.Data != null) && (this.Data.Length > 0)) ? BitConverter.ToInt32(this.Data, 0) : 0;
            } else if (this.DataType == typeof(uint)) {
                return ((this.Data != null) && (this.Data.Length > 0)) ? BitConverter.ToUInt32(this.Data, 0) : 0;
            } else if (this.DataType == typeof(short)) {
                return ((this.Data != null) && (this.Data.Length > 0)) ? BitConverter.ToInt16(this.Data, 0) : 0;
            } else if (this.DataType == typeof(ushort)) {
                return ((this.Data != null) && (this.Data.Length > 0)) ? BitConverter.ToUInt16(this.Data, 0) : 0;
            } else if (this.DataType == typeof(float)) {
                return ((this.Data != null) && (this.Data.Length > 0)) ? BitConverter.ToSingle(this.Data, 0) : 0.0f;
            } else if (this.DataType == typeof(double)) {
                return ((this.Data != null) && (this.Data.Length > 0)) ? BitConverter.ToDouble(this.Data, 0) : 0.0;
            } else if (this.DataType == typeof(string)) {
                return ((this.Data != null) && (this.Data.Length > 0)) ? System.Text.Encoding.UTF8.GetString(this.Data) : "";
            } else if (this.DataType == typeof(bool)) {
                return ((this.Data != null) && (this.Data.Length > 0)) ? BitConverter.ToBoolean(this.Data, 0) : false;
            } else {
                return default(object);
            }
        }

        /// <summary>
        /// Serializes and sets the parameter's value as raw binary data.
        /// </summary>
        /// <param name="value">The value to serialize and store.</param>
        private void SetData(object value) {
            // Serialize the value based on the DataType and store it in the Data field.
            if (value == null) {
                this.Data = null;
            } else if (this.DataType == typeof(int)) {
                this.Data = BitConverter.GetBytes((int)value);
            } else if (this.DataType == typeof(uint)) {
                this.Data = BitConverter.GetBytes((uint)value);
            } else if (this.DataType == typeof(short)) {
                this.Data = BitConverter.GetBytes((short)value);
            } else if (this.DataType == typeof(ushort)) {
                this.Data = BitConverter.GetBytes((ushort)value);
            } else if (this.DataType == typeof(float)) {
                this.Data = BitConverter.GetBytes((float)value);
            } else if (this.DataType == typeof(double)) {
                this.Data = BitConverter.GetBytes((double)value);
            } else if (this.DataType == typeof(string)) {
                this.Data = Encoding.ASCII.GetBytes((string)value);
            } else if (this.DataType == typeof(bool)) {
                this.Data = BitConverter.GetBytes((bool)value);
            }
        }

    }


}