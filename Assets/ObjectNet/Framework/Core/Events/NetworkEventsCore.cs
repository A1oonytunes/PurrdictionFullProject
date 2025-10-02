using System;
using System.Collections.Generic;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Core class for managing network events within a system.
    /// </summary>
    public class NetworkEventsCore : INetworkEventsCore {

        // List to keep track of event codes that are meant for broadcasting.
        private List<int> broadcastEventsCode = new List<int>();

        // Dictionary to map event codes to their corresponding network event handlers.
        private Dictionary<int, INetworkEvent> events = new Dictionary<int, INetworkEvent>();

        /// <summary>
        /// Constructor for the NetworkEventsCore class.
        /// </summary>
        public NetworkEventsCore() {
            // Invoke the event manager creation callback if it's set.
            if (CoreCallbacks.OnEventsManagerCreate != null) {
                CoreCallbacks.OnEventsManagerCreate.Invoke(this);
            }
        }

        /// <summary>
        /// Checks if an event with the specified code is registered.
        /// </summary>
        /// <param name="eventCode">The event code to check.</param>
        /// <returns>True if the event is registered; otherwise, false.</returns>
        public bool HasEvent(int eventCode) {
            return this.events.ContainsKey(eventCode);
        }

        /// <summary>
        /// Registers a new event with a callback action.
        /// </summary>
        /// <param name="eventCode">The unique code for the event.</param>
        /// <param name="callBack">The callback action to be executed when the event is triggered.</param>
        /// <param name="replace">Replace a previous existent event if event is already registeredexists.</param>
        /// <exception cref="Exception">Thrown when the event is already registered.</exception>
        /// <param name="broadcast">This event shall be broadcast to all players.</param>
        public void RegisterEvent(int eventCode, Action<IDataStream> callBack, bool replace = false, bool broadcast = false) {
            if ((!this.events.ContainsKey(eventCode)) || (replace)) {
                if (!this.events.ContainsKey(eventCode)) {
                    this.events.Add(eventCode, new NetworkEvent(eventCode, callBack));
                } else if (replace)  {
                    this.events[eventCode] = new NetworkEvent(eventCode, callBack);
                } else {
                    throw new Exception(String.Format("Event \"{0}\" is already registered", eventCode.ToString()));
                }
                // If event shall be broadcast i need to register it
                if (broadcast) {
                    this.RegisterBroadcastEvent(eventCode);
                } else if (replace) {
                    this.UnregisterBroadcastEvent(eventCode); // If is registered i'm going to unregister
                }
            } else {
                throw new Exception(String.Format("Event \"{0}\" is already registered", eventCode.ToString()));
            }
        }

        /// <summary>
        /// Executes the event associated with the given event code.
        /// </summary>
        /// <param name="eventCode">The event code to trigger.</param>
        /// <param name="reader">The data stream to be passed to the event's callback.</param>
        /// <exception cref="Exception">Thrown when the event is not registered.</exception>
        public void ExecuteEvent(int eventCode, IDataStream reader) {
            if (this.events.ContainsKey(eventCode)) {
                this.events[eventCode].ExecuteEvent(reader);
            } else {
                throw new Exception(String.Format("Event \"{0}\" is not registered", eventCode.ToString()));
            }
        }

        /// <summary>
        /// Registers an event code as a broadcast event.
        /// </summary>
        /// <param name="eventCode">The event code to be marked for broadcasting.</param>
        public void RegisterBroadcastEvent(int eventCode) {
            if (!this.broadcastEventsCode.Contains(eventCode)) {
                this.broadcastEventsCode.Add(eventCode);
            }
        }

        /// <summary>
        /// Unregisters an event code from being a broadcast event.
        /// </summary>
        /// <param name="eventCode">The event code to be unmarked for broadcasting.</param>
        public void UnregisterBroadcastEvent(int eventCode) {
            if (this.broadcastEventsCode.Contains(eventCode)) {
                this.broadcastEventsCode.Remove(eventCode);
            }
        }

        /// <summary>
        /// Checks if an event code is marked as a broadcast event.
        /// </summary>
        /// <param name="eventCode">The event code to check.</param>
        /// <returns>True if the event code is marked for broadcasting; otherwise, false.</returns>
        public bool IsBroadcastEvent(int eventCode) {
            return this.broadcastEventsCode.Contains(eventCode);
        }
    }

}