using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class ControllerAnimationSync : MonoBehaviour {

        public GameObject targetController;

        private Animator animator;

        private Animator animatorTarget;

        private float previousNomalizedPosition = 0f;

        private Dictionary<int, AnimationClip> hashToClip = new Dictionary<int, AnimationClip>();

        const int LAYER_ANIMATION = 0;

        const string ANIMATION_BASE_LAYER = "Base Layer";

        void Start() {
            this.animator = this.GetComponent<Animator>();
            this.animatorTarget = this.targetController.GetComponent<Animator>();
            AnimatorClipInfo[] existentClipInfo = this.animator.GetCurrentAnimatorClipInfo(LAYER_ANIMATION);
            foreach (AnimatorClipInfo clip in existentClipInfo) {
                this.hashToClip.Add(Animator.StringToHash(String.Format("{0}.{1}", ANIMATION_BASE_LAYER, clip.clip.name)), clip.clip);
            }
        }

        void Update() {
            AnimatorStateInfo clipInfoOrigin = this.animator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo clipInfoTarget = this.animatorTarget.GetCurrentAnimatorStateInfo(0);
            if ((clipInfoTarget.fullPathHash != clipInfoOrigin.fullPathHash) ||
                (clipInfoOrigin.normalizedTime < this.previousNomalizedPosition)) {
                this.animatorTarget.Play(clipInfoOrigin.fullPathHash, LAYER_ANIMATION, clipInfoOrigin.normalizedTime);
            }
            this.previousNomalizedPosition = clipInfoOrigin.normalizedTime;
            AnimatorClipInfo[] existentClipInfo = this.animator.GetCurrentAnimatorClipInfo(LAYER_ANIMATION);
            if (existentClipInfo.Count() > 0) {
                foreach (AnimatorClipInfo clip in existentClipInfo) {
                    int animationHash = Animator.StringToHash(String.Format("{0}.{1}", ANIMATION_BASE_LAYER, clip.clip.name));
                    if (this.hashToClip.ContainsKey(animationHash) == false) {
                        this.hashToClip.Add(animationHash, clip.clip);
                    }
                }                
            }
        }

        private AnimationClip GetClipFromHash(int hash) {
            AnimationClip clip;
            if (hashToClip.TryGetValue(hash, out clip)) {
                return clip;
            } else {
                return null;
            }
        }

        private float GetCurrentAnimatorTime(Animator targetAnim, int layer = 0) {
            AnimatorStateInfo animState = targetAnim.GetCurrentAnimatorStateInfo(layer);
            int currentAnimHash = animState.fullPathHash;
            AnimationClip clip = GetClipFromHash(currentAnimHash);
            float currentTime = clip.length * animState.normalizedTime;
            return currentTime;
        }

        private bool IsAnimationPlaying(Animator targetAnim, int layer = 0) {
            AnimatorStateInfo animState = targetAnim.GetCurrentAnimatorStateInfo(layer);
            int currentAnimHash = animState.fullPathHash;
            AnimationClip clip = GetClipFromHash(currentAnimHash);
            if (targetAnim.GetCurrentAnimatorStateInfo(layer).IsName(clip.name) &&
                targetAnim.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1.0f)
                return true;
            else
                return false;
        }
    }
}