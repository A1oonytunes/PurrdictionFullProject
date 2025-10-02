using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.onlineobject.objectnet {
    public class PlayerSpawnChild : MonoBehaviour {

        public GameObject clientAccessPrefab;

        public GameObject fullAccessPrefab;

        public Transform spawnClientPosition;

        public Transform spawnFullPosition;

        public Text objectIdText;

        public Text playerIdText;

        public float interval = 1f;

        private float timeout = 0f;

        public void SpawnClientAccessPrefab() {
            if (this.timeout < Time.time) {
                this.timeout = (Time.time + this.interval);
                // Spawn projectile
                Vector3 playerPos = this.spawnClientPosition.position;
                Vector3 playerDirection = this.spawnClientPosition.forward;
                float spawnDistance = 10;
                Vector3 frontPos = playerPos + playerDirection * spawnDistance;
                Quaternion rocketRotation = Quaternion.LookRotation(frontPos - this.spawnClientPosition.position);
                NetworkGameObject.NetworkInstantiate(this.clientAccessPrefab, this.spawnClientPosition.position, rocketRotation);
            }
        }

        public void SpawnFullAccessPrefab() {
            if (this.timeout < Time.time) {
                this.timeout = (Time.time + this.interval);
                // Spawn projectile
                Vector3 playerPos = this.spawnFullPosition.position;
                Vector3 playerDirection = this.spawnFullPosition.forward;
                float spawnDistance = 10;
                Vector3 frontPos = playerPos + playerDirection * spawnDistance;
                Quaternion rocketRotation = Quaternion.LookRotation(frontPos - this.spawnFullPosition.position);
                NetworkGameObject.NetworkInstantiate(this.fullAccessPrefab, this.spawnFullPosition.position, rocketRotation);
            }
        }

        public void TakeObjectControl() {
            // Get player object
            int objectId = 0;
            Int32.TryParse(this.objectIdText.text, out objectId);
            if (objectId > 0) {
                if (NetworkManager.Container.IsRegistered(objectId)) {
                    NetworkElement cubeElement = (NetworkManager.Container.GetElement(objectId) as NetworkElement);
                    cubeElement.GetNetworkObject().TakeControl();
                }
            }
        }

        public void ReleaseObjectControl() {
            // Get player object
            int objectId = 0;
            Int32.TryParse(this.objectIdText.text, out objectId);
            if (objectId > 0) {
                if (NetworkManager.Container.IsRegistered(objectId)) {
                    NetworkElement cubeElement = (NetworkManager.Container.GetElement(objectId) as NetworkElement);
                    cubeElement.GetNetworkObject().ReleaseControl();
                }
            }
        }

        public void TranferObject() {
            int playerId = 0; // Get player object
            int objectId = 0; // Get player object
            Int32.TryParse(this.playerIdText.text, out playerId);
            Int32.TryParse(this.objectIdText.text, out objectId);
            if ((playerId > 0) && (objectId > 0)) {
                if ((NetworkManager.Container.IsRegistered(playerId)) &&
                    (NetworkManager.Container.IsRegistered(objectId))) {
                    // First i need to get play object
                    NetworkElement playerElement = (NetworkManager.Container.GetElement(playerId) as NetworkElement);
                    NetworkObject playerObject = playerElement.GetGameObject().GetComponent<NetworkObject>();
                    // Get network object
                    NetworkElement cubeElement = (NetworkManager.Container.GetElement(objectId) as NetworkElement);
                    // Then i need to get object
                    cubeElement.GetNetworkObject().TransferControl(playerObject);
                }
            }
        }
    }
}