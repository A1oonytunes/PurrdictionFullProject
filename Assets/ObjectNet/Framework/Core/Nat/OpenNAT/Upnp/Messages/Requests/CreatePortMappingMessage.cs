using System.Collections.Generic;
using System.Net;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a request message for creating a port mapping.
    /// </summary>
    internal class CreatePortMappingRequestMessage : RequestMessageBase {
        private readonly Mapping _mapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePortMappingRequestMessage"/> class with the specified mapping.
        /// </summary>
        /// <param name="mapping">The mapping to be used for creating the port mapping request.</param>
        public CreatePortMappingRequestMessage(Mapping mapping) {
            _mapping = mapping;
        }

        /// <summary>
        /// Converts the request message to an XML representation.
        /// </summary>
        /// <returns>A dictionary representing the XML representation of the request message.</returns>
        public override IDictionary<string, object> ToXml() {
            string remoteHost = _mapping.PublicIP.Equals(IPAddress.None)
                                    ? string.Empty
                                    : _mapping.PublicIP.ToString();

            return new Dictionary<string, object>
                       {
                       {"NewRemoteHost", remoteHost},
                       {"NewExternalPort", _mapping.PublicPort},
                       {"NewProtocol", _mapping.Protocol == Protocol.Tcp ? "TCP" : "UDP"},
                       {"NewInternalPort", _mapping.PrivatePort},
                       {"NewInternalClient", _mapping.PrivateIP},
                       {"NewEnabled", 1},
                       {"NewPortMappingDescription", _mapping.Description},
                       {"NewLeaseDuration", _mapping.Lifetime}
                   };
        }
    }
}