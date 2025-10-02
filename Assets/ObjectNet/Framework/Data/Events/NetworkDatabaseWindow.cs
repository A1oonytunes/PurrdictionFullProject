using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    /// <summary>
    /// Class NetworkDatabaseWindow.
    /// Implements the <see cref="EditorWindow" />
    /// </summary>
    /// <seealso cref="EditorWindow" />
    public class NetworkDatabaseWindow : EditorWindow {
        /// <summary>
        /// The scroll position
        /// </summary>
        private Vector2 scrollPos;

        /// <summary>
        /// The database to register
        /// </summary>
        private string databaseToRegister;

        /// <summary>
        /// The register error
        /// </summary>
        private string registerError = null;

        /// <summary>
        /// The databases
        /// </summary>
        private string[] databases;

        /// <summary>
        /// The current selected
        /// </summary>
        private string currentSelected = GlobalResources.DEFAULT_DATABASE;

        /// <summary>
        /// The current manager
        /// </summary>
        private NetworkManager currentManager;

        /// <summary>
        /// The current events manager
        /// </summary>
        private NetworkEventsManager currentEventsManager;

        /// <summary>
        /// The target editor
        /// </summary>
        private IDatabaseTargetEditor targetEditor;

        /// <summary>
        /// The minimum network database name
        /// </summary>
        const int MIN_NETWORK_DATABASE_NAME = 8;

        /// <summary>
        /// Opens the network database window.
        /// </summary>
        /// <param name="networkManager">The network manager.</param>
        /// <param name="editor">The editor.</param>
        /// <param name="selectedDatabase">The selected database.</param>
        public static void OpenNetworkDatabaseWindow(NetworkManager networkManager, IDatabaseTargetEditor editor, string selectedDatabase = GlobalResources.DEFAULT_DATABASE) {
            NetworkDatabaseWindow window = ScriptableObject.CreateInstance(typeof(NetworkDatabaseWindow)) as NetworkDatabaseWindow;
            window.currentSelected  = selectedDatabase;
            window.currentManager   = networkManager;
            window.targetEditor     = editor;
            window.ShowUtility();
        }

        /// <summary>
        /// Opens the network database window.
        /// </summary>
        /// <param name="eventsManager">The events manager.</param>
        /// <param name="editor">The editor.</param>
        /// <param name="selectedDatabase">The selected database.</param>
        public static void OpenNetworkDatabaseWindow(NetworkEventsManager eventsManager, IDatabaseTargetEditor editor, string selectedDatabase = GlobalResources.DEFAULT_DATABASE) {
            NetworkDatabaseWindow window = ScriptableObject.CreateInstance(typeof(NetworkDatabaseWindow)) as NetworkDatabaseWindow;
            window.currentSelected      = selectedDatabase;
            window.currentEventsManager = eventsManager;
            window.targetEditor         = editor;
            window.ShowUtility();
        }

        /// <summary>
        /// Called when [enable].
        /// </summary>
        void OnEnable() {
            this.databases          = GlobalResources.GetAvaiableDatabases(false);
            this.maxSize            = new Vector2(420f, 730f);
            this.minSize            = this.maxSize;
            this.titleContent       = new GUIContent ("Network Databases", Resources.Load("oo_info") as Texture);
            EditorWindowExtensions.CenterOnMainWin(this);
        }

        /// <summary>
        /// Called when [GUI].
        /// </summary>
        void OnGUI() {
            // Not allow chnages during playmode
            if (Application.isPlaying) {
                EditorUtils.PrintImage("objectnet_logo", Color.blue, 0, 35);
                GUILayout.Space(5.0f);
                EditorUtils.PrintImageButton("Changes are disabled during PlayMode", "oo_info", Color.red.WithAlpha(0.15f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {                    
                });
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.Space(5);
            EditorUtils.PrintImage("objectnet_logo", Color.blue, 0, 35);
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.yellow.WithAlpha(0.10f)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 35.0f));
            EditorGUILayout.Space(5);
            EditorUtils.PrintExplanationLabel("Fill database name and press \"Create\" to create a new network database", "oo_event", Color.green.WithAlpha(0.5f));
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth - 35.0f));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.Space(20);

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Database name");
            this.databaseToRegister = EditorGUILayout.TextField(this.databaseToRegister, GUILayout.Width(240));
            EditorGUILayout.EndVertical();
                
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintImageButton("Create", "oo_cache", EditorUtils.EVENT_BACKGROUND_COLOR.WithAlpha(0.50f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, 14, 14f, 14f, () => {
                bool registerDatabase = true;
                if (!string.IsNullOrEmpty(this.databaseToRegister)) {
                    if (Array.IndexOf(databases, this.databaseToRegister) == -1) {
                        if (this.databaseToRegister.Trim().Length < MIN_NETWORK_DATABASE_NAME) {
                            registerDatabase = false;
                            this.registerError = string.Format("Network database must have at least {0} characters", MIN_NETWORK_DATABASE_NAME);
                        } else if (!System.Text.RegularExpressions.Regex.IsMatch(this.databaseToRegister, @"^[a-zA-Z0-9_.-]*$")) {
                            registerDatabase = false;
                            this.registerError = "Network database should not have any special character, spaces or numbers";
                        }
                    } else {
                        registerDatabase = false;
                        this.registerError = string.Format("Network database \"{0}\" is already registered", this.databaseToRegister);
                    }
                } else {
                    registerDatabase    = false;
                    this.registerError  = "Network database name must be filled";
                }
                if (registerDatabase) {
                    NetworkPrefabsDatabaseWindow.CreateNetworkDatabaseSource(this.databaseToRegister);
                    NetworkEventsDatabaseWindow.CreateNetworkDatabaseSource(this.databaseToRegister);
                    // Refresh unity database
                    AssetDatabase.SaveAssets();
		            AssetDatabase.Refresh();
                    // Invalidate data to no repeat
                    this.databaseToRegister     = "";
                    this.registerError          = null; 
                    // Refresh database array
                    this.databases = GlobalResources.GetAvaiableDatabases(false);
                }
                this.OnGUI();
            });
            EditorGUILayout.Space(12);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal((this.registerError == null) ? BackgroundStyle.Get(Color.white.WithAlpha(0.10f)) : BackgroundStyle.Get(Color.red.WithAlpha(0.10f)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 35.0f));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(5);
            if (this.registerError == null) {
                EditorUtils.PrintExplanationLabel("Enter a unique database name to create a new database", "oo_info", Color.yellow, 5f);
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

            EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth - 35.0f));
            EditorUtils.PrintHeader("Registered Databases", EditorUtils.SUB_DETAIL_PANEL_COLOR, Color.white, 12, "oo_event");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal("Box");
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayout.Width(EditorGUIUtility.currentViewWidth - 35.0f), GUILayout.Height(240));
            string databaseToRemove = null;
            foreach (string databaseTarget in this.databases) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(8);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(2);
                if (!databaseTarget.ToUpper().Equals(GlobalResources.DEFAULT_DATABASE.ToUpper())) {
                    if (GUILayout.Button(Resources.Load("oo_delete") as Texture, GUIStyle.none, GUILayout.Width(14), GUILayout.Height(14))) {
                        if (databaseTarget.Equals(this.currentSelected)) {
                            this.registerError = "You can't delete current selected network database";
                        } else if (databaseTarget.Equals(GlobalResources.DEFAULT_DATABASE)) {
                            this.registerError = "You can't delete the DEFAULT network database";
                        } else  {
                            databaseToRemove = databaseTarget;    
                        }
                    }
                } else {
                    if (GUILayout.Button(Resources.Load("oo_lock") as Texture, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) {                        
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(databaseTarget, GUILayout.Width(220), GUILayout.Height(20));
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                EditorUtils.PrintImageButton("Select", 
                                             "oo_cache", 
                                             (databaseTarget.ToUpper().Equals(this.currentSelected.ToUpper())) ? Color.red.WithAlpha(0.15f) : EditorUtils.IMAGE_BUTTON_COLOR, 
                                             EditorUtils.IMAGE_BUTTON_FONT_COLOR, 12, 12f, 12f, () => {
                    // Update target and close
                    if ( this.currentManager != null ) {
                        this.currentManager.SetTargetDatabase(databaseTarget);
                        this.targetEditor.RefreshDatabase();
                        this.targetEditor.RefreshWindow();
                    } else if ( this.currentEventsManager != null ) {
                        this.currentEventsManager.SetTargetDatabase(databaseTarget);
                        this.targetEditor.RefreshDatabase();
                        this.targetEditor.RefreshWindow();
                    }
#if UNITY_EDITOR
                    if (this.currentManager != null) {
                        UnityEditor.EditorUtility.SetDirty(this.currentManager);
                    } else if ( this.currentEventsManager != null ) {
                        UnityEditor.EditorUtility.SetDirty(this.currentEventsManager);
                    }
#endif
                    AssetDatabase.SaveAssets();
		            AssetDatabase.Refresh();
                    Close();
                });

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            if ( databaseToRemove != null ) {
                // Delete database folder
                NetworkDatabaseWindow.DeleteNetworkDatabaseSource(databaseToRemove);
                // Refresh database array
                this.databases = GlobalResources.GetAvaiableDatabases(false);

            }
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(0.05f)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 30.0f));
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.Space(5);
            EditorUtils.PrintExplanationLabel("The database system is an optional feature that allows manually defined the database in the game.\n\nThis option allow users to create and test different database options.", "oo_info", Color.white, 5f);
            EditorGUILayout.Space(5);
            EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
            EditorGUILayout.Space(5);
            EditorUtils.PrintExplanationLabel("DO NOT CHANGE DATABASES AFTER GAME WAS STARTED, THIS MAY LEFT GAME OUT OF SYNCHRONIZED AND GENERATE INSTABILITY.", "oo_error", EditorUtils.EXPLANATION_FONT_COLOR, 5f);
            EditorGUILayout.Space(10);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth - 30.0f));
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

        /// <summary>
        /// Deletes the network database source.
        /// </summary>
        /// <param name="databasePath">The database path.</param>
        /// <exception cref="System.Exception">You can't delete the default database</exception>
        private static void DeleteNetworkDatabaseSource(string databasePath) {
            if ((GlobalResources.GetPrefabsDatabase(databasePath)           == null) ||
                (GlobalResources.GetPrefabsDatabase(databasePath).Length    == 0)) {
                return;
            } else if (GlobalResources.DEFAULT_DATABASE.ToUpper().Equals(databasePath.ToUpper())) {
                throw new System.Exception("You can't delete the default database");
            }
            UnityEngine.Object GlobalSource = Resources.Load(GlobalResources.GetPrefabsDatabase(databasePath));
            string databaseFolder   = ResourceUtils.ResourcesPath;
            string fullresFolder    = Application.dataPath + databaseFolder.Replace("Assets", "");
            if (System.IO.Directory.Exists(fullresFolder)) {
                string sourcePath = databaseFolder + "/" + GlobalResources.GetPrefabsDatabase(databasePath) + ".asset";
                string pathOfFile = Path.GetDirectoryName(sourcePath);
                if (System.IO.Directory.Exists(pathOfFile)) {
                    // Delete from from file system
                    System.IO.Directory.Delete(pathOfFile, true);
                    // Delete folder on unity
                    FileUtil.DeleteFileOrDirectory(pathOfFile);
                    AssetDatabase.DeleteAsset(pathOfFile);
                    AssetDatabase.MoveAssetToTrash(pathOfFile);
                    // Delete meta file
                    string metaFilePath = string.Format("{0}.meta", pathOfFile);
                    FileUtil.DeleteFileOrDirectory(metaFilePath);
                    AssetDatabase.DeleteAsset(metaFilePath);
                    // Refresh unity database
                    AssetDatabase.SaveAssets();
		            AssetDatabase.Refresh();
                }
            }            
	    }
    }

#endif
}