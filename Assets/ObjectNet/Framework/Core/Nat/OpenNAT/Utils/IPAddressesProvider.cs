using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Provides methods to retrieve different types of IP addresses from network interfaces.
    /// </summary>
    internal class IPAddressesProvider : IIPAddressesProvider {
        #region IIPAddressesProvider Members

        /// <summary>
        /// Retrieves all unicast IP addresses from active network interfaces.
        /// </summary>
        /// <returns>An enumerable collection of unicast IP addresses.</returns>
        public IEnumerable<IPAddress> UnicastAddresses() {
            return IPAddresses(p => p.UnicastAddresses.Select(x => x.Address));
        }

        /// <summary>
        /// Retrieves all DNS IP addresses from active network interfaces.
        /// </summary>
        /// <returns>An enumerable collection of DNS IP addresses.</returns>
        public IEnumerable<IPAddress> DnsAddresses() {
            return IPAddresses(p => p.DnsAddresses);
        }

        /// <summary>
        /// Retrieves all gateway IP addresses from active network interfaces.
        /// </summary>
        /// <returns>An enumerable collection of gateway IP addresses.</returns>
        public IEnumerable<IPAddress> GatewayAddresses() {
            return IPAddresses(p => p.GatewayAddresses.Select(x => x.Address));
        }

        #endregion

        /// <summary>
        /// Retrieves IP addresses based on a specified property extractor function.
        /// </summary>
        /// <param name="ipExtractor">A function that extracts the desired IP address collection from IPInterfaceProperties.</param>
        /// <returns>An enumerable collection of IP addresses.</returns>
        private static IEnumerable<IPAddress> IPAddresses(Func<IPInterfaceProperties, IEnumerable<IPAddress>> ipExtractor) {
            // Query all network interfaces to extract the specified types of IP addresses
            // only from interfaces that are operational or have an unknown status.
            return from networkInterface in NetworkInterface.GetAllNetworkInterfaces()
                   where
                       networkInterface.OperationalStatus == OperationalStatus.Up ||
                       networkInterface.OperationalStatus == OperationalStatus.Unknown
                   let properties = networkInterface.GetIPProperties()
                   from address in ipExtractor(properties)
                   where address.AddressFamily == AddressFamily.InterNetwork
                      || address.AddressFamily == AddressFamily.InterNetworkV6
                   select address;
        }
    }
}