using System;
using System.Collections.Generic;
using System.Linq;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a response message for a discovery protocol, encapsulating the headers of the message.
    /// </summary>
    class DiscoveryResponseMessage {
        // Dictionary to store message headers with case-insensitive keys.
        private readonly IDictionary<string, string> _headers;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryResponseMessage"/> class by parsing the raw message string.
        /// </summary>
        /// <param name="message">The raw message string containing headers separated by new lines.</param>
        public DiscoveryResponseMessage(string message) {
            // Split the message into lines using carriage return and line feed as separators.
            var lines = message.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Parse the headers starting from the second line, assuming the first line is the status line.
            var headers = from h in lines.Skip(1)
                          let c = h.Split(':') // Split each line into key and value by colon.
                          let key = c[0] // The key is the first part before the colon.
                          let value = c.Length > 1
                              ? string.Join(":", c.Skip(1).ToArray()) // Join the remaining parts as the value.
                              : string.Empty // If no colon is found, the value is empty.
                          select new { Key = key, Value = value.Trim() }; // Trim any whitespace from the value.

            // Convert the anonymous type collection to a dictionary with uppercase keys for case-insensitivity.
            _headers = headers.ToDictionary(x => x.Key.ToUpperInvariant(), x => x.Value);
        }

        /// <summary>
        /// Gets the header value for the specified key.
        /// </summary>
        /// <param name="key">The header key to look up.</param>
        /// <returns>The header value associated with the specified key.</returns>
        public string this[string key] {
            get { return _headers[key.ToUpperInvariant()]; } // Retrieve the value using an uppercase key.
        }
    }

}
