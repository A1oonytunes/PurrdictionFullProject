using com.onlineobject.objectnet;
using UnityEngine;

public class TesteSpawmPause : MonoBehaviour
{
    public void LateUpdate() {
        if (Input.GetKeyDown(KeyCode.P)) {
            NetworkManager.Instance().DisableAutoLoadSceneElements();
            Debug.Log("NetworkManager.Instance().DisableAutoLoadSceneElements();");
        } else if (Input.GetKeyDown(KeyCode.R)) {
            NetworkManager.Instance().EnableAutoLoadSceneElements();
            Debug.Log("NetworkManager.Instance().EnableAutoLoadSceneElements();");
            NetworkManager.Instance().Enqueue(() => {
                NetworkManager.Instance().RequestRemoteSceneElements();
            }, 0.5f);

        } else if (Input.GetKeyDown(KeyCode.U)) {
            NetworkManager.Instance().DetectVariablesChanges();
            Debug.Log("NetworkManager.Instance().DetectVariablesChanges();");
        }
    }
}
