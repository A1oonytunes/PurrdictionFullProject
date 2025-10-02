using System;
using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// Represents an entry for a network event, containing a code and a name.
    /// </summary>
    [Serializable]
    public class NetworkEventEntry {

        /// <summary>
        /// The unique code identifying the network event.
        /// </summary>
        [SerializeField]
        private int Code;

        /// <summary>
        /// The name or description of the network event.
        /// </summary>
        [SerializeField]
        private string Name;

        /// <summary>
        /// Constructs a new NetworkEventEntry with the specified code and name.
        /// </summary>
        /// <param name="code">The unique code for the network event.</param>
        /// <param name="name">The name or description of the network event.</param>
        public NetworkEventEntry(int code, string name) {
            this.Code = code;
            this.Name = name;
        }

        /// <summary>
        /// Sets the code for the network event.
        /// </summary>
        /// <param name="code">The new code to be set for the event.</param>
        public void SetCode(int code) {
            this.Code = code;
        }

        /// <summary>
        /// Sets the name for the network event.
        /// </summary>
        /// <param name="name">The new name to be set for the event.</param>
        public void SetName(string name) {
            this.Name = name;
        }

        /// <summary>
        /// Gets the code of the network event.
        /// </summary>
        /// <returns>The code of the event.</returns>
        public int GetCode() {
            return this.Code;
        }

        /// <summary>
        /// Gets the name of the network event.
        /// </summary>
        /// <returns>The name of the event.</returns>
        public string GetName() {
            return this.Name;
        }
    }

}