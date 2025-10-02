using UnityEngine;

namespace com.onlineobject.objectnet {
    public class GlassTransformUpdate : MonoBehaviour {

        public Transform origin;

        public Transform target;

        public AnimationCurve curve = AnimationCurve.Constant(0f, 1f, 1f);

        float ellapsed = 0f;

        void Update() {
            this.ellapsed += Time.deltaTime;
            this.transform.localPosition = Vector3.Lerp(this.origin.localPosition, this.target.localPosition, this.curve.Evaluate(ellapsed % 1.0f));            
        }
    }
}