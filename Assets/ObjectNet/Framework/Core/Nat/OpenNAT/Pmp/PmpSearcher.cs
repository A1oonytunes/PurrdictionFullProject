using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a NAT-PMP (Network Address Translation Port Mapping Protocol) searcher.
    /// This class is responsible for discovering NAT devices that support NAT-PMP.
    /// </summary>
    internal class PmpSearcher : Searcher {
        // Provides IP addresses for gateway and DNS.
        private readonly IIPAddressesProvider _ipprovider;

        // Maps UDP clients to their corresponding gateway IP endpoints.
        private Dictionary<UdpClient, IEnumerable<IPEndPoint>> _gatewayLists;

        // Timeout for discovery attempts, in milliseconds.
        private int _timeout;

        /// <summary>
        /// Initializes a new instance of the PmpSearcher class.
        /// </summary>
        /// <param name="ipprovider">An IIPAddressesProvider instance for obtaining IP addresses.</param>
        internal PmpSearcher(IIPAddressesProvider ipprovider) {
            _ipprovider = ipprovider;
            _timeout = 250; // Initial timeout value.
            CreateSocketsAndAddGateways();
        }

        /// <summary>
        /// Creates UDP clients for each unicast address and associates them with gateway endpoints.
        /// </summary>
        private void CreateSocketsAndAddGateways() {
            UdpClients = new List<UdpClient>();
            _gatewayLists = new Dictionary<UdpClient, IEnumerable<IPEndPoint>>();

            try {
                // Get gateway addresses and convert them to endpoints with the PMP port.
                List<IPEndPoint> gatewayList = _ipprovider.GatewayAddresses()
                    .Select(ip => new IPEndPoint(ip, PmpConstants.ServerPort))
                    .ToList();

                // If no gateway addresses are found, try DNS addresses.
                if (!gatewayList.Any()) {
                    gatewayList.AddRange(
                        _ipprovider.DnsAddresses()
                            .Select(ip => new IPEndPoint(ip, PmpConstants.ServerPort)));
                }

                // If still no addresses are found, exit the method.
                if (!gatewayList.Any())
                    return;

                // Create a UDP client for each unicast address.
                foreach (IPAddress address in _ipprovider.UnicastAddresses()) {
                    UdpClient client;

                    try {
                        client = new UdpClient(new IPEndPoint(address, 0));
                    } catch (SocketException) {
                        continue; // Move on to the next address if socket creation fails.
                    }

                    // Add the client and associated gateway list to the dictionary.
                    _gatewayLists.Add(client, gatewayList);
                    UdpClients.Add(client);
                }
            } catch (Exception e) {
                NatDiscoverer.TraceSource.LogError("There was a problem finding gateways: " + e);
                // NAT-PMP does not use multicast, so there isn't really a good fallback.
            }
        }

        /// <summary>
        /// Discovers NAT-PMP compatible devices by sending a search message to the gateway endpoints.
        /// </summary>
        /// <param name="client">The UDP client used to send the discovery message.</param>
        /// <param name="cancelationToken">A token to observe while waiting for the discovery to complete.</param>
        protected override void Discover(UdpClient client, CancellationToken cancelationToken) {
            // Schedule the next search and double the timeout according to the spec.
            NextSearch = DateTime.UtcNow.AddMilliseconds(_timeout);
            _timeout *= 2;

            // If the timeout exceeds a threshold, reset it and schedule the next search.
            if (_timeout >= 3000) {
                _timeout = 250;
                NextSearch = DateTime.UtcNow.AddSeconds(10);
                return;
            }

            // Prepare the NAT-PMP search message.
            var buffer = new[] { PmpConstants.Version, PmpConstants.OperationExternalAddressRequest };
            foreach (IPEndPoint gatewayEndpoint in _gatewayLists[client]) {
                if (cancelationToken.IsCancellationRequested)
                    return;

                // Send the search message to the gateway endpoint.
                client.Send(buffer, buffer.Length, gatewayEndpoint);
            }
        }

        /// <summary>
        /// Checks if the given IP address is one of the search addresses.
        /// </summary>
        /// <param name="address">The IP address to check.</param>
        /// <returns>True if the address is a search address; otherwise, false.</returns>
        private bool IsSearchAddress(IPAddress address) {
            return _gatewayLists.Values.SelectMany(x => x)
                .Any(x => x.Address.Equals(address));
        }

        /// <summary>
        /// Analyzes the response received from a NAT device to determine if it is a valid NAT-PMP device.
        /// </summary>
        /// <param name="localAddress">The local IP address that received the response.</param>
        /// <param name="response">The response bytes from the NAT device.</param>
        /// <param name="endpoint">The endpoint from which the response was received.</param>
        /// <returns>A PmpNatDevice if the response is valid; otherwise, null.</returns>
        public override NatDevice AnalyseReceivedResponse(IPAddress localAddress, byte[] response, IPEndPoint endpoint) {
            // Validate the response based on the NAT-PMP protocol specifications.
            if (!IsSearchAddress(endpoint.Address)
                || response.Length != 12
                || response[0] != PmpConstants.Version
                || response[1] != PmpConstants.ServerNoop)
                return null;

            // Extract the error code from the response.
            int errorcode = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(response, 2));
            if (errorcode != 0)
                NatDiscoverer.TraceSource.LogError("Non zero error: {0}", errorcode);

            // Extract the public IP address from the response.
            var publicIp = new IPAddress(new[] { response[8], response[9], response[10], response[11] });
            // Reset the timeout for the next discovery.
            _timeout = 250;
            // Return a new PmpNatDevice based on the response.
            return new PmpNatDevice(localAddress, endpoint.Address, publicIp);
        }
    }

}