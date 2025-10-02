using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class InputProvider : MonoBehaviour, IInputProvider {
        
        public bool IsJumpPressed() {
            return Input.GetKey(KeyCode.Space);
        }

        public bool IsSprintPressed() {
            return Input.GetKey(KeyCode.LeftShift);
        }

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

        public Vector2 GetMovement() {
            return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
    }
}