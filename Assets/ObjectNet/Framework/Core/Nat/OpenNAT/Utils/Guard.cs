using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A utility class providing guard clauses that can be used to protect methods from invalid input.
    /// </summary>
    internal class Guard {
        // Private constructor to prevent instantiation of this utility class.
        private Guard() {
        }

        /// <summary>
        /// Checks if a given integer parameter value is within a specified range.
        /// </summary>
        /// <param name="paramValue">The value of the parameter to check.</param>
        /// <param name="lowerBound">The inclusive lower bound of the valid range.</param>
        /// <param name="upperBound">The inclusive upper bound of the valid range.</param>
        /// <param name="paramName">The name of the parameter, used in the exception message if the check fails.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the parameter value is outside the specified range.</exception>
        internal static void IsInRange(int paramValue, int lowerBound, int upperBound, string paramName) {
            if (paramValue < lowerBound || paramValue > upperBound)
                throw new ArgumentOutOfRangeException(paramName);
        }

        /// <summary>
        /// Checks if a boolean expression is true.
        /// </summary>
        /// <param name="exp">The boolean expression to check.</param>
        /// <param name="paramName">The name of the parameter, used in the exception message if the check fails.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the expression is false.</exception>
        internal static void IsTrue(bool exp, string paramName) {
            if (!exp)
                throw new ArgumentOutOfRangeException(paramName);
        }

        /// <summary>
        /// Checks if an object is not null.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <param name="paramName">The name of the parameter, used in the exception message if the check fails.</param>
        /// <exception cref="ArgumentNullException">Thrown if the object is null.</exception>
        internal static void IsNotNull(object obj, string paramName) {
            if (obj == null)
                throw new ArgumentNullException(paramName);
        }
    }

}