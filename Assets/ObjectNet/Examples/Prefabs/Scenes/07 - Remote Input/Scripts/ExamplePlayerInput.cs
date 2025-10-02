using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class ExamplePlayerInput : MonoBehaviour {

        public float speed = 1f;

        public float gravity = 9.8f;

        public Transform bottomPoint;

        private NetworkObject network;

        private void Update() {
            if (this.network == null) {
                this.network = GetComponent<NetworkObject>();
            }
            if (this.network.GetInput<bool>("left")) {
                this.transform.Translate(Vector3.left * Time.deltaTime * this.speed, Space.World);
            } else if (this.network.GetInput<bool>("right")) {
                this.transform.Translate(Vector3.right * Time.deltaTime * this.speed, Space.World);
            } else if (this.network.GetInput<bool>("up")) {
                this.transform.Translate(Vector3.forward * Time.deltaTime * this.speed, Space.World);
            } else if (this.network.GetInput<bool>("down")) {
                this.transform.Translate(Vector3.back * Time.deltaTime * this.speed, Space.World);
            }
        }

        void FixedUpdate() {
            RaycastHit hit;
            if (Physics.Raycast(this.bottomPoint.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity)) {
                if ( hit.distance > 0.01f ) {
                    this.transform.Translate(Vector3.down * Time.deltaTime * this.gravity, Space.World);
                }                
            }
        }

    }
}