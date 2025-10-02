using System.Collections.Generic;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Struct AnimationInfoPacket is used to detect animations chnages over network
    /// </summary>
    public struct AnimationInfoPacket {

        /// <summary>
        /// Number of time that animation was played
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; set; }

        /// <summary>
        /// Hash of current animation
        /// </summary>
        /// <value>The animation hash.</value>
        public int AnimationHash { get; set; }

        /// <summary>
        /// Current animation time positon
        /// </summary>
        /// <value>The animation time.</value>
        public float AnimationTime { get; set; }

        /// <summary>
        /// Current speed of animation clip
        /// </summary>
        /// <value>The animation speed.</value>
        public float AnimationSpeed { get; set; }

        /// <summary>
        /// Current layer of animation clip
        /// </summary>
        /// <value>The animation layer.</value>
        public int AnimationLayer { get; set; }

        /// <summary>
        /// Sequence of plays
        /// </summary>
        /// <value>The play sequence.</value>
        public int PlaySequence { get; set; }

        /// <summary>
        /// Animation clip playting at layer
        /// </summary>
        /// <value>The playing animation clips.</value>
        public Dictionary<int, AnimationClipPacket> PlayingAnimationClips { get; set; }

        /// <summary>
        /// Animation parameters
        /// </summary>
        /// <value>The animation parameters.</value>
        public Dictionary<int, AnimationParamValues> AnimationParameters { get; set; }

    }
}