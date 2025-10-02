#pragma warning disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a networked object that can synchronize state, animations, audio, and other properties over the network.
    /// </summary>
    public class NetworkObject : NetworkLocalEvents, INetworkControl, INetworkBehaviorInternal {

        public INetworkAnimation Animation {
            get {
                return (this.animationController != null) ? this.animationController : throw new Exception("There's no animations support on this object");
            }
        }

        public INetworkAudio Audio {
            get {
                return (this.audioController != null) ? this.audioController : throw new Exception("There's no audio support on this object");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        ///
        /// Network Events Associated With This Object ( Came From NetworkPrefabEntry )
        ///
        //////////////////////////////////////////////////////////////////////////////////////////////
        [HideInInspector]
        [SerializeField]
        private EventReference onSpawnPrefab;

        [HideInInspector]
        [SerializeField]
        private EventReference onDespawnPrefab;

        [HideInInspector]
        [SerializeField]
        private EventReference onAcceptOwnerShip;

        [HideInInspector]
        [SerializeField]
        private EventReference onAcceptReleaseOwnerShip;

        [HideInInspector]
        [SerializeField]
        private EventReference onTakeObjectOwnerShip;

        [HideInInspector]
        [SerializeField]
        private EventReference onReleaseObjectOwnerShip;

        [HideInInspector]
        [SerializeField]
        private NetworkTransport transport;

        [HideInInspector]
        [SerializeField]
        private BehaviorMode behaviorMode = BehaviorMode.Both;

        [HideInInspector]
        [SerializeField]
        private DeliveryMode deliveryMode = DeliveryMode.Unreliable;

        [HideInInspector]
        [SerializeField]
        private OwnerShipAccessLevel ownerShipAccessLevel = OwnerShipAccessLevel.Full;

        [HideInInspector]
        [SerializeField]
        private PredictionType movementType = PredictionType.Automatic;

        [HideInInspector]
        [SerializeField]
        protected bool[] syncPosition = new bool[3] { true, true, true };

        [HideInInspector]
        [SerializeField]
        private bool[] syncRotation = new bool[3] { true, true, true };

        [HideInInspector]
        [SerializeField]
        private bool[] syncScale = new bool[3] { true, true, true };

        [HideInInspector]
        [SerializeField]
        private bool disableObjectGravity = true;

        [HideInInspector]
        [SerializeField]
        private bool enableObjectKinematic = true;

        [HideInInspector]
        [SerializeField]
        private bool enablePhysicsInterpolation = false;

        [HideInInspector]
        [SerializeField]
        private bool usePrefabRotation = false; 

        [HideInInspector]
        [SerializeField]
        private bool remoteControls = false;

        [HideInInspector]
        [SerializeField]
        private bool syncAnimation = false;

        [HideInInspector]
        [SerializeField]
        private AnimationSyncType syncAnimationMode = AnimationSyncType.UseController;

        [HideInInspector]
        [SerializeField]
        private int animationLayerCount = 1;

        [HideInInspector]
        [SerializeField]
        private string animationDefaultStatus = "";

        [HideInInspector]
        [SerializeField]
        private bool syncParticles = false;

        // Private fields for internal state management.

        // This is a different cas when i'm going to use a getter and setter into a private field, i will do this to check if has another
        // network object attached on this gameobject
        private INetworkElement networkElement { get { return this.GetNetworkObjectFromCache(); } set { this.internalNetworkElement = value; } }
        private INetworkElement internalNetworkElement;
        private INetworkAnimation animationController;
        private INetworkAudio audioController;
        private IClient client;
        private INetworkInput input;
        private NetworkInternalExecutor internalExecutor;
        private int sendRateAmount = SEND_RATE_AMOUNT;
        private bool networkInitialized = false;
        private bool behaviorInitialized = false;
        private bool identified = false;
        private bool isPlayer = false;
        private bool physicsCollected = false;
        private bool defaultGravity = false;
        private float defaultGravityScale = 0f;
        private bool defaultKinematic = false;
        private bool activeApplied = false;
        private bool passiveApplied = false;
        private bool respawnedObject = false;
        private bool ownerShipTranferReceived = false;
        private bool hashingMethodsCodesExecuting = false;
        private bool hashingMethodsCodesComplete = false;
        private Rigidbody rigidBody3D = null;
        private Rigidbody2D rigidBody2D = null;
        private Dictionary<string, IVariable> variables = new Dictionary<string, IVariable>();
        private Dictionary<string, ulong> variablesVersion = new Dictionary<string, ulong>();
        private VariablesNetwork variabledNetworkControl;
        private Dictionary<Type, Dictionary<String, FieldInfo>> cachedVariables = new Dictionary<Type, Dictionary<string, FieldInfo>>();
        private Dictionary<String, IVariable> cachedNetworkVariables = new Dictionary<string, IVariable>();
        private BehaviorMode onStartBehaviourMode = BehaviorMode.Unknow;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Use the pair [Class, Method] CRC to identify methods; this implementation is far away better than sending the method name over the network 
        /// since uses less bit than method literal string names
        /// 
        /// Note: This idea was originally conceived by @Brighty97 [ Discord ID : 512963848815378434 ]
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private Dictionary<uint, MethodInfo> methodsHashCodes = new Dictionary<uint, MethodInfo>();
        
        /// <summary>
        /// I'm going to store the childs sequence, and this will be used to redirect the NetworkExecute method according to the Child index on parent
        /// </summary>
        private int childObjectIndex = 0;

        // List all internal methods that will not be listed on the HASHES 
        private HashSet<string> internalMethodsToExclude = new HashSet<string>
                {
                    "Awake", 
                    "Start", 
                    "OnEnable", 
                    "OnDisable", 
                    "Update", 
                    "FixedUpdate", 
                    "LateUpdate", 
                    "OnDestroy",
                    "ActiveAwake", 
                    "OnNetworkStarted", 
                    "PassiveAwake", 
                    "ActiveStart", 
                    "PassiveStart", 
                    "ActiveUpdate",
                    "PassiveUpdate", 
                    "ActiveFixedUpdate", 
                    "PassiveFixedUpdate", 
                    "ActiveLateUpdate", 
                    "PassiveLateUpdate",
                    "OnApplicationQuit", 
                    "Reset", 
                    "OnValidate", 
                    "OnApplicationFocus", 
                    "OnApplicationPause",
                    "OnCollisionEnter", 
                    "OnCollisionExit", 
                    "OnCollisionStay", 
                    "OnTriggerEnter", 
                    "OnTriggerExit",
                    "OnTriggerStay", 
                    "OnMouseDown", 
                    "OnMouseUp", 
                    "OnMouseDrag", 
                    "OnMouseEnter", 
                    "OnMouseExit",
                    "OnMouseOver", 
                    "OnPreRender", 
                    "OnPostRender", 
                    "OnRenderObject", 
                    "OnWillRenderObject", 
                    "OnGUI",
                    "OnAnimatorMove", 
                    "OnAnimatorIK", 
                    "OnBeforeSerialize", 
                    "OnAfterDeserialize", 
                    "OnAudioFilterRead"
                };

        // Constants for default values and array indices.
        public const int DEFAULT_NETWORK_ID = 0;
        public const ushort DEFAULT_NETWORK_TAG = 0;
        const int OBJECT_PACKET_SIZE = 1024;
        const int X_POSITION_IN_ARRAY = 0;
        const int Y_POSITION_IN_ARRAY = 1;
        const int Z_POSITION_IN_ARRAY = 2;
        const int SEND_RATE_AMOUNT = 30;
        const float TELEPORT_DURATION = 0.0f;

        // Virtual methods for network lifecycle events that can be overridden by derived classes.
        /// <summary>
        /// Called when the network is being initialized.
        /// This is a virtual method that can be overridden by derived classes to perform custom actions upon network awakening.
        /// </summary>
        public virtual void InternalNetworkAwake() { }

        /// <summary>
        /// Internal start method called when the network object is enabled network operations.
        /// This is similar to a OnEnable method but specific to network initialization.
        /// </summary>
        public virtual void InternalNetworkOnEnable() { }

        /// <summary>
        /// Internal start method called when the network object is disabled network operations.
        /// This is similar to a OnDisable method but specific to network initialization.
        /// </summary>
        public virtual void InternalNetworkOnDisable() { }

        /// <summary>
        /// Called when the network starts.
        /// This is a virtual method that can be overridden by derived classes to perform custom actions at the start of the network lifecycle.
        /// </summary>
        public virtual void InternalNetworkStart() { }

        /// <summary>
        /// Called every frame while the network is active.
        /// This is a virtual method that can be overridden by derived classes to perform custom actions during the network update phase.
        /// </summary>
        public virtual void InternalNetworkUpdate() { }

        /// <summary>
        /// Called after all Update functions have been called.
        /// This is a virtual method that can be overridden by derived classes to perform custom actions after the network update phase, such as post-processing actions.
        /// </summary>
        public virtual void InternalNetworkLateUpdate() { }

        /// <summary>
        /// Called at a fixed time interval determined by the physics engine.
        /// This is a virtual method that can be overridden by derived classes to perform custom actions in sync with the physics engine's update cycle.
        /// </summary>
        public virtual void InternalNetworkFixedUpdate() { }

        /// <summary>
        /// Return the delivery mode of network variable
        /// </summary>
        /// <param name="field">Field to check</param>
        /// <returns>The delivery mode of requqsted field</returns>
        public virtual DeliveryMode GetSynchonizedFieldDeliveryMode(String field) { return DeliveryMode.Reliable; }

        /// <summary>
        /// Return the detection mode of network variable
        /// </summary>
        /// <param name="field">Field to check</param>
        /// <returns>The detection mode of requqsted field</returns>
        public virtual ReliableDetectionMode GetSynchonizedFieldDetectionMode(String field) { return ReliableDetectionMode.Automatic; }

        /// <summary>
        /// Return the order whene OnNetworkStart will be called
        /// </summary>
        /// <returns>Network start order</returns>
        public NetworkStartOrder GetStartOrder() {
            NetworkStartOrder result = (this.GetComponent<NetworkMasterPlayer>() != null) ? NetworkStartOrder.OnConnectionStablished : NetworkStartOrder.OnModeAssigned;
            if (this.GetComponent<NetworkMasterPlayer>() == null) {
                BehaviourConfig config = this.GetType().GetAttribute<BehaviourConfig>();
                if (config != null) {
                    result = config.LoadOrder;
                }
            }
            return result;
        }

        /// <summary>
        /// Initializes the network by creating network elements and registering the object with the internal network executor.
        /// </summary>
        /// <param name="networkId">The network identifier for this object.</param>
        public virtual void StartNetwork(int networkId = DEFAULT_NETWORK_ID, Func<INetworkElement, INetworkElement> onNetworkStartedCallback = null) {
            if (this.transport != null) {
                // Insert internal network component on object
                this.internalExecutor = NetworkInternalExecutor.Register(this.gameObject, this, 
                    () => {
                        if (this.transport.GetSocket() != null) {
                            NetworkManager.Instance().OnPlayerStartedEvent(((onNetworkStartedCallback != null) ? onNetworkStartedCallback.Invoke(this.CreateNetworkElements(networkId)) : this.CreateNetworkElements(networkId)));
                        } else {
                            this.transport.RegisterAwake(() => {
                                NetworkManager.Instance().OnPlayerStartedEvent(((onNetworkStartedCallback != null) ? onNetworkStartedCallback.Invoke(this.CreateNetworkElements(networkId)) : this.CreateNetworkElements(networkId)));
                            });
                        }
                    },
                    (NetworkBehaviour childBehaviour) => {
                        childBehaviour.CollectNetworkVariables();
                        childBehaviour.InitializeSynchonizedFields();
                        childBehaviour.CollectMethodsImplementations();
                        if (NetworkStartOrder.OnConnectionStablished.Equals(childBehaviour.GetStartOrder())) {
                            childBehaviour.OnNetworkStarted();
                        }}
                    );
                           
                if (this.animationController != null) {
                    this.animationController.Initialize();
                }
                if (this.audioController != null) {
                    this.audioController.Intialize();
                }
                // Register remove method invoke event
                int currentObjectIndex = 0;
                NetworkObject[] networkObjects = GetComponentsInChildren<NetworkObject>(true).OrderBy(c => c.transform.parent == null ? 0 : 1) // Root objects first
                                                                                             .ThenBy(c => c.transform.parent != null ? c.transform.GetSiblingIndex() : 0) // Sort by sibling index for non-root
                                                                                             .ThenBy(c => c.gameObject.name) // Name fallback
                                                                                             .ToArray();

                foreach (NetworkObject childObject in networkObjects) {
                    childObject.childObjectIndex = currentObjectIndex;
                    childObject.RegisterEvent(UserCustomObjectEvents.REMOTE_METHOD_EXECUTION + currentObjectIndex, (NetworkManager.Instance().InRelayMode()) ? childObject.OnRemoteMethodInvokedOnRelay : childObject.OnRemoteMethodInvoked);
                    currentObjectIndex++;                  
                }
                this.RegisterEvent(UserCustomObjectEvents.ObjectTeleport,           (NetworkManager.Instance().InRelayMode()) ? this.OnTeleportObjectOnRelay            : this.OnTeleportObject);
                // Take object ownership
                this.RegisterEvent(UserCustomObjectEvents.TakeControl,              (NetworkManager.Instance().InRelayMode()) ? this.OnOwnerShipTakeOnRelay             : this.OnOwnerShipTake);
                this.RegisterEvent(UserCustomObjectEvents.TakeControlSucess,        (NetworkManager.Instance().InRelayMode()) ? this.OnOwnerShipTakeSucessOnRelay       : this.OnOwnerShipTakeSucess);
                this.RegisterEvent(UserCustomObjectEvents.ReleaseControl,           (NetworkManager.Instance().InRelayMode()) ? this.OnOwnerShipReleaseOnRelay          : this.OnOwnerShipRelease);
                this.RegisterEvent(UserCustomObjectEvents.ReleaseControlSucess,     (NetworkManager.Instance().InRelayMode()) ? this.OnOwnerShipReleaseSucessOnRelay    : this.OnOwnerShipReleaseSucess);
                this.RegisterEvent(UserCustomObjectEvents.TransferControl,          (NetworkManager.Instance().InRelayMode()) ? this.OnOwnerShipTranferredOnRelay       : this.OnOwnerShipTranferred);
                this.RegisterEvent(UserCustomObjectEvents.OwnerShipDefined,         (NetworkManager.Instance().InRelayMode()) ? this.OnOwnerDefinedOnRelay              : this.OnOwnerDefined);
            }
        }

        /// <summary>
        /// Update ths NetworkObject based into another Network object
        /// Note : This method exists to allow to use more than one NetworkBehaviour on same GameObject 
        ///        NetworkElement wil be the same, nonetheless, i also need update this NetworkObject with the values of original copied
        /// </summary>
        /// <param name="source">NetworkObject source of information</param>
        private void UpdateFromSource(NetworkObject source) {
            // Configure network object based into another
            this.transport                  = source.transport;
            this.client                     = source.client;
            this.deliveryMode               = source.deliveryMode;
            this.behaviorMode               = source.behaviorMode;
            this.ownerShipAccessLevel       = source.ownerShipAccessLevel;
            this.remoteControls             = source.remoteControls;
            this.identified                 = source.identified;
            this.behaviorMode               = source.behaviorMode;
            this.syncParticles              = source.syncParticles;
            this.syncAnimation              = source.syncAnimation;
            this.syncAnimationMode          = source.syncAnimationMode;
            this.animationLayerCount        = source.animationLayerCount;
            this.sendRateAmount             = source.sendRateAmount;
            this.isPlayer                   = source.isPlayer;
            this.animationController        = source.animationController;
            this.audioController            = source.audioController;
            this.input                      = source.input;
            this.syncPosition[0]            = source.syncPosition[0];
            this.syncPosition[1]            = source.syncPosition[1];
            this.syncPosition[2]            = source.syncPosition[2];
            this.syncRotation[0]            = source.syncRotation[0];
            this.syncRotation[1]            = source.syncRotation[1];
            this.syncRotation[2]            = source.syncRotation[2];
            this.syncScale[0]               = source.syncScale[0];
            this.syncScale[1]               = source.syncScale[1];
            this.syncScale[2]               = source.syncScale[2];
            this.movementType               = source.movementType;
            this.disableObjectGravity       = source.disableObjectGravity;
            this.enableObjectKinematic      = source.enableObjectKinematic;
        }

        /// <summary>
        /// Synchronize all network behaviour belonging to the same GameObject
        /// </summary>
        private void SynchronizeSiblingsObjects() {
            foreach (NetworkObject obj in this.GetComponents<NetworkObject>()) {
                if (obj != this) {
                    obj.UpdateFromSource(this);
                }
            }
        }

        /// <summary>
        /// Return network element associated with this network object
        /// Note: In case of dont'have an netwokr element, im going to check if has another network objects attached on the same gameobject
        ///       and if yes, i'm going to reuse it
        /// </summary>
        /// <returns>Instance of network element</returns>
        private INetworkElement GetNetworkObjectFromCache() {
            // If network element is null i'm going to reuse an network element from another NetworkBehaviour
            if (this.internalNetworkElement == null) {
                foreach (NetworkObject obj in this.GetComponents<NetworkObject>()) {
                    this.internalNetworkElement = obj.internalNetworkElement;
                    if (this.internalNetworkElement != null) {
                        this.UpdateFromSource(obj);
                        break;
                    }
                }
                // In case of be a child component, i'm going to find his parent to configure child to be liek owner
                if (this.internalNetworkElement == null) {
                    Transform currentRoot = this.transform;
                    NetworkObject parentObjectElement = null;
                    while (currentRoot.parent != null) {
                        NetworkObject objectElement = currentRoot.parent.GetComponent<NetworkObject>();
                        if (objectElement != null) {
                            parentObjectElement = objectElement;
                        }
                        currentRoot = currentRoot.parent;
                    }
                    if (parentObjectElement != null) {
                        this.internalNetworkElement = parentObjectElement.networkElement;
                        if (this.internalNetworkElement != null) {
                            this.UpdateFromSource(parentObjectElement);
                        }
                    }
                }
            }
            return this.internalNetworkElement;
        }

        /// <summary>
        /// Initializes the network elements associated with this instance.
        /// </summary>
        /// <param name="networkId">The network ID to associate with the network element. Defaults to DEFAULT_NETWORK_ID.</param>
        /// <returns>Instance of created network element</returns>
        private INetworkElement CreateNetworkElements(int networkId = DEFAULT_NETWORK_ID) {
            // Check if the network element has not been created yet
            if (this.networkElement == null) {                
                // Create a new NetworkElement with the provided parameters
                this.networkElement = new NetworkElement(this,
                                                         NetworkManager.Container,
                                                         this.gameObject,
                                                         this.behaviorMode,
                                                         networkId);
                // Initialize the data stream for the network element
                this.networkElement.SetWritterStream(new DataStream());
                this.networkElement.GetWritterStream().Allocate(OBJECT_PACKET_SIZE);
                // Set the transport socket for the network element
                this.networkElement.SetTransport(this.transport.GetSocket());
                // Configure the network element with the delivery and behavior modes
                this.networkElement.SetDeliveryMode(this.deliveryMode);
                // Set whether the network element represents a player
                this.networkElement.SetIsPlayer(this.isPlayer);
                // Configure ownership mode
                this.networkElement.SetOwnershipAccessLevel(this.ownerShipAccessLevel);
                // Flag if minimun send rate is enabled
                this.networkElement.SetEnableMinimunRate(this.isPlayer && (NetworkManager.Instance().GetMinimumSendRate() > 0));
                // Configure minimum rate update
                this.networkElement.SetMinimunRateValue(NetworkManager.Instance().GetMinimumSendRate());
                // Configure the method used for sending object events
                this.networkElement.ConfigureSendMethod(this.SendObjectEvent);
            } else if (DEFAULT_NETWORK_ID != networkId) {
                // If the network element exists but the network ID is different, update it
                this.networkElement.SetNetworkId(networkId);
            }
            // Determine if this player is the owner (only for Server player)
            this.networkElement.SetOwner(this.networkElement.IsOwner() ||
                                         this.networkElement.GetGameObject().GetComponent<NetworkMasterPlayer>());

            // Initialize network controllers for animation and audio if they are not already set
            if (this.animationController == null) {
                this.animationController = new NetworkAnimationController(this.networkElement);
                this.animationController.Initialize();
            }
            if (this.audioController == null) {
                this.audioController = new NetworkAudioController(this.networkElement);
                this.audioController.Intialize();
            }
            // Initialize network input controller
            if (this.input == null) {
                this.input = new NetworkInput(this.networkElement,
                                              this.networkElement.IsActive(),
                                              NetworkManager.Instance().IsRemoteInputEnabled());
            }
            // Update all network behaviours belonging to the same GameObject
            this.SynchronizeSiblingsObjects();
            // Initialize synchronized fields and collect network variables
            // Note: System can accept mode than one NetworkBehaviour peer object, this means that i need to iteract over all NetworkObject into this
            foreach (NetworkObject obj in this.GetComponents<NetworkObject>()) {
                obj.InitializeSynchonizedFields();
                obj.CollectNetworkVariables();
                obj.ConfigureBehaviors();
            }            
            this.ConfigureBehaviors();
            // Configure the active and passive rates for the network element
            this.networkElement.SetActiveRate(this.sendRateAmount);
            this.networkElement.SetPassiveRate(0);

            // Configure behaviour mode
            this.SetBehaviorMode(this.behaviorMode, this.isPlayer == false);

            // Mark the network as initialized
            this.networkInitialized = true;

            return this.networkElement;
        }

        /// <summary>
        /// Collects network variables from the current instance using reflection.
        /// </summary>
        private void CollectNetworkVariables() {
            // Define the binding flags to search for fields in the current type
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            // Retrieve the list of fields from the current type
            var variablesList = this.GetType()
                                    .GetFields(bindingFlags)
                                    .ToList()
                                    .OrderBy(v => v.Name).ToList();
            // Get from bae class
            Type baseClass = this.GetType().BaseType;
            while ((baseClass != null) && (baseClass.Equals(typeof(NetworkBehaviour))) == false) {
                variablesList.AddRange(baseClass.GetFields(BindingFlags.Public |
                                                           BindingFlags.NonPublic |
                                                           BindingFlags.Instance |
                                                           BindingFlags.DeclaredOnly).ToList<FieldInfo>());
                baseClass = baseClass.BaseType;
            }
            // Iterate over each field and add it to the variables dictionary if it implements IVariable
            foreach (FieldInfo variable in variablesList) {
                if (typeof(IVariable).IsAssignableFrom(variable.FieldType)) {
                    IVariable locatedVariable = variable.GetValue(this) as IVariable;
                    if (locatedVariable != null) {
                        locatedVariable.SetOrigin(NetworkVariableOrigin.Wrapped);
                        locatedVariable.SetDeliveryMode(this.GetSynchonizedFieldDeliveryMode(variable.Name));
                        locatedVariable.SetDetectionMode(this.GetSynchonizedFieldDetectionMode(variable.Name));
                        locatedVariable.SetComponent(this);
                        locatedVariable.SetVariableName(variable.Name);
                        this.variables.Add(locatedVariable.GetUniqueId(), locatedVariable);
                        this.variablesVersion.Add(locatedVariable.GetUniqueId(), (locatedVariable != null) ? locatedVariable.GetVersion() : 0);
                    } else {
                        Debug.LogError(string.Format("The network variable \"{0}\" was not initialized on class \"{1}\"", variable.Name, this.GetType().Name));
                    }
                }
            }
        }

        /// <summary>
        /// Generate a method signature hash with parameters included
        /// Note : This is used to avoid methods collision in case of methods overload
        /// </summary>
        /// <param name="method">Relfection method info</param>
        /// <returns>generated signature</returns>
        private string GetUniqueMethodSignature(MethodInfo method) {
            string sig = $"{method.DeclaringType.FullName}.{method.Name}"
                         + "(" + string.Join(",", method.GetParameters()
                                       .Select(p => p.ParameterType.FullName))
                        + ")";
            return sig;
        }

        /// <summary>
        /// Collects network variables from the current instance using reflection.
        /// </summary>
        public void UpdateSynchonizedVariables() {
            // Define the binding flags to search for fields in the current type
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            // If there's no variables do nothing
            if (this.variables.Count == 0) return;
            // Iterate over each field and add it to the variables dictionary if it implements IVariable
            foreach (var variable in this.variables) {
                string variableName = variable.Value.GetVariableName();
                if (string.IsNullOrEmpty(variableName)) {
                    string[] explodedName = variable.Key.Split('.');
                    variableName = explodedName[1];
                    variable.Value.SetVariableName(variableName);
                    variable.Value.SetComponent(this);
                }
                // Cache IVariables
                if (this.cachedVariables.ContainsKey(this.GetType()) == false) {
                    this.cachedVariables.Add(this.GetType(), new Dictionary<string, FieldInfo>());
                }
                if (this.cachedVariables[this.GetType()].ContainsKey(variable.Value.GetVariableName()) == false) {
                    FieldInfo currentField = this.GetType().GetField(variable.Value.GetVariableName(), bindingFlags);
                    // Get from bae class
                    if (currentField == null) {
                        Type baseClass = this.GetType().BaseType;
                        while ((baseClass != null) && (baseClass.Equals(typeof(NetworkBehaviour))) == false) {
                            currentField = baseClass.GetField(variable.Value.GetVariableName(), bindingFlags);
                            if (currentField == null) {
                                baseClass = baseClass.BaseType;
                            } else {
                                break;
                            }
                        }
                    }
                    this.cachedVariables[this.GetType()].Add(variable.Value.GetVariableName(), currentField);
                }
                IVariable currentVariable = (NetworkVariableOrigin.Primitive == variable.Value.GetOrigin()) ? variable.Value : (this.cachedVariables[this.GetType()][variable.Value.GetVariableName()].GetValue(this) as IVariable);
                if ((currentVariable != null) && (variable.Value != null)) {
                    if (!currentVariable.GetVersion().Equals(variable.Value.GetVersion())) {
                        this.variablesVersion[variable.Key] = currentVariable.GetVersion();
                    }
                } else {
                    this.variablesVersion[variable.Key] = 0;
                }
            }
            // Get variable control
            if (this.variabledNetworkControl == null) {
                this.variabledNetworkControl = this.networkElement.GetBehavior<VariablesNetwork>();
            }
            // Now update variables
            if (this.variabledNetworkControl != null) {
                foreach (var variable in this.variablesVersion) {
                    if ((this.variables[variable.Key] == null) ||
                        (this.variables[variable.Key].GetVersion() != variable.Value) ||
                        (this.variabledNetworkControl.HasRegisteredVariable(variable.Key) == false)) {
                        // Get field from cache
                        FieldInfo currentField = this.cachedVariables[this.GetType()][this.variables[variable.Key].GetVariableName()];
                        if (typeof(IVariable).IsAssignableFrom(currentField.FieldType)) {
                            // Get new version
                            IVariable currentVariable = (currentField.GetValue(this) as IVariable);
                            // Update to the new version
                            this.variables[variable.Key] = currentVariable;
                            // Update 
                            this.variabledNetworkControl.RegisterVariable(variable.Key, currentVariable);
                        } else if (this.variabledNetworkControl.HasRegisteredVariable(variable.Key) == false) {
                            this.variabledNetworkControl.RegisterVariable(variable.Key, this.variables[variable.Key]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Executes the network processing if the network has been initialized and the instance was identified.
        /// </summary>
        public void ExecuteNetworkProcess() {
            // Check if the network has been initialized
            if (this.networkInitialized) {
                // Process network element if the instance was identified
                if (this.WasIdentified()) {
                    this.networkElement.Process();
                    // Update rigidbody
                    this.UpdateRigidBody();
                }
            }
        }

        /// <summary>
        /// Update rigidbpdy values according prefab configurations
        /// </summary>
        private void UpdateRigidBody() {
            // Update status according his status
            if (this.physicsCollected == true) {
                if ((this.rigidBody3D != null) ||
                    (this.rigidBody2D != null)) {
                    if (this.IsPassive()) {
                        if (this.passiveApplied == false) {
                            this.passiveApplied = true;
                            this.activeApplied  = false;                            
                            if (this.IsToDisableGravity()) {
                                if (this.rigidBody3D != null) {
                                    if (this.rigidBody3D.useGravity == true) {
                                        this.rigidBody3D.useGravity = false;
                                    }
                                } else if (this.rigidBody3D != null) {
                                    if (Mathf.Abs(this.rigidBody2D.gravityScale) > 0f) {
                                        this.rigidBody2D.gravityScale = 0f;
                                    }
                                }
                            }
                            if (this.IsToEnableKinematic()) {
                                if (this.rigidBody3D != null) {
                                    if (this.rigidBody3D.isKinematic == false) {
                                        this.rigidBody3D.isKinematic = true;
                                    }
                                } else if (this.rigidBody3D != null) {
                                    if (this.rigidBody2D.isKinematic == false) {
                                        this.rigidBody2D.isKinematic = true;
                                    }
                                }
                            }
                        }
                    } else if (this.IsActive()) {
                        if ( this.activeApplied == false ) {
                            this.activeApplied  = true;
                            this.passiveApplied = false;
                            if (this.rigidBody3D != null) {
                                this.rigidBody3D.useGravity     = this.defaultGravity;
                                this.rigidBody3D.isKinematic    = this.defaultKinematic;
                            } else if (this.rigidBody2D != null) {
                                this.rigidBody2D.gravityScale   = this.defaultGravityScale;
                                this.rigidBody2D.isKinematic    = this.defaultKinematic;
                            }
                        }
                    }
                }
            } else {
                this.physicsCollected       = true;
                this.rigidBody3D            = this.gameObject.GetComponent<Rigidbody>();
                this.rigidBody2D            = this.gameObject.GetComponent<Rigidbody2D>();
                this.defaultGravity         = (this.rigidBody3D != null) ? this.rigidBody3D.useGravity : ((this.rigidBody2D != null) && (Mathf.Abs(this.rigidBody2D.gravityScale) > 0f)) ? true : false;
                this.defaultGravityScale    = ((this.rigidBody2D != null) && (Mathf.Abs(this.rigidBody2D.gravityScale) > 0f)) ? this.rigidBody2D.gravityScale : 0f;
                this.defaultKinematic       = (this.rigidBody3D != null) ? this.rigidBody3D.isKinematic : ((this.rigidBody2D != null) && this.rigidBody2D.isKinematic) ? true : false;
            }
        }

        /// <summary>
        /// Checks if the network has been initialized.
        /// </summary>
        /// <returns>True if the network is initialized, otherwise false.</returns>
        public bool IsNetworkInitialized() {
            if (this.networkInitialized == false) {
                INetworkControl networkObject = (this.networkElement != null) ? this.networkElement.GetNetworkObject() : null;
                if (networkObject != null) {
                    this.networkInitialized = (networkObject as NetworkObject).networkInitialized;
                }
            }
            return this.networkInitialized;
        }

        /// <summary>
        /// Checks if the current instance is the owner of the network element.
        /// </summary>
        /// <returns>True if the instance is the owner, otherwise false.</returns>
        public bool IsOwner() {
            return ((this.IsNetworkInitialized()) &&
                    (this.networkElement != null) &&
                    (this.networkElement.IsOwner()));
        }

        /// <summary>
        /// Return true if this is a local Object and belongs to this game instance
        /// 
        /// Note: This method helps to identify if player is local or some object is under this client ( or server ) control
        /// </summary>
        /// <returns>True is this object is local, otherwise false</returns>
        public bool IsLocal() {
            return NetworkManager.Instance().IsRunningLogic() ? this.IsOwner() : this.IsActive();
        }

        /// <summary>
        /// Retrieves the network ID associated with the network element.
        /// </summary>
        /// <returns>The network ID if the network element exists, otherwise DEFAULT_NETWORK_ID.</returns>
        public int GetNetworkId() {
            return (this.networkElement == null) ? DEFAULT_NETWORK_ID : this.networkElement.GetNetworkId();
        }

        /// <summary>
        /// Sets the network ID for the network element if it exists.
        /// </summary>
        /// <param name="networkId">The network ID to set.</param>
        public void SetNetworkId(int networkId) {
            if (this.networkElement != null) {
                this.networkElement.SetNetworkId(networkId);
            }
        }

        /// <summary>
        /// Flag if this object is a respawned object
        /// </summary>
        public void FlagRespawned() {
            this.respawnedObject = true;
        }

        /// <summary>
        /// Return if this object is a respawned instance of this object
        /// </summary>
        /// <returns>true is a respawned object, otherwise false</returns>
        public bool IsRespawned() {
            return this.respawnedObject;
        }

        /// <summary>
        /// Checks if the network element has been created.
        /// </summary>
        /// <returns>True if the network element exists, otherwise false.</returns>
        public bool HasNetworkElement() {
            return (this.networkElement != null);
        }

        /// <summary>
        /// Retrieves the network element associated with the current instance.
        /// </summary>
        /// <returns>The network element if it exists, otherwise null.</returns>
        public INetworkElement GetNetworkElement() {
            return this.networkElement;
        }

        /// <summary>
        /// Sets the network transport and updates the transport for the network element if it exists.
        /// </summary>
        /// <param name="transport">The network transport to set.</param>
        public void SetTransport(NetworkTransport transport) {
            this.transport = transport;
            if (this.networkElement != null) {
                this.networkElement.SetTransport(this.transport.GetSocket());
            }
        }

        /// <summary>
        /// Retrieves the current network transport instance.
        /// </summary>
        /// <returns>The <see cref="NetworkTransport"/> instance associated with this object.</returns>
        public NetworkTransport GetTransport() {
            return this.transport;
        }

        /// <summary>
        /// Sets the behavior mode for the network element and updates the element if it exists.
        /// </summary>
        /// <param name="behaviorMode">The behavior mode to set.</param>
        /// <param name="detectOnStart">Shall to execute network start detection.</param>
        public void SetBehaviorMode(BehaviorMode behaviorMode, bool detectOnStart = true) {
            this.behaviorMode = behaviorMode;
            if (this.networkElement != null) {
                this.networkElement.SetMode(this.behaviorMode);
                if (detectOnStart) {
                    if (this is NetworkBehaviour) {
                        if (BehaviorMode.Unknow.Equals(this.onStartBehaviourMode)) {
                            bool    executeAssignment  = (OwnerShipAccessLevel.Full.Equals(this.GetOwnershipAccessLevel()) || OwnerShipAccessLevel.TakeObject.Equals(this.GetOwnershipAccessLevel()) || OwnerShipAccessLevel.TransferObject.Equals(this.GetOwnershipAccessLevel()));
                                    executeAssignment |= (OwnerShipAccessLevel.ClientOnly.Equals(this.GetOwnershipAccessLevel()) && this.WasTranferReceived());

                            if (executeAssignment) {
                                this.onStartBehaviourMode = behaviorMode;
                                if (NetworkStartOrder.OnModeAssigned.Equals(this.GetStartOrder())) {
                                    // Execute OnNetworkStarted
                                    (this as NetworkBehaviour).OnNetworkStarted();
                                    // Will add on each NetworkBehaviour
                                    foreach (NetworkBehaviour child in this.GetComponentsInChildren<NetworkBehaviour>()) {
                                        if (child != this) {
                                            child.OnNetworkStarted();
                                        }
                                    }
                                    // Execute ActiveStart, PassiveStart, ActiveAwake, PassiveAwake
                                    NetworkInternalExecutor internalExecutor = this.GetComponent<NetworkInternalExecutor>();
                                    if (internalExecutor != null) {
                                        internalExecutor.OnSetModeAwake();
                                        internalExecutor.OnSetModeStart();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialize execution iof internal network behaviors
        /// </summary>
        public void InitializeExecutor() {
            if (this.networkElement != null) {
                if (this.internalExecutor != null) {
                    if (this.internalExecutor.WasInitialize() == false) {
                        this.internalExecutor.InitializeExecutors();                        
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the current behavior mode.
        /// </summary>
        /// <returns>The <see cref="BehaviorMode"/> that is currently set.</returns>
        public BehaviorMode GetBehaviorMode() {
            if (this.networkElement != null) {
                this.behaviorMode = this.networkElement.GetMode();
                return this.networkElement.GetMode();
            } else {
                return this.behaviorMode;
            }
        }

        /// <summary>
        /// Sets the delivery mode for the network element and updates the element if it exists.
        /// </summary>
        /// <param name="deliveryMode">The delivery mode to set.</param>
        public void SetDeliveryMode(DeliveryMode deliveryMode) {
            this.deliveryMode = deliveryMode;
            if (this.networkElement != null) {
                this.networkElement.SetDeliveryMode(this.deliveryMode);
            }
        }

        /// <summary>
        /// Sets the delivery mode for the network element and updates the element if it exists.
        /// </summary>
        /// <param name="deliveryMode">The delivery mode to set.</param>
        public void SetOwnershipAccessLevel(OwnerShipAccessLevel accessLevel) {
            this.ownerShipAccessLevel = accessLevel;            
        }

        /// <summary>
        /// Retrieves the current ownership access level.
        /// </summary>
        /// <returns>The <see cref="OwnerShipAccessLevel"/>Current ownership access level.</returns>
        public OwnerShipAccessLevel GetOwnershipAccessLevel() {
            return this.ownerShipAccessLevel;
        }

        
        /// <summary>
        /// Define the type of movement that object will use to move itself
        /// </summary>
        /// <param name="movementType">Type of movement</param>
        public void SetMovementType(PredictionType movementType) {
            this.movementType = movementType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public PredictionType GetMovementType() {
            return this.movementType;
        }

        /// <summary>
        /// Enables or disables remote controls.
        /// </summary>
        /// <param name="value">True to enable remote controls, false to disable them.</param>
        public void SetRemoteControlsEnabled(bool value) {
            this.remoteControls = value;
        }

        /// <summary>
        /// Sets the synchronization state for animations.
        /// </summary>
        /// <param name="value">True to synchronize animations, false otherwise.</param>
        public void SetSyncAnimation(bool value) {
            this.syncAnimation = value;
        }

        /// <summary>
        /// Sets the synchronization mode for animations.
        /// </summary>
        /// <param name="value">The animation synchronization type to set.</param>
        public void SetSyncAnimationMode(AnimationSyncType value) {
            this.syncAnimationMode = value;
        }

        /// <summary>
        /// Sets the synchronization state for particles.
        /// </summary>
        /// <param name="value">True to synchronize particles, false otherwise.</param>
        public void SetSyncParticles(bool value) {
            this.syncParticles = value;
        }

        /// <summary>
        /// Sets the client associated with this network element.
        /// </summary>
        /// <param name="value">The client to associate with this network element.</param>
        public void SetClient(IClient value) {
            this.client = value;
        }

        /// <summary>
        /// Retrieves the client associated with this network element.
        /// </summary>
        /// <returns>The <see cref="IClient"/> instance associated with this object.</returns>
        public IClient GetClient() {
            return this.client;
        }

        /// <summary>
        /// Sets the send rate and updates the network element's active rate if it exists.
        /// </summary>
        /// <param name="value">The send rate to set.</param>
        public void SetSendRate(int value) {
            this.sendRateAmount = value;
            if (this.networkElement != null) {
                this.networkElement.SetActiveRate(this.sendRateAmount);
            }
        }

        /// <summary>
        /// Sets the count of animation layers.
        /// </summary>
        /// <param name="value">The number of animation layers to set.</param>
        public void SetAnimationLayerCount(int value) {
            this.animationLayerCount = value;
        }

        /// <summary>
        /// Sets the default status for animations.
        /// </summary>
        /// <param name="value">The default animation status to set.</param>
        public void SetAnimationDefaultStatus(string value) {
            this.animationDefaultStatus = value;
        }

        /// <summary>
        /// Checks if the network element has been identified.
        /// </summary>
        /// <returns>True if the network element has been identified, false otherwise.</returns>
        public bool WasIdentified() {
            return this.identified;
        }

        /// <summary>
        /// Sets the identified state of the network element.
        /// </summary>
        /// <param name="value">True to mark as identified, false otherwise.</param>
        public void SetIdentified(bool value) {
            this.identified = value;
        }

        /// <summary>
        /// Sets the state indicating whether this network element represents a player.
        /// </summary>
        /// <param name="value">True if this represents a player, false otherwise.</param>
        public void SetIsPlayer(bool value) {
            this.isPlayer = value;
        }

        /// <summary>
        /// Checks if this network element represents a player.
        /// </summary>
        /// <returns>True if this represents a player, false otherwise.</returns>
        public bool IsPlayer() {
            return this.isPlayer;
        }

        /// <summary>
        /// Determines if the network object is in an active state.
        /// </summary>
        /// <returns>True if the object is active, false otherwise.</returns>
        public bool IsActive() {
            return (BehaviorMode.Active == this.behaviorMode) || (BehaviorMode.Both == this.behaviorMode);
        }

        /// <summary>
        /// Determines if the network object is in a passive state.
        /// </summary>
        /// <returns>True if the object is passive, false otherwise.</returns>
        public bool IsPassive() {
            return (BehaviorMode.Passive == this.behaviorMode) || (BehaviorMode.Both == this.behaviorMode);
        }

        /// <summary>
        /// Set ownership access level
        /// </summary>
        /// <param name="level">Level to apply</param>
        public void SetAccessLevel(OwnerShipAccessLevel level) {
            this.ownerShipAccessLevel = level;
        }

        /// <summary>
        /// Return ownership access level
        /// </summary>
        /// <returns>Current ownership access level</returns>
        public OwnerShipAccessLevel GetAccessLevel() {
            return this.ownerShipAccessLevel;
        }

        /// <summary>
        /// Teleport network object to some position without inteporlating his movement
        /// </summary>
        /// <param name="position">Position to teleport object</param>
        /// <param name="teleportTime">Optional time duration of teleporting to ensure that object will be teleported</param>
        public void Teleport(Vector3 position, float teleportTime = TELEPORT_DURATION) { 
            if ( this.HasNetworkElement() ) {
                if ( this.GetNetworkElement().HasBehavior<PositionNetwork>() ) {
                    // On active instances the object will be immediatly moved and other instances will detect this
                    if (this.IsActive()) {
                        this.GetNetworkElement().GetBehavior<PositionNetwork>().Teleport(position, teleportTime);
                    } else if (this.IsPassive()) {
                        this.TeleportControl(position, teleportTime); // Request to teleport on server, or into active instance
                    }
                } else {
                    NetworkDebugger.LogError("Object {0} can't be teleported since they havent \"PositionNetwork\" associated", this.gameObject.name);
                }
            }
        }

        /// <summary>
        /// Return synchronize position
        /// </summary>
        public bool[] GetSyncPosition() {
            if ((this.syncPosition == null) || (this.syncPosition.Length == 0)) {
                this.syncPosition = new bool[3] { true, true, true };
            }
            return this.syncPosition;
        }

        /// <summary>
        /// Set synchronize position
        /// </summary>
        /// <param name="axys"></param>
        public void SetSyncPosition(bool[] axys) {
            this.syncPosition[0] = axys[0];
            this.syncPosition[1] = axys[1];
            this.syncPosition[2] = axys[2];
        }

        /// <summary>
        /// Return synchronize rotation
        /// </summary>
        public bool[] GetSyncRotation() {
            if ((this.syncRotation == null) || (this.syncRotation.Length == 0)) {
                this.syncRotation = new bool[3] { true, true, true };
            }
            return this.syncRotation;
        }

        /// <summary>
        /// Set synchronize rotation
        /// </summary>
        /// <param name="axys">Axys</param>
        public void SetSyncRotation(bool[] axys) {
            this.syncRotation[0] = axys[0];
            this.syncRotation[1] = axys[1];
            this.syncRotation[2] = axys[2];
        }

        /// <summary>
        /// Return synchronize scale
        /// </summary>
        public bool[] GetSyncScale() {
            if ((this.syncScale == null) || (this.syncScale.Length == 0)) {
                this.syncScale = new bool[3] { true, true, true };
            }
            return this.syncScale;
        }

        /// <summary>
        /// Set synchronize scale
        /// </summary>
        /// <param name="axys">Axys</param>
        public void SetSyncScale(bool[] axys) {
            this.syncScale[0] = axys[0];
            this.syncScale[1] = axys[1];
            this.syncScale[2] = axys[2];
        }

        /// <summary>
        /// Set prefab to disable gravity on passive instances
        /// </summary>
        /// <param name="value">Disable gravity value</param>
        public void SetDisableGravity(bool value) {
            this.disableObjectGravity = value;
        }

        /// <summary>
        /// Return if is to disable gravity on passive instance
        /// </summary>
        /// <returns>Is to disbale gravity</returns>
        public bool IsToDisableGravity() {
            return this.disableObjectGravity;
        }

        /// <summary>
        /// Set prefab to enable kinematic on passive instances
        /// </summary>
        /// <param name="value">Enable kinematic value</param>
        public void SetEnableKinematic(bool value) {
            this.enableObjectKinematic = value;
        }

        /// <summary>
        /// Return if is to enable kinematic on passive instance
        /// </summary>
        /// <returns>True if is to enable kinematic</returns>
        public bool IsToEnableKinematic() {
            return this.enableObjectKinematic;
        }

        /// <summary>
        /// Set prefab to enable "RigidbodyInterpolation.Interpolate"
        /// </summary>
        /// <param name="value">Enable RigidbodyInterpolation.Interpolate</param>
        public void SetEnablePhysicsInterpolation(bool value) {
            this.enablePhysicsInterpolation = value;
        }

        /// <summary>
        /// Return if is to enable RigidbodyInterpolation.Interpolate
        /// </summary>
        /// <returns>True if is to enable RigidbodyInterpolation.Interpolate</returns>
        public bool IsToEnablePhysicsInterpolation() {
            return this.enablePhysicsInterpolation;
        }

        /// <summary>
        /// Set to use prefab rotation
        /// </summary>
        /// <param name="value">use prefab rotation</param>
        public void SetToUsePrefabRotation(bool value) {
            this.usePrefabRotation = value;
        }

        /// <summary>
        /// Return if will use prefab rotation
        /// </summary>
        /// <returns>True if use prefab rotation</returns>
        public bool IsToUsePrefabRotation() {
            return this.usePrefabRotation;
        }

        /// <summary>
        /// Initializes the serializable attributes for position, rotation, and scale synchronization.
        /// </summary>
        public void InitializeSerializableAttributes() {
            if (this.syncPosition == null) {
                this.syncPosition = new bool[3] { true, true, true };
            }
            if (this.syncRotation == null) {
                this.syncRotation = new bool[3] { true, true, true };
            }
            if (this.syncScale == null) {
                this.syncScale = new bool[3] { true, true, true };
            }
        }

        /// <summary>
        /// Checks if an event with the specified event code is present.
        /// </summary>
        /// <param name="eventCode">The event code to check for.</param>
        /// <returns>True if the event exists, otherwise false.</returns>
        public bool HasEvent(int eventCode) {
            return this.networkElement.HasEvent(eventCode);
        }

        /// <summary>
        /// Return fi is running logic
        /// </summary>
        /// <returns>True is is running logicv, otherwise false</returns>
        public bool IsRunningLogic() {
            return NetworkManager.Instance().IsRunningLogic();
        }

        /// <summary>
        /// Return manuela execution tick used to control if some manual update was request for network variables
        /// </summary>
        /// <returns>Manual tick update</returns>
        public int GetManualExecutionTick() {
            return NetworkManager.Instance().GetManualExecutionTick();
        }

        /// <summary>
        /// Return manuela execution tick used to control if some manual update was request for network variables
        /// </summary>
        /// <returns>Manual tick update for reliable variables</returns>
        public int GetReliableExecutionTick() {
            return NetworkManager.Instance().GetReliableExecutionTick();
        }
        
        /// <summary>
        /// Return clients that is out of date and  need to receive update of relianle variables
        /// </summary>
        /// <param name="executionTick">Execution tick to check variables.</param>
        /// <returns>List of pending clients to update</returns>
        public List<KeyValuePair<IClient, int>> GetClientsToUpdateVariables(int executionTick) {
            return NetworkManager.Instance().GetClientsToUpdateVariables(executionTick);
        }

        /// <summary>
        /// Registers a callback action for a specific event code.
        /// </summary>
        /// <param name="eventCode">The event code to register the callback for.</param>
        /// <param name="callBack">The callback action to be invoked when the event occurs.</param>
        /// <param name="broadcast">This event shall be broadcast to all players.</param>
        public void RegisterEvent(int eventCode, Action<IDataStream> callBack, bool broadcast = false) {
            // Network object event codes are transformed to a negative value
            this.networkElement.RegisterEvent(UserCustomObjectEvents.ToObjectEvent(eventCode), callBack);
            // If event shall be broadcast i need to register it
            if ( broadcast ) {
                NetworkManager.RegisterBroadcastEvent(UserCustomObjectEvents.ToObjectEvent(eventCode));                
            }
        }

        /// <summary>
        /// Registers a callback action for a specific event code.
        /// </summary>
        /// <param name="eventCode">The event code to register the callback for.</param>
        /// <param name="callBack">The callback action to be invoked when the event occurs.</param>
        /// <param name="broadcast">This event shall be broadcast to all players.</param>
        public void RegisterObjectEvent(int eventCode, Action<IDataStream> callBack, bool broadcast = false) {
            this.networkElement.RegisterEvent(eventCode, callBack);
            // If event shall be broadcast i need to register it
            if (broadcast) {
                NetworkManager.RegisterBroadcastEvent(eventCode);
            }
        }

        /// <summary>
        /// Unregisters a previously registered event using its event code.
        /// </summary>
        /// <param name="eventCode">The event code to unregister.</param>
        public void UnregisterEvent(int eventCode) {
            // Network object event codes are transformed to a negative value
            this.networkElement.UnregisterEvent(UserCustomObjectEvents.ToObjectEvent(eventCode));
            // JUst in case of being registered as broadcast event
            NetworkManager.UnregisterBroadcastEvent(UserCustomObjectEvents.ToObjectEvent(eventCode));
        }

        /// <summary>
        /// Invokes an event with the specified event code and passes a data stream to it.
        /// </summary>
        /// <param name="eventCode">The event code to invoke.</param>
        /// <param name="reader">The data stream to pass to the event.</param>
        public void InvokeEvent(int eventCode, IDataStream reader) {
            this.networkElement.InvokeEvent(eventCode, reader);
        }

        /// <summary>
        /// Retrieves the input value of a specified type and name.
        /// </summary>
        /// <typeparam name="T">The type of the input value to retrieve.</typeparam>
        /// <param name="name">The name of the input to retrieve.</param>
        /// <returns>The input value of the specified type and name.</returns>
        public T GetInput<T>(string name) {
            return this.input.GetInput<T>(name);
        }

        /// <summary>
        /// Registers an input with a specified name and type.
        /// </summary>
        /// <typeparam name="T">The type of the input to register.</typeparam>
        /// <param name="name">The name of the input to register.</param>
        /// <returns>An interface to the registered input entry.</returns>
        public IInputEntry<T> RegisterInput<T>(string name) {
            return this.input.RegisterInput<T>(name);
        }

        /// <summary>
        /// Generates and initializes inputs for the network element based on the active state.
        /// </summary>
        /// <param name="active">Indicates whether the inputs should be active.</param>
        /// <returns>A new instance of InputNetwork with the initialized inputs.</returns>
        private InputNetwork GenerateInputs(bool active) {
            // Iterate through all managed inputs and register them with their respective types
            foreach (ManagedInput input in NetworkManager.Instance().GetManagedInputs()) {
                Type managedType = input.GetInputType();
                string inputProviderMethod = input.GetMethod();
                if (!string.IsNullOrEmpty(inputProviderMethod)) {
                    if (managedType == typeof(bool)) {
                        this.RegisterInput<bool>(input.GetInputName()).OnEvaluate(() => {
                            return NetworkManager.Instance().GetCurrentInputValue<bool>(input.GetMethod());
                        });
                    } else if (managedType == typeof(float)) {
                        this.RegisterInput<float>(input.GetInputName()).OnEvaluate(() => {
                            return NetworkManager.Instance().GetCurrentInputValue<float>(input.GetMethod());
                        });
                    } else if (managedType == typeof(Vector2)) {
                        this.RegisterInput<Vector2>(input.GetInputName()).OnEvaluate(() => {
                            return NetworkManager.Instance().GetCurrentInputValue<Vector2>(input.GetMethod());
                        });
                    }
                } else {
                    NetworkDebugger.LogError("The configured input [{0}] doesn't have a method provider", input.GetInputName());
                }
            }
            // Return a new InputNetwork instance with the registered inputs and specified active state
            return new InputNetwork(this.input, active);
        }


        /// <summary>
        /// This will send a messages targered to this object on network
        /// 
        /// This event will be redirected to the target network object
        /// </summary>
        /// <param name="eventCode"></param>
        /// <param name="writer"></param>
        /// <param name="mode"></param>
        public void Send(int eventCode, DataStream writer, DeliveryMode mode = DeliveryMode.Unreliable) {
            // Note: Write on inverse order of parameters to put ID first and then event code ( but the real order will be [ EVENT CODE -> NETWORK OBJECT ID ] )

            // Open space to write Network ID
            writer.ShiftRight(0, sizeof(int));
            writer.Write(this.networkElement.GetNetworkId(), 0); // Write network ID

            // Open space to write object event code
            writer.ShiftRight(0, sizeof(int));
            writer.Write(UserCustomObjectEvents.ToObjectEvent(eventCode), 0); // Write event code

            // Send buffer
            this.GetTransport().GetSocket().Send(writer.GetBuffer(), mode);
        }

        /// <summary>
        /// This will send a messages targered to this object on network
        /// 
        /// This event will be redirected to the target network object
        /// </summary>
        /// <param name="eventCode"></param>
        /// <param name="writer"></param>
        /// <param name="mode"></param>
        public void SendObjectEvent(int eventCode, DataStream writer, DeliveryMode mode = DeliveryMode.Unreliable, IClient client = null) {
            // Note: Write on inverse order of parameters to put ID first and then event code ( but the real order will be [ EVENT CODE -> NETWORK OBJECT ID ] )

            // Open space to write Network ID
            writer.ShiftRight(0, sizeof(int));
            writer.Write(this.networkElement.GetNetworkId(), 0); // Write network ID

            // Open space to write object event code
            writer.ShiftRight(0, sizeof(int));
            writer.Write(eventCode, 0); // Write event code

            // Open space to write event code ( Internal EventCode ID )
            writer.ShiftRight(0, sizeof(int));
            writer.Write(InternalGameEvents.ObjectEvent, 0); // Write event code used to identify event that need to be executed into NetworkObject scope

            // Send buffer
            if (client == null) {
                this.GetTransport().GetSocket().Send(writer.GetBuffer(), mode);
            } else {
                client.Send(writer.GetBuffer(), mode);
            }
        }

        /// <summary>
        /// Configures the behaviors for network synchronization.
        /// </summary>
        private void ConfigureBehaviors() {
            if (this.behaviorInitialized == false) {
                this.behaviorInitialized = true;

                // Variables network
                // Note: I'm going to register only in case of has any variable to sync, otherwise do not make sense at all
                if (this.variables.Count > 0) {
                    this.networkElement.RegisterBehavior(new VariablesNetwork());
                    this.networkElement.GetBehavior<VariablesNetwork>().SetComponentTarget(this);
                    this.networkElement.GetBehavior<VariablesNetwork>().InitializeEventsCallback(this.HasEvent, 
                                                                                                 this.RegisterObjectEvent, 
                                                                                                 this.GetClientsToUpdateVariables, 
                                                                                                 this.IsRunningLogic,
                                                                                                 this.GetManualExecutionTick,
                                                                                                 this.GetReliableExecutionTick); 
                    this.networkElement.GetBehavior<VariablesNetwork>().InitalizeReliableEvents();
                    // Register all variables
                    foreach (var variableEntry in this.variables) {
                        this.networkElement.GetBehavior<VariablesNetwork>().RegisterVariable(variableEntry.Key, variableEntry.Value);
                    }
                }
                // Position network
                if (this.syncPosition[X_POSITION_IN_ARRAY] ||
                    this.syncPosition[Y_POSITION_IN_ARRAY] ||
                    this.syncPosition[Z_POSITION_IN_ARRAY]) {
                    bool isClientConnectionOfGame = ((NetworkManager.Instance().IsClientConnection() == true) ||
                                                     ((NetworkManager.Instance().IsConnectedOnRelayServer() && NetworkManager.Instance().IsMasterPlayer()) == false));
                    this.networkElement.RegisterBehavior(new PositionNetwork(this.syncPosition[X_POSITION_IN_ARRAY],
                                                                             this.syncPosition[Y_POSITION_IN_ARRAY],
                                                                             this.syncPosition[Z_POSITION_IN_ARRAY],
                                                                             NetworkManager.Instance().UseMovementPrediction(),     // To enable movement prediction
                                                                             NetworkManager.Instance().UseInterpolation(),          // To enable position interpolation to move object smoothly
                                                                             this.IsToEnablePhysicsInterpolation(),                 // To set RigidbodyInterpolation to "RigidbodyInterpolation.Interpolate"
                                                                             NetworkManager.Instance().GetPredictionTechnique(),    // To configure prediction type ( Transform or physics )
                                                                             (PredictionType.Automatic.Equals(this.GetMovementType()) ? NetworkManager.Instance().GetMovementTechnique() : this.GetMovementType()),
                                                                             !isClientConnectionOfGame));
                    if (NetworkManager.Instance().HasPredictionOverride()) {
                        this.networkElement.GetBehavior<PositionNetwork>().SetCustomPrediction(NetworkManager.Instance().GetPredictionOverride());
                    }
                } else if (this.networkElement.HasBehavior<PositionNetwork>()) {
                    this.networkElement.UnregisterBehavior<PositionNetwork>();
                }

                // Rotation network
                if (this.syncRotation[X_POSITION_IN_ARRAY] ||
                    this.syncRotation[Y_POSITION_IN_ARRAY] ||
                    this.syncRotation[Z_POSITION_IN_ARRAY]) {
                    this.networkElement.RegisterBehavior(new RotationNetwork(this.syncRotation[X_POSITION_IN_ARRAY],
                                                                             this.syncRotation[Y_POSITION_IN_ARRAY],
                                                                             this.syncRotation[Z_POSITION_IN_ARRAY]));
                } else if (this.networkElement.HasBehavior<RotationNetwork>()) {
                    this.networkElement.UnregisterBehavior<RotationNetwork>();
                }

                // Scale network
                if (this.syncScale[X_POSITION_IN_ARRAY] ||
                    this.syncScale[Y_POSITION_IN_ARRAY] ||
                    this.syncScale[Z_POSITION_IN_ARRAY]) {
                    this.networkElement.RegisterBehavior(new ScaleNetwork(this.syncScale[X_POSITION_IN_ARRAY],
                                                                          this.syncScale[Y_POSITION_IN_ARRAY],
                                                                          this.syncScale[Z_POSITION_IN_ARRAY]));
                } else if (this.networkElement.HasBehavior<ScaleNetwork>()) {
                    this.networkElement.UnregisterBehavior<ScaleNetwork>();
                }
                // Animation network
                // Check if need to synchronize animations
                if (this.syncAnimation) {
                    this.networkElement.RegisterBehavior(new AnimationNetwork().SetSyncronizationMode(this.syncAnimationMode)
                                                                               .SetLayerCount(this.animationLayerCount)
                                                                               .SetDefaultStatus(this.animationDefaultStatus));
                }

                // Particles network
                // Check if need to synchronize particles system
                if (this.syncParticles) {
                    this.networkElement.RegisterBehavior(new ParticlesNetwork());
                }

                // Input network
                if (NetworkManager.Instance().IsRunningLogic()) {
                    // To not generate inputs is remote input isn't enabled
                    if (NetworkManager.Instance().UseRemoteInput()) {
                        this.ConfigureNetworkInput();
                    }
                }
                // Register childs synchronize
                GameObjectStatus[]  childInformations = NetworkManager.Instance().GetChildsToSynchronizeTransform(this.gameObject);
                if (childInformations.Length > 0) {
                    this.networkElement.RegisterBehavior(new ChildsTransformNetwork());
                    foreach (GameObjectStatus childToSync in childInformations) {
                        this.networkElement.GetBehavior<ChildsTransformNetwork>().RegisterChild(childToSync.TargetInstance, 
                                                                                                childToSync.SyncPosition, 
                                                                                                childToSync.SyncRotation, 
                                                                                                childToSync.SyncScale);
                    }
                }
            }
        }

        /// <summary>
        /// Configures the network input behavior.
        /// </summary>
        public void ConfigureNetworkInput() {
            this.networkElement.RegisterDeviceBehavior(this.GenerateInputs(this.remoteControls));
            this.networkElement.GetDeviceBehavior<InputNetwork>().SetLogicInverted(this.remoteControls);
            this.networkElement.SetToComputeRemoteDevice(this.remoteControls);
            this.networkElement.SetToComputeLocalDevice(this.networkElement.IsOwner());
        }


        /// <summary>
        /// This method create all network variables from variables flagged to be synchronized
        /// </summary>
        public virtual void InitializeSynchonizedFields() {
            // Do nothing
        }

        /// <summary>
        /// Register a network variuable to be synchronized on clients
        /// </summary>
        /// <param name="variableName">Variable name to be synchronized</param>
        /// <param name="variable">Network variable</param>
        public virtual void RegisterSynchronizedVariable(string variableName, IVariable variable) {
            variable.SetVariableName(variableName);
            variable.SetComponent(this);
            if (!this.variables.ContainsKey(variable.GetUniqueId())) {
                this.variables.Add(variable.GetUniqueId(), variable);
                this.variablesVersion.Add(variable.GetUniqueId(), (variable != null) ? variable.GetVersion() : 0);
            }
        }

        /// <summary>
        /// Configure OnSpawnPrefab event
        /// </summary>
        /// <param name="eventCallback">Callback event method</param>
        public void OnSpawnPrefab(EventReference eventCallback) {
            this.onSpawnPrefab = eventCallback;
        }

        /// <summary>
        /// Execute callback when object was spawned
        /// </summary>
        public void ExecuteOnSpawnPrefab() {
            if (this.onSpawnPrefab != null) {
                if ((this.onSpawnPrefab.GetEventTarget() != null) &&
                    (this.onSpawnPrefab.GetEventComponent() != null) &&
                    (this.onSpawnPrefab.GetEventMethod() != null)) {
                    MethodInfo executionMethod = this.onSpawnPrefab.GetEventComponent().GetType().GetMethod(this.onSpawnPrefab.GetEventMethod());
                    if (executionMethod != null) {
                        executionMethod.Invoke(this.onSpawnPrefab.GetEventComponent(), new object[] { this });
                    }
                }
            }            
        }

        /// <summary>
        /// Configure OnDespawnPrefab event
        /// </summary>
        /// <param name="eventCallback">Callback event method</param>
        public void OnDespawnPrefab(EventReference eventCallback) {
            this.onDespawnPrefab = eventCallback;
        }

        /// <summary>
        /// Execute callback when object was despawned
        /// </summary>
        public void ExecuteOnDespawnPrefab() {
            if (this.onDespawnPrefab != null) {
                if ((this.onDespawnPrefab.GetEventTarget() != null) &&
                    (this.onDespawnPrefab.GetEventComponent() != null) &&
                    (this.onDespawnPrefab.GetEventMethod() != null)) {
                    MethodInfo executionMethod = this.onDespawnPrefab.GetEventComponent().GetType().GetMethod(this.onDespawnPrefab.GetEventMethod());
                    if (executionMethod != null) {
                        executionMethod.Invoke(this.onDespawnPrefab.GetEventComponent(), new object[] { this });
                    }
                }
            }
        }

        /// <summary>
        /// Configure OnAcceptOwnerShip event
        /// </summary>
        /// <param name="eventCallback">Callback event method</param>
        public void OnAcceptOwnerShip(EventReference eventCallback) {
            this.onAcceptOwnerShip = eventCallback;
        }

        /// <summary>
        /// Execute method who test if object ownership can be acccepted
        /// </summary>
        /// <param name="spawnedObject">Object to check</param>
        /// <returns>True if accepted</returns>
        private bool GetAcceptOwnerShipResult(NetworkObject spawnedObject) {
            bool result = true;
            if (this.onAcceptOwnerShip != null) {
                if ((this.onAcceptOwnerShip.GetEventTarget() != null) &&
                    (this.onAcceptOwnerShip.GetEventComponent() != null) &&
                    (this.onAcceptOwnerShip.GetEventMethod() != null)) {
                    MethodInfo executionMethod = this.onAcceptOwnerShip.GetEventComponent().GetType().GetMethod(this.onAcceptOwnerShip.GetEventMethod());
                    if (executionMethod != null) {
                        object invokeResult = executionMethod.Invoke(this.onAcceptOwnerShip.GetEventComponent(), new object[] { spawnedObject });
                        result = (bool)invokeResult;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Configure OnAcceptReleaseOwnerShip event
        /// </summary>
        /// <param name="eventCallback">Callback event method</param>
        public void OnAcceptReleaseOwnerTakeShip(EventReference eventCallback) {
            this.onAcceptReleaseOwnerShip = eventCallback;
        }

        /// <summary>
        /// Execute method who test if object ownership can be acccepted
        /// </summary>
        /// <param name="acceptObject">Object to check</param>
        /// <returns>True if accepted</returns>
        private bool GetAcceptOwnerShipReleaseResult(NetworkObject acceptObject) {
            bool result = true;
            if (this.onAcceptReleaseOwnerShip != null) {
                if ((this.onAcceptReleaseOwnerShip.GetEventTarget() != null) &&
                    (this.onAcceptReleaseOwnerShip.GetEventComponent() != null) &&
                    (this.onAcceptReleaseOwnerShip.GetEventMethod() != null)) {
                    MethodInfo executionMethod = this.onAcceptReleaseOwnerShip.GetEventComponent().GetType().GetMethod(this.onAcceptReleaseOwnerShip.GetEventMethod());
                    if (executionMethod != null) {
                        object invokeResult = executionMethod.Invoke(this.onAcceptReleaseOwnerShip.GetEventComponent(), new object[] { acceptObject });
                        result = (bool)invokeResult;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Configure OnTakeObjectOwnerShip event
        /// </summary>
        /// <param name="eventCallback">Callback event method</param>
        public void OnTakeObjectOwnerShip(EventReference eventCallback) {
            this.onTakeObjectOwnerShip = eventCallback;
        }

        /// <summary>
        /// Execute callback when object was taken
        /// </summary>
        public void ExecuteOnTakeObjectOwnerShip() {
            if (this.onTakeObjectOwnerShip != null) {
                if ((this.onTakeObjectOwnerShip.GetEventTarget() != null) &&
                    (this.onTakeObjectOwnerShip.GetEventComponent() != null) &&
                    (this.onTakeObjectOwnerShip.GetEventMethod() != null)) {
                    MethodInfo executionMethod = this.onTakeObjectOwnerShip.GetEventComponent().GetType().GetMethod(this.onTakeObjectOwnerShip.GetEventMethod());
                    if (executionMethod != null) {
                        executionMethod.Invoke(this.onTakeObjectOwnerShip.GetEventComponent(), new object[] { this });
                    }
                }
            }
        }

        /// <summary>
        /// Configure OnReleaseObjectOwnerShip event
        /// </summary>
        /// <param name="eventCallback">Callback event method</param>
        public void OnReleaseObjectOwnerShip(EventReference eventCallback) {
            this.onReleaseObjectOwnerShip = eventCallback;
        }

        /// <summary>
        /// Execute callback when object was released
        /// </summary>
        public void ExecuteOnReleaseObjectOwnerShip() {
            if (this.onReleaseObjectOwnerShip != null) {
                if ((this.onReleaseObjectOwnerShip.GetEventTarget() != null) &&
                    (this.onReleaseObjectOwnerShip.GetEventComponent() != null) &&
                    (this.onReleaseObjectOwnerShip.GetEventMethod() != null)) {
                    MethodInfo executionMethod = this.onReleaseObjectOwnerShip.GetEventComponent().GetType().GetMethod(this.onReleaseObjectOwnerShip.GetEventMethod());
                    if (executionMethod != null) {
                        executionMethod.Invoke(this.onReleaseObjectOwnerShip.GetEventComponent(), new object[] { this });
                    }
                }
            }
        }

        /// <summary>
        /// Executes a parameterless action remotely.
        /// </summary>
        /// <param name="method">The action to be executed remotely.</param>
        public void NetworkExecute(Action method) {
            this.ExecuteMethodOnLocalAndSend(this.GetUniqueMethodSignature(method.Method));
        }

        /// <summary>
        /// Executes a single-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T">The type of the parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecute<T>(Action<T> method, params object[] args) {
            this.ExecuteMethodOnLocalAndSend(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a two-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecute<T0, T1>(Action<T0, T1> method, params object[] args) {
            this.ExecuteMethodOnLocalAndSend(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a three-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecute<T0, T1, T2>(Action<T0, T1, T2> method, params object[] args) {
            this.ExecuteMethodOnLocalAndSend(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a four-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecute<T0, T1, T2, T3>(Action<T0, T1, T2, T3> method, params object[] args) {
            this.ExecuteMethodOnLocalAndSend(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a five-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecute<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> method, params object[] args) {
            this.ExecuteMethodOnLocalAndSend(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a six-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecute<T0, T1, T2, T3, T4, T5>(Action<T0, T1, T2, T3, T4, T5> method, params object[] args) {
            this.ExecuteMethodOnLocalAndSend(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a seven-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecute<T0, T1, T2, T3, T4, T5, T6>(Action<T0, T1, T2, T3, T4, T5, T6> method, params object[] args) {
            this.ExecuteMethodOnLocalAndSend(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a eight-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecute<T0, T1, T2, T3, T4, T5, T6, T7>(Action<T0, T1, T2, T3, T4, T5, T6, T7> method, params object[] args) {
            this.ExecuteMethodOnLocalAndSend(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes an nine-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <typeparam name="T8">The type of the ninth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecute<T0, T1, T2, T3, T4, T5, T6, T7, T8>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> method, params object[] args) {
            this.ExecuteMethodOnLocalAndSend(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a ten-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <typeparam name="T8">The type of the ninth parameter for the action.</typeparam>
        /// <typeparam name="T9">The type of the tenth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecute<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> method, params object[] args) {
            this.ExecuteMethodOnLocalAndSend(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a parameterless action remotely on server or host
        /// </summary>
        /// <param name="method">The action to be executed remotely.</param>
        public void NetworkExecuteOnClient(Action method) {
            this.InternalExecuteMethodOnClient(this.GetUniqueMethodSignature(method.Method));
        }

        /// <summary>
        /// Executes a single-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T">The type of the parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnClient<T>(Action<T> method, params object[] args) {
            this.InternalExecuteMethodOnClient(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a two-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnClient<T0, T1>(Action<T0, T1> method, params object[] args) {
            this.InternalExecuteMethodOnClient(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a two-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnClient<T0, T1, T2>(Action<T0, T1, T2> method, params object[] args) {
            this.InternalExecuteMethodOnClient(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a three-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnClient<T0, T1, T2, T3>(Action<T0, T1, T2, T3> method, params object[] args) {
            this.InternalExecuteMethodOnClient(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a four-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnClient<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> method, params object[] args) {
            this.InternalExecuteMethodOnClient(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a five-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnClient<T0, T1, T2, T3, T4, T5>(Action<T0, T1, T2, T3, T4, T5> method, params object[] args) {
            this.InternalExecuteMethodOnClient(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a six-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnClient<T0, T1, T2, T3, T4, T5, T6>(Action<T0, T1, T2, T3, T4, T5, T6> method, params object[] args) {
            this.InternalExecuteMethodOnClient(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a seven-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnClient<T0, T1, T2, T3, T4, T5, T6, T7>(Action<T0, T1, T2, T3, T4, T5, T6, T7> method, params object[] args) {
            this.InternalExecuteMethodOnClient(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes an eight-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <typeparam name="T8">The type of the ninth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnClient<T0, T1, T2, T3, T4, T5, T6, T7, T8>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> method, params object[] args) {
            this.InternalExecuteMethodOnClient(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a nine-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <typeparam name="T8">The type of the ninth parameter for the action.</typeparam>
        /// <typeparam name="T9">The type of the tenth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnClient<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> method, params object[] args) {
            this.InternalExecuteMethodOnClient(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a parameterless action remotely on server only
        /// </summary>
        /// <param name="method">The action to be executed remotely.</param>
        public void NetworkExecuteOnServer(Action method) {
            this.InternalExecuteMethodOnServer(this.GetUniqueMethodSignature(method.Method));
        }

        /// <summary>
        /// Executes a single-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T">The type of the parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnServer<T>(Action<T> method, params object[] args) {
            this.InternalExecuteMethodOnServer(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a two-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnServer<T0, T1>(Action<T0, T1> method, params object[] args) {
            this.InternalExecuteMethodOnServer(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a two-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnServer<T0, T1, T2>(Action<T0, T1, T2> method, params object[] args) {
            this.InternalExecuteMethodOnServer(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a three-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnServer<T0, T1, T2, T3>(Action<T0, T1, T2, T3> method, params object[] args) {
            this.InternalExecuteMethodOnServer(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a four-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnServer<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> method, params object[] args) {
            this.InternalExecuteMethodOnServer(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a five-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnServer<T0, T1, T2, T3, T4, T5>(Action<T0, T1, T2, T3, T4, T5> method, params object[] args) {
            this.InternalExecuteMethodOnServer(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a six-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnServer<T0, T1, T2, T3, T4, T5, T6>(Action<T0, T1, T2, T3, T4, T5, T6> method, params object[] args) {
            this.InternalExecuteMethodOnServer(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a seven-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnServer<T0, T1, T2, T3, T4, T5, T6, T7>(Action<T0, T1, T2, T3, T4, T5, T6, T7> method, params object[] args) {
            this.InternalExecuteMethodOnServer(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes an eight-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <typeparam name="T8">The type of the ninth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnServer<T0, T1, T2, T3, T4, T5, T6, T7, T8>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> method, params object[] args) {
            this.InternalExecuteMethodOnServer(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Executes a nine-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <typeparam name="T8">The type of the ninth parameter for the action.</typeparam>
        /// <typeparam name="T9">The type of the tenth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        public void NetworkExecuteOnServer<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> method, params object[] args) {
            this.InternalExecuteMethodOnServer(this.GetUniqueMethodSignature(method.Method), args);
        }

        /// <summary>
        /// Execute method on local instance and send over network
        /// </summary>
        /// <param name="methodName">Name of method to be executed</param>
        /// <param name="arguments">Method arguments ( if appliable )</param>
        private void ExecuteMethodOnLocalAndSend(string methodName, params object[] arguments) {
            // First execute on local
            this.InternalExecuteMethod(CRC32Utils.ComputeHash(methodName), arguments);
            // Then send to be executed on remote instance ( Server or CLient )
            this.InternalExecuteRemoteMethod(methodName, true, arguments);
        }

        /// <summary>
        /// Execute method method on server instance
        /// </summary>
        /// <param name="methodName">Name of method to be executed</param>
        /// <param name="arguments">Method arguments ( if appliable )</param>
        private void InternalExecuteMethodOnServer(string methodName, params object[] arguments) {
            // If in not host will send over the network
            if (!NetworkManager.Instance().IsRunningLogic()) {
                this.InternalExecuteRemoteMethod(methodName, false, arguments);
            } else {
                // If is host will execute locally
                this.InternalExecuteMethod(CRC32Utils.ComputeHash(methodName), arguments);
            }
        }

        /// <summary>
        /// Execute method method on client instance
        /// </summary>
        /// <param name="methodName">Name of method to be executed</param>
        /// <param name="arguments">Method arguments ( if appliable )</param>
        private void InternalExecuteMethodOnClient(string methodName, params object[] arguments) {
            // If is not wehre logic is running will send execution over the network
            if (NetworkManager.Instance().IsRunningLogic()) {
                this.InternalExecuteRemoteMethod(methodName, false, arguments); 
            }
        }

        /// <summary>
		/// Execute method on local instance of game
		/// </summary>
		/// <param name="methodHashCode">Hash Code of method name to be execute</param>
		/// <param name="arguments">Method arguments ( if appliable )</param>
		private async void InternalExecuteMethod(uint methodHashCode, params object[] arguments) {
            //make sure the hashing process is not going on before checking the stored hashes
            //else two networkexecute will try to hash all methods at the sametime causing coliision 
            while (this.hashingMethodsCodesExecuting) await Task.Yield();
            
            if ( this == null ) return;

            // first check if the method was stored before to save memory
            if (this.methodsHashCodes.TryGetValue(methodHashCode, out MethodInfo method)){
                // if the method to be executed is not null
                if (method != null){
                    method.Invoke(this, arguments);
                    return;
                }
            } else if (this.hashingMethodsCodesComplete){
                NetworkDebugger.LogError(string.Format("The method you are trying to network execute does not exist in component name: " + GetType().Name));
                return;
            }

            // if not found in the dictionary store every executable method into the MethodHashCodes dictionary
            // start the hashing prosses, this will only ever be hashed once 
            this.hashingMethodsCodesExecuting = true;
            MethodInfo finalMethodToExecute = null;
            //get all methods in the NetworkObject type
            var methodsToExecute = this.GetType().GetMethods(BindingFlags.DeclaredOnly |
                                                             BindingFlags.NonPublic    |
                                                             BindingFlags.Instance     |
                                                             BindingFlags.Public).ToList();


            // Get from base class
            Type baseClass = this.GetType().BaseType;
            while ((baseClass != null) && (baseClass.Equals(typeof(NetworkBehaviour))) == false) {
                methodsToExecute.AddRange(baseClass.GetMethods(BindingFlags.Public |
                                                               BindingFlags.NonPublic |
                                                               BindingFlags.Instance |
                                                               BindingFlags.DeclaredOnly).ToList());
                baseClass = baseClass.BaseType;
            }

            // add all the methods with there script refrence
            foreach (MethodInfo _method in methodsToExecute){
                //ignore Common MonoBehavior methods like Start, Awake ,OnEnable so less methods are computed resulting in less collision posibilities
                if (this.internalMethodsToExclude.Contains<string>(_method.Name)) { continue; }
                //combine both component and methd hashes in a bit shift to create a unique code for every method and script
                uint hashcode = CRC32Utils.ComputeHash(this.GetUniqueMethodSignature(_method));
                //check if the method was not added before first
                if (!this.methodsHashCodes.ContainsKey(hashcode)) {
                    this.methodsHashCodes.Add(hashcode, _method);
                } else {//if a collision occur which can happen in 0.00000000001% chance, notifie the developer to change the name of the method
                    NetworkDebugger.LogError(string.Format("Unfortunately the method \"{0}\" signature is colliding with another method, you will need to rename this method name to anything else to fix this collision problem.", _method.Name));
                    continue;
                }
                //Save the method result that has the same Hash Code recieved from the DataStream to execute after the hash finish
                if (hashcode == methodHashCode){
                    finalMethodToExecute = _method;
                }
            }
            // end the hashing process now any new NetworkExcecute can access the stored hashcodes
            this.hashingMethodsCodesExecuting = false;
            //execute the method outside the hashing process so the process complete as fast as possible without delay
            if (finalMethodToExecute != null){
                finalMethodToExecute.Invoke(this, arguments);
            }else {
                NetworkDebugger.LogError(string.Format("The method you are trying to network execute does not exist in component name: " + GetType().Name));
            }

            this.hashingMethodsCodesComplete = true;
        }


		/// <summary>
		/// Execute method on remote instance ( send over the network )
		/// </summary>
		/// <param name="methodName">Name of method to be executed</param>
		/// <param name="propagate">Shall to propagate this method to all connected clients</param>
		/// <param name="arguments">Method arguments ( if appliable )</param>
		private async void InternalExecuteRemoteMethod(string methodName, bool propagate, params object[] arguments) {
            if ( NetworkManager.Instance().HasConnection() ) {
                while (this.GetClient() == null) await Task.Yield(); // Wait to stablish connection before to try to execute
            }
            // Send an event to execute this method remotelly on client(s)
            using (DataStream writer = new DataStream()) {
				writer.Write(CRC32Utils.ComputeHash(methodName)); // Method HashCode extracted from method name
                writer.Write(propagate); // Shall to propagate to all clients
                writer.Write((byte)arguments.Count()); // Number of arguments
                // write each argument
                foreach (object argument in arguments) {
                    Type datatype = argument.GetType().IsEnum? Enum.GetUnderlyingType(argument.GetType()) : argument.GetType();
                    if (writer.IsValidType(datatype)) {
                        // Write type
                        writer.Write(writer.GetTypeIndex(datatype));
                        // Write value
                        writer.Write(argument, datatype);
                    } else {
                        throw new Exception(string.Format("The method \"{0}\" has invalid type \"{1}\" as argument", methodName, argument.GetType()));
                    }
                }
                this.Send(UserCustomObjectEvents.REMOTE_METHOD_EXECUTION + childObjectIndex, writer, DeliveryMode.Reliable);
            }
        }

        /// <summary>
        /// Callback to be executed when method invoke was arrived
        /// </summary>
        /// <param name="reader">Network object reader</param>
        private void OnRemoteMethodInvoked(IDataStream reader) {
            // Read method Information
            uint        methodHash      = reader.Read<uint>();
            bool        propagate       = reader.Read<bool>();
            int         argumentsCount  = reader.Read<byte>();
            object[]    args            = new object[argumentsCount];
            int         index           = 0;
            while (argumentsCount > 0) {
                Type valueType  = reader.GetType(reader.Read<ushort>());
                args[index++]   = reader.Read(valueType);
                argumentsCount--;
            }
            // First i need to propagate to all client's
            if ((NetworkManager.Instance().IsRunningLogic()) &&
                (!NetworkManager.Instance().IsConnectedOnRelayServer())) {
                if (propagate) {
                    IClient clientOrigin = (reader as INetworkStream).GetClient();
                    foreach (NetworkClient client in this.transport.GetSocket().GetConnectedClients()) {
                        if (clientOrigin != client) {
                            if (NetworkManager.Instance().InRelayMode()) {
                                client.Transmit(reader.GetBuffer(), DeliveryMode.Reliable);
                            } else {
                                client.Send(reader.GetBuffer(), DeliveryMode.Reliable);
                            }                            
                        }
                    }
                }
            }
            // Now im'm going to Invoke received method
            this.InternalExecuteMethod(methodHash, args);
        }
        
        /// <summary>
        /// Callback to be executed when method invoke was arrived on relay server
        /// </summary>
        /// <param name="reader">Network object reader</param>
        private void OnRemoteMethodInvokedOnRelay(IDataStream reader) {
            // propagare to all client's
            IClient clientOrigin = (reader as INetworkStream).GetClient();
            foreach (NetworkClient client in this.transport.GetSocket().GetConnectedClients()) {
                client.Transmit(reader.GetBuffer(), DeliveryMode.Reliable);
            }
        }

        /// <summary>
        /// Teleport control to another posiution immediadly
        /// </summary>
        /// <param name="position">Position to teleport</param>
        private void TeleportControl(Vector3 position, float duration = 0.0f) {
            using (DataStream writer = new DataStream()) {
                writer.Write(this.GetNetworkId());  // Object to tranfer
                writer.Write(position);             // Element id of player that will receive this object
                writer.Write(duration);             // Duration of teleport
                // Send to the client
                this.Send(UserCustomObjectEvents.ObjectTeleport, writer, DeliveryMode.Reliable);
            }
        }

        private void OnOwnerShipTake(IDataStream reader) {
            bool executeTake = NetworkManager.Instance().IsOwnerShipAllowed() ||
                               (NetworkManager.Instance().IsOwnerShipUsingPrefab() && (OwnerShipAccessLevel.Full.Equals(this.ownerShipAccessLevel) ||
                                                                                       OwnerShipAccessLevel.TakeObject.Equals(this.ownerShipAccessLevel)));
            executeTake &= !this.isPlayer;
            executeTake &= this.GetAcceptOwnerShipResult(this);
            if (executeTake) {
                int  objectNetworkId     = reader.Read<int>();
                bool requestedByServer   = reader.Read<bool>();
                bool sendAnswerBack = this.IsActive(); 
                this.SetBehaviorMode(BehaviorMode.Passive); // Update ownership
                if (NetworkManager.Instance().IsRunningLogic()) {
                    // Send back to the client that he has ownership now
                    NetworkClient clientOrigin = ((reader as INetworkStream).GetClient() as NetworkClient);
                    using (DataStream writer = new DataStream()) {
                        writer.Write(this.GetNetworkId());
                        // Send back to the client
                        clientOrigin.Send(UserCustomObjectEvents.TakeControlSucess, writer, DeliveryMode.Reliable);
                    }
                    // Tell to the other clients to release control of this object if they has
                    if (!NetworkManager.Instance().IsConnectedOnRelayServer()) {
                        foreach (NetworkClient client in this.transport.GetSocket().GetConnectedClients()) {
                            if (!client.Equals(clientOrigin)) {
                                client.Send(reader.GetBuffer(), DeliveryMode.Reliable);
                            }
                        }
                    }
                    // Register into client
                    clientOrigin.RegisterControl(this);
                } else if (requestedByServer) {
                    // Send back to the client that he has ownership now
                    NetworkClient clientOrigin = ((reader as INetworkStream).GetClient() as NetworkClient);
                    using (DataStream writer = new DataStream()) {
                        writer.Write(this.GetNetworkId());
                        // Send back to the client
                        clientOrigin.Send(UserCustomObjectEvents.TakeControlSucess, writer, DeliveryMode.Reliable);
                    }
                }
            }
        }

        private void OnOwnerShipTakeOnRelay(IDataStream reader) {
            IClient clientOrigin = (reader as INetworkStream).GetClient();
            foreach (NetworkClient client in this.transport.GetSocket().GetConnectedClients()) {
                if (client.Equals(clientOrigin) == false) {
                    client.Transmit(reader.GetBuffer(), DeliveryMode.Reliable);
                }
            }
        }

        private void OnOwnerShipTakeSucess(IDataStream reader) {
            this.SetBehaviorMode(BehaviorMode.Active);
            // Execute network prefab callback
            this.ExecuteOnTakeObjectOwnerShip();
        }

        private void OnOwnerShipTakeSucessOnRelay(IDataStream reader) {
            IClient clientOrigin = (reader as INetworkStream).GetClient();
            foreach (NetworkClient client in this.transport.GetSocket().GetConnectedClients()) {
                if (client.Equals(clientOrigin) == false) {
                    client.Transmit(reader.GetBuffer(), DeliveryMode.Reliable);
                }
            }
        }

        private void OnOwnerShipRelease(IDataStream reader) {
            bool executeRelease = NetworkManager.Instance().IsOwnerShipAllowed() ||
                                  (NetworkManager.Instance().IsOwnerShipUsingPrefab() && !OwnerShipAccessLevel.ClientOnly.Equals(this.ownerShipAccessLevel));
            executeRelease &= !this.isPlayer;
            executeRelease &= this.GetAcceptOwnerShipReleaseResult(this);
            if (executeRelease) {
                this.SetBehaviorMode(BehaviorMode.Active); // Update ownership
                                                           // Send back to the client that he has ownership now
                NetworkClient clientOrigin = ((reader as INetworkStream).GetClient() as NetworkClient);
                using (DataStream writer = new DataStream()) {
                    writer.Write(this.GetNetworkId());
                    // Send back to the client
                    clientOrigin.Send(UserCustomObjectEvents.ReleaseControlSucess, writer, DeliveryMode.Reliable);
                }
                // 
                // Unregister into client
                clientOrigin.ReleaseControl(this);
                // Execute network prefab callback
                this.ExecuteOnTakeObjectOwnerShip();
            }
        }

        private void OnOwnerShipReleaseOnRelay(IDataStream reader) {
            IClient clientOrigin = (reader as INetworkStream).GetClient();
            foreach (NetworkClient client in this.transport.GetSocket().GetConnectedClients()) {
                if (client.Equals(clientOrigin) == false) {
                    client.Transmit(reader.GetBuffer(), DeliveryMode.Reliable);
                }
            }
        }

        private void OnOwnerShipReleaseSucess(IDataStream reader) {
            this.SetBehaviorMode(BehaviorMode.Passive);
            // Execute network prefab callback
            this.ExecuteOnReleaseObjectOwnerShip();
        }

        private void OnOwnerShipReleaseSucessOnRelay(IDataStream reader) {
            IClient clientOrigin = (reader as INetworkStream).GetClient();
            foreach (NetworkClient client in this.transport.GetSocket().GetConnectedClients()) {
                if (client.Equals(clientOrigin) == false) {
                    client.Transmit(reader.GetBuffer(), DeliveryMode.Reliable);
                }
            }
        }

        private void OnOwnerDefinedOnRelay(IDataStream reader) {
            IClient clientOrigin = (reader as INetworkStream).GetClient();
            foreach (NetworkClient client in this.transport.GetSocket().GetConnectedClients()) {
                if (client.Equals(clientOrigin) == false) {
                    client.Transmit(reader.GetBuffer(), DeliveryMode.Reliable);
                }
            }
        }

        private void OnOwnerShipTranferredOnRelay(IDataStream reader) {
            bool executeTransfer = NetworkManager.Instance().IsOwnerShipAllowed() ||
                                   (NetworkManager.Instance().IsOwnerShipUsingPrefab() && (OwnerShipAccessLevel.Full.Equals(this.ownerShipAccessLevel) ||
                                                                                           OwnerShipAccessLevel.TransferObject.Equals(this.ownerShipAccessLevel)));
            executeTransfer &= !this.isPlayer;
            if (executeTransfer) {
                int receivedNetworkID     = reader.Read<int>();
                int receivedConnectionID  = reader.Read<int>();
                // Send transfer to target player
                IClient clientOrigin = (reader as INetworkStream).GetClient();
                foreach (NetworkClient client in this.transport.GetSocket().GetConnectedClients()) {
                    if ((client.Equals(clientOrigin) == false) &&
                        (client.GetConnectionId().Equals(receivedConnectionID) == true)) {
                        client.Transmit(reader.GetBuffer(), DeliveryMode.Reliable);
                        break;
                    }
                }
            } else {
                NetworkDebugger.LogWarning("Transfer is disabled");
            }
        }

        private void OnTeleportObjectOnRelay(IDataStream reader) {
            int receivedNetworkID       = reader.Read<int>();
            int receivedConnectionID    = reader.Read<int>();
            // Send transfer to target player
            IClient clientOrigin = (reader as INetworkStream).GetClient();
            foreach (NetworkClient client in this.transport.GetSocket().GetConnectedClients()) {
                if ((client.Equals(clientOrigin) == false) &&
                    (client.GetConnectionId().Equals(receivedConnectionID) == true)) {
                    client.Transmit(reader.GetBuffer(), DeliveryMode.Reliable);
                    break;
                }
            }            
        }

        private void OnTeleportObject(IDataStream reader) {
            int     receivedConnectionID    = reader.Read<int>();
            Vector3 teleportPosition        = reader.Read<Vector3>();
            float   teleportDuration        = reader.Read<float>();
            // If arrives on active instance i'm going to make to teletransport it
            if (this.IsActive()) {
                this.Teleport(teleportPosition, teleportDuration);
            } else if (NetworkManager.Instance().IsRunningLogic() ) {
                // Otherwise send to the final active instance
                IClient clientOrigin = (reader as INetworkStream).GetClient();
                foreach (NetworkClient client in this.transport.GetSocket().GetConnectedClients()) {
                    // Send to Since i have no idea who is controlling this object i'm going to send to all and filter when receive
                    if (client.Equals(clientOrigin) == false) {
                        client.Transmit(reader.GetBuffer(), DeliveryMode.Reliable);
                    }
                }
            }
        }
        

        private void OnOwnerShipTranferred(IDataStream reader) {
            bool executeTransfer = NetworkManager.Instance().IsOwnerShipAllowed() ||
                                   (NetworkManager.Instance().IsOwnerShipUsingPrefab() && (OwnerShipAccessLevel.Full.Equals(this.ownerShipAccessLevel) ||
                                                                                           OwnerShipAccessLevel.TransferObject.Equals(this.ownerShipAccessLevel)));
            executeTransfer &= !this.isPlayer;
            if (executeTransfer) {
                // Extract message data
                int receivedNetworkId       = reader.Read<int>();
                int receivedElementId       = reader.Read<int>();
                NetworkObject playerObject  = NetworkManager.Container.IsRegistered(receivedElementId) ? NetworkManager.Container.GetElement(receivedElementId).GetGameObject().GetComponent<NetworkObject>() : null;
                if ((playerObject != null) && (playerObject.IsPlayer())) {
                    NetworkClient targetClient  = playerObject.IsOwner() ? (playerObject.GetClient() as NetworkClient) : NetworkManager.Instance().GetClient<NetworkClient>(playerObject.GetNetworkElement());
                    int receivedConnectionId    = (targetClient != null) ? targetClient.GetConnectionId() : (playerObject.IsOwner() && NetworkManager.Instance().IsRunningLogic() ? NetworkManager.Instance().GetConnection().GetSocket().GetConnectionID() : 0);
                    int currentConnectionId     = NetworkManager.Instance().GetConnection().GetSocket().GetConnectionID();
                    // Find connection
                    if (NetworkManager.Instance().IsRunningLogic()) {
                        // Check if is the master
                        IClient clientOrigin = (reader as INetworkStream).GetClient();
                        // If ownersghip was tranfered to the master i need only to release and tell to previous owner
                        if (currentConnectionId.Equals(receivedConnectionId)) {
                            clientOrigin.ReleaseControl(this); // Release control
                            // Tell previou owner to release
                            using (DataStream writer = new DataStream()) {
                                writer.Write(this.GetNetworkId());
                                // Send back to the client
                                clientOrigin.Send(UserCustomObjectEvents.ReleaseControlSucess, writer, DeliveryMode.Reliable);
                            }
                            // I'm i'm owner of object i will take the control
                            if (playerObject.IsOwner()) {
                                this.SetBehaviorMode(BehaviorMode.Active); // Update ownership
                                NetworkDebugger.Log("Server took control of object [{0}]", receivedNetworkId);
                            }
                        } else {
                            foreach (NetworkClient client in this.transport.GetSocket().GetConnectedClients()) {
                                if (client.Equals(clientOrigin) == false) {
                                    // Send to the target client
                                    client.Send(reader.GetBuffer(), DeliveryMode.Reliable);
                                    break;
                                }
                            }
                        }
                    } else if (currentConnectionId.Equals(receivedConnectionId)) {
                        NetworkDebugger.Log("[{0}] Will take control of object [{1}]", receivedConnectionId, receivedNetworkId);
                        this.TakeControl(); // If someone said to me to take control, i will
                    }
                } else {
                    NetworkDebugger.LogError("Object can't be transferred to non players objects [{0}]", receivedElementId);
                }
            } else {
                NetworkDebugger.LogWarning("Tranfer is disabled");
            }
        }

        private void OnOwnerDefined(IDataStream reader) {
            this.SetTranferReceived(true);
            this.SetBehaviorMode(this.GetBehaviorMode());
        }

        /// <summary>
        /// Flag is initial object transfer wa already executed 
        /// </summary>
        /// <param name="value">True is was wexecuted, otherwise false</param>
        public void SetTranferReceived(bool value) {
            this.ownerShipTranferReceived = value;
        }

        /// <summary>
        /// Returen if this object was already transferred
        /// </summary>
        /// <returns>true is was transferred, otherwise false</returns>
        public bool WasTranferReceived() {
            return this.ownerShipTranferReceived;
        }

        /// <summary>
        /// Retrieves the current behavior mode.
        /// </summary>
        /// <returns>The <see cref="BehaviorMode"/> that is currently set.</returns>
        public void TakeControl() {
            if (this.IsActive() == false) {
                bool executeTake = NetworkManager.Instance().IsOwnerShipAllowed() ||
                               (NetworkManager.Instance().IsOwnerShipUsingPrefab() && (OwnerShipAccessLevel.Full.Equals(this.ownerShipAccessLevel) ||
                                                                                       OwnerShipAccessLevel.TakeObject.Equals(this.ownerShipAccessLevel)));
                executeTake &= !this.isPlayer;
                if (executeTake) {
                    using (DataStream writer = new DataStream()) {
                        writer.Write(this.GetNetworkId());
                        writer.Write(NetworkManager.Instance().IsRunningLogic()); // Was requested by server ?
                        // Send back to the client
                        this.Send(UserCustomObjectEvents.TakeControl, writer, DeliveryMode.Reliable);
                    }
                }                
            }
        }

        /// <summary>
        /// Retrieves the current behavior mode.
        /// </summary>
        /// <returns>The <see cref="BehaviorMode"/> that is currently set.</returns>
        public void ReleaseControl() {
            if (NetworkManager.Instance().IsRunningLogic() == false) {
                if (this.IsActive() == true) {
                    bool executeRelease = NetworkManager.Instance().IsOwnerShipAllowed() ||
                                          (NetworkManager.Instance().IsOwnerShipUsingPrefab() && !OwnerShipAccessLevel.ClientOnly.Equals(this.ownerShipAccessLevel));
                    executeRelease &= !this.isPlayer;
                    if (executeRelease) {
                        using (DataStream writer = new DataStream()) {
                            writer.Write(this.GetNetworkId());
                            // Send back to the client
                            this.Send(UserCustomObjectEvents.ReleaseControl, writer, DeliveryMode.Reliable);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Transfer control of object to another player.
        /// </summary>
        /// <param name="playerNetworkId">Element id of player that will received object control</param>
        public void TransferControl(int playerNetworkId) {
            bool executeTransfer = NetworkManager.Instance().IsOwnerShipAllowed() ||
                                   (NetworkManager.Instance().IsOwnerShipUsingPrefab() && (OwnerShipAccessLevel.Full.Equals(this.ownerShipAccessLevel) ||
                                                                                           OwnerShipAccessLevel.TransferObject.Equals(this.ownerShipAccessLevel)));
            executeTransfer &= !this.isPlayer;
            if (executeTransfer) {
                using (DataStream writer = new DataStream()) {
                    writer.Write(this.GetNetworkId());  // Object to transfer
                    writer.Write(playerNetworkId);      // Element id of player that will receive this object
                    // Send to the client
                    this.Send(UserCustomObjectEvents.TransferControl, writer, DeliveryMode.Reliable);
                }
            }
        }

        /// <summary>
        /// Transfer control of object to another player.
        /// </summary>
        /// <param name="connectionId">Client that will received object control</param>
        public void TransferControl(IClient target) {
            this.TransferControl((target as NetworkClient).GetNetworkObjectId());
        }

        /// <summary>
        /// Transfer control of object to another player.
        /// </summary>
        /// <param name="target">Network object associated with player that will received object control</param>
        public void TransferControl(INetworkControl target) {
            this.TransferControl((target as NetworkObject).GetNetworkId());
        }

        /// <summary>
        /// Order to client to take control of this object
        /// </summary>
        /// <param name="targetClient">Client that will take control of this object</param>
        public void TransferControlToClient(IClient targetClient) {
            bool executeTake = (NetworkManager.Instance().IsOwnerShipUsingPrefab() && (OwnerShipAccessLevel.ClientOnly == this.ownerShipAccessLevel));
            if (executeTake) {
                using (DataStream writer = new DataStream()) {
                    writer.Write(this.GetNetworkId());
                    // Send back to the client
                    targetClient.Send(UserCustomObjectEvents.TakeControlSucess, writer, DeliveryMode.Reliable);
                }
                this.SetBehaviorMode(BehaviorMode.Passive); // On this case object must be passive since client will be the real owner
                targetClient.RegisterControl(this);

                // Send transfer notification
                foreach (NetworkClient clientToSend in transport.GetSocket().GetConnectedClients()) {
                    using (DataStream writer = new DataStream()) {
                        writer.Write(this.GetNetworkId());
                        writer.Write(targetClient == clientToSend);
                        // Send back to the client
                        clientToSend.Send(UserCustomObjectEvents.OwnerShipDefined, writer, DeliveryMode.Reliable);
                    }
                }

                // Flag as transferred
                this.SetTranferReceived(true);
                // Force to update behaviour mode
                this.SetBehaviorMode(this.GetBehaviorMode());
            }            
        }

        /// <summary>
        /// Return a child component based on his vehavior ID
        /// </summary>
        /// <param name="childId">Child id to find</param>
        /// <typeparam name="T">Obejct type to return</typeparam>
        /// <returns>Instance of child component</returns>
        public T GetNetworkChild<T>(ushort childId) where T : NetworkBehaviour {
            T result = default(T);
            NetworkBehaviour[] componentBehaviours = this.gameObject.GetComponentsInChildren<NetworkBehaviour>();
            foreach (NetworkBehaviour childBehaviour in componentBehaviours) {
                if (childBehaviour.GetBehaviorId() == childId) {
                    result = childBehaviour as T;
                    break;
                }
            }
            return result;
        }

    }
}
#pragma warning restore