using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// Provides automatic port forwarding via UPNP (Universal Plug and Play) and PMP (Port Mapping Protocol).
    /// </summary>
    public class NatHelper : INatHelper {
        // Properties to store the TCP and UDP ports to be forwarded.
        public int TcpPort { get; set; }
        public int UdpPort { get; set; }

        // Properties to store the public and private IP addresses after successful port forwarding.
        public string PublicIp { get; private set; }
        public string PrivateIp { get; private set; }

        // Indicates whether the port forwarding was successful.
        public bool PortOpened { get; private set; }

        // The external IP address of the NAT device.
        public IPAddress ExternalIPAddress { get; private set; }

        // The current status of the port forwarding process.
        public ForwardingStatus ForwardingStatus { get; private set; } = ForwardingStatus.NotStarted;

        // Callbacks for success and failure of port forwarding.
        private Action onSucessCallback;
        private Action onFailCallback;

        /// <summary>
        /// Constructor for the NatHelper class.
        /// </summary>
        /// <param name="tcpPort">The TCP port to forward.</param>
        /// <param name="udpPort">The UDP port to forward.</param>
        public NatHelper(int tcpPort, int udpPort) : base() {
            this.TcpPort = tcpPort;
            this.UdpPort = udpPort;
        }

        /// <summary>
        /// Define tcp port used on nat service
        /// </summary>
        /// <param name="tcpPort">Tcp/Ip port</param>
        public void SetTcpPort(int tcpPort) {
            this.TcpPort = tcpPort;
        }

        /// <summary>
        /// Define udp port used on nat service
        /// </summary>
        /// <param name="udpPort">Udp port</param>
        public void SetUdpPort(int udpPort) {
            this.UdpPort = udpPort;
        }

        /// <summary>
        /// Checks if the port forwarding has been successful.
        /// </summary>
        /// <returns>True if forwarding succeeded, otherwise false.</returns>
        public bool IsForwarded() {
            return ForwardingStatus.Succeeded.Equals(this.ForwardingStatus);
        }

        /// <summary>
        /// Configures the router for port forwarding and sets up callbacks for success or failure.
        /// </summary>
        /// <param name="onSucess">Callback to invoke on success.</param>
        /// <param name="onFail">Callback to invoke on failure.</param>
        public void ConfigureRouter(Action onSucess, Action onFail) {
            this.onSucessCallback = onSucess;
            this.onFailCallback = onFail;
            this.Setup();
        }

        /// <summary>
        /// Initiates the setup process for port forwarding.
        /// </summary>
        private async void Setup() {
            try {
                ForwardingStatus = ForwardingStatus.InProgress;

                NatDiscoverer discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(10000);
                NatDevice device;
                try {
                    device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
                } catch (NatDeviceNotFoundException e) {
                    NetworkDebugger.Log(String.Format("No UPnP nor PnP compatible device found to port forward. {0}", e.Message));
                    ForwardingStatus = ForwardingStatus.Unavailable;
                    this.PortOpened = false;
                    onFailCallback?.Invoke();
                    return;
                }

                await FetchIPAddress(device);
                await Task.WhenAll(PortForwardFor(device, this.TcpPort, this.UdpPort));

                if (ForwardingStatus != ForwardingStatus.Failed) {
                    ForwardingStatus = ForwardingStatus.Succeeded;
                    var externalIp = await device.GetExternalIPAsync();
                    this.PublicIp = externalIp.ToString();
                    this.PrivateIp = device.LocalAddress.ToString();
                    this.PortOpened = true;
                    NetworkDebugger.Log(String.Format("UPnP configured successfully {0}", externalIp.ToString()));
                    onSucessCallback?.Invoke();
                } else {
                    this.PortOpened = false;
                    NetworkDebugger.Log("UPnP configure was failed");
                    onFailCallback?.Invoke();
                }
            } catch (Exception err) {
                this.PortOpened = false;
                ForwardingStatus = ForwardingStatus.Failed;
                NetworkDebugger.Log(String.Format("No UPNP nor PMP compatible device found to port forward. {0}", err.Message));
                onFailCallback?.Invoke();
            }
        }

        /// <summary>
        /// Fetches the external IP address of the NAT device.
        /// </summary>
        /// <param name="device">The NAT device to query.</param>
        private async Task FetchIPAddress(NatDevice device) {
            ExternalIPAddress = await device.GetExternalIPAsync();
        }

        /// <summary>
        /// Sets up port forwarding for the specified TCP and UDP ports on the NAT device.
        /// </summary>
        /// <param name="device">The NAT device to configure.</param>
        /// <param name="tcpPort">The TCP port to forward.</param>
        /// <param name="udpPort">The UDP port to forward.</param>
        private async Task PortForwardFor(NatDevice device, int tcpPort, int udpPort) {
            try {
                Task<Mapping> tcpMapped = device.GetSpecificMappingAsync(Protocol.Tcp, tcpPort);
                Task<Mapping> udpMapped = device.GetSpecificMappingAsync(Protocol.Udp, udpPort);

                await tcpMapped;
                await udpMapped;

                // Remove existing mappings if they exist.
                if (tcpMapped != null) {
                    await device.DeletePortMapAsync(new Mapping(Protocol.Tcp, tcpPort, tcpPort, 0, "ObjectNet TCP"));
                }
                if (udpMapped != null) {
                    await device.DeletePortMapAsync(new Mapping(Protocol.Udp, udpPort, udpPort, 0, "ObjectNet UDP"));
                }

                // Create new port mappings.
                Task tcpForward = device.CreatePortMapAsync(new Mapping(Protocol.Tcp, tcpPort, tcpPort, 0, "ObjectNet TCP"));
                Task udpForward = device.CreatePortMapAsync(new Mapping(Protocol.Udp, udpPort, udpPort, 0, "ObjectNet UDP"));

                await tcpForward;
                await udpForward;
                NetworkDebugger.Log(String.Format("Successfully forwarded ports for listener '{0}'.", "Device"));
            } catch (MappingException e) {
                NetworkDebugger.Log(String.Format("Failed to forward ports for listener '{0}' port TCP {1} UDP {2}.", "Device", tcpPort, udpPort));
                NetworkDebugger.LogError(e.Message);
                ForwardingStatus = ForwardingStatus.Failed;
            }
        }
    }

}