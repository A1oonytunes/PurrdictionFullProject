namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a delayed removal operation for a network element.
    /// </summary>
    public class DelayedRemove {

        // Reference to the client associated with the removal.
        private IClient Client;

        // Reference to the network element to be removed.
        private INetworkElement NetworkElement;

        // The timeout duration after which the element should be removed.
        private float Timeout = 0f;

        /// <summary>
        /// Initializes a new instance of the DelayedRemove class.
        /// </summary>
        /// <param name="client">The client associated with the removal.</param>
        /// <param name="element">The network element to be removed.</param>
        /// <param name="timeout">The timeout duration in seconds.</param>
        public DelayedRemove(IClient client, INetworkElement element, float timeout) {
            this.Client = client;
            this.NetworkElement = element;
            this.Timeout = timeout;
        }

        /// <summary>
        /// Retrieves the client associated with the removal.
        /// </summary>
        /// <returns>The client object.</returns>
        public IClient GetClient() {
            return this.Client;
        }

        /// <summary>
        /// Retrieves the network element to be removed.
        /// </summary>
        /// <returns>The network element object.</returns>
        public INetworkElement GetElement() {
            return this.NetworkElement;
        }

        /// <summary>
        /// Determines if the current time has exceeded the timeout duration.
        /// </summary>
        /// <returns>True if the timeout has been exceeded, otherwise false.</returns>
        public bool IsTimedOut() {
            // NetworkClock.time is assumed to be a static property representing the current network time.
            return (this.Timeout < NetworkClock.time);
        }
    }

}
