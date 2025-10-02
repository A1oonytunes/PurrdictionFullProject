using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a custom database for network events, stored as a ScriptableObject.
    /// This allows for easy editing and management of event references within the Unity Editor.
    /// </summary>
    public class NetworkCustomDatabase : ScriptableObject {
        // A list of EventReference objects that this database will hold.
        // This can be populated in the Unity Editor and accessed at runtime.
        public List<EventReference> Events = new List<EventReference>();
    }
}