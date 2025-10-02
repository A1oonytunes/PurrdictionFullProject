using com.onlineobject.objectnet;
using UnityEngine;

public class NetworkTest : MonoBehaviour
{
    public void OnPlayerOwnerDetected(INetworkElement element) {
        if (element.IsOwner()) {
            Debug.Log("I'm owner of this element " + element.GetNetworkId());
        }
    }
}
