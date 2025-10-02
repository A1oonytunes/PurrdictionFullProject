using System.Net.Sockets;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Provides extension methods for the Socket class related to network connectivity.
    /// </summary>
    public static class NetworkSocketExtensions {
        /// <summary>
        /// Checks if the socket is currently connected to a remote host.
        /// </summary>
        /// <param name="socket">The Socket object to check for connectivity.</param>
        /// <returns>True if the socket is connected, false otherwise.</returns>
        public static bool IsConnected(this Socket socket) {
            try {
                return !((socket.Poll(1, SelectMode.SelectRead) == true) &&
                         (socket.Available == 0));
            } catch {
                return false;
            }
        }
    }
}
