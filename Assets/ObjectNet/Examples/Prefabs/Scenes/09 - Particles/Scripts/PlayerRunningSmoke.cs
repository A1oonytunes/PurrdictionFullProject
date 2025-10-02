using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class PlayerRunningSmoke : MonoBehaviour {

        [SerializeField]
        private ParticleSystem particles;

        [SerializeField]
        private float detectionSpeed = 2.0f;

        private ParticleSystem.EmissionModule emissor;

        private Vector3 previousPosition = Vector3.zero;

        private float speed = 0f;

        private float nextCheck = 0f;

        const float DETECTION_INTERVAL = 0.1f;

        void Start() {
            this.emissor = this.particles.emission;
            this.particles.Stop();
        }

        // Update is called once per frame
        void Update() {
            if ( this.speed > this.detectionSpeed ) {
                if (this.particles.isPlaying == false) {
                    this.particles.Simulate(0.1f);
                    this.particles.Play();
                }
            } else if (this.particles.isPlaying == true) {
                this.particles.Stop();
            }
        }

        public void FixedUpdate() {
            if (this.nextCheck < Time.time) {
                this.speed              = (this.previousPosition - this.transform.position).magnitude;
                this.previousPosition   = this.transform.position;
                this.nextCheck          = (Time.time + DETECTION_INTERVAL);
            }
        }
    }
}