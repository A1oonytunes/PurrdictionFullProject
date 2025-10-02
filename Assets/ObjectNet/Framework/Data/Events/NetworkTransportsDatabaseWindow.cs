#pragma warning disable
#pragma warning CS0618
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace com.onlineobject.objectnet {
#if UNITY_EDITOR
    /// <summary>
    /// This class draws a Network Transports Database Window
    /// Implements the <see cref="EditorWindow" />
    /// </summary>
    /// <seealso cref="EditorWindow" />
    public class NetworkTransportsDatabaseWindow : EditorWindow {
        /// <summary>
        /// The scroll position
        /// </summary>
        private Vector2 scrollPos;

        /// <summary>
        /// The transport to register
        /// </summary>
        private string transportToRegister;

        /// <summary>
        /// The server class to register
        /// </summary>
        private string serverClassToRegister;

        /// <summary>
        /// The client class to register
        /// </summary>
        private string clientClassToRegister;

        /// <summary>
        /// The transport database
        /// </summary>
        private NetworkTransportsDatabase transportDatabase;

        /// <summary>
        /// The register error
        /// </summary>
        private string registerError = null;

        /// <summary>
        ///  Store if unity transport system is activated
        /// </summary>
        private bool unityTransportActivated = false;

        /// <summary>
        ///  Store if unity transport system is installed
        /// </summary>
        private bool unityTransportInstalled = false;

        /// <summary>
        ///  Store if steam transport system is activated
        /// </summary>
        private bool steamTransportActivated = false;

        /// <summary>
        ///  Store if steam transport system is installed
        /// </summary>
        private bool steamTransportInstalled = false;

        /// <summary>
        ///  Store if healten package is installed
        /// </summary>
        private bool healtenPackageInstalled = false;

        /// <summary>
        /// The stored database on editor
        /// </summary>
        static NetworkTransportsDatabase storedDatabaseOnEditor = null;

        /// <summary>
        /// The minimum network event name
        /// </summary>
        const int MIN_NETWORK_EVENT_NAME = 8;

        /// <summary>
        /// The default Unity Transport assembly
        /// </summary>
        const string UNITY_TRANSPORT_ASSEMBLY= "Unity.Networking.Transport";

        /// <summary>
        /// The default Unity Transport namespace
        /// </summary>
        const string UNITY_TRANSPORT_NAMESPACE = "Unity.Networking.Transport.INetworkInterface";

        /// <summary>
        /// The default Unity Transport full namespace ( Class + Assembly )
        /// </summary>
        const string UNITY_TRANSPORT_FULL_NAMESPACE = UNITY_TRANSPORT_NAMESPACE + ", " + UNITY_TRANSPORT_ASSEMBLY;

        /// <summary>
        /// The default Steam Transport assembly
        /// </summary>
        const string STEAMWORKS_TRANSPORT_ASSEMBLY = "com.rlabrecque.steamworks.net";

        /// <summary>
        /// The default Steam Transport namespace
        /// </summary>
        const string STEAMWORKS_TRANSPORT_NAMESPACE = "Steamworks.SteamNetworkingSockets";

        /// <summary>
        /// The default Steam Transport full namespace ( Class + Assembly )
        /// </summary>
        const string STEAMWORKS_TRANSPORT_FULL_NAMESPACE = STEAMWORKS_TRANSPORT_NAMESPACE + ", " + STEAMWORKS_TRANSPORT_ASSEMBLY;

        /// <summary>
        /// The default Healten Steam Transport assembly
        /// </summary>
        const string HEALTEN_PACKAGE_ASSEMBLY = "Heathen.Steamworks";

        /// <summary>
        /// The default Healten Steam Transport namespace
        /// </summary>
        const string HEALTEN_PACKAGE_NAMESPACE = "HeathenEngineering.SteamworksIntegration.SteamSettings";

        /// <summary>
        /// The default Healten Steam Transport namespace ( 2025 support )
        /// </summary>
        const string HEALTEN_2025_PACKAGE_NAMESPACE = "Heathen.SteamworksIntegration.SteamSettings";

        /// <summary>
        /// The default Healten Steam Transport full namespace ( Class + Assembly )
        /// </summary>
        const string HEALTEN_PACKAGE_FULL_NAMESPACE = HEALTEN_PACKAGE_NAMESPACE + ", " + HEALTEN_PACKAGE_ASSEMBLY;

        /// <summary>
        /// The default Healten 2025 Steam Transport full namespace ( Class + Assembly )
        /// </summary>
        const string HEALTEN_2025_PACKAGE_FULL_NAMESPACE = HEALTEN_2025_PACKAGE_NAMESPACE + ", " + HEALTEN_PACKAGE_ASSEMBLY;

        /// <summary>
        /// Opens the network transports database window.
        /// </summary>
        public static void OpenNetworkTransportsDatabaseWindow() {
            NetworkTransportsDatabaseWindow window = ScriptableObject.CreateInstance(typeof(NetworkTransportsDatabaseWindow)) as NetworkTransportsDatabaseWindow;
            window.ShowUtility();
        }

        /// <summary>
        /// Opens the network database source.
        /// </summary>
        /// <param name="selectOnInspector">if set to <c>true</c> [select on inspector].</param>
        public static void OpenNetworkDatabaseSource(bool selectOnInspector = true) {
            NetworkTransportsDatabase assetObject = Resources.Load<NetworkTransportsDatabase>(GlobalResources.TRANSPORTS_DATABASE_PATH);
            if (assetObject == null) {
                CreateNetworkDatabaseSource();
                assetObject = Resources.Load<NetworkTransportsDatabase>(GlobalResources.TRANSPORTS_DATABASE_PATH);
                if (assetObject == null) {
                    string sourcePath = ResourceUtils.ResourcesPath + "/" + GlobalResources.TRANSPORTS_DATABASE_PATH + ".asset";
                    NetworkDebugger.Log(string.Format("Unable to find Network Transport Database at {0}.asset", sourcePath));
                }
            }
            if (selectOnInspector) {
                Selection.activeObject = assetObject;
            }
        }

        /// <summary>
        /// Creates the network database source.
        /// </summary>
        public static void CreateNetworkDatabaseSource() {
            if ((GlobalResources.TRANSPORTS_DATABASE_PATH == null) ||
                (GlobalResources.TRANSPORTS_DATABASE_PATH.Length == 0)) {
                return;
            }
            UnityEngine.Object GlobalSource = Resources.Load(GlobalResources.TRANSPORTS_DATABASE_PATH);
            NetworkTransportsDatabase sourceData = ScriptableObject.CreateInstance<NetworkTransportsDatabase>();
            string databaseFolder   = ResourceUtils.ResourcesPath;
            string fullresFolder    = Application.dataPath + databaseFolder.Replace("Assets", "");
            if (!System.IO.Directory.Exists(fullresFolder)) {
                System.IO.Directory.CreateDirectory(fullresFolder);
            }
            string sourcePath = databaseFolder + "/" + GlobalResources.TRANSPORTS_DATABASE_PATH + ".asset";            
            EditorUtility.SetDirty(sourceData);
            AssetDatabase.CreateAsset(sourceData, sourcePath);
            AssetDatabase.SaveAssets();
		    AssetDatabase.Refresh();
		}

        /// <summary>
        /// Gets the name of the active transport.
        /// </summary>
        /// <returns>System.String.</returns>
        public static string GetActiveTransportName() {
            string result = "Unloaded";
            if (NetworkTransportsDatabaseWindow.storedDatabaseOnEditor == null) {
                NetworkTransportsDatabaseWindow.storedDatabaseOnEditor  = Resources.Load<NetworkTransportsDatabase>(GlobalResources.TRANSPORTS_DATABASE_PATH);
            }
            if (NetworkTransportsDatabaseWindow.storedDatabaseOnEditor != null) {
                NetworkTransportEntry transport = NetworkTransportsDatabaseWindow.storedDatabaseOnEditor.GetActiveTransport();
                if (transport != null) {
                    result = transport.GetName();
                } else {
                    result = "Not Selected";
                    NetworkTransportsDatabaseWindow.storedDatabaseOnEditor.ActivateDefaultTransport();
                }
            }
            return result;
        }

        /// <summary>
        /// Determines whether [is double channel transport].
        /// </summary>
        /// <returns><c>true</c> if [is double channel transport]; otherwise, <c>false</c>.</returns>
        public static bool IsDoubleChannelTransport() {
            bool result = false;
            if (NetworkTransportsDatabaseWindow.storedDatabaseOnEditor == null) {
                NetworkTransportsDatabaseWindow.storedDatabaseOnEditor  = Resources.Load<NetworkTransportsDatabase>(GlobalResources.TRANSPORTS_DATABASE_PATH);
            }
            if (NetworkTransportsDatabaseWindow.storedDatabaseOnEditor != null) {
                Type transportClassType = Type.GetType(NetworkTransportsDatabaseWindow.storedDatabaseOnEditor.GetActiveTransport().GetServer());
                var attribute           = transportClassType.GetCustomAttributes(typeof(TransportType), true).FirstOrDefault() as TransportType;
                // Return if double channel is configured
                result = ((attribute != null) && ( attribute.IsDoubleChannel() ));
            }
            return result;
        }

        /// <summary>
        /// Determines whether [peer to peer is supported].
        /// </summary>
        /// <returns><c>true</c> if [peer to peer is supported]; otherwise, <c>false</c>.</returns>
        public static bool IsPeerToPeerSupported() {
            bool result = false;
            if (NetworkTransportsDatabaseWindow.storedDatabaseOnEditor == null) {
                NetworkTransportsDatabaseWindow.storedDatabaseOnEditor = Resources.Load<NetworkTransportsDatabase>(GlobalResources.TRANSPORTS_DATABASE_PATH);
            }
            if (NetworkTransportsDatabaseWindow.storedDatabaseOnEditor != null) {
                Type transportClassType = Type.GetType(NetworkTransportsDatabaseWindow.storedDatabaseOnEditor.GetActiveTransport().GetClient());
                var attribute = transportClassType.GetCustomAttributes(typeof(TransportType), true).FirstOrDefault() as TransportType;
                // Return if double channel is configured
                result = ((attribute == null) || (attribute.IsPeerToPeerSupported()));
            }
            return result;
        }

        /// <summary>
        /// Determines whether [connection delay is supported].
        /// </summary>
        /// <returns><c>true</c> if [cxonnection delay is supported]; otherwise, <c>false</c>.</returns>
        public static bool IsConnectionDelaySupported() {
            bool result = false;
            if (NetworkTransportsDatabaseWindow.storedDatabaseOnEditor == null) {
                NetworkTransportsDatabaseWindow.storedDatabaseOnEditor = Resources.Load<NetworkTransportsDatabase>(GlobalResources.TRANSPORTS_DATABASE_PATH);
            }
            if (NetworkTransportsDatabaseWindow.storedDatabaseOnEditor != null) {
                Type transportClassType = Type.GetType(NetworkTransportsDatabaseWindow.storedDatabaseOnEditor.GetActiveTransport().GetClient());
                var attribute = transportClassType.GetCustomAttributes(typeof(TransportType), true).FirstOrDefault() as TransportType;
                // Return if double channel is configured
                result = ((attribute != null) && (attribute.IsClientConnectionDelaySupported()));
            }
            return result;
        }

        /// <summary>
        /// Return the minimum time to wait before send connection stablished to the client
        /// </summary>
        /// <returns>Minimum connection delay value</returns>
        public static int GetMinimumConnectionDelay() {
            int result = 0;
            if (NetworkTransportsDatabaseWindow.storedDatabaseOnEditor == null) {
                NetworkTransportsDatabaseWindow.storedDatabaseOnEditor = Resources.Load<NetworkTransportsDatabase>(GlobalResources.TRANSPORTS_DATABASE_PATH);
            }
            if (NetworkTransportsDatabaseWindow.storedDatabaseOnEditor != null) {
                Type transportClassType = Type.GetType(NetworkTransportsDatabaseWindow.storedDatabaseOnEditor.GetActiveTransport().GetClient());
                var attribute = transportClassType.GetCustomAttributes(typeof(TransportType), true).FirstOrDefault() as TransportType;
                result = (attribute == null) ? 0 : attribute.ClientConnectionDelayInitialValue();
            }
            return result;
        }

        /// <summary>
        /// Return the minimum time to wait before send connection stablished to the client
        /// </summary>
        /// <returns>Maximum connection delay value</returns>
        public static int GetMaximumConnectionDelay() {
            int result = 0;
            if (NetworkTransportsDatabaseWindow.storedDatabaseOnEditor == null) {
                NetworkTransportsDatabaseWindow.storedDatabaseOnEditor = Resources.Load<NetworkTransportsDatabase>(GlobalResources.TRANSPORTS_DATABASE_PATH);
            }
            if (NetworkTransportsDatabaseWindow.storedDatabaseOnEditor != null) {
                Type transportClassType = Type.GetType(NetworkTransportsDatabaseWindow.storedDatabaseOnEditor.GetActiveTransport().GetClient());
                var attribute = transportClassType.GetCustomAttributes(typeof(TransportType), true).FirstOrDefault() as TransportType;
                result = (attribute == null) ? 0 : attribute.ClientConnectionDelayFinalValue();
            }
            return result;
        }

        /// <summary>
        /// Return the defaul time to wait before send connection stablished to the client
        /// </summary>
        /// <returns>Defaul connection delay value</returns>
        public static int GetDefaulConnectionDelay() {
            int result = 0;
            if (NetworkTransportsDatabaseWindow.storedDatabaseOnEditor == null) {
                NetworkTransportsDatabaseWindow.storedDatabaseOnEditor = Resources.Load<NetworkTransportsDatabase>(GlobalResources.TRANSPORTS_DATABASE_PATH);
            }
            if (NetworkTransportsDatabaseWindow.storedDatabaseOnEditor != null) {
                Type transportClassType = Type.GetType(NetworkTransportsDatabaseWindow.storedDatabaseOnEditor.GetActiveTransport().GetClient());
                var attribute = transportClassType.GetCustomAttributes(typeof(TransportType), true).FirstOrDefault() as TransportType;
                result = (attribute == null) ? 0 : attribute.ClientConnectionDelayDefaultValue();
            }
            return result;
        }

        /// <summary>
        /// Called when [enable].
        /// </summary>
        void OnEnable() {
            this.maxSize                    = new Vector2(850f, 700f);
            this.minSize                    = this.maxSize;
            this.titleContent               = new GUIContent ("Transports Database", Resources.Load("oo_info") as Texture);
            this.transportDatabase          = Resources.Load<NetworkTransportsDatabase>(GlobalResources.TRANSPORTS_DATABASE_PATH);
            // Check if unity transport is installed and activated
            this.unityTransportInstalled    = (Type.GetType(UNITY_TRANSPORT_FULL_NAMESPACE) != null);
#if UNITY_TRANSPORT_ENABLED
            this.unityTransportActivated    = true;            
#else
            this.unityTransportActivated    = false;
#endif

            // Check if unity transport is installed and activated
            this.steamTransportInstalled    = (Type.GetType(STEAMWORKS_TRANSPORT_FULL_NAMESPACE) != null);
#if STEAMWORKS_NET
            this.steamTransportActivated    = true;
#else
            this.steamTransportActivated    = false;
#endif

            // Check if unity transport is installed and activated
            this.healtenPackageInstalled    = (Type.GetType(HEALTEN_PACKAGE_FULL_NAMESPACE) != null);

            // Check new Healten version
            if (this.healtenPackageInstalled == false) {
                this.healtenPackageInstalled = (Type.GetType(HEALTEN_2025_PACKAGE_FULL_NAMESPACE) != null);
            }

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
            EditorUtils.PrintExplanationLabel("Fill requires fields and press \"Register\" to register a new transport system", "oo_event", Color.green.WithAlpha(0.5f));
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth - 35.0f));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Transport name");
            this.transportToRegister = EditorGUILayout.TextField(this.transportToRegister, GUILayout.Width(170));
            EditorGUILayout.EndVertical();
                
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Server class");
            this.serverClassToRegister = EditorGUILayout.TextField(this.serverClassToRegister, GUILayout.Width(500));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Client class");
            this.clientClassToRegister = EditorGUILayout.TextField(this.clientClassToRegister, GUILayout.Width(500));
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.Space(-15);
            EditorUtils.PrintImageButton("Register", "oo_event", EditorUtils.EVENT_BACKGROUND_COLOR.WithAlpha(0.50f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, 12, 12f, 12f, () => {
                bool registerTransport = true;
                if (!string.IsNullOrEmpty(this.transportToRegister)) {
                    if (!this.transportDatabase.TransportExists(this.transportToRegister)) {
                        if (this.transportToRegister.Trim().Length < MIN_NETWORK_EVENT_NAME) {
                            registerTransport = false;
                            this.registerError = string.Format("Network transport must have at least {0} characters", MIN_NETWORK_EVENT_NAME);
                        } else if (!System.Text.RegularExpressions.Regex.IsMatch(this.transportToRegister, @"^[a-zA-Z]+$")) {
                            registerTransport = false;
                            this.registerError = "Network transport should not have any special character, spaces or numbers";
                        } else if (string.IsNullOrEmpty(this.serverClassToRegister)) {
                            registerTransport = false;
                            this.registerError = "Server class must not be empty";
                        } else if (string.IsNullOrEmpty(this.clientClassToRegister)) {
                            registerTransport = false;
                            this.registerError = "Client class must not be empty";
                        } else if (Type.GetType(this.serverClassToRegister) == null) {
                            registerTransport = false;
                            this.registerError = "Server class could no be loaded, check if namespace and class is a valid class loaded on project";
                        } else if (Type.GetType(this.clientClassToRegister) == null) {
                            registerTransport = false;
                            this.registerError = "Client class could no be loaded, check if namespace and class is a valid class loaded on project";
                        } else if ((new List<Type>(Type.GetType(this.serverClassToRegister).GetInterfaces()).Contains(typeof(ITransportServer))) == false) {
                            registerTransport = false;
                            this.registerError = "Server class must implements \"ITransportServer\" interface";
                        } else if ((new List<Type>(Type.GetType(this.clientClassToRegister).GetInterfaces()).Contains(typeof(ITransportClient))) == false) {
                            registerTransport = false;
                            this.registerError = "Client class must implements \"ITransportClient\" interface";
                        }
                    } else {
                        registerTransport = false;
                        this.registerError = string.Format("Network transport \"{0}\" is already registered", this.transportToRegister);
                    }
                } else {
                    registerTransport = false;
                    this.registerError = "Network transport name must be filled";
                }
                if (registerTransport) {
                    this.transportDatabase.RegisterTransport(this.transportToRegister, this.serverClassToRegister, this.clientClassToRegister);
                    this.transportToRegister    = "";
                    this.serverClassToRegister  = "";
                    this.clientClassToRegister  = "";
                    this.registerError      = null;
                    EditorUtility.SetDirty(transportDatabase);
                    AssetDatabase.SaveAssets();
		            AssetDatabase.Refresh();
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
                EditorUtils.PrintExplanationLabel("Enter a unique transport name to create a new transport system", "oo_info", Color.yellow, 5f);
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
            EditorUtils.PrintHeader("Registered Transports", EditorUtils.SUB_DETAIL_PANEL_COLOR, Color.white, 12, "oo_event");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal("Box");
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayout.Width(EditorGUIUtility.currentViewWidth - 35.0f), GUILayout.Height(310));
            NetworkTransportEntry eventToRemove = null;
            if (this.transportDatabase != null) {
                int step = 0;
                foreach (NetworkTransportEntry eventEntry in this.transportDatabase.GetTransports()) {
                    EditorGUILayout.BeginHorizontal(((step++ % 2) == 0) ? BackgroundStyle.Get(Color.gray.WithAlpha(0.10f)) : BackgroundStyle.Get(Color.yellow.WithAlpha(0.10f)));
                    EditorGUILayout.Space(5);
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    if (eventEntry.IsAllowedToRemove()) {
                        if (GUILayout.Button(Resources.Load("oo_delete") as Texture, GUIStyle.none, GUILayout.Width(14), GUILayout.Height(14))) {
                            eventToRemove = eventEntry;
                        }
                    } else {
                        if (GUILayout.Button(Resources.Load("oo_lock") as Texture, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) {
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.TextField(eventEntry.GetCode().ToString(), GUILayout.Width(40), GUILayout.Height(20));
                    EditorGUILayout.TextField(eventEntry.GetName(), GUILayout.Width(200), GUILayout.Height(20));
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.Space(2);
                    bool isActive = eventEntry.IsActive();
                    if (eventEntry.GetName().ToUpper().Equals("UNITYTRANSPORT") && (!this.unityTransportInstalled || !this.unityTransportActivated)) {
                        if (EditorUtils.PrintBoolean(ref isActive, null, null, 18, 10, true)) {
                            EditorUtility.DisplayDialog("Unity Transport", "Unity transport system is not installed/activated, you need to install it to use Unity Transport on ObjectNet.\n\nSee ObjectNet manual for more informations.", "Understood");
                        }
                    } else if (eventEntry.GetName().ToUpper().Equals("STEAMTRANSPORT") && (!this.steamTransportInstalled || !this.steamTransportActivated)) {
                        if (EditorUtils.PrintBoolean(ref isActive, null, null, 18, 10, true)) {
                            EditorUtility.DisplayDialog("Steam Transport", "Steam transport system is not installed/activated, you need to install it to use Steam Transport on ObjectNet.\n\nSee ObjectNet manual for more informations.", "Understood");
                        }
                    } else if (eventEntry.GetName().ToUpper().Equals("STEAMTRANSPORT") && (!this.healtenPackageInstalled)) {
                        if (EditorUtils.PrintBoolean(ref isActive, null, null, 18, 10, true)) {
                            EditorUtility.DisplayDialog("Steam Transport", "Heathen Steam package is not installed, you need to install Heathen package to use SteamWorks library.\n\nHeathen free package is enough to make it work.", "Understood");
                        }
                    } else {
                        if (EditorUtils.PrintBoolean(ref isActive, null, null, 18, 10, true)) {
                            foreach (NetworkTransportEntry transportEntry in this.transportDatabase.GetTransports()) {
                                bool wasUpdated = (transportEntry.IsActive() != (transportEntry == eventEntry));
                                transportEntry.SetActive(transportEntry == eventEntry);
                                // Flag to save
                                if (wasUpdated) {
                                    UnityEditor.EditorUtility.SetDirty(this.transportDatabase);
                                    AssetDatabase.SaveAssets();
                                    AssetDatabase.Refresh();
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                    if (eventEntry.GetName().ToUpper().Equals("UNITYTRANSPORT") && (!this.unityTransportInstalled || !this.unityTransportActivated)) {
                        EditorGUILayout.Space(2);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        if ((this.unityTransportInstalled) && (!this.unityTransportActivated)) {
                            EditorGUILayout.Space(-5);
                            EditorUtils.PrintExplanationLabel("Unity Transport is not activated\nActivate it to use", "oo_info");
                        } else if (!this.unityTransportInstalled) {
                            EditorGUILayout.Space(-5);
                            EditorUtils.PrintExplanationLabel("Unity Transport is not installed\n Install to use it", "oo_error");
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.FlexibleSpace();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.Space(-7);
                        EditorUtils.PrintImageButton("Activate", "oo_install", Color.red.WithAlpha(0.25f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                            Type unityTransportDriverType = Type.GetType(UNITY_TRANSPORT_FULL_NAMESPACE);
                            if (unityTransportDriverType != null) {
                                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                                var buildGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
#pragma warning disable
                                string defineValues = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildGroup);
                                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildGroup, defineValues + ";UNITY_TRANSPORT_ENABLED");
#pragma warning restore
                            } else {
                                EditorUtility.DisplayDialog("Unity Transport", "Unity transport system is not installed, you need to install it to use Unity Transport on ObjectNet.\n\nSee ObjectNet manual for more informations.", "Understood");
                            }
                        });
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    } else if (eventEntry.GetName().ToUpper().Equals("STEAMTRANSPORT") && (!this.steamTransportInstalled || !this.steamTransportActivated || !this.healtenPackageInstalled)) {
                        EditorGUILayout.Space(2);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        if ((this.steamTransportInstalled) && (!this.steamTransportActivated)) {
                            EditorGUILayout.Space(-5);
                            EditorUtils.PrintExplanationLabel("Steam Transport is not activated\nActivate it to use", "oo_info");
                        } else if (!this.steamTransportInstalled) {
                            EditorGUILayout.Space(-5);
                            EditorUtils.PrintExplanationLabel("SteamWorks is not installed, you need to install SteamWorks to use Steam transport system", "oo_error");
                        } else if (!this.healtenPackageInstalled) {
                            EditorGUILayout.Space(-5);
                            EditorUtils.PrintExplanationLabel("Heathen Steam package is not installed, this package is required to use SteamWorks library ( Heathen free version is enough to make it work )", "oo_error");
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();                        
                    } else {
                        EditorGUI.BeginDisabledGroup(true);
                        GUILayout.FlexibleSpace();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.TextField(eventEntry.GetServer(), GUILayout.Width((eventEntry.GetName().ToUpper().Equals("UNITYTRANSPORT") ? 339 : 480)), GUILayout.Height(20));
                        EditorGUILayout.TextField(eventEntry.GetClient(), GUILayout.Width((eventEntry.GetName().ToUpper().Equals("UNITYTRANSPORT") ? 339 : 480)), GUILayout.Height(20));
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();

                        EditorGUI.EndDisabledGroup();
                        if (eventEntry.GetName().ToUpper().Equals("UNITYTRANSPORT")) {
                            // Create a button to disable unity transport
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.BeginVertical();
                            EditorUtils.PrintImageButton("Disable", "oo_delete", Color.red.WithAlpha(0.10f), EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                                var buildGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
#pragma warning disable CS0618
                                string defineValues = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildGroup).Replace("UNITY_TRANSPORT_ENABLED", "").Replace(";;", ";");
                                this.unityTransportActivated = false;
                                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildGroup, defineValues);
#pragma warning restore CS0618
                            });
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    if ((eventEntry.GetName().ToUpper().Equals("UNITYTRANSPORT")) && this.unityTransportInstalled && this.unityTransportActivated) {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.BeginVertical();
                        EditorUtils.PrintSimpleExplanation("Warning : Always disable Unity Transport before uninstall package to avoid compilation errors", EditorUtils.EXPLANATION_FONT_COLOR);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    } else if ((eventEntry.GetName().ToUpper().Equals("UNITYTRANSPORT")) && this.unityTransportInstalled && this.unityTransportActivated) {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.BeginVertical();
                        EditorUtils.PrintSimpleExplanation("Warning : Always disable SteamWorks before uninstall package to avoid compilation errors", EditorUtils.EXPLANATION_FONT_COLOR);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space(5);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(10);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space(5);
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            if ( eventToRemove != null ) {
                this.transportDatabase.UnregisterTransport(eventToRemove);
                EditorUtility.SetDirty(this.transportDatabase);
                AssetDatabase.SaveAssets();
		        AssetDatabase.Refresh();  
            }
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(0.05f)), GUILayout.Width(EditorGUIUtility.currentViewWidth - 30.0f));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(5);
            EditorUtils.PrintExplanationLabel("System provides some mebdded transport layers, nonetheless, you can create your own transport system or encode a wrapper to use some other standard transport system", "oo_info", Color.white, 5f);
            EditorGUILayout.Space(10);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();            
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth - 30.0f));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            EditorUtils.PrintImageButton("Close", "oo_close", EditorUtils.IMAGE_BUTTON_COLOR, EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                Close();
            });
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
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
#pragma warning restore