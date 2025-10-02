using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents metadata for an event, including its name, description, return type, parameter types, and execution side.
    /// This class is intended to be used as an attribute to provide additional information about events.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public class EventInformations : Attribute {

        /// <summary>
        /// The name of the event.
        /// </summary>
        private string eventName;

        /// <summary>
        /// A description of the event.
        /// </summary>
        private string eventDescriptiom;

        /// <summary>
        /// The return type of the event.
        /// </summary>
        private Type returnType;

        /// <summary>
        /// An array of types representing the parameters of the event.
        /// </summary>
        private Type[] parametersTypes;

        /// <summary>
        /// The side (client or server) where the event is expected to be executed.
        /// Defaults to server side.
        /// </summary>
        private EventReferenceSide executionSide = EventReferenceSide.ServerSide;

        /// <summary>
        /// Indicates whether the event returns void (no value).
        /// Defaults to true.
        /// </summary>
        private bool voidReturn = true;

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        public virtual String EventName {
            get { return eventName; }
            set { eventName = value; }
        }

        /// <summary>
        /// Gets or sets the description of the event.
        /// </summary>
        public virtual String EventDescriptiom {
            get { return eventDescriptiom; }
            set { eventDescriptiom = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the event returns void.
        /// </summary>
        public virtual bool VoidReturn {
            get { return voidReturn; }
            set { voidReturn = value; }
        }

        /// <summary>
        /// Gets or sets the return type of the event.
        /// </summary>
        public virtual Type ReturnType {
            get { return returnType; }
            set { returnType = value; }
        }

        /// <summary>
        /// Gets or sets the types of the parameters for the event.
        /// </summary>
        public virtual Type[] ParametersType {
            get { return parametersTypes; }
            set { parametersTypes = value; }
        }

        /// <summary>
        /// Gets or sets the execution side of the event (client or server).
        /// </summary>
        public virtual EventReferenceSide ExecutionSide {
            get { return executionSide; }
            set { executionSide = value; }
        }
    }

}