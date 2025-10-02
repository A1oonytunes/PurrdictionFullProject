using System.Collections.Generic;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a request message for retrieving a specific port mapping entry.
    /// </summary>
    internal class GetSpecificPortMappingEntryRequestMessage : RequestMessageBase {
        // The external port number for the port mapping.
        private readonly int _externalPort;

        // The protocol (TCP/UDP) for the port mapping.
        private readonly Protocol _protocol;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSpecificPortMappingEntryRequestMessage"/> class.
        /// </summary>
        /// <param name="protocol">The protocol (TCP/UDP) for the port mapping.</param>
        /// <param name="externalPort">The external port number for the port mapping.</param>
        public GetSpecificPortMappingEntryRequestMessage(Protocol protocol, int externalPort) {
            _protocol = protocol;
            _externalPort = externalPort;
        }

        /// <summary>
        /// Converts the request message to its XML representation.
        /// </summary>
        /// <returns>A dictionary representing the XML elements of the request message.</returns>
        public override IDictionary<string, object> ToXml() {
            return new Dictionary<string, object> {
                {"NewRemoteHost", string.Empty}, // The remote host in the port mapping (empty in this context).
                {"NewExternalPort", _externalPort}, // The external port number for the port mapping.
                {"NewProtocol", _protocol == Protocol.Tcp ? "TCP" : "UDP"} // The protocol for the port mapping, converted to string.
            };
        }
    }

}