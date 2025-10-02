using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a database of network transports.
    /// </summary>
    public class NetworkTransportsDatabase : ScriptableObject {

        [HideInInspector]
        [SerializeField]
        private List<NetworkTransportEntry> Transports = new List<NetworkTransportEntry>();

        const int TRANSPORTS_OFFSET = 1;

        /// <summary>
        /// Retrieves an array of all network transport entries.
        /// </summary>
        /// <returns>An array of NetworkTransportEntry objects.</returns>
        public NetworkTransportEntry[] GetTransports() {
            return this.Transports.ToArray();
        }

        /// <summary>
        /// Checks if a transport with the specified name exists.
        /// </summary>
        /// <param name="name">The name of the transport to check.</param>
        /// <returns>True if the transport exists, otherwise false.</returns>
        public bool TransportExists(string name) {
            bool result = false;
            if (!string.IsNullOrEmpty(name)) {
                foreach (NetworkTransportEntry transportEntry in this.Transports) {
                    result |= (transportEntry.GetName().ToUpper().Equals(name.ToUpper()));
                    if (result) {
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if a transport with the specified code exists.
        /// </summary>
        /// <param name="code">The code of the transport to check.</param>
        /// <returns>True if the transport exists, otherwise false.</returns>
        public bool TransportExists(int code) {
            bool result = false;
            foreach (NetworkTransportEntry transportEntry in this.Transports) {
                result |= (transportEntry.GetCode().Equals(code));
                if (result) {
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves the transport code associated with a given transport name.
        /// </summary>
        /// <param name="name">The name of the transport to look up.</param>
        /// <returns>The code of the transport if found; otherwise, 0.</returns>
        public int GetTransportCode(string name) {
            int result = 0;
            // Check if the provided name is not null or empty
            if (!string.IsNullOrEmpty(name)) {
                // Iterate through the list of transports
                foreach (NetworkTransportEntry transportEntry in this.Transports) {
                    // Compare the transport name case-insensitively
                    if (transportEntry.GetName().ToUpper().Equals(name.ToUpper())) {
                        result = transportEntry.GetCode();
                        break; // Exit the loop once the transport is found
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves the transport name associated with a given transport code.
        /// </summary>
        /// <param name="code">The code of the transport to look up.</param>
        /// <returns>The name of the transport if found; otherwise, null.</returns>
        public string GetTransportName(int code) {
            string result = null;
            // Iterate through the list of transports
            foreach (NetworkTransportEntry transportEntry in this.Transports) {
                // Check if the current transport's code matches the provided code
                if (transportEntry.GetCode().Equals(code)) {
                    result = transportEntry.GetName();
                    break; // Exit the loop once the transport is found
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves the transport entry associated with a given transport name.
        /// </summary>
        /// <param name="name">The name of the transport to look up.</param>
        /// <returns>The transport entry if found; otherwise, null.</returns>
        public NetworkTransportEntry GetTransport(string name) {
            NetworkTransportEntry result = null;
            // Check if the provided name is not null or empty
            if (!string.IsNullOrEmpty(name)) {
                // Iterate through the list of transports
                foreach (NetworkTransportEntry transportEntry in this.Transports) {
                    // Compare the transport name case-insensitively
                    if (transportEntry.GetName().ToUpper().Equals(name.ToUpper())) {
                        result = transportEntry;
                        break; // Exit the loop once the transport is found
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Return if a transport systenm is registered.
        /// </summary>
        /// <param name="name">The name of the transport to look up.</param>
        /// <returns>true if found; otherwise, false.</returns>
        public bool HasTransport(string name) {
            bool result = false;
            // Check if the provided name is not null or empty
            if (!string.IsNullOrEmpty(name)) {
                // Iterate through the list of transports
                foreach (NetworkTransportEntry transportEntry in this.Transports) {
                    // Compare the transport name case-insensitively
                    if (transportEntry.GetName().ToUpper().Equals(name.ToUpper())) {
                        result = true;
                        break; // Exit the loop once the transport is found
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Registers a new transport with the given details, or retrieves an existing one if it already exists.
        /// </summary>
        /// <param name="name">The name of the transport to register.</param>
        /// <param name="server">The server associated with the transport.</param>
        /// <param name="client">The client associated with the transport.</param>
        /// <param name="allowDelete">Indicates whether the transport can be deleted. Defaults to true.</param>
        /// <returns>The registered or retrieved transport entry.</returns>
        public NetworkTransportEntry RegisterTransport(string name, string server, string client, bool allowDelete = true) {
            NetworkTransportEntry result = null;
            // Check if the transport already exists
            if (!this.TransportExists(name)) {
                // Determine the next available code for the new transport
                int nextCode = (this.Transports.Count > 0) ? (this.Transports.OrderBy(e => e.GetCode()).Last().GetCode() + 1) : TRANSPORTS_OFFSET;
                // Create a new transport entry
                result = new NetworkTransportEntry(nextCode, name, server, client, allowDelete);
                // Add the new transport to the list
                this.Transports.Add(result);
            } else {
                // Retrieve the existing transport entry
                result = this.GetTransport(name);
            }
            return result;
        }

        /// <summary>
        /// Unregisters a transport by its name.
        /// </summary>
        /// <param name="name">The name of the transport to unregister.</param>
        public void UnregisterTransport(string name) {
            // Check if the transport exists before attempting to remove it
            if (this.TransportExists(name)) {
                this.Transports.Remove(this.GetTransport(name));
            }
        }

        /// <summary>
        /// Unregisters a transport by its entry.
        /// </summary>
        /// <param name="eventToRemove">The transport entry to remove.</param>
        public void UnregisterTransport(NetworkTransportEntry eventToRemove) {
            // Check if the transport entry is in the list before attempting to remove it
            if (this.Transports.Contains(eventToRemove)) {
                this.Transports.Remove(eventToRemove);
            }
        }

        /// <summary>
        /// Retrieves the names of all registered transports, excluding any specified transports.
        /// </summary>
        /// <param name="transportsToHide">An array of transport names to exclude from the result.</param>
        /// <returns>An array of transport names.</returns>
        public string[] GetRegisteredTransportsName(params string[] transportsToHide) {
            List<string> result = new List<string>();
            // Iterate through the list of transports
            foreach (NetworkTransportEntry transportEntry in this.Transports) {
                // Check if the current transport should be excluded
                if ((transportsToHide == null) || (!transportsToHide.Contains(transportEntry.GetName()))) {
                    result.Add(transportEntry.GetName());
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Retrieves the currently active transport entry.
        /// </summary>
        /// <returns>The active transport entry if one is active; otherwise, null.</returns>
        public NetworkTransportEntry GetActiveTransport() {
            NetworkTransportEntry result = null;
            // Iterate through the list of transports to find the active one
            foreach (NetworkTransportEntry transportEntry in this.Transports) {
                if (transportEntry.IsActive()) {
                    result = transportEntry;
                    break; // Exit the loop once the active transport is found
                }
            }
            return result;
        }

        /// <summary>
        /// Activate default trasnport system in case ha no selected transport
        /// </summary>
        public void ActivateDefaultTransport() {
            if (this.Transports.Count > 0) {
                this.Transports[0].SetActive(true);
            }
        }


    }
}