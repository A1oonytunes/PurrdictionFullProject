#if UNITY_TRANSPORT_ENABLED
using Unity.Networking.Transport;
#endif

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Interface defining the contract for a Unity-specific transport layer.
    /// </summary>
    public interface IUnityTransport : ITransportClient {
#if UNITY_TRANSPORT_ENABLED
        /// <summary>
        /// Sets the network connection for the transport.
        /// </summary>
        /// <param name="connection">The NetworkConnection instance to be used by the transport.</param>
        void SetConnection(NetworkConnection connection);

        /// <summary>
        /// Retrieves the current network connection being used by the transport.
        /// </summary>
        /// <returns>The current NetworkConnection instance.</returns>
        NetworkConnection GetConnection();

        /// <summary>
        /// Checks if the connection has been lost.
        /// </summary>
        /// <returns>True if the connection is lost, otherwise false.</returns>
        bool IsConnectionLost();

        /// <summary>
        /// Determines whether a connection attempt is currently in progress.
        /// </summary>
        /// <returns>True if a connection attempt is in progress, otherwise false.</returns>
        bool IsConnectionInProgress();
#endif
    }

}