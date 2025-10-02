using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Extension methods for the Animator class.
    /// </summary>
    public static class AnimatorExtensions {
        /// <summary>
        /// Plays the specified state on the animator.
        /// </summary>
        /// <param name="animator">The Animator to play the state on.</param>
        /// <param name="stateName">The name of the state to play.</param>
        public static void Play(this Animator animator, string stateName) {
            animator.Play(stateName);
        }
    }

}