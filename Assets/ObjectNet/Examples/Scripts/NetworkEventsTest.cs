using UnityEngine;

namespace com.onlineobject.objectnet {
    public class NetworkEventsTest : NetworkBehaviour {

        void ActiveAwake() {
            Debug.Log(string.Format("ActiveAwake [{0}]", this.gameObject.name));
        }

        void PassiveAwake() {
            Debug.Log(string.Format("PassiveAwake [{0}]", this.gameObject.name));
        }

        void ActiveOnEnable() {
            Debug.Log(string.Format("ActiveOnEnable [{0}]", this.gameObject.name));
        }

        void PassiveOnEnable() {
            Debug.Log(string.Format("PassiveOnEnable [{0}]", this.gameObject.name));
        }

        void ActiveStart() {
            Debug.Log(string.Format("ActiveStart [{0}]", this.gameObject.name));
        }

        void PassiveStart() {
            Debug.Log(string.Format("PassiveStart [{0}]", this.gameObject.name));
        }

    }
}