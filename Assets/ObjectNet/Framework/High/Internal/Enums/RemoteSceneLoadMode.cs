namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines how scene will be loaded on the server side
    /// </summary>
    public enum RemoteSceneLoadMode {

        /// <summary>
        /// Load scene first in the server side and notify clients to load
        /// </summary>
        LoadFirst,

        /// <summary>
        /// Request clients to load scene and wait all of the load to load on server
        /// </summary>
        LoadAfter
    }
}