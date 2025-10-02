using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Controls networked animations by synchronizing animation states across different clients.
    /// </summary>
    public class NetworkAnimationController : INetworkAnimation {

        // Reference to the network element that this controller will manage.
        private INetworkElement networkElement;

        // Animator component used to control animations.
        private Animator animator;

        const int DEFAULT_LAYER_ANIMATION = -1;

        /// <summary>
        /// Constructor for the NetworkAnimationController class.
        /// </summary>
        /// <param name="networkElement">The network element that contains the animations.</param>
        public NetworkAnimationController(INetworkElement networkElement) {
            this.networkElement = networkElement;
        }

        /// <summary>
        /// Initializes the NetworkAnimationController by setting up the animator and registering network events.
        /// </summary>
        public void Initialize() {
            // Check if the network element has an AnimationNetwork behavior and get the animator from it.
            if (this.networkElement.HasBehavior<AnimationNetwork>()) {
                this.animator = this.networkElement.GetBehavior<AnimationNetwork>().getAnimator();
            } else {
                // If not, find the Animator component in the children of the GameObject.
                this.animator = this.networkElement.GetGameObject().GetComponentInChildren<Animator>();
            }
            // Register a network event to handle animation play requests from other clients.
            if (this.networkElement.HasEvent(InternalGameEvents.AnimationPlay) == false) {
                this.networkElement.RegisterEvent(InternalGameEvents.AnimationPlay, OnReceiveAnimationPlay);
            }
            if (this.networkElement.HasEvent(InternalGameEvents.AnimationFade) == false) {
                this.networkElement.RegisterEvent(InternalGameEvents.AnimationFade, OnReceiveAnimationFade);
            }
            
        }

        /// <summary>
        /// Plays the specified animation state on the given layer.
        /// </summary>
        /// <param name="stateName">The name of the animation state to play.</param>
        /// <param name="layerIndex">The layer on which to play the animation.</param>
        public void Play(string stateName, int layerIndex) {
            // Initialize animation if has
            if (this.networkElement != null) {
                if (this.animator == null) {
                    this.Initialize();
                }
            }
            if (this.networkElement.IsActive()) {
                // Send a message to other clients to play the same animation.
                using (DataStream writer = new DataStream()) {
                    writer.Write(Animator.StringToHash(stateName));
                    writer.Write((short)layerIndex);
                    this.networkElement.Send(InternalGameEvents.AnimationPlay, writer, DeliveryMode.Reliable);
                }
            }                 
            // Play the animation locally.
            this.animator.Play(stateName, layerIndex);
        }

        /// <summary>
        /// Overloaded Play method to play the specified animation state on the base layer.
        /// </summary>
        /// <param name="stateName">The name of the animation state to play.</param>
        public void Play(string stateName) {
            // Initialize animation if has
            if (this.networkElement != null) {
                if (this.animator == null) {
                    this.Initialize();
                }
            }
            if (this.networkElement.IsActive()) {

                // Send a message to other clients to play the same animation on the base layer.
                using (DataStream writer = new DataStream()) {
                    writer.Write(Animator.StringToHash(stateName));
                    writer.Write((short)DEFAULT_LAYER_ANIMATION); // Zero is the base layer in Unity.
                    this.networkElement.Send(InternalGameEvents.AnimationPlay, writer, DeliveryMode.Reliable);
                }
            } 
            // Play the animation locally.
            this.animator.Play(stateName);
        }

        /// <summary>
        /// CrossFade the specified animation state on the given layer.
        /// </summary>
        /// <param name="stateName">The layer on which to play the animation</param>
        /// <param name="layerIndex">The layer on which to play the animation.</param>
        /// <param name="normalizedTransitionDuration">The duration of the transition (normalized).</param>
        /// <param name="normalizedTimeOffset">The time of the state (normalized).</param>
        /// <param name="normalizedTransitionTime">The time of the transition (normalized).</param>
        public void CrossFade(string stateName, int layerIndex, float normalizedTransitionDuration, float normalizedTimeOffset = float.NegativeInfinity, float normalizedTransitionTime = 0.0f) {
            // Initialize animation if has
            if (this.networkElement != null) {
                if (this.animator == null) {
                    this.Initialize();
                }
            }
            if (this.networkElement.IsActive()) {
                // Send a message to other clients to play the same animation on the base layer.
                using (DataStream writer = new DataStream()) {
                    writer.Write(Animator.StringToHash(stateName));
                    writer.Write((short)layerIndex);
                    writer.Write(normalizedTransitionDuration);
                    writer.Write(normalizedTimeOffset);
                    writer.Write((byte)Mathf.Abs(Mathf.RoundToInt(normalizedTransitionTime * 100f)));
                    writer.Write(false); // FadeInFixedTime ?
                    this.networkElement.Send(InternalGameEvents.AnimationFade, writer, DeliveryMode.Reliable);
                }
            }
            // Play the animation locally.
            this.animator.CrossFade(stateName, normalizedTransitionDuration, layerIndex, normalizedTimeOffset, normalizedTransitionTime);
            
        }

        /// <summary>
        /// Overloaded CrossFade method to fade the specified animation state on the base layer.
        /// </summary>
        /// <param name="stateName">The layer on which to play the animation</param>
        /// <param name="normalizedTransitionDuration">The duration of the transition (normalized).</param>
        /// <param name="normalizedTimeOffset">The time of the state (normalized).</param>
        /// <param name="normalizedTransitionTime">The time of the transition (normalized).</param>
        public void CrossFade(string stateName, float normalizedTransitionDuration, float normalizedTimeOffset = float.NegativeInfinity, float normalizedTransitionTime = 0.0f) {
            // Initialize animation if has
            if (this.networkElement != null) {
                if (this.animator == null) {
                    this.Initialize();
                }
            }
            if (this.networkElement.IsActive()) {
                // Send a message to other clients to play the same animation on the base layer.
                using (DataStream writer = new DataStream()) {
                    writer.Write(Animator.StringToHash(stateName));
                    writer.Write((short)DEFAULT_LAYER_ANIMATION); // Zero is the base layer in Unity.
                    writer.Write(normalizedTransitionDuration);
                    writer.Write(normalizedTimeOffset);
                    writer.Write((byte)Mathf.Abs(Mathf.RoundToInt(normalizedTransitionTime * 100f)));
                    writer.Write(false); // FadeInFixedTime ?
                    this.networkElement.Send(InternalGameEvents.AnimationFade, writer, DeliveryMode.Reliable);
                }
            }                
            // Play the animation locally.
            this.animator.CrossFade(stateName, normalizedTransitionDuration, DEFAULT_LAYER_ANIMATION, normalizedTimeOffset, normalizedTransitionTime);
        }

        /// <summary>
        /// CrossFadeInFixedTime the specified animation state on the given layer.
        /// </summary>
        /// <param name="stateName">The layer on which to play the animation</param>
        /// <param name="layerIndex">The layer on which to play the animation.</param>
        /// <param name="fixedTransitionDuration">The duration of the transition (in seconds).</param>
        /// <param name="fixedTimeOffset">The time of the state (in seconds).</param>
        /// <param name="normalizedTransitionTime">The time of the transition (normalized).</param>
        public void CrossFadeInFixedTime(string stateName, int layerIndex, float fixedTransitionDuration, float fixedTimeOffset = float.NegativeInfinity, float normalizedTransitionTime = 0.0f) {
            // Initialize animation if has
            if (this.networkElement != null) {
                if (this.animator == null) {
                    this.Initialize();
                }
            }
            if (this.networkElement.IsActive()) {
                // Send a message to other clients to play the same animation on the base layer.
                using (DataStream writer = new DataStream()) {
                    writer.Write(Animator.StringToHash(stateName));
                    writer.Write((short)layerIndex);
                    writer.Write(fixedTransitionDuration);
                    writer.Write(fixedTimeOffset);
                    writer.Write((byte)Mathf.Abs(Mathf.RoundToInt(normalizedTransitionTime * 100f)));
                    writer.Write(true); // FadeInFixedTime ?
                    this.networkElement.Send(InternalGameEvents.AnimationFade, writer, DeliveryMode.Reliable);
                }
            }
            // Play the animation locally.
            this.animator.CrossFadeInFixedTime(stateName, fixedTransitionDuration, layerIndex, fixedTimeOffset, normalizedTransitionTime);
        }

        /// <summary>
        /// Overloaded CrossFadeInFixedTime method to fade the specified animation state on the base layer.
        /// </summary>
        /// <param name="stateName">The layer on which to play the animation</param>
        /// <param name="fixedTransitionDuration">The duration of the transition (in seconds).</param>
        /// <param name="fixedTimeOffset">The time of the state (in seconds).</param>
        /// <param name="normalizedTransitionTime">The time of the transition (normalized).</param>
        public void CrossFadeInFixedTime(string stateName, float fixedTransitionDuration, float fixedTimeOffset = float.NegativeInfinity, float normalizedTransitionTime = 0.0f) {
            // Initialize animation if has
            if (this.networkElement != null) {
                if (this.animator == null) {
                    this.Initialize();
                }
            }
            if (this.networkElement.IsActive()) {
                // Send a message to other clients to play the same animation on the base layer.
                using (DataStream writer = new DataStream()) {
                    writer.Write(Animator.StringToHash(stateName));
                    writer.Write((short)DEFAULT_LAYER_ANIMATION); // Zero is the base layer in Unity.
                    writer.Write(fixedTransitionDuration);
                    writer.Write(fixedTimeOffset);
                    writer.Write((byte)Mathf.Abs(Mathf.RoundToInt(normalizedTransitionTime * 100f)));
                    writer.Write(true); // FadeInFixedTime ?
                    this.networkElement.Send(InternalGameEvents.AnimationFade, writer, DeliveryMode.Reliable);
                }
            }

            // Play the animation locally.
            this.animator.CrossFadeInFixedTime(stateName, fixedTransitionDuration, DEFAULT_LAYER_ANIMATION, fixedTimeOffset, normalizedTransitionTime);       
        }

        /// <summary>
        /// Event handler that is triggered when a passive client receives an animation play request.
        /// </summary>
        /// <param name="reader">The data stream containing the animation state and layer information.</param>
        private void OnReceiveAnimationPlay(IDataStream reader) {
            int animation = reader.Read<int>();
            int layerToPlay = reader.Read<short>();

            //broadcast the data to other clients
            if (NetworkManager.Instance().IsRunningLogic()){
                  IClient clientOrigin = (reader as INetworkStream).GetClient();
                  foreach (NetworkClient client in NetworkManager.Instance().GetConnectedClients<IClient>()) {
                        if (clientOrigin != client) {
                            if (NetworkManager.Instance().InRelayMode()) {
                                client.Transmit(reader.GetBuffer(), DeliveryMode.Reliable);
                            } else {
                                client.Send(reader.GetBuffer(), DeliveryMode.Reliable);
                            }                            
                        }
                  } 
            }
            // Play the received animation on the specified layer.
            if (this.networkElement != null){
                if (this.animator == null){
                    this.Initialize();
                }
            } 
            this.animator.Play(animation, layerToPlay);
        }

        /// <summary>
        /// Event handler that is triggered when a passive client receives an animation crossfade request.
        /// </summary>
        /// <param name="reader">The data stream containing the animation state and layer information.</param>
        private void OnReceiveAnimationFade(IDataStream reader) {
            int animation = reader.Read<int>();
            int layerToPlay = reader.Read<short>();
            float normalizedTransitionDuration = reader.Read<float>();
            float normalizedTimeOffset = reader.Read<float>();
            float normalizedTransitionTime = reader.Read<byte>() / 100f;
            bool fadeInFixedTime = reader.Read<bool>();

            //broadcast the data to other clients
            if (NetworkManager.Instance().IsRunningLogic()){
                  IClient clientOrigin = (reader as INetworkStream).GetClient();
                  foreach (NetworkClient client in NetworkManager.Instance().GetConnectedClients<IClient>()) {
                        if (clientOrigin != client) {
                            if (NetworkManager.Instance().InRelayMode()) {
                                client.Transmit(reader.GetBuffer(), DeliveryMode.Reliable);
                            } else {
                                client.Send(reader.GetBuffer(), DeliveryMode.Reliable);
                            }                            
                        }
                  } 
            }
            // Fade animation on the specified layer.
            if (this.networkElement != null){
                if (this.animator == null){
                    this.Initialize();
                }
            }
            if (fadeInFixedTime) {
                this.animator.CrossFadeInFixedTime(animation, normalizedTransitionDuration, layerToPlay, normalizedTimeOffset, normalizedTransitionTime);
            } else {
                this.animator.CrossFade(animation, normalizedTransitionDuration, layerToPlay, normalizedTimeOffset, normalizedTransitionTime);
            }
        }
    }

}