using System.IO;
using UnityEditor;
using UnityEngine;

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    /// <summary>
    /// This class draws a Network Events Database Window
    /// Implements the <see cref="EditorWindow" />
    /// </summary>
    /// <seealso cref="EditorWindow" />
    public class NetworkEventsDatabaseWindow : EditorWindow {
        /// <summary>
        /// The scroll position
        /// </summary>
        private Vector2 scrollPos;

        /// <summary>
        /// The event to register
        /// </summary>
        private string eventToRegister;

        /// <summary>
        /// The event database
        /// </summary>
        private NetworkEventsDatabase eventDatabase;

        /// <summary>
        /// The target database
        /// </summary>
        private string targetDatabase = GlobalResources.DEFAULT_DATABASE;

        /// <summary>
        /// The register error
        /// </summary>
        private string registerError = null;

        /// <summary>
        /// The minimum network event name
        /// </summary>
        const int MIN_NETWORK_EVENT_NAME = 8;

        /// <summary>
        /// Opens the network events database window.
        /// </summary>
        /// <param name="database">The database.</param>
        public static void OpenNetworkEventsDatabaseWindow(string database = GlobalResources.DEFAULT_DATABASE) {
            NetworkEventsDatabaseWindow window = ScriptableObject.CreateInstance(typeof(NetworkEventsDatabaseWindow)) as NetworkEventsDatabaseWindow;
            window.targetDatabase = database;
            if (!GlobalResources.DEFAULT_DATABASE.Equals(database)) {
                window.eventDatabase = Resources.Load<NetworkEventsDatabase>(GlobalResources.GetEventsDatabase(window.targetDatabase));
            }
            window.ShowUtility();
        }

        /// <summary>
        /// Opens the network database source.
        /// </summary>
        /// <param name="selectOnInspector">if set to <c>true</c> [select on inspector].</param>
        public static void OpenNetworkDatabaseSource(bool selectOnInspector = true) {
            NetworkEventsDatabase assetObject = Resources.Load<NetworkEventsDatabase>(GlobalResources.GetEventsDatabase());
            if (assetObject == null) {
                CreateNetworkDatabaseSource();
                assetObject = Resources.Load<NetworkEventsDatabase>(GlobalResources.GetEventsDatabase());
                if (assetObject == null) {
                    string sourcePath = ResourceUtils.ResourcesPath + "/" + GlobalResources.GetEventsDatabase() + ".asset";
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
            if ((GlobalResources.GetEventsDatabase(databasePath) == null) ||
                (GlobalResources.GetEventsDatabase(databasePath).Length == 0)) {
                return;
            }			
			Object GlobalSource = Resources.Load(GlobalResources.GetEventsDatabase(databasePath));
            NetworkEventsDatabase sourceData = ScriptableObject.CreateInstance<NetworkEventsDatabase>();
            string databaseFolder   = ResourceUtils.ResourcesPath;
            string fullresFolder    = Application.dataPath + databaseFolder.Replace("Assets", "");
            if (!System.IO.Directory.Exists(fullresFolder)) {
                System.IO.Directory.CreateDirectory(fullresFolder);
            }
            string sourcePath   = databaseFolder + "/" + GlobalResources.GetEventsDatabase(databasePath) + ".asset";
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
            this.maxSize        = new Vector2(440f, 830f);
            this.minSize        = this.maxSize;
            this.titleContent   = new GUIContent ("Events Database", Resources.Load("oo_info") as Texture);
            this.eventDatabase  = Resources.Load<NetworkEventsDatabase>(GlobalResources.GetEventsDatabase(this.targetDatabase));
            EditorWindowExtensions.CenterOnMainWin(this);
        }

        /// <summary>
        /// Called when [GUI].
        /// </summary>
        void OnGUI() {
            // Not allow chnages during playmode
            if (Application.isPlaying) {
                EditorUtils.PrintImage("objectnet_logo", Color.blue, 0, 25);
                GUILayout.Space(5.0f);
                EditorUtils.PrintImageButton("Changes are disabled during PlayMode", "oo_info", Color.red.WithAlpha(0.15f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {                    
                });
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.Space(5);
            EditorUtils.PrintImage("objectnet_logo", Color.blue, 0, 25);
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.yellow.WithAlpha(0.10f)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 25.0f));
            EditorGUILayout.Space(5);
            EditorUtils.PrintExplanationLabel("Fill the event name and press \"Register Event\" to create a new event", "oo_event", Color.green.WithAlpha(0.5f));
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Event name to register");
            EditorGUILayout.BeginHorizontal();
            this.eventToRegister = EditorGUILayout.TextField(this.eventToRegister, GUILayout.Width(240));
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(-15);
            EditorUtils.PrintImageButton("Register Event", "oo_event", EditorUtils.EVENT_BACKGROUND_COLOR.WithAlpha(0.50f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, 12, 12f, 12f, () => {
                bool registerEvent = true;
                if (!this.eventDatabase.EventExists(this.eventToRegister)) {
                    if (this.eventToRegister.Trim().Length < MIN_NETWORK_EVENT_NAME) { 
                        registerEvent = false;
                        this.registerError = string.Format("Network event must have at least {0} characters", MIN_NETWORK_EVENT_NAME);
                    } else if (!System.Text.RegularExpressions.Regex.IsMatch(this.eventToRegister, @"^[a-zA-Z]+$")) { 
                        registerEvent = false;
                        this.registerError = "Network events should not have any special character, spaces or numbers";
                    }
                } else {
                    registerEvent = false;
                    this.registerError = string.Format("Network Event \"{0}\" is already registered", this.eventToRegister);
                }
                if (registerEvent) {
                    this.eventDatabase.RegisterEvent(this.eventToRegister);
                    this.eventToRegister    = "";
                    this.registerError      = null;
                    EditorUtility.SetDirty(eventDatabase);
                    AssetDatabase.SaveAssets();
		            AssetDatabase.Refresh();                    
                }
                this.OnGUI();
            });
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal((this.registerError == null) ? BackgroundStyle.Get(Color.white.WithAlpha(0.10f)) : BackgroundStyle.Get(Color.red.WithAlpha(0.10f)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 25.0f));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(5);
            if (this.registerError == null) {
                EditorUtils.PrintExplanationLabel("Enter a unique event name to create a new event", "oo_info", Color.yellow, 5f);
            } else {
                EditorUtils.PrintExplanationLabel(this.registerError, "oo_error", EditorUtils.ERROR_FONT_COLOR);
            }
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth - 25.0f));
            EditorUtils.PrintHeader("Registered Events", EditorUtils.SUB_DETAIL_PANEL_COLOR, Color.white, 12, "oo_event");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal("Box");
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayout.Width(410), GUILayout.Height(445));
            NetworkEventEntry eventToRemove = null;
            foreach (NetworkEventEntry eventEntry in this.eventDatabase.GetEvents()) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(2);
                if (GUILayout.Button(Resources.Load("oo_delete") as Texture, GUIStyle.none, GUILayout.Width(14), GUILayout.Height(14))) {
                    eventToRemove = eventEntry;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(eventEntry.GetCode().ToString(), GUILayout.Width(100), GUILayout.Height(20));
                GUILayout.FlexibleSpace();
                EditorGUILayout.TextField(eventEntry.GetName(), GUILayout.Width(240), GUILayout.Height(20));
                EditorGUI.EndDisabledGroup();                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            if ( eventToRemove != null ) {
                this.eventDatabase.UnregisterEvent(eventToRemove);
            }
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(0.05f)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 25.0f));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(5);
            EditorUtils.PrintExplanationLabel("All events are unique for entirelly game, and only registered events can be used on inspector. Nonetheless you can send and receive unregistered events directly on code", "oo_info", Color.white, 5f);
            EditorGUILayout.Space(10);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();            
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth - 25.0f));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            EditorUtils.PrintImageButton("Close", "oo_close", EditorUtils.IMAGE_BUTTON_COLOR, EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
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
    }
#endif
}