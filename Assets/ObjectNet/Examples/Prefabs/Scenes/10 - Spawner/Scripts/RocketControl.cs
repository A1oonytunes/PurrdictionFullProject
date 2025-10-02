using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class RocketControl : MonoBehaviour {

        public float forceAmount = 10f;

        public float delayToDespaw = 5f;

        public GameObject explosion;

        private Rigidbody body;

        private float destroyTime = 0f;

        // Start is called before the first frame update
        void Start() {
            this.body = this.GetComponent<Rigidbody>();
            this.destroyTime = (Time.time + this.delayToDespaw);
        }

        // Update is called once per frame
        void FixedUpdate() {
            this.body.AddForce(this.transform.TransformDirection(Vector3.forward) * this.forceAmount, ForceMode.Impulse);
            if ( this.destroyTime < Time.time ) {
                GameObject.Destroy(this.gameObject);
            }
        }

        void OnCollisionEnter(Collision collision) {
            Destroy(GameObject.Instantiate(this.explosion, collision.contacts[0].point, Quaternion.identity), 2f);
            GameObject.Destroy(this.gameObject);
        }
    }
}