using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Controls the networked audio playback for a network element.
    /// </summary>
    public class NetworkAudioController : INetworkAudio {

        // Reference to the network element that this audio controller is associated with.
        private INetworkElement networkElement;

        // The AudioSource component used for playing audio.
        private AudioSource audioSource;

        /// <summary>
        /// Constructor for the NetworkAudioController.
        /// </summary>
        /// <param name="networkElement">The network element that this controller will manage audio for.</param>
        public NetworkAudioController(INetworkElement networkElement) {
            this.networkElement = networkElement;
        }

        /// <summary>
        /// Initializes the audio source and registers network events.
        /// </summary>
        public void Intialize() {
            // Attempt to get an AudioSource component attached to the network element's GameObject.
            if (this.networkElement.GetGameObject().GetComponent<AudioSource>()) {
                this.audioSource = this.networkElement.GetGameObject().GetComponent<AudioSource>();
            } else {
                // If no AudioSource is attached directly, try to find one in the children.
                this.audioSource = this.networkElement.GetGameObject().GetComponentInChildren<AudioSource>();
                // If still not found, try to get the AudioSource from the main camera.
                if (this.audioSource == null && Camera.main != null) {
                    this.audioSource = Camera.main.GetComponent<AudioSource>();
                }
            }
            // Register a network event to handle audio play requests from other clients.
            if (this.networkElement.HasEvent(InternalGameEvents.AudioPlay) == false) {
                this.networkElement.RegisterEvent(InternalGameEvents.AudioPlay, OnReceiveAudioPlay);
            }
        }

        /// <summary>
        /// Plays the given audio clip and sends a network event to other clients to play the same audio.
        /// </summary>
        /// <param name="audioClip">The audio clip to play.</param>
        public void Play(AudioClip audioClip) {
            // Check if the network element is active before playing audio.
            if (this.networkElement.IsActive()) {
                // Set the audio clip and play it.
                this.audioSource.clip = audioClip;
                this.audioSource.Play();
                // Send a network event with the audio clip name to other players.
                using (DataStream writer = new DataStream()) {
                    writer.Write(audioClip.name);
                    this.networkElement.Send(InternalGameEvents.AnimationPlay, writer, DeliveryMode.Reliable);
                }
            } else if (this.networkElement.IsPassive()) {
                // If the network element is passive, just play the audio without sending a network event.
                this.audioSource.clip = audioClip;
                this.audioSource.Play();
            }
        }

        /// <summary>
        /// Handles the reception of a network event to play an audio clip.
        /// </summary>
        /// <param name="reader">The data stream containing the audio clip information.</param>
        private void OnReceiveAudioPlay(IDataStream reader) {
            // Read the audio clip file name from the data stream.
            string audioClipFile = reader.Read<string>();
            // Load the audio clip from resources.
            AudioClip clipToPlay = Resources.Load<AudioClip>(audioClipFile);
            // If the clip is found, play it.
            if (clipToPlay != null) {
                this.Play(clipToPlay);
            }
        }
    }

}