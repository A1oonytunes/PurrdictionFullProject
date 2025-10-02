using UnityEngine;
using com.onlineobject.objectnet;

public class TempRespawnScript : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    void Update()
    {
        if (playerTransform.position.y < -100)
        {
            // If this instance is the host/server
            if (NetworkManager.Instance().IsServerConnection())
            {
                // Respawn the local server player directly
                NetworkManager.Instance().RespawnServerPlayer();
            }
            else
            {
                // If this instance is a client, request respawn from server
                INetworkElement playerElement = NetworkManager.Instance().GetLocalPlayerElement<INetworkElement>();
                if (playerElement != null)
                {
                    NetworkManager.Instance().RequestPlayerRespawn(playerElement);
                }
            }
        }
    }
}