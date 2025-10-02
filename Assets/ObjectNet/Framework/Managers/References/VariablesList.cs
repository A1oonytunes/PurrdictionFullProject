using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {

    // Represents a sealed class that maintains a list of variables with their statuses.
    [Serializable]
    public sealed class VariablesList : System.Object {

        // A list to hold the variables with their statuses.
        [SerializeField]
        private List<VariableStatus> Variables = new List<VariableStatus>();

        /// <summary>
        /// Determines whether the specified variable is contained within the list.
        /// </summary>
        /// <param name="variable">The variable to check for existence.</param>
        /// <returns>True if the variable is found, otherwise false.</returns>
        public bool ContainsVariable(String variable) {
            bool result = false;
            // Iterate through the list to find if the variable exists.
            foreach (VariableStatus scriptEntry in this.Variables) {
                if (scriptEntry.Variable == variable) {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Registers a new variable unless it already exists in the list.
        /// </summary>
        /// <param name="variable">The variable to register.</param>
        public void RegisterVariable(String variable) {
            bool exists = false;
            // Check if the variable already exists in the list.
            foreach (VariableStatus variableEntry in this.Variables) {
                if (variableEntry.Variable == variable) {
                    exists = true;
                    break;
                }
            }
            // If the variable does not exist, add it to the list.
            if (!exists) {
                this.Variables.Add(new VariableStatus(variable));
            }
        }

        /// <summary>
        /// Unregisters a variable by name if it exists in the list.
        /// </summary>
        /// <param name="variable">The name of the variable to unregister.</param>
        public void UnRegisterVariable(String variable) {
            bool exists = false;
            int index = 0;
            // Find the index of the variable to be unregistered.
            foreach (VariableStatus variableEntry in this.Variables) {
                if (variableEntry.Variable == variable) {
                    exists = true;
                    break;
                }
                index++;
            }
            // If found, remove the variable from the list.
            if (exists) {
                this.Variables.RemoveAt(index);
            }
        }

        /// <summary>
        /// Unregisters a variable by reference if it exists in the list.
        /// </summary>
        /// <param name="variable">The reference to the variable to unregister.</param>
        public void UnRegisterVariable(VariableStatus variable) {
            // Remove the variable if it is found in the list.
            if (this.Variables.Contains(variable)) {
                this.Variables.Remove(variable);
            }
        }

        /// <summary>
        /// Retrieves the list of variables with their statuses.
        /// </summary>
        /// <returns>A list of VariableStatus objects.</returns>
        public List<VariableStatus> GetVariables() {
            return this.Variables;
        }

        /// <summary>
        /// Clears all variables from the list.
        /// </summary>
        public void ClearVariable() {
            this.Variables.Clear();
        }
    }


}