using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    [CustomEditor(typeof(NetworkPlayerTag))]
    [CanEditMultipleObjects]
    public class NetworkPlayerTagEditor : Editor {
        /// <summary>
        /// The network manager debugger
        /// </summary>
        NetworkPlayerTag networkPlayerTag;

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public void OnEnable() {
            this.networkPlayerTag = (this.target as NetworkPlayerTag);            
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorUtils.PrintImage("objectnet_logo", Color.blue, 0, 25);

            GUILayout.Space(5.0f);

            if (Application.isPlaying) {
                EditorUtils.PrintImageButton(string.Format("Player Index [{0}]", this.networkPlayerTag.GetPlayerIndex()), "oo_info", Color.red.WithAlpha(0.15f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                });
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}