using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace com.onlineobject.objectnet {
    public class AnimationNetwork : NetworkEntity<AnimationInfoPacket, IDataStream> {

        /// <summary>
        /// Represents a sequence of animations with associated metadata.
        /// </summary>
        class AnimationSequence {
            /// <summary>
            /// Information about the animation, such as frames, timing, etc.
            /// </summary>
            public AnimationInfoPacket Animation;

            /// <summary>
            /// The start time of the animation sequence in seconds.
            /// </summary>
            public float AnimationStart;

            /// <summary>
            /// The total duration of the animation sequence in seconds.
            /// </summary>
            public float AnimationDuration;

            /// <summary>
            /// Flag indicating whether the animation is currently playing.
            /// </summary>
            public bool IsPlaying = false;
        }

        // Defines the synchronization type for the animations.
        private AnimationSyncType syncronizationType = AnimationSyncType.UseParameters;

        // Holds the current animation data.
        private AnimationInfoPacket animationData;

        // Holds the previous state of the animation data for comparison.
        private AnimationInfoPacket previousAnimationData;

        // Reference to the Animator component.
        private Animator animator;

        // Determines if the entity should automatically transition to idle.
        private bool autoIdle = false;

        // Indicates if the AnimationNetwork has been initialized.
        private bool initialized = false;

        // The number of layers in the Animator.
        private int layersCount = 0;

        // The default status of the animation.
        private string defaultStatus = "";

        // Hash of the current animation being played.
        private int currentAnimationHash = 0;

        // Duration of the current animation.
        private float currentAnimationDuration = 0f;

        // Time after which the entity should transition to idle.
        private float timeToStartIdle = 0f;

        // The current animation sequence being played.
        private AnimationSequence currentAnimation;

        // Queue of pending animations to be played.
        private Queue<AnimationSequence> pendingAnimations = new Queue<AnimationSequence>();

        // Maps animation hash codes to their respective AnimationClip objects.
        private Dictionary<int, AnimationClip> hashToClip = new Dictionary<int, AnimationClip>();

        // Maps parameter names to their respective AnimatorControllerParameter objects.
        private Dictionary<int, AnimatorControllerParameter> animatorParameters = new Dictionary<int, AnimatorControllerParameter>();

        // Maps every float parameter controlled by a curve ( to resolve warning )
        private Dictionary<int, bool> parameterControlledByCurve = new Dictionary<int, bool>();

        // Cached parameters to reduce the garbage generation
        private AnimatorControllerParameter[] cachedParameters = null;

        // Cached clips to reduce the garbage generation
        private AnimationClip[] cachedClips = null;

        // Constants for layer and base layer naming.
        const int LAYER_ANIMATION = 0;

        // Layer base of animations
        const string ANIMATION_BASE_LAYER = "Base Layer";

        // Constant delay time before starting idle animation.
        const float DELAY_TO_START_IDLE = 2f;

        /// <summary>
        /// Default constructor for the AnimationNetwork.
        /// </summary>
        public AnimationNetwork() : base() {
            this.InitializeAnimationData();
        }

        /// <summary>
        /// Constructor for the AnimationNetwork with a specified network object.
        /// </summary>
        /// <param name="networkObject">The network object to associate with this AnimationNetwork.</param>
        public AnimationNetwork(INetworkElement networkObject) : base(networkObject) {
            this.InitializeAnimationData();
        }

        /// <summary>
        /// Sets the synchronization mode for the AnimationNetwork.
        /// </summary>
        /// <param name="syncronizationMode">The synchronization mode to set.</param>
        /// <returns>The current instance of AnimationNetwork for method chaining.</returns>
        public AnimationNetwork SetSyncronizationMode(AnimationSyncType syncronizationMode) {
            this.syncronizationType = syncronizationMode;
            return this;
        }

        /// <summary>
        /// Sets the number of layers in the Animator.
        /// </summary>
        /// <param name="layerCount">The number of layers to set.</param>
        /// <returns>The current instance of AnimationNetwork for method chaining.</returns>
        public AnimationNetwork SetLayerCount(int layerCount) {
            this.layersCount = layerCount;
            return this;
        }

        /// <summary>
        /// Sets the default status for the AnimationNetwork.
        /// </summary>
        /// <param name="status">The default status to set.</param>
        /// <returns>The current instance of AnimationNetwork for method chaining.</returns>
        public AnimationNetwork SetDefaultStatus(string status) {
            this.defaultStatus = status;
            return this;
        }

        /// <summary>
        /// Gets the Animator component associated with this AnimationNetwork.
        /// </summary>
        /// <returns>The Animator component.</returns>
        public Animator getAnimator() {
            return this.animator;
        }

        /// <summary>
        /// Computes and updates the active state of the AnimationNetwork.
        /// </summary>
        public override void ComputeActive() {
            if (!this.initialized) {
                this.InitializeAnimator();
            }
            // Synchronization logic based on the current synchronization type.
            if (this.animator != null) {
                if (AnimationSyncType.UseController == this.syncronizationType) {
                    // Synchronization logic when using the Animator Controller.
                    SynchronizeUsingController();
                } else if (AnimationSyncType.UseParameters == this.syncronizationType) {
                    // Synchronization logic when using Animator parameters.
                    SynchronizeUsingParameters();
                }
            }
        }

        /// <summary>
        /// Synchronizes the animation state using the Animator Controller.
        /// </summary>
        private void SynchronizeUsingController() {
            AnimatorClipInfo[] existentClipInfo = this.animator.GetCurrentAnimatorClipInfo(LAYER_ANIMATION);
            if (existentClipInfo.Count() > 0) {
                foreach (AnimatorClipInfo clip in existentClipInfo) {
                    int animationHash = Animator.StringToHash(String.Format("{0}.{1}", ANIMATION_BASE_LAYER, clip.clip.name));
                    if (this.hashToClip.ContainsKey(animationHash) == false) {
                        this.hashToClip.Add(animationHash, clip.clip);
                    }
                }
                if (this.IsAnimationPlaying(this.animator)) {
                    float actualAnimationTime = this.GetCurrentAnimatorTime(this.animator);
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(LAYER_ANIMATION);

                    this.FlagUpdated(animationData.Count != (((stateInfo.fullPathHash != animationData.AnimationHash) ||
                                                              (actualAnimationTime < animationData.AnimationTime)) ? 1 : 0));
                    this.FlagUpdated(animationData.AnimationHash != stateInfo.fullPathHash);
                    this.FlagUpdated(animationData.AnimationTime != actualAnimationTime);

                    animationData.Count += ((stateInfo.fullPathHash != animationData.AnimationHash) ||
                                            (actualAnimationTime < animationData.AnimationTime)) ? 1 : 0;
                    animationData.AnimationHash = stateInfo.fullPathHash;
                    animationData.AnimationTime = actualAnimationTime;
                }
            }
        }

        /// <summary>
        /// Synchronizes the animation state using Animator parameters.
        /// </summary>
        private void SynchronizeUsingParameters() {
            if (this.cachedParameters == null) {
                this.cachedParameters = this.animator.parameters;
            }
            if (this.cachedClips == null) {
                this.cachedClips = this.animator.runtimeAnimatorController.animationClips;
                InitializeClipHashMap();
            }
            foreach (AnimatorControllerParameter param in this.cachedParameters) {
                int paramNameHash = param.nameHash;
                if (!this.animationData.AnimationParameters.TryGetValue(paramNameHash, out var paramValues)) {
                    this.animationData.AnimationParameters[paramNameHash] = paramValues = new AnimationParamValues();
                }
                if (param.type == AnimatorControllerParameterType.Float) {
                    float previous = paramValues.floatValue;
                    paramValues.floatValue = this.animator.GetFloat(paramNameHash);
                    this.FlagUpdated(paramValues.floatValue != previous);
                } else if (param.type == AnimatorControllerParameterType.Int) {
                    int previous = paramValues.intValue;
                    paramValues.intValue = this.animator.GetInteger(paramNameHash);
                    this.FlagUpdated(paramValues.intValue != previous);
                } else if (param.type == AnimatorControllerParameterType.Bool) {
                    bool previous = paramValues.boolValue;
                    paramValues.boolValue = this.animator.GetBool(paramNameHash);
                    this.FlagUpdated(paramValues.boolValue != previous);
                } else if (param.type == AnimatorControllerParameterType.Trigger) {
                    bool previous = paramValues.triggerValue;
                    paramValues.triggerValue = this.animator.GetBool(paramNameHash);
                    this.FlagUpdated(paramValues.triggerValue != previous);
                }
            }
            // Get playing clip ( is something are being played )
            for (int layerIndex = 0; layerIndex < this.layersCount; layerIndex++) {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
                int currentStateHash = stateInfo.shortNameHash;
                int animationLayer = animationData.AnimationLayer;
                if (clipHashMap.TryGetValue(currentStateHash, out AnimationClip currentClip))
                {
                    this.FlagUpdated(this.animationData.PlaySequence != ((stateInfo.normalizedTime < this.animationData.AnimationTime) ? 1 : 0));
                    this.FlagUpdated(this.animationData.AnimationHash != stateInfo.fullPathHash);
                    this.FlagUpdated(Mathf.Abs(this.animationData.AnimationTime - stateInfo.normalizedTime) > 0.01f);
                    this.FlagUpdated(this.animationData.AnimationLayer != layerIndex);

                    this.animationData.PlaySequence += (stateInfo.normalizedTime < this.animationData.AnimationTime) ? 1 : 0;
                    this.animationData.AnimationHash = stateInfo.fullPathHash;
                    this.animationData.AnimationTime = stateInfo.normalizedTime;
                    this.animationData.AnimationLayer = layerIndex;
                    animationLayer = layerIndex;
                } else {
                    foreach (AnimationClip clip in this.cachedClips) {
                        if (animationLayer == layerIndex) {
                            this.FlagUpdated(Mathf.Abs(this.animationData.AnimationTime - stateInfo.normalizedTime) > 0.01f);
                            this.animationData.AnimationTime = stateInfo.normalizedTime;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the animation data dictionaries.
        /// </summary>
        private void InitializeAnimationData() {
            this.animationData.PlayingAnimationClips = new Dictionary<int, AnimationClipPacket>();
            this.animationData.AnimationParameters = new Dictionary<int, AnimationParamValues>();

            this.previousAnimationData.PlayingAnimationClips = new Dictionary<int, AnimationClipPacket>();
            this.previousAnimationData.AnimationParameters = new Dictionary<int, AnimationParamValues>();
        }

        private Dictionary<int, AnimationClip> clipHashMap = new Dictionary<int, AnimationClip>();
        /// <summary>
        /// hash Animation Clips for performance
        /// </summary>
        void InitializeClipHashMap(){
            foreach (AnimationClip clip in this.cachedClips){
                int hash = Animator.StringToHash(clip.name);
                clipHashMap[hash] = clip;
            }
        }
        /// <summary>
        /// Initializes the Animator component and populates the animatorParameters dictionary.
        /// </summary>
        private void InitializeAnimator() {
            if (this.animator == null) {
                this.animator = this.GetNetworkObject().GetGameObject().GetComponentInChildren<Animator>();
                if (this.animator != null) {
                    foreach (AnimatorControllerParameter param in this.animator.parameters) {
                        this.animatorParameters.Add(param.nameHash, param);
                        if (this.parameterControlledByCurve.ContainsKey(param.nameHash) == false) {
                            this.parameterControlledByCurve[param.nameHash] = this.animator.IsParameterControlledByCurve(param.nameHash);
                        }
                    }
                }
                this.initialized = true;
            }
        }


        /// <summary>
        /// Computes the passive state of the animation, initializing the animator if necessary,
        /// and synchronizing animation states based on the current synchronization type.
        /// </summary>
        public override void ComputePassive() {
            if (!this.initialized) {
                this.InitializeAnimator();
            }
            if (AnimationSyncType.UseController == this.syncronizationType) {
                if ((this.animationData.AnimationHash != this.previousAnimationData.AnimationHash) ||
                    (this.previousAnimationData.Count < this.animationData.Count)) {
                    this.previousAnimationData.Count = this.animationData.Count;
                    this.previousAnimationData.AnimationHash = this.animationData.AnimationHash;
                    this.previousAnimationData.AnimationTime = this.animationData.AnimationTime;
                    this.animator.Play(this.animationData.AnimationHash, LAYER_ANIMATION, this.animationData.AnimationTime);
                }
            } else if (AnimationSyncType.UseParameters == this.syncronizationType) {
                foreach (var parameterData in this.animationData.AnimationParameters) {
                    if (!this.previousAnimationData.AnimationParameters.TryGetValue(parameterData.Key, out var tempData)) {
                        this.previousAnimationData.AnimationParameters.Add(parameterData.Key, new AnimationParamValues());
                    }
                    if (((this.animatorParameters[parameterData.Key].type == AnimatorControllerParameterType.Int)       && (this.previousAnimationData.AnimationParameters[parameterData.Key].intValue      != parameterData.Value.intValue)) ||
                        ((this.animatorParameters[parameterData.Key].type == AnimatorControllerParameterType.Float)     && (this.previousAnimationData.AnimationParameters[parameterData.Key].floatValue    != parameterData.Value.floatValue)) ||
                        ((this.animatorParameters[parameterData.Key].type == AnimatorControllerParameterType.Bool)      && (this.previousAnimationData.AnimationParameters[parameterData.Key].boolValue     != parameterData.Value.boolValue)) ||
                        ((this.animatorParameters[parameterData.Key].type == AnimatorControllerParameterType.Trigger)   && (this.previousAnimationData.AnimationParameters[parameterData.Key].triggerValue  != parameterData.Value.triggerValue))) {
                        this.previousAnimationData.AnimationParameters[parameterData.Key].intValue      = parameterData.Value.intValue;
                        this.previousAnimationData.AnimationParameters[parameterData.Key].floatValue    = parameterData.Value.floatValue;
                        this.previousAnimationData.AnimationParameters[parameterData.Key].boolValue     = parameterData.Value.boolValue;
                        this.previousAnimationData.AnimationParameters[parameterData.Key].triggerValue  = parameterData.Value.triggerValue;
                        AnimatorControllerParameter paramFromAnimator = this.animatorParameters[parameterData.Key];
                        if (paramFromAnimator.type == AnimatorControllerParameterType.Float) {
                            if (this.parameterControlledByCurve[parameterData.Key] == false) {
                                this.animator.SetFloat(parameterData.Key, parameterData.Value.floatValue);
                            }
                        } else if (paramFromAnimator.type == AnimatorControllerParameterType.Int) {
                            this.animator.SetInteger(parameterData.Key, parameterData.Value.intValue);
                        } else if (paramFromAnimator.type == AnimatorControllerParameterType.Bool) {
                            this.animator.SetBool(parameterData.Key, parameterData.Value.boolValue);
                        } else if (paramFromAnimator.type == AnimatorControllerParameterType.Trigger) {
                            if (parameterData.Value.boolValue == true) {
                                this.animator.SetTrigger(parameterData.Key);
                            }
                        }
                    }
                }
                if (this.animationData.PlaySequence != this.previousAnimationData.PlaySequence) {
                    this.previousAnimationData.AnimationHash = this.animationData.AnimationHash;
                    this.previousAnimationData.AnimationTime = this.animationData.AnimationTime;
                    this.previousAnimationData.AnimationLayer = this.animationData.AnimationLayer;
                    this.previousAnimationData.PlaySequence = this.animationData.PlaySequence;

                    AnimationSequence sequence = new AnimationSequence();
                    sequence.AnimationStart = NetworkClock.time;
                    sequence.AnimationDuration = this.currentAnimationDuration;
                    sequence.Animation = this.animationData;
                    // Enqueue to be executed
                    this.pendingAnimations.Enqueue(sequence);
                } else if ((this.currentAnimation != null) && ((this.currentAnimation.AnimationStart + this.currentAnimation.AnimationDuration) < NetworkClock.time)) {
                    if (this.pendingAnimations.Count > 0) {
                        this.currentAnimation = this.pendingAnimations.Dequeue();
                    } else {
                        this.currentAnimation = null;
                        this.timeToStartIdle = (NetworkClock.time + DELAY_TO_START_IDLE);
                    }
                } else if ((this.currentAnimation != null) && ((this.currentAnimation.AnimationStart + this.currentAnimation.AnimationDuration) >= NetworkClock.time)) {
                    if (this.currentAnimation.IsPlaying == false) {
                        this.currentAnimation.IsPlaying = true;
                        this.animator.Play(this.currentAnimation.Animation.AnimationHash, this.currentAnimation.Animation.AnimationLayer);
                    }
                    // Update duration
                    this.currentAnimation.AnimationDuration = this.currentAnimationDuration;
                } else if ((this.currentAnimation == null) && (this.autoIdle == true)) {
                    if (this.timeToStartIdle < NetworkClock.time) {
                        AnimatorStateInfo animState = this.animator.GetCurrentAnimatorStateInfo(this.animationData.AnimationLayer);
                        if ((animState.normalizedTime > 1f) ||
                            (this.animationData.AnimationTime > 2f)) {
                            for (int layerIndex = 0; layerIndex < this.layersCount; layerIndex++) {
                                this.animator.Play(this.defaultStatus, layerIndex);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Synchronizes the active state of the animation by writing the current animation data to a data stream.
        /// </summary>
        /// <param name="writer">The data stream to write the animation data to.</param>
        public override void SynchonizeActive(IDataStream writer) {
            // Write if update is paused
            writer.Write(this.IsPaused());
            if (this.IsPaused() == false) {
                if (AnimationSyncType.UseController == this.syncronizationType) {
                    writer.Write(animationData.Count);
                    writer.Write(animationData.AnimationHash);
                    writer.Write(animationData.AnimationTime);
                } else if (AnimationSyncType.UseParameters == this.syncronizationType) {
                    writer.Write((byte)this.animationData.AnimationParameters.Count); // Number of parameters
                    foreach (var animatorParam in this.animationData.AnimationParameters) {
                        AnimatorControllerParameter paramFromAnimator = this.animatorParameters[animatorParam.Key];
                        writer.Write(animatorParam.Key); // Parameter name hash
                        writer.Write((byte)paramFromAnimator.type); // Animation type
                        if (paramFromAnimator.type == AnimatorControllerParameterType.Float) {
                            writer.Write<float>(animatorParam.Value.floatValue);                            
                        } else if (paramFromAnimator.type == AnimatorControllerParameterType.Int) {
                            writer.Write<int>(animatorParam.Value.intValue);
                        } else if (paramFromAnimator.type == AnimatorControllerParameterType.Bool) {
                            writer.Write<bool>(animatorParam.Value.boolValue);
                        } else if (paramFromAnimator.type == AnimatorControllerParameterType.Trigger) {
                            writer.Write<bool>(animatorParam.Value.triggerValue); // Trigger is a ordinary bool
                        }
                    }
                    // Increase duration
                    if (this.currentAnimationHash == this.animationData.AnimationHash) {
                        this.currentAnimationDuration += NetworkClock.deltaTime;
                    }
                    writer.Write(this.animationData.AnimationHash);     // Hash of current animation
                    writer.Write(this.animationData.AnimationTime);     // Time of current animation
                    writer.Write((byte)this.animationData.AnimationLayer);    // Layer of current animation
                    writer.Write(this.animationData.PlaySequence);      // Sequence of play
                    writer.Write(this.currentAnimationDuration);        // Send how many time this animation takes                

                    // Control time during current animation is playing
                    if (this.currentAnimationHash != this.animationData.AnimationHash) {
                        this.currentAnimationHash = this.animationData.AnimationHash;
                        this.currentAnimationDuration = 0f;
                    }
                }
            }
        }
        private readonly HashSet<int> _tempNewKeys = new HashSet<int>();
        /// <summary>
        /// Synchronizes the passive state of the animation by updating the animation data based on the received packet.
        /// </summary>
        /// <param name="data">The animation info packet containing the updated animation data.</param>
        public override void SynchonizePassive(AnimationInfoPacket data) {
            if (AnimationSyncType.UseController == this.syncronizationType) {
                this.animationData.Count = data.Count;
                this.animationData.AnimationHash = data.AnimationHash;
                this.animationData.AnimationTime = data.AnimationTime;
            } else if (AnimationSyncType.UseParameters == this.syncronizationType) {
                this.animationData.Count = data.AnimationParameters.Count;
                // cahce the keys 
                _tempNewKeys.Clear();
                foreach ( int key in data.AnimationParameters.Keys ) _tempNewKeys.Add(key);
                foreach (int paramNameHash in _tempNewKeys) {
                    if (!this.animationData.AnimationParameters.TryGetValue(paramNameHash, out var paramValues)) {
                        this.animationData.AnimationParameters[paramNameHash] = paramValues = new AnimationParamValues();
                    }
                    paramValues.floatValue    = data.AnimationParameters[paramNameHash].floatValue;
                    paramValues.intValue      = data.AnimationParameters[paramNameHash].intValue;
                    paramValues.boolValue     = data.AnimationParameters[paramNameHash].boolValue;
                    paramValues.triggerValue  = data.AnimationParameters[paramNameHash].triggerValue;
                }
                this.animationData.AnimationHash    = data.AnimationHash;
                this.animationData.AnimationTime    = data.AnimationTime;
                this.animationData.AnimationLayer   = data.AnimationLayer;
                this.animationData.PlaySequence     = data.PlaySequence;
            }
        }

        /// <summary>
        /// Retrieves the current animation data for passive synchronization.
        /// </summary>
        /// <returns>The current animation data.</returns>
        public override AnimationInfoPacket GetPassiveArguments() {
            return this.animationData;
        }

        /// <summary>
        /// Extracts and updates the animation data from a data stream.
        /// </summary>
        /// <param name="reader">The data stream to read the animation data from.</param>
        public override void Extract(IDataStream reader) {
            // First extract if position is paused by other side
            bool isSenderPaused = reader.Read<bool>();
            if (isSenderPaused == false) {
                this.Resume();
                if (AnimationSyncType.UseController == this.syncronizationType) {
                    this.animationData.Count = reader.Read<int>();
                    this.animationData.AnimationHash = reader.Read<int>();
                    this.animationData.AnimationTime = reader.Read<float>();
                } else if (AnimationSyncType.UseParameters == this.syncronizationType) {
                    this.animationData.Count = reader.Read<byte>();
                    for (int indexCount = 0; indexCount < this.animationData.Count; indexCount++) {
                        int paramNameHash   = reader.Read<int>();
                        int paramType       = reader.Read<byte>();
                        AnimatorControllerParameterType paramEnumType = (AnimatorControllerParameterType)paramType;
                        // Check if exists, othrwise add
                        if (!this.animationData.AnimationParameters.TryGetValue(paramNameHash, out var tempData)) {
                            this.animationData.AnimationParameters[paramNameHash] = new AnimationParamValues();
                        }
                        if (paramEnumType == AnimatorControllerParameterType.Float) {
                            this.animationData.AnimationParameters[paramNameHash].floatValue = reader.Read<float>();
                        } else if (paramEnumType == AnimatorControllerParameterType.Int) {
                            this.animationData.AnimationParameters[paramNameHash].intValue = reader.Read<int>();
                        } else if (paramEnumType == AnimatorControllerParameterType.Bool) {
                            this.animationData.AnimationParameters[paramNameHash].boolValue = reader.Read<bool>();
                        } else if (paramEnumType == AnimatorControllerParameterType.Trigger) {
                            this.animationData.AnimationParameters[paramNameHash].triggerValue = reader.Read<bool>();
                        }
                    }
                    this.animationData.AnimationHash    = reader.Read<int>();   // Hash of current animation
                    this.animationData.AnimationTime    = reader.Read<float>(); // Time of current animation
                    this.animationData.AnimationLayer   = reader.Read<byte>();   // Layer of current animation
                    this.animationData.PlaySequence     = reader.Read<int>();   // Play sequence
                    this.currentAnimationDuration       = reader.Read<float>(); // Duration of current animation
                }
            } else {
                this.Pause();
            }
        }

        /// <summary>
        /// Retrieves the animation clip associated with a given hash.
        /// </summary>
        /// <param name="hash">The hash code of the animation clip to retrieve.</param>
        /// <returns>The animation clip if found; otherwise, null.</returns>
        private AnimationClip GetClipFromHash(int hash) {
            AnimationClip clip;
            if (hashToClip.TryGetValue(hash, out clip)) {
                return clip;
            } else {
                return null;
            }
        }


        /// <summary>
        /// Retrieves the current time of the animation being played on the specified Animator.
        /// </summary>
        /// <param name="targetAnim">The Animator component to check the animation time on.</param>
        /// <param name="layer">The layer of the Animator to check. Default is 0.</param>
        /// <param name="targetClip">Optional specific AnimationClip to check the time of. If null, the current clip is used.</param>
        /// <returns>The current time of the animation, clamped between 0 and 1.</returns>
        private float GetCurrentAnimatorTime(Animator targetAnim, int layer = 0, AnimationClip targetClip = null) {
            // Get the current state information of the specified layer.
            AnimatorStateInfo animState = targetAnim.GetCurrentAnimatorStateInfo(layer);
            // Retrieve the hash code for the current animation.
            int currentAnimHash = animState.fullPathHash;
            // Determine the clip to use, either the provided targetClip or the current clip based on the hash.
            AnimationClip clip = (targetClip == null) ? GetClipFromHash(currentAnimHash) : targetClip;
            // Calculate the current time of the animation based on its length and normalized time.
            float currentTime = (clip != null) ? clip.length * animState.normalizedTime : 0f;
            // Return the current time, ensuring it is within the range [0, 1].
            return Mathf.Clamp(currentTime, 0f, 1f);
        }

        /// <summary>
        /// Checks if an animation is currently playing on the specified Animator.
        /// </summary>
        /// <param name="targetAnim">The Animator component to check the animation state on.</param>
        /// <param name="layer">The layer of the Animator to check. Default is 0.</param>
        /// <param name="targetClip">Optional specific AnimationClip to check the playing state of. If null, the current clip is used.</param>
        /// <returns>True if the animation is currently playing, false otherwise.</returns>
        private bool IsAnimationPlaying(Animator targetAnim, int layer = 0, AnimationClip targetClip = null) {
            // Get the current state information of the specified layer.
            AnimatorStateInfo animState = targetAnim.GetCurrentAnimatorStateInfo(layer);
            // Retrieve the hash code for the current animation.
            int currentAnimHash = animState.fullPathHash;
            // Determine the clip to use, either the provided targetClip or the current clip based on the hash.
            AnimationClip clip = (targetClip == null) ? GetClipFromHash(currentAnimHash) : targetClip;
            // Check if the specified clip is currently playing.
            if (clip != null) {
                // Return true if the animation with the same name is playing or if the normalized time is less than 1 (not finished).
                if (targetAnim.GetCurrentAnimatorStateInfo(layer).IsName(clip.name) ||
                    targetAnim.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1.0f) {
                    return true;
                } else {
                    return false;
                }
            } else {
                // If the clip is null, return false as no animation is playing.
                return false;
            }
        }

    }
}