using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class NetworkVariablesPlayer : NetworkBehaviour {

        [SerializeField]
        private Color playerColor = Color.white;

        private NetworkVariable<Color> networkPlayerColor = Color.white;

        private Renderer playerBodyRender;

        public void Start() {
            this.playerBodyRender = this.GetComponent<Renderer>();

            // You can assign any value directly to the variable and this value will be propagated to all connected clients
            this.networkPlayerColor = this.playerColor;

            // You can automatically syncronize network variables with some local variable
            this.networkPlayerColor.OnSynchonize(() => { return this.playerColor; },
                                                 (Color value) => { this.playerColor = value; });

            // You can also apply changes only when values was updated using "OnValueChange"
            this.networkPlayerColor.OnValueChange((Color oldColor, Color newColor) => {
                NetworkDebugger.Log(string.Format("Color was updated from [ {0} ] to [ {1} ] ", oldColor.ToString("F5"), newColor.ToString("F5")));
            });
        }

        public void Update() {
            this.playerBodyRender.material.SetColor("_BaseColor", this.playerColor);
        }
    }
}