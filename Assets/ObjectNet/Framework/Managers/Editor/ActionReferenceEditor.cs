using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    /// <summary>
    /// Class ActionReferenceEditor.
    /// Implements the <see cref="com.onlineobject.objectnet.EventReferenceEditor" />
    /// Implements the <see cref="com.onlineobject.objectnet.IEventEditor" />
    /// Implements the <see cref="com.onlineobject.objectnet.IDatabaseTargetEditor" />
    /// </summary>
    /// <seealso cref="com.onlineobject.objectnet.EventReferenceEditor" />
    /// <seealso cref="com.onlineobject.objectnet.IEventEditor" />
    /// <seealso cref="com.onlineobject.objectnet.IDatabaseTargetEditor" />
    [CustomEditor(typeof(EventActionReference))]
    [CanEditMultipleObjects]
    public class ActionReferenceEditor : EventReferenceEditor, IEventEditor, IDatabaseTargetEditor {

        /// <summary>
        /// The event database target
        /// </summary>
        SerializedProperty EventDatabaseTarget;

        /// <summary>
        /// The event manager
        /// </summary>
        SerializedProperty EventManager;

        /// <summary>
        /// The event name
        /// </summary>
        SerializedProperty EventName;

        /// <summary>
        /// The execution mode
        /// </summary>
        SerializedProperty ExecutionMode;

        /// <summary>
        /// The event code to send
        /// </summary>
        SerializedProperty EventCodeToSend;

        /// <summary>
        /// The event to send
        /// </summary>
        SerializedProperty EventToSend;

        /// <summary>
        /// The trigger target
        /// </summary>
        SerializedProperty TriggerTarget;

        /// <summary>
        /// The trigger component
        /// </summary>
        SerializedProperty TriggerComponent;

        /// <summary>
        /// The trigger expressions
        /// </summary>
        SerializedProperty TriggerExpressions;

        /// <summary>
        /// The attribute expressions
        /// </summary>
        SerializedProperty AttributeExpressions;

        /// <summary>
        /// The target detection error
        /// </summary>
        SerializedProperty TargetDetectionError;

        /// <summary>
        /// The target detection error timeout
        /// </summary>
        SerializedProperty TargetDetectionErrorTimeout;

        /// <summary>
        /// The event database
        /// </summary>
        NetworkEventsDatabase eventDatabase;

        /// <summary>
        /// The delay to dismiss error
        /// </summary>
        const float DELAY_TO_DISMISS_ERROR = 3f;

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public override void OnEnable() {
            base.OnEnable();
            this.ExecutionMode                  = serializedObject.FindProperty("ExecutionMode");
            this.EventCodeToSend                = serializedObject.FindProperty("EventCodeToSend");
            this.EventToSend                    = serializedObject.FindProperty("EventToSend");
            this.EventName                      = serializedObject.FindProperty("EventName");
            this.TriggerTarget                  = serializedObject.FindProperty("TriggerTarget");
            this.TriggerComponent               = serializedObject.FindProperty("TriggerComponent");
            this.TriggerExpressions             = serializedObject.FindProperty("TriggerExpressions");
            this.AttributeExpressions           = serializedObject.FindProperty("AttributeExpressions");
            this.TargetDetectionError           = serializedObject.FindProperty("TargetDetectionError");
            this.TargetDetectionErrorTimeout    = serializedObject.FindProperty("TargetDetectionErrorTimeout");
            this.EventDatabaseTarget            = serializedObject.FindProperty("EventDatabaseTarget");
            this.EventManager                   = serializedObject.FindProperty("EventManager");

            // Initialize database
            this.RefreshDatabase();
        }

        /// <summary>
        /// Befores the draw childs data.
        /// </summary>
        public override void BeforeDrawChildsData() {
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Event to Send");

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            
            Color previousExecutionModeColor    = GUI.backgroundColor;
            GUI.backgroundColor                 = (string.IsNullOrEmpty(this.EventToSend.stringValue)) ? Color.red.WithAlpha(0.5f) : Color.green.WithAlpha(0.5f);
            string[] avaiableEvents             = this.eventDatabase.GetRegisteredEventsName();
            int selectedEventToRegisterIndex    = Array.IndexOf(avaiableEvents, this.EventToSend.stringValue);
            int selectedEventToRegister         = EditorGUILayout.Popup(selectedEventToRegisterIndex, avaiableEvents, GUILayout.Width(250));
            this.EventToSend.stringValue        = (selectedEventToRegister > -1) ? avaiableEvents[selectedEventToRegister] : null;
            this.EventCodeToSend.intValue       = (selectedEventToRegister > -1) ? this.eventDatabase.GetEventCode(avaiableEvents[selectedEventToRegister]) : -1;
            GUI.backgroundColor                 = previousExecutionModeColor;

            List<ActionExecutionMode>   executionModeTypes = new List<ActionExecutionMode>();
            List<String>                executionModeNames = new List<String>();
            foreach(ActionExecutionMode enumVal in Enum.GetValues(typeof(ActionExecutionMode)).Cast<ActionExecutionMode>()) {
                executionModeTypes.Add(enumVal);
                executionModeNames.Add(enumVal.ToString());
            }
            previousExecutionModeColor      = GUI.backgroundColor;
            GUI.backgroundColor             = Color.yellow;
            int selectedExecutionModeIndex  = (ExecutionMode != null) ? Array.IndexOf(executionModeTypes.ToArray<ActionExecutionMode>(), (ActionExecutionMode)ExecutionMode.enumValueIndex) : (int)ActionExecutionMode.OnBecameTrue;
            int selectedExecutionMode       = EditorGUILayout.Popup(selectedExecutionModeIndex, executionModeNames.ToArray<string>(), GUILayout.Width(250));
            ExecutionMode.enumValueIndex    = selectedExecutionMode;
            GUI.backgroundColor             = previousExecutionModeColor;

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
            // Draw triger detection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.gray.WithAlpha(0.5f)));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(2.0f);
            EditorUtils.PrintExplanationLabel("Detection Trigger", "oo_trigger_activation", Color.white, 5f);
            GUILayout.Space(4.0f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Detection target");
            GameObject previousObject = (this.TriggerTarget.objectReferenceValue as GameObject);
            this.TriggerTarget.objectReferenceValue = (EditorGUILayout.ObjectField(this.TriggerTarget.objectReferenceValue, typeof(GameObject), true, GUILayout.Width(250)) as GameObject);
            EditorGUILayout.EndHorizontal();

            if (this.TriggerTarget.objectReferenceValue != null) {
                if (previousObject != this.TriggerTarget.objectReferenceValue) {
                    bool inScene = false;
#pragma warning disable CS0618
                    foreach (GameObject obj in GameObject.FindObjectsOfType(typeof(GameObject))) {
                        inScene |= (obj == (this.TriggerTarget.objectReferenceValue as GameObject));
                        if (inScene) break;
                    }
#pragma warning restore CS0618
                    if ( !inScene ) {
                        this.TriggerTarget.objectReferenceValue = null;
                        this.SetExecutionError("Only objects in scene can be used to trigger an event");
                    } else {
                        this.TargetDetectionError.stringValue = null;
                    }
                }
                // Draw trigger filter
                if (this.TriggerTarget.objectReferenceValue != null) { 
                    List<MonoBehaviour> components = new List<MonoBehaviour>();
                    List<String>        componentsName = new List<String>();
                    foreach (MonoBehaviour component in (TriggerTarget.objectReferenceValue as GameObject).GetComponents<MonoBehaviour>()) {
                        if (typeof(MonoBehaviour).IsAssignableFrom(component.GetType())) {
                            components.Add(component);
                            componentsName.Add(component.GetType().Name);
                        }
                    }

                    if (this.TriggerTarget.objectReferenceValue != null) {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Component Source");
                        int selectedObjectIndex = (this.TriggerComponent.objectReferenceValue != null) ? Array.IndexOf(componentsName.ToArray<string>(), this.TriggerComponent.objectReferenceValue.GetType().Name) : -1;
                        int selectedObject      = EditorGUILayout.Popup(selectedObjectIndex, componentsName.ToArray<string>(), GUILayout.Width(250));
                        this.TriggerComponent.objectReferenceValue = (((selectedObject < components.Count) && (selectedObject > -1)) ? components[selectedObject] : null);
                        EditorGUILayout.EndHorizontal();

                        if (selectedObject > -1) {
                            List<ParameterSourceType>   paramsTypes = new List<ParameterSourceType>();
                            List<String>                paramsTypeNames = new List<String>();

                            foreach (ParameterSourceType enumVal in Enum.GetValues(typeof(ParameterSourceType)).Cast<ParameterSourceType>()) {
                                paramsTypes.Add(enumVal);
                                paramsTypeNames.Add(enumVal.ToString());
                            }
                            GUILayout.Space(5.0f);

                            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.EVENT_BACKGROUND_COLOR.WithAlpha(0.25f)));
                            EditorGUILayout.BeginVertical();
                            EditorUtils.PrintExplanationLabel("Trigger expression", "oo_logic", Color.white, 5f);
                            GUILayout.Space(4.0f);
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();

                            if ( this.TriggerExpressions.arraySize == 0 ) {
                                TriggerExpression newAction = new TriggerExpression();
                                this.TriggerExpressions.InsertArrayElementAtIndex(this.TriggerExpressions.arraySize);
                                this.TriggerExpressions.GetArrayElementAtIndex(this.TriggerExpressions.arraySize - 1).objectReferenceValue = newAction;
                            }
                            int addOnIndex = -1;
                            int deleteOnIndex = -1;
                            
                            for (int actionIndex = 0; actionIndex < this.TriggerExpressions.arraySize; actionIndex++) { 
                                GUILayout.Space(5.0f);
                                TriggerExpression eventTriggerAction = (this.TriggerExpressions.GetArrayElementAtIndex(actionIndex).objectReferenceValue as TriggerExpression);
                                
                                GUILayout.Space(5.0f);
                                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.SUBDETAIL_ITEM_COLOR.WithAlpha(0.25f)));
                                EditorGUILayout.BeginVertical();
                                int selectedParamIndex         = (eventTriggerAction != null)? Array.IndexOf(paramsTypes.ToArray<ParameterSourceType>(), eventTriggerAction.SourceType) : 0;
                                int selectedParam              = EditorGUILayout.Popup(selectedParamIndex, paramsTypeNames.ToArray<string>(), GUILayout.Width(150));
                                eventTriggerAction.SourceType  = (ParameterSourceType)selectedParam;

                                EditorGUILayout.BeginHorizontal();
                                if (actionIndex == this.TriggerExpressions.arraySize - 1) {
                                    GUILayout.Space(10.0f);
                                }
                                EditorGUILayout.BeginVertical();
                                GUILayout.Space(2.0f);
                                EditorGUILayout.BeginHorizontal();
                                if (actionIndex == this.TriggerExpressions.arraySize - 1) {                                    
                                    if (GUILayout.Button(Resources.Load("oo_add") as Texture, GUIStyle.none, GUILayout.Width(14), GUILayout.Height(14))) {
                                        addOnIndex = actionIndex;
                                    }
                                    GUILayout.Space(5.0f);
                                    if (GUILayout.Button(Resources.Load("oo_delete") as Texture, GUIStyle.none, GUILayout.Width(14), GUILayout.Height(14))) {
                                        deleteOnIndex = actionIndex;
                                    }
                                } else {
                                    List<TriggerCondition>  conditionTypes = new List<TriggerCondition>();
                                    List<String>            conditionTypeNames = new List<String>();
                                    foreach(TriggerCondition enumVal in Enum.GetValues(typeof(TriggerCondition)).Cast<TriggerCondition>()) {
                                        conditionTypes.Add(enumVal);
                                        conditionTypeNames.Add(enumVal.ToString());
                                    }
                                    Color previousColor = GUI.backgroundColor;
                                    GUI.backgroundColor = Color.green;
                                    int selectedConditionIndex      = (eventTriggerAction != null) ? Array.IndexOf(conditionTypes.ToArray<TriggerCondition>(), eventTriggerAction.Condition) : 0;
                                    int selectedCondition           = EditorGUILayout.Popup(selectedConditionIndex, conditionTypeNames.ToArray<string>(), GUILayout.Width(80));
                                    eventTriggerAction.Condition    = (TriggerCondition)selectedCondition;
                                    GUI.backgroundColor = previousColor;
                                }
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.EndVertical();
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.EndVertical();
                                
                                GUILayout.FlexibleSpace();
                                if (ParameterSourceType.Function.Equals(eventTriggerAction.SourceType)) {
                                    this.DrawTriggerFunctionSelector(eventTriggerAction);
                                } else if (ParameterSourceType.Attribute.Equals(eventTriggerAction.SourceType)) {
                                    this.DrawTriggerAttributeSelector(eventTriggerAction);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            if ( addOnIndex > -1 ) {
                                TriggerExpression newAction = new TriggerExpression();
                                this.TriggerExpressions.InsertArrayElementAtIndex(addOnIndex + 1);
                                this.TriggerExpressions.GetArrayElementAtIndex(addOnIndex + 1).objectReferenceValue = newAction;
                            } else if (deleteOnIndex > -1) {
                                this.TriggerExpressions.DeleteArrayElementAtIndex(deleteOnIndex);
                            }
                        }
                    }
                }
            }
            
            if ( this.HasExecutionError() ) {
                GUILayout.Space(10.0f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5.0f);
                EditorUtils.PrintExplanationLabel(this.GetExecutionError(), "oo_error", EditorUtils.EXPLANATION_FONT_COLOR);
                GUILayout.Space(10.0f);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10.0f);
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.gray.WithAlpha(0.5f)));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(2.0f);
            EditorUtils.PrintExplanationLabel("Message arguments", "oo_send", Color.white, 5f);
            GUILayout.Space(4.0f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Afters the draw childs data.
        /// </summary>
        public override void AfterDrawChildsData() {
            if (( this.GetEventTarget() != null ) &&
                ( this.GetEventTarget().objectReferenceValue != null )) {
                // Draw trigger filter
                bool registerNewArgument = false;
                int deleteOnIndex = -1;
                if (this.GetEventTarget().objectReferenceValue != null) { 
                    List<AttributeSourceType>   paramsTypes     = new List<AttributeSourceType>();
                    List<String>                paramsTypeNames = new List<String>();

                    foreach (AttributeSourceType enumVal in Enum.GetValues(typeof(AttributeSourceType)).Cast<AttributeSourceType>()) {
                        paramsTypes.Add(enumVal);
                        paramsTypeNames.Add(enumVal.ToString());
                    }
                    GUILayout.Space(5.0f);

                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.EVENT_BACKGROUND_COLOR.WithAlpha(0.25f)));
                    EditorGUILayout.BeginVertical();
                    EditorUtils.PrintExplanationLabel("Arguments", "oo_attributes", Color.white, 5f);
                    GUILayout.Space(4.0f);
                    EditorGUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(4.0f);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(Resources.Load("oo_add") as Texture, GUIStyle.none, GUILayout.Width(14), GUILayout.Height(14))) {
                        registerNewArgument = true;
                    }
                    GUILayout.Space(4.0f);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(4.0f);
                    EditorGUILayout.EndHorizontal();

                    if (this.AttributeExpressions.arraySize > 0) {
                        GUILayout.Space(5.0f);
                    }
                    for (int actionIndex = 0; actionIndex < this.AttributeExpressions.arraySize; actionIndex++) { 
                        AttributeExpression actionArgument = (this.AttributeExpressions.GetArrayElementAtIndex(actionIndex).objectReferenceValue as AttributeExpression);
                        
                        GUILayout.Space(2.0f);
                        EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorUtils.SUBDETAIL_ITEM_COLOR.WithAlpha(0.25f)));
                        GUILayout.Space(5.0f);
                        EditorGUILayout.BeginVertical();
                        GUILayout.Space(3.0f);
                        EditorGUILayout.BeginVertical();
                        if (GUILayout.Button(Resources.Load("oo_delete") as Texture, GUIStyle.none, GUILayout.Width(14), GUILayout.Height(14))) {
                            deleteOnIndex = actionIndex;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();

                        GUILayout.Space(5.0f);
                        EditorGUILayout.BeginVertical();
                        int selectedParamIndex          = (actionArgument != null)? Array.IndexOf(paramsTypes.ToArray<AttributeSourceType>(), actionArgument.SourceType) : 0;
                        int selectedParam               = EditorGUILayout.Popup(selectedParamIndex, paramsTypeNames.ToArray<string>(), GUILayout.Width(150));
                        if ( actionArgument.SourceType != (AttributeSourceType)selectedParam ) {
                            actionArgument.ParameterValue = null;
                        }
                        actionArgument.SourceType       = (AttributeSourceType)selectedParam;

                        EditorGUILayout.BeginHorizontal();
                        if (actionIndex == this.TriggerExpressions.arraySize - 1) {
                            GUILayout.Space(10.0f);
                        }                        
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                                
                        GUILayout.FlexibleSpace();
                        if (AttributeSourceType.Function.Equals(actionArgument.SourceType)) {
                            this.DrawActionFunctionSelector(actionArgument);
                        } else if (AttributeSourceType.Attribute.Equals(actionArgument.SourceType)) {
                            this.DrawActionAttributeSelector(actionArgument);
                        } else if (AttributeSourceType.String.Equals(actionArgument.SourceType)) {
                            actionArgument.DataType         = typeof(string);
                            actionArgument.ParameterValue   = EditorGUILayout.TextField((actionArgument.ParameterValue != null) ? (actionArgument.ParameterValue as string) : null, GUILayout.Width(250));
                        } else if (AttributeSourceType.Integer.Equals(actionArgument.SourceType)) {
                            actionArgument.DataType         = typeof(int);
                            actionArgument.ParameterValue   = EditorGUILayout.IntField((actionArgument.ParameterValue != null) ? (int)actionArgument.ParameterValue : 0, GUILayout.Width(250));        
                        } else if (AttributeSourceType.Float.Equals(actionArgument.SourceType)) {
                            actionArgument.DataType         = typeof(float);
                            actionArgument.ParameterValue   = EditorGUILayout.FloatField((actionArgument.ParameterValue != null) ? (float)actionArgument.ParameterValue : 0, GUILayout.Width(250));        
                        } else if (AttributeSourceType.Double.Equals(actionArgument.SourceType)) {
                            actionArgument.DataType         = typeof(double);
                            actionArgument.ParameterValue   = EditorGUILayout.DoubleField((actionArgument.ParameterValue != null) ? (double)actionArgument.ParameterValue : 0.0, GUILayout.Width(250));
                        } else if (AttributeSourceType.Boolean.Equals(actionArgument.SourceType)) {
                            actionArgument.DataType         = typeof(bool);
                            actionArgument.ParameterValue   = (EditorGUILayout.Popup(((actionArgument.ParameterValue != null) && ((bool)actionArgument.ParameterValue == true)) ? 0 : 1, new string[] { "true", "false" }, GUILayout.Width(250)) == 0);                            
                        }
                        EditorGUILayout.EndHorizontal();                        
                    }
                    if ( registerNewArgument ) {
                        AttributeExpression newExpression = new AttributeExpression();
                        this.AttributeExpressions.InsertArrayElementAtIndex(this.AttributeExpressions.arraySize);
                        this.AttributeExpressions.GetArrayElementAtIndex(this.AttributeExpressions.arraySize - 1).objectReferenceValue = newExpression;
                    } else if (deleteOnIndex > -1) {
                        this.AttributeExpressions.DeleteArrayElementAtIndex(deleteOnIndex);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the trigger function selector.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        private void DrawTriggerFunctionSelector(TriggerExpression trigger) {
            List<MethodInfo>    parameters        = new List<MethodInfo>();
            List<String>        parametersName    = new List<String>();
            if (this.TriggerComponent.objectReferenceValue != null) {
                foreach (MethodInfo method in this.TriggerComponent.objectReferenceValue.GetType().GetMethods(BindingFlags.Public
                                                                                                              | BindingFlags.NonPublic
                                                                                                              | BindingFlags.Instance
                                                                                                              | BindingFlags.DeclaredOnly)) {
                    if ((method.ReturnParameter.ParameterType == typeof(string)) ||
                        (method.ReturnParameter.ParameterType == typeof(int)) ||
                        (method.ReturnParameter.ParameterType == typeof(uint)) ||
                        (method.ReturnParameter.ParameterType == typeof(short)) ||
                        (method.ReturnParameter.ParameterType == typeof(ushort)) ||
                        (method.ReturnParameter.ParameterType == typeof(bool)) ||
                        (method.ReturnParameter.ParameterType == typeof(float)) ||
                        (method.ReturnParameter.ParameterType == typeof(double))) {
                        ParameterInfo[] arguments = method.GetParameters();
                        if (arguments.Count() == 0) {
                            parameters.Add(method);
                            parametersName.Add(method.Name);
                        }
                    }
                }                
            }

            EditorGUILayout.BeginVertical();

            int selectedParameterIndex   = ((trigger != null) && (trigger.Parameter != null)) ? Array.IndexOf(parametersName.ToArray<string>(), trigger.Parameter) : -1;
            int selectedParameter        = EditorGUILayout.Popup(selectedParameterIndex, parametersName.ToArray<string>(), GUILayout.Width(250));
            
            if (( selectedParameter > -1 ) && (selectedParameter < parameters.Count)) {
                if ( trigger.Parameter != parameters[selectedParameter].Name ) {
                    trigger.Expected    = null;
                    NetworkDebugger.Log("NULL 1");
                }
                trigger.Parameter = parameters[selectedParameter].Name;
            } else if (trigger != null){
                trigger.Parameter   = null;
                trigger.Expected    = null;
                NetworkDebugger.Log("NULL 2");
            }

            GUILayout.Space(2.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
                        
            Type returnType = null;
            if ((selectedParameter > -1) && (selectedParameter < parameters.Count)) {
                returnType = parameters[selectedParameter].ReturnType;
            } else {
                returnType = typeof(bool);
            }

            List<TriggerComparators>    comparatorTypes = new List<TriggerComparators>();
            List<String>                comparatorTypeNames = new List<String>();
            foreach(TriggerComparators enumVal in Enum.GetValues(typeof(TriggerComparators)).Cast<TriggerComparators>()) {
                if ((enumVal <= TriggerComparators.Different) ||
                    (returnType == typeof(int))              ||
                    (returnType == typeof(float))            ||
                    (returnType == typeof(double))) {
                    comparatorTypes.Add(enumVal);
                    comparatorTypeNames.Add(enumVal.ToString());
                }
            }
            int selectedComparatorIndex = (trigger != null) ? Array.IndexOf(comparatorTypes.ToArray<TriggerComparators>(), trigger.Comparator) : 0;
            int selectedComparator      = EditorGUILayout.Popup(selectedComparatorIndex, comparatorTypeNames.ToArray<string>(), GUILayout.Width(80));

            trigger.Comparator          = (TriggerComparators)selectedComparator;
            // Get selected method return type
            if (( selectedParameter > -1 ) && (selectedParameter < parameters.Count)) {
                trigger.DataType = returnType;
                if ( returnType == typeof(string) ) {
                    trigger.Expected = EditorGUILayout.TextField((trigger.Expected != null) ? (trigger.Expected as string) : null, GUILayout.Width(165));
                } else if ( returnType == typeof(int) ) {
                    trigger.Expected = EditorGUILayout.IntField((trigger.Expected != null) ? (int)trigger.Expected : 0, GUILayout.Width(165));
                } else if ( returnType == typeof(bool) ) {
                    trigger.Expected = (EditorGUILayout.Popup(((trigger.Expected != null) && ((bool)trigger.Expected == true)) ? 0 : 1, new string[] { "true", "false" }, GUILayout.Width(165)) == 0);
                } else if ( returnType == typeof(float) ) {
                    trigger.Expected = EditorGUILayout.FloatField((trigger.Expected != null) ? (float)trigger.Expected : 0.0f, GUILayout.Width(165));
                } else if ( returnType == typeof(double) ) {
                    trigger.Expected = EditorGUILayout.DoubleField((trigger.Expected != null) ? (double)trigger.Expected : 0.0, GUILayout.Width(165));
                }
            }
            
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10.0f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the trigger attribute selector.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        private void DrawTriggerAttributeSelector(TriggerExpression trigger) {
            List<FieldInfo> parameters        = new List<FieldInfo>();
            List<String>    parametersName    = new List<String>();
            if (this.TriggerComponent.objectReferenceValue != null) {
                foreach (FieldInfo info in this.TriggerComponent.objectReferenceValue.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                    if (info.Attributes != FieldAttributes.Literal) {
                        if ((info.FieldType == typeof(string)) ||
                            (info.FieldType == typeof(int)) ||
                            (info.FieldType == typeof(bool)) ||
                            (info.FieldType == typeof(float)) ||
                            (info.FieldType == typeof(double))) {
                            parameters.Add(info);
                            parametersName.Add(info.Name);
                        }
                    }
                }                
            }

            EditorGUILayout.BeginVertical();

            int selectedParameterIndex   = ((trigger != null) && (trigger.Parameter != null)) ? Array.IndexOf(parametersName.ToArray<string>(), trigger.Parameter) : -1;
            int selectedParameter        = EditorGUILayout.Popup(selectedParameterIndex, parametersName.ToArray<string>(), GUILayout.Width(250));
            
            if (( selectedParameter > -1 ) && (selectedParameter < parameters.Count)) {
                if ( trigger.Parameter != parameters[selectedParameter].Name ) {
                    trigger.DataType    = null;
                    trigger.Expected    = null;
                }
                trigger.Parameter = parameters[selectedParameter].Name;
            } else if (trigger != null){
                trigger.DataType    = null;
                trigger.Parameter   = null;
                trigger.Expected    = null;
            }

            GUILayout.Space(2.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
                        
            Type returnType = null;
            if ((selectedParameter > -1) && (selectedParameter < parameters.Count)) {
                returnType = parameters[selectedParameter].FieldType;
            } else {
                returnType = typeof(bool);
            }

            List<TriggerComparators>    comparatorTypes = new List<TriggerComparators>();
            List<String>                comparatorTypeNames = new List<String>();
            foreach(TriggerComparators enumVal in Enum.GetValues(typeof(TriggerComparators)).Cast<TriggerComparators>()) {
                if ((enumVal <= TriggerComparators.Different) ||
                    (returnType == typeof(int))              ||
                    (returnType == typeof(float))            ||
                    (returnType == typeof(double))) {
                    comparatorTypes.Add(enumVal);
                    comparatorTypeNames.Add(enumVal.ToString());
                }
            }
            int selectedComparatorIndex = (trigger != null) ? Array.IndexOf(comparatorTypes.ToArray<TriggerComparators>(), trigger.Comparator) : 0;
            int selectedComparator      = EditorGUILayout.Popup(selectedComparatorIndex, comparatorTypeNames.ToArray<string>(), GUILayout.Width(80));

            trigger.Comparator          = (TriggerComparators)selectedComparator;
            // Get selected method return type
            if (( selectedParameter > -1 ) && (selectedParameter < parameters.Count)) {     
                trigger.DataType = returnType;
                if ( returnType == typeof(string) ) {
                    trigger.Expected = EditorGUILayout.TextField((trigger.Expected != null) ? (trigger.Expected as string) : null, GUILayout.Width(165));
                } else if (( returnType == typeof(int) ) ||
                           ( returnType == typeof(uint) ) ||
                           ( returnType == typeof(short) ) ||
                           ( returnType == typeof(ushort) )) {
                    trigger.Expected = EditorGUILayout.IntField((trigger.Expected != null) ? (int)trigger.Expected : 0, GUILayout.Width(165));
                } else if ( returnType == typeof(bool) ) {
                    trigger.Expected = (EditorGUILayout.Popup(((trigger.Expected != null) && ((bool)trigger.Expected == true)) ? 0 : 1, new string[] { "true", "false" }, GUILayout.Width(165)) == 0);
                } else if ( returnType == typeof(float) ) {
                    trigger.Expected = EditorGUILayout.FloatField((trigger.Expected != null) ? (float)trigger.Expected : 0.0f, GUILayout.Width(165));
                } else if ( returnType == typeof(double) ) {
                    trigger.Expected = EditorGUILayout.DoubleField((trigger.Expected != null) ? (double)trigger.Expected : 0.0, GUILayout.Width(165));
                }
            }
            
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10.0f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the action function selector.
        /// </summary>
        /// <param name="actionEvent">The action event.</param>
        private void DrawActionFunctionSelector(AttributeExpression actionEvent) {
            List<MethodInfo>    parameters        = new List<MethodInfo>();
            List<String>        parametersName    = new List<String>();
            if (this.GetEventComponent().objectReferenceValue != null) {
                foreach (MethodInfo method in this.GetEventComponent().objectReferenceValue.GetType().GetMethods(BindingFlags.Public 
                                                                                                                 | BindingFlags.NonPublic
                                                                                                                 | BindingFlags.Instance
                                                                                                                 | BindingFlags.DeclaredOnly)) {
                    if ((method.ReturnParameter.ParameterType == typeof(string)) ||
                        (method.ReturnParameter.ParameterType == typeof(int)) ||
                        (method.ReturnParameter.ParameterType == typeof(bool)) ||
                        (method.ReturnParameter.ParameterType == typeof(float)) ||
                        (method.ReturnParameter.ParameterType == typeof(double))) {
                        ParameterInfo[] arguments = method.GetParameters();
                        if (arguments.Count() == 0) {
                            parameters.Add(method);
                            parametersName.Add(method.Name);
                        }
                    }
                }                
            }

            EditorGUILayout.BeginVertical();
            int selectedParameterIndex   = ((actionEvent != null) && (actionEvent.Parameter != null)) ? Array.IndexOf(parametersName.ToArray<string>(), actionEvent.Parameter) : -1;
            int selectedParameter        = EditorGUILayout.Popup(selectedParameterIndex, parametersName.ToArray<string>(), GUILayout.Width(250));
            
            if (( selectedParameter > -1 ) && (selectedParameter < parameters.Count)) {
                actionEvent.DataType    = parameters[selectedParameter].GetType();
                actionEvent.Parameter   = parameters[selectedParameter].Name;
            } else if (actionEvent != null){
                actionEvent.DataType    = null;
                actionEvent.Parameter   = null;
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the action attribute selector.
        /// </summary>
        /// <param name="actionEvent">The action event.</param>
        private void DrawActionAttributeSelector(AttributeExpression actionEvent) {
            List<FieldInfo> parameters        = new List<FieldInfo>();
            List<String>    parametersName    = new List<String>();
            if (this.GetEventComponent().objectReferenceValue != null) {
                foreach (FieldInfo info in this.GetEventComponent().objectReferenceValue.GetType().GetFields(BindingFlags.Public
                                                                                                             | BindingFlags.NonPublic
                                                                                                             | BindingFlags.Instance
                                                                                                             | BindingFlags.DeclaredOnly)) {
                    if (info.Attributes != FieldAttributes.Literal) {
                        if ((info.FieldType == typeof(string)) ||
                            (info.FieldType == typeof(int)) ||
                            (info.FieldType == typeof(bool)) ||
                            (info.FieldType == typeof(float)) ||
                            (info.FieldType == typeof(double))) {
                            parameters.Add(info);
                            parametersName.Add(info.Name);
                        }
                    }
                }                
            }

            EditorGUILayout.BeginVertical();

            int selectedParameterIndex   = ((actionEvent != null) && (actionEvent.Parameter != null)) ? Array.IndexOf(parametersName.ToArray<string>(), actionEvent.Parameter) : -1;
            int selectedParameter        = EditorGUILayout.Popup(selectedParameterIndex, parametersName.ToArray<string>(), GUILayout.Width(250));
            
            if (( selectedParameter > -1 ) && (selectedParameter < parameters.Count)) {
                actionEvent.Parameter   = parameters[selectedParameter].Name;
            } else if (actionEvent != null){
                actionEvent.Parameter   = null;
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Refreshes the database.
        /// </summary>
        public void RefreshDatabase() {
            serializedObject.Update();
            string database = (this.EventManager.objectReferenceValue as NetworkEventsManager).GetTargetDatabase();
            this.eventDatabase = Resources.Load<NetworkEventsDatabase>(GlobalResources.GetEventsDatabase(database));
            if (this.eventDatabase == null) {
                NetworkEventsDatabaseWindow.CreateNetworkDatabaseSource(database);
                this.eventDatabase = Resources.Load<NetworkEventsDatabase>(GlobalResources.GetEventsDatabase(database));
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
        /// Determines whether [has execution error].
        /// </summary>
        /// <returns><c>true</c> if [has execution error]; otherwise, <c>false</c>.</returns>
        public override bool HasExecutionError() {
            return !string.IsNullOrEmpty(this.TargetDetectionError.stringValue) && (this.TargetDetectionErrorTimeout.floatValue > (float)EditorApplication.timeSinceStartup);
        }

        /// <summary>
        /// Gets the execution error.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetExecutionError() {
            return this.TargetDetectionError.stringValue;
        }

        /// <summary>
        /// Sets the execution error.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetExecutionError(string value) {
            this.TargetDetectionError.stringValue = value;
            this.TargetDetectionErrorTimeout.floatValue = ((float)EditorApplication.timeSinceStartup + DELAY_TO_DISMISS_ERROR);
        }

        /// <summary>
        /// Determines whether [has overriden event types].
        /// </summary>
        /// <returns><c>true</c> if [has overriden event types]; otherwise, <c>false</c>.</returns>
        public override bool HasOverridenEventTypes() {
            return true;
        }

        /// <summary>
        /// Gets the type of the return.
        /// </summary>
        /// <returns>Type.</returns>
        public override Type GetReturnType() {
            return typeof(void);
        }

        /// <summary>
        /// Gets the type of the parameters.
        /// </summary>
        /// <returns>Type[].</returns>
        public override Type[] GetParametersType() {
            return new Type[] { 
                typeof(IDataStream)
            };
        }

        /// <summary>
        /// Determines whether [is method selectable].
        /// </summary>
        /// <returns><c>true</c> if [is method selectable]; otherwise, <c>false</c>.</returns>
        public override bool IsMethodSelectable() {
            return false;
        }
    }
#endif
}
