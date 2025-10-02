using TMPro;
using UnityEngine;

namespace com.onlineobject.objectnet.integration {
    public class UIHostIndicator : MonoBehaviour {

        // The prefab for an indicate that this instance is the current host
        public GameObject HostIndicator;


        public TextMeshProUGUI textComponent;

        // Update is called once per frame
        void LateUpdate() {
            if (this.HostIndicator != null) {
                this.HostIndicator.SetActive(NetworkSteamManager.Instance().IsHostInstance() ||
                                             NetworkSteamManager.Instance().IsReconnecting());
                if (NetworkSteamManager.Instance().IsHostInstance()) {
                    this.textComponent.SetText("Is Host");
                } else if (NetworkSteamManager.Instance().IsReconnecting()) {
                    this.textComponent.SetText("Reconnecting");
                }
            }
        }
    }
}