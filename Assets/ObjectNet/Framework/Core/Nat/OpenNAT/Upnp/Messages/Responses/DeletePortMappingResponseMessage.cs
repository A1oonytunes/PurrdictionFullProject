using System.Xml;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a response message for a request to delete a port mapping.
    /// This class inherits from the ResponseMessageBase class and is specific to the action of deleting a port mapping.
    /// </summary>
    internal class DeletePortMappingResponseMessage : ResponseMessageBase {

        /// <summary>
        /// Initializes a new instance of the DeletePortMappingResponseMessage class with the specified response data.
        /// </summary>
        /// <param name="response">The XML document containing the response data from the UPnP service.</param>
        /// <param name="serviceType">The type of service that sent the response.</param>
        /// <param name="typeName">The name of the type associated with the response message.</param>
        public DeletePortMappingResponseMessage(XmlDocument response, string serviceType, string typeName)
            : base(response, serviceType, typeName) {
            // The constructor is using the base class constructor to initialize the response message.
            // Additional initialization specific to the DeletePortMappingResponseMessage can be added here if needed.
        }
    }
}
