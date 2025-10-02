using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// Provides utility methods for network operations.
    /// </summary>
    public static class NetworkUtils {

        /// <summary>
        /// Retrieves the local IP address of the default network interface.
        /// </summary>
        /// <returns>A string representing the local IP address.</returns>
        public static String GetLocaIpAddress() {
            String result = "";
            // Attempt to get the default network interface
            NetworkInterface defaultAdapter = NetworkUtils.GetDefaultInterface();
            if (defaultAdapter != null) {
                // If a default network interface is found, get its IPv4 address
                result = (NetworkUtils.GetDefaultIPV4Address(defaultAdapter) != null) ? NetworkUtils.GetDefaultIPV4Address(defaultAdapter).MapToIPv4().ToString() : null;
            } else {
                // If no default network interface is found, iterate through all network interfaces
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var adapter in interfaces) {
                    // Check if the interface is either Ethernet or Wireless
                    if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet || adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) {
                        // Get the IPv4 address of the first suitable interface and break the loop
                        result = NetworkUtils.GetDefaultIPV4Address(adapter).MapToIPv4().ToString();
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the default network interface that is currently operational.
        /// </summary>
        /// <returns>The default <see cref="NetworkInterface"/>, or null if none is found.</returns>
        private static NetworkInterface GetDefaultInterface() {
            var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            foreach (var intf in interfaces) {
                // Skip non-operational interfaces
                if (intf.OperationalStatus != OperationalStatus.Up) {
                    continue;
                }
                // Skip loopback interfaces
                if (intf.NetworkInterfaceType == NetworkInterfaceType.Loopback) {
                    continue;
                }

                var properties = intf.GetIPProperties();
                // Skip interfaces without IP properties
                if (properties == null) {
                    continue;
                }
                var gateways = properties.GatewayAddresses;
                // Skip interfaces without a gateway
                if ((gateways == null) || (gateways.Count == 0)) {
                    continue;
                }
                var addresses = properties.UnicastAddresses;
                // Skip interfaces without unicast addresses
                if ((addresses == null) || (addresses.Count == 0)) {
                    continue;
                }
                // Return the first operational interface with a gateway and unicast addresses
                return intf;
            }
            return null;
        }

        /// <summary>
        /// Gets the default IPv4 address associated with a given network interface.
        /// </summary>
        /// <param name="intf">The network interface to inspect.</param>
        /// <returns>The IPv4 <see cref="IPAddress"/>, or null if none is found.</returns>
        private static IPAddress GetDefaultIPV4Address(NetworkInterface intf) {
            if (intf == null) {
                return null;
            }
            foreach (var address in intf.GetIPProperties().UnicastAddresses) {
                // Skip non-IPv4 addresses
                if (address.Address.AddressFamily != AddressFamily.InterNetwork) {
                    continue;
                }
                // Skip transient addresses
                if (address.IsTransient) {
                    continue;
                }
                // Return the first non-transient IPv4 address found
                return address.Address;
            }
            return null;
        }
    }



}