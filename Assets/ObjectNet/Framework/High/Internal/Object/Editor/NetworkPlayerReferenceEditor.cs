using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    [CustomEditor(typeof(NetworkPlayerReference))]
    [CanEditMultipleObjects]
    public class NetworkPlayerReferenceEditor : Editor {
        /// <summary>
        /// The network manager debugger
        /// </summary>
        NetworkPlayerReference networkPlayerReference;

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public void OnEnable() {
            this.networkPlayerReference = (this.target as NetworkPlayerReference);
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorUtils.PrintImage("objectnet_logo", Color.blue, 0, 25);

            GUILayout.Space(5.0f);

            if (Application.isPlaying) {
                EditorUtils.PrintImageButton(string.Format("Player Id [{0}]", this.networkPlayerReference.GetPlayerId()), "oo_info", Color.red.WithAlpha(0.15f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                });
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}