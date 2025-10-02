using System;
using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// This class represents a prefab entry on database prefabs
    /// </summary>
    [Serializable]
    public class NetworkPrefabEntry {

        /// <summary>
        /// The identifier
        /// </summary>
        [SerializeField]
        private int Id;

        /// <summary>
        /// The prefab
        /// </summary>
        [SerializeField]
        private GameObject Prefab;

        /// <summary>
        /// The automatic synchronize
        /// </summary>
        [SerializeField]
        private bool AutoSync = true;

        /// <summary>
        /// Synchronize position
        /// </summary>
        [SerializeField]
        protected bool[] syncPosition = new bool[3] { true, true, true };

        /// <summary>
        /// Synchronize rotation
        /// </summary>
        [SerializeField]
        private bool[] syncRotation = new bool[3] { true, true, true };

        /// <summary>
        /// Synchronize scale
        /// </summary>
        [SerializeField]
        private bool[] syncScale = new bool[3] { true, true, true };

        /// <summary>
        /// The animation automatic synchronize
        /// </summary>
        [SerializeField]
        private bool AnimationAutoSync = false;

        /// <summary>
        /// The particles automatic synchronize
        /// </summary>
        [SerializeField]
        private bool ParticlesAutoSync = false;

        /// <summary>
        /// The show scripts
        /// </summary>
        [SerializeField]
        private bool ShowScripts = false;

        /// <summary>
        /// Shall to select all scripts
        /// </summary>
        [SerializeField]
        private bool CheckAllScripts = false;

        /// <summary>
        /// Shall to select all child objects
        /// </summary>
        [SerializeField]
        private bool CheckAllChilds = false;

        /// <summary>
        /// The prefab scripts
        /// </summary>
        [SerializeField]
        private ScriptList PrefabScripts;

        /// <summary>
        /// The show childs
        /// </summary>
        [SerializeField]
        private bool ShowChilds = false;

        /// <summary>
        /// The child objets
        /// </summary>
        [SerializeField]
        private GameObjectList ChildObjets;

        /// <summary>
        /// The show input scripts
        /// </summary>
        [SerializeField]
        private bool ShowInputScripts = false;

        /// <summary>
        /// The input scripts
        /// </summary>
        [SerializeField]
        private ScriptList InputScripts;

        /// <summary>
        /// The show variables
        /// </summary>
        [SerializeField]
        private bool ShowVariables = false;

        /// <summary>
        /// The show events
        /// </summary>
        [SerializeField]
        private bool ShowEvents = false;

        /// <summary>
        /// The show tranform
        /// </summary>
        [SerializeField]
        private bool ShowTranform = false;

        /// <summary>
        /// The show child tree
        /// </summary>
        [SerializeField]
        private bool ShowChildTree = false; 

        /// <summary>
        /// Events associated with this prefab entry
        /// </summary>
        [SerializeField]
        private NetworkPrefabsEventsEntry prefabEvents;

        /// <summary>
        /// The animation synchronize mode
        /// </summary>
        [SerializeField]
        private AnimationSyncType AnimationSyncMode = AnimationSyncType.UseParameters;

        /// <summary>
        /// The animation count
        /// </summary>
        [SerializeField]
        private int AnimationCount = 0;

        /// <summary>
        /// The animation default status
        /// </summary>
        [SerializeField]
        private string AnimationDefaultStatus;

        /// <summary>
        /// The ownership access level
        /// </summary>
        [SerializeField]
        private OwnerShipAccessLevel ownershipAccessLevel = OwnerShipAccessLevel.Full;

        /// <summary>
        /// How object movement will be done
        /// </summary>
        [SerializeField]
        private PredictionType movementType = PredictionType.Automatic;

        /// <summary>
        /// Disable rigidbody gravity on passive instances
        /// </summary>
        [SerializeField]
        private bool disableObjectGravity = true;

        /// <summary>
        /// Define object as Kinematic on passive instances
        /// </summary>
        [SerializeField]
        private bool enableObjectKinematic = true;

        /// <summary>
        /// Define if object will use "RigidbodyInterpolation.Interpolate" mode
        /// </summary>
        [SerializeField]
        private bool enableObjectInterpolate = false;

        /// <summary>
        /// Define if object will use the curren prefab roptation during spawn
        /// </summary>
        [SerializeField]
        private bool usePrefabRotation = false;

        /// <summary>
        /// Define if object has a custom framerate for this specific object
        /// </summary>
        [SerializeField]
        private bool customSendRate = false;

        /// <summary>
        /// Define if object will use the curren prefab roptation during spawn
        /// </summary>
        [SerializeField]
        private int sendRateAmount = 1;

        /// <summary>
        /// Flag if need to update values to default state
        /// </summary>
        [SerializeField]
        private string updateFieldsToDefault = "";

        /// <summary>
        /// Filter to be applied on childs display
        /// </summary>
        [SerializeField]
        private String childsFilter = "";

        /// <summary>
        /// Filter to be applied on scripts display
        /// </summary>
        [SerializeField]
        private String scriptsFilter = "";

        /// <summary>
        /// Constant to store current default value
        /// </summary>
        const string DEFAULT_VALUE_KEY = "v.1";

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkPrefabEntry"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="prefab">The prefab.</param>
        public NetworkPrefabEntry(int id, GameObject prefab) {
            this.Id = id;
            this.Prefab = prefab;            
        }

        /// <summary>
        /// Sets the identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void SetId(int id) {
            this.Id = id;
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetId() {
            return this.Id;
        }

        /// <summary>
        /// Sets the prefab.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetPrefab(GameObject value) {
            this.Prefab = value;
        }

        /// <summary>
        /// Gets the prefab.
        /// </summary>
        /// <returns>GameObject.</returns>
        public GameObject GetPrefab() {
            return this.Prefab;
        }

        /// <summary>
        /// Sets the automatic synchronize.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void SetAutoSync(bool value) {
            this.AutoSync = value;
        }

        /// <summary>
        /// Gets the automatic synchronize.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool GetAutoSync() {
            return this.AutoSync;
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
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetSyncPosition(bool x, bool y, bool z) {
            this.syncPosition[0] = x;
            this.syncPosition[1] = y;
            this.syncPosition[2] = z;
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
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetSyncRotation(bool x, bool y, bool z) {
            this.syncRotation[0] = x;
            this.syncRotation[1] = y;
            this.syncRotation[2] = z;
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
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetSyncScale(bool x, bool y, bool z) {
            this.syncScale[0] = x;
            this.syncScale[1] = y;
            this.syncScale[2] = z;
        }

        /// <summary>
        /// Sets the animation automatic synchronize.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void SetAnimationAutoSync(bool value) {
            this.AnimationAutoSync = value;
        }

        /// <summary>
        /// Gets the animation automatic synchronize.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool GetAnimationAutoSync() {
            return this.AnimationAutoSync;
        }

        /// <summary>
        /// Sets the particles automatic synchronize.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void SetParticlesAutoSync(bool value) {
            this.ParticlesAutoSync = value;
        }

        /// <summary>
        /// Gets the particles automatic synchronize.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool GetParticlesAutoSync() {
            return this.ParticlesAutoSync;
        }

        /// <summary>
        /// Sets the show scripts.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void SetShowScripts(bool value) {
            this.ShowScripts = value;
        }

        /// <summary>
        /// Gets the show scripts.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool GetShowScripts() {
            return this.ShowScripts;
        }

        /// <summary>
        /// Sets the check all scripts.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void SetCheckAllScripts(bool value) {
            this.CheckAllScripts = value;
        }

        /// <summary>
        /// Gets the check all scripts.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool GetCheckAllScripts() {
            return this.CheckAllScripts;
        }
        
        /// <summary>
        /// Sets the check all child objects.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void SetCheckAllChilds(bool value) {
            this.CheckAllChilds = value;
        }

        /// <summary>
        /// Gets the check all child objects.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool GetCheckAllChilds() {
            return this.CheckAllChilds;
        }

        /// <summary>
        /// Gets the prefab scripts.
        /// </summary>
        /// <returns>ScriptList.</returns>
        public ScriptList GetPrefabScripts() {
            return this.PrefabScripts;
        }

        /// <summary>
        /// Sets the prefab scripts.
        /// </summary>
        /// <param name="scriptList">The script list.</param>
        public void SetPrefabScripts(ScriptList scriptList) {
            this.PrefabScripts = scriptList;
        }

        /// <summary>
        /// Gets the input scripts.
        /// </summary>
        /// <returns>ScriptList.</returns>
        public ScriptList GetInputScripts() {
            return this.InputScripts;
        }

        /// <summary>
        /// Sets the input scripts.
        /// </summary>
        /// <param name="scriptList">The script list.</param>
        public void SetInputScripts(ScriptList scriptList) {
            this.InputScripts = scriptList;
        }

        /// <summary>
        /// Sets the show childs.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void SetShowChilds(bool value) {
            this.ShowChilds = value;
        }

        /// <summary>
        /// Gets the show childs.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool GetShowChilds() {
            return this.ShowChilds;
        }

        /// <summary>
        /// Gets the child objects.
        /// </summary>
        /// <returns>GameObjectList.</returns>
        public GameObjectList GetChildObjects() {
            return this.ChildObjets;
        }

        /// <summary>
        /// Sets the child objects.
        /// </summary>
        /// <param name="childList">The child list.</param>
        public void SetChildObjects(GameObjectList childList) {
            this.ChildObjets = childList;
        }

        /// <summary>
        /// Sets the show input scripts.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void SetShowInputScripts(bool value) {
            this.ShowInputScripts = value;
        }

        /// <summary>
        /// Gets the show input scripts.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool GetShowInputScripts() {
            return this.ShowInputScripts;
        }

        /// <summary>
        /// Sets the show variables.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void SetShowVariables(bool value) {
            this.ShowVariables = value;
        }

        /// <summary>
        /// Gets the show variables.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool GetShowVariables() {
            return this.ShowVariables;
        }

        /// <summary>
        /// Sets the show events.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void SetShowEvents(bool value) {
            this.ShowEvents = value;
        }

        /// <summary>
        /// Gets the show events.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool GetShowEvents() {
            return this.ShowEvents;
        }
        
        /// <summary>
        /// Sets the show tranform.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void SetShowTransform(bool value) {
            this.ShowTranform = value;
        }

        /// <summary>
        /// Gets the show child tree.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool GetShowTransform() {
            return this.ShowTranform;
        }

        /// <summary>
        /// Sets the show child tree.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void SetShowChildTree(bool value) {
            this.ShowChildTree = value;
        }

        /// <summary>
        /// Gets the show tranform.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool GetShowChildTree() {
            return this.ShowChildTree;
        }


        /// <summary>
        /// Return events for this prefab entry
        /// </summary>
        /// <returns>Events entry object</returns>
        public NetworkPrefabsEventsEntry GetEvents() {
            return this.prefabEvents;
        }

        /// <summary>
        /// Set the network prefabs events entry
        /// </summary>
        /// <param name="events">Prefabs events</param>
        public void SetEvents(NetworkPrefabsEventsEntry events) {
            this.prefabEvents = events;
        }

        public EventReferencePrefab GetOnSpawnPrefab() {
            return this.prefabEvents.onSpawnPrefab;
        }

        public void SetOnSpawnPrefab(EventReferencePrefab eventReference) {
            this.prefabEvents.onSpawnPrefab = eventReference;
        }

        public EventReferencePrefab GetOnDespawnPrefab() {
            return this.prefabEvents.onDespawnPrefab;
        }

        public EventReferencePrefab GetOnTakeObjectOwnership() {
            return this.prefabEvents.onTakeObjectOwnerShip;
        }

        public EventReferencePrefab GetOnReleaseObjectOwnership() {
            return this.prefabEvents.onReleaseObjectOwnerShip;
        }

        public EventReferencePrefab GetOnAcceptObjectOwnerShip() {
            return this.prefabEvents.onAcceptOwnerShip;
        }

        public EventReferencePrefab GetOnAcceptReleaseObjectOwnerShip() {
            return this.prefabEvents.onAcceptReleaseOwnerShip;
        }


        /// <summary>
        /// Sets the animation synchronize mode.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetAnimationSyncMode(AnimationSyncType value) {
            this.AnimationSyncMode = value;
        }

        /// <summary>
        /// Gets the animation synchronize mode.
        /// </summary>
        /// <returns>AnimationSyncType.</returns>
        public AnimationSyncType GetAnimationSyncMode() {
            return this.AnimationSyncMode;
        }

        /// <summary>
        /// Sets the animation count.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetAnimationCount(int value) {
            this.AnimationCount = value;
        }

        /// <summary>
        /// Gets the animation count.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetAnimationCount() {
            return this.AnimationCount;
        }

        /// <summary>
        /// Sets the animation default status.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetAnimationDefaultStatus(string value) {
            this.AnimationDefaultStatus = value;
        }

        /// <summary>
        /// Gets the animation default status.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetAnimationDefaultStatus() {
            return this.AnimationDefaultStatus;
        }

        /// <summary>
        /// Sets the ownership access level.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetOwnerShipAccessLevel(OwnerShipAccessLevel value) {
            this.ownershipAccessLevel = value;
        }

        /// <summary>
        /// Gets the ownership access level.
        /// </summary>
        /// <returns>OwnerShipAccessLevel.</returns>
        public OwnerShipAccessLevel GetOwnerShipAccessLevel() {
            return this.ownershipAccessLevel;
        }

        /// <summary>
        /// Set current movement type used by network prefab
        /// </summary>
        /// <param name="value">Movement type</param>
        public void SetMovementType(PredictionType value) {
            this.movementType = value;
        }

        /// <summary>
        /// Return current movement type used by network prefab
        /// </summary>
        /// <returns>Movement type</returns>
        public PredictionType GetMovementType() {
            return this.movementType;
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
        /// Set prefab to enable interpolate physics movement on passive instances
        /// </summary>
        /// <param name="value">Enable interpolate physics value</param>
        public void SetEnableInterpolate(bool value) {
            this.enableObjectInterpolate = value;
        }

        /// <summary>
        /// Return if is to enable interpolate physics movement on passive instance
        /// </summary>
        /// <returns>True if is to enable interpolate</returns>
        public bool IsToEnableInterpolate() {
            return this.enableObjectInterpolate;
        }

        /// <summary>
        /// Set to use prefab rotation
        /// </summary>
        /// <param name="value">Flag to use prefab doration</param>
        public void SetToUsePrefabRotation(bool value) {
            this.usePrefabRotation = value;
        }

        /// <summary>
        /// Return if is to enable interpolate physics movement on passive instance
        /// </summary>
        /// <returns>True if is to enable interpolate</returns>
        public bool IsToUsePrefabRotation() {
            return this.usePrefabRotation;
        }
        
        /// <summary>
        /// Define if object shall to use custom rate
        /// </summary>
        /// <param name="value">Flag to use custom rate amount</param>
        public void  SetUseCustomRate(bool value) {
            this.customSendRate = value;
        }

        /// <summary>
        /// Return if this object uise a custom rate amount
        /// </summary>
        /// <returns>True if is to use custom rate amount</returns>
        public bool IsToUseCustomRate() {
            return this.customSendRate;
        }

        /// <summary>
        /// Set the filter to be applied on this prefab to display filter
        /// </summary>
        /// <param name="value">Child name values</param>
        public void SetChildsFilter(string value) {
            this.childsFilter = value;
        }

        /// <summary>
        /// Return child name filter
        /// </summary>
        /// <returns>Child name filter value</returns>
        public string GetChildsFilter() {
            return this.childsFilter;
        }

        /// <summary>
        /// Set the filter to be applied on this scripts to display filter
        /// </summary>
        /// <param name="value">Child name values</param>
        public void SetScriptsFilter(string value) {
            this.scriptsFilter = value;
        }

        /// <summary>
        /// Return scripts name filter
        /// </summary>
        /// <returns>Scripts filter value</returns>
        public string GetScriptsFilter() {
            return this.scriptsFilter;
        }

        /// <summary>
        /// Set to use custom rate amo0unt
        /// </summary>
        /// <param name="value">Rate amount</param>
        public void SetSendRateAmount(int value) {
            this.sendRateAmount = value;
        }

        /// <summary>
        /// Return custom rate amount value
        /// </summary>
        /// <returns>Custom rate amount</returns>
        public int GetSendRateAmount() {
            return this.sendRateAmount;
        }

        /// <summary>
        /// Define new variables to his currenet default values
        /// </summary>
        /// <returns></returns>
        public void SetDefaultValues() {
            this.updateFieldsToDefault      = NetworkPrefabEntry.DEFAULT_VALUE_KEY;
            this.disableObjectGravity       = true;
            this.enableObjectKinematic      = true;
            this.enableObjectInterpolate    = false;
        }

        /// <summary>
        /// Return if need to set default values
        /// </summary>
        /// <returns>True if need to set to default values</returns>
        public bool IsToSetDefaultValues() {
            return (this.updateFieldsToDefault != NetworkPrefabEntry.DEFAULT_VALUE_KEY);
        }
    }

}