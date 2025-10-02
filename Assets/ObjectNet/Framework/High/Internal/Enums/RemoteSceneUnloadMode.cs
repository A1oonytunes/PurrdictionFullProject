namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines how scene will be loaded on the server side
    /// </summary>
    public enum RemoteSceneUnloadMode {

        /// <summary>
        /// Unload scene first in the server side and notify clients to unload
        /// </summary>
        UnloadFirst,

        /// <summary>
        /// Request clients to unload scene and wait all of the unload to load on server
        /// </summary>
        UnloadAfter
    }
}