using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Define an interface for network audio operations.
    /// </summary>
    public interface INetworkAudio {

        /// <summary>
        /// Initializes the network audio system.
        /// </summary>
        void Intialize();

        /// <summary>
        /// Plays the given audio clip over the network.
        /// </summary>
        /// <param name="audioClip">The audio clip to be played.</param>
        void Play(AudioClip audioClip);

    }

}