using UnityEngine;

namespace com.onlineobject.objectnet {
    public class RemoteLoadSceneExample : MonoBehaviour {

        public string sceneToLoad = "ExampleSceneLoad";

        public GameObject finishedLabelObject;

        bool sceneLoadRequested = false;

        void Update() {
            if (this.sceneLoadRequested == false) {
                if (Input.GetKeyDown(KeyCode.L)) {
                    this.sceneLoadRequested = true;
                    // Load scene
                    NetworkManager.Instance().LoadSceneRemote(this.sceneToLoad, 
                                                              RemoteSceneLoadMode.LoadAfter, 
                                                              UnityEngine.SceneManagement.LoadSceneMode.Additive);
                }
            }
        }

        public void OnSceneLoadingFinished(string sceneName) {
            this.finishedLabelObject.SetActive(true);
        }

    }
}