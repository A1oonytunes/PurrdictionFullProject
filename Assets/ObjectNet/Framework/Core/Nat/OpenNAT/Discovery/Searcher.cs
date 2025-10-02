using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Abstract base class for implementing network address translation (NAT) device searchers.
    /// </summary>
    internal abstract class Searcher {
        // Holds the list of discovered NAT devices.
        private readonly List<NatDevice> _devices = new List<NatDevice>();

        // List of UDP clients used for sending and receiving discovery messages.
        protected List<UdpClient> UdpClients;

        // Event triggered when a new NAT device is found.
        public EventHandler<DeviceEventArgs> DeviceFound;

        // The time at which the next search is allowed to start.
        internal DateTime NextSearch = DateTime.UtcNow;

#if NET35
        /// <summary>
        /// Searches for NAT devices using synchronous approach for .NET 3.5.
        /// </summary>
        /// <param name="cancelationToken">Token to signal cancellation of the search.</param>
        /// <returns>A task representing the asynchronous operation, with a list of discovered NAT devices.</returns>
        public Task<IEnumerable<NatDevice>> Search(CancellationToken cancelationToken)
        {
            return Task.Factory.StartNew(_ =>
            {
                NatDiscoverer.TraceSource.LogInfo("Searching for: {0}", GetType().Name);
                while (!cancelationToken.IsCancellationRequested)
                {
                    Discover(cancelationToken);
                    Receive(cancelationToken);
                }
                CloseUdpClients();
            }, cancelationToken)
            .ContinueWith<IEnumerable<NatDevice>>((Task task) => _devices);
        }
#else
        /// <summary>
        /// Searches for NAT devices using asynchronous approach for .NET 4.0 and above.
        /// </summary>
        /// <param name="cancelationToken">Token to signal cancellation of the search.</param>
        /// <returns>A task representing the asynchronous operation, with a list of discovered NAT devices.</returns>
        public async Task<IEnumerable<NatDevice>> Search(CancellationToken cancelationToken) {
            await Task.Factory.StartNew(_ =>
            {
                NatDiscoverer.TraceSource.LogInfo("Searching for: {0}", GetType().Name);
                while (!cancelationToken.IsCancellationRequested) {
                    Discover(cancelationToken);
                    Receive(cancelationToken);
                }
                CloseUdpClients();
            }, null, cancelationToken);
            return _devices;
        }
#endif

        /// <summary>
        /// Initiates the discovery process for NAT devices.
        /// </summary>
        /// <param name="cancelationToken">Token to signal cancellation of the discovery.</param>
        private void Discover(CancellationToken cancelationToken) {
            if (DateTime.UtcNow < NextSearch)
                return;

            foreach (var socket in UdpClients) {
                try {
                    Discover(socket, cancelationToken);
                } catch (Exception e) {
                    NatDiscoverer.TraceSource.LogError("Error searching {0} - Details:", GetType().Name);
                    NatDiscoverer.TraceSource.LogError(e.ToString());
                }
            }
        }

        /// <summary>
        /// Receives responses from NAT devices.
        /// </summary>
        /// <param name="cancelationToken">Token to signal cancellation of the receiving process.</param>
        private void Receive(CancellationToken cancelationToken) {
            foreach (var client in UdpClients.Where(x => x.Available > 0)) {
                if (cancelationToken.IsCancellationRequested)
                    return;

                var localHost = ((IPEndPoint)client.Client.LocalEndPoint).Address;
                var receivedFrom = new IPEndPoint(IPAddress.None, 0);
                var buffer = client.Receive(ref receivedFrom);
                var device = AnalyseReceivedResponse(localHost, buffer, receivedFrom);

                if (device != null)
                    RaiseDeviceFound(device);
            }
        }

        /// <summary>
        /// Abstract method to be implemented for discovering NAT devices.
        /// </summary>
        /// <param name="client">The UDP client used for sending discovery messages.</param>
        /// <param name="cancelationToken">Token to signal cancellation of the discovery.</param>
        protected abstract void Discover(UdpClient client, CancellationToken cancelationToken);

        /// <summary>
        /// Abstract method to be implemented for analyzing the response received from NAT devices.
        /// </summary>
        /// <param name="localAddress">The local IP address of the UDP client.</param>
        /// <param name="response">The response received from the NAT device.</param>
        /// <param name="endpoint">The endpoint from which the response was received.</param>
        /// <returns>The NAT device if the response could be analyzed successfully; otherwise, null.</returns>
        public abstract NatDevice AnalyseReceivedResponse(IPAddress localAddress, byte[] response, IPEndPoint endpoint);

        /// <summary>
        /// Closes all UDP clients used in the discovery process.
        /// </summary>
        public void CloseUdpClients() {
            foreach (var udpClient in UdpClients) {
                udpClient.Close();
            }
        }

        /// <summary>
        /// Raises the DeviceFound event when a NAT device is discovered.
        /// </summary>
        /// <param name="device">The discovered NAT device.</param>
        private void RaiseDeviceFound(NatDevice device) {
            _devices.Add(device);
            var handler = DeviceFound;
            if (handler != null)
                handler(this, new DeviceEventArgs(device));
        }
    }

}