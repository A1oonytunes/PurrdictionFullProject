using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// Defines an interface for a container that manages network elements.
    /// </summary>
    public interface INetworkContainer {
        /// <summary>
        /// Registers a network element with the container.
        /// </summary>
        /// <param name="element">The network element to register.</param>
        void Register(INetworkElement element);

        /// <summary>
        /// Unregisters a network element from the container.
        /// </summary>
        /// <param name="element">The network element to unregister.</param>
        void UnRegister(INetworkElement element);

        /// <summary>
        /// Unregisters a network element using its network ID.
        /// </summary>
        /// <param name="networkId">The network ID of the element to unregister.</param>
        void UnRegister(int networkId);

        /// <summary>
        /// Checks if a network element is registered with the container.
        /// </summary>
        /// <param name="element">The network element to check.</param>
        /// <returns>True if the element is registered, false otherwise.</returns>
        bool IsRegistered(INetworkElement element);

        /// <summary>
        /// Checks if a GameObject is associated with a registered network element.
        /// </summary>
        /// <param name="element">The GameObject to check.</param>
        /// <returns>True if the GameObject is associated with a registered element, false otherwise.</returns>
        bool IsRegistered(GameObject element);

        /// <summary>
        /// Checks if a network ID is associated with a registered network element.
        /// </summary>
        /// <param name="networkId">The network ID to check.</param>
        /// <returns>True if the network ID is associated with a registered element, false otherwise.</returns>
        bool IsRegistered(int networkId);


        /// <summary>
        /// Checks if a network ID is associated with a registered network element exists.
        /// </summary>
        /// <param name="networkId">The network ID to check.</param>
        /// <returns>True if the network ID is associated with a registered element, false otherwise.</returns>
        bool HasElement(int networkId);

        /// <summary>
        /// Checks if a player ID is associated with a registered network element.
        /// </summary>
        /// <param name="playerId">The player ID to check.</param>
        /// <returns>True if the player ID is associated with a registered element, false otherwise.</returns>
        bool IsRegistered(ushort playerId);

        /// <summary>
        /// Updates the network ID for the container, optionally incrementally.
        /// </summary>
        /// <param name="networkId">The new network ID to set.</param>
        /// <param name="incremental">Whether to increment the network ID or set it directly.</param>
        void UpdateNetworkId(int networkId, bool incremental = true);

        /// <summary>
        /// Retrieves the network element associated with a GameObject.
        /// </summary>
        /// <param name="element">The GameObject associated with the network element.</param>
        /// <returns>The network element associated with the GameObject.</returns>
        INetworkElement GetElement(GameObject element);

        /// <summary>
        /// Retrieves the network element associated with a network ID.
        /// </summary>
        /// <param name="networkId">The network ID associated with the network element.</param>
        /// <returns>The network element associated with the network ID.</returns>
        INetworkElement GetElement(int networkId);

        /// <summary>
        /// Retrieves the network element associated with a player ID.
        /// </summary>
        /// <param name="playerId">The player ID associated with the network element.</param>
        /// <returns>The network element associated with the player ID.</returns>
        INetworkElement GetElement(ushort playerId);

        /// <summary>
        /// Retrieves all registered network elements.
        /// </summary>
        /// <returns>An array of all registered network elements.</returns>
        INetworkElement[] GetElements();

        /// <summary>
        /// Registers an events manager with the network container.
        /// </summary>
        /// <param name="eventManager">The events manager to register.</param>
        void RegisterEventsManager(INetworkEventsCore eventManager);

        /// <summary>
        /// Unregisters an events manager from the network container.
        /// </summary>
        /// <param name="eventManager">The events manager to unregister.</param>
        void UnRegisterEventsManager(INetworkEventsCore eventManager);

        /// <summary>
        /// Checks if an event with the specified event code exists.
        /// </summary>
        /// <param name="eventCode">The event code to check.</param>
        /// <returns>True if the event exists, false otherwise.</returns>
        bool HasEvent(int eventCode);

        /// <summary>
        /// Invokes an event with the specified event code, passing in a data stream.
        /// </summary>
        /// <param name="eventCode">The event code to invoke.</param>
        /// <param name="reader">The data stream to pass to the event.</param>
        void InvokeEvent(int eventCode, IDataStream reader);
    }

}