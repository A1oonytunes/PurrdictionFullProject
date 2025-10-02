using System;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    /// <summary>
    /// Class NetworkDebuggerManagerEditor.
    /// Implements the <see cref="Editor" />
    /// </summary>
    /// <seealso cref="Editor" />
    [CustomEditor(typeof(NetworkInstantiateDetection))]
    [CanEditMultipleObjects]
    public class NetworkInstantiateDetectionEditor : Editor {

        /// <summary>
        /// The network manager debugger
        /// </summary>
        NetworkInstantiateDetection networkDetection;

        /// <summary>
        /// Store if this object was already in scene when game starts
        /// </summary>
        SerializedProperty staticSpawn;

        /// <summary>
        /// Serialized object network id
        /// </summary>
        SerializedProperty staticId;

        /// <summary>
        /// The enable console log
        /// </summary>
        SerializedProperty instancePrefabSignature;

        /// <summary>
        /// The detail background opacity
        /// </summary>
        const float DETAIL_BACKGROUND_OPACITY = 0.05f;

        /// <summary>
        /// Min id of in scene objects
        /// </summary>
        readonly int MIN_RANGE_BOUNDS = (ushort)Mathf.FloorToInt(ushort.MaxValue / 2.0f);

        /// <summary>
        /// Max id of in scene objects
        /// </summary>
        readonly int MAX_RANGE_BOUNDS = ushort.MaxValue;

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public void OnEnable() {
            this.networkDetection = (this.target as NetworkInstantiateDetection);
            // Get all serializable objects
            this.staticSpawn             = serializedObject.FindProperty("staticSpawn");
            this.staticId                = serializedObject.FindProperty("staticId");
            this.instancePrefabSignature = serializedObject.FindProperty("instancePrefabSignature");
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorUtils.PrintImage("objectnet_logo", Color.blue, 0, 25);

            GUILayout.Space(5.0f);

            if (Application.isPlaying) {
                EditorUtils.PrintImageButton("Changes are disabled during PlayMode", "oo_info", Color.red.WithAlpha(0.15f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                });
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorUtils.PrintImageButton("Documentation", "oo_document", EditorUtils.IMAGE_BUTTON_COLOR, EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                Help.BrowseURL("https://onlineobject.net/objectnet/docs/manual/ObjectNet.html");
            });
            EditorUtils.PrintImageButton("Tutorials", "oo_youtube", EditorUtils.IMAGE_BUTTON_COLOR, EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                Help.BrowseURL("https://www.youtube.com/@TheObjectNet");
            });
            EditorGUILayout.EndHorizontal();

            // Check if peer to peer is enabled
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            GUILayout.Space(15.0f);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10.0f);
            EditorUtils.PrintSimpleExplanation("Prefab Unique Signature ID", EditorUtils.EXPLANATION_FONT_COLOR);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(this.instancePrefabSignature.stringValue, GUILayout.Width(300), GUILayout.Height(38));
            EditorGUI.EndDisabledGroup();
            
            GUILayout.FlexibleSpace();

            bool inPrefabScene  = (this.networkDetection.gameObject.scene == null);
                 inPrefabScene |= ((this.networkDetection.gameObject.scene.buildIndex   == -1) &&
                                   (this.networkDetection.gameObject.scene.rootCount    == 1) &&
                                   (this.networkDetection.gameObject.scene.name         == this.networkDetection.gameObject.name));
            if (inPrefabScene == false) {
                EditorGUI.BeginDisabledGroup(true);
            }

            EditorUtils.PrintImageButton("Re-Generate", "oo_prefab", Color.red.WithAlpha(0.25f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                this.instancePrefabSignature.stringValue = Guid.NewGuid().ToString();

                // Generate internal network ID
                this.staticId.intValue = UnityEngine.Random.Range(MIN_RANGE_BOUNDS, MAX_RANGE_BOUNDS);
            });

            if (inPrefabScene == false) {
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5.0f);
            EditorUtils.PrintExplanationLabel("If you duplicate any prefab you need to re generate prefab signature to keep the signature unique peer prefab", "oo_note");
            GUILayout.Space(5.0f);
            EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
            GUILayout.Space(5.0f);
            EditorUtils.PrintExplanationLabel("If different prefabs use the same signature you may expect some strange behavior since object will be different on each game instance", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
            GUILayout.Space(10.0f);
            EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));

            if ((this.staticSpawn.boolValue == true) && (inPrefabScene == true)) {
                this.staticSpawn.boolValue = false;
                this.staticId.intValue = 0;
            }

            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal("box");
            if (inPrefabScene == true) {
                EditorGUI.BeginDisabledGroup(true);
            }
            bool executeRefresh = EditorUtils.PrintBooleanSquaredByRef(ref this.staticSpawn, "In Scene object", null, 14, 12);
            if (inPrefabScene == true) {
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.FlexibleSpace();
            if(this.staticSpawn.boolValue) {
                EditorUtils.PrintSimpleExplanation("Network Id");
                GUILayout.Space(5.0f);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(this.staticId.intValue.ToString(), GUILayout.Width(120));
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(5.0f);
                if (inPrefabScene == true) {
#if ALLOW_CHANGE_INSCENE_ON_PREFAB
                    EditorGUI.BeginDisabledGroup(true);
#endif
                }
                if (GUILayout.Button(Resources.Load("oo_refresh") as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18)) || executeRefresh) {
                    // Generate internal network ID
                    this.staticId.intValue = UnityEngine.Random.Range(MIN_RANGE_BOUNDS, MAX_RANGE_BOUNDS);
                }
                if (inPrefabScene == true) {
#if ALLOW_CHANGE_INSCENE_ON_PREFAB
                    EditorGUI.EndDisabledGroup();
#endif
                }
                GUILayout.Space(15.0f);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5.0f);
            EditorUtils.PrintSimpleExplanation("Flag this option if this object already exists in scene when game starts", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
            GUILayout.Space(5.0f);
            EditorUtils.PrintExplanationLabel("WARNING", "oo_info");
            GUILayout.Space(5.0f);
            EditorUtils.PrintSimpleExplanation("Don't enable this option directly on prefabs, use only when obejcts is already on scene", Color.red.WithAlpha(0.75f));
            GUILayout.Space(10.0f);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
                }
