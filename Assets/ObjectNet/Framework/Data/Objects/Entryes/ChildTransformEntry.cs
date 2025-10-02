using UnityEngine;

namespace com.onlineobject.objectnet {
    public class ChildTransformEntry {

        private ushort childIndex;

        private GameObject childObject;

        private Vector3 position;

        private Quaternion rotation;

        private Vector3 scale;

        // Flag if shall to synchronize position
        private bool syncPosition = false;

        // Flag if shall to synchronize rotation
        private bool syncRotation = false;

        // Flag if shall to synchronize scale
        private bool syncScale = false;

        public ChildTransformEntry(ushort index, GameObject child, bool position, bool rotation, bool scale) {
            this.childIndex     = index;
            this.childObject    = child;
            this.syncPosition   = position;
            this.syncRotation   = rotation;
            this.syncScale      = scale;
            this.position       = child.transform.localPosition;
            this.rotation       = child.transform.localRotation;
            this.scale          = child.transform.localEulerAngles;
        }

        public ushort GetChildIndex() {
            return this.childIndex;
        }

        public GameObject GetChildObject() {
            return this.childObject;
        }

        public Vector3 GetPosition() {
            return this.position;
        }

        public void SetPosition(Vector3 value) {
            this.position = value;
        }

        public Quaternion GetRotation() {
            return this.rotation;
        }

        public void SetRotation(Quaternion value) {
            this.rotation = value;
        }

        public Vector3 GetScale() {
            return this.scale;
        }

        public void SetScale(Vector3 value) {
            this.scale = value;
        }

        public bool IsToSyncPosition() {
            return this.syncPosition;
        }

        public bool IsToSyncRotation() {
            return this.syncRotation;
        }

        public bool IsToSyncScale() {
            return this.syncScale;
        }
    }
}