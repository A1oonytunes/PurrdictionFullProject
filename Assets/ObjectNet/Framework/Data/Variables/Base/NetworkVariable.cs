using System;
using System.Collections.Generic;
using System.Reflection;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a network variable that can be synchronized across the network.
    /// </summary>
    /// <typeparam name="T">The type of the variable to be synchronized.</typeparam>
    public class NetworkVariable<T> : INetworkVariable<T> {

        // Component associated with this variable
        UnityEngine.MonoBehaviour component;

        // The name of NetworkVariable
        string variableName;

        // The current value of the network variable.
        T value;

        // The previous value of the network variable.
        T previous;

        // Network variable origin
        NetworkVariableOrigin origin = NetworkVariableOrigin.Wrapped;

        // Flag indicating whether the variable has been modified since the last validation.
        bool modified = false;

        FieldInfo synchonizedField;

        NetworkBehaviour synchonizedComponent;

        // Delegate for handling value changes.
        Action<T, T> onValueChange;

        // Delegate for handling value synchronization when setting the value.
        Action<T> onSynchonizeSet;

        // Delegate for handling value synchronization when getting the value.
        Func<T> onSynchonizeGet;

        // Behavior ID
        ushort behaviorId = 0;

        // Variable unique ID
        string variableUniqueId = null;

        // Control variable version
        ulong version = 0;

        // Delivery mode of variable
        DeliveryMode deliveryMode = DeliveryMode.Reliable;

        // Detection mode
        ReliableDetectionMode detectionMode = ReliableDetectionMode.Automatic;

        // Global variable version sequence
        static ulong versionProvider = 0;

        // Lock protection of variable provider
        static Object variableProviderLock = new Object();

        /// <summary>
        /// Initializes a new instance of the NetworkVariable class with the specified initial value.
        /// </summary>
        /// <param name="value">The initial value of the network variable.</param>
        public NetworkVariable(T value, UnityEngine.MonoBehaviour component = null) {
            this.value = value;
            this.component = component;
            lock (NetworkVariable<T>.variableProviderLock) {
                this.version = NetworkVariable<T>.versionProvider++;
            }
        }

        /// <summary>
        /// Initializes a new instance of the NetworkVariable class with the specified initial value.
        /// </summary>
        /// <param name="value">The initial value of the network variable.</param>
        private NetworkVariable(T value, Action<T, T> onValueChange, Func<T> onGet, Action<T> onSet) {
            this.value = value;
            this.onValueChange = onValueChange;
            this.onSynchonizeGet = onGet;
            this.onSynchonizeSet = onSet;            
        }

        /// <summary>
        /// Tranfer events from one Network variables to another
        /// </summary>
        /// <param name="from">Source network variable.</param>
        public void TransferEvents(IVariable from) { 
            NetworkVariable<T> fromVariable = (from as NetworkVariable<T>);
            this.component          = (fromVariable.component       != null) ? fromVariable.component       : this.component;
            this.variableName       = (fromVariable.variableName    != null) ? fromVariable.variableName    : this.variableName;
            this.onValueChange      = (fromVariable.onValueChange   != null) ? fromVariable.onValueChange   : this.onValueChange;
            this.onSynchonizeGet    = (fromVariable.onSynchonizeGet != null) ? fromVariable.onSynchonizeGet : this.onSynchonizeGet;
            this.onSynchonizeSet    = (fromVariable.onSynchonizeSet != null) ? fromVariable.onSynchonizeSet : this.onSynchonizeSet;            
        }

        /// <summary>
        /// Return variable version
        /// </summary>
        /// <returns>Sequential variable version number</returns>
        public ulong GetVersion() {
            return this.version;
        }

        /// <summary>
        /// Return variable name
        /// </summary>
        /// <returns>Get variables</returns>
        public string GetVariableName() {
            return this.variableName;
        }

        /// <summary>
        /// Set variable name
        /// </summary>
        /// <param name="name">Variable name</param>
        public void SetVariableName(string name) {
            this.variableName = name;
        }

        /// <summary>
        /// Return associated component
        /// </summary>
        /// <returns>Associated component</returns>
        public UnityEngine.MonoBehaviour GetComponent() {
            return this.component;
        }

        /// <summary>
        /// Set associatec compoent with variable
        /// </summary>
        /// <param name="control">Associated control</param>
        public void SetComponent(UnityEngine.MonoBehaviour control) {
            this.component = control;
        }

        /// <summary>
        /// Return associated behavior ID
        /// </summary>
        /// <returns>Behavior ID of component</returns>
        public ushort GetBehaviourId() {
            if ( this.behaviorId == 0 ) {
                this.behaviorId = (this.component as NetworkBehaviour).GetBehaviorId();
            }
            return this.behaviorId;
        }

        /// <summary>
        /// Set origin of network variable
        /// </summary>
        /// <param name="origin">Origin of variables source</param>
        public void SetOrigin(NetworkVariableOrigin origin) {
            this.origin = origin;
        }

        /// <summary>
        /// Return origin of network variable
        /// </summary>
        /// <returns>Origin of variables sourc</returns>
        public NetworkVariableOrigin GetOrigin() {
            return this.origin;
        }

        /// <summary>
        /// Return variableu
        /// </summary>
        /// <returns>Behavior ID of component</returns>
        public string GetUniqueId() {
            if ( string.IsNullOrEmpty(this.variableUniqueId) == true ) {
                this.variableUniqueId = string.Format("{0}.{1}", this.GetBehaviourId(), this.GetVariableName());
            }
            return this.variableUniqueId;            
        }

        /// <summary>
        /// Gets the current value of the network variable, potentially invoking synchronization logic.
        /// </summary>
        /// <returns>The current value of the network variable.</returns>
        public T GetValue() {
            if (this.onSynchonizeGet != null)
                this.value = this.onSynchonizeGet.Invoke();
            return this.value;
        }

        /// <summary>
        /// Sets the value of the network variable and triggers synchronization logic if set.
        /// </summary>
        /// <param name="value">The new value to set.</param>
        public void SetValue(T value) {
            this.modified |= this.CheckWasModified(this.value, value);
            this.value = value;
            if (this.onSynchonizeSet != null) {
                this.onSynchonizeSet.Invoke(value);
            } else if ((this.synchonizedField != null) && (this.synchonizedComponent != null)) {
                this.synchonizedField.SetValue(this.synchonizedComponent, value);
            }
        }

        /// <summary>
        /// Gets the type of the network variable.
        /// </summary>
        /// <returns>The type of the network variable.</returns>
        public Type GetVariableType() {
            return typeof(T).IsEnum? Enum.GetUnderlyingType(typeof(T)) : typeof(T);
        }

        /// <summary>
        /// Gets the current value of the network variable as an object.
        /// </summary>
        /// <returns>The current value of the network variable.</returns>
        public object GetVariableValue() {
            if (this.onSynchonizeGet != null) {
                this.value = this.onSynchonizeGet.Invoke();
            } else if ((this.synchonizedField != null) && (this.synchonizedComponent != null)) {
                this.value = (T)this.synchonizedField.GetValue(this.synchonizedComponent);
            }

            return this.value;
        }

        /// <summary>
        /// Gets the previous value of the network variable.
        /// </summary>
        /// <returns>The previous value of the network variable.</returns>
        public object GetPreviousValue() {
            return this.previous;
        }

        /// <summary>
        /// Sets the value of the network variable using an object and triggers synchronization logic if set.
        /// </summary>
        /// <param name="value">The new value to set as an object.</param>
        public void SetVariableValue(object value) {
            if (typeof(T).IsEnum) value = (value != null) ? (T)value : default(T);
            this.modified  |= ((value != null) && (value is T)) ? this.CheckWasModified(this.value, (T)value) : (this.value != null);
            this.value      = ((value != null) && (value is T)) ? (T)value : default(T);
            if (this.onSynchonizeSet != null) {
                this.onSynchonizeSet.Invoke(((value != null) && (value is T)) ? (T)value : default(T));
            } else if ((this.synchonizedField != null) && (this.synchonizedComponent != null)) {
                this.synchonizedField.SetValue(this.synchonizedComponent, value);
            }
        }

        /// <summary>
        /// Clear current variuble value
        /// </summary>
        public void ClearVariableValue() {
            T defaltValue = default(T);
            this.modified |= ((value != null) && (Object.Equals((T)value, defaltValue) == false));
            this.value = default(T);
            if (this.onSynchonizeSet != null) {
                this.onSynchonizeSet.Invoke(default(T));
            } else if ((this.synchonizedField != null) && (this.synchonizedComponent != null)) {
                this.synchonizedField.SetValue(this.synchonizedComponent, default(T));
            }
        }

        /// <summary>
        /// Force to refresh variable value in order to detect changes
        /// </summary>
        public void Refresh() {
            if (this.WasModified() == false) {
                this.SetVariableValue(this.GetVariableValue());
            }   
        }

        /// <summary>
        /// Flag this variable was updated
        /// </summary>
        public void Invalidate() {
            this.modified = true;
        }

        /// <summary>
        /// Gets the delivery mode of this variable.
        /// </summary>
        /// <returns>The configured delivery mode.</returns>
        public DeliveryMode GetDeliveryMode() {
            return this.deliveryMode;
        }

        /// <summary>
        /// Sets the variable delivery mode.
        /// </summary>
        /// <param name="value">The new delivery mode.</param>
        public void SetDeliveryMode(DeliveryMode value) {
            this.deliveryMode = value;
        }

        /// <summary>
        /// Gets the detection mode of this variable.
        /// </summary>
        /// <returns>The configured detection mode.</returns>
        public ReliableDetectionMode GetDetectionMode() {
            return this.detectionMode;
        }

        /// <summary>
        /// Sets the variable detection mode.
        /// </summary>
        /// <param name="value">The new detection mode.</param>
        public void SetDetectionMode(ReliableDetectionMode value) {
            this.detectionMode = value;
        }

        /// <summary>
        /// Checks if the network variable has been modified since the last validation.
        /// </summary>
        /// <returns>True if the variable has been modified; otherwise, false.</returns>
        public bool WasModified() {
            return this.modified || (((ReliableDetectionMode.Automatic == this.GetDetectionMode()) || (DeliveryMode.Unreliable == this.GetDeliveryMode())) && 
                                     (this.CheckWasModified(this.previous, this.GetValue())));
        }

        /// <summary>
        /// Checks if the value of the network variable has been modified compared to the previous value.
        /// </summary>
        /// <param name="previous">The previous value of the network variable.</param>
        /// <param name="current">The current value of the network variable.</param>
        /// <returns>True if the value has been modified; otherwise, false.</returns>
        private bool CheckWasModified(T previous, T current) {
            bool result = (((previous == null) && (current != null)) ||
                           ((previous != null) && (current == null)) ||
                           ((previous != null) && !EqualityComparer<T>.Default.Equals(previous, current)));
            if (result) {
                this.previous = previous;                
            }
            return result;
        }

        /// <summary>
        /// Validates the network variable, resetting the modified flag and updating the previous value.
        /// </summary>
        public void Validate() {
            this.modified = false;
            this.previous = this.value;
        }

        /// <summary>
        /// Set the field and component source of this network variable
        /// </summary>
        /// <param name="field">Source field</param>
        /// <param name="component">Source component</param>
        public void SetSynchonizedField(in FieldInfo field, in NetworkBehaviour component) {
            this.synchonizedField = field;
            this.synchonizedComponent = component;
        }

        /// <summary>
        /// Sets the delegate to be called when the value of the network variable changes.
        /// </summary>
        /// <param name="onValueChange">The delegate to call on value change.</param>
        public void OnValueChange(Action<T, T> onValueChange) {
            this.onValueChange = onValueChange;
        }

        /// <summary>
        /// Triggers the onValueChange delegate with the specified old and new values.
        /// </summary>
        /// <param name="oldValue">The old value of the network variable.</param>
        /// <param name="newValue">The new value of the network variable.</param>
        public void TriggerOnChange(object oldValue, object newValue) {
            if (this.onValueChange != null) {
                this.onValueChange.Invoke((T)oldValue, (T)newValue);
            }
        }

        /// <summary>
        /// Sets the synchronization delegates for getting and setting the value of the network variable.
        /// </summary>
        /// <param name="onGet">The delegate to call when getting the value.</param>
        /// <param name="onSet">The delegate to call when setting the value.</param>
        public void OnSynchonize(Func<T> onGet, Action<T> onSet) {
            this.onSynchonizeGet = onGet;
            this.onSynchonizeSet = onSet;
        }

        /// <summary>
        /// Implicitly converts a value of type T to a NetworkVariable of type T.
        /// </summary>
        /// <param name="value">The value to be wrapped in a NetworkVariable.</param>
        /// <returns>A new NetworkVariable instance containing the value.</returns>
        public static implicit operator NetworkVariable<T>(T value) {
            return new NetworkVariable<T>(value);
        }

        /// <summary>
        /// Implicitly converts a NetworkVariable of type T to a value of type T.
        /// </summary>
        /// <param name="value">The NetworkVariable instance to extract the value from.</param>
        /// <returns>The value contained in the NetworkVariable.</returns>
        public static implicit operator T(NetworkVariable<T> value) {
            // Synchronize the value before returning, if a synchronization callback is set.
            if (value?.onSynchonizeGet != null) {
                value.value = value.onSynchonizeGet.Invoke();
            } else if ((value.synchonizedField != null) && (value.synchonizedComponent != null)) {
                value.value = (T)value.synchonizedField.GetValue(value.synchonizedComponent);
            }
            return value.GetValue();
        }

        /// <summary>
        /// Determines whether two NetworkVariable instances are equal.
        /// </summary>
        /// <param name="valueLeft">The first NetworkVariable instance to compare.</param>
        /// <param name="valueRight">The second NetworkVariable instance to compare.</param>
        /// <returns>True if both NetworkVariable instances are equal; otherwise, false.</returns>
        public static bool operator ==(NetworkVariable<T> valueLeft, NetworkVariable<T> valueRight) {
            // Synchronize values before comparison, if synchronization callbacks are set.
            if (valueLeft?.onSynchonizeGet != null) {
                valueLeft.value = valueLeft.onSynchonizeGet.Invoke();
            } else if ((valueLeft.synchonizedField != null) && (valueLeft.synchonizedComponent != null)) {
                valueLeft.value = (T)valueLeft.synchonizedField.GetValue(valueLeft.synchonizedComponent);
            }

            if (valueRight?.onSynchonizeGet != null) {
                valueRight.value = valueRight.onSynchonizeGet.Invoke();
            } else if ((valueRight.synchonizedField != null) && (valueRight.synchonizedComponent != null)) {
                valueRight.value = (T)valueRight.synchonizedField.GetValue(valueRight.synchonizedComponent);
            }

            // Check for inequality.
            return (Object.Equals(valueLeft, valueRight)) ||
                   (Object.Equals(valueLeft, null) && Object.Equals(null, valueRight)) ||
                   (Object.Equals(valueLeft.GetValue(), valueRight.GetValue()));
        }

        /// <summary>
        /// Determines whether two NetworkVariable instances are equal.
        /// </summary>
        /// <param name="valueLeft">The first NetworkVariable instance to compare.</param>
        /// <param name="valueRight">The second NetworkVariable instance to compare.</param>
        /// <returns>True if both NetworkVariable instances are equal; otherwise, false.</returns>
        public static bool operator ==(NetworkVariable<T> valueLeft, T valueRight) {
            // Synchronize values before comparison, if synchronization callbacks are set.
            if (valueLeft?.onSynchonizeGet != null) {
                valueLeft.value = valueLeft.onSynchonizeGet.Invoke();
            } else if ((valueLeft.synchonizedField != null) && (valueLeft.synchonizedComponent != null)) {
                valueLeft.value = (T)valueLeft.synchonizedField.GetValue(valueLeft.synchonizedComponent);
            }

            // Check for inequality.
            return (Object.Equals(valueLeft, valueRight)) ||
                   (Object.Equals(valueLeft, null) && Object.Equals(null, valueRight)) ||
                   (Object.Equals(valueLeft.GetValue(), valueRight));
        }

        /// <summary>
        /// Determines whether two NetworkVariable instances are equal.
        /// </summary>
        /// <param name="valueLeft">The first NetworkVariable instance to compare.</param>
        /// <param name="valueRight">The second NetworkVariable instance to compare.</param>
        /// <returns>True if both NetworkVariable instances are equal; otherwise, false.</returns>
        public static bool operator ==(T valueLeft, NetworkVariable<T> valueRight) {
            // Synchronize values before comparison, if synchronization callbacks are set.
            if (valueRight?.onSynchonizeGet != null) {
                valueRight.value = valueRight.onSynchonizeGet.Invoke();
            } else if ((valueRight.synchonizedField != null) && (valueRight.synchonizedComponent != null)) {
                valueRight.value = (T)valueRight.synchonizedField.GetValue(valueRight.synchonizedComponent);
            }

            // Check for inequality.
            return (Object.Equals(valueLeft, valueRight)) ||
                   (Object.Equals(valueLeft, null) && Object.Equals(null, valueRight)) ||
                   (Object.Equals(valueLeft, valueRight.GetValue()));
        }

        /// <summary>
        /// Determines whether two NetworkVariable instances are not equal.
        /// </summary>
        /// <param name="valueLeft">The first NetworkVariable instance to compare.</param>
        /// <param name="valueRight">The second NetworkVariable instance to compare.</param>
        /// <returns>True if both NetworkVariable instances are not equal; otherwise, false.</returns>
        public static bool operator !=(NetworkVariable<T> valueLeft, NetworkVariable<T> valueRight) {
            // Synchronize values before comparison, if synchronization callbacks are set.
            if (valueLeft?.onSynchonizeGet != null) {
                valueLeft.value = valueLeft.onSynchonizeGet.Invoke();
            } else if ((valueLeft.synchonizedField != null) && (valueLeft.synchonizedComponent != null)) {
                valueLeft.value = (T)valueLeft.synchonizedField.GetValue(valueLeft.synchonizedComponent);
            }

            if (valueRight?.onSynchonizeGet != null) {
                valueRight.value = valueRight.onSynchonizeGet.Invoke();
            } else if ((valueRight.synchonizedField != null) && (valueRight.synchonizedComponent != null)) {
                valueRight.value = (T)valueRight.synchonizedField.GetValue(valueRight.synchonizedComponent);
            }

            // Check for inequality.
            return (Object.Equals(valueLeft, null) && !Object.Equals(null, valueRight)) ||
                   (!Object.Equals(valueLeft, null) && Object.Equals(null, valueRight)) ||
                   (!Object.Equals(valueLeft.GetValue(), valueRight.GetValue()));
        }

        /// <summary>
        /// Determines whether two NetworkVariable instances are not equal.
        /// </summary>
        /// <param name="valueLeft">The first NetworkVariable instance to compare.</param>
        /// <param name="valueRight">The second NetworkVariable instance to compare.</param>
        /// <returns>True if both NetworkVariable instances are not equal; otherwise, false.</returns>
        public static bool operator !=(NetworkVariable<T> valueLeft, T valueRight) {
            // Synchronize values before comparison, if synchronization callbacks are set.
            if (valueLeft?.onSynchonizeGet != null) {
                valueLeft.value = valueLeft.onSynchonizeGet.Invoke();
            } else if ((valueLeft.synchonizedField != null) && (valueLeft.synchonizedComponent != null)) {
                valueLeft.value = (T)valueLeft.synchonizedField.GetValue(valueLeft.synchonizedComponent);
            }

            // Check for inequality.
            return (Object.Equals(valueLeft,  null) && !Object.Equals(null, valueRight)) ||
                   (!Object.Equals(valueLeft, null) && Object.Equals(null, valueRight)) ||
                   (!Object.Equals(valueLeft.GetValue(), valueRight));
        }

        /// <summary>
        /// Determines whether two NetworkVariable instances are not equal.
        /// </summary>
        /// <param name="valueLeft">The first NetworkVariable instance to compare.</param>
        /// <param name="valueRight">The second NetworkVariable instance to compare.</param>
        /// <returns>True if both NetworkVariable instances are not equal; otherwise, false.</returns>
        public static bool operator !=(T valueLeft, NetworkVariable<T> valueRight) {
            // Synchronize values before comparison, if synchronization callbacks are set.
            if (valueRight?.onSynchonizeGet != null) {
                valueRight.value = valueRight.onSynchonizeGet.Invoke();
            } else if ((valueRight.synchonizedField != null) && (valueRight.synchonizedComponent != null)) {
                valueRight.value = (T)valueRight.synchonizedField.GetValue(valueRight.synchonizedComponent);
            }

            // Check for inequality.
            return (Object.Equals(valueLeft, null) && !Object.Equals(null, valueRight)) ||
                   (!Object.Equals(valueLeft, null) && Object.Equals(null, valueRight)) ||
                   (!Object.Equals(valueLeft, valueRight.GetValue()));
        }

        /// <summary>
        /// Unary plus operator to retrieve the value from a NetworkVariable.
        /// </summary>
        /// <param name="value">The NetworkVariable instance to extract the value from.</param>
        /// <returns>The value contained in the NetworkVariable.</returns>
        public static T operator +(NetworkVariable<T> value) {
            // Synchronize the value before returning, if a synchronization callback is set.
            if (value?.onSynchonizeGet != null) {
                value.value = value.onSynchonizeGet.Invoke();
            } else if ((value.synchonizedField != null) && (value.synchonizedComponent != null)) {
                value.value = (T)value.synchonizedField.GetValue(value.synchonizedComponent);
            }

            return value;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}