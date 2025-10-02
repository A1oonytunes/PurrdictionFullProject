using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Manages network events, including registering listeners, invoking methods, and sending actions.
    /// </summary>
    public class NetworkEventsManager : MonoBehaviour {

        // Serialized fields are hidden in the inspector but can be set in the editor.

        /// <summary>
        /// List of event listener references that are registered with the network manager.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        public List<EventListenerReference> ListenerEvents;

        /// <summary>
        /// List of event action references that are used to send actions over the network.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        public List<EventActionReference> ActionsEvents;

        // Flags to control the visibility of internal events in the inspector.
        [HideInInspector]
        [SerializeField]
        public bool InternalEventsHidden;

        [HideInInspector]
        [SerializeField]
        public bool ListenersEventsHidden;

        [HideInInspector]
        [SerializeField]
        public bool ActionsEventsHidden;

        /// <summary>
        /// The target database for storing event data. Defaults to a global constant.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private string databaseTarget = GlobalResources.DEFAULT_DATABASE;

        // Tracks whether listener events have been registered.
        private bool ListenerEventsRegistered = false;

        private float nextActionComputingTime = 0f;

        /// <summary>
        /// Called once per frame after all Update functions have been called.
        /// </summary>
        private void LateUpdate() {
            if ( this.ListenerEventsRegistered == false ) {
                this.ListenerEventsRegistered = this.RegisterCustomEvents();
            }
            // Detect and send action events
            if (this.HasActionEvents()) {
                if (this.nextActionComputingTime > Time.time) {
                    this.nextActionComputingTime = Time.time + (1.0f / NetworkManager.Instance().SendRate / 8.0f);
                    this.ComputeActionEvents();
                }
            }
        }

        /// <summary>
        /// Sets the target database for event data storage.
        /// </summary>
        /// <param name="database">The name of the target database.</param>
        public void SetTargetDatabase(string database) {
            this.databaseTarget = database;
            foreach (EventActionReference action in this.ActionsEvents) {
                action.SetEventManager(this); // Update events manager
            }
        }

        /// <summary>
        /// Gets the name of the target database for event data storage.
        /// </summary>
        /// <returns>The name of the target database.</returns>
        public string GetTargetDatabase() {
            return this.databaseTarget;
        }

        /// <summary>
        /// Retrieves the MethodInfo for a given event reference.
        /// </summary>
        /// <param name="reference">The event reference to retrieve the method for.</param>
        /// <returns>The MethodInfo of the event method.</returns>
        private MethodInfo GetEventMethod(EventReference reference) {
            return reference.GetEventComponent().GetType().GetMethod(reference.GetEventMethod());
        }

        /// <summary>
        /// Invokes a method with no return value using the provided event reference and arguments.
        /// </summary>
        /// <param name="reference">The event reference containing the method to invoke.</param>
        /// <param name="arguments">The arguments to pass to the method.</param>
        protected void InvokeVoidMethod(EventReference reference, params object[] arguments) {
            if (( reference                     != null ) &&
                ( reference.GetEventTarget()    != null ) &&
                ( reference.GetEventComponent() != null ) &&
                ( reference.GetEventMethod()    != null )) {
                if (!string.IsNullOrEmpty(reference.GetEventMethod())) {
                    this.GetEventMethod(reference).Invoke(reference.GetEventComponent(), arguments);
                } else {
                    NetworkDebugger.LogError("InvokeVoidMethod was called with not associated method. Check each events manager event to fix this problem");
                }
            }            
        }

        /// <summary>
        /// Invokes a method with a return value using the provided event reference and arguments.
        /// </summary>
        /// <typeparam name="T">The return type of the method.</typeparam>
        /// <param name="reference">The event reference containing the method to invoke.</param>
        /// <param name="arguments">The arguments to pass to the method.</param>
        /// <returns>The return value of the invoked method.</returns>
        protected T InvokeReturnMethod<T>(EventReference reference, params object[] arguments) {
            if ((reference != null) &&
                (reference.GetEventTarget() != null) &&
                (reference.GetEventComponent() != null) &&
                (reference.GetEventMethod() != null)) {
                return (T)this.GetEventMethod(reference).Invoke(reference.GetEventComponent(), arguments);
            } else {
                return default(T);
            }
        }

        /// <summary>
        /// Gets the return type of the method associated with the given event reference.
        /// </summary>
        /// <param name="eventReference">The event reference to check.</param>
        /// <returns>The return type of the method.</returns>
        protected Type GetReturnType(EventReference eventReference) {
            var property = typeof(NetworkEventsManager).GetField(eventReference.name);
            return (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).ReturnType;
        }

        /// <summary>
        /// Invokes the method associated with the given event reference, with or without a return value.
        /// </summary>
        /// <typeparam name="T">The expected return type of the method.</typeparam>
        /// <param name="eventReference">The event reference containing the method to invoke.</param>
        /// <param name="arguments">The arguments to pass to the method.</param>
        /// <returns>The return value of the invoked method, if any.</returns>
        public T Invoke<T>(EventReference eventReference, params object[] arguments) {
            if ( this.GetReturnType(eventReference) == typeof(void) ) {
                this.InvokeVoidMethod(eventReference, arguments);
                return default(T);
            } else {
                return this.InvokeReturnMethod<T>(eventReference, arguments);
            }            
        }

        /// <summary>
        /// Registers user-defined events with the network manager to be listened for.
        /// </summary>
        /// <returns>True if registration is successful, false otherwise.</returns>
        private bool RegisterCustomEvents() {
            bool result = false;
            if (NetworkManager.Instance() != null) {
                if (this.ListenerEvents != null) {
                    foreach (EventListenerReference eventListener in this.ListenerEvents) {
                        NetworkManager.Events.RegisterEvent(eventListener.GetEventCode(), (IDataStream reader) => {
                            // Execute event
                            if ((eventListener.GetEventTarget()     != null) &&
                                (eventListener.GetEventComponent()  != null) &&
                                (eventListener.GetEventMethod()     != null)) {
                                MethodInfo executionMethod = eventListener.GetEventComponent().GetType().GetMethod(eventListener.GetEventMethod());
                                if (executionMethod != null) {
                                    executionMethod.Invoke(eventListener.GetEventComponent(), new object[] { reader });
                                }
                            }
                        });
                    }
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Return if this manager has registered any action event
        /// </summary>
        /// <returns>True if has registered action event</returns>
        private bool HasActionEvents() {
            return ((this.ActionsEvents != null) &&
                    (this.ActionsEvents.Count > 0));
        }

        /// <summary>
        /// Processes and sends action events that have been triggered.
        /// </summary>
        private void ComputeActionEvents() {
            if (this.HasActionEvents()) {
                foreach (EventActionReference actionEvent in this.ActionsEvents) {
                    if (actionEvent.IsActionTriggered()) {
                        // Prepare writter data
                        DataStream writer = new DataStream();
                        foreach (var argument in actionEvent.GetActionArguments()) {
                            if (argument.Item1 == typeof(string)) {
                                writer.Write<string>((string)argument.Item2);
                            } else if (argument.Item1 == typeof(int)) {
                                writer.Write<int>((int)argument.Item2);
                            } else if (argument.Item1 == typeof(float)) {
                                writer.Write<float>((float)argument.Item2);
                            } else if (argument.Item1 == typeof(double)) {
                                writer.Write<double>((double)argument.Item2);
                            } else if (argument.Item1 == typeof(bool)) {
                                writer.Write<bool>((bool)argument.Item2);
                            } else {
                                throw new Exception(String.Format("Type not supported : {0}", argument.Item1));
                            }
                        }
                        // Send event
                        NetworkManager.Instance().Send(actionEvent.GetEventCodeToSend(),
                                                       writer,
                                                       (ActionExecutionMode.Continuous.Equals(actionEvent.GetExecutionMode()) ? DeliveryMode.Unreliable : DeliveryMode.Reliable));
                    }
                }
            }
        }

    }
}