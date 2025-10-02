namespace com.onlineobject.objectnet.editor {
#if UNITY_EDITOR
    /// <summary>
    /// Interface IDatabaseTargetEditor
    /// </summary>
    public interface IDatabaseTargetEditor {

        /// <summary>
        /// Refreshes the database.
        /// </summary>
        void RefreshDatabase();

        /// <summary>
        /// Refreshes the window.
        /// </summary>
        void RefreshWindow();

    }
#endif
}