using System;
using System.Xml;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// An abstract base class for handling response messages parsed as XML documents.
    /// </summary>
    internal abstract class ResponseMessageBase {
        // Holds the XML document representing the response message.
        private readonly XmlDocument _document;

        // Represents the service type namespace used in the XML document.
        protected string ServiceType;

        // The type name of the response message, used to identify the relevant XML node.
        private readonly string _typeName;

        /// <summary>
        /// Initializes a new instance of the ResponseMessageBase class.
        /// </summary>
        /// <param name="response">The XML document representing the response message.</param>
        /// <param name="serviceType">The namespace URI for the service type.</param>
        /// <param name="typeName">The type name of the response message.</param>
        protected ResponseMessageBase(XmlDocument response, string serviceType, string typeName) {
            _document = response;
            ServiceType = serviceType;
            _typeName = typeName;
        }

        /// <summary>
        /// Retrieves the XML node corresponding to the response message type.
        /// </summary>
        /// <returns>The XmlNode representing the message type.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the expected node is not found in the response.</exception>
        protected XmlNode GetNode() {
            // Create a namespace manager to handle namespaces in the XML document.
            var nsm = new XmlNamespaceManager(_document.NameTable);
            nsm.AddNamespace("responseNs", ServiceType);

            // Extract the message name from the type name by removing the "Message" suffix.
            string typeName = _typeName;
            string messageName = typeName.Substring(0, typeName.Length - "Message".Length);

            // Select the node corresponding to the message name using the namespace manager.
            XmlNode node = _document.SelectSingleNode("//responseNs:" + messageName, nsm);

            // If the node is not found, throw an exception indicating an invalid response.
            if (node == null)
                throw new InvalidOperationException("The response is invalid: " + messageName);

            return node;
        }
    }

}