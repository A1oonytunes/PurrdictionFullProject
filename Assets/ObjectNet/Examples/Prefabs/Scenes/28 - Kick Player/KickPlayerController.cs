using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class KickPlayerController : MonoBehaviour {
        void Update() {
            if (Input.GetKeyDown(KeyCode.K)) {
                if (NetworkManager.Instance().IsServerConnection()) {
                    IClient[] connectedClients = NetworkManager.Instance().GetConnectedClients<IClient>();
                    if (connectedClients.Length > 0) {
                        NetworkManager.Instance().DisconnectClient(connectedClients[0]);
                    } else {
                        Debug.LogError("There's no client to disconnect");
                    }
                } else {
                    Debug.LogError("Client can be disconnected by server");
                }
            }
        }
    }
}