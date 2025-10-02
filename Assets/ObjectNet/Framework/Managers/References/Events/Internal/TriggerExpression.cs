using System;
using System.Text;
using UnityEngine;

namespace com.onlineobject.objectnet {

    [Serializable]
    /// <summary>
    /// Represents a trigger expression that can be used to evaluate conditions based on various parameters.
    /// </summary>
    public class TriggerExpression : ScriptableObject {
        // Serialized fields allow these properties to be edited in the Unity Inspector.

        /// <summary>
        /// The source type of the parameter, which determines where the parameter's value comes from.
        /// </summary>
        [SerializeField]
        public ParameterSourceType SourceType = ParameterSourceType.Attribute;

        /// <summary>
        /// The comparator used to compare the parameter against an expected value.
        /// </summary>
        [SerializeField]
        public TriggerComparators Comparator;

        /// <summary>
        /// The logical condition used when evaluating multiple triggers.
        /// </summary>
        [SerializeField]
        public TriggerCondition Condition = TriggerCondition.And;

        /// <summary>
        /// The name of the parameter to be evaluated.
        /// </summary>
        [SerializeField]
        public string Parameter;

        // Buffer to store the type name of the expected data.
        [SerializeField]
        private string TypeBuffer;

        // Raw byte data used to store the expected value in a serialized format.
        [SerializeField]
        private byte[] Data;

        /// <summary>
        /// The data type of the expected value.
        /// </summary>
        public Type DataType { get { return this.GetDataType(); } set { this.SetDataType(value); } }

        /// <summary>
        /// The expected value to compare against the parameter's value.
        /// </summary>
        public object Expected { get { return this.GetData(); } set { this.SetData(value); } }

        /// <summary>
        /// Retrieves the data type from the type buffer.
        /// </summary>
        /// <returns>The Type object of the expected value.</returns>
        private Type GetDataType() {
            return (this.TypeBuffer != null) ? Type.GetType(this.TypeBuffer) : null;
        }

        /// <summary>
        /// Sets the data type and updates the type buffer accordingly.
        /// </summary>
        /// <param name="type">The Type of the expected value.</param>
        private void SetDataType(Type type) { 
            this.TypeBuffer = (type != null) ? type.FullName : null;
        }

        /// <summary>
        /// Deserializes and retrieves the expected value from the raw byte data.
        /// </summary>
        /// <returns>The expected value in its original data type.</returns>
        private object GetData() {
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
                this.Data = null;
                return default(object);
            }
        }

        /// <summary>
        /// Serializes and sets the expected value as raw byte data.
        /// </summary>
        /// <param name="value">The expected value to be serialized.</param>
        private void SetData(object value) {
            if (value == null) {
                this.Data = null;
            } if (this.DataType == typeof(int)) {
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
            } else {
                this.Data = null;
            }
        }
    }

}