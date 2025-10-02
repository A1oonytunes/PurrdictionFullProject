using UnityEngine;

namespace com.onlineobject.objectnet {

    [BehaviourConfig(LoadOrder = NetworkStartOrder.OnConnectionStablished)]
    public class CubeOwnerShip : NetworkBehaviour {
        public Color activeColor;

        public Color passiveColor;

        private MeshRenderer render;

        private NetworkObject networkObject;

        private Vector3 offset;

        private float coordinates;

        private bool previousActive = false;

        private bool previousPassive = false;

        // Start is called before the first frame update
        void Start() {
            this.render = this.GetComponent<MeshRenderer>();
            this.networkObject = this.GetComponent<NetworkObject>();
        }

        private void OnGUI() {
            if (this.networkObject != null) {
                Vector3 labelCoordinate = Camera.main.WorldToScreenPoint(this.gameObject.transform.position);
                GUI.Label(new Rect(labelCoordinate.x, Screen.height - labelCoordinate.y, 100, 20), string.Format("ID : {0}", this.networkObject.GetNetworkId()));
            }
        }

        // Update is called once per frame
        void LateUpdate() {
            if (this.networkObject != null) {
                // Only when chnage
                if ((this.networkObject.IsActive()  != this.previousActive) ||
                    (this.networkObject.IsPassive() != this.previousPassive)) {
                    if (this.networkObject.IsActive()) {
                        this.render.material.SetColor("_BaseColor", this.activeColor);
                    } else if (this.networkObject.IsPassive()) {
                        this.render.material.SetColor("_BaseColor", this.passiveColor);
                    }
                    this.previousActive = this.networkObject.IsActive();
                    this.previousPassive = this.networkObject.IsPassive();
                }                
            } else {
                this.networkObject = this.GetComponent<NetworkObject>();
            }            
        }

        public void OnMouseDown() {
            this.coordinates = Camera.main.WorldToScreenPoint(this.gameObject.transform.position).z;
            this.offset = this.gameObject.transform.position - this.GetMouseWorldPos();
        }

        public void OnMouseDrag() {
            if (this.networkObject != null) {
                if (this.networkObject.IsActive()) {
                    this.transform.position = this.GetMouseWorldPos() + this.offset;
                }
            }
        }

        private Vector3 GetMouseWorldPos() {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = this.coordinates;
            return Camera.main.ScreenToWorldPoint(mousePoint);
        }
    }
}