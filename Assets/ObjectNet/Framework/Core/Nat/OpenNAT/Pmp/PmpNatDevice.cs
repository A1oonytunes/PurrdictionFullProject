using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a NAT (Network Address Translation) device using the PMP (Port Mapping Protocol).
    /// </summary>
    internal sealed class PmpNatDevice : NatDevice {
        /// <summary>
        /// Gets the host endpoint associated with the NAT device.
        /// </summary>
        public override IPEndPoint HostEndPoint {
            get { return _hostEndPoint; }
        }

        /// <summary>
        /// Gets the local IP address associated with the NAT device.
        /// </summary>
        public override IPAddress LocalAddress {
            get { return _localAddress; }
        }

        // Backing field for the host endpoint.
        private readonly IPEndPoint _hostEndPoint;
        // Backing field for the local IP address.
        private readonly IPAddress _localAddress;
        // Backing field for the public IP address.
        private readonly IPAddress _publicAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="PmpNatDevice"/> class.
        /// </summary>
        /// <param name="hostEndPointAddress">The IP address of the host endpoint.</param>
        /// <param name="localAddress">The local IP address.</param>
        /// <param name="publicAddress">The public IP address.</param>
        internal PmpNatDevice(IPAddress hostEndPointAddress, IPAddress localAddress, IPAddress publicAddress) {
            _hostEndPoint = new IPEndPoint(hostEndPointAddress, PmpConstants.ServerPort);
            _localAddress = localAddress;
            _publicAddress = publicAddress;
        }

#if NET35
    /// <summary>
    /// Asynchronously creates a port mapping on the NAT device.
    /// </summary>
    /// <param name="mapping">The mapping to create.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override Task CreatePortMapAsync(Mapping mapping)
    {
        return InternalCreatePortMapAsync(mapping, true)
            .TimeoutAfter(TimeSpan.FromSeconds(4))
            .ContinueWith(t => RegisterMapping(mapping));
    }
#else
        /// <summary>
        /// Asynchronously creates a port mapping on the NAT device.
        /// </summary>
        /// <param name="mapping">The mapping to create.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task CreatePortMapAsync(Mapping mapping) {
            await InternalCreatePortMapAsync(mapping, true)
                .TimeoutAfter(TimeSpan.FromSeconds(4));
            RegisterMapping(mapping);
        }
#endif

#if NET35
    /// <summary>
    /// Asynchronously deletes a port mapping from the NAT device.
    /// </summary>
    /// <param name="mapping">The mapping to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override Task DeletePortMapAsync(Mapping mapping)
    {
        return InternalCreatePortMapAsync(mapping, false)
            .TimeoutAfter(TimeSpan.FromSeconds(4))
            .ContinueWith(t => UnregisterMapping(mapping));
    }
#else
        /// <summary>
        /// Asynchronously deletes a port mapping from the NAT device.
        /// </summary>
        /// <param name="mapping">The mapping to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task DeletePortMapAsync(Mapping mapping) {
            await InternalCreatePortMapAsync(mapping, false)
                .TimeoutAfter(TimeSpan.FromSeconds(4));
            UnregisterMapping(mapping);
        }
#endif

        /// <summary>
        /// Asynchronously retrieves all mappings from the NAT device.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="NotSupportedException">Thrown when the operation is not supported.</exception>
        public override Task<IEnumerable<Mapping>> GetAllMappingsAsync() {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Asynchronously retrieves the external IP address of the NAT device.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation and returns the external IP address.</returns>
        public override Task<IPAddress> GetExternalIPAsync() {
#if NET35
        return Task.Factory.StartNew(() => _publicAddress)
#else
            return Task.Run(() => _publicAddress)
#endif
                .TimeoutAfter(TimeSpan.FromSeconds(4));
        }

        /// <summary>
        /// Asynchronously retrieves a specific mapping from the NAT device.
        /// </summary>
        /// <param name="protocol">The protocol of the mapping to retrieve.</param>
        /// <param name="port">The port of the mapping to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="NotSupportedException">Thrown when the operation is not supported.</exception>
        public override Task<Mapping> GetSpecificMappingAsync(Protocol protocol, int port) {
            throw new NotSupportedException("NAT-PMP does not specify a way to get a specific port map");
        }

        // Conditional compilation for different .NET framework versions
#if NET35
    // Asynchronously creates or deletes a port mapping for .NET 3.5
    private Task<Mapping> InternalCreatePortMapAsync(Mapping mapping, bool create)
    {
        // Initialize the package to be sent for PMP operation
        var package = new List<byte>();

        // Add PMP version and operation code based on the protocol
        package.Add(PmpConstants.Version);
        package.Add(mapping.Protocol == Protocol.Tcp ? PmpConstants.OperationCodeTcp : PmpConstants.OperationCodeUdp);
        package.Add(0); // Reserved byte
        package.Add(0); // Reserved byte
        // Add the private port and public port (if creating) to the package
        package.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short) mapping.PrivatePort)));
        package.AddRange(
            BitConverter.GetBytes(create ? IPAddress.HostToNetworkOrder((short) mapping.PublicPort) : (short) 0));
        // Add the requested lifetime of the mapping
        package.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(mapping.Lifetime)));

        // Convert the package to a byte array for sending
        byte[] buffer = package.ToArray();
        // Initialize attempt counter and delay for retries
        int attempt = 0;
        int delay = PmpConstants.RetryDelay;

        // Create a UDP client for communication
        var udpClient = new UdpClient();
        // Start listening for the PMP response
        CreatePortMapListen(udpClient, mapping);

        // Begin sending the mapping request asynchronously
        Task task = Task.Factory.FromAsync<byte[], int, IPEndPoint, int>(
                    udpClient.BeginSend, udpClient.EndSend,
                    buffer, buffer.Length,
                    HostEndPoint,
                    null);

        // Retry sending the request with exponential backoff
        while (attempt < PmpConstants.RetryAttempts - 1)
        {
            task = task.ContinueWith(t =>
            {
                // If the task is faulted, log the error and throw an exception
                if (t.IsFaulted)
                {
                    string type = create ? "create" : "delete";
                    string message = String.Format("Failed to {0} portmap (protocol={1}, private port={2})",
                        type,
                        mapping.Protocol,
                        mapping.PrivatePort);
                    NatDiscoverer.TraceSource.LogError(message);
                    throw new MappingException(message, t.Exception);
                }

                // Retry sending the request
                return Task.Factory.FromAsync<byte[], int, IPEndPoint, int>(
                    udpClient.BeginSend, udpClient.EndSend,
                    buffer, buffer.Length,
                    HostEndPoint,
                    null);
            }).Unwrap();

            attempt++;
            delay *= 2;
            Thread.Sleep(delay);
        }

        // Close the UDP client and return the mapping after the final attempt
        return task.ContinueWith(t =>
        {
            udpClient.Close();
            return mapping;
        });
    }
#else
        /// <summary>
        // Asynchronously creates or deletes a port mapping for .NET 4.0 and above
        /// <summary>
        /// <returns>Mapped task</returns>
        private async Task<Mapping> InternalCreatePortMapAsync(Mapping mapping, bool create) {
            // Initialize the package to be sent for PMP operation
            var package = new List<byte>();

            // Add PMP version and operation code based on the protocol
            package.Add(PmpConstants.Version);
            package.Add(mapping.Protocol == Protocol.Tcp ? PmpConstants.OperationCodeTcp : PmpConstants.OperationCodeUdp);
            package.Add(0); // Reserved byte
            package.Add(0); // Reserved byte
                            // Add the private port and public port (if creating) to the package
            package.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mapping.PrivatePort)));
            package.AddRange(
                BitConverter.GetBytes(create ? IPAddress.HostToNetworkOrder((short)mapping.PublicPort) : (short)0));
            // Add the requested lifetime of the mapping
            package.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(mapping.Lifetime)));

            try {
                // Convert the package to a byte array for sending
                byte[] buffer = package.ToArray();
                // Initialize attempt counter and delay for retries
                int attempt = 0;
                int delay = PmpConstants.RetryDelay;

                // Create and use a UDP client within a using block for automatic disposal
                using (var udpClient = new UdpClient()) {
                    // Start listening for the PMP response
                    CreatePortMapListen(udpClient, mapping);

                    // Retry sending the request with exponential backoff
                    while (attempt < PmpConstants.RetryAttempts) {
                        await udpClient.SendAsync(buffer, buffer.Length, HostEndPoint);

                        attempt++;
                        delay *= 2;
                        Thread.Sleep(delay);
                    }
                }
            } catch (Exception e) {
                // Log the error and throw a MappingException if an error occurs
                string type = create ? "create" : "delete";
                string message = String.Format("Failed to {0} portmap (protocol={1}, private port={2})",
                                               type,
                                               mapping.Protocol,
                                               mapping.PrivatePort);
                NatDiscoverer.TraceSource.LogError(message);
                var pmpException = e as MappingException;
                throw new MappingException(message, pmpException);
            }

            // Return the mapping after the operation is complete
            return mapping;
        }
#endif
        /// <summary>
        // Listens for PMP responses and processes them
        /// <summary>
        private void CreatePortMapListen(UdpClient udpClient, Mapping mapping) {
            var endPoint = HostEndPoint;

            // Continuously listen for incoming PMP responses
            while (true) {
                // Receive data from the UDP client
                byte[] data = udpClient.Receive(ref endPoint);

                // Validate the length of the received data
                if (data.Length < 16)
                    continue;

                // Check if the PMP version matches
                if (data[0] != PmpConstants.Version)
                    continue;

                // Extract the operation code and determine the protocol
                var opCode = (byte)(data[1] & 127);

                var protocol = Protocol.Tcp;
                if (opCode == PmpConstants.OperationCodeUdp)
                    protocol = Protocol.Udp;

                // Extract result code, epoch, and port information from the response
                short resultCode = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 2));
                int epoch = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 4));

                short privatePort = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 8));
                short publicPort = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 10));

                var lifetime = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 12));

                // Check for errors in the response and throw an exception if necessary
                if (privatePort < 0 || publicPort < 0 || resultCode != PmpConstants.ResultCodeSuccess) {
                    var errors = new[]
                                     {
                                     "Success",
                                     "Unsupported Version",
                                     "Not Authorized/Refused (e.g. box supports mapping, but user has turned feature off)",
                                     "Network Failure (e.g. NAT box itself has not obtained a DHCP lease)",
                                     "Out of resources (NAT box cannot create any more mappings at this time)",
                                     "Unsupported opcode"
                                 };
                    throw new MappingException(resultCode, errors[resultCode]);
                }

                // If the lifetime is 0, the mapping was deleted
                if (lifetime == 0)
                    return;

                // Update the mapping with the public port, protocol, and expiration
                // TODO: verify that the private port+protocol are a match
                mapping.PublicPort = publicPort;
                mapping.Protocol = protocol;
                mapping.Expiration = DateTime.Now.AddSeconds(lifetime);
                return;
            }
        }
        /// <summary>
        /// Returns a string representation of the PmpNatDevice instance
        /// </summary>
        /// <returns>Local address information</returns>
        public override string ToString() {
            return String.Format("Local Address: {0}\nPublic IP: {1}\nLast Seen: {2}",
                                 HostEndPoint.Address, _publicAddress, LastSeen);
        }
    }
}