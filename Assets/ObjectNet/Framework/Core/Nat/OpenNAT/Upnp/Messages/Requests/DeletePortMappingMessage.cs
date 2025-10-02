using System.Collections.Generic;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a request message for deleting an existing port mapping.
    /// </summary>
    internal class DeletePortMappingRequestMessage : RequestMessageBase {
        // Holds the mapping information to be deleted.
        private readonly Mapping _mapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeletePortMappingRequestMessage"/> class.
        /// </summary>
        /// <param name="mapping">The mapping details to be used for the delete request.</param>
        public DeletePortMappingRequestMessage(Mapping mapping) {
            _mapping = mapping;
        }

        /// <summary>
        /// Converts the delete port mapping request into an XML representation suitable for UPnP communication.
        /// </summary>
        /// <returns>A dictionary representing the XML elements of the delete request.</returns>
        public override IDictionary<string, object> ToXml() {
            // Create a dictionary to hold the XML elements for the delete request.
            return new Dictionary<string, object>
            {
                // UPnP specifies an empty string for the remote host when deleting a mapping.
                {"NewRemoteHost", string.Empty},
                // The external port of the mapping to be deleted.
                {"NewExternalPort", _mapping.PublicPort},
                // The protocol of the mapping to be deleted, converted to the appropriate string representation.
                {"NewProtocol", _mapping.Protocol == Protocol.Tcp ? "TCP" : "UDP"}
            };
        }
    }

}