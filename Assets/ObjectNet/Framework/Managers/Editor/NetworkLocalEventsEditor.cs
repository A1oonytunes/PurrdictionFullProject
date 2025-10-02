using System;
using UnityEditor;

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    /// <summary>
    /// Class NetworkLocalEventsEditor.
    /// Implements the <see cref="com.onlineobject.objectnet.NetworkEventsManagerEditor{com.onlineobject.objectnet.NetworkLocalEvents}" />
    /// </summary>
    /// <seealso cref="com.onlineobject.objectnet.NetworkEventsManagerEditor{com.onlineobject.objectnet.NetworkLocalEvents}" />
    [CustomEditor(typeof(NetworkLocalEvents))]
    [CanEditMultipleObjects]
    public class NetworkLocalEventsEditor : NetworkEventsManagerEditor<NetworkLocalEvents> {

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public override void OnEnable() {
            this.SetManager(this.target as NetworkLocalEvents);
            base.OnEnable();
        }

        /// <summary>
        /// Gets the type of the managed.
        /// </summary>
        /// <returns>Type.</returns>
        public override Type GetManagedType() {
            return typeof(NetworkLocalEvents);
        }

    }
#endif
}
