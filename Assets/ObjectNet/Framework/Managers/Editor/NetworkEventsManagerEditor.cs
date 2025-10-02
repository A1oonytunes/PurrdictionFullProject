using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    /// <summary>
    /// Class NetworkEventsManagerEditor.
    /// Implements the <see cref="Editor" />
    /// Implements the <see cref="com.onlineobject.objectnet.IDatabaseTargetEditor" />
    /// Implements the <see cref="com.onlineobject.objectnet.INetworkEventEditor{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Editor" />
    /// <seealso cref="com.onlineobject.objectnet.IDatabaseTargetEditor" />
    /// <seealso cref="com.onlineobject.objectnet.INetworkEventEditor{T}" />
    [CustomEditor(typeof(NetworkEventsManager))]
    [CanEditMultipleObjects]
    public abstract class NetworkEventsManagerEditor<T> : Editor, IDatabaseTargetEditor, INetworkEventEditor<T> where T : NetworkEventsManager {

        /// <summary>
        /// Flag if this object will not be destroyed when scene changes
        /// </summary>
        SerializedProperty dontDestroyOnLoad;

        /// <summary>
        /// The internal events hidden
        /// </summary>
        SerializedProperty InternalEventsHidden;

        /// <summary>
        /// The listeners events hidden
        /// </summary>
        SerializedProperty ListenersEventsHidden;

        /// <summary>
        /// The actions events hidden
        /// </summary>
        SerializedProperty ActionsEventsHidden;

        /// <summary>
        /// The events managed
        /// </summary>
        List<SerializedProperty> eventsManaged = new List<SerializedProperty>();

        /// <summary>
        /// The listener events
        /// </summary>
        SerializedProperty ListenerEvents;

        /// <summary>
        /// The actions events
        /// </summary>
        SerializedProperty ActionsEvents;

        /// <summary>
        /// The database target
        /// </summary>
        SerializedProperty databaseTarget;

        /// <summary>
        /// The new registered event name
        /// </summary>
        string newRegisteredEventName = null;

        /// <summary>
        /// The new registered actions name
        /// </summary>
        string newRegisteredActionsName = null;

        /// <summary>
        /// The register event error
        /// </summary>
        string registerEventError = "";

        /// <summary>
        /// The register action error
        /// </summary>
        string registerActionError = "";

        /// <summary>
        /// The event database
        /// </summary>
        NetworkEventsDatabase eventDatabase;

        /// <summary>
        /// The events manager
        /// </summary>
        NetworkEventsManager eventsManager;

        /// <summary>
        /// The constructor of types
        /// </summary>
        Dictionary<Type, Func<UnityEngine.Object>> constructorOfTypes = new Dictionary<Type, Func<UnityEngine.Object>>();

        /// <summary>
        /// The minimum network event name
        /// </summary>
        const int MIN_NETWORK_EVENT_NAME = 8;

        /// <summary>
        /// The detail background opacity
        /// </summary>
        const float DETAIL_BACKGROUND_OPACITY = 0.05f;

        /// <summary>
        /// The transport background alpha
        /// </summary>
        const float TRANSPORT_BACKGROUND_ALPHA = 0.25f;

        /// <summary>
        /// Gets the manager.
        /// </summary>
        /// <returns>T.</returns>
        public virtual T GetManager() {
            return (T)this.eventsManager;
        }

        /// <summary>
        /// Sets the manager.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public virtual void SetManager(T manager) {
            this.eventsManager = manager;
        }

        /// <summary>
        /// Gets the type of the managed.
        /// </summary>
        /// <returns>Type.</returns>
        public virtual Type GetManagedType() {
            return typeof(NetworkEventsManager);
        }

        /// <summary>
        /// Instantiates the type.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <returns>UnityEngine.Object.</returns>
        public virtual UnityEngine.Object InstantiateType(Type eventType) {
            return this.constructorOfTypes[eventType].Invoke();
        }

        /// <summary>
        /// Registers the type of the constructor of.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="method">The method.</param>
        public virtual void RegisterConstructorOfType(Type type, Func<UnityEngine.Object> method) {
            this.constructorOfTypes.Add(type, method);
        }

        /// <summary>
        /// Refreshes the database.
        /// </summary>
        public void RefreshDatabase() {
            serializedObject.Update();
            this.eventDatabase = Resources.Load<NetworkEventsDatabase>(GlobalResources.GetEventsDatabase(this.databaseTarget.stringValue));
            if (this.eventDatabase == null) {
                NetworkEventsDatabaseWindow.CreateNetworkDatabaseSource(this.databaseTarget.stringValue);
                this.eventDatabase = Resources.Load<NetworkEventsDatabase>(GlobalResources.GetEventsDatabase(this.databaseTarget.stringValue));
            }
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Refreshes the window.
        /// </summary>
        public void RefreshWindow() {
            this.OnInspectorGUI();
        }

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public virtual void OnEnable() {
            this.eventsManager          = (this.target as NetworkEventsManager);
            this.dontDestroyOnLoad      = serializedObject.FindProperty("dontDestroyOnLoad");
            this.InternalEventsHidden   = serializedObject.FindProperty("InternalEventsHidden");
            this.ListenersEventsHidden  = serializedObject.FindProperty("ListenersEventsHidden");
            this.ActionsEventsHidden    = serializedObject.FindProperty("ActionsEventsHidden");
            this.ListenerEvents         = serializedObject.FindProperty("ListenerEvents");
            this.ActionsEvents          = serializedObject.FindProperty("ActionsEvents");
            this.databaseTarget         = serializedObject.FindProperty("databaseTarget");
            // Initialize database
            this.RefreshDatabase();

            this.eventsManaged.Clear();
            foreach (FieldInfo fieldEvent in this.GetManagedType()
                                                 .GetFields(BindingFlags.NonPublic | 
                                                            BindingFlags.Instance | 
                                                            BindingFlags.Public).Where(fi => fi.FieldType == typeof(EventReference))) {
                this.eventsManaged.Add(serializedObject.FindProperty(fieldEvent.Name));
            }
            // Register contructor of each reference
            // Note: I need to do this because unity has some problems when trying to use inheritance with an editor ( god knows why )
            this.constructorOfTypes.Add(typeof(EventReference), () => { return ScriptableObject.CreateInstance<EventReference>(); }); // new EventReference(); });            
            this.constructorOfTypes.Add(typeof(EventActionReference), () => { return ScriptableObject.CreateInstance<EventActionReference>();  }); // new EventActionReference(); });
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorUtils.PrintImage("objectnet_logo", Color.blue, 0, 35);            
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
            EditorUtils.PrintImageButton("Tutorial", "oo_youtube", EditorUtils.IMAGE_BUTTON_COLOR, EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                Help.BrowseURL("https://www.youtube.com/@TheObjectNet");
            });
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5.0f);

            EditorUtils.PrintImageButton("Events Editor", "oo_colored_bell", EditorUtils.IMAGE_BUTTON_COLOR, EditorUtils.IMAGE_BUTTON_FONT_COLOR, () => {
                NetworkEventsDatabaseWindow.OpenNetworkEventsDatabaseWindow(this.databaseTarget.stringValue);
            });

            GUILayout.Space(5.0f);

            EditorUtils.PrintHeader("Network Events Manager", Color.blue, Color.white, 16, "oo_event", true, () => {
                if (this.dontDestroyOnLoad != null) {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(3.0f);
                    EditorGUILayout.BeginHorizontal();
                    EditorUtils.PrintSimpleExplanation("Don't destroy");
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(3.0f);
                    EditorUtils.PrintBooleanSquaredByRef(ref this.dontDestroyOnLoad, "", null, 14, 12, false);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            });

            GUILayout.Space(5.0f);

            // Target Database
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.SIMPLE_DANGER_FONT_COLOR.WithAlpha(TRANSPORT_BACKGROUND_ALPHA)));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5.0f);

            EditorGUILayout.BeginHorizontal();
            EditorUtils.PrintExplanationLabel(String.Format("Selected Database \"{0}\"", this.databaseTarget.stringValue.ToUpper()),
                                              "oo_cache",
                                              EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR,
                                              5,
                                              13,
                                              0,
                                              false);
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(-5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5);
            EditorUtils.PrintSimpleExplanation("Click to modify ", Color.white, 13, false);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(2);
            if (GUILayout.Button(Resources.Load("oo_eye_clickable") as Texture, GUIStyle.none, GUILayout.Width(24), GUILayout.Height(24))) {
                NetworkDatabaseWindow.OpenNetworkDatabaseWindow(this.eventsManager, this, this.databaseTarget.stringValue);
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(10.0f);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            // Target Database - END

            GUILayout.Space(10.0f);

            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(0.05f)));
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintVisibilityBoolean(ref this.InternalEventsHidden, "Global Events");
            if (!this.InternalEventsHidden.boolValue) {
                GUILayout.Space(5.0f);
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginVertical();            
            if (!this.InternalEventsHidden.boolValue) {
                foreach (SerializedProperty serializedEvent in this.eventsManaged) {
                    this.DrawEvent(serializedEvent, typeof(EventReference));
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            this.DrawListenerEventsArea();
            this.DrawActionsEventsArea();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the listener events area.
        /// </summary>
        private void DrawListenerEventsArea() {
            GUILayout.Space(10.0f);

            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(0.05f)));
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintVisibilityBooleanWithIcon(ref this.ListenersEventsHidden, String.Format("User Events [ {0} event(s) ]", this.ListenerEvents.arraySize ), null, "oo_event");
            if ((!this.ListenersEventsHidden.boolValue) &&
                (this.ListenerEvents.arraySize > 0)) {
                GUILayout.Space(5.0f);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15.0f);
            if (GUILayout.Button(Resources.Load("oo_add") as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18))) {
                bool registerEvent = true;
                if ( !string.IsNullOrEmpty(this.newRegisteredEventName) ) {
                    if (this.eventAlreadyRegistered(this.newRegisteredEventName)) {
                        registerEvent = false;
                        this.registerEventError = String.Format("Network event \"{0}\" is already registered", this.newRegisteredEventName);
                    } else if (!this.eventDatabase.EventExists(this.newRegisteredEventName)) { 
                        registerEvent = false;
                        this.registerEventError = String.Format("Network event \"{0}\" not exists", this.newRegisteredEventName);
                    }
                } else {
                    registerEvent = false;
                    this.registerEventError = "Select event before continue, if you haven't any use events editor window to create";
                }
                if ( registerEvent ) {
                    EventListenerReference newEvent = new EventListenerReference();
                    newEvent.SetEventCode(this.eventDatabase.GetEventCode(this.newRegisteredEventName));
                    newEvent.SetEventName(this.newRegisteredEventName);
                    newEvent.SetEditorVisible(true);
                    this.ListenerEvents.InsertArrayElementAtIndex(this.ListenerEvents.arraySize);
                    this.ListenerEvents.GetArrayElementAtIndex(this.ListenerEvents.arraySize - 1).objectReferenceValue = newEvent;
                    // Force to make events visible
                    this.ListenersEventsHidden.boolValue = false;
                    // Clean event to not be repeated
                    this.newRegisteredEventName = null;
                    this.registerEventError     = null;
                }                
            }                
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 14;
            GUILayout.Label("Register event to listen");
            GUI.skin.label.fontSize = previousSize;

            string[] registeredEvents = new string[this.ListenerEvents.arraySize];
            for (int eventIndex = 0; eventIndex < this.ListenerEvents.arraySize; eventIndex++) {
                EventListenerReference eventListener    = (this.ListenerEvents.GetArrayElementAtIndex(eventIndex).objectReferenceValue as EventListenerReference);
                registeredEvents[eventIndex]            = (eventListener != null) ? eventListener.GetEventName() : "";
            }
            
            string[] avaiableEvents             = this.eventDatabase.GetRegisteredEventsName(registeredEvents);
            int selectedEventToRegisterIndex    = Array.IndexOf(avaiableEvents, this.newRegisteredEventName);
            int selectedEventToRegister         = EditorGUILayout.Popup(selectedEventToRegisterIndex, avaiableEvents, GUILayout.Width(250));
            this.newRegisteredEventName         = (selectedEventToRegister > -1) ? avaiableEvents[selectedEventToRegister] : null;

            EditorGUILayout.EndHorizontal();
            if ( !string.IsNullOrEmpty(this.registerEventError) ) {
                GUILayout.Space(10.0f);
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                GUILayout.Space(5.0f);
                EditorUtils.PrintExplanationLabel(this.registerEventError, "oo_error", EditorUtils.EXPLANATION_FONT_COLOR);
            }
            GUILayout.Space(10.0f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginVertical();            
            if (!this.ListenersEventsHidden.boolValue) {
                int remoteElementIndex = -1;
                for (int eventIndex = 0; eventIndex < this.ListenerEvents.arraySize; eventIndex++) { 
                    GUILayout.Space(5.0f);
                    SerializedProperty eventListener = this.ListenerEvents.GetArrayElementAtIndex(eventIndex);
                    if (eventListener.objectReferenceValue != null) {
                        this.DrawEvent(eventListener, typeof(EventListenerReference),
                            (eventListener.objectReferenceValue as EventListenerReference).GetEventName(),
                            () => { remoteElementIndex = eventIndex; });
                    }
                }
                if ( remoteElementIndex > -1 ) {
                    this.ListenerEvents.DeleteArrayElementAtIndex(remoteElementIndex);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the actions events area.
        /// </summary>
        private void DrawActionsEventsArea() {
            GUILayout.Space(10.0f);

            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.red.WithAlpha(0.05f)));
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintVisibilityBooleanWithIcon(ref this.ActionsEventsHidden, String.Format("User Actions [ {0} action(s) ]", this.ActionsEvents.arraySize ), null, "oo_action");
            if ((!this.ActionsEventsHidden.boolValue) &&
                (this.ActionsEvents.arraySize > 0)) {
                GUILayout.Space(5.0f);                
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15.0f);
            if (GUILayout.Button(Resources.Load("oo_add") as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18))) {
                bool registerAction = true;
                if ( !string.IsNullOrEmpty(this.newRegisteredActionsName) ) {
                    if (this.eventAlreadyRegistered(this.newRegisteredActionsName)) {
                        registerAction = false;
                        this.registerActionError = String.Format("Network action [ {0} ] is already registered", this.newRegisteredActionsName);
                    } else if (this.newRegisteredActionsName.Trim().Length < MIN_NETWORK_EVENT_NAME) { 
                        registerAction = false;
                        this.registerActionError = String.Format("Network action must have at least {0} characters", MIN_NETWORK_EVENT_NAME);
                    } else if (!Regex.IsMatch(this.newRegisteredActionsName, @"^[a-zA-Z]+$")) { 
                        registerAction = false;
                        this.registerActionError = "Network action should not have any special character, spaces or numbers";
                    }
                } else {
                    registerAction = false;
                    this.registerActionError = "Network action must be filled with action name";
                }
                if ( registerAction ) {
                    EventActionReference newAction = new EventActionReference();
                    newAction.SetEventManager(this.eventsManager);
                    newAction.SetEventName(this.newRegisteredActionsName);
                    newAction.SetEditorVisible(true);
                    this.ActionsEvents.InsertArrayElementAtIndex(this.ActionsEvents.arraySize);
                    this.ActionsEvents.GetArrayElementAtIndex(this.ActionsEvents.arraySize - 1).objectReferenceValue = newAction;
                    // Force to make events visible
                    this.ActionsEventsHidden.boolValue = false;
                    // Clean event to not be repeated
                    this.newRegisteredActionsName   = null;
                    this.registerActionError         = null;
                }                
            }                
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 14;
            GUILayout.Label("Register Action");
            GUI.skin.label.fontSize = previousSize;
            this.newRegisteredActionsName = EditorGUILayout.TextField(this.newRegisteredActionsName, GUILayout.Width(240));
            EditorGUILayout.EndHorizontal();
            if ( !string.IsNullOrEmpty(this.registerActionError) ) {
                GUILayout.Space(10.0f);
                EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                GUILayout.Space(5.0f);
                EditorUtils.PrintExplanationLabel(this.registerActionError, "oo_error", EditorUtils.EXPLANATION_FONT_COLOR);
            }
            GUILayout.Space(10.0f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginVertical();            
            if (!this.ActionsEventsHidden.boolValue) {
                int remoteElementIndex = -1;
                for (int eventIndex = 0; eventIndex < this.ActionsEvents.arraySize; eventIndex++) { 
                    GUILayout.Space(5.0f);
                    SerializedProperty eventAction = this.ActionsEvents.GetArrayElementAtIndex(eventIndex);
                    this.DrawEvent(eventAction, typeof(EventActionReference), (eventAction.objectReferenceValue as EventActionReference).GetEventName(), () => { 
                        remoteElementIndex = eventIndex;
                    });
                }
                if ( remoteElementIndex > -1 ) {
                    this.ActionsEvents.DeleteArrayElementAtIndex(remoteElementIndex);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the event.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="onDelete">The on delete.</param>
        public void DrawEvent(SerializedProperty eventToDraw, Type eventType, string eventName = null, Action onDelete = null) {
            if ( eventToDraw.objectReferenceValue == null ) {
                eventToDraw.objectReferenceValue = this.InstantiateType(eventType);
            }
            if (eventToDraw.objectReferenceValue != null) {
                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.EVENT_BACKGROUND_COLOR.WithAlpha(0.10f)));
                EditorGUILayout.BeginVertical();

                bool isVisible  = (eventToDraw.objectReferenceValue as EventReference).IsEditorVisible();
                bool isFilled   = ((eventToDraw.objectReferenceValue as EventReference).GetEventTarget() != null);
                EditorGUILayout.BeginHorizontal();
                if (onDelete == null) {
                    EventReferenceSide referenceType = this.GetReferenceSide(eventToDraw);
                    EditorUtils.PrintVisibilityBooleanWithIcon(ref isVisible, isFilled, (eventName == null) ? this.GetEventName(eventToDraw) : eventName, null, (EventReferenceSide.ServerSide.Equals(referenceType)) ? "oo_cloud" : (EventReferenceSide.ClientSide.Equals(referenceType)) ? "oo_workstation" : "oo_both_sides");
                } else {
                    EditorUtils.PrintDeletableBoolean(ref isVisible, (eventName == null) ? this.GetEventName(eventToDraw) : eventName, 18, 14, true, onDelete);
                }
                EditorGUILayout.EndHorizontal();
                
                (eventToDraw.objectReferenceValue as EventReference).SetEditorVisible(isVisible);

                if (isVisible) {
                    IEventEditor eventEditor = (Editor.CreateEditor(eventToDraw.objectReferenceValue) as IEventEditor);
                    eventEditor.DrawInspector((eventEditor.HasOverridenEventTypes()) ? eventEditor.GetReturnType()      : this.GetReturnType(eventToDraw),
                                              (eventEditor.HasOverridenEventTypes()) ? eventEditor.GetParametersType()  : this.GetParametersType(eventToDraw));

                    string eventDescription = this.GetEventDescriptiom(eventToDraw);
                    if (!string.IsNullOrEmpty(eventDescription)) {
                        GUILayout.Space(10.0f);
                        EditorUtils.HorizontalLine(EditorUtils.LINE_DIVISOR_COLOR, 1.0f, new Vector2(5f, 5f));
                        GUILayout.Space(5.0f);
                        EditorUtils.PrintExplanationLabel(eventDescription, "oo_info");
                        // Draw event parameters
                        Type[] parametersType = (eventEditor.HasOverridenEventTypes()) ? eventEditor.GetParametersType() : this.GetParametersType(eventToDraw);
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
                        Type returnType = (eventEditor.HasOverridenEventTypes()) ? eventEditor.GetReturnType() : this.GetReturnType(eventToDraw);
                        if (returnType != null) {
                            EditorUtils.PrintSimpleExplanation("Return Type");
                            EditorUtils.PrintExplanationLabel(string.Format("{0}", returnType.Name), "oo_variables_inactive", EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
                        }
                        // Create the example function
                        string resultTypeStr = (returnType != null) ? returnType.Name : "void";
                        string parametersTypeStr = " ";
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
                                                                        this.GetEventName(eventToDraw),
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
        }

        /// <summary>
        /// Events the already registered.
        /// </summary>
        /// <param name="eventNameToCheck">The event name to check.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool eventAlreadyRegistered(string eventNameToCheck) {
            bool result = false;
            for (int eventIndex = 0; eventIndex < this.ListenerEvents.arraySize; eventIndex++) { 
                SerializedProperty eventListener = this.ListenerEvents.GetArrayElementAtIndex(eventIndex);
                result |= ((eventListener.objectReferenceValue as EventListenerReference).GetEventName().ToUpper().Equals(eventNameToCheck.ToUpper()));
            }
            return result;
        }

        /// <summary>
        /// Gets the name of the event.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <returns>System.String.</returns>
        private string GetEventName(SerializedProperty eventToDraw) {
            var property = this.GetManagedType().GetField(eventToDraw.name);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).EventName : null;
        }

        /// <summary>
        /// Gets the event descriptiom.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <returns>System.String.</returns>
        private string GetEventDescriptiom(SerializedProperty eventToDraw) {
            var property = this.GetManagedType().GetField(eventToDraw.name);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).EventDescriptiom : null;
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
        /// Gets the reference side.
        /// </summary>
        /// <param name="eventToDraw">The event to draw.</param>
        /// <returns>EventReferenceSide.</returns>
        private EventReferenceSide GetReferenceSide(SerializedProperty eventToDraw) {
            var property = this.GetManagedType().GetField(eventToDraw.name);
            return ((property != null) && (property.GetCustomAttributes(typeof(EventInformations), false).Count() > 0)) ? (property.GetCustomAttributes(typeof(EventInformations), false).First() as EventInformations).ExecutionSide : EventReferenceSide.ServerSide;
        }

    }
#endif
}
