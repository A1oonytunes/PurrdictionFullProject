using System.Xml;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a response message for adding a port mapping.
    /// </summary>
    internal class AddPortMappingResponseMessage : ResponseMessageBase {
        /// <summary>
        /// Initializes a new instance of the AddPortMappingResponseMessage class with the specified parameters.
        /// </summary>
        /// <param name="response">The XML document containing the response data.</param>
        /// <param name="serviceType">The service type associated with the response.</param>
        /// <param name="typeName">The type name associated with the response.</param>
        public AddPortMappingResponseMessage(XmlDocument response, string serviceType, string typeName) : base(response, serviceType, typeName) {
        }
    }

}
