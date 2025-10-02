#if UNITY_EDITOR
#endif

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    [CustomEditor(typeof(NetworkDiscoveryManager))]
    [CanEditMultipleObjects]
    public class NetworkDiscoveryManagerEditor : Editor {

        SerializedProperty IsServer;

        SerializedProperty IsClient;

        SerializedProperty AutoStart;

        SerializedProperty AutoStartDelay;

        SerializedProperty ShowLogs;

        SerializedProperty SimplePooling;

        SerializedProperty FullPooling;

        SerializedProperty OnServerDiscovered;

        SerializedProperty BroadcastInterval;

        SerializedProperty BroastCastPort;

        SerializedProperty avaiableClients;

        SerializedProperty avaiableServers;

        NetworkDiscoveryManager discoverManager;

        // <summary>
        /// The detail background opacity
        /// </summary>
        const float DETAIL_BACKGROUND_OPACITY = 0.05f;

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public void OnEnable() {
            this.discoverManager = (this.target as NetworkDiscoveryManager);
            // Get all serializable objects
            this.IsServer = serializedObject.FindProperty("IsServer");
            this.AutoStart = serializedObject.FindProperty("AutoStart");
            this.AutoStartDelay = serializedObject.FindProperty("AutoStartDelay");
            this.IsClient = serializedObject.FindProperty("IsClient");
            this.ShowLogs = serializedObject.FindProperty("ShowLogs");
            this.SimplePooling = serializedObject.FindProperty("SimplePooling");
            this.FullPooling = serializedObject.FindProperty("FullPooling");
            this.BroastCastPort = serializedObject.FindProperty("BroastCastPort");
            this.OnServerDiscovered = serializedObject.FindProperty("OnServerDiscovered");
            this.BroadcastInterval = serializedObject.FindProperty("BroadcastInterval");
            this.avaiableClients = serializedObject.FindProperty("avaiableClients");
            this.avaiableServers = serializedObject.FindProperty("avaiableServers");
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorUtils.PrintImage("objectnet_logo", Color.blue, 0, 25);

            GUILayout.Space(5.0f);

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
            EditorUtils.PrintBooleanSquaredByRef(ref this.ShowLogs, "Show logs", "oo_bug", 16, 12);

            GUILayout.Space(5.0f);
            EditorUtils.PrintExplanationLabel("When enabled, this option will display internal discover logs on unity console", "oo_info");
            GUILayout.Space(10.0f);

            EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
            EditorGUILayout.BeginHorizontal("box");
            if (Application.isPlaying) {
                EditorGUI.BeginDisabledGroup(true);
            }
            if ( EditorUtils.PrintBoolean(ref this.IsServer, "Server Discoverable", null, 16, 12) ) {
                this.IsClient.boolValue = !this.IsServer.boolValue;
            }
            GUILayout.Space(10.0f);
            if (EditorUtils.PrintBoolean(ref this.IsClient, "Client Discover", null, 16, 12)) {
                this.IsServer.boolValue = !this.IsClient.boolValue;
            }
            if (Application.isPlaying) {
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5.0f);
            if (this.IsServer.boolValue) {
                EditorUtils.PrintExplanationLabel("The server will listen to other clients on the network to answer if they ask", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);

                GUILayout.Space(8.0f);
                EditorUtils.DrawHorizontalIntBar(ref this.AutoStartDelay, "Broadcast interval (sec)", 2, 60, 12);
                GUILayout.Space(5.0f);
                EditorUtils.PrintExplanationLabel("Interval of time where server will send a broadcast message to be listen by network peers", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                GUILayout.Space(10.0f);

            } else if (this.IsClient.boolValue) {
                EditorUtils.PrintExplanationLabel("The client will send a broadcast over the network to check if there are any servers available", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                GUILayout.Space(5.0f);
                if (this.OnServerDiscovered.objectReferenceValue == null) {
                    this.OnServerDiscovered.objectReferenceValue = ScriptableObject.CreateInstance<EventReference>(); // new EventReference();
                }
                if (this.OnServerDiscovered.objectReferenceValue != null) { 
                    IEventEditor eventEditor = (Editor.CreateEditor(this.OnServerDiscovered.objectReferenceValue) as IEventEditor);
                    eventEditor.DrawInspector(this.GetReturnType(this.OnServerDiscovered), this.GetParametersType(this.OnServerDiscovered));
                } else {
                    EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                    GUILayout.Space(5.0f);
                }

                GUILayout.Space(5.0f);
                EditorUtils.PrintExplanationLabel("This method will be called when a new server was discovered, use to get discovered server address.", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                GUILayout.Space(5.0f);
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                GUILayout.Space(2.0f);
                EditorUtils.PrintExplanationLabel("The function parameter is the ip address of discovered server.", "oo_note", EditorUtils.EXPLANATION_FONT_COLOR);
                GUILayout.Space(2.0f);
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                GUILayout.Space(5.0f);
            }
            GUILayout.Space(5.0f);

            if (Application.isPlaying) {
                EditorGUI.BeginDisabledGroup(true);
            }
            EditorUtils.PrintBooleanSquaredByRef(ref this.AutoStart, "Auto Start", null, 16, 12);

            if (this.AutoStart.boolValue) {
                GUILayout.Space(8.0f);
                EditorUtils.DrawHorizontalIntBar(ref this.AutoStartDelay, "Start Delay (sec)", 1, 60, 12);
                GUILayout.Space(5.0f);
                EditorUtils.PrintExplanationLabel("Select how many seconds game will wait before start to discover updates", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                GUILayout.Space(10.0f);
            } else {
                GUILayout.Space(5.0f);
                EditorUtils.PrintExplanationLabel("Enable this option to automatically start the discovery update", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                GUILayout.Space(8.0f);
            }

            if (Application.isPlaying) {
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();


            EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
            EditorGUILayout.BeginHorizontal("box");
            if (Application.isPlaying) {
                EditorGUI.BeginDisabledGroup(true);
            }
            if (EditorUtils.PrintBoolean(ref this.SimplePooling, "Simple Pooling", null, 16, 12)) {
                this.FullPooling.boolValue = !this.SimplePooling.boolValue;
            }
            GUILayout.Space(10.0f);
            if (EditorUtils.PrintBoolean(ref this.FullPooling, "Full Pooling", null, 16, 12)) {
                this.SimplePooling.boolValue = !this.FullPooling.boolValue;
            }
            if (Application.isPlaying) {
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5.0f);
            if (this.SimplePooling.boolValue) {
                EditorUtils.PrintExplanationLabel("Simple pooling is faster and consume less resources from server\nBroadcast over [ 255.255.255.??? ]", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
            } else if (this.FullPooling.boolValue) {
                EditorUtils.PrintExplanationLabel("Full pooling will send many messages over the network and may consume more resources\nBroadcast over [ 255.255.???.??? ]", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
            }
            GUILayout.Space(10.0f);
            if (this.IsClient.boolValue) {
                GUILayout.Space(5.0f);
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                GUILayout.Space(5.0f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10.0f);
                EditorUtils.PrintSizedLabel((this.IsServer.boolValue) ? "Discovered Client's" : "Avaiable Servers", 12, EditorUtils.EXPLANATION_FONT_COLOR);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5.0f);
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                GUILayout.Space(10.0f);

                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.SUB_DETAIL_BACKGROUND_COLOR.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
                if ((this.IsClient.boolValue) && (this.avaiableServers.arraySize == 0)) {
                    GUILayout.Space(10.0f);
                    EditorUtils.PrintSizedLabel("There's no avaiable server's", 12, Color.white.WithAlpha(0.75f));
                    GUILayout.Space(10.0f);
                } else if (this.IsClient.boolValue) {
                    for (int clientIndex = 0; clientIndex < this.avaiableServers.arraySize; clientIndex++) {
                        string serverAddress = this.avaiableServers.GetArrayElementAtIndex(clientIndex).stringValue;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10.0f);
                        EditorGUILayout.BeginVertical();
                        GUILayout.Space(4.0f);
                        EditorUtils.PrintSizedLabel(serverAddress, 12, Color.white.WithAlpha(0.75f));
                        GUILayout.Space(4.0f);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                } /* else if ((this.IsServer.boolValue) && (this.avaiableClients.arraySize == 0)) {
                    GUILayout.Space(10.0f);
                    EditorUtils.PrintSizedLabel("There's no discovered client's", 12, Color.white.WithAlpha(0.75f));
                    GUILayout.Space(10.0f);
                } else if (this.IsServer.boolValue) {
                    for (int clientIndex = 0; clientIndex < this.avaiableClients.arraySize; clientIndex++) {
                        string clientAddress = this.avaiableClients.GetArrayElementAtIndex(clientIndex).stringValue;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10.0f);
                        EditorGUILayout.BeginVertical();
                        GUILayout.Space(4.0f);
                        EditorUtils.PrintSizedLabel(clientAddress, 12, Color.white.WithAlpha(0.75f));
                        GUILayout.Space(4.0f);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                }
                */
                EditorGUILayout.EndHorizontal();
            }
            

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Gets the type of the return.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <returns>Type.</returns>
        private Type GetReturnType(SerializedProperty eventToDraw) {
            var property = this.GetManagedType().GetField(eventToDraw.name);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).ReturnType : null;
        }

        /// <summary>
        /// Gets the type of the parameters.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <returns>Type[].</returns>
        private Type[] GetParametersType(SerializedProperty eventToDraw) {
            var property = this.GetManagedType().GetField(eventToDraw.name);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).ParametersType : null;
        }

        /// <summary>
        /// Gets the type of the managed.
        /// </summary>
        /// <returns>Type.</returns>
        public virtual Type GetManagedType() {
            return typeof(NetworkDiscoveryManager);
        }

    }
#endif
}
