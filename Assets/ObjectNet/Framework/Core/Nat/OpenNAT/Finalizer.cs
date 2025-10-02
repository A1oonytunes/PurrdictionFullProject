namespace com.onlineobject.objectnet {
    /// <summary>
    /// The Finalizer class is a sealed class that ensures that any network address translation (NAT) resources
    /// opened during the session are properly released when the object is being garbage collected.
    /// </summary>
    sealed class Finalizer {
        /// <summary>
        /// The destructor for the Finalizer class.
        /// This method is called by the garbage collector when the object is being collected.
        /// It logs the closure of ports, disposes of the renewal timer, and releases any session mappings.
        /// </summary>
        ~Finalizer() {
            // Log the information that the Finalizer is closing ports.
            NatDiscoverer.TraceSource.LogInfo("Closing ports opened in this session");

            // Dispose of the renewal timer to free up resources.
            NatDiscoverer.RenewTimer.Dispose();

            // Release any mappings created during the session to free up the ports.
            NatDiscoverer.ReleaseSessionMappings();
        }
    }

}