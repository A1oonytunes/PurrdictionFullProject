using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    [CustomEditor(typeof(NetworkInternalExecutor))]
    [CanEditMultipleObjects]
    public class NetworkInternalExecutorEditor : Editor {
        /// <summary>
        /// The network manager debugger
        /// </summary>
        NetworkInternalExecutor networkExecutor;

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public void OnEnable() {
            this.networkExecutor = (this.target as NetworkInternalExecutor);            
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorUtils.PrintImage("objectnet_logo", Color.blue, 0, 25);

            GUILayout.Space(5.0f);

            if (Application.isPlaying) {
                EditorUtils.PrintImageButton(string.Format("Behaviours in execution [{0}]", this.networkExecutor.GetBehaviourCounts()), "oo_info", Color.red.WithAlpha(0.15f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                });
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}