using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class SpawnPlayerController : MonoBehaviour {

        public GameObject manualPlayer;

        private bool spawned = false;

        void Update() {
            if (this.spawned == false) {
                if (Input.GetKeyDown(KeyCode.P)) {
                    this.spawned = true;
                    NetworkManager.Instance().SpawnPlayer();
                } else if (Input.GetKeyDown(KeyCode.O)) {
                    this.spawned = true;
                    NetworkManager.Instance().SpawnPlayer(this.manualPlayer);
                }
            }
        }
    }
}