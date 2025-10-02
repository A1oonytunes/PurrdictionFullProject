using System.Net;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A static class containing well-known network constants.
    /// </summary>
    internal static class WellKnownConstants {
        /// <summary>
        /// The IPv4 multicast address used for mDNS (Multicast DNS).
        /// </summary>
        public static readonly IPAddress IPv4MulticastAddress = IPAddress.Parse("239.255.255.250");

        /// <summary>
        /// The IPv6 link-local multicast address used for SSDP (Simple Service Discovery Protocol).
        /// </summary>
        public static readonly IPAddress IPv6LinkLocalMulticastAddress = IPAddress.Parse("FF02::C");

        /// <summary>
        /// The IPv6 site-local multicast address, which is now deprecated in favor of scoped addresses.
        /// </summary>
        public static readonly IPAddress IPv6LinkSiteMulticastAddress = IPAddress.Parse("FF05::C");

        /// <summary>
        /// The IPEndPoint for NAT-PMP (Network Address Translation Port Mapping Protocol) on the default gateway.
        /// </summary>
        public static readonly IPEndPoint NatPmpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.1"), 5351);
    }

}