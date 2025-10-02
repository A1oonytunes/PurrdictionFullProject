#if STEAMWORKS_NET
using Steamworks;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
#else
using UnityEngine;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    /// <summary>
    /// Class NetworkSteamEditor.
    /// Implements the <see cref="Editor" />
    /// </summary>
    /// <seealso cref="Editor" />
    [CustomEditor(typeof(NetworkSteamManager))]
    [CanEditMultipleObjects]
    public class NetworkSteamManagerEditor : Editor {
        /// <summary>
        /// The network steam manager
        /// </summary>
        NetworkSteamManager networkSteamManager;

        SerializedProperty dontDestroyOnLoad;

        SerializedProperty debugLogger;

        SerializedProperty lobbyType;

        SerializedProperty lobbyDistance;

        SerializedProperty autoRefresh;
        
        SerializedProperty refreshRate;

        SerializedProperty maximumOfPlayers;

        SerializedProperty hostMigration;

        SerializedProperty dontMigrateDuringAutoLoadPause;

        SerializedProperty despawnDisconnectedServerPlayer;

        SerializedProperty despawnDisconnectedClientsAfterMigration;

        SerializedProperty reconnectionClientToolerance;

        SerializedProperty disconnectDetection;

        SerializedProperty steamEvents;

        /// <summary>
        /// The detail background opacity
        /// </summary>
        const float DETAIL_BACKGROUND_OPACITY = 0.05f;

        /// <summary>
        /// The transport background alpha
        /// </summary>
        const float TRANSPORT_BACKGROUND_ALPHA = 0.25f;

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public void OnEnable() {
            this.networkSteamManager                        = (this.target as NetworkSteamManager);
            // Get all serializable objects
            this.dontDestroyOnLoad                          = serializedObject.FindProperty("dontDestroyOnLoad");
            this.debugLogger                                = serializedObject.FindProperty("debugLogger");
            this.lobbyType                                  = serializedObject.FindProperty("lobbyType");
            this.lobbyDistance                              = serializedObject.FindProperty("lobbyDistance");
            this.autoRefresh                                = serializedObject.FindProperty("autoRefresh");
            this.refreshRate                                = serializedObject.FindProperty("refreshRate");
            this.maximumOfPlayers                           = serializedObject.FindProperty("maximumOfPlayers");
            this.hostMigration                              = serializedObject.FindProperty("hostMigration");
            this.dontMigrateDuringAutoLoadPause             = serializedObject.FindProperty("dontMigrateDuringAutoLoadPause");
            this.despawnDisconnectedServerPlayer            = serializedObject.FindProperty("despawnDisconnectedServerPlayer");
            this.despawnDisconnectedClientsAfterMigration   = serializedObject.FindProperty("despawnDisconnectedClientsAfterMigration");
            this.reconnectionClientToolerance               = serializedObject.FindProperty("reconnectionClientToolerance");
            this.disconnectDetection                        = serializedObject.FindProperty("disconnectDetection");
            this.steamEvents                                = serializedObject.FindProperty("steamEvents");            
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI() {
#if STEAMWORKS_NET
            serializedObject.Update();
            EditorUtils.PrintImage("objectnet_steam_logo", Color.blue, 0, 25);

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

            GUILayout.Space(5.0f);

            EditorUtils.PrintHeader("Steam Manager", Color.blue, Color.white, 16, "oo_steam", true, () => {
                if (this.dontDestroyOnLoad != null) {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(3.0f);
                    
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorUtils.PrintSimpleExplanation("Don't destroy");
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(3.0f);
                    EditorUtils.PrintBooleanSquaredByRef(ref this.dontDestroyOnLoad, "", null, 14, 12, false);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();


                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorUtils.PrintSimpleExplanation("Display logs on console");
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(3.0f);
                    EditorUtils.PrintBooleanSquaredByRef(ref this.debugLogger, "", null, 14, 12, false);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            });

            // SteamEventsEntry eventrEntry
            if (this.steamEvents.objectReferenceValue == null) {
                this.steamEvents.objectReferenceValue = new SteamEventsEntry();
            }
            this.DrawEvent((this.steamEvents.objectReferenceValue as SteamEventsEntry).GetOnHostLeaveLobby(), "onHostLeaveLobby");
            this.DrawEvent((this.steamEvents.objectReferenceValue as SteamEventsEntry).GetOnBecameHost(),     "onBecameHost");
            this.DrawEvent((this.steamEvents.objectReferenceValue as SteamEventsEntry).GetOnDetectNewHost(),  "onDetectNewHost");

            // Lobby type
            EditorGUILayout.BeginHorizontal();
            int selectedLobbyType = (int)this.DrawLobbyType((ELobbyType)this.lobbyType.intValue);
            if (selectedLobbyType != this.lobbyType.intValue) {
                this.lobbyType.intValue = selectedLobbyType;
            }
            EditorGUILayout.EndHorizontal();

            EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));

            // Lobby distance
            EditorGUILayout.BeginHorizontal();
            int selectedDistance = (int)this.DrawLobbyDistance((ELobbyDistanceFilter)this.lobbyDistance.intValue);
            if (selectedDistance != this.lobbyDistance.intValue) {
                this.lobbyDistance.intValue = selectedDistance;
            }
            EditorGUILayout.EndHorizontal();

            EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5.0f);
            int previousPlayers = this.maximumOfPlayers.intValue;
            int newPlayers = previousPlayers;
            GUILayout.FlexibleSpace();
            EditorUtils.DrawHorizontalIntBar(ref newPlayers, 2, 101, "Maximum of players", 500, true, " unlimited ");
            if (previousPlayers != newPlayers) {
                this.maximumOfPlayers.intValue = newPlayers;
            }
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
            EditorUtils.PrintExplanationLabel("The maximum of players accepted on each lobby", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(2.0f);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10.0f);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10.0f);
            EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));

            GUILayout.Space(5.0f);
            EditorUtils.PrintBooleanSquaredByRef(ref this.autoRefresh, "Auto Refresh Lobby", "", 16, 12);
            GUILayout.Space(2.0f);
            EditorUtils.PrintExplanationLabel("This option will refresh the lobby list according to the time defined on the refresh rate parameter", "oo_refresh", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
            GUILayout.Space(5.0f);
            EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
            GUILayout.Space(5.0f);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5.0f);
            GUILayout.FlexibleSpace();
            EditorUtils.DrawHorizontalIntBar(ref this.refreshRate, "Refresh Rate", 1, 50, EditorUtils.DEFAULT_SLIDER_FONT_SIZE, String.Format("{0} ms", this.refreshRate.intValue * 100));
            GUILayout.Space(5.0f);
            EditorUtils.PrintExplanationLabel("Time in milliseconds that manager will try to refresh the lobby list", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
            GUILayout.Space(2.0f);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10.0f);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10.0f);

            EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));

            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            GUILayout.Space(5.0f);
            EditorUtils.PrintBooleanSquaredByRef(ref this.hostMigration, "Enable Host Migration Support", "", 16, 12);
            GUILayout.Space(2.0f);
            EditorUtils.PrintExplanationLabel("This option will designate another player to be the host in case the current host player disconnects or leaves the match", "oo_peer_to_peer", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
            GUILayout.Space(5.0f);
            EditorGUILayout.EndVertical();
            if (this.hostMigration.boolValue) {
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));

                GUILayout.Space(10.0f);
                EditorUtils.DrawHorizontalIntBar(ref this.disconnectDetection, "Disconnect detection", 0, 50, EditorUtils.DEFAULT_SLIDER_FONT_SIZE, String.Format("{0} ms", this.disconnectDetection.intValue * 100));
                GUILayout.Space(5.0f);
                EditorUtils.PrintExplanationLabel("This is the amount of time that the system will try to detect when the player disconnect from the host.", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);

                GUILayout.Space(10.0f);
                EditorUtils.PrintBooleanSquaredByRef(ref this.despawnDisconnectedClientsAfterMigration, "Despawn disconnected player(s)", "", 16, 12);
                if (this.despawnDisconnectedClientsAfterMigration.boolValue) {
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("This option will disconnect the player after migration if the player does not reconnect to the new host after the configured time.", "oo_clock", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                    GUILayout.Space(5.0f);
                    GUILayout.Space(5.0f);
                    EditorUtils.DrawHorizontalIntBar(ref this.reconnectionClientToolerance, "Reconnection timeout", 1, 60, EditorUtils.DEFAULT_SLIDER_FONT_SIZE, String.Format("{0} sec", this.reconnectionClientToolerance.intValue));
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("Players who do not reconnect after this time will be automatically despawned.", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                    GUILayout.Space(5.0f);
                }

                EditorUtils.PrintBooleanSquaredByRef(ref this.despawnDisconnectedServerPlayer, "Direclty despawn disconnected host player", "", 16, 12);
                if (this.despawnDisconnectedServerPlayer.boolValue) {
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("If the current host disconnects or leaves the match, his player will be directly removed", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                    GUILayout.Space(5.0f);
                }

                EditorUtils.PrintBooleanSquaredByRef(ref this.dontMigrateDuringAutoLoadPause, "Disable migration during auto load pause", "", 16, 12);
                if (this.dontMigrateDuringAutoLoadPause.boolValue) {
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("This option will disable host migration if auto-load elements are paused, meaning that migration will not be executed in this case", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                    GUILayout.Space(5.0f);
                    EditorUtils.PrintExplanationLabel("This option is useful to avoid any event that may arise before a player is in the game", "oo_note", EditorUtils.EXPLANATION_FONT_COLOR);
                    GUILayout.Space(5.0f);
                }


            }

            GUILayout.Space(20.0f);
            serializedObject.ApplyModifiedProperties();
#else
            serializedObject.Update();
            EditorUtils.PrintImage("objectnet_steam_logo", Color.blue, 0, 25);

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

            GUILayout.Space(10.0f);

            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(0.10f)));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10.0f);
            EditorUtils.PrintExplanationLabel("Some dependencies are missing, you need to install SteamWorks packaged to use Steam integration", "oo_info");
            GUILayout.Space(10.0f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
#endif
        }

#if STEAMWORKS_NET
        /// <summary>
        /// Draws lobby types.
        /// </summary>
        /// <param name="selectedIndex">Index of the selected.</param>
        /// <returns>ELobbyType.</returns>
        private ELobbyType DrawLobbyType(ELobbyType selectedIndex) {
            ELobbyType result = ELobbyType.k_ELobbyTypeFriendsOnly;
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);

            EditorUtils.PrintSimpleExplanation("Lobby Access Level");

            GUILayout.Space(10.0f);

            int selectedLobbyTypeIndex = (int)selectedIndex;
            List<string> lobbyTypesName = new List<string>();
            lobbyTypesName.Add(ELobbyType.k_ELobbyTypePrivate.ToString().Replace("k_ELobbyType", ""));
            lobbyTypesName.Add(ELobbyType.k_ELobbyTypeFriendsOnly.ToString().Replace("k_ELobbyType", ""));
            lobbyTypesName.Add(ELobbyType.k_ELobbyTypePublic.ToString().Replace("k_ELobbyType", ""));
            lobbyTypesName.Add(ELobbyType.k_ELobbyTypeInvisible.ToString().Replace("k_ELobbyType", ""));
            lobbyTypesName.Add(ELobbyType.k_ELobbyTypePrivateUnique.ToString().Replace("k_ELobbyType", ""));

            int selectedMethod = EditorGUILayout.Popup(selectedLobbyTypeIndex, lobbyTypesName.ToArray<string>(), GUILayout.Width(200));
            if (selectedMethod == (int)ELobbyType.k_ELobbyTypePrivate) {
                result = ELobbyType.k_ELobbyTypePrivate;
            } else if (selectedMethod == (int)ELobbyType.k_ELobbyTypeFriendsOnly) {
                result = ELobbyType.k_ELobbyTypeFriendsOnly;
            } else if (selectedMethod == (int)ELobbyType.k_ELobbyTypePublic) {
                result = ELobbyType.k_ELobbyTypePublic;
            } else if (selectedMethod == (int)ELobbyType.k_ELobbyTypeInvisible) {
                result = ELobbyType.k_ELobbyTypeInvisible;
            } else if (selectedMethod == (int)ELobbyType.k_ELobbyTypePrivateUnique) {
                result = ELobbyType.k_ELobbyTypePrivateUnique;
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5.0f);
            EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
            GUILayout.Space(5.0f);
            EditorUtils.PrintExplanationLabel("Lobby Access Level when other players try to find your lobby to play on it", "oo_info", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
            GUILayout.Space(5.0f);
            EditorGUILayout.EndVertical();
            GUILayout.Space(5.0f);
            EditorGUILayout.EndHorizontal();
            
            return result;
        }

        /// <summary>
        /// Draws distances
        /// </summary>
        /// <param name="selectedIndex">Index of the selected.</param>
        /// <returns>ELobbyDistanceFilter.</returns>
        private ELobbyDistanceFilter DrawLobbyDistance(ELobbyDistanceFilter selectedIndex) {
            ELobbyDistanceFilter result = ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide;
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(DETAIL_BACKGROUND_OPACITY)));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);

            EditorUtils.PrintSimpleExplanation("Lobby Distance");

            GUILayout.Space(10.0f);

            int selectedLobbyDistanceIndex = (int)selectedIndex;
            List<string> lobbyDistanceNames = new List<string>();
            
            lobbyDistanceNames.Add(ELobbyDistanceFilter.k_ELobbyDistanceFilterClose.ToString().Replace("k_ELobbyDistance", ""));
            lobbyDistanceNames.Add(ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault.ToString().Replace("k_ELobbyDistance", ""));
            lobbyDistanceNames.Add(ELobbyDistanceFilter.k_ELobbyDistanceFilterFar.ToString().Replace("k_ELobbyDistance", ""));
            lobbyDistanceNames.Add(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide.ToString().Replace("k_ELobbyDistance", ""));

            int selectedMethod = EditorGUILayout.Popup(selectedLobbyDistanceIndex, lobbyDistanceNames.ToArray<string>(), GUILayout.Width(200));
            if (selectedMethod == (int)ELobbyDistanceFilter.k_ELobbyDistanceFilterClose) {
                result = ELobbyDistanceFilter.k_ELobbyDistanceFilterClose;
            } else if (selectedMethod == (int)ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault) {
                result = ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault;
            } else if (selectedMethod == (int)ELobbyDistanceFilter.k_ELobbyDistanceFilterFar) {
                result = ELobbyDistanceFilter.k_ELobbyDistanceFilterFar;
            } else if (selectedMethod == (int)ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide) {
                result = ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide;
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5.0f);
            EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
            GUILayout.Space(5.0f);
            EditorUtils.PrintExplanationLabel("Lobby Distance will filter the distance where you will find lobbies when a search is performed", "oo_network", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
            GUILayout.Space(5.0f);
            EditorGUILayout.EndVertical();
            GUILayout.Space(5.0f);
            EditorGUILayout.EndHorizontal();

            return result;
        }

        /// <summary>
        /// Draws the event.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="onDelete">The on delete.</param>
        private bool DrawEvent(SteamEventReference eventToDraw, string propertyName) {
            bool result = false;
            if (eventToDraw != null) {
                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.EVENT_BACKGROUND_COLOR.WithAlpha(0.10f)));
                EditorGUILayout.BeginVertical();

                bool isVisible = eventToDraw.IsEditorVisible();
                EditorGUILayout.BeginHorizontal();
                EventReferenceSide referenceType = this.GetReferenceSide(eventToDraw, propertyName);
                result = EditorUtils.PrintVisibilityBooleanWithIcon(ref isVisible, this.GetEventName(eventToDraw, propertyName), null, (EventReferenceSide.ServerSide.Equals(referenceType)) ? "oo_cloud" : (EventReferenceSide.ClientSide.Equals(referenceType)) ? "oo_workstation" : "oo_both_sides");

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
        private bool DrawEventEditor(SteamEventReference eventToDraw, Type returnType, Type[] parametersType) {
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
        private string GetEventName(SteamEventReference eventToDraw, string propertyName) {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var property = this.GetManagedType().GetField(propertyName, bindingFlags);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).EventName : null;
        }

        /// <summary>
        /// Gets the event descriptiom.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <returns>System.String.</returns>
        private string GetEventDescription(SteamEventReference eventToDraw, string propertyName) {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var property = this.GetManagedType().GetField(propertyName, bindingFlags);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).EventDescriptiom : null;
        }

        /// <summary>
        /// Gets the type of the return.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <returns>Type.</returns>
        private System.Type GetReturnType(SteamEventReference eventToDraw, string propertyName) {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var property = this.GetManagedType().GetField(propertyName, bindingFlags);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).ReturnType : null;
        }

        /// <summary>
        /// Gets the type of the parameters.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <returns>Type[].</returns>
        private System.Type[] GetParametersType(SteamEventReference eventToDraw, string propertyName) {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var property = this.GetManagedType().GetField(propertyName, bindingFlags);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).ParametersType : null;
        }

        /// <summary>
        /// Gets the reference side.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <returns>EventReferenceSide.</returns>
        private EventReferenceSide GetReferenceSide(SteamEventReference eventToDraw, string propertyName) {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var property = this.GetManagedType().GetField(propertyName, bindingFlags);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).ExecutionSide : EventReferenceSide.ServerSide;
        }

        /// <summary>
        /// Gets the type of the managed.
        /// </summary>
        /// <returns>Type.</returns>
        private System.Type GetManagedType() {
            return typeof(SteamEventsEntry);
        }
#endif
    }
#endif
}