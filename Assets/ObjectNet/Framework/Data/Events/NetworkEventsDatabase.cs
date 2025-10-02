using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a database of network events.
    /// </summary>
    public class NetworkEventsDatabase : ScriptableObject {

        /// <summary>
        /// The events
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private List<NetworkEventEntry> Events = new List<NetworkEventEntry>();

        /// <summary>
        /// The user events offset
        /// </summary>
        const int USER_EVENTS_OFFSET = 50000;

        /// <summary>
        /// Retrieves an array of all registered network events.
        /// </summary>
        /// <returns>An array of NetworkEventEntry objects.</returns>
        public NetworkEventEntry[] GetEvents() {
            return this.Events.ToArray();
        }

        /// <summary>
        /// Checks if an event with the specified name exists.
        /// </summary>
        /// <param name="name">The name of the event to check for.</param>
        /// <returns>True if the event exists, false otherwise.</returns>
        public bool EventExists(string name) {
            bool result = false;
            foreach (NetworkEventEntry eventEntry in this.Events) {
                result |= (eventEntry.GetName().ToUpper().Equals(name.ToUpper()));
                if (result) {
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if an event with the specified code exists.
        /// </summary>
        /// <param name="code">The code of the event to check for.</param>
        /// <returns>True if the event exists, false otherwise.</returns>
        public bool EventExists(int code) {
            bool result = false;
            foreach (NetworkEventEntry eventEntry in this.Events) {
                result |= (eventEntry.GetCode().Equals(code));
                if (result) {
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves the code of an event by its name.
        /// </summary>
        /// <param name="name">The name of the event.</param>
        /// <returns>The code of the event if found, 0 otherwise.</returns>
        public int GetEventCode(string name) {
            int result = 0;
            foreach (NetworkEventEntry eventEntry in this.Events) {
                if (eventEntry.GetName().ToUpper().Equals(name.ToUpper())) {
                    result = eventEntry.GetCode();
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves the name of an event by its code.
        /// </summary>
        /// <param name="code">The code of the event.</param>
        /// <returns>The name of the event if found, null otherwise.</returns>
        public string GetEventName(int code) {
            string result = null;
            foreach (NetworkEventEntry eventEntry in this.Events) {
                if (eventEntry.GetCode().Equals(code)) {
                    result = eventEntry.GetName();
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves a NetworkEventEntry by its name.
        /// </summary>
        /// <param name="name">The name of the event.</param>
        /// <returns>The NetworkEventEntry if found, null otherwise.</returns>
        public NetworkEventEntry GetEvent(string name) {
            NetworkEventEntry result = null;
            foreach (NetworkEventEntry eventEntry in this.Events) {
                if (eventEntry.GetName().ToUpper().Equals(name.ToUpper())) {
                    result = eventEntry;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Registers a new event with the given name.
        /// </summary>
        /// <param name="name">The name of the event to register.</param>
        /// <returns>The registered NetworkEventEntry, or the existing one if it's already registered.</returns>
        public NetworkEventEntry RegisterEvent(string name) {
            NetworkEventEntry result = null;
            if (!this.EventExists(name)) {
                int nextCode = (this.Events.Count > 0) ? (this.Events.OrderBy(e => e.GetCode()).Last().GetCode() + 1) : USER_EVENTS_OFFSET;
                result = new NetworkEventEntry(nextCode, name);
                this.Events.Add(result);
            } else {
                result = this.GetEvent(name);
            }
            return result;
        }

        /// <summary>
        /// Unregisters an event by its name.
        /// </summary>
        /// <param name="name">The name of the event to unregister.</param>
        public void UnregisterEvent(string name) {
            if (this.EventExists(name)) {
                this.Events.Remove(this.GetEvent(name));
            }
        }

        /// <summary>
        /// Unregisters a specific NetworkEventEntry.
        /// </summary>
        /// <param name="eventToRemove">The NetworkEventEntry to remove.</param>
        public void UnregisterEvent(NetworkEventEntry eventToRemove) {
            if (this.Events.Contains(eventToRemove)) {
                this.Events.Remove(eventToRemove);
            }
        }

        /// <summary>
        /// Retrieves the names of all registered events, optionally excluding specified events.
        /// </summary>
        /// <param name="eventsToHide">An array of event names to exclude from the result.</param>
        /// <returns>An array of event names.</returns>
        public string[] GetRegisteredEventsName(params string[] eventsToHide) {
            List<string> result = new List<string>();
            foreach (NetworkEventEntry eventEntry in this.Events) {
                if ((eventsToHide == null) || (!eventsToHide.Contains(eventEntry.GetName()))) {
                    result.Add(eventEntry.GetName());
                }
            }
            return result.ToArray();
        }

    }
}