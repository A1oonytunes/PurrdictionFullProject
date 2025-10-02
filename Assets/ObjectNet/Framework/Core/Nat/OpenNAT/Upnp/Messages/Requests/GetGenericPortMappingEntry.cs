using System.Collections.Generic;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a request message to retrieve a generic port mapping entry.
    /// </summary>
    internal class GetGenericPortMappingEntry : RequestMessageBase {
        // Holds the index of the port mapping entry to retrieve.
        private readonly int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetGenericPortMappingEntry"/> class.
        /// </summary>
        /// <param name="index">The index of the port mapping entry to retrieve.</param>
        public GetGenericPortMappingEntry(int index) {
            _index = index;
        }

        /// <summary>
        /// Converts the request message to its XML representation.
        /// </summary>
        /// <returns>A dictionary representing the XML elements of the request message.</returns>
        public override IDictionary<string, object> ToXml() {
            // Create a dictionary to hold the XML representation of the request.
            return new Dictionary<string, object> {
                // Add the port mapping index to the dictionary with the appropriate XML element name.
                {"NewPortMappingIndex", _index}
            };
        }
    }

}