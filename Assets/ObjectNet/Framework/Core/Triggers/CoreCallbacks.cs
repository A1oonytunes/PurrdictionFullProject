using System;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// Define a static class to hold core callback actions for network events.
    /// </summary>
    public static class CoreCallbacks {

        /// <summary>
        /// Action to be called when an INetworkEventsCore manager is created.
        /// </summary>
        /// <remarks>
        /// External systems can subscribe to this action to be notified when a new
        /// network events manager is instantiated. The INetworkEventsCore instance
        /// is passed to the subscribers.
        /// </remarks>
        public static Action<INetworkEventsCore> OnEventsManagerCreate;

        /// <summary>
        /// Action to be called when an INetworkEventsCore manager is destroyed.
        /// </summary>
        /// <remarks>
        /// External systems can subscribe to this action to be notified when an
        /// existing network events manager is destroyed. The INetworkEventsCore instance
        /// is passed to the subscribers, allowing them to perform cleanup or other
        /// necessary actions before the manager is completely disposed of.
        /// </remarks>
        public static Action<INetworkEventsCore> OnEventsDestroy;

    }

}