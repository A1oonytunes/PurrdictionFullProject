using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// Abstract base class for network containers that manage network elements and network events.
    /// </summary>
    public abstract class NetworkContainerBase : INetworkContainer {

        // Indicates whether network elements should be automatically despawned upon unregistration.
        private bool autoDespawn = false;

        // The highest network ID assigned to a network element within this container.
        private int sourceNetworkId = 0;

        // A dictionary mapping network IDs to their corresponding network elements.
        private Dictionary<int, INetworkElement> objects = new Dictionary<int, INetworkElement>();

        // A list of network event managers that handle network events.
        private List<INetworkEventsCore> events = new List<INetworkEventsCore>();

        /// <summary>
        /// Constructor that initializes the network container and sets up core callbacks.
        /// </summary>
        public NetworkContainerBase() {
            CoreCallbacks.OnEventsManagerCreate = (INetworkEventsCore eventManager) => {
                this.RegisterEventsManager(eventManager);
            };
            CoreCallbacks.OnEventsDestroy = (INetworkEventsCore eventManager) => {
                this.UnRegisterEventsManager(eventManager);
            };
        }

        /// <summary>
        /// Updates the source network ID for the container.
        /// </summary>
        /// <param name="networkId">The new network ID to be set.</param>
        /// <param name="incremental">If true, only updates if the new ID is greater than the current one.</param>
        public void UpdateNetworkId(int networkId, bool incremental = true) {
            this.sourceNetworkId = (incremental) ? Mathf.Max(networkId, this.sourceNetworkId) : networkId;
        }

        /// <summary>
        /// Registers a network element with the container.
        /// </summary>
        /// <param name="element">The network element to register.</param>
        public void Register(INetworkElement element) {
            if (element != null) {
                if (element.GetNetworkId() == 0) {
                    element.SetNetworkId(++this.sourceNetworkId);
                }
                if (!this.objects.ContainsKey(element.GetNetworkId())) {
                    this.objects.Add(element.GetNetworkId(), element);
                    // Update output buffer size
                    TransportDefinitions.AdjustBufferSize(this.objects.Count);
                }
            }
        }

        /// <summary>
        /// Unregisters a network element from the container.
        /// </summary>
        /// <param name="element">The network element to unregister.</param>
        public void UnRegister(INetworkElement element) {
            if (element != null) {
                if (this.objects.ContainsKey(element.GetNetworkId())) {
                    this.objects.Remove(element.GetNetworkId());
                    if (this.autoDespawn) {
                        if (element.GetGameObject() != null) {
                            GameObject.Destroy(element.GetGameObject());
                        }
                    }
                    // Update output buffer size
                    TransportDefinitions.AdjustBufferSize(this.objects.Count);
                }
            }
        }

        /// <summary>
        /// Unregisters a network element from the container by its network ID.
        /// </summary>
        /// <param name="networkId">The network ID of the element to unregister.</param>
        public void UnRegister(int networkId) {
            if (this.objects.ContainsKey(networkId)) {
                INetworkElement elementToRemove = this.objects[networkId];
                this.objects.Remove(networkId);
                if (this.autoDespawn) {
                    if (elementToRemove.GetGameObject() != null) {
                        GameObject.Destroy(elementToRemove.GetGameObject());
                    }
                }
                // Update output buffer size
                TransportDefinitions.AdjustBufferSize(this.objects.Count);
            }
        }

        /// <summary>
        /// Checks if the network element is registered using its network ID.
        /// </summary>
        /// <param name="element">The network element to check.</param>
        /// <returns>True if the element is registered, otherwise false.</returns>
        public bool IsRegistered(INetworkElement element) {
            return this.objects.ContainsKey(element.GetNetworkId());
        }

        /// <summary>
        /// Checks if the GameObject is registered as a network element.
        /// </summary>
        /// <param name="element">The GameObject to check.</param>
        /// <returns>True if the GameObject is associated with a registered network element, otherwise false.</returns>
        public bool IsRegistered(GameObject element) {
            bool result = false;
            foreach (INetworkElement obj in this.objects.Values) {
                result = ((obj.GetGameObject() != null) && (obj.GetGameObject().Equals(element)));
                if (result) {
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if a player is registered using their player ID.
        /// </summary>
        /// <param name="playerId">The player ID to check.</param>
        /// <returns>True if the player is registered, otherwise false.</returns>
        public bool IsRegistered(ushort playerId) {
            bool result = false;
            foreach (INetworkElement obj in this.objects.Values) {
                result = (obj.GetPlayerId().Equals(playerId));
                if (result) {
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if a network element is registered using its network ID.
        /// </summary>
        /// <param name="networkId">The network ID to check.</param>
        /// <returns>True if the network ID is registered, otherwise false.</returns>
        public bool IsRegistered(int networkId) {
            return this.objects.ContainsKey(networkId);
        }

        /// <summary>
        /// Checks if a network ID is associated with a registered network element exists.
        /// </summary>
        /// <param name="networkId">The network ID to check.</param>
        /// <returns>True if the network ID is associated with a registered element, false otherwise.</returns>
        public bool HasElement(int networkId) {
            return this.objects.ContainsKey(networkId);
        }

        /// <summary>
        /// Retrieves the network element associated with the given GameObject.
        /// </summary>
        /// <param name="element">The GameObject to find the network element for.</param>
        /// <returns>The associated network element if found, otherwise null.</returns>
        public INetworkElement GetElement(GameObject element) {
            INetworkElement result = null;
            foreach (INetworkElement obj in this.objects.Values) {
                if ((obj.GetGameObject() != null) && (obj.GetGameObject().Equals(element))) {
                    result = obj;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves the network element associated with the given network ID.
        /// </summary>
        /// <param name="networkId">The network ID to find the network element for.</param>
        /// <returns>The associated network element.</returns>
        public INetworkElement GetElement(int networkId) {
            return this.objects[networkId];
        }

        /// <summary>
        /// Retrieves the network element associated with the given player ID.
        /// </summary>
        /// <param name="playerId">The player ID to find the network element for.</param>
        /// <returns>The associated network element if found, otherwise null.</returns>
        public INetworkElement GetElement(ushort playerId) {
            INetworkElement result = null;
            foreach (INetworkElement element in this.GetElements()) {
                if (element.GetPlayerId().Equals(playerId)) {
                    result = element;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves all registered network elements.
        /// </summary>
        /// <returns>An array of all registered network elements.</returns>
        public INetworkElement[] GetElements() {
            return this.objects.Values.ToArray();
        }

        /// <summary>
        /// Registers an event manager to the network system.
        /// </summary>
        /// <param name="eventManager">The event manager to register.</param>
        public void RegisterEventsManager(INetworkEventsCore eventManager) {
            if (!this.events.Contains(eventManager)) {
                this.events.Add(eventManager);
            }
        }

        /// <summary>
        /// Unregisters an event manager from the network system.
        /// </summary>
        /// <param name="eventManager">The event manager to unregister.</param>
        public void UnRegisterEventsManager(INetworkEventsCore eventManager) {
            if (this.events.Contains(eventManager)) {
                this.events.Remove(eventManager);
            }
        }

        /// <summary>
        /// Checks if any registered event manager has the specified event code.
        /// </summary>
        /// <param name="eventCode">The event code to check for.</param>
        /// <returns>True if any event manager has the event code, otherwise false.</returns>
        public bool HasEvent(int eventCode) {
            bool result = false;
            foreach (INetworkEventsCore eventManager in this.events) {
                try {
                    result |= eventManager.HasEvent(eventCode);
                } catch (Exception err) {
                    NetworkDebugger.Log(String.Format("Error when try to execute event [{0}]", eventCode));
                    NetworkDebugger.LogError(err.Message);
                }
            }
            return result;
        }


        /// <summary>
        /// Invokes a network event by its event code, passing the data stream to the event manager.
        /// </summary>
        /// <param name="eventCode">The code of the event to invoke.</param>
        /// <param name="reader">The data stream containing event data.</param>
        public void InvokeEvent(int eventCode, IDataStream reader) {
            foreach (INetworkEventsCore eventManager in this.events) {
                try {
                    if (eventManager.HasEvent(eventCode)) {
                        eventManager.ExecuteEvent(eventCode, reader);
                    }
                } catch (Exception err) {
                    NetworkDebugger.Log(String.Format("Error when try to execute event [{0}]", eventCode));
                    NetworkDebugger.LogError(err.Message);
                }
            }
        }

    }
}