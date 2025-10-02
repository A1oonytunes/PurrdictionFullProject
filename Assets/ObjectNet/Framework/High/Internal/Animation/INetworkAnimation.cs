namespace com.onlineobject.objectnet {
    /// <summary>
    /// Define an interface for network animation control.
    /// </summary>
    public interface INetworkAnimation {

        /// <summary>
        /// Initializes the network animation system.
        /// This method should set up any necessary state or resources needed before animations can be played.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Plays an animation state on a specific layer.
        /// </summary>
        /// <param name="stateName">The name of the animation state to play.</param>
        /// <param name="layerIndex">The index of the layer where the animation will be played.</param>
        void Play(string stateName, int layerIndex);

        /// <summary>
        /// Plays an animation state on the default layer.
        /// </summary>
        /// <param name="stateName">The name of the animation state to play.</param>
        void Play(string stateName);

        /// <summary>
        /// CrossFade the specified animation state on the given layer.
        /// </summary>
        /// <param name="stateName">The layer on which to play the animation</param>
        /// <param name="layerIndex">The layer on which to play the animation.</param>
        /// <param name="normalizedTransitionDuration"></param>
        /// <param name="normalizedTimeOffset"></param>
        /// <param name="normalizedTransitionTime"></param>
        public void CrossFade(string stateName, int layerIndex, float normalizedTransitionDuration, float normalizedTimeOffset = float.NegativeInfinity, float normalizedTransitionTime = 0.0f);

        /// <summary>
        /// Overloaded CrossFade method to fade the specified animation state on the base layer.
        /// </summary>
        /// <param name="stateName">The layer on which to play the animation</param>
        /// <param name="normalizedTransitionDuration"></param>
        /// <param name="normalizedTimeOffset"></param>
        /// <param name="normalizedTransitionTime"></param>
        public void CrossFade(string stateName, float normalizedTransitionDuration, float normalizedTimeOffset = float.NegativeInfinity, float normalizedTransitionTime = 0.0f);

        /// <summary>
        /// CrossFadeInFixedTime the specified animation state on the given layer.
        /// </summary>
        /// <param name="stateName">The layer on which to play the animation</param>
        /// <param name="layerIndex">The layer on which to play the animation.</param>
        /// <param name="normalizedTransitionDuration"></param>
        /// <param name="normalizedTimeOffset"></param>
        /// <param name="normalizedTransitionTime"></param>
        public void CrossFadeInFixedTime(string stateName, int layerIndex, float normalizedTransitionDuration, float normalizedTimeOffset = float.NegativeInfinity, float normalizedTransitionTime = 0.0f);

        /// <summary>
        /// Overloaded CrossFadeInFixedTime method to fade the specified animation state on the base layer.
        /// </summary>
        /// <param name="stateName">The layer on which to play the animation</param>
        /// <param name="normalizedTransitionDuration"></param>
        /// <param name="normalizedTimeOffset"></param>
        /// <param name="normalizedTransitionTime"></param>
        public void CrossFadeInFixedTime(string stateName, float normalizedTransitionDuration, float normalizedTimeOffset = float.NegativeInfinity, float normalizedTransitionTime = 0.0f);
         

    }

}