using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    /// <summary>
    /// Class EventReferenceEditor.
    /// Implements the <see cref="Editor" />
    /// Implements the <see cref="com.onlineobject.objectnet.IEventEditor" />
    /// </summary>
    /// <seealso cref="Editor" />
    /// <seealso cref="com.onlineobject.objectnet.IEventEditor" />
    [CustomEditor(typeof(EventReference))]
    [CanEditMultipleObjects]
    public class EventReferenceEditor : Editor, IEventEditor {

        /// <summary>
        /// The event target
        /// </summary>
        SerializedProperty EventTarget;

        /// <summary>
        /// The event component
        /// </summary>
        SerializedProperty EventComponent;

        /// <summary>
        /// The event method
        /// </summary>
        SerializedProperty EventMethod;

        /// <summary>
        /// The execution error
        /// </summary>
        SerializedProperty ExecutionError;

        /// <summary>
        /// The return type
        /// </summary>
        Type returnType;

        /// <summary>
        /// The parameters type
        /// </summary>
        Type[] parametersType;

        /// <summary>
        /// Flag if this editor will used serialized fielf or object directly
        /// </summary>
        bool useSerializedField = true;

        /// <summary>
        /// Instance of target event reference
        /// </summary>
        EventReference eventReferenceInstance;

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public virtual void OnEnable() {
            // Get all serializable objects
            try {
                if (this.useSerializedField) {
                    this.EventTarget    = serializedObject.FindProperty("EventTarget");
                    this.EventComponent = serializedObject.FindProperty("EventComponent");
                    this.EventMethod    = serializedObject.FindProperty("EventMethod");
                    this.ExecutionError = serializedObject.FindProperty("ExecutionError");
                }
            } catch(Exception err) {
                NetworkDebugger.LogWarning("Serialization error, restart editor to fix {0}", err.Message);
            }
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI() {
            if (this.useSerializedField) serializedObject.Update();

            this.DrawEventEditor();

            if (this.useSerializedField) serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the inspector.
        /// </summary>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="parametersType">Type of the parameters.</param>
        public virtual void DrawInspector(Type returnType, Type[] parametersType) {
            this.returnType     = returnType;
            this.parametersType = parametersType;
            this.OnInspectorGUI();
        }

        /// <summary>
        /// Flag if this editor will used serialized fielf or object directly
        /// </summary>
        /// <param name="value">True if need ro use serializable field</param>
        public void SetToUseSeriableField(bool value) {
            this.useSerializedField = value;
        }

        /// <summary>
        /// Set instance of target event reference
        /// </summary>
        /// <param name="value">Event instance</param>
        public void SetReferenceInstance(EventReference value) {
            this.eventReferenceInstance = value;
        }

        /// <summary>
        /// Gets the event target.
        /// </summary>
        /// <returns>SerializedProperty.</returns>
        public virtual SerializedProperty GetEventTarget() {
            return this.EventTarget;
        }

        /// <summary>
        /// Gets the event component.
        /// </summary>
        /// <returns>SerializedProperty.</returns>
        public virtual SerializedProperty GetEventComponent() {
            return this.EventComponent;
        }

        /// <summary>
        /// Gets the event method.
        /// </summary>
        /// <returns>SerializedProperty.</returns>
        public virtual SerializedProperty GetEventMethod() {
            return this.EventMethod;
        }

        /// <summary>
        /// Determines whether [has overriden event types].
        /// </summary>
        /// <returns><c>true</c> if [has overriden event types]; otherwise, <c>false</c>.</returns>
        public virtual bool HasOverridenEventTypes() {
            return false;
        }

        /// <summary>
        /// Gets the type of the return.
        /// </summary>
        /// <returns>Type.</returns>
        public virtual Type GetReturnType() {
            return null;
        }

        /// <summary>
        /// Gets the type of the parameters.
        /// </summary>
        /// <returns>Type[].</returns>
        public virtual Type[] GetParametersType() {
            return null;
        }

        /// <summary>
        /// Determines whether [is object selectable].
        /// </summary>
        /// <returns><c>true</c> if [is object selectable]; otherwise, <c>false</c>.</returns>
        public virtual bool IsObjectSelectable() {
            return true;
        }

        /// <summary>
        /// Determines whether [is component selectable].
        /// </summary>
        /// <returns><c>true</c> if [is component selectable]; otherwise, <c>false</c>.</returns>
        public virtual bool IsComponentSelectable() {
            return true;
        }

        /// <summary>
        /// Determines whether [is method selectable].
        /// </summary>
        /// <returns><c>true</c> if [is method selectable]; otherwise, <c>false</c>.</returns>
        public virtual bool IsMethodSelectable() {
            return true;
        }

        /// <summary>
        /// Determines whether [has execution error].
        /// </summary>
        /// <returns><c>true</c> if [has execution error]; otherwise, <c>false</c>.</returns>
        public virtual bool HasExecutionError() {
            return !string.IsNullOrEmpty(this.ExecutionError.stringValue);
        }

        /// <summary>
        /// Gets the execution error.
        /// </summary>
        /// <returns>System.String.</returns>
        public virtual string GetExecutionError() {
            return this.ExecutionError.stringValue;
        }

        /// <summary>
        /// Sets the execution error.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void SetExecutionError(string value) {
            this.ExecutionError.stringValue = value;
        }

        /// <summary>
        /// Draws the event editor.
        /// </summary>
        private void DrawEventEditor() {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical();
            this.BeforeDrawChildsData(); // Paint any data on inherithed classes
            GUILayout.Space(5.0f);
            EditorUtils.HorizontalLine(Color.gray, 1.0f, Vector2.zero);
            GUILayout.Space(5.0f);
            if (this.IsComponentSelectable()) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Object Source");
                if (this.useSerializedField) {
                    EventTarget.objectReferenceValue = (EditorGUILayout.ObjectField(EventTarget.objectReferenceValue, typeof(GameObject), true, GUILayout.Width(250)) as GameObject);
                } else {
                    this.eventReferenceInstance.SetEventTarget(EditorGUILayout.ObjectField(this.eventReferenceInstance.GetEventTarget(), typeof(GameObject), true, GUILayout.Width(250)) as GameObject);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            
            List<MonoBehaviour> components = new List<MonoBehaviour>();
            List<String>        componentsName = new List<String>();
            if (EventTarget.objectReferenceValue != null) {
                foreach (MonoBehaviour component in (this.useSerializedField ? (EventTarget.objectReferenceValue as GameObject): this.eventReferenceInstance.GetEventTarget()).GetComponents<MonoBehaviour>()) {
                    if (typeof(MonoBehaviour).IsAssignableFrom(component.GetType())) {
                        components.Add(component);
                        componentsName.Add(component.GetType().Name);
                    }
                }
            }

            if ((this.useSerializedField && this.EventTarget.objectReferenceValue != null) ||
                (!this.useSerializedField && this.eventReferenceInstance.GetEventTarget() != null)){
                if (this.IsComponentSelectable()) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Component Source");
                    int selectedObjectIndex = (this.useSerializedField) ? ((EventComponent.objectReferenceValue != null) ? Array.IndexOf(componentsName.ToArray<string>(), EventComponent.objectReferenceValue.GetType().Name) : -1) :
                                                                          ((this.eventReferenceInstance.GetEventComponent() != null) ? Array.IndexOf(componentsName.ToArray<string>(), this.eventReferenceInstance.GetEventComponent().GetType().Name) : -1);
                    int selectedObject = EditorGUILayout.Popup(selectedObjectIndex, componentsName.ToArray<string>(), GUILayout.Width(250));
                    if (this.useSerializedField) {
                        EventComponent.objectReferenceValue = (((selectedObject < components.Count) && (selectedObject > -1)) ? components[selectedObject] : null);
                    } else {
                        this.eventReferenceInstance.SetEventComponent(((selectedObject < components.Count) && (selectedObject > -1)) ? components[selectedObject] : null);
                    }
                    EditorGUILayout.EndHorizontal();
                    if (selectedObject > -1) {
                        List<MethodInfo> methods = new List<MethodInfo>();
                        List<String> methodsName = new List<String>();
                        if ((this.useSerializedField && EventComponent.objectReferenceValue != null) ||
                            (!this.useSerializedField && this.eventReferenceInstance.GetEventComponent() != null)) {
                            foreach (MethodInfo method in (this.useSerializedField ? EventComponent.objectReferenceValue : 
                                                                                     this.eventReferenceInstance.GetEventComponent()).GetType().GetMethods(BindingFlags.Public
                                                                                                                                                          | BindingFlags.Instance
                                                                                                                                                          | BindingFlags.DeclaredOnly)) {
                                if ((method.ReturnParameter.ParameterType == typeof(void)) ||
                                    (method.ReturnParameter.ParameterType == this.returnType)) {
                                    bool parametersMatch = false;
                                    ParameterInfo[] arguments = method.GetParameters();
                                    parametersMatch |= ((parametersType == null) && ((arguments == null) || (arguments.Length == 0)));
                                    if (!parametersMatch) {
                                        if ((this.parametersType != null) && (arguments != null)) {
                                            if (this.parametersType.Length == arguments.Length) {
                                                parametersMatch = true; // True to be checked behind
                                                for (int parameterindex = 0; parameterindex < this.parametersType.Length; parameterindex++) {
                                                    parametersMatch &= (this.parametersType[parameterindex] == arguments[parameterindex].ParameterType);
                                                    parametersMatch &= (this.parametersType[parameterindex].IsArray == arguments[parameterindex].ParameterType.IsArray);
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
                        if (this.IsMethodSelectable()) {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("Method To Execute");
                            int selectedMethodIndex = (this.useSerializedField ? ((EventMethod != null) && (EventMethod.stringValue != null)) :
                                                                                 ((this.eventReferenceInstance.GetEventMethod() != null) && (this.eventReferenceInstance.GetEventMethod() != null))) ? Array.IndexOf(methodsName.ToArray<string>(), EventMethod.stringValue) : -1;
                            int selectedMethod = EditorGUILayout.Popup(selectedMethodIndex, methodsName.ToArray<string>(), GUILayout.Width(250));
                            EditorGUILayout.EndHorizontal();
                            if ((selectedMethod > -1) && (selectedMethod < methods.Count)) {
                                if (this.useSerializedField) {
                                    EventMethod.stringValue = methods[selectedMethod].Name;
                                } else {
                                    this.eventReferenceInstance.SetEventMethod(methods[selectedMethod].Name);
                                }
                            } else if ((this.useSerializedField) && (EventMethod != null)) {
                                EventMethod.stringValue = null;
                            } else {
                                this.eventReferenceInstance.SetEventMethod(null);
                            }
                        }
                    }                    
                }
            } else if ((this.useSerializedField) && (EventMethod != null)) {
                EventMethod.stringValue = null;
            } else {
                this.eventReferenceInstance.SetEventMethod(null);
            }

            this.AfterDrawChildsData();

            EditorGUILayout.EndVertical();
            GUILayout.Space(10.0f);            
            EditorGUILayout.EndHorizontal();
        }


        /// <summary>
        /// Befores the draw childs data.
        /// </summary>
        public virtual void BeforeDrawChildsData() {
        }

        /// <summary>
        /// Afters the draw childs data.
        /// </summary>
        public virtual void AfterDrawChildsData() {
        }
    }
#endif
}
