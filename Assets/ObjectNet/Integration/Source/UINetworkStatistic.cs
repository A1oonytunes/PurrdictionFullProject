using UnityEngine;
using UnityEngine.UI;

namespace com.onlineobject.objectnet.integration {
    /// <summary>
    /// UINetworkStatistic is a MonoBehaviour that updates UI elements with network statistics.
    /// </summary>
    public class UINetworkStatistic : MonoBehaviour {

        /// <summary>
        /// Text component to display the public IP address.
        /// </summary>
        public Text PublicIp;

        /// <summary>
        /// Text component to display the local IP address.
        /// </summary>
        public Text LocalIp;

        /// <summary>
        /// Text component to display the network latency in milliseconds.
        /// </summary>
        public Text Latency;

        /// <summary>
        /// Text component to display the NAT status as "true" or "false".
        /// </summary>
        public Text NatStatus;

        /// <summary>
        /// LateUpdate is called every frame, if the Behaviour is enabled.
        /// It updates the UI elements with the latest network statistics.
        /// </summary>
        void LateUpdate() {
            // Check if the NetworkManager instance is not null
            if (NetworkManager.Instance() != null) {
                // Update the LocalIp text with the private IP address
                this.LocalIp.text = NetworkManager.Instance().GetPrivateIp();
                // Update the PublicIp text with the public IP address
                this.PublicIp.text = NetworkManager.Instance().GetPublicIp();
                // Update the Latency text with the average ping time in milliseconds
                this.Latency.text = string.Format("{0} ms", NetworkManager.Instance().GetPingAverage());
                // Update the NatStatus text with the router port mapping status ("true" or "false")
                this.NatStatus.text = NetworkManager.Instance().IsRouterPortMapped() ? "true" : "false";
            }
        }
    }

}