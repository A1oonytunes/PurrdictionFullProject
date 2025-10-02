using System;
using System.Runtime.Serialization;

namespace com.onlineobject.objectnet {
    [Serializable]
    /// <summary>
    /// Represents errors that occur when a NAT device is not found.
    /// </summary>
    public class NatDeviceNotFoundException : Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="NatDeviceNotFoundException"/> class.
        /// </summary>
        public NatDeviceNotFoundException() {
            // Default constructor
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NatDeviceNotFoundException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NatDeviceNotFoundException(string message) : base(message) {
            // Constructor with a specified error message
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NatDeviceNotFoundException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public NatDeviceNotFoundException(string message, Exception innerException) : base(message, innerException) {
            // Constructor with a specified error message and a reference to the inner exception
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NatDeviceNotFoundException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected NatDeviceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) {
            // Constructor used for deserialization of the exception object
        }
    }

}