using System;
using System.Xml;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a response message for retrieving port mapping entry information.
    /// </summary>
    internal class GetPortMappingEntryResponseMessage : ResponseMessageBase {
        /// <summary>
        /// Gets the remote host for the port mapping entry.
        /// </summary>
        public string RemoteHost { get; private set; }

        /// <summary>
        /// Gets the external port for the port mapping entry.
        /// </summary>
        public int ExternalPort { get; private set; }

        /// <summary>
        /// Gets the protocol for the port mapping entry.
        /// </summary>
        public Protocol Protocol { get; private set; }

        /// <summary>
        /// Gets the internal port for the port mapping entry.
        /// </summary>
        public int InternalPort { get; private set; }

        /// <summary>
        /// Gets the internal client for the port mapping entry.
        /// </summary>
        public string InternalClient { get; private set; }

        /// <summary>
        /// Gets the status of whether the port mapping entry is enabled.
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// Gets the description of the port mapping entry.
        /// </summary>
        public string PortMappingDescription { get; private set; }

        /// <summary>
        /// Gets the lease duration for the port mapping entry.
        /// </summary>
        public int LeaseDuration { get; private set; }

        /// <summary>
        /// Initializes a new instance of the GetPortMappingEntryResponseMessage class with the specified parameters.
        /// </summary>
        /// <param name="response">The XML response document.</param>
        /// <param name="serviceType">The type of service.</param>
        /// <param name="genericMapping">A boolean value indicating whether the mapping is generic.</param>
        internal GetPortMappingEntryResponseMessage(XmlDocument response, string serviceType, bool genericMapping)
            : base(response, serviceType, genericMapping ? "GetGenericPortMappingEntryResponseMessage" : "GetSpecificPortMappingEntryResponseMessage") {
            XmlNode data = GetNode();

            RemoteHost = (genericMapping) ? data.GetXmlElementText("NewRemoteHost") : string.Empty;
            ExternalPort = (genericMapping) ? Convert.ToInt32(data.GetXmlElementText("NewExternalPort")) : ushort.MaxValue;
            if (genericMapping)
                Protocol = data.GetXmlElementText("NewProtocol").Equals("TCP", StringComparison.InvariantCultureIgnoreCase)
                               ? Protocol.Tcp
                               : Protocol.Udp;
            else
                Protocol = Protocol.Udp;

            InternalPort = Convert.ToInt32(data.GetXmlElementText("NewInternalPort"));
            InternalClient = data.GetXmlElementText("NewInternalClient");
            Enabled = data.GetXmlElementText("NewEnabled") == "1";
            PortMappingDescription = data.GetXmlElementText("NewPortMappingDescription");
            LeaseDuration = Convert.ToInt32(data.GetXmlElementText("NewLeaseDuration"));
        }
    }

}