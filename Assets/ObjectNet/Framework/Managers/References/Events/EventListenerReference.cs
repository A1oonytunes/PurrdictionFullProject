using System;
using UnityEngine;


namespace com.onlineobject.objectnet {

    [Serializable]
    /// <summary>
    /// Represents a reference to an event listener, including its name and code.
    /// Inherits from the EventReference class.
    /// </summary>
    public class EventListenerReference : EventReference {

        // Serialized field to store the name of the event. This allows the event name to be set in the Unity Inspector.
        [SerializeField]
        private string EventName;

        // Serialized field to store the unique code of the event. This allows the event code to be set in the Unity Inspector.
        [SerializeField]
        private int EventCode;

        /// <summary>
        /// Default constructor for the EventListenerReference class.
        /// Calls the base class constructor.
        /// </summary>
        public EventListenerReference() : base() {
        }

        /// <summary>
        /// Retrieves the name of the event.
        /// </summary>
        /// <returns>The name of the event as a string.</returns>
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
        /// Retrieves the unique code of the event.
        /// </summary>
        /// <returns>The event code as an integer.</returns>
        public int GetEventCode() {
            return this.EventCode;
        }

        /// <summary>
        /// Sets the unique code for the event.
        /// </summary>
        /// <param name="code">The code to set for the event.</param>
        public void SetEventCode(int code) {
            this.EventCode = code;
        }
    }


}