using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class NetworkVariablesPlayerExtended : NetworkBehaviour {

        [SerializeField]
        private Color playerColor = Color.white;
        
        private NetworkVariable<Color> networkPlayerColor = Color.white;

        private Renderer playerBodyRender;

        public void Start() {
            this.playerBodyRender = this.GetComponent<Renderer>();

            // You can also apply changes only when values was updated using "OnValueChange"
            this.networkPlayerColor.OnValueChange((Color oldColor, Color newColor) => {
                NetworkDebugger.Log(string.Format("Color was updated from [ {0} ] to [ {1} ] ", oldColor.ToString("F5"), newColor.ToString("F5")));
            });
        }

        public void Update() {
            this.playerBodyRender.material.SetColor("_BaseColor", this.playerColor); // Since variable is already updated you can apply direct instead of using "NetworkVariable"
        }

        /// <summary>
        /// This event is called when Network features starts
        /// 
        /// Note : This event is controlled internally by framework and ensure that 
        ///        network is up and running
        /// </summary>
        public override void OnNetworkStarted() { 
            // Here you can do any that you need after ensure that Network is initialized
        }

        /// <summary>
        /// Active Awake is executed when objects on Active mode Awake
        /// </summary>
        public void ActiveAwake() {
        }

        /// <summary>
        /// Active Awake is executed when objects on Passive mode Awake
        /// </summary>
        public void PassiveAwake() {
        }

        /// <summary>
        /// Active Start is executed when objects on Active mode Start
        /// </summary>
        public void ActiveStart() {
        }

        /// <summary>
        /// Passive Start is executed when objects on Passive mode Start
        /// </summary>
        public void PassiveStart() {
        }

        /// <summary>
        /// Active Update is executed every frame for Active object
        /// </summary>
        public void ActiveUpdate() {
            this.networkPlayerColor = this.playerColor; // On Active i'm going to update network color ( he will be send over network to the client's )
        }

        /// <summary>
        /// Passive Update is executed every frame for Passive object
        /// </summary>
        public void PassiveUpdate() {
            this.playerColor = this.networkPlayerColor; // On Passive mode i'm going to update local player variable ( to paint player body )
        }

        /// <summary>
        /// Active FixedUpdate is executed every physics frame for Active object
        /// </summary>
        public void ActiveFixedUpdate() {
        }

        /// <summary>
        /// Passive FixedUpdate is executed every physics frame for Passive object
        /// </summary>
        public void PassiveFixedUpdate() {
        }

        /// <summary>
        /// Active LateUpdate is executed every frame after Update for Active object
        /// </summary>
        public void ActiveLateUpdate() {
        }

        /// <summary>
        /// Passive LateUpdate is executed every frame after Update for Passive object
        /// </summary>
        public void PassiveLateUpdate() {
        }

    }
}