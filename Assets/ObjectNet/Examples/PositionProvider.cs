using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class PositionProvider : MonoBehaviour, IInformationProvider {

        [Header("This attribute will return value of \"GetSpawnPosition\" method")]
        public Vector3 PositionToSpawn;

        public Vector3 GetSpawnPosition() {
            return this.PositionToSpawn;
        }

        public Vector3 GetSpawnPositonToGroup() {
            return Vector3.zero;
        }
        
    }
}
