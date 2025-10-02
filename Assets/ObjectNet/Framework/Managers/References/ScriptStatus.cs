using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace com.onlineobject.objectnet {

    [Serializable]
    public class ScriptStatus : System.Object {

        [SerializeField]
        public MonoBehaviour Script = null;

        [SerializeField]
        public Boolean Enabled = false;

        [SerializeField]
        public int Delay = 0;

        [SerializeField]
        private VariablesList Variables;

        [SerializeField]
        private VariablesList NetworkVariables;

        /// <summary>
        /// Constructor for the ScriptStatus class.
        /// </summary>
        /// <param name="script">The MonoBehaviour script associated with this status.</param>
        /// <param name="scriptEnabled">Initial enabled state of the script. Defaults to false.</param>
        public ScriptStatus(MonoBehaviour script, bool scriptEnabled = false) {
            this.Script             = script;
            this.Enabled            = scriptEnabled;
            this.Variables          = new VariablesList();
            this.NetworkVariables   = new VariablesList();
        }

        /// <summary>
        /// Retrieves the list of primitive variables associated with the script.
        /// </summary>
        /// <returns>A VariablesList containing the variables.</returns>
        public VariablesList GetVariables() {
            return this.Variables;
        }

        /// <summary>
        /// Retrieves the list of NetworkVariable(s) associated with the script.
        /// </summary>
        /// <returns>A VariablesList containing the variables.</returns>
        public VariablesList GetNetworkVariables() {
            return this.NetworkVariables;
        }

        /// <summary>
        /// Retrieves an array of variable names that are enabled.
        /// </summary>
        /// <returns>An array of strings containing the names of enabled variables.</returns>
        public VariableStatus[] GetSynchronizedVariables() {
            List<VariableStatus> result = new List<VariableStatus>();
            foreach (VariableStatus variable in this.Variables.GetVariables()) {
                if (variable.Enabled) {
                    result.Add(variable);
                }
            }
            foreach (VariableStatus variable in this.NetworkVariables.GetVariables()) {
                result.Add(variable);
            }            
            return result.ToArray<VariableStatus>();
        }

        /// <summary>
        /// Refreshes the list of variables by synchronizing with the actual fields of the MonoBehaviour script.
        /// </summary>
        /// <returns>The current instance of ScriptStatus after updating the variables.</returns>
        public ScriptStatus RefreshVariables() {
            this.RefreshPrimitiveVariables();
            this.RefreshNetworkVariables();
            return this;
        }


        private void RefreshPrimitiveVariables() {
            if (this.Script != null) {
                // First list all class variables
                List<FieldInfo> fields = this.Script.GetType().GetFields(BindingFlags.Public |
                                                                         BindingFlags.NonPublic |
                                                                         BindingFlags.Instance |
                                                                         BindingFlags.DeclaredOnly).ToList<FieldInfo>();
                // Get from bae class
                Type baseClass = this.Script.GetType().BaseType;
                while ((baseClass != null) && (baseClass.Equals(typeof(NetworkBehaviour))) == false) {
                    fields.AddRange(baseClass.GetFields(BindingFlags.Public |
                                                        BindingFlags.NonPublic |
                                                        BindingFlags.Instance |
                                                        BindingFlags.DeclaredOnly).ToList<FieldInfo>());
                    baseClass = baseClass.BaseType;
                }

                // Register missing variables
                foreach (FieldInfo field in fields) {
                    if (this.IsAllowedVariableOverNetwork(field.FieldType)) {
                        if (!this.Variables.ContainsVariable(field.Name)) {
                            this.Variables.RegisterVariable(field.Name);
                        }
                    }
                }
                // Remove not existent variables
                List<VariableStatus> toRemove = new List<VariableStatus>();
                foreach (VariableStatus field in this.Variables.GetVariables()) {
                    if (!fields.Exists(fd => fd.Name.Equals(field.Variable))) {
                        toRemove.Add(field);
                    }
                }
                // Remove
                while (toRemove.Count > 0) {
                    VariableStatus variable = toRemove[0];
                    toRemove.RemoveAt(0);
                    this.Variables.UnRegisterVariable(variable);
                }
            }
        }

        private void RefreshNetworkVariables() {
            if (this.Script != null) {
                // First list all class variables
                List<FieldInfo> fields = this.Script.GetType().GetFields(BindingFlags.Public |
                                                                         BindingFlags.NonPublic |
                                                                         BindingFlags.Instance |
                                                                         BindingFlags.DeclaredOnly).ToList<FieldInfo>();
                // Get from bae class
                Type baseClass = this.Script.GetType().BaseType;
                while ((baseClass != null) && (baseClass.Equals(typeof(NetworkBehaviour))) == false) {
                    fields.AddRange(baseClass.GetFields(BindingFlags.Public |
                                                        BindingFlags.NonPublic |
                                                        BindingFlags.Instance |
                                                        BindingFlags.DeclaredOnly).ToList<FieldInfo>());
                    baseClass = baseClass.BaseType;
                }

                // Register missing variables
                foreach (FieldInfo field in fields) {
                    if ((typeof(IVariable).IsAssignableFrom(field.FieldType))) {
                        if (!this.NetworkVariables.ContainsVariable(field.Name)) {
                            this.NetworkVariables.RegisterVariable(field.Name);
                        }
                    }
                }
                // Remove not existent variables
                List<VariableStatus> toRemove = new List<VariableStatus>();
                foreach (VariableStatus field in this.NetworkVariables.GetVariables()) {
                    if (!fields.Exists(fd => fd.Name.Equals(field.Variable))) {
                        toRemove.Add(field);
                    }
                }
                // Remove
                while (toRemove.Count > 0) {
                    VariableStatus variable = toRemove[0];
                    toRemove.RemoveAt(0);
                    this.NetworkVariables.UnRegisterVariable(variable);
                }
            }
        }

        /// <summary>
        /// Determines if a given type is allowed to be synchronized over the network.
        /// </summary>
        /// <param name="type">The Type to check.</param>
        /// <returns>True if the type is allowed, false otherwise.</returns>
        private bool IsAllowedVariableOverNetwork(Type type) {
            if (type.IsEnum) type = Enum.GetUnderlyingType(type);
            return (type == typeof(int)) ||
                   (type == typeof(uint)) ||
                   (type == typeof(long)) ||
                   (type == typeof(ulong)) ||
                   (type == typeof(short)) ||
                   (type == typeof(ushort)) ||
                   (type == typeof(float)) ||
                   (type == typeof(double)) ||
                   (type == typeof(byte)) ||
                   (type == typeof(sbyte)) ||
                   (type == typeof(byte[])) ||
                   (type == typeof(string)) ||
                   (type == typeof(char)) ||
                   (type == typeof(char[])) ||
                   (type == typeof(Vector2)) ||
                   (type == typeof(Vector3)) ||
                   (type == typeof(Vector4)) ||
                   (type == typeof(Color)) ||
                   (type == typeof(Quaternion)) ||
                   (type == typeof(Matrix4x4)) ||
                   (type == typeof(bool));
                   
        }

    }

}