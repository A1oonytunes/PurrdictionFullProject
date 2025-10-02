using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class PlayerShoot : NetworkBehaviour {

        public GameObject projectile;

        public Transform spawnPosition;

        public float interval = 1f;

        private float timeout = 0f;

        void Start() {
        }


        void Update() {
            if (Input.GetKey(KeyCode.F)) {
                if ( this.timeout < Time.time ) {
                    this.timeout = (Time.time + this.interval);
                    // Spawn projectile
                    Vector3     playerPos       = this.spawnPosition.position;
                    Vector3     playerDirection = this.spawnPosition.forward;
                    // Quaternion  playerRotation  = this.spawnPosition.rotation;
                    float       spawnDistance   = 10;
                    Vector3     frontPos        = playerPos + playerDirection * spawnDistance;
                    Quaternion  rocketRotation  = Quaternion.LookRotation(frontPos - this.spawnPosition.position);
                    NetworkGameObject.NetworkInstantiate(this.projectile, this.spawnPosition.position, rocketRotation);
                }
            } else if (Input.GetKey(KeyCode.G)) {
                if (this.timeout < Time.time) {
                    this.timeout = (Time.time + this.interval);

                    /**
                     * NOTE
                     * You can execute network methods with parameters by using, this option will select the best place to execute method ( Server or Client )
                     * 
                     *      this.NetworkExecute<Type1, Type2, Type3, ... TypeN>(MethodName, TypeValue1, TypeValue2, TypeValue3, ... TypeValueN);
                     * 
                     * You can also select if code shall be executed on each client 
                     * 
                     *      this.NetworkExecuteOnClient<Type1, Type2, Type3, ... TypeN>(MethodName, TypeValue1, TypeValue2, TypeValue3, ... TypeValueN);
                     * 
                     * Or executed on Server
                     * 
                     *      this.NetworkExecuteOnServer<Type1, Type2, Type3, ... TypeN>(MethodName, TypeValue1, TypeValue2, TypeValue3, ... TypeValueN);
                     * 
                     **/
                    this.NetworkExecute(this.Shoot);

                }
            }
        }

        public void Shoot() {
            Debug.LogError("Network Event executed");
        }

    }
}