using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Define an internal interface for searching devices.
    /// </summary>
    internal interface ISearcher {
        /// <summary>
        /// Initiates a search operation that can be cancelled using the provided CancellationToken.
        /// </summary>
        /// <param name="cancellationToken">Token to signal cancellation of the search operation.</param>
        void Search(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves an enumerable collection of NatDevice objects that have been found.
        /// </summary>
        /// <returns>An IEnumerable of NatDevice representing the devices received from the search.</returns>
        IEnumerable<NatDevice> Receive();

        /// <summary>
        /// Analyzes the received response and constructs a NatDevice object based on the response data.
        /// </summary>
        /// <param name="localAddress">The local IP address used for the search operation.</param>
        /// <param name="response">The byte array containing the response data from a device.</param>
        /// <param name="endpoint">The IPEndPoint from which the response was received.</param>
        /// <returns>A NatDevice object constructed from the response data.</returns>
        NatDevice AnalyseReceivedResponse(IPAddress localAddress, byte[] response, IPEndPoint endpoint);
    }

}
