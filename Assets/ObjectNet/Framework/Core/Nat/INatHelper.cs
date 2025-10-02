using System;
using System.Net;
#if EMBEDDED_SERVER
using DarkRift.Server;
#endif


namespace com.onlineobject.objectnet {
    /// <summary>
    /// Provides automatic port forwarding via UPNP and PMP.
    /// </summary>
    public interface INatHelper
    {
        /// <summary>
        /// The external IP address of the server.
        /// </summary>
        /// <value>The external IP address of the server.</value>
        IPAddress ExternalIPAddress { get; }

        /// <summary>
        /// The current state of the remote router.
        /// </summary>
        /// <value>The current state of the remote router</value>
        ForwardingStatus ForwardingStatus { get; }

        /// <summary>
        /// Try to configure NAT route
        /// </summary>
        void ConfigureRouter(Action onSucess, Action onFail);

        /// <summary>
        /// Return if port was already forward
        /// </summary>
        bool IsForwarded();

        /// <summary>
        /// Define tcp port used on nat service
        /// </summary>
        /// <param name="tcpPort">Tcp/Ip port</param>
        void SetTcpPort(int tcpPort);

        /// <summary>
        /// Define udp port used on nat service
        /// </summary>
        /// <param name="udpPort">Udp port</param>
        void SetUdpPort(int udpPort);
    }
}
