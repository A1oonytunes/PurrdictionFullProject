using System.Collections.Generic;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Base class for request messages.
    /// </summary>
    internal abstract class RequestMessageBase {
        /// <summary>
        /// Converts the request message to a dictionary representing XML data.
        /// </summary>
        /// <returns>A dictionary containing the XML data of the request message.</returns>
        public abstract IDictionary<string, object> ToXml();
    }
}