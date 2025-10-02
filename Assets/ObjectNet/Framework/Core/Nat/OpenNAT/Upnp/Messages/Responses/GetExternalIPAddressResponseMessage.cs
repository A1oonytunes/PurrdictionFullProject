using System.Net;
using System.Xml;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a response message for retrieving the external IP address from a service.
    /// </summary>
    internal class GetExternalIPAddressResponseMessage : ResponseMessageBase {
        /// <summary>
        /// Gets the external IP address retrieved from the response.
        /// </summary>
        public IPAddress ExternalIPAddress { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetExternalIPAddressResponseMessage"/> class.
        /// </summary>
        /// <param name="response">The XML document containing the response data.</param>
        /// <param name="serviceType">The type of service that provided the response.</param>
        public GetExternalIPAddressResponseMessage(XmlDocument response, string serviceType)
            : base(response, serviceType, "GetExternalIPAddressResponseMessage") {
            // Extract the text content of the 'NewExternalIPAddress' XML element.
            string ip = GetNode().GetXmlElementText("NewExternalIPAddress");

            IPAddress ipAddr;
            // Try to parse the extracted text as an IP address.
            if (IPAddress.TryParse(ip, out ipAddr))
                ExternalIPAddress = ipAddr; // If parsing is successful, set the ExternalIPAddress property.
        }

    }

}