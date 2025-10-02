namespace com.onlineobject.objectnet {

    /// <summary>
    /// Represents the arguments for a device-related event within the object network.
    /// </summary>
    internal class DeviceEventArgs : System.EventArgs {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceEventArgs"/> class.
        /// </summary>
        /// <param name="device">The device associated with the event.</param>
        public DeviceEventArgs(NatDevice device) {
            Device = device;
        }

        /// <summary>
        /// Gets the device associated with the event.
        /// </summary>
        public NatDevice Device { get; private set; }
    }
}
