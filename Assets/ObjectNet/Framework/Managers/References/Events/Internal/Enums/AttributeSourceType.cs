namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the source types for attributes.
    /// </summary>
    /// <remarks>
    /// This enumeration is used to specify the type of source from which an attribute's value is derived.
    /// </remarks>
    public enum AttributeSourceType {
        /// <summary>
        /// The attribute value is obtained from a function.
        /// </summary>
        Function,

        /// <summary>
        /// The attribute value is another attribute.
        /// </summary>
        Attribute,

        /// <summary>
        /// The attribute value is a string.
        /// </summary>
        String,

        /// <summary>
        /// The attribute value is an integer.
        /// </summary>
        Integer,

        /// <summary>
        /// The attribute value is a floating-point number with single precision.
        /// </summary>
        Float,

        /// <summary>
        /// The attribute value is a floating-point number with double precision.
        /// </summary>
        Double,

        /// <summary>
        /// The attribute value is a boolean.
        /// </summary>
        Boolean
    }

}