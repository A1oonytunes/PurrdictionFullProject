using System;
using UnityEngine;


namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a reference to an event, including the target object, component, and method to be invoked.
    /// </summary>
    [Serializable]
    public class EventReference : ScriptableObject {

        /// <summary>
        /// The target GameObject on which the event is to be invoked.
        /// </summary>
        [SerializeField]
        private GameObject EventTarget;

        /// <summary>
        /// The MonoBehaviour component attached to the EventTarget that contains the method to invoke.
        /// </summary>
        [SerializeField]
        private MonoBehaviour EventComponent;

        /// <summary>
        /// The name of the method to be invoked on the EventComponent.
        /// </summary>
        [SerializeField]
        private string EventMethod;

        /// <summary>
        /// Flag to control the visibility of this event reference in the editor.
        /// </summary>
        [SerializeField]
        private bool EditorVisible;

        /// <summary>
        /// A message that describes an execution error, if any occurs during the event invocation.
        /// </summary>
        [SerializeField]
        private string ExecutionError;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventReference"/> class.
        /// </summary>
        public EventReference() {
        }

        /// <summary>
        /// Gets the target GameObject for the event.
        /// </summary>
        /// <returns>The target GameObject.</returns>
        public GameObject GetEventTarget() {
            return this.EventTarget;
        }

        /// <summary>
        /// Gets the MonoBehaviour component that contains the event method.
        /// </summary>
        /// <returns>The MonoBehaviour component.</returns>
        public MonoBehaviour GetEventComponent() {
            return this.EventComponent;
        }

        /// <summary>
        /// Gets the name of the method to be invoked for the event.
        /// </summary>
        /// <returns>The name of the method.</returns>
        public string GetEventMethod() {
            return this.EventMethod;
        }

        /// <summary>
        /// Determines whether the event reference is visible in the editor.
        /// </summary>
        /// <returns>True if visible in the editor; otherwise, false.</returns>
        public bool IsEditorVisible() {
            return this.EditorVisible;
        }

        /// <summary>
        /// Sets the target GameObject for the event.
        /// </summary>
        /// <param name="value">The GameObject to set as the event target.</param>
        public void SetEventTarget(GameObject value) {
            this.EventTarget = value;
        }

        /// <summary>
        /// Sets the MonoBehaviour component that contains the event method.
        /// </summary>
        /// <param name="value">The MonoBehaviour to set as the event component.</param>
        public void SetEventComponent(MonoBehaviour value) {
            this.EventComponent = value;
        }

        /// <summary>
        /// Sets the name of the method to be invoked for the event.
        /// </summary>
        /// <param name="value">The method name to set.</param>
        public void SetEventMethod(string value) {
            this.EventMethod = value;
        }

        /// <summary>
        /// Sets the visibility of the event reference in the editor.
        /// </summary>
        /// <param name="value">True to make the event reference visible in the editor; otherwise, false.</param>
        public void SetEditorVisible(bool value) {
            this.EditorVisible = value;
        }

        /// <summary>
        /// Set execution error
        /// </summary>
        /// <param name="value">Execution error value</param>
        public void SetExecutionError(string value) {
            this.ExecutionError = value;
        }

    }
}