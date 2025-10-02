using System.Collections.Generic;
using System.Net;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines an interface for a provider that can retrieve different types of IP addresses.
    /// </summary>
    internal interface IIPAddressesProvider {
        /// <summary>
        /// Retrieves a collection of DNS server IP addresses.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IPAddress"/> representing DNS server addresses.</returns>
        IEnumerable<IPAddress> DnsAddresses();

        /// <summary>
        /// Retrieves a collection of gateway IP addresses.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IPAddress"/> representing gateway addresses.</returns>
        IEnumerable<IPAddress> GatewayAddresses();

        /// <summary>
        /// Retrieves a collection of unicast IP addresses.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IPAddress"/> representing unicast addresses.</returns>
        IEnumerable<IPAddress> UnicastAddresses();
    }

}