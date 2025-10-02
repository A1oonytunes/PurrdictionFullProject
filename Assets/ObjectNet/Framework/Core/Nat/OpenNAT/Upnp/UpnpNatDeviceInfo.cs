using System;
using System.Net;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents UPnP NAT device information.
    /// </summary>
    internal class UpnpNatDeviceInfo {

        /// <summary>
        /// Gets the host endpoint of the UPnP device.
        /// </summary>
        public IPEndPoint HostEndPoint { get; private set; }

        /// <summary>
        /// Gets the local IP address of the UPnP device.
        /// </summary>
        public IPAddress LocalAddress { get; private set; }

        /// <summary>
        /// Gets the service type provided by the UPnP device.
        /// </summary>
        public string ServiceType { get; private set; }

        /// <summary>
        /// Gets the URI for the service control of the UPnP device.
        /// </summary>
        public Uri ServiceControlUri { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpnpNatDeviceInfo"/> class.
        /// </summary>
        /// <param name="localAddress">The local IP address of the UPnP device.</param>
        /// <param name="locationUri">The URI of the UPnP device description.</param>
        /// <param name="serviceControlUrl">The URL for the service control of the UPnP device.</param>
        /// <param name="serviceType">The type of service provided by the UPnP device.</param>
        public UpnpNatDeviceInfo(IPAddress localAddress, Uri locationUri, string serviceControlUrl, string serviceType) {
            LocalAddress = localAddress;
            ServiceType = serviceType;
            // Create an endpoint using the host and port from the location URI.
            HostEndPoint = new IPEndPoint(IPAddress.Parse(locationUri.Host), locationUri.Port);

            // Check if the service control URL is an absolute URI.
            if (Uri.IsWellFormedUriString(serviceControlUrl, UriKind.Absolute)) {
                var u = new Uri(serviceControlUrl);
                IPEndPoint old = HostEndPoint;
                serviceControlUrl = u.PathAndQuery;

                // Log the detection of an absolute URI and the updated host address.
                NatDiscoverer.TraceSource.LogInfo("{0}: Absolute URI detected. Host address is now: {1}", old, HostEndPoint);
                NatDiscoverer.TraceSource.LogInfo("{0}: New control url: {1}", HostEndPoint, serviceControlUrl);
            }

            // Build the full service control URI using the host and port from the location URI.
            var builder = new UriBuilder("http", locationUri.Host, locationUri.Port);
            ServiceControlUri = new Uri(builder.Uri, serviceControlUrl);
        }

    }

}