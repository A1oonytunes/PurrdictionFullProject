using System;
using UnityEditor;

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    /// <summary>
    /// Class ListenerReferenceEditor.
    /// Implements the <see cref="com.onlineobject.objectnet.EventReferenceEditor" />
    /// Implements the <see cref="com.onlineobject.objectnet.IEventEditor" />
    /// </summary>
    /// <seealso cref="com.onlineobject.objectnet.EventReferenceEditor" />
    /// <seealso cref="com.onlineobject.objectnet.IEventEditor" />
    [CustomEditor(typeof(EventListenerReference))]
    [CanEditMultipleObjects]
    public class ListenerReferenceEditor : EventReferenceEditor, IEventEditor {

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public override void OnEnable() {
            base.OnEnable();
        }

        /// <summary>
        /// Determines whether [has overriden event types].
        /// </summary>
        /// <returns><c>true</c> if [has overriden event types]; otherwise, <c>false</c>.</returns>
        public override bool HasOverridenEventTypes() {
            return true;
        }

        /// <summary>
        /// Gets the type of the return.
        /// </summary>
        /// <returns>Type.</returns>
        public override Type GetReturnType() {
            return typeof(void);
        }

        /// <summary>
        /// Gets the type of the parameters.
        /// </summary>
        /// <returns>Type[].</returns>
        public override Type[] GetParametersType() {
            return new Type[] { 
                typeof(IDataStream)
            };
        }
    }
#endif
}
