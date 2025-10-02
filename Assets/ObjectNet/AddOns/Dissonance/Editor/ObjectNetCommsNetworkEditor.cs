#if DISSONANCE_ENABLED
using Dissonance;
using Dissonance.Editor;
using UnityEngine;
#else
using UnityEngine;
using System;
#endif
using UnityEditor;

namespace com.onlineobject.objectnet.voice {
    [CustomEditor(typeof(ObjectNetCommsNetwork))]
    public class ObjectNetCommsNetworkEditor
#if DISSONANCE_ENABLED
        : BaseDissonnanceCommsNetworkEditor<ObjectNetCommsNetwork, ObjectNetVoiceServer, ObjectNetVoiceClient, ObjectNetVoicePeer, Unit, Unit> {
#else
        : Editor {
#endif

#if !DISSONANCE_ENABLED

        /// <summary>
        /// Dissonance
        /// </summary>
        const string DISSONANCE_TRANSPORT_ASSEMBLY = "DissonanceVoip";

        /// <summary>
        /// 
        /// </summary>
        const string DISSONANCE_TRANSPORT_NAMESPACE = "Dissonance.IDissonancePlayer";

        /// <summary>
        /// 
        /// </summary>
        const string DISSONANCE_TRANSPORT_FULL_NAMESPACE = DISSONANCE_TRANSPORT_NAMESPACE + ", " + DISSONANCE_TRANSPORT_ASSEMBLY;

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public void OnEnable() {            
        }

        public override void OnInspectorGUI() {
            EditorUtils.PrintImage("objectnet_dissonance_logo", Color.blue, 0, 25);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.Space(5);
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(0.10f)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 35.0f));
            EditorGUILayout.Space(5);
            Type dissonanceClassType = Type.GetType(DISSONANCE_TRANSPORT_FULL_NAMESPACE);
            if (dissonanceClassType == null) {
                EditorUtils.PrintExplanationLabel("Dissonance is not installed on your project; please install Dissonance before trying to use it.", "oo_error", Color.yellow.WithAlpha(1.0f));
            } else {
                EditorUtils.PrintExplanationLabel("Press \"Activate Dissonance\" button to enable voice channel over Dissonance on your project.", "oo_place_holder", Color.yellow.WithAlpha(1.0f));
            }
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);

            if (dissonanceClassType == null) {
                EditorGUI.BeginDisabledGroup(true);
            }
            EditorUtils.PrintImageButton("Activate Dissonance", "oo_dissonance", (dissonanceClassType == null) ? Color.gray.WithAlpha(0.55f) : Color.red.WithAlpha(0.25f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                var buildGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
                string defineValues = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildGroup);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildGroup, defineValues + ";DISSONANCE_ENABLED");                
            });
            if (dissonanceClassType == null) {
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
#else
        public override void OnInspectorGUI() {
            EditorUtils.PrintImage("objectnet_dissonance_logo", Color.blue, 0, 25);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.Space(5);
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(0.10f)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 35.0f));
            EditorGUILayout.Space(5);
            EditorUtils.PrintExplanationLabel("Press \"Remove Dissonance\" if you wish to remove dissonance from your system; this will remove the Dissonance support from ObjectNet.", "oo_place_holder", Color.yellow.WithAlpha(1.0f));
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
            EditorUtils.PrintImageButton("Remove Dissonance", "oo_dissonance", Color.red.WithAlpha(0.25f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
#pragma warning disable CS0618
                var buildGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
                string defineValues = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildGroup).Replace("DISSONANCE_ENABLED", "").Replace(";;", ";");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildGroup, defineValues);
#pragma warning restore CS0618
            });
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();            
        }
#endif
    }
}