using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a packet of information for an animation clip, including the clip itself,
    /// a count (possibly for reference counting or usage tracking), and details about the
    /// current state of the animation such as its hash and time position.
    /// </summary>
    public class AnimationClipPacket {

        /// <summary>
        /// Gets or sets the animation clip associated with this packet.
        /// </summary>
        /// <value>The animation clip.</value>
        public AnimationClip Clip { get; set; }

        /// <summary>
        /// Gets or sets the count associated with this animation clip.
        /// This could be used to track how many times the clip is used or referenced.
        /// </summary>
        /// <value>The count of how many times the animation clip is used or referenced.</value>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the hash of the current animation.
        /// The hash can be used to uniquely identify the animation clip for quick comparisons or lookups.
        /// </summary>
        /// <value>The hash code representing the current animation.</value>
        public int AnimationHash { get; set; }

        /// <summary>
        /// Gets or sets the current animation time position.
        /// This represents the current time within the animation clip's timeline.
        /// </summary>
        /// <value>The current time position within the animation clip.</value>
        public float AnimationTime { get; set; }
    }

}