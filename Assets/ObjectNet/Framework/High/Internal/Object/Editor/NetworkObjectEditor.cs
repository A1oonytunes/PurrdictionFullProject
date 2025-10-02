using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    [CustomEditor(typeof(NetworkObject))]
    [CanEditMultipleObjects]
    public class NetworkObjectEditor : NetworkLocalEventsEditor {

        NetworkObject networkObject;

        SerializedProperty networkSyncPosition;
        SerializedProperty networkSyncRotation;
        SerializedProperty networkSyncScale;
        SerializedProperty behaviorMode;

        public override void OnEnable() {
            base.OnEnable();
            this.networkObject = (this.target as NetworkObject);
            if (this.networkObject != null) {
                this.networkObject.InitializeSerializableAttributes();
            }
            this.networkSyncPosition    = serializedObject.FindProperty("syncPosition");
            this.networkSyncRotation    = serializedObject.FindProperty("syncRotation");
            this.networkSyncScale       = serializedObject.FindProperty("syncScale");
            this.behaviorMode           = serializedObject.FindProperty("behaviorMode");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            if (!Application.isPlaying) {
                EditorGUILayout.BeginHorizontal();
                EditorUtils.PrintImageButton("Documentation", "oo_document", EditorUtils.IMAGE_BUTTON_COLOR, EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                    Help.BrowseURL("https://onlineobject.net/objectnet/docs/manual/ObjectNet.html");
                });
                EditorUtils.PrintImageButton("Tutorial", "oo_youtube", EditorUtils.IMAGE_BUTTON_COLOR, EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                    Help.BrowseURL("https://www.youtube.com/@TheObjectNet");
                });
                EditorGUILayout.EndHorizontal();
            }

            if (Application.isPlaying) {
                string currentMode = "";
                if (this.behaviorMode.enumValueIndex == (int)BehaviorMode.Active) {
                    currentMode = "Active Mode";
                } else if (this.behaviorMode.enumValueIndex == (int)BehaviorMode.Passive) {
                    currentMode = "Passive Mode";
                } else if (this.behaviorMode.enumValueIndex == (int)BehaviorMode.Both) {
                    currentMode = "Both Mode";
                }
                EditorUtils.PrintHeader(string.Format("[{0}] Network Behaviour [{1}]", (networkObject != null) ? networkObject.GetNetworkId() : "none", currentMode), Color.blue, Color.white, 14, "oo_gizmo");
            }
            GUILayout.Space(5);
            serializedObject.ApplyModifiedProperties();            
        }

        public T GetTarget<T>() where T : NetworkObject {
            return (T)this.networkObject;
        }
    }
#endif
}