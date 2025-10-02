using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// ManagedInput is a ScriptableObject that holds information about a particular input.
    /// It implements the IManagerInput interface and provides methods to get and set input properties.
    /// </summary>
    [Serializable]
    public class ManagedInput : ScriptableObject, IManagerInput {

        // Serialized fields to store input properties, which can be edited in the Unity Editor.
        [SerializeField]
        private byte inputCode;

        [SerializeField]
        private string inputName;

        [SerializeField]
        private string inputMethod;

        [SerializeField]
        private string inputType;

        // Dictionary to map input type names to their corresponding System.Type.
        private Dictionary<string, Type> dictionaryTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Constructor for ManagedInput.
        /// </summary>
        /// <param name="code">The input code as a byte.</param>
        /// <param name="name">The input name as a string, with a default value of null.</param>
        public ManagedInput(byte code, string name = null) {
            this.inputCode = code;
            this.inputName = name;
        }

        /// <summary>
        /// Gets the input code.
        /// </summary>
        /// <returns>The input code as a byte.</returns>
        public byte GetInputCode() {
            return this.inputCode;
        }

        /// <summary>
        /// Gets the input name.
        /// </summary>
        /// <returns>The input name as a string.</returns>
        public string GetInputName() {
            return this.inputName;
        }

        /// <summary>
        /// Gets the input method.
        /// </summary>
        /// <returns>The input method as a string.</returns>
        public string GetMethod() {
            return this.inputMethod;
        }

        /// <summary>
        /// Gets the manager type.
        /// </summary>
        /// <returns>The manager type as a string.</returns>
        public string GetManagerType() {
            return this.inputType;
        }

        /// <summary>
        /// Gets the input type as a System.Type.
        /// </summary>
        /// <returns>The input type as a System.Type.</returns>
        public Type GetInputType() {
            this.InitializeDictionary();
            return this.dictionaryTypes[this.inputType];
        }

        /// <summary>
        /// Sets the input code.
        /// </summary>
        /// <param name="value">The new input code as a byte.</param>
        public void SetInputCode(byte value) {
            this.inputCode = value;
        }

        /// <summary>
        /// Sets the input name.
        /// </summary>
        /// <param name="value">The new input name as a string.</param>
        public void SetInputName(string value) {
            this.inputName = value;
        }

        /// <summary>
        /// Sets the input method.
        /// </summary>
        /// <param name="method">The new input method as a string.</param>
        public void SetMethod(string method) {
            this.inputMethod = method;
        }

        /// <summary>
        /// Sets the managed type.
        /// </summary>
        /// <param name="managedType">The new managed type as a string.</param>
        public void SetManagedType(string managedType) {
            this.inputType = managedType;
        }

        /// <summary>
        /// Initializes the dictionary mapping input type names to their corresponding System.Type.
        /// This method is called before accessing the dictionary to ensure it is populated.
        /// </summary>
        private void InitializeDictionary() {
            if (this.dictionaryTypes == null) {
                this.dictionaryTypes = new Dictionary<string, Type>();
            }
            if (this.dictionaryTypes.Count == 0) {
                // Populate the dictionary with a few common types.
                this.dictionaryTypes.Add(typeof(bool).FullName, typeof(bool));
                this.dictionaryTypes.Add(typeof(float).FullName, typeof(float));
                this.dictionaryTypes.Add(typeof(Vector2).FullName, typeof(Vector2));
            }
        }
    }

}