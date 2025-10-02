using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines a contract for handling network events within a system.
    /// </summary>
    public interface INetworkEventsCore {

        /// <summary>
        /// Checks if an event with the specified event code has been registered.
        /// </summary>
        /// <param name="eventCode">The unique identifier for the event.</param>
        /// <returns>True if the event is registered, otherwise false.</returns>
        bool HasEvent(int eventCode);

        /// <summary>
        /// Registers a callback action to be invoked when an event with the specified event code occurs.
        /// </summary>
        /// <param name="eventCode">The unique identifier for the event.</param>
        /// <param name="callBack">The action to be called when the event occurs, receiving an IDataStream as a parameter.</param>
        /// <param name="replace">Replace a previous existent event if event is already registeredexists.</param>
        /// <param name="broadcast">This event shall be broadcast to all players.</param>
        void RegisterEvent(int eventCode, Action<IDataStream> callBack, bool replace = false, bool broadcast = false);
        
        /// <summary>
        /// Executes the callback associated with the specified event code, passing the provided IDataStream to it.
        /// </summary>
        /// <param name="eventCode">The unique identifier for the event.</param>
        /// <param name="reader">The data stream to be passed to the callback.</param>
        void ExecuteEvent(int eventCode, IDataStream reader);

        /// <summary>
        /// Registers an event as a broadcast event, which may be sent to all interested parties.
        /// </summary>
        /// <param name="eventCode">The unique identifier for the event.</param>
        void RegisterBroadcastEvent(int eventCode);

        /// <summary>
        /// Unregisters an event from being a broadcast event.
        /// </summary>
        /// <param name="eventCode">The unique identifier for the event.</param>
        void UnregisterBroadcastEvent(int eventCode);

        /// <summary>
        /// Checks if an event with the specified event code is registered as a broadcast event.
        /// </summary>
        /// <param name="eventCode">The unique identifier for the event.</param>
        /// <returns>True if the event is a broadcast event, otherwise false.</returns>
        bool IsBroadcastEvent(int eventCode);

    }

}