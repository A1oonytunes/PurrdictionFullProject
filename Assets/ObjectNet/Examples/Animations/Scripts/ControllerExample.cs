using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class ControllerExample : MonoBehaviour {

        public string example1Trigger;

        public string example2Trigger;

        private Animator animator;

        void Start() {
            this.animator = this.GetComponent<Animator>();
        }

        void Update() {
            if (this.animator != null) {
                if (Input.GetKeyDown(KeyCode.A)) {
                    this.animator.SetTrigger(this.example1Trigger);
                }
                if (Input.GetKeyDown(KeyCode.S)) {
                    this.animator.SetTrigger(this.example2Trigger);
                }
            }
        }
    }
}