using System;
using UnityEditor;

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    /// <summary>
    /// Class NetworkGlobalEventsEditor.
    /// Implements the <see cref="com.onlineobject.objectnet.NetworkEventsManagerEditor{com.onlineobject.objectnet.NetworkGlobalEvents}" />
    /// </summary>
    /// <seealso cref="com.onlineobject.objectnet.NetworkEventsManagerEditor{com.onlineobject.objectnet.NetworkGlobalEvents}" />
    [CustomEditor(typeof(NetworkGlobalEvents))]
    [CanEditMultipleObjects]
    public class NetworkGlobalEventsEditor : NetworkEventsManagerEditor<NetworkGlobalEvents> {

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public override void OnEnable() {
            this.SetManager(this.target as NetworkGlobalEvents);
            base.OnEnable();
        }

        /// <summary>
        /// Gets the type of the managed.
        /// </summary>
        /// <returns>Type.</returns>
        public override Type GetManagedType() {
            return typeof(NetworkGlobalEvents);
        }

    }
#endif
}
