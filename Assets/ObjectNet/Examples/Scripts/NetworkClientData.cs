using UnityEngine;

namespace com.onlineobject.objectnet {
    public class NetworkClientData : NetworkBehaviour {
        void LateUpdate() {
            NetworkObject obj = this.GetComponent<NetworkObject>();
            Debug.Log(string.Format("[{0}] NetworkID [{1}] PlayerID [{2}] Owner [{3}]", 
                      this.gameObject.name, 
                      obj.GetNetworkId(), 
                      obj.GetNetworkElement().GetPlayerId(), 
                      obj.IsOwner()));
        }
    }
}