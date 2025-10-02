using System;
using System.Runtime.Serialization;
using System.Security.Permissions;


namespace com.onlineobject.objectnet {

    /// <summary>
    /// Define a custom exception class for mapping-related errors.
    /// </summary>
    [Serializable]
    public class MappingException : Exception {
        // Property to store the error code associated with the exception.
        public int ErrorCode { get; private set; }

        /// <summary>
        /// Property to store a descriptive text associated with the error code.
        /// </summary>
        public string ErrorText { get; private set; }

        /// <summary>
        // Constructors region to group all constructor overloads.
        /// <summary>
        #region Constructors

        /// <summary>
        // Default internal constructor.
        /// <summary>
        internal MappingException() {
        }

        /// <summary>
        // Internal constructor that takes a message string.
        /// <summary>
        internal MappingException(string message)
            : base(message) {
        }

        /// <summary>
        // Internal constructor that takes an error code and a descriptive text.
        // Initializes the base exception with a formatted message.
        /// <summary>
        internal MappingException(int errorCode, string errorText)
            : base(string.Format("Error {0}: {1}", errorCode, errorText)) {
            ErrorCode = errorCode;
            ErrorText = errorText;
        }

        /// <summary>
        // Internal constructor that takes a message string and an inner exception.
        /// <summary>
        internal MappingException(string message, Exception innerException)
            : base(message, innerException) {
        }

        /// <summary>
        // Protected constructor used for serialization.
        // Allows the MappingException to be serialized and deserialized.
        /// <summary>
        protected MappingException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

        #endregion

        /// <summary>
        // Method to serialize the exception data.
        // Requires security permission to ensure the caller has the right to serialize an object.
        /// <summary>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            // Validate the SerializationInfo argument.
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            // Serialize the error code and error text properties.
            info.AddValue("errorCode", ErrorCode);
            info.AddValue("errorText", ErrorText);

            // Call the base class to serialize its data.
            base.GetObjectData(info, context);
        }
    }

}