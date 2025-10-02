using System;

namespace com.onlineobject.objectnet.editor {
    /// <summary>
    /// Interface INetworkEventEditor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INetworkEventEditor<T> where T : NetworkEventsManager {
        /// <summary>
        /// Gets the manager.
        /// </summary>
        /// <returns>T.</returns>
        T GetManager();

        /// <summary>
        /// Sets the manager.
        /// </summary>
        /// <param name="manager">The manager.</param>
        void SetManager(T manager);

        /// <summary>
        /// Gets the type of the managed.
        /// </summary>
        /// <returns>Type.</returns>
        Type GetManagedType();

        /// <summary>
        /// Instantiates the type.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <returns>UnityEngine.Object.</returns>
        UnityEngine.Object InstantiateType(Type eventType);

        /// <summary>
        /// Registers the type of the constructor of.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="method">The method.</param>
        void RegisterConstructorOfType(Type type, Func<UnityEngine.Object> method);
    }
}