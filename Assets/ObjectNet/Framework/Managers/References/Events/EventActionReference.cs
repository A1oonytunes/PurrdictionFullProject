using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace com.onlineobject.objectnet {

    [Serializable]
    /// <summary>
    /// Represents a reference to an event action, including its execution mode, trigger conditions, and associated data.
    /// </summary>
    public class EventActionReference : EventReference {

        // Serialized fields to be set in the Unity Inspector or through scripts.

        [SerializeField]
        private string EventName; // The name of the event.

        [SerializeField]
        private ActionExecutionMode ExecutionMode = ActionExecutionMode.OnBecameTrue; // The mode in which the action is executed.

        [SerializeField]
        private int EventCodeToSend; // The event code to send when the action is triggered.

        [SerializeField]
        private string EventToSend; // The event to send when the action is triggered.

        [SerializeField]
        private GameObject TriggerTarget; // The target GameObject that triggers the event.

        [SerializeField]
        private MonoBehaviour TriggerComponent; // The component on the trigger target that is used to evaluate trigger expressions.

        [SerializeField]
        private List<TriggerExpression> TriggerExpressions = new List<TriggerExpression>(); // The list of trigger expressions to evaluate.

        [SerializeField]
        private List<AttributeExpression> AttributeExpressions = new List<AttributeExpression>(); // The list of attribute expressions to evaluate.

        [SerializeField]
        private string TargetDetectionError; // The error message for target detection.

        [SerializeField]
        private float TargetDetectionErrorTimeout; // The timeout duration for target detection errors.

        [SerializeField]
        private NetworkEventsManager EventManager; // The manager responsible for handling network events.

        [NonSerialized]
        private bool CurrentStatus = false; // The current status of the action trigger.

        /// <summary>
        /// Gets the name of the event.
        /// </summary>
        /// <returns>The name of the event.</returns>
        public string GetEventName() {
            return this.EventName;
        }

        /// <summary>
        /// Sets the name of the event.
        /// </summary>
        /// <param name="eventName">The name to set for the event.</param>
        public void SetEventName(string eventName) {
            this.EventName = eventName;
        }

        /// <summary>
        /// Retrieves the event code that should be sent.
        /// </summary>
        /// <returns>The event code to send.</returns>
        public int GetEventCodeToSend() {
            return this.EventCodeToSend;
        }

        /// <summary>
        /// Sets the event code that should be sent.
        /// </summary>
        /// <param name="code">The event code to be set.</param>
        public void SetEventCodeToSend(int code) {
            this.EventCodeToSend = code;
        }

        /// <summary>
        /// Retrieves the current execution mode for the action.
        /// </summary>
        /// <returns>The execution mode of the action.</returns>
        public ActionExecutionMode GetExecutionMode() {
            return this.ExecutionMode;
        }

        /// <summary>
        /// Sets the event manager responsible for handling network events.
        /// </summary>
        /// <param name="manager">The network events manager to be set.</param>
        public void SetEventManager(NetworkEventsManager manager) {
            this.EventManager = manager;
        }

        /// <summary>
        /// Evaluates the trigger expressions to determine the action's trigger status.
        /// </summary>
        /// <returns>True if the action is triggered, false otherwise.</returns>
        private bool GetActionTriggerStatus() {
            bool firstIteraction    = true;
            bool result             = false;
            foreach (TriggerExpression expression in this.TriggerExpressions) {
                if ( ParameterSourceType.Attribute.Equals(expression.SourceType) ) {
                    // Get attribute
                    FieldInfo field = this.TriggerComponent.GetType().GetField(expression.Parameter, BindingFlags.Public
                                                                                                    | BindingFlags.NonPublic
                                                                                                    | BindingFlags.Instance
                                                                                                    | BindingFlags.DeclaredOnly);
                    if ( field != null ) {
                        object value = field.GetValue(this.TriggerComponent);
                        if ( value != null ) {
                            if (TriggerCondition.And.Equals(expression.Condition)) {
                                if ( firstIteraction == true ) {
                                    result          = true;
                                    firstIteraction = false;
                                }
                                if (TriggerComparators.EqualsTo.Equals(expression.Comparator)) {
                                    result &= (value.Equals(expression.Expected));
                                } else if (TriggerComparators.Different.Equals(expression.Comparator)) {
                                    result &= (value.Equals(expression.Expected) == false);
                                } else if (TriggerComparators.Greather.Equals(expression.Comparator)) {
                                    if ( value is float ) {
                                        result &= ((float)value > (float)expression.Expected);
                                    } else if ( value is double ) {
                                        result &= ((double)value > (double)expression.Expected);
                                    } else if ( value is int ) {
                                        result &= ((int)value > (int)expression.Expected);
                                    } else if ( value is short ) {
                                        result &= ((short)value > (short)expression.Expected);
                                    } else if ( value is byte ) {
                                        result &= ((byte)value > (byte)expression.Expected);
                                    }                                    
                                } else if (TriggerComparators.Less.Equals(expression.Comparator)) {
                                    if ( value is float ) {
                                        result &= ((float)value < (float)expression.Expected);
                                    } else if ( value is double ) {
                                        result &= ((double)value < (double)expression.Expected);
                                    } else if ( value is int ) {
                                        result &= ((int)value < (int)expression.Expected);
                                    } else if ( value is short ) {
                                        result &= ((short)value < (short)expression.Expected);
                                    } else if ( value is byte ) {
                                        result &= ((byte)value < (byte)expression.Expected);
                                    }                                    
                                }                                
                                if (!result) break;
                            } else if (TriggerCondition.Or.Equals(expression.Condition)) {
                                if ( firstIteraction == true ) {
                                    result          = false;
                                    firstIteraction = false;
                                }
                                if (TriggerComparators.EqualsTo.Equals(expression.Comparator)) {
                                    result |= (value.Equals(expression.Expected));
                                } else if (TriggerComparators.Different.Equals(expression.Comparator)) {
                                    result |= (value.Equals(expression.Expected) == false);
                                } else if (TriggerComparators.Greather.Equals(expression.Comparator)) {
                                    if ( value is float ) {
                                        result |= ((float)value > (float)expression.Expected);
                                    } else if ( value is double ) {
                                        result |= ((double)value > (double)expression.Expected);
                                    } else if ( value is int ) {
                                        result |= ((int)value > (int)expression.Expected);
                                    } else if ( value is short ) {
                                        result |= ((short)value > (short)expression.Expected);
                                    } else if ( value is byte ) {
                                        result |= ((byte)value > (byte)expression.Expected);
                                    }                                    
                                } else if (TriggerComparators.Less.Equals(expression.Comparator)) {
                                    if ( value is float ) {
                                        result |= ((float)value < (float)expression.Expected);
                                    } else if ( value is double ) {
                                        result |= ((double)value < (double)expression.Expected);
                                    } else if ( value is int ) {
                                        result |= ((int)value < (int)expression.Expected);
                                    } else if ( value is short ) {
                                        result |= ((short)value < (short)expression.Expected);
                                    } else if ( value is byte ) {
                                        result |= ((byte)value < (byte)expression.Expected);
                                    }                                    
                                }
                            }
                        }
                    }
                } else if ( ParameterSourceType.Function.Equals(expression.SourceType) ) {
                    // Get attribute
                    MethodInfo method = this.TriggerComponent.GetType().GetMethod(expression.Parameter);
                    if ( method != null ) {
                        object value = method.Invoke(this.TriggerComponent, new object[] { });
                        if ( value != null ) {
                            if (TriggerCondition.And.Equals(expression.Comparator)) {
                                result &= (value.Equals(expression.Expected));
                                if (!result) break;
                            } else if (TriggerCondition.And.Equals(expression.Comparator)) {
                                result |= (value.Equals(expression.Expected));
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Determines whether the action should be triggered based on the current status and execution mode.
        /// </summary>
        /// <returns>True if the action should be triggered, false otherwise.</returns>
        public bool IsActionTriggered() {
            bool result = false;
            bool status = this.GetActionTriggerStatus();
            if (ActionExecutionMode.Continuous.Equals(this.ExecutionMode)) {
                result = (status == true);
            } else if (ActionExecutionMode.OnBecameFalse.Equals(this.ExecutionMode)) {
                result = ((this.CurrentStatus == true) && (status == false));
            } else if (ActionExecutionMode.OnBecameTrue.Equals(this.ExecutionMode)) {
                result = ((this.CurrentStatus == false) && (status == true));
            } else if (ActionExecutionMode.OnTransition.Equals(this.ExecutionMode)) {
                result = (this.CurrentStatus != status);
            }
            // Update result
            this.CurrentStatus = status;
            return result;
        }

        /// <summary>
        /// Retrieves an array of tuples containing the types and values of action arguments
        /// based on the AttributeExpressions of the current instance.
        /// </summary>
        /// <returns>
        /// An array of Tuple<Type, object> where each tuple represents the type and value
        /// of an argument.
        /// </returns>
        public Tuple<Type, object>[] GetActionArguments() {
            Tuple<Type, 
                  object>[] arguments       = new Tuple<Type, object>[this.AttributeExpressions.Count];
            int             indexOfArgument = 0;
            foreach (AttributeExpression argument in this.AttributeExpressions) {
                Type    argumentType    = null;
                object  argumentValue   = null;

                if (AttributeSourceType.Attribute.Equals(argument.SourceType)) {
                    // Get attribute
                    FieldInfo field = this.TriggerComponent.GetType().GetField(argument.Parameter, BindingFlags.Public
                                                                                                   | BindingFlags.NonPublic
                                                                                                   | BindingFlags.Instance
                                                                                                   | BindingFlags.DeclaredOnly);
                    if (field != null) {
                        argumentType    = field.FieldType;
                        argumentValue   = field.GetValue(this.TriggerComponent);
                    }
                } else if (AttributeSourceType.Function.Equals(argument.SourceType)) {
                    // Get attribute
                    MethodInfo method = this.TriggerComponent.GetType().GetMethod(argument.Parameter, BindingFlags.Public
                                                                                                      | BindingFlags.NonPublic
                                                                                                      | BindingFlags.Instance
                                                                                                      | BindingFlags.DeclaredOnly);
                    if (method != null) {
                        argumentType    = method.ReturnType;
                        argumentValue   = method.Invoke(this.TriggerComponent, new object[] { });
                    }
                } else if (AttributeSourceType.String.Equals(argument.SourceType)) {
                    argumentType    = typeof(string);
                    argumentValue   = (argument.ParameterValue as String);
                } else if (AttributeSourceType.Integer.Equals(argument.SourceType)) {
                    argumentType    = typeof(int);
                    argumentValue   = (argument.ParameterValue != null) ? argument.ParameterValue : 0;
                } else if (AttributeSourceType.Float.Equals(argument.SourceType)) {
                    argumentType    = typeof(float);
                    argumentValue   = (argument.ParameterValue != null) ? argument.ParameterValue : 0.0f;
                } else if (AttributeSourceType.Double.Equals(argument.SourceType)) {
                    argumentType    = typeof(double);
                    argumentValue   = (argument.ParameterValue != null) ? argument.ParameterValue : 0.0;
                } else if (AttributeSourceType.Boolean.Equals(argument.SourceType)) {
                    argumentType    = typeof(bool);
                    argumentValue   = (argument.ParameterValue != null) ? argument.ParameterValue : false;
                }

                arguments[indexOfArgument++] = new Tuple<Type, object>(argumentType, argumentValue);
            }
            return arguments;
        }
    }
}