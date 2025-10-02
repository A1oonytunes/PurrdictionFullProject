using System;
using UnityEditor;

namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    /// <summary>
    /// Interface IEventEditor
    /// </summary>
    public interface IEventEditor {

        /// <summary>
        /// Gets the event target.
        /// </summary>
        /// <returns>SerializedProperty.</returns>
        SerializedProperty GetEventTarget();

        /// <summary>
        /// Gets the event component.
        /// </summary>
        /// <returns>SerializedProperty.</returns>
        SerializedProperty GetEventComponent();

        /// <summary>
        /// Gets the event method.
        /// </summary>
        /// <returns>SerializedProperty.</returns>
        SerializedProperty GetEventMethod();

        /// <summary>
        /// Draws the inspector.
        /// </summary>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="parametersType">Type of the parameters.</param>
        void DrawInspector(Type returnType, Type[] parametersType);

        /// <summary>
        /// Befores the draw childs data.
        /// </summary>
        void BeforeDrawChildsData();

        /// <summary>
        /// Afters the draw childs data.
        /// </summary>
        void AfterDrawChildsData();

        /// <summary>
        /// Determines whether [has overriden event types].
        /// </summary>
        /// <returns><c>true</c> if [has overriden event types]; otherwise, <c>false</c>.</returns>
        bool HasOverridenEventTypes();

        /// <summary>
        /// Gets the type of the return.
        /// </summary>
        /// <returns>Type.</returns>
        Type GetReturnType();

        /// <summary>
        /// Gets the type of the parameters.
        /// </summary>
        /// <returns>Type[].</returns>
        Type[] GetParametersType();

        /// <summary>
        /// Determines whether [is object selectable].
        /// </summary>
        /// <returns><c>true</c> if [is object selectable]; otherwise, <c>false</c>.</returns>
        bool IsObjectSelectable();

        /// <summary>
        /// Determines whether [is component selectable].
        /// </summary>
        /// <returns><c>true</c> if [is component selectable]; otherwise, <c>false</c>.</returns>
        bool IsComponentSelectable();

        /// <summary>
        /// Determines whether [is method selectable].
        /// </summary>
        /// <returns><c>true</c> if [is method selectable]; otherwise, <c>false</c>.</returns>
        bool IsMethodSelectable();

        /// <summary>
        /// Flag if this editor will used serialized fielf or object directly
        /// </summary>
        /// <param name="value">True if need ro use serializable field</param>
        void SetToUseSeriableField(bool value);

        /// <summary>
        /// Set instance of target event reference
        /// </summary>
        /// <param name="value">Event instance</param>
        void SetReferenceInstance(EventReference value);
    }
#endif
}
