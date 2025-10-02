using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Reflection;
using System;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace com.onlineobject.objectnet {
#if UNITY_EDITOR
    /// <summary>
    /// This class draws a Network Prefabs Database Window.
    /// Implements the <see cref="EditorWindow" />
    /// </summary>
    /// <seealso cref="EditorWindow" />
    public class NetworkPrefabsDatabaseWindow : EditorWindow {
        /// <summary>
        /// The scroll position
        /// </summary>
        private Vector2 scrollPos;

        /// <summary>
        /// The prefab to register
        /// </summary>
        private GameObject prefabToRegister;

        /// <summary>
        /// The prefabs database
        /// </summary>
        private NetworkPrefabsDatabase prefabsDatabase;

        /// <summary>
        /// The register error
        /// </summary>
        private string registerError = null;

        /// <summary>
        /// The target database
        /// </summary>
        private string targetDatabase = GlobalResources.DEFAULT_DATABASE;

        /// <summary>
        /// Filter to be applied on prefabs
        /// </summary>
        private string prefabsFilter = "";

        /// <summary>
        /// The detail background opacity
        /// </summary>
        const float DETAIL_BACKGROUND_OPACITY = 0.05f;

        /// <summary>
        /// The transport background alpha
        /// </summary>
        const float TRANSPORT_BACKGROUND_ALPHA = 0.25f;

        /// <summary>
        /// The detail background alpha
        /// </summary>
        const float DETAIL_BACKGROUND_ALPHA = 0.05f;

        /// <summary>
        /// Opens the network prefabs database window.
        /// </summary>
        /// <param name="database">The database.</param>
        public static void OpenNetworkPrefabsDatabaseWindow(string database = GlobalResources.DEFAULT_DATABASE) {
            NetworkPrefabsDatabaseWindow window = ScriptableObject.CreateInstance(typeof(NetworkPrefabsDatabaseWindow)) as NetworkPrefabsDatabaseWindow;
            window.targetDatabase   = database;
            if (!GlobalResources.DEFAULT_DATABASE.Equals(database)) {
                window.prefabsDatabase = Resources.Load<NetworkPrefabsDatabase>(GlobalResources.GetPrefabsDatabase(window.targetDatabase));
            }
            window.ShowUtility();
        }

        /// <summary>
        /// Opens the network database source.
        /// </summary>
        /// <param name="selectOnInspector">if set to <c>true</c> [select on inspector].</param>
        public static void OpenNetworkDatabaseSource(bool selectOnInspector = true) {
            NetworkPrefabsDatabase assetObject = Resources.Load<NetworkPrefabsDatabase>(GlobalResources.GetPrefabsDatabase());
            if (assetObject == null) {
                CreateNetworkDatabaseSource();
                assetObject = Resources.Load<NetworkPrefabsDatabase>(GlobalResources.GetPrefabsDatabase());
                if (assetObject == null) {
                    string sourcePath = ResourceUtils.ResourcesPath + "/" + GlobalResources.GetPrefabsDatabase() + ".asset";
                    NetworkDebugger.Log(string.Format("Unable to find Network Events Database at {0}.asset", sourcePath));
                }
            }
            if (selectOnInspector) {
                Selection.activeObject = assetObject;
            }
        }

        /// <summary>
        /// Creates the network database source.
        /// </summary>
        /// <param name="databasePath">The database path.</param>
        public static void CreateNetworkDatabaseSource(string databasePath = GlobalResources.DEFAULT_DATABASE) {
            if ((GlobalResources.GetPrefabsDatabase(databasePath)          == null) ||
                (GlobalResources.GetPrefabsDatabase(databasePath).Length   == 0)) {
                return;
            }			
			System.Object GlobalSource = Resources.Load(GlobalResources.GetPrefabsDatabase(databasePath));
            NetworkPrefabsDatabase sourceData = ScriptableObject.CreateInstance<NetworkPrefabsDatabase>();
            string databaseFolder   = ResourceUtils.ResourcesPath;
            string fullresFolder    = Application.dataPath + databaseFolder.Replace("Assets", "");
            if (!System.IO.Directory.Exists(fullresFolder)) {
                System.IO.Directory.CreateDirectory(fullresFolder);
            }
            string sourcePath   = databaseFolder + "/" + GlobalResources.GetPrefabsDatabase(databasePath) + ".asset";
            string pathOfFile   = Path.GetDirectoryName(sourcePath);
            if (!System.IO.Directory.Exists(pathOfFile)) {
                System.IO.Directory.CreateDirectory(pathOfFile);
            }
            
            AssetDatabase.CreateAsset(sourceData, sourcePath);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(sourceData);
#endif
            AssetDatabase.SaveAssets();
		    AssetDatabase.Refresh();
		}

        /// <summary>
        /// Called when [enable].
        /// </summary>
        void OnEnable() {
            this.maxSize            = new Vector2(990f, 895f);
            this.minSize            = this.maxSize;
            this.titleContent       = new GUIContent ("Prefabs Database", Resources.Load("oo_info") as Texture);
            this.prefabsDatabase    = Resources.Load<NetworkPrefabsDatabase>(GlobalResources.GetPrefabsDatabase(this.targetDatabase));
            EditorWindowExtensions.CenterOnMainWin(this);
        }

        /// <summary>
        /// Called when [GUI].
        /// </summary>
        void OnGUI() {
            // Not allow chnages during playmode
            if (Application.isPlaying) {
                EditorUtils.PrintImage("objectnet_logo", Color.blue, 0, 30);            
                GUILayout.Space(5.0f);            
                EditorUtils.PrintImageButton("Changes are disabled during PlayMode", "oo_info", Color.red.WithAlpha(0.15f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {                    
                });
                return;
            }

            bool displayTooltip = false;
            string tolltipText = "";

            EditorGUILayout.BeginHorizontal(GUILayout.Width(this.maxSize.x - 100.0f));
            GUILayout.Space(5.0f);
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Space(5.0f);
            EditorUtils.PrintImage("objectnet_logo", Color.blue, 0, 30);
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.yellow.WithAlpha(0.10f)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 30.0f));
            EditorGUILayout.Space(5);
            EditorUtils.PrintExplanationLabel("All registered prefab will be automatically synchronized between the server and clients. Drag prefab to the field bellow and press \"Register Prefab\"", "oo_prefab", Color.green.WithAlpha(0.5f));
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Prefab to register");
            EditorGUILayout.BeginHorizontal();
            this.prefabToRegister = (EditorGUILayout.ObjectField(this.prefabToRegister, typeof(GameObject), false, GUILayout.Width(350)) as GameObject);
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(-15);
            EditorUtils.PrintImageButton("Register Prefab", "oo_prefab", EditorUtils.EVENT_BACKGROUND_COLOR.WithAlpha(0.50f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, 12, 12f, 12f, () => {
                bool registerPrefab = true;
                if (this.prefabToRegister == null) {
                    registerPrefab = false;
                    this.registerError = "Drag prefab field above before register a new network prefab";
                } else if (this.prefabsDatabase.PrefabExists(this.prefabToRegister)) {
                    registerPrefab = false;
                    this.registerError = string.Format("Network Prefab \"{0}\" is already registered", this.prefabToRegister.name);
                } 
                if (registerPrefab) {
                    this.prefabsDatabase.RegisterPrefab(this.prefabsDatabase.GetNextId(), this.prefabToRegister).SetPrefabScripts(new ScriptList());
                    this.prefabToRegister   = null;
                    this.registerError      = null;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                    AssetDatabase.SaveAssets();
		            AssetDatabase.Refresh();
                }
                this.OnGUI();
            });
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal((this.registerError == null) ? BackgroundStyle.Get(Color.white.WithAlpha(0.10f)) : BackgroundStyle.Get(Color.red.WithAlpha(0.10f)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 30.0f));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(5);
            if (string.IsNullOrEmpty(this.registerError)) {
                EditorUtils.PrintExplanationLabel("Drag prefab to field above and press \"Register Prefab\"", "oo_info", Color.yellow, 5f);
            } else {
                EditorUtils.PrintExplanationLabel(this.registerError, "oo_error", EditorUtils.ERROR_FONT_COLOR);
            }
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(0.20f)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 30.0f), GUILayout.Height(34.0f));
            EditorGUILayout.Space(2);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.yellow.WithAlpha(0.10f)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 34.0f), GUILayout.Height(30.0f));
            EditorUtils.PrintHeader("Registered Prefabs", Color.white.WithAlpha(0.0f), Color.white, 12, "oo_event");
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter : ");
            EditorGUILayout.Space(-140);
            this.prefabsFilter = EditorGUILayout.TextField("", this.prefabsFilter, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth - 30.0f), GUILayout.Height(5));
            EditorGUILayout.Space(25);
            EditorUtils.PrintSizedLabel("Prefab", 12, Color.yellow.WithAlpha(0.75f));
            GUILayout.FlexibleSpace();
            EditorUtils.PrintSizedLabel("Ownership access level", 12, Color.yellow.WithAlpha(0.75f));
            EditorGUILayout.Space(18);
            EditorUtils.PrintSizedLabel("Movement Type", 12, Color.yellow.WithAlpha(0.75f));
            EditorGUILayout.Space(22);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(-12);

            EditorGUILayout.BeginHorizontal("Box");
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayout.Width(this.maxSize.x - 30.0f), GUILayout.Height(515));
            NetworkPrefabEntry prefabToRemove = null;
            foreach (NetworkPrefabEntry prefabEntry in this.prefabsDatabase.GetPrefabs()) {
                if (prefabEntry.IsToSetDefaultValues() == true) {
                    prefabEntry.SetDefaultValues();
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                if ((string.IsNullOrEmpty(this.prefabsFilter) == false) &&
                    (prefabEntry.GetPrefab().name.ToUpper().Contains(this.prefabsFilter.ToUpper()) == false)) {
                    continue;
                }

                bool isAutoSync             = prefabEntry.GetAutoSync();
                bool isAnimationSync        = prefabEntry.GetAnimationAutoSync();
                bool isParticlesSync        = prefabEntry.GetParticlesAutoSync();
                bool isToShowScripts        = prefabEntry.GetShowScripts();
                bool isToCheckAllScripts    = prefabEntry.GetCheckAllScripts();
                bool isToCheckAllChilds     = prefabEntry.GetCheckAllChilds();
                bool isToShowChilds         = prefabEntry.GetShowChilds();
                bool isShowInputScripts     = prefabEntry.GetShowInputScripts();
                bool isToShowVariables      = prefabEntry.GetShowVariables();
                bool isToShowEvents         = prefabEntry.GetShowEvents();
                bool isToShowTransform      = prefabEntry.GetShowTransform();
                bool isToShowChildTree      = prefabEntry.GetShowChildTree();
                bool isToDisableGravity     = prefabEntry.IsToDisableGravity();
                bool isToEnableKinematic    = prefabEntry.IsToEnableKinematic();
                bool isToEnableInterpolate  = prefabEntry.IsToEnableInterpolate();
                bool isToUsePrefabRotation  = prefabEntry.IsToUsePrefabRotation();
                bool isToUseCustomSendRate  = prefabEntry.IsToUseCustomRate();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(2);
                if (GUILayout.Button(Resources.Load("oo_delete") as Texture, GUIStyle.none, GUILayout.Width(14), GUILayout.Height(14))) {
                    prefabToRemove = prefabEntry;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(prefabEntry.GetPrefab(), typeof(GameObject), false, GUILayout.Width(180));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space(10);
                // Scripts button
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(3);
                if (GUILayout.Button(Resources.Load((isToShowScripts) ? "oo_script_visible" : "oo_script_invisible") as Texture, GUIStyle.none, GUILayout.Width(14), GUILayout.Height(14))) {
                    isToShowScripts = !isToShowScripts;
                    prefabEntry.SetShowScripts(isToShowScripts);
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(4.0f);
                
                // Child objects button
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(3);
                if (GUILayout.Button(Resources.Load((isToShowChilds) ? "oo_prefab" : "oo_prefab_disabled") as Texture, GUIStyle.none, GUILayout.Width(14), GUILayout.Height(14))) {
                    isToShowChilds = !isToShowChilds;
                    prefabEntry.SetShowChilds(isToShowChilds);
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(4.0f);

                // When using remote inputs i need to provide a way to redefine scripts to be disabled
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(3);
                if (GUILayout.Button(Resources.Load((isShowInputScripts) ? "oo_gamepad_activated" : "oo_gamepad_inactive") as Texture, GUIStyle.none, GUILayout.Width(14), GUILayout.Height(14))) {
                    isShowInputScripts = !isShowInputScripts;
                    prefabEntry.SetShowInputScripts(isShowInputScripts);
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(4.0f);

                // Show variables attributes
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(3);
                if (GUILayout.Button(Resources.Load((isToShowVariables) ? "oo_variables_activated" : "oo_variables_inactive") as Texture, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) {
                    isToShowVariables = !isToShowVariables;
                    prefabEntry.SetShowVariables(isToShowVariables);
                    // Reload variables
                    if (isToShowVariables == true) {
                        ScriptList scriptList = prefabEntry.GetPrefabScripts();
                        if (scriptList != null) {
                            foreach (ScriptStatus script in scriptList.GetScripts()) {
                                if (script.Script != null) {
                                    if (typeof(NetworkBehaviour).IsAssignableFrom(script.Script.GetType())) {
                                        if (script.GetVariables() != null) {
                                            script.RefreshVariables();
#if UNITY_EDITOR
                                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                                            AssetDatabase.SaveAssets();
                                            AssetDatabase.Refresh();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(4.0f);

                // Show variables attributes
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(3);
                if (GUILayout.Button(Resources.Load((isToShowEvents) ? "oo_event_activated" : "oo_event") as Texture, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) {
                    isToShowEvents = !isToShowEvents;
                    prefabEntry.SetShowEvents(isToShowEvents);                    
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);

                // Show variables attributes
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(3);
                if (GUILayout.Button(Resources.Load((isToShowTransform) ? "oo_gizmo" : "oo_gizmo_inactive") as Texture, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) {
                    isToShowTransform = !isToShowTransform;
                    prefabEntry.SetShowTransform(isToShowTransform);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);

                EditorGUILayout.BeginVertical();
                GUILayout.Space(3.0f);
                Animator animator = (prefabEntry.GetPrefab() != null) ? prefabEntry.GetPrefab().GetComponentInChildren<Animator>() : null;
                if ( EditorUtils.PrintBooleanSquared(ref isAutoSync, "Auto Sync", null, 14, 10, true) ) {
                    prefabEntry.SetAutoSync(isAutoSync);
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                    AssetDatabase.SaveAssets();
		            AssetDatabase.Refresh();
                }   
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.BeginHorizontal();
                ParticleSystem particles = (prefabEntry.GetPrefab() != null) ? prefabEntry.GetPrefab().GetComponentInChildren<ParticleSystem>() : null;
                if (particles == null) {
                    prefabEntry.SetParticlesAutoSync(false);
                    GUILayout.Space(1.0f);
                } else {
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(3.0f);
                    if (EditorUtils.PrintBooleanSquared(ref isParticlesSync, "Sync Particles", null, 14, 10, true)) {
                        prefabEntry.SetParticlesAutoSync(isParticlesSync);
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
		                AssetDatabase.Refresh();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal(GUILayout.Width((animator == null) ? (particles == null) ? 225 : 114 : (particles == null) ? 225 : 80));
                if (animator == null) {
                    prefabEntry.SetAnimationAutoSync(false);
                    GUILayout.Space(1.0f);
                } else {
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(3.0f);
                    if (EditorUtils.PrintBooleanSquared(ref isAnimationSync, "Sync Animation", null, 14, 10, true)) {
                        prefabEntry.SetAnimationAutoSync(isAnimationSync);
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
		                AssetDatabase.Refresh();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                // Ownership access level
                EditorGUILayout.BeginHorizontal();
                int selectedOwnershipLevel = (int)this.DrawOwnerShipLevel(prefabEntry.GetOwnerShipAccessLevel());
                if (selectedOwnershipLevel != (int)prefabEntry.GetOwnerShipAccessLevel()) {
                    prefabEntry.SetOwnerShipAccessLevel((OwnerShipAccessLevel)selectedOwnershipLevel);
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                EditorGUILayout.EndHorizontal();

                // Movement type
                EditorGUILayout.BeginHorizontal();
                int selectedMovementType = (int)this.DrawMovementType(prefabEntry.GetMovementType());
                if (selectedMovementType != (int)prefabEntry.GetMovementType()) {
                    prefabEntry.SetMovementType((PredictionType)selectedMovementType);
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);

                if (animator != null) {
                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintSimpleExplanation("Select animation synchonization mode", Color.yellow.WithAlpha(0.75f));
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10.0f);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(4.0f);
                    EditorGUILayout.BeginHorizontal();
                    int selectedAnimationMode = (int)this.DrawAnimationModes(prefabEntry.GetAnimationSyncMode());
                    if (selectedAnimationMode != (int)prefabEntry.GetAnimationSyncMode()) {
                        prefabEntry.SetAnimationSyncMode((AnimationSyncType)selectedAnimationMode);
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
		                AssetDatabase.Refresh();
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(4.0f);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();

                    // Collect internal data
                    int     currentAnimationCount   = prefabEntry.GetAnimationCount();
                    string  currentDefaultStatus    = prefabEntry.GetAnimationDefaultStatus();
                    int     newAnimationCount       = ((animator.runtimeAnimatorController != null) && 
                                                       (typeof(AnimatorController).IsAssignableFrom(animator.runtimeAnimatorController.GetType()))) ? (animator.runtimeAnimatorController as AnimatorController).layers.Count() : 0;
                    string  newDefaultStatus        = this.CollectDefaultAnimationStatus(animator);
                    if ((currentAnimationCount  != newAnimationCount) ||
                        (currentDefaultStatus   != newDefaultStatus)) {
                        prefabEntry.SetAnimationCount(newAnimationCount);
                        prefabEntry.SetAnimationDefaultStatus(newDefaultStatus);
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
		                AssetDatabase.Refresh();
                    }
                } else {
                    if ((prefabEntry.GetAnimationCount() > 0) ||
                        (!string.IsNullOrEmpty(prefabEntry.GetAnimationDefaultStatus()))) {
                        prefabEntry.SetAnimationCount(0);
                        prefabEntry.SetAnimationDefaultStatus("");
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
		                AssetDatabase.Refresh();
                    }
                }

                // Draw synchonize tranform options
                if (isToShowTransform) {
                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.DETAIL_INFO_COLOR.WithAlpha(TRANSPORT_BACKGROUND_ALPHA)));
                    EditorUtils.PrintHeader("Transform and Physics options", EditorUtils.DETAIL_INFO_COLOR.WithAlpha(1.0f), Color.white, 14, "oo_gizmo");
                    EditorGUILayout.EndHorizontal();

                    bool[] syncPosition = prefabEntry.GetSyncPosition();
                    bool[] syncRotation = prefabEntry.GetSyncRotation();
                    bool[] syncScale    = prefabEntry.GetSyncScale();

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    bool syncPos = EditorUtils.PrintVector3BooleanAxis(ref syncPosition, "Synchonize Position", "oo_position");
                    bool syncRot = EditorUtils.PrintVector3BooleanAxis(ref syncRotation, "Synchonize Rotation", "oo_rotation");
                    bool syncSca = EditorUtils.PrintVector3BooleanAxis(ref syncScale, "Synchonize Scale", "oo_scale");
                    if (syncPos || syncRot || syncSca) {
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("You can disable some axis synchronization if your game\nor the player doesn't have movements on this axis;\nthis might reduce the amount of transferred data", "oo_info");
                    GUILayout.Space(10.0f);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.yellow.WithAlpha(DETAIL_BACKGROUND_ALPHA)));
                    GUILayout.Space(20.0f);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(6.0f);
                    if (EditorUtils.PrintBooleanSquared(ref isToDisableGravity, "Disable gravity", null, 14, 12, true, 2.5f)) {
                        prefabEntry.SetDisableGravity(isToDisableGravity);
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }                    
                    EditorUtils.PrintSimpleExplanation("Disable gravity on passive instance of this element");

                    if (EditorUtils.PrintBooleanSquared(ref isToEnableKinematic, "Set as Kinematic", null, 14, 12, true, 2.5f)) {
                        prefabEntry.SetEnableKinematic(isToEnableKinematic);
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    EditorUtils.PrintSimpleExplanation("Put object as kinematic on passive instance of this element");

                    if (EditorUtils.PrintBooleanSquared(ref isToEnableInterpolate, "Interpolate physics", null, 14, 12, true, 2.5f)) {
                        prefabEntry.SetEnableInterpolate(isToEnableInterpolate);
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    EditorUtils.PrintSimpleExplanation("Set the object rigidbody to use \"Interpolate\" mode.");

                    if (EditorUtils.PrintBooleanSquared(ref isToUsePrefabRotation, "Use prefab rotation ( Players )", null, 14, 12, true, 2.5f)) {
                        prefabEntry.SetToUsePrefabRotation(isToUsePrefabRotation);
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    EditorGUILayout.BeginHorizontal();
                    EditorUtils.PrintSimpleExplanation("Set to use prefab rotation during spawn");
                    EditorUtils.PrintSimpleExplanation("( this option works for players object only ).", EditorUtils.DEFAULT_EXPLANATION_FONT_SIZE, Color.yellow.WithAlpha(0.80f));
                    EditorGUILayout.EndHorizontal();

                    if (EditorUtils.PrintBooleanSquared(ref isToUseCustomSendRate, 
                                                        (isToUseCustomSendRate) ? String.Format("Use custom send rate [ {0} messages peer second ]", prefabEntry.GetSendRateAmount()) : "Use custom send rate", 
                                                        null, 
                                                        14, 
                                                        12, 
                                                        true, 
                                                        2.5f)) {
                        prefabEntry.SetUseCustomRate(isToUseCustomSendRate);
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    if (isToUseCustomSendRate) {
                        GUILayout.Space(10.0f);
                        EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                        int rateValue = Mathf.Clamp(prefabEntry.GetSendRateAmount(), 1, 60);
                        GUILayout.Space(10.0f);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15.0f);
                        EditorUtils.DrawHorizontalIntBar(ref rateValue, 1, 60, null, 500f);
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(10.0f);
                        prefabEntry.SetSendRateAmount(rateValue);
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(5f);
                    } else {
                        EditorGUILayout.BeginHorizontal();
                        EditorUtils.PrintSimpleExplanation("You can enable this option to define a send rate frequency for this object only.");
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(15.0f);
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5f);
                }                    

                // Draw script enable/disable
                if ( isToShowScripts ) {
                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.DETAIL_INFO_COLOR.WithAlpha(TRANSPORT_BACKGROUND_ALPHA)));
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.Space(-4);
                    EditorUtils.PrintHeader("Object scripts", EditorUtils.DETAIL_INFO_COLOR.WithAlpha(0.0f), Color.white, 14, "oo_script_invisible");
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.Space(2);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("Filter : ");
                    EditorGUILayout.Space(-140);
                    prefabEntry.SetScriptsFilter(EditorGUILayout.TextField("", prefabEntry.GetScriptsFilter(), GUILayout.Width(250)));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();

                    // Need to have child list to check id script is into child
                    if (prefabEntry.GetChildObjects() == null) {
                        prefabEntry.SetChildObjects(new GameObjectList());
                    }

                    GameObject  networkPrefab   = prefabEntry.GetPrefab();
                    ScriptList  scriptList      = prefabEntry.GetPrefabScripts();
                    if ( scriptList == null ) {
                        prefabEntry.SetPrefabScripts(new ScriptList());
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
		                AssetDatabase.Refresh();
                        scriptList = prefabEntry.GetPrefabScripts();
                    }
                    // Check if all script are in list
                    MonoBehaviour[] componentsInPrefab = (networkPrefab != null) ? networkPrefab.GetComponentsInChildren<MonoBehaviour>(true) : new MonoBehaviour[0];
                    foreach (MonoBehaviour component in componentsInPrefab) {
                        if (component != null) {
                            if (!NetworkScriptsReference.IsIgnoreType(component.GetType())) {
                                if (!scriptList.ContainsScript(component)) {
                                    // To not put some ghosth script
                                    if ((component.gameObject.Equals(networkPrefab)) ||
                                        (prefabEntry.GetChildObjects().HasChildScript(component))) {
                                        scriptList.RegisterScript(component);
                                    }
                                }
                            }
                        }
                    }
                    // Now remove components that are not in prefab
                    int indexOfScript = 0;
                    List<int> scriptsToRemove = new List<int>();
                    foreach (ScriptStatus script in scriptList.GetScripts()) {
                        if ( !componentsInPrefab.Contains(script.Script) ) {
                            scriptsToRemove.Add(indexOfScript);
                        }
                        indexOfScript++;
                    }
                    while (scriptsToRemove.Count > 0) {
                        int indexToRemove = scriptsToRemove[0];
                        scriptsToRemove.RemoveAt(0);
                        if (indexToRemove < scriptList.GetScripts().Count) {
                            scriptList.GetScripts().RemoveAt(indexToRemove);
                        }
                    }
                    // Now show all scripts list to allow user to enable/disable
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(30.0f);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintSimpleExplanation("By default all logic scripts are disabled on passive objects, nonetheless, you can enable some listed script below by clicking in check in front of script");
                    EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                    EditorGUILayout.BeginHorizontal();
                    bool executeSelect = false;
                    bool previous = isToCheckAllScripts;
                    if (EditorUtils.PrintBooleanSquared(ref isToCheckAllScripts, "", null, 14, 12, true, 2.5f)) {
                        executeSelect = (previous != isToCheckAllScripts);
                        if (executeSelect) {
                            prefabEntry.SetCheckAllScripts(isToCheckAllScripts);
                            foreach (ScriptStatus script in scriptList.GetScripts()) {
                                script.Enabled = isToCheckAllScripts;
                            }
                        }
                    }
                    GUILayout.Label("Don't Disable");
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Delay to disable");
                    GUILayout.Space(210.0f);
                    EditorGUILayout.EndHorizontal();
                    EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                    GUILayout.Space(2.0f);
                    GameObject currentChild = null;
                    foreach (ScriptStatus script in scriptList.GetScripts()) {
                        if (string.IsNullOrEmpty(prefabEntry.GetScriptsFilter()) == false) {
                            if (script.Script.GetType().Name.ToUpper().Contains(prefabEntry.GetScriptsFilter().ToUpper()) == false) {
                                continue;
                            }
                        }
                        if (!script.Script.gameObject.Equals(currentChild)) {
                            EditorGUILayout.BeginHorizontal();
                            EditorUtils.PrintHeader(script.Script.gameObject.name, EditorUtils.SUB_DETAIL_PANEL_COLOR, Color.white, 12, "oo_prefab");
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(this.GetHierarchySpace(script.Script, networkPrefab));
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.BeginVertical();
                        if ( EditorUtils.PrintBooleanSquared(ref script.Enabled, script.Script.GetType().Name, null, 14, 12, true, 2.5f) ) {                            
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                            AssetDatabase.SaveAssets();
		                    AssetDatabase.Refresh();
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.FlexibleSpace();

                        int previousDelay = script.Delay;
                        EditorUtils.DrawHorizontalIntBar(ref script.Delay, 0, 999);
                        if ( previousDelay != script.Delay ) {
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                            AssetDatabase.SaveAssets();
		                    AssetDatabase.Refresh();
                        }
                        GUILayout.Space(2.0f);
                        GUILayout.Label(string.Format("{0} ms", script.Delay.ToString("D3")));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndHorizontal();
                        // Update new child
                        currentChild = script.Script.gameObject;
                    }
                    if (executeSelect) {
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(15.0f);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(10.0f);
                }

                // Draw child objects enable/disable
                if ((isToShowChilds == true) && (prefabEntry.GetPrefab() != null)) { 
                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.DETAIL_INFO_COLOR.WithAlpha(TRANSPORT_BACKGROUND_ALPHA)));
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.Space(-4);
                    EditorUtils.PrintHeader("Child game objects", EditorUtils.DETAIL_INFO_COLOR.WithAlpha(0.0f), Color.white, 14, "oo_prefab");
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.Space(2);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("Filter : ");
                    EditorGUILayout.Space(-140);
                    prefabEntry.SetChildsFilter(EditorGUILayout.TextField("", prefabEntry.GetChildsFilter(), GUILayout.Width(250)));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();

                    GameObject                  networkPrefab   = prefabEntry.GetPrefab();
                    GameObjectList              childList       = prefabEntry.GetChildObjects();
                    NetworkInstantiateDetection detectionScript = networkPrefab.GetComponent<NetworkInstantiateDetection>();
                    // Inject "NetworkInstantiateDetection" component into registered prefab if dont'exists
                    if (detectionScript == null) {
                        NetworkObjectExtension.InjectNetwork(networkPrefab);
                    }
                    if ( childList == null ) {
                        prefabEntry.SetChildObjects(new GameObjectList());
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
		                AssetDatabase.Refresh();
                        childList = prefabEntry.GetChildObjects();
                    }
                    // Check if all script are in list
                    Transform[] childs = networkPrefab.transform.GetComponentsInChildren<Transform>(true);
                    foreach (Transform child in childs) {
                        if (networkPrefab.transform != child) {
                            if (!childList.ContainsObject(child.gameObject)) {
                                childList.RegisterObject(child.gameObject);
                            }
                        } else if (childList.ContainsObject(child.gameObject)) {
                            childList.UnRegisterObject(child.gameObject);
                        }
                    }
                    // Now remove components that are not in prefab
                    int indexOfObject = 0;
                    List<int> objectsToRemove = new List<int>();
                    foreach (GameObjectStatus child in childList.GetObjects()) {
                        bool found = false;
                        foreach (Transform objectChild in childs) { 
                            if ( child.Target.Equals(objectChild.gameObject) ) {
                                found = true;
                                break;
                            }
                        }
                        if ( !found ) {
                            objectsToRemove.Add(indexOfObject);
                        }
                        indexOfObject++;
                    }
                    while (objectsToRemove.Count > 0) {
                        int indexToRemove = objectsToRemove[0];
                        objectsToRemove.RemoveAt(0);
                        if (indexToRemove < childList.GetObjects().Count) {
                            childList.GetObjects().RemoveAt(indexToRemove);
                        }
                    }
                    // Now show all scripts list to allow user to enable/disable
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(30.0f);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintSimpleExplanation("You can disable any child object when object is running in passive mode. This can be usefull specially when network input is enabled");
                    EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space(-20);
                    if (GUILayout.Button(Resources.Load((isToShowChildTree) ? "oo_tree_active" : "oo_tree") as Texture, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) {
                        isToShowChildTree = !isToShowChildTree;
                        prefabEntry.SetShowChildTree(isToShowChildTree);
                    }
                    if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) {
                        displayTooltip = true;
                        tolltipText = "Show/Hide all childs os this network prefabs";
                    }
                    EditorGUILayout.Space(5);
                    bool executeCheck = false;
                    bool previous = isToCheckAllChilds;
                    if (EditorUtils.PrintBooleanSquared(ref isToCheckAllChilds, "", null, 14, 12, true, 2.5f)) {
                        executeCheck = (previous != isToCheckAllChilds);
                        if (executeCheck) {
                            prefabEntry.SetCheckAllChilds(isToCheckAllChilds);
                        }
                    }
                    GUILayout.Label("Don't Disable");
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Auto Sync");
                    GUILayout.Space(10.0f);
                    GUILayout.Label("Delay to disable");
                    GUILayout.Space(210.0f);
                    EditorGUILayout.EndHorizontal();
                    EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                    GUILayout.Space(2.0f);
                    int childIndexToDisable = 0;
                    foreach (GameObjectStatus child in childList.GetObjects()) {
                        if (isToShowChildTree == false) {
                            if (child.Target.transform.parent != null && !child.Target.transform.parent.Equals(networkPrefab.transform)) {
                                // Go to the next child index
                                ++childIndexToDisable;
                                continue;
                            }
                        }
                        if (string.IsNullOrEmpty(prefabEntry.GetChildsFilter()) == false) {
                            if (child.Target.name.ToUpper().Contains(prefabEntry.GetChildsFilter().ToUpper()) == false) {
                                continue;
                            }
                        }
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(this.GetInheritanceLevel(networkPrefab, child.Target.transform) * 10.0f);
                        GUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                        if (executeCheck) {
                            child.Enabled = isToCheckAllChilds;
                        }
                        if ( EditorUtils.PrintBooleanSquared(ref child.Enabled, child.Target.name, null, 14, 12, true, 2.5f) ) {                                                        
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                            AssetDatabase.SaveAssets();
		                    AssetDatabase.Refresh();
                        }

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        GUILayout.FlexibleSpace();

                        EditorGUILayout.Space(3);
                        if (GUILayout.Button(Resources.Load((child.SyncPosition) ? "oo_position_active" : "oo_position") as Texture, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) {
                            child.SyncPosition = !child.SyncPosition;
                            if (detectionScript != null) {
                                if ((child.SyncPosition == true) && (detectionScript.IsFlaggedToSync(child.Target) == false)) {
                                    detectionScript.RegisterChildToSync(child.Target);
                                } else if ((child.SyncPosition  == false) &&
                                           (child.SyncRotation  == false) &&
                                           (child.SyncScale     == false) && 
                                           (detectionScript.IsFlaggedToSync(child.Target) == true)) {
                                    detectionScript.UnRegisterChildToSync(child.Target);
                                }
                            }
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
                            UnityEditor.EditorUtility.SetDirty(detectionScript);
#endif
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        } else if (((child.SyncPosition == true) ||
                                    (child.SyncRotation == true) ||
                                    (child.SyncScale    == true)) &&
                                   (detectionScript.IsFlaggedToSync(child.Target) == false)) {
                            detectionScript.RegisterChildToSync(child.Target);
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
                            UnityEditor.EditorUtility.SetDirty(detectionScript);
#endif
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        } else if ((child.SyncPosition  == false) &&
                                   (child.SyncRotation  == false) &&
                                   (child.SyncScale     == false) &&
                                   (detectionScript.IsFlaggedToSync(child.Target) == true)) {
                            detectionScript.UnRegisterChildToSync(child.Target);
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
                            UnityEditor.EditorUtility.SetDirty(detectionScript);
#endif
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                        if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) {
                            displayTooltip = true;
                            tolltipText = "Enable this option to synchronize this child \"Position\" over network on all instances ";
                        }
                        EditorGUILayout.Space(5);
                        if (GUILayout.Button(Resources.Load((child.SyncRotation) ? "oo_rotation_active" : "oo_rotation") as Texture, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) {
                            child.SyncRotation = !child.SyncRotation;
                            if (detectionScript != null) {
                                if ((child.SyncPosition == true) && (detectionScript.IsFlaggedToSync(child.Target) == false)) {
                                    detectionScript.RegisterChildToSync(child.Target);
                                } else if ((child.SyncPosition == false) &&
                                           (child.SyncRotation == false) &&
                                           (child.SyncScale == false) &&
                                           (detectionScript.IsFlaggedToSync(child.Target) == true)) {
                                    detectionScript.UnRegisterChildToSync(child.Target);
                                }
                            }
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        } else if (((child.SyncPosition  == true) ||
                                    (child.SyncRotation  == true) ||
                                    (child.SyncScale     == true)) &&
                                   (detectionScript.IsFlaggedToSync(child.Target) == false)) {
                            detectionScript.RegisterChildToSync(child.Target);
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
                            UnityEditor.EditorUtility.SetDirty(detectionScript);
#endif
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        } else if ((child.SyncPosition  == false) &&
                                   (child.SyncRotation  == false) &&
                                   (child.SyncScale     == false) &&
                                   (detectionScript.IsFlaggedToSync(child.Target) == true)) {
                            detectionScript.UnRegisterChildToSync(child.Target);
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
                            UnityEditor.EditorUtility.SetDirty(detectionScript);
#endif
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                        if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) {
                            displayTooltip = true;
                            tolltipText = "Enable this option to synchronize this child \"Rotation\" over network on all instances ";
                        }
                        EditorGUILayout.Space(5);
                        if (GUILayout.Button(Resources.Load((child.SyncScale) ? "oo_scale_active" : "oo_scale") as Texture, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) {
                            child.SyncScale = !child.SyncScale;
                            if (detectionScript != null) {
                                if ((child.SyncPosition == true) && (detectionScript.IsFlaggedToSync(child.Target) == false)) {
                                    detectionScript.RegisterChildToSync(child.Target);
                                } else if ((child.SyncPosition == false) &&
                                           (child.SyncRotation == false) &&
                                           (child.SyncScale == false) &&
                                           (detectionScript.IsFlaggedToSync(child.Target) == true)) {
                                    detectionScript.UnRegisterChildToSync(child.Target);
                                }
                            }
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        } else if (((child.SyncPosition == true) ||
                                    (child.SyncRotation == true) ||
                                    (child.SyncScale    == true)) &&
                                   (detectionScript.IsFlaggedToSync(child.Target) == false)) {
                            detectionScript.RegisterChildToSync(child.Target);
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
                            UnityEditor.EditorUtility.SetDirty(detectionScript);
#endif
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        } else if ((child.SyncPosition  == false) &&
                                   (child.SyncRotation  == false) &&
                                   (child.SyncScale     == false) &&
                                   (detectionScript.IsFlaggedToSync(child.Target) == true)) {
                            detectionScript.UnRegisterChildToSync(child.Target);
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
                            UnityEditor.EditorUtility.SetDirty(detectionScript);
#endif
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                        if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) {
                            displayTooltip = true;
                            tolltipText = "Enable this option to synchronize this child \"Scale\" over network on all instances ";
                        }
                        EditorGUILayout.Space(15);
                        int previousDelay = child.Delay;
                        EditorUtils.DrawHorizontalIntBar(ref child.Delay, 0, 999);
                        if ( previousDelay != child.Delay ) {
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                            AssetDatabase.SaveAssets();
		                    AssetDatabase.Refresh();
                        }
                        GUILayout.Space(2.0f);
                        GUILayout.Label(string.Format("{0} ms", child.Delay.ToString("D3")));
                        EditorGUILayout.EndHorizontal();
                        if (child.Enabled == false) {
                            if ((detectionScript != null) && (detectionScript.IsFlaggedToDisable(child.Target) == false)) {
                                detectionScript.RegisterChildToDisable(child.Target);
#if UNITY_EDITOR
                                UnityEditor.EditorUtility.SetDirty(detectionScript);
#endif
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();                                
                            }
                        } else if ((detectionScript != null) && (detectionScript.IsFlaggedToDisable(child.Target) == true)) {
                            detectionScript.UnRegisterChildToDisable(child.Target);
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(detectionScript);
#endif
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                        GUILayout.Space(3.0f);
                        // Go to the next child index
                        ++childIndexToDisable;
                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(15.0f);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(10.0f);

                    if (executeCheck) {
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                } else {
                    GameObjectList childList = prefabEntry.GetChildObjects();
                    if ((prefabEntry != null) && (prefabEntry.GetPrefab() != null) && (childList != null)) {
                        NetworkInstantiateDetection detectionScript = prefabEntry.GetPrefab().GetComponent<NetworkInstantiateDetection>();
                        if (detectionScript != null) {
                            foreach (GameObjectStatus child in childList.GetObjects()) {
                                if (child.Enabled == false) {
                                    if (detectionScript.IsFlaggedToDisable(child.Target) == false) {
                                        detectionScript.RegisterChildToDisable(child.Target);
#if UNITY_EDITOR
                                        UnityEditor.EditorUtility.SetDirty(detectionScript);
#endif
                                        AssetDatabase.SaveAssets();
                                        AssetDatabase.Refresh();
                                    }
                                } else if (detectionScript.IsFlaggedToDisable(child.Target) == true) {
                                    detectionScript.UnRegisterChildToDisable(child.Target);
#if UNITY_EDITOR
                                    UnityEditor.EditorUtility.SetDirty(detectionScript);
#endif
                                    AssetDatabase.SaveAssets();
                                    AssetDatabase.Refresh();
                                }
                            }
                        }
                    }
                }

                // Draw script enable/disable for remote input
                if ( isShowInputScripts ) {
                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.DETAIL_INFO_COLOR.WithAlpha(TRANSPORT_BACKGROUND_ALPHA)));
                    EditorUtils.PrintHeader("Object scripts [ Using remote input ]", EditorUtils.DETAIL_INFO_COLOR.WithAlpha(1.0f), Color.white, 14, "oo_gamepad");
                    EditorGUILayout.EndHorizontal();

                    // Need to have child list to check id script is into child
                    if (prefabEntry.GetChildObjects() == null) {
                        prefabEntry.SetChildObjects(new GameObjectList());
                    }

                    GameObject  networkPrefab   = prefabEntry.GetPrefab();
                    ScriptList  scriptList      = prefabEntry.GetInputScripts();
                    if ( scriptList == null ) {
                        prefabEntry.SetInputScripts(new ScriptList());
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
		                AssetDatabase.Refresh();
                        scriptList = prefabEntry.GetInputScripts();
                    }
                    // Check if all script are in list
                    MonoBehaviour[] componentsInPrefab = networkPrefab.GetComponentsInChildren<MonoBehaviour>(true);
                    foreach (MonoBehaviour component in componentsInPrefab) {
                        if (!NetworkScriptsReference.IsIgnoreType(component.GetType())) {
                            if (!scriptList.ContainsScript(component)) {
                                // To not put some ghosth script
                                if ((component.gameObject.Equals(networkPrefab)) ||
                                    (prefabEntry.GetChildObjects().HasChildScript(component))) {
                                    scriptList.RegisterScript(component);
                                }
                            }
                        }
                    }
                    // Now remove components that are not in prefab
                    int indexOfScript = 0;
                    List<int> scriptsToRemove = new List<int>();
                    foreach (ScriptStatus script in scriptList.GetScripts()) {
                        if ( !componentsInPrefab.Contains(script.Script) ) {
                            scriptsToRemove.Add(indexOfScript);
                        }
                        indexOfScript++;
                    }
                    while (scriptsToRemove.Count > 0) {
                        int indexToRemove = scriptsToRemove[0];
                        scriptsToRemove.RemoveAt(0);
                        if (indexToRemove < scriptList.GetScripts().Count) {
                            scriptList.GetScripts().RemoveAt(indexToRemove);
                        }
                    }
                    // Now show all scripts list to allow user to enable/disable
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(30.0f);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("When remote input is enabled some script's may need to keep enabled. You can use this option to keep script enabled for local player when using remote input mode", "oo_info");
                    EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Don't Disable");
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Delay to disable");
                    GUILayout.Space(210.0f);
                    EditorGUILayout.EndHorizontal();
                    EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                    GUILayout.Space(2.0f);
                    GameObject currentChild = null;
                    foreach (ScriptStatus script in scriptList.GetScripts()) {
                        if (!script.Script.gameObject.Equals(currentChild)) {
                            EditorGUILayout.BeginHorizontal();
                            EditorUtils.PrintHeader(script.Script.gameObject.name, EditorUtils.SUB_DETAIL_PANEL_COLOR, Color.white, 12, "oo_prefab");
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(this.GetHierarchySpace(script.Script, networkPrefab));
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.BeginVertical();
                        if ( EditorUtils.PrintBooleanSquared(ref script.Enabled, script.Script.GetType().Name, null, 14, 12, true, 2.5f) ) {                            
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                            AssetDatabase.SaveAssets();
		                    AssetDatabase.Refresh();
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.FlexibleSpace();
                        int previousDelay = script.Delay;
                        EditorUtils.DrawHorizontalIntBar(ref script.Delay, 0, 999);
                        if ( previousDelay != script.Delay ) {
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                            AssetDatabase.SaveAssets();
		                    AssetDatabase.Refresh();
                        }
                        GUILayout.Space(2.0f);
                        GUILayout.Label(string.Format("{0} ms", script.Delay.ToString("D3")));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndHorizontal();
                        // Update new child
                        currentChild = script.Script.gameObject;
                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(15.0f);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(10.0f);
                }


                // Draw script variables
                if ( isToShowVariables ) {
                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.DETAIL_INFO_COLOR.WithAlpha(TRANSPORT_BACKGROUND_ALPHA)));
                    EditorUtils.PrintHeader("Scripts variables", EditorUtils.DETAIL_INFO_COLOR.WithAlpha(1.0f), Color.white, 14, "oo_script_invisible");
                    EditorGUILayout.EndHorizontal();

                    // Need to have child list to check id script is into child
                    if (prefabEntry.GetChildObjects() == null) {
                        prefabEntry.SetChildObjects(new GameObjectList());
                    }

                    GameObject      networkPrefab   = prefabEntry.GetPrefab();
                    ScriptList      scriptList      = prefabEntry.GetPrefabScripts();
                    if ( scriptList == null ) {
                        prefabEntry.SetPrefabScripts(new ScriptList());
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
		                AssetDatabase.Refresh();
                        scriptList = prefabEntry.GetPrefabScripts();
                    }
                    // Check if all script are in list
                    MonoBehaviour[] componentsInPrefab = networkPrefab.GetComponentsInChildren<MonoBehaviour>(true);
                    foreach (MonoBehaviour component in componentsInPrefab) {
                        if (!NetworkScriptsReference.IsIgnoreType(component.GetType())) {
                            if (!scriptList.ContainsScript(component)) {
                                // To not put some gosth script
                                if ((component.gameObject.Equals(networkPrefab)) ||
                                    (prefabEntry.GetChildObjects().HasChildScript(component))) {
                                    scriptList.RegisterScript(component);
                                }
                            }
                        }
                    }
                    // Now remove components that are not in prefab
                    int indexOfScript = 0;
                    List<int> scriptsToRemove = new List<int>();
                    foreach (ScriptStatus script in scriptList.GetScripts()) {
                        if ( !componentsInPrefab.Contains(script.Script) ) {
                            scriptsToRemove.Add(indexOfScript);
                        }
                        indexOfScript++;
                    }
                    while (scriptsToRemove.Count > 0) {
                        int indexToRemove = scriptsToRemove[0];
                        scriptsToRemove.RemoveAt(0);
                        if (indexToRemove < scriptList.GetScripts().Count) {
                            scriptList.GetScripts().RemoveAt(indexToRemove);
                        }
                    }
                    // Now show all scripts list to allow user to enable/disable
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(30.0f);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintSimpleExplanation("By default class variables are not automatically synchronized, nonetheless, you can check to synchronize it");
                    EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Enable Synchronize");
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Click to refresh");
                    GUILayout.Space(5.0f);
                    if (GUILayout.Button(Resources.Load("oo_refresh") as Texture, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) {
                        // Reload variables
                        if (scriptList != null) {
                            foreach (ScriptStatus script in scriptList.GetScripts()) {
                                if (script.Script != null) {
                                    if (typeof(NetworkBehaviour).IsAssignableFrom(script.Script.GetType())) {
                                        if (script.GetVariables() != null) {
                                            script.RefreshVariables();
#if UNITY_EDITOR
                                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                                            AssetDatabase.SaveAssets();
                                            AssetDatabase.Refresh();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    GUILayout.Space(10.0f);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(2.0f);
                    EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                    GUILayout.Space(2.0f);
                    GameObject currentChild = null;
                    foreach (ScriptStatus script in scriptList.GetScripts()) {
                        if (script.Script != null) {
                            if (typeof(NetworkBehaviour).IsAssignableFrom(script.Script.GetType())) {
                                if (!script.Script.gameObject.Equals(currentChild)) {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorUtils.PrintHeader(script.Script.gameObject.name, EditorUtils.SUB_DETAIL_PANEL_COLOR, Color.white, 12, "oo_prefab");
                                    EditorGUILayout.EndHorizontal();
                                }
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(this.GetHierarchySpace(script.Script, networkPrefab));
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.BeginVertical();
                                EditorUtils.PrintExplanationLabel(script.Script.GetType().Name, "oo_script_visible", EditorUtils.SUB_DETAIL_FONT_COLOR);
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(20.0f);
                                GUILayout.BeginVertical();


                                GUILayout.Space(5.0f);
                                GUILayout.BeginHorizontal();
                                GUILayout.BeginHorizontal(GUILayout.Width(180));
                                EditorUtils.PrintExplanationLabel("Primitive Variables", "oo_variables_activated", EditorUtils.SUB_DETAIL_FONT_COLOR, 15f, 13, 0, true, 0f);
                                GUILayout.EndHorizontal();
                                GUILayout.Space(-30.0f);
                                GUILayout.BeginHorizontal(GUILayout.Width(180));
                                GUILayout.BeginVertical();
                                GUILayout.Space(-1.0f);
                                EditorUtils.PrintSimpleExplanation("[ Declared as normal variable ]", EditorUtils.DEFAULT_EXPLANATION_FONT_SIZE, Color.cyan);
                                GUILayout.EndVertical();                                
                                GUILayout.EndHorizontal();

                                GUILayout.FlexibleSpace();
                                GUILayout.Label("Changes Detection");
                                GUILayout.Space(45.0f);
                                GUILayout.Label("Delivery Mode");
                                GUILayout.Space(66.0f);

                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(20.0f);
                                GUILayout.BeginVertical();
                                GUILayout.Space(5.0f);
                                Color[] gridColors = { Color.gray, Color.white };
                                int index = 0;
                                foreach (VariableStatus variable in script.GetVariables().GetVariables()) {
                                    GUILayout.BeginHorizontal(BackgroundStyle.Get(gridColors[index++ % 2].WithAlpha(0.05f)));
                                    GUILayout.BeginVertical();
                                    GUILayout.Space(3.0f);
                                    GUILayout.BeginHorizontal();                                    
                                    if (EditorUtils.PrintBooleanSquared(ref variable.Enabled, variable.Variable, null, 14, 12, true, 2.5f)) {
#if UNITY_EDITOR
                                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                                        AssetDatabase.SaveAssets();
                                        AssetDatabase.Refresh();
                                    }
                                    GUILayout.FlexibleSpace();
                                    // Draw detection mode
                                    if (DeliveryMode.Reliable.Equals(variable.DeliveryMode)) {
                                        GUILayout.BeginVertical();
                                        GUILayout.Space(-1.0f);
                                        ReliableDetectionMode previousDetectionMode = variable.DetectionMode;
                                        variable.DetectionMode = this.DrawReliableDetectionMode(variable.DetectionMode);
                                        if (previousDetectionMode != variable.DetectionMode) {
#if UNITY_EDITOR
                                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                                            AssetDatabase.SaveAssets();
                                            AssetDatabase.Refresh();
                                        }
                                        GUILayout.EndVertical();
                                    }
                                    // Draw delivery mode
                                    GUILayout.BeginVertical();
                                    GUILayout.Space(-1.0f);
                                    DeliveryMode previousDeliveryMode = variable.DeliveryMode;
                                    variable.DeliveryMode = this.DrawDeliveryMode(variable.DeliveryMode);
                                    if (previousDeliveryMode != variable.DeliveryMode) {
#if UNITY_EDITOR
                                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                                        AssetDatabase.SaveAssets();
                                        AssetDatabase.Refresh();
                                    }
                                    GUILayout.EndVertical();
                                    GUILayout.EndHorizontal();
                                    GUILayout.Space(3.0f);
                                    GUILayout.EndVertical();
                                    GUILayout.EndHorizontal();
                                    GUILayout.Space(2.0f);
                                }
                                GUILayout.Space(5.0f);
                                EditorGUILayout.EndVertical();
                                GUILayout.EndHorizontal();

                                GUILayout.Space(5.0f);
                                GUILayout.BeginHorizontal();
                                GUILayout.BeginHorizontal(GUILayout.Width(180));
                                EditorUtils.PrintExplanationLabel("Wrapped Variables", "oo_variables_activated", EditorUtils.SUB_DETAIL_FONT_COLOR, 15f, 13, 0, true, 0f);
                                GUILayout.EndHorizontal();
                                GUILayout.Space(-30.0f);
                                GUILayout.BeginHorizontal(GUILayout.Width(220));
                                GUILayout.BeginVertical();
                                GUILayout.Space(-1.0f);
                                EditorUtils.PrintSimpleExplanation("[ Declared as NetworkVariable<T> ]", EditorUtils.DEFAULT_EXPLANATION_FONT_SIZE, Color.cyan);
                                GUILayout.EndVertical();
                                GUILayout.EndHorizontal();
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(20.0f);
                                GUILayout.BeginVertical();
                                GUILayout.Space(5.0f);
                                foreach (VariableStatus variable in script.GetNetworkVariables().GetVariables()) {
                                    GUILayout.BeginHorizontal(BackgroundStyle.Get(gridColors[index++ % 2].WithAlpha(0.05f)));
                                    GUILayout.BeginVertical();
                                    GUILayout.Space(3.0f);
                                    GUILayout.BeginHorizontal();
                                    EditorUtils.PrintExplanationLabel(variable.Variable, "oo_variables_inactive", EditorUtils.SUB_DETAIL_FONT_COLOR, 7f, 13, 0, true, 0f);
                                    GUILayout.FlexibleSpace();
                                    GUILayout.BeginVertical();
                                    GUILayout.Space(-1.0f);

                                    GUILayout.BeginHorizontal();
                                    // Draw detection mode
                                    if (DeliveryMode.Reliable.Equals(variable.DeliveryMode)) {
                                        GUILayout.BeginVertical();
                                        GUILayout.Space(-1.0f);
                                        ReliableDetectionMode previousDetectionMode = variable.DetectionMode;
                                        variable.DetectionMode = this.DrawReliableDetectionMode(variable.DetectionMode);
                                        if (previousDetectionMode != variable.DetectionMode) {
#if UNITY_EDITOR
                                            UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                                            AssetDatabase.SaveAssets();
                                            AssetDatabase.Refresh();
                                        }
                                        GUILayout.EndVertical();
                                    }
                                    // Draw delivery mode
                                    GUILayout.BeginVertical();
                                    GUILayout.Space(-1.0f);
                                    DeliveryMode previousDeliveryMode = variable.DeliveryMode;
                                    variable.DeliveryMode = this.DrawDeliveryMode(variable.DeliveryMode);
                                    if (previousDeliveryMode != variable.DeliveryMode) {
#if UNITY_EDITOR
                                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                                        AssetDatabase.SaveAssets();
                                        AssetDatabase.Refresh();
                                    }
                                    GUILayout.EndVertical();
                                    GUILayout.EndHorizontal();

                                    GUILayout.EndVertical();
                                    GUILayout.EndHorizontal();
                                    GUILayout.Space(3.0f);
                                    GUILayout.EndVertical();
                                    GUILayout.EndHorizontal();
                                    GUILayout.Space(2.0f);
                                }
                                GUILayout.Space(5.0f);
                                EditorGUILayout.EndVertical();
                                GUILayout.EndHorizontal();

                                EditorGUILayout.EndVertical();
                                GUILayout.EndHorizontal();
                                EditorGUILayout.EndVertical();

                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.EndHorizontal();
                                // Update new child
                                currentChild = script.Script.gameObject;
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(15.0f);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(10.0f);
                }
                NetworkPrefabsEventsEntry prefabEvents = prefabEntry.GetEvents();
                if (prefabEvents == null) {
                    Debug.Log("NetworkPrefabsEventsEntry created");
                    prefabEntry.SetEvents(new NetworkPrefabsEventsEntry());
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    prefabEvents = prefabEntry.GetEvents();
                }
                if (prefabEvents.onSpawnPrefab == null) {
                    Debug.Log("EventReference created");
                    prefabEvents.onSpawnPrefab = new EventReferencePrefab();
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
                    //UnityEditor.EditorUtility.SetDirty(prefabEvents.onSpawnPrefab);
#endif
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                
                if (isToShowEvents) {
                    // var serializedEvents = new SerializedObject(prefabEntry.GetEvents());
                    // serializedEvents.Update();
                    bool updated = false;
                    updated |= this.DrawEvent(prefabEntry.GetOnSpawnPrefab(),                  "onSpawnPrefab");
                    updated |= this.DrawEvent(prefabEntry.GetOnDespawnPrefab(),                "onDespawnPrefab");
                    updated |= this.DrawEvent(prefabEntry.GetOnTakeObjectOwnership(),          "onTakeObjectOwnerShip");
                    updated |= this.DrawEvent(prefabEntry.GetOnReleaseObjectOwnership(),       "onReleaseObjectOwnerShip");
                    updated |= this.DrawEvent(prefabEntry.GetOnAcceptObjectOwnerShip(),        "onAcceptOwnerShip");
                    updated |= this.DrawEvent(prefabEntry.GetOnAcceptReleaseObjectOwnerShip(), "onAcceptReleaseOwnerShip");
                    // serializedEvents.ApplyModifiedProperties();
                    if (updated) {
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }

                EditorUtils.HorizontalLine(Color.white.WithAlpha(0.25f), 1.0f, new Vector2(0f, 0f));
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            if ( prefabToRemove != null ) {
                this.prefabsDatabase.UnregisterPrefab(prefabToRemove);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(prefabsDatabase);
#endif
                AssetDatabase.SaveAssets();
		        AssetDatabase.Refresh();
            }
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(0.05f)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 30.0f));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(5);
            EditorUtils.PrintExplanationLabel((displayTooltip) ? tolltipText : "You can register all already existent prefabs on your game without changing your game login. Those objects will work seamless into the multiplayer game", (displayTooltip) ? "oo_note" : "oo_info", Color.white, 5f);
            EditorGUILayout.Space(10);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();            
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth - 30.0f));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();           
            GUILayout.FlexibleSpace();
            EditorUtils.PrintImageButton("Close", "oo_close", EditorUtils.IMAGE_BUTTON_COLOR, EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Close();
            });
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();            
        }

        /// <summary>
        /// Called when [inspector update].
        /// </summary>
        void OnInspectorUpdate() {
            Repaint();
        }

        /// <summary>
        /// Draws the animation modes.
        /// </summary>
        /// <param name="selectedIndex">Index of the selected.</param>
        /// <returns>AnimationSyncType.</returns>
        private AnimationSyncType DrawAnimationModes(AnimationSyncType selectedIndex) {
            AnimationSyncType result = AnimationSyncType.UseParameters;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);
            int          selectedAnimationTypeIndex  = (int)selectedIndex;
            List<string> animationTypesName          = new List<string>();
            animationTypesName.Add(AnimationSyncType.UseController.ToString());
            animationTypesName.Add(AnimationSyncType.UseParameters.ToString());
            animationTypesName.Add(AnimationSyncType.ManualControl.ToString());
            int previousSize = GUI.skin.label.fontSize;
            int selectedMethod = EditorGUILayout.Popup(selectedAnimationTypeIndex, animationTypesName.ToArray<string>(), GUILayout.Width(150));
            if ( selectedMethod == (int)AnimationSyncType.UseController ) {
                result = AnimationSyncType.UseController;
            } else if ( selectedMethod == (int)AnimationSyncType.UseParameters ) {
                result = AnimationSyncType.UseParameters;
            } else if ( selectedMethod == (int)AnimationSyncType.ManualControl ) {
                result = AnimationSyncType.ManualControl;
            }
            EditorGUILayout.EndHorizontal();
            return result;
        }

        /// <summary>
        /// Collects the default animation status.
        /// </summary>
        /// <param name="targetAnimator">The target animator.</param>
        /// <returns>System.String.</returns>
        private string CollectDefaultAnimationStatus(Animator targetAnimator) {
            string  result      = "";
            bool    collected   = false;
            if (targetAnimator.runtimeAnimatorController != null) {
                AnimatorController controller = targetAnimator.runtimeAnimatorController as AnimatorController;
                if (controller != null) {
                    AnimatorControllerLayer[] layers = controller.layers;
                    foreach (AnimatorControllerLayer layer in layers) {
                        ChildAnimatorState[] animStates = layer.stateMachine.states;
                        foreach (ChildAnimatorState state in animStates) {
                            if ((state.state.motion is BlendTree) &&
                                (collected == false)) {
                                result = state.state.name;
                                collected = true;
                                break;
                            }
                        }
                        if (collected) {
                            break;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Draws ownership levels.
        /// </summary>
        /// <param name="selectedIndex">Index of the selected.</param>
        /// <returns>OwnerShipAccessLevel.</returns>
        private OwnerShipAccessLevel DrawOwnerShipLevel(OwnerShipAccessLevel selectedIndex) {
            OwnerShipAccessLevel result = OwnerShipAccessLevel.Full;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);
            int selectedLevelTypeIndex = (int)selectedIndex;
            List<string> ownershipTypesName = new List<string>();
            ownershipTypesName.Add(OwnerShipAccessLevel.Full.ToString());
            ownershipTypesName.Add(OwnerShipAccessLevel.TakeObject.ToString());
            ownershipTypesName.Add(OwnerShipAccessLevel.TransferObject.ToString());
            ownershipTypesName.Add(OwnerShipAccessLevel.ClientOnly.ToString());
            ownershipTypesName.Add(OwnerShipAccessLevel.ServerOnly.ToString());

            int selectedMethod = EditorGUILayout.Popup(selectedLevelTypeIndex, ownershipTypesName.ToArray<string>(), GUILayout.Width(150));
            if (selectedMethod == (int)OwnerShipAccessLevel.Full) {
                result = OwnerShipAccessLevel.Full;
            } else if (selectedMethod == (int)OwnerShipAccessLevel.TakeObject) {
                result = OwnerShipAccessLevel.TakeObject;
            } else if (selectedMethod == (int)OwnerShipAccessLevel.TransferObject) {
                result = OwnerShipAccessLevel.TransferObject;
            } else if (selectedMethod == (int)OwnerShipAccessLevel.ClientOnly) {
                result = OwnerShipAccessLevel.ClientOnly;
            } else if (selectedMethod == (int)OwnerShipAccessLevel.ServerOnly) {
                result = OwnerShipAccessLevel.ServerOnly;
            }
            EditorGUILayout.EndHorizontal();
            return result;
        }

        /// <summary>
        /// Draws movement type.
        /// </summary>
        /// <param name="selectedIndex">Index of the selected.</param>
        /// <returns>OwnerShipAccessLevel.</returns>
        private PredictionType DrawMovementType(PredictionType selectedIndex) {
            PredictionType result = PredictionType.Automatic;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);
            int selectedLevelTypeIndex = (int)selectedIndex;
            List<string> movementTypesName = new List<string>();
            movementTypesName.Add(PredictionType.Automatic.ToString());
            movementTypesName.Add(PredictionType.UsePhysics.ToString());
            movementTypesName.Add(PredictionType.UseTransform.ToString());

            int selectedMethod = EditorGUILayout.Popup(selectedLevelTypeIndex, movementTypesName.ToArray<string>(), GUILayout.Width(120));
            if (selectedMethod == (int)PredictionType.Automatic) {
                result = PredictionType.Automatic;
            } else if (selectedMethod == (int)PredictionType.UsePhysics) {
                result = PredictionType.UsePhysics;
            } else if (selectedMethod == (int)PredictionType.UseTransform) {
                result = PredictionType.UseTransform;
            }
            EditorGUILayout.EndHorizontal();
            return result;
        }


        /// <summary>
        /// Draws DeliveryMode.
        /// </summary>
        /// <param name="mode">Index of the selected.</param>
        /// <returns>DeliveryMode.</returns>
        private DeliveryMode DrawDeliveryMode(DeliveryMode mode) {
            DeliveryMode result = DeliveryMode.Reliable;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);
            int selectedModeTypeIndex = (int)mode;
            List<string> modesTypesName = new List<string>();
            modesTypesName.Add(DeliveryMode.Reliable.ToString());
            modesTypesName.Add(DeliveryMode.Unreliable.ToString());

            int selectedMethod = EditorGUILayout.Popup(selectedModeTypeIndex, modesTypesName.ToArray<string>(), GUILayout.Width(150));
            if (selectedMethod == (int)DeliveryMode.Reliable) {
                result = DeliveryMode.Reliable;
            } else if (selectedMethod == (int)DeliveryMode.Unreliable) {
                result = DeliveryMode.Unreliable;
            } else {
                result = DeliveryMode.Reliable;
            }
            EditorGUILayout.EndHorizontal();
            return result;
        }

        /// <summary>
        /// Draws DeliveryMode.
        /// </summary>
        /// <param name="mode">Index of the selected.</param>
        /// <returns>DeliveryMode.</returns>
        private ReliableDetectionMode DrawReliableDetectionMode(ReliableDetectionMode mode) {
            ReliableDetectionMode result = ReliableDetectionMode.Automatic;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);
            int selectedModeTypeIndex = (int)mode;
            List<string> modesTypesName = new List<string>();
            modesTypesName.Add(ReliableDetectionMode.Automatic.ToString());
            modesTypesName.Add(ReliableDetectionMode.Manual.ToString());
            
            int selectedMethod = EditorGUILayout.Popup(selectedModeTypeIndex, modesTypesName.ToArray<string>(), GUILayout.Width(150));
            if (selectedMethod == (int)ReliableDetectionMode.Automatic) {
                result = ReliableDetectionMode.Automatic;
            } else if (selectedMethod == (int)ReliableDetectionMode.Manual) {
                result = ReliableDetectionMode.Manual;
            } else {
                result = ReliableDetectionMode.Automatic;
            }
            EditorGUILayout.EndHorizontal();
            return result;
        }

        /// <summary>
        /// Gets the hierarchy space.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="root">The root.</param>
        /// <returns>System.Single.</returns>
        private float GetHierarchySpace(MonoBehaviour script, GameObject root) {
            const float PADDING = 5f;
            int spacesAmount = 0;
            GameObject currentGameObject = script.gameObject;
            while (currentGameObject != root) {
                currentGameObject    = currentGameObject.transform.parent.gameObject;
                spacesAmount++;
            }
            return (spacesAmount * PADDING);
        }

        /// <summary>
        /// Return how many inheritance levels this component has
        /// </summary>
        /// <param name="root">GameObject root of this transform</param>
        /// <param name="child">Transform child to check</param>
        /// <returns></returns>
        private int GetInheritanceLevel(GameObject root, Transform child) {
            int counter = 0;
            Transform current = child;
            while (current != root.transform) {
                current = current.parent;
                counter++;
            }
            return Mathf.Clamp(counter-1, 0, Int16.MaxValue);
        }

        /// <summary>
        /// Draws the event.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="onDelete">The on delete.</param>
        private bool DrawEvent(EventReferencePrefab eventToDraw, string propertyName) {
            bool result = false;
            if (eventToDraw != null) {
                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.EVENT_BACKGROUND_COLOR.WithAlpha(0.10f)));
                EditorGUILayout.BeginVertical();

                bool isFilled   = (eventToDraw.GetEventTarget() != null);
                bool isVisible  = eventToDraw.IsEditorVisible();
                EditorGUILayout.BeginHorizontal();
                EventReferenceSide referenceType = this.GetReferenceSide(eventToDraw, propertyName);
                result = EditorUtils.PrintVisibilityBooleanWithIcon(ref isVisible, isFilled, this.GetEventName(eventToDraw, propertyName), null, (EventReferenceSide.ServerSide.Equals(referenceType)) ? "oo_cloud" : (EventReferenceSide.ClientSide.Equals(referenceType)) ? "oo_workstation" : "oo_both_sides");
                
                EditorGUILayout.EndHorizontal();
                eventToDraw.SetEditorVisible(isVisible);

                if (isVisible) {
                    result |= this.DrawEventEditor(eventToDraw, this.GetReturnType(eventToDraw, propertyName), this.GetParametersType(eventToDraw, propertyName));
                    string eventDescription = this.GetEventDescription(eventToDraw, propertyName);
                    if (!string.IsNullOrEmpty(eventDescription)) {
                        GUILayout.Space(10.0f);
                        EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                        GUILayout.Space(5.0f);
                        EditorUtils.PrintExplanationLabel(eventDescription, "oo_info");
                        // Draw event parameters
                        Type[] parametersType = this.GetParametersType(eventToDraw, propertyName);
                        if (parametersType.Length > 0) {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30.0f);
                            EditorGUILayout.BeginVertical();
                            GUILayout.Space(10.0f);
                            EditorUtils.PrintSimpleExplanation("Function Arguments");
                            int argumentIndex = 1;
                            foreach (Type tp in parametersType) {
                                EditorUtils.PrintExplanationLabel(string.Format("[{0}] Argument : {1}", argumentIndex++, tp.Name), "oo_variables_activated");
                                GUILayout.Space(5.0f);
                            }
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        }
                        Type returnType = this.GetReturnType(eventToDraw, propertyName);
                        if (returnType != null) {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30.0f);
                            EditorGUILayout.BeginVertical();
                            GUILayout.Space(10.0f);
                            EditorUtils.PrintSimpleExplanation("Return Type");
                            EditorUtils.PrintExplanationLabel(string.Format("{0}", returnType.Name), "oo_variables_inactive", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        }
                        // Create the example function
                        string resultTypeStr        = (returnType != null) ? returnType.Name : "void";
                        string parametersTypeStr    = " ";
                        int paramCount = 0;
                        foreach (Type tp in parametersType) {
                            if (paramCount > 0)
                                parametersTypeStr += ",  ";
                            parametersTypeStr += string.Format("{0}  {1}", parametersType[0].Name.ToLower(), parametersType[0].Name);
                            paramCount++;
                        }
                        parametersTypeStr += " ";
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(30.0f);
                        EditorGUILayout.BeginVertical();
                        GUILayout.Space(10.0f);
                        EditorUtils.PrintSimpleExplanation("Function example");
                        EditorUtils.PrintExplanationLabel(string.Format("  {0}  {1} ({2})",
                                                                        resultTypeStr,
                                                                        this.GetEventName(eventToDraw, propertyName),
                                                                        parametersTypeStr), "oo_note", Color.white);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                    GUILayout.Space(10.0f);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5.0f);
            }
            return result;
        }

        /// <summary>
        /// Draws the event editor.
        /// </summary>
        private bool DrawEventEditor(EventReferencePrefab eventToDraw, Type returnType, Type[] parametersType) {
            bool result = false;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical();
            // this.BeforeDrawChildsData(); // Paint any data on inherithed classes
            GUILayout.Space(5.0f);
            EditorUtils.HorizontalLine(Color.gray, 1.0f, Vector2.zero);
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Object Source");
            GameObject eventTarget = eventToDraw.GetEventTarget();
            eventToDraw.SetEventTarget(EditorGUILayout.ObjectField(eventToDraw.GetEventTarget(), typeof(GameObject), true, GUILayout.Width(250)) as GameObject);
            result |= (eventTarget != eventToDraw.GetEventTarget());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            List<MonoBehaviour> components = new List<MonoBehaviour>();
            List<String> componentsName = new List<String>();
            if (eventToDraw.GetEventTarget() != null) {
                foreach (MonoBehaviour component in eventToDraw.GetEventTarget().GetComponents<MonoBehaviour>()) {
                    if (typeof(MonoBehaviour).IsAssignableFrom(component.GetType())) {
                        components.Add(component);
                        componentsName.Add(component.GetType().Name);
                    }
                }
            }

            if (eventToDraw.GetEventTarget() != null) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Component Source");
                int selectedObjectIndex = ((eventToDraw.GetEventComponent() != null) ? Array.IndexOf(componentsName.ToArray<string>(), eventToDraw.GetEventComponent().GetType().Name) : -1);
                int selectedObject = EditorGUILayout.Popup(selectedObjectIndex, componentsName.ToArray<string>(), GUILayout.Width(250));
                MonoBehaviour eventComponent = eventToDraw.GetEventComponent();
                eventToDraw.SetEventComponent(((selectedObject < components.Count) && (selectedObject > -1)) ? components[selectedObject] : null);
                result |= (eventComponent != eventToDraw.GetEventComponent());
                EditorGUILayout.EndHorizontal();
                if (selectedObject > -1) {
                    List<MethodInfo> methods = new List<MethodInfo>();
                    List<String> methodsName = new List<String>();
                    if (eventToDraw.GetEventComponent() != null) {
                        foreach (MethodInfo method in eventToDraw.GetEventComponent().GetType().GetMethods(BindingFlags.Public
                                                                                                           | BindingFlags.Instance
                                                                                                           | BindingFlags.DeclaredOnly)) {
                            if ((method.ReturnParameter.ParameterType == typeof(void)) ||
                                (method.ReturnParameter.ParameterType == returnType)) {
                                bool parametersMatch = false;
                                ParameterInfo[] arguments = method.GetParameters();
                                parametersMatch |= ((parametersType == null) && ((arguments == null) || (arguments.Length == 0)));
                                if (!parametersMatch) {
                                    if ((parametersType != null) && (arguments != null)) {
                                        if (parametersType.Length == arguments.Length) {
                                            parametersMatch = true; // True to be checked behind
                                            for (int parameterindex = 0; parameterindex < parametersType.Length; parameterindex++) {
                                                parametersMatch &= (parametersType[parameterindex] == arguments[parameterindex].ParameterType);
                                                parametersMatch &= (parametersType[parameterindex].IsArray == arguments[parameterindex].ParameterType.IsArray);
                                            }
                                        }
                                    }
                                }
                                if (parametersMatch) {
                                    methods.Add(method);
                                    methodsName.Add(method.Name);
                                }
                            }
                        }
                    }
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Method To Execute");
                    int selectedMethodIndex = (eventToDraw.GetEventMethod() != null) ? Array.IndexOf(methodsName.ToArray<string>(), eventToDraw.GetEventMethod()) : -1;
                    int selectedMethod = EditorGUILayout.Popup(selectedMethodIndex, methodsName.ToArray<string>(), GUILayout.Width(250));
                    EditorGUILayout.EndHorizontal();
                    string eventName = eventToDraw.GetEventMethod();
                    if ((selectedMethod > -1) && (selectedMethod < methods.Count)) {                        
                        eventToDraw.SetEventMethod(methods[selectedMethod].Name);                        
                    } else {
                        eventToDraw.SetEventMethod(null);
                    }
                    result |= (eventName != eventToDraw.GetEventMethod());
                }
            } else {
                eventToDraw.SetEventMethod(null);
            }

            // this.AfterDrawChildsData();

            EditorGUILayout.EndVertical();
            GUILayout.Space(10.0f);
            EditorGUILayout.EndHorizontal();

            return result;
        }

        /// <summary>
        /// Gets the name of the event.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <returns>System.String.</returns>
        private string GetEventName(EventReferencePrefab eventToDraw, string propertyName) {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var property = this.GetManagedType().GetField(propertyName, bindingFlags);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).EventName : null;
        }

        /// <summary>
        /// Gets the event descriptiom.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <returns>System.String.</returns>
        private string GetEventDescription(EventReferencePrefab eventToDraw, string propertyName) {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var property = this.GetManagedType().GetField(propertyName, bindingFlags);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).EventDescriptiom : null;
        }

        /// <summary>
        /// Gets the type of the return.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <returns>Type.</returns>
        private System.Type GetReturnType(EventReferencePrefab eventToDraw, string propertyName) {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var property = this.GetManagedType().GetField(propertyName, bindingFlags);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).ReturnType : null;
        }

        /// <summary>
        /// Gets the type of the parameters.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <returns>Type[].</returns>
        private System.Type[] GetParametersType(EventReferencePrefab eventToDraw, string propertyName) {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var property = this.GetManagedType().GetField(propertyName, bindingFlags);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).ParametersType : null;
        }

        /// <summary>
        /// Gets the reference side.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <returns>EventReferenceSide.</returns>
        private EventReferenceSide GetReferenceSide(EventReferencePrefab eventToDraw, string propertyName) {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var property = this.GetManagedType().GetField(propertyName, bindingFlags);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).ExecutionSide : EventReferenceSide.ServerSide;
        }

        /// <summary>
        /// Gets the type of the managed.
        /// </summary>
        /// <returns>Type.</returns>
        public virtual System.Type GetManagedType() {
            return typeof(NetworkPrefabsEventsEntry);
        }
    }
#endif
}