using System;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a network player tag attached to a game object.
    /// </summary>
    public class NetworkPlayerTag : MonoBehaviour
    {

        private object[] LoginAttributeValues;  // Array to store login attribute values

        private Type[] LoginAttributeTypes;   // Array to store login attribute types

        private ushort playerIndex = 0; // Index of player into the game

        /// <summary>
        /// Sets the login attribute values.
        /// </summary>
        /// <param name="values">The values to be set.</param>
        public void SetAttributesValues(object[] values)
        {
            this.LoginAttributeValues = values;
        }

        /// <summary>
        /// Sets the login attribute types.
        /// </summary>
        /// <param name="values">The types to be set.</param>
        public void SetAttributesTypes(Type[] values)
        {
            this.LoginAttributeTypes = values;
        }

        /// <summary>
        /// Sets the player index.
        /// </summary>
        /// <param name="index">The player index on game.</param>
        public void SetPlayerIndex(ushort index) {
            this.playerIndex = index;
        }

        /// <summary>
        /// Gets the login attribute values.
        /// </summary>
        /// <returns>The login attribute values.</returns>
        public object[] GetAttributesValues()
        {
            return this.LoginAttributeValues;
        }

        /// <summary>
        /// Gets the login attribute types.
        /// </summary>
        /// <returns>The login attribute types.</returns>
        public Type[] GetAttributeTypes()
        {
            return this.LoginAttributeTypes;
        }

        /// <summary>
        /// Gets player index.
        /// </summary>
        /// <returns>The player index.</returns>
        public ushort GetPlayerIndex() {
            return this.playerIndex;
        }
    }
}
