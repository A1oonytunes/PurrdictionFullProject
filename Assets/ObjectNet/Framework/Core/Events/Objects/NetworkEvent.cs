using System;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// Represents a network event with a specific action to be executed.
    /// </summary>
    public class NetworkEvent : NetworkEventBase {

        // Event code that uniquely identifies the network event.
        private int eventCode;

        // The action to be executed when the event is triggered.
        private Action<IDataStream> actionEvent;

        /// <summary>
        /// Initializes a new instance of the NetworkEvent class.
        /// </summary>
        /// <param name="eventCode">The unique code for the event.</param>
        /// <param name="actionEvent">The action to be executed when the event is triggered.</param>
        public NetworkEvent(int eventCode, Action<IDataStream> actionEvent) {
            this.eventCode = eventCode;
            this.actionEvent = actionEvent;
        }

        /// <summary>
        /// Executes the associated action for this network event.
        /// </summary>
        /// <param name="reader">The data stream to be passed to the action.</param>
        public override void ExecuteEvent(IDataStream reader) {
            // Check if the action event is not null before invoking.
            if (this.actionEvent != null) {
                this.actionEvent.Invoke(reader);
            }
        }

        /// <summary>
        /// Retrieves the event code associated with this network event.
        /// </summary>
        /// <returns>The unique event code.</returns>
        public override int GetEvent() {
            return this.eventCode;
        }
    }

}