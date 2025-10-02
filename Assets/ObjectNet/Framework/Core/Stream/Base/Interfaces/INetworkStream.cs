using System;

namespace com.onlineobject.objectnet {
    // Define an interface for network streams.
    // This interface includes the capability to retrieve a client and to dispose of resources.
    public interface INetworkStream : IDisposable {
        /// <summary>
        /// Retrieves the client associated with the network stream.
        /// </summary>
        /// <returns>An object implementing the IClient interface.</returns>
        IClient GetClient();
    }

}