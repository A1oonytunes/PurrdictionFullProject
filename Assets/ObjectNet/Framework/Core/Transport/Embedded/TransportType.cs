namespace com.onlineobject.objectnet {

    // Define a custom attribute to specify the transport type characteristics.
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class TransportType : System.Attribute {

        // Indicates whether the transport type uses a double channel.
        private bool DoubleChannel = false;

        // Indicates whether the transport support peer to peer.
        private bool PeerToPeerSupport = false;

        // Indicates whether the transport support peer to peer.
        private bool ClientConnectionDelaySupported = false;

        // Indicate the minumum amout of time to wait before send client connection message
        public int ClientConnectionDelayInitial = 0;
        
        // Indicate the maximum amout of time to wait before send client connection message
        public int ClientConnectionDelayFinal = 250;

        // Indicate the default amout of time to wait before send client connection message
        public int ClientConnectionDelayDefault = 100;

        /// <summary>
        /// Initializes a new instance of the TransportType attribute with the specified channel type.
        /// </summary>
        /// <param name="BiChannel">If set to true, the transport type is considered to have a double channel.</param>
        /// <param name="PeerToPeerSupport">If set to true, the transport type will support peer to peer.</param>
        public TransportType(bool BiChannel, bool PeerToPeerSupport) {
            this.DoubleChannel = BiChannel;
            this.PeerToPeerSupport = PeerToPeerSupport;
        }

        /// <summary>
        /// Initializes a new instance of the TransportType attribute with the specified channel type.
        /// </summary>
        /// <param name="PeerToPeerSupport">If set to true, the transport type will support peer to peer.</param>
        public TransportType(bool PeerToPeerSupport) {
            this.DoubleChannel = false;
            this.PeerToPeerSupport = PeerToPeerSupport;
        }

        /// <summary>
        /// Initializes a new instance of the TransportType attribute with the specified channel type.
        /// </summary>
        /// <param name="PeerToPeerSupport">If set to true, the transport type will support peer to peer.</param>
        /// <param name="ConnectionDelaySupport">If set to true, the transport type will support connection delay.</param>
        /// <param name="MinimunConnectionDelay">Minimum connection delay time.</param>
        /// <param name="MaximumConnectionDelay">Maximum connection delay time.</param>
        public TransportType(bool PeerToPeerSupport, bool ConnectionDelaySupport, int MinimunConnectionDelay, int MaximumConnectionDelay) {
            this.DoubleChannel = false;
            this.PeerToPeerSupport = PeerToPeerSupport;
            this.ClientConnectionDelaySupported = ConnectionDelaySupport;
            this.ClientConnectionDelayInitial = MinimunConnectionDelay;
            this.ClientConnectionDelayFinal = MaximumConnectionDelay;
        }

        /// <summary>
        /// Initializes a new instance of the TransportType attribute with the specified channel type.
        /// </summary>
        /// <param name="BiChannel">If set to true, the transport type is considered to have a double channel.</param>
        /// <param name="PeerToPeerSupport">If set to true, the transport type will support peer to peer.</param>
        /// <param name="ConnectionDelaySupport">If set to true, the transport type will support connection delay.</param>
        /// <param name="MinimunConnectionDelay">Minimum connection delay time.</param>
        /// <param name="MaximumConnectionDelay">Maximum connection delay time.</param>
        /// <param name="DefaultClientConnectionDelay">Default connection delay time.</param>
        public TransportType(bool BiChannel, bool ConnectionDelaySupport, bool PeerToPeerSupport, int MinimunConnectionDelay, int MaximumConnectionDelay, int DefaultClientConnectionDelay) {
            this.DoubleChannel = BiChannel;
            this.PeerToPeerSupport = PeerToPeerSupport;
            this.ClientConnectionDelaySupported = ConnectionDelaySupport;
            this.ClientConnectionDelayInitial = MinimunConnectionDelay;
            this.ClientConnectionDelayFinal = MaximumConnectionDelay;
            this.ClientConnectionDelayDefault = DefaultClientConnectionDelay;
        }

        /// <summary>
        /// Initializes a new instance of the TransportType attribute with the specified channel type.
        /// </summary>
        /// <param name="ConnectionDelaySupport">If set to true, the transport type will support connection delay.</param>
        /// <param name="MinimunConnectionDelay">Minimum connection delay time.</param>
        /// <param name="MaximumConnectionDelay">Maximum connection delay time.</param>
        /// <param name="DefaultClientConnectionDelay">Default connection delay time.</param>
        public TransportType(bool ConnectionDelaySupport, int MinimunConnectionDelay, int MaximumConnectionDelay, int DefaultClientConnectionDelay) {
            this.DoubleChannel = false;
            this.PeerToPeerSupport = false;
            this.ClientConnectionDelaySupported = ConnectionDelaySupport;
            this.ClientConnectionDelayInitial = MinimunConnectionDelay;
            this.ClientConnectionDelayFinal = MaximumConnectionDelay;
            this.ClientConnectionDelayDefault = DefaultClientConnectionDelay;
        }

        /// <summary>
        /// Gets a value indicating whether the transport type uses a double channel.
        /// </summary>
        /// <returns>Returns true if the transport type is double channel; otherwise, false.</returns>
        public bool IsDoubleChannel() => DoubleChannel;

        /// <summary>
        /// Gets a value indicating whether the transport support peer to peer.
        /// </summary>
        /// <returns>Returns true if the transport type supports peer to peer; otherwise, false.</returns>
        public bool IsPeerToPeerSupported() => PeerToPeerSupport;

        /// <summary>
        /// Gets a value indicating whether the transport support client connewction delay.
        /// </summary>
        /// <returns>Returns true if the transport type supports client connection delay; otherwise, false.</returns>
        public bool IsClientConnectionDelaySupported() => ClientConnectionDelaySupported;

        /// <summary>
        /// Gets a the minimun client connection delay.
        /// </summary>
        /// <returns>Return the minimun client connection Delay.</returns>
        public int ClientConnectionDelayInitialValue() => ClientConnectionDelayInitial;

        /// <summary>
        /// Gets a the maximun client connection delay.
        /// </summary>
        /// <returns>Return the maximun client connection Delay.</returns>
        public int ClientConnectionDelayFinalValue() => ClientConnectionDelayFinal;

        /// <summary>
        /// Gets a the default client connection delay.
        /// </summary>
        /// <returns>Return the default client connection Delay.</returns>
        public int ClientConnectionDelayDefaultValue() => ClientConnectionDelayDefault;
    }

}