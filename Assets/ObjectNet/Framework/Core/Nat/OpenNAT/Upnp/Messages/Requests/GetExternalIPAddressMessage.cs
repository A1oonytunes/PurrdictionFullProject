using System.Collections.Generic;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a request message for retrieving the external IP address.
    /// </summary>
    internal class GetExternalIPAddressRequestMessage : RequestMessageBase {
        /// <summary>
        /// Converts the request message to an XML representation.
        /// </summary>
        /// <returns>A dictionary representing an empty XML structure, as this request does not require additional data.</returns>
        public override IDictionary<string, object> ToXml() {
            // Since this request does not require additional data, we return an empty dictionary.
            return new Dictionary<string, object>();
        }
    }
}