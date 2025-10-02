using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class GlobalListenerEventsByCode : MonoBehaviour {

        [SerializeField]
        private GameObject[] spawnedElement;

        [SerializeField]
        private Transform spawnCenter;

        private bool initialized = false;

        private const int SPAWN_ELEMENT     = 800001;

        private const int DESPAWN_ELEMENTS  = 800002;

        private List<GameObject> spawnedElements = new List<GameObject>();

        private readonly KeyCode[] action_keys = new KeyCode[9]{ KeyCode.Alpha1, 
                                                                 KeyCode.Alpha2,
                                                                 KeyCode.Alpha3,
                                                                 KeyCode.Alpha4, 
                                                                 KeyCode.Alpha5,
                                                                 KeyCode.Alpha6,
                                                                 KeyCode.Alpha7,
                                                                 KeyCode.Alpha8,
                                                                 KeyCode.Alpha9 };
        public void LateUpdate() {
            if (!this.initialized) {
                NetworkManager.RegisterEvent(SPAWN_ELEMENT, this.OnReceiveSpawnElement);
                NetworkManager.RegisterEvent(DESPAWN_ELEMENTS, this.OnReceiveDespawnElement);

                this.initialized = true;
            }

            // Key 1 to 9 will spawn object
            int indexOnKey = 0;
            foreach(KeyCode key in this.action_keys) {
                if ( Input.GetKeyDown(key) ) {
                    this.SendSpawnElement(indexOnKey);
                }
                indexOnKey++;
            }
            // Check if despawn was pressed ( C to Clean )
            if ( Input.GetKeyDown(KeyCode.C) ) {
                this.SendDespawnElements();
            }
        }

        private void OnReceiveSpawnElement(IDataStream reader) {
            int prefabIndex = reader.Read<int>();
            Vector3 spawnPosition = this.spawnCenter.position + (UnityEngine.Random.insideUnitSphere * 10);
            spawnPosition.y = this.spawnCenter.position.y;
            this.spawnedElements.Add(Instantiate(this.spawnedElement[prefabIndex], spawnPosition, Quaternion.identity));
            
        }

        private void OnReceiveDespawnElement(IDataStream reader) {
            while(this.spawnedElements.Count > 0) {
                GameObject obj = this.spawnedElements[0];
                this.spawnedElements.Remove(obj);
                Destroy(obj);
            }
            
        }

        private void SendSpawnElement(int spawnObjectIndex) {
            if (!NetworkManager.Instance().IsRunningLogic()) {
                using (DataStream writer = new DataStream()) {
                    writer.Write<int>(spawnObjectIndex);
                    NetworkManager.Instance().Send(SPAWN_ELEMENT, writer, DeliveryMode.Reliable); // Send event
                }
            } else {
                Vector3 spawnPosition = this.spawnCenter.position + (UnityEngine.Random.insideUnitSphere * 10);
                spawnPosition.y = this.spawnCenter.position.y;
                this.spawnedElements.Add(Instantiate(this.spawnedElement[spawnObjectIndex], spawnPosition, Quaternion.identity));
            }
        }

        private void SendDespawnElements() {
            if (!NetworkManager.Instance().IsRunningLogic()) {
                using (DataStream writer = new DataStream()) {
                    NetworkManager.Instance().Send(DESPAWN_ELEMENTS, writer, DeliveryMode.Reliable); // Send event
                }
            } else {
                while(this.spawnedElements.Count > 0) {
                    GameObject obj = this.spawnedElements[0];
                    this.spawnedElements.Remove(obj);
                    Destroy(obj);
                }
            }
        }
    }
}