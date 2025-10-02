using System;
using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// This class represents an transport entry on transports database
    /// </summary>
    [Serializable]
    public class NetworkTransportEntry {

        /// <summary>
        /// The code
        /// </summary>
        [SerializeField]
        private int Code;

        /// <summary>
        /// The name
        /// </summary>
        [SerializeField]
        private string Name;

        /// <summary>
        /// The server
        /// </summary>
        [SerializeField]
        private string Server;

        /// <summary>
        /// The client
        /// </summary>
        [SerializeField]
        private string Client;

        /// <summary>
        /// The allow to remove
        /// </summary>
        [SerializeField]
        private bool AllowToRemove;

        /// <summary>
        /// The active
        /// </summary>
        [SerializeField]
        private bool Active;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkTransportEntry"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="name">The name.</param>
        /// <param name="server">The server.</param>
        /// <param name="client">The client.</param>
        /// <param name="allowRemove">if set to <c>true</c> [allow remove].</param>
        public NetworkTransportEntry(int code, string name, string server, string client, bool allowRemove) {
            this.Code = code;
            this.Name = name;
            this.Server = server;
            this.Client = client;
            this.AllowToRemove = allowRemove;
        }

        /// <summary>
        /// Sets the code.
        /// </summary>
        /// <param name="code">The code.</param>
        public void SetCode(int code) {
            this.Code = code;
        }

        /// <summary>
        /// Sets the name.
        /// </summary>
        /// <param name="name">The name.</param>
        public void SetName(string name) {
            this.Name = name;
        }

        /// <summary>
        /// Sets the server.
        /// </summary>
        /// <param name="server">The server.</param>
        public void SetServer(string server) {
            this.Server = server;
        }

        /// <summary>
        /// Sets the active.
        /// </summary>
        /// <param name="active">if set to <c>true</c> [active].</param>
        public void SetActive(bool active) {
            this.Active = active;
        }

        /// <summary>
        /// Gets the code.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetCode() {
            return this.Code;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetName() {
            return this.Name;
        }

        /// <summary>
        /// Gets the server.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetServer() {
            return this.Server;
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetClient() {
            return this.Client;
        }

        /// <summary>
        /// Determines whether [is allowed to remove].
        /// </summary>
        /// <returns><c>true</c> if [is allowed to remove]; otherwise, <c>false</c>.</returns>
        public bool IsAllowedToRemove() {
            return this.AllowToRemove;
        }

        /// <summary>
        /// Determines whether this instance is active.
        /// </summary>
        /// <returns><c>true</c> if this instance is active; otherwise, <c>false</c>.</returns>
        public bool IsActive() {
            return this.Active;
        }

    }

}