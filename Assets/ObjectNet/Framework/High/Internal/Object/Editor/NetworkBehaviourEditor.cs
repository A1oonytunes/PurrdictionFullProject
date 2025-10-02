#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
#endif

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    [CustomEditor(typeof(NetworkBehaviour), true)]
    [CanEditMultipleObjects]
    public class NetworkBehaviourEditor : NetworkObjectEditor {

        SerializedProperty BehaviorlId;

        SerializedProperty ShowNetworkParameters;

        public override void OnEnable() {
            this.BehaviorlId            = serializedObject.FindProperty("BehaviorlId");
            this.ShowNetworkParameters  = serializedObject.FindProperty("ShowNetworkParameters");
            this.SetManager(this.target as NetworkBehaviour);
            base.OnEnable();
        }


        public override Type GetManagedType() {
            return typeof(NetworkBehaviour);
        }

        public override void OnInspectorGUI() {
            EditorGUILayout.BeginVertical();

            serializedObject.Update();

            // Try to fill InternalId if not exists
#if UNITY_2022_1_OR_NEWER
            if ( this.BehaviorlId.uintValue == 0 ) {
#else
            if (this.BehaviorlId.intValue == 0) {
#endif
                int maxChildLoops = 100;
                Transform current = this.GetTarget<NetworkBehaviour>().transform;
                while ((current.parent != null) && (maxChildLoops > 0)) {
                    current = current.parent;
                    maxChildLoops--;
                }
                NetworkBehaviour[] componentBehaviours = current.gameObject.GetComponentsInChildren<NetworkBehaviour>();
                ushort lastComponentId = 0;
                // Find the last id of components
                foreach (NetworkBehaviour child in componentBehaviours) {
                    lastComponentId = (ushort)Mathf.Max(lastComponentId, child.GetBehaviorId());                    
                }
                // Set object behavior ID of this component
                #if UNITY_2022_1_OR_NEWER
                    this.BehaviorlId.uintValue = (ushort)(lastComponentId + 1);
                #else
                    this.BehaviorlId.intValue = (ushort)(lastComponentId + 1);
                #endif
                
            }

            EditorUtils.PrintImage("objectnet_logo", Color.blue, 0, 35);
            GUILayout.Space(5.0f);

            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(0.05f)));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(2.0f);
            EditorGUILayout.BeginHorizontal();
            EditorUtils.PrintVisibilityBoolean(ref this.ShowNetworkParameters, string.Format("Network Options [ {0} ]",
#if UNITY_2022_1_OR_NEWER                    
                this.BehaviorlId.uintValue),
#else
                this.BehaviorlId.intValue),
#endif 
                null, 18, 14, false);
            this.ShowNetworkParameters.boolValue = true;
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(5.0f);
            EditorUtils.HorizontalLine(Color.white.WithAlpha(0.5f), 1.0f, new Vector2(5f, 5f));
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);
            EditorUtils.PrintExplanationLabel("NetworkBehaviour brings the network features to a custom component.", "oo_info", Color.yellow, 5f);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10.0f);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5.0f);
            serializedObject.ApplyModifiedProperties();
            
            base.OnInspectorGUI();                
            
            GUILayout.Space(2.0f);
            EditorUtils.HorizontalLine(Color.white.WithAlpha(0.5f), 1.0f, new Vector2(5f, 5f));
            GUILayout.Space(2.0f);
            EditorUtils.PrintExplanationLabel(string.Format("[ {0} ] script attributes", this.GetTarget<NetworkBehaviour>().GetType().Name), "oo_script_invisible", Color.white, 15, 14, -2);
            EditorUtils.HorizontalLine(Color.white.WithAlpha(0.5f), 1.0f, new Vector2(5f, 1f));
            EditorGUILayout.EndVertical();
            this.DrawDefaultInspector();
        }
    }
#endif
}