namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the source types for a parameter.
    /// </summary>
    /// <remarks>
    /// The ParameterSourceType enum is used to distinguish between different origins
    /// of parameters within a system. It can be used to specify whether a parameter
    /// comes from a function or is an attribute of an object.
    /// </remarks>
    public enum ParameterSourceType {
        /// <summary>
        /// Indicates that the parameter source is a function.
        /// </summary>
        Function,

        /// <summary>
        /// Indicates that the parameter source is an attribute of an object.
        /// </summary>
        Attribute
    }

}