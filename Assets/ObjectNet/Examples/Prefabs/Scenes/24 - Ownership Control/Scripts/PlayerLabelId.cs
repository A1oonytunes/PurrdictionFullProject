using UnityEngine;

namespace com.onlineobject.objectnet {
    public class PlayerLabelId : MonoBehaviour {
        private NetworkObject networkObject;

        private void OnGUI() {
            if (this.networkObject != null) {
                Vector3 labelCoordinate = Camera.main.WorldToScreenPoint(this.gameObject.transform.position);
                GUI.Label(new Rect(labelCoordinate.x, Screen.height - labelCoordinate.y, 100, 20), string.Format("ID : {0}", this.networkObject.GetNetworkId()));
            } else {
                this.networkObject = this.GetComponent<NetworkObject>();
            }
        }
    }
}