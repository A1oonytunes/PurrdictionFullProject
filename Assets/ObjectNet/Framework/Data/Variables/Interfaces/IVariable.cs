using System;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Interface defining the contract for a variable object.
    /// This interface allows for getting and setting variable values,
    /// triggering change events, validation, and checking modification status.
    /// </summary>
    public interface IVariable {
        /// <summary>
        /// Return variable version
        /// </summary>
        /// <returns>Sequential variable version number</returns>
        ulong GetVersion();

        /// <summary>
        /// Return associated behavior ID
        /// </summary>
        /// <returns>Behavior ID of component</returns>
        ushort GetBehaviourId();

        /// <summary>
        /// Return variableu
        /// </summary>
        /// <returns>Behavior ID of component</returns>
        string GetUniqueId();

        /// <summary>
        /// Set origin of network variable
        /// </summary>
        /// <param name="origin">Origin of variables source</param>
        void SetOrigin(NetworkVariableOrigin origin);

        /// <summary>
        /// Return origin of network variable
        /// </summary>
        /// <returns>Origin of variables sourc</returns>
        NetworkVariableOrigin GetOrigin();

        /// <summary>
        /// Return variable name
        /// </summary>
        /// <returns>Get variables</returns>
        string GetVariableName();

        /// <summary>
        /// Set variable name
        /// </summary>
        /// <param name="name">Variable name</param>
        void SetVariableName(string name);

        /// <summary>
        /// Return associated component
        /// </summary>
        /// <returns>Associated component</returns>
        MonoBehaviour GetComponent();

        /// <summary>
        /// Set associatec compoent with variable
        /// </summary>
        /// <param name="control">Associated control</param>
        void SetComponent(MonoBehaviour control);

        /// <summary>
        /// Gets the Type of the variable.
        /// </summary>
        /// <returns>The Type object representing the type of the variable.</returns>
        Type GetVariableType();

        /// <summary>
        /// Gets the current value of the variable.
        /// </summary>
        /// <returns>The current value of the variable as an object.</returns>
        object GetVariableValue();

        /// <summary>
        /// Gets the previous value of the variable before the last change.
        /// </summary>
        /// <returns>The previous value of the variable as an object.</returns>
        object GetPreviousValue();

        /// <summary>
        /// Sets the value of the variable.
        /// </summary>
        /// <param name="value">The new value to be set for the variable.</param>
        void SetVariableValue(object value);

        /// <summary>
        /// Gets the delivery mode of this variable.
        /// </summary>
        /// <returns>The configured delivery mode.</returns>
        DeliveryMode GetDeliveryMode();

        /// <summary>
        /// Sets the variable delivery mode.
        /// </summary>
        /// <param name="value">The new delivery mode.</param>
        void SetDeliveryMode(DeliveryMode value);

        /// <summary>
        /// Gets the detection mode of this variable.
        /// </summary>
        /// <returns>The configured detection mode.</returns>
        ReliableDetectionMode GetDetectionMode();

        /// <summary>
        /// Sets the variable detection mode.
        /// </summary>
        /// <param name="value">The new detection mode.</param>
        void SetDetectionMode(ReliableDetectionMode value);

        /// <summary>
        /// Clear current variuble value
        /// </summary>
        void ClearVariableValue();

        /// <summary>
        /// Force to refresh variable value in order to detect changes
        /// </summary>
        void Refresh();

        /// <summary>
        /// Flag this variable was updated
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Triggers an event or action when the variable value changes.
        /// </summary>
        /// <param name="oldValue">The value of the variable before the change.</param>
        /// <param name="newValue">The new value of the variable after the change.</param>
        void TriggerOnChange(object oldValue, object newValue);

        /// <summary>
        /// Checks if the variable was modified since the last reset or creation.
        /// </summary>
        /// <returns>True if the variable was modified; otherwise, false.</returns>
        bool WasModified();

        /// <summary>
        /// Validates the current value of the variable according to predefined rules or constraints.
        /// </summary>
        /// <exception cref="ValidationException">Thrown when the value does not meet the validation criteria.</exception>
        void Validate();

        /// <summary>
        /// Tranfer events from one Network variables to another
        /// </summary>
        /// <param name="from">Source network variable.</param>
        void TransferEvents(IVariable from);

    }

}