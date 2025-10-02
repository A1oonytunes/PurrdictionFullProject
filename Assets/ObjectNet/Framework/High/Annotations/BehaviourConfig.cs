using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Attribuite to be used to define the configuration of a Netgwork behaviour
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class BehaviourConfig : Attribute {

        /// <summary>
        /// The name of the loadOrder.
        /// </summary>
        private NetworkStartOrder loadOrder = NetworkStartOrder.OnModeAssigned;

        /// <summary>
        /// Gets or sets the name of the loadOrder.
        /// </summary>
        public virtual NetworkStartOrder LoadOrder {
            get { return this.loadOrder; }
            set { this.loadOrder = value; }
        }
    }

}