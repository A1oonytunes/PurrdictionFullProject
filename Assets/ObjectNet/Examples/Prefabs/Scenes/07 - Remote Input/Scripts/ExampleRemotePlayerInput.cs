using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class ExampleRemotePlayerInput : MonoBehaviour, IInputProvider {

        public string horizontalInputAxis = "Horizontal";

        public bool IsFrontPressed() {
            return Input.GetKey(KeyCode.UpArrow);
        }

        public bool IsBackPressed() {
            return Input.GetKey(KeyCode.DownArrow);
        }

        public bool IsLeftPressed() {
            return Input.GetKey(KeyCode.LeftArrow);
        }

        public bool IsRightPressed() {
            return Input.GetKey(KeyCode.RightArrow);
        }

    }
}