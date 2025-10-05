using System;
using System.Collections.Generic;
using KinematicCharacterController;
using PurrNet;
using PurrNet.Prediction;
using UnityEngine;
using UnityEngine.InputSystem;

public class PurrnetPlayerMovement :
    PredictedIdentity<PurrnetPlayerMovement.PlayerInput, PurrnetPlayerMovement.PlayerState>,
    ICharacterController
{
    [Header("Components")]
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget; 
    [SerializeField] private InputActionAsset asset;
    [SerializeField] private GameObject cameraParentPrefab;
    [SerializeField] private GameObject thirdPersonMesh;

    [Header("Movement - Ground")]
    [SerializeField] private float walkSpeed = 20f;
    [SerializeField] private float crouchSpeed = 7f;
    [SerializeField] private float walkResponse = 25f;
    [SerializeField] private float crouchResponse = 20f;

    [Header("Movement - Air")]
    [SerializeField] private float airSpeed = 15f;
    [SerializeField] private float airAcceleration = 70f;
    [SerializeField] private float gravity = -90f;

    [Header("Jumping")]
    [SerializeField] private float jumpSpeed = 20f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private bool isDoubleJumpEnabled = false;
    [SerializeField] private float jumpCooldown = 0.2f;
    [SerializeField] private int maxJumps = 2;
    [Range(0f, 1f)] [SerializeField] private float jumpSustainGravity = 0.4f;

    [Header("Sliding")]
    [SerializeField] private float slideStartSpeed = 25f;
    [SerializeField] private float slideEndSpeed = 15f;
    [SerializeField] private float slideFriction = 0.8f;
    [SerializeField] private float slideSteerAcceleration = 5f;
    [SerializeField] private float slideGravity = -90f;

    [Header("Crouching")]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchHeightResponse = 15f;
    [SerializeField] private bool holdToCrouch = false;
    [Range(0f, 1f)] [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0f, 1f)] [SerializeField] private float crouchCameraTargetHeight = 0.7f;

    [Header("Wall Mechanics")]
    [SerializeField] private float wallRunCheckDistance = 0.7f;
    [SerializeField] private float wallJumpCastDistance = 0.7f;
    [SerializeField] private float wallRunGravity = -30f;
    [SerializeField] private float wallRunMaxTime = 3f;
    [SerializeField] private bool isWallJumpEnabled = false;
    [SerializeField] private bool isWallRunEnabled = false;
    [SerializeField] private float wallCoyoteTime = 0.1f;
    [SerializeField] private float wallJumpVerticalSpeed = 20f;
    [SerializeField] private float wallJumpHorizontalSpeed = 20f;
    [SerializeField] private float wallRunStartSpeed = 22f;
    [Range(0f, 1f)] [SerializeField] private float wallRunVerticalMomentumPreservation = 0.3f;
    [SerializeField] private float wallRunInitialSpeedBoost = 5f;
    [SerializeField] private float wallRunSpeedDecayRate = 0.5f;
    [SerializeField] private float wallStickForce = 5f;
    [SerializeField] private float wallJumpInputLockoutDuration = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    // --- Input & Private Fields ---
    private PurrnetCrouchMode _crouchMode;
    private Collider[] _uncrouchOverlapResults;
    private KCCSettings _kccSettings;
    private List<KinematicCharacterMotor> _motorList = new List<KinematicCharacterMotor>();
    private List<PhysicsMover> _physicsMovers = new List<PhysicsMover>();
    private UnityEngine.InputSystem.PlayerInput _playerInput;
    private InputAction _mLookAction;
    private InputAction _mMoveAction;
    private InputAction _mJumpAction;
    private InputAction _mCrouchAction;
    private bool _inputsInitialized = false;
    private PurrnetCameraHelper cameraHelper;
    private GameObject _cameraParentInstance;

    // Store the current state being simulated (used during KCC callbacks)
    private PlayerState _currentSimulationState;
    private PlayerInput _currentSimulationInput;
    
    // Store the last simulated state for public accessors
    private PlayerState _lastSimulatedState;
    
    // DEBUG: Track predictions vs corrections
    private PlayerState _lastPredictedState;
    private bool _isRestoringState = false;

    public enum Stance { Stand, Crouch, Slide, WallRun }
    public enum PurrnetCrouchMode { Hold, Toggle }

    public struct PlayerInput : IPredictedData
    {
        public Quaternion Look;
        public Vector2 Move;
        public bool Jump;
        public bool JumpSustain;
        public bool CrouchPressed;
        public bool CrouchReleased;

        public void Dispose() { }
    }
    
    public struct PlayerState : IPredictedData<PlayerState>
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 Acceleration;

        public bool Grounded;
        public Stance Stance;
        public Stance PreviousStance;

        public float TimeSinceUngrounded;
        public float TimeSinceJumpRequest;
        public bool UngroundedDueToJump;
        public int NumJumpsUsed;

        public float TimeSinceLeftWall;
        public Vector3 WallJumpNormal;
        public Vector3 WallRunNormal;
        public float WallRunTime;
        public bool IsWallOnLeft;
        public bool IsWallOnRight;
        public Vector3 LeftWallNormal;
        public Vector3 RightWallNormal;

        public float JumpCooldownTimer;
        public float WallJumpInputLockoutTimer;

        public void Dispose() { }
    }
    
    #region State Handling (Purrnet Overrides)

    protected override void GetUnityState(ref PlayerState state)
    {
        state.Position = motor.TransientPosition;
        state.Rotation = motor.TransientRotation;
        state.Velocity = motor.BaseVelocity;
        
        // Store this as our last prediction
        if (!_isRestoringState)
        {
            _lastPredictedState = state;
        }
    }

    protected override void SetUnityState(PlayerState state)
    {
        _isRestoringState = true;
    
        // Compare with our last predicted state to see if server corrected us
        if (enableDebugLogs)
        {
            CompareStates(_lastPredictedState, state, "SERVER CORRECTION");
        }
    
        // Sync the core motor properties from the corrected state
        motor.SetPositionAndRotation(state.Position, state.Rotation);
        motor.BaseVelocity = state.Velocity;
    
        // ✨ [FIX] IMMEDIATELY sync the capsule height to match the corrected stance.
        // This prevents a one-frame visual glitch where the model is the wrong size.
        float targetHeight = (state.Stance == Stance.Stand || state.Stance == Stance.WallRun) 
            ? standHeight 
            : crouchHeight;
        motor.SetCapsuleDimensions(motor.Capsule.radius, targetHeight, targetHeight * 0.5f);

        // ✨ [FIX] Also, ensure the local simulation state is updated with the correction.
        // This provides the next simulation tick with the correct starting point.
        _currentSimulationState = state;

        _isRestoringState = false;
    }

    #endregion

    #region Initialization & Input

    public void Initialize()
    {
        _crouchMode = holdToCrouch ? PurrnetCrouchMode.Hold : PurrnetCrouchMode.Toggle;
        _uncrouchOverlapResults = new Collider[8];

        // Initialize KCC for ALL clients, not just owner
        motor.CharacterController = this;
        _kccSettings = ScriptableObject.CreateInstance<KCCSettings>();
        _kccSettings.AutoSimulation = false;
        _kccSettings.Interpolate = false;

        _motorList.Clear();
        _motorList.Add(motor);
        KinematicCharacterSystem.Settings = _kccSettings;
    }
    private void InitializeInputs()
    {
        if (_inputsInitialized || !isOwner) return;
        
        _playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (_playerInput == null)
        {
            Debug.LogError($"[{name}] PlayerInput component not found!");
            return;
        }

        var map = _playerInput.currentActionMap;
        if (map == null)
        {
            _playerInput.currentActionMap = asset.FindActionMap("Player");
            map = _playerInput.currentActionMap;
        }

        // Find actions - using FindAction which returns null if not found
        _mLookAction = map.FindAction("Look");
        _mMoveAction = map.FindAction("Move");
        _mJumpAction = map.FindAction("Jump");
        _mCrouchAction = map.FindAction("Crouch");

        if (_mLookAction == null || _mMoveAction == null || _mJumpAction == null || _mCrouchAction == null)
        {
            Debug.LogError($"[{name}] One or more input actions not found in action map!");
            return;
        }

        // Enable actions
        _mLookAction.Enable();
        _mMoveAction.Enable();
        _mJumpAction.Enable();
        _mCrouchAction.Enable();

        _inputsInitialized = true;
    }
    

    public override void OnViewOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner)
    {
        Initialize();
        KinematicCharacterSystem.Settings.Interpolate = false;

        if (isOwner)
        {
            // Instantiate camera parent at attach point
            if (_cameraParentInstance == null && cameraParentPrefab != null && cameraTarget != null)
            {
                _cameraParentInstance = Instantiate(
                    cameraParentPrefab,
                    cameraTarget.position,
                    cameraTarget.rotation,
                    cameraTarget
                );

                // Cache the CameraHelper from the new prefab
                cameraHelper = _cameraParentInstance.GetComponentInChildren<PurrnetCameraHelper>();
                if (cameraHelper != null)
                    cameraHelper.Init(this, cameraTarget);
            }
        }
        else
        {
            // No need to destroy, just ensure no camera is attached
            if (_cameraParentInstance != null)
            {
                Destroy(_cameraParentInstance);
                _cameraParentInstance = null;
                cameraHelper = null;
            }
        }

        InitializeInputs();
        
        if(thirdPersonMesh)
            thirdPersonMesh.SetActive(!isOwner);
    }

    protected override void GetFinalInput(ref PlayerInput input)
    {
        if (!isOwner || !_inputsInitialized) return;

        // Get camera look rotation
        input.Look = cameraHelper != null ? cameraHelper.GetLookRotation() : Quaternion.identity;
    
        // Read WASD input from new Input System
        if (_mMoveAction != null)
            input.Move = _mMoveAction.ReadValue<Vector2>();
    }

    protected override void UpdateInput(ref PlayerInput input)
    {
        if (!isOwner || !_inputsInitialized) return;

        if (_mJumpAction != null)
        {
            input.Jump |= _mJumpAction.WasPressedThisFrame();
        }
        
        if (_mCrouchAction != null)
        {
            input.CrouchPressed |= _mCrouchAction.WasPressedThisFrame();
            input.CrouchReleased |= _mCrouchAction.WasReleasedThisFrame();
        }
    }

    protected override void Simulate(PlayerInput input, ref PlayerState state, float delta)
    {
        // if (enableDebugLogs && isOwner)
        // {
        //     Debug.Log($"[Simulate] Move: {input.Move}, Jump: {input.Jump}, Look: {input.Look.eulerAngles.y:F1}°, Stance: {state.Stance}");
        // }

        // Store current state and input for KCC callbacks
        _currentSimulationState = state;
        _currentSimulationInput = input;

        // Apply the state to the motor before simulation
        SetUnityState(state);

        // Handle crouch input
        if (input.CrouchPressed)
        {
            if (_crouchMode == PurrnetCrouchMode.Toggle)
                state.Stance = state.Stance == Stance.Crouch ? Stance.Stand : Stance.Crouch;
            else
                state.Stance = Stance.Crouch;
        }
        else if (input.CrouchReleased && _crouchMode == PurrnetCrouchMode.Hold)
        {
            state.Stance = Stance.Stand;
        }

        // Update the stored state with stance changes
        _currentSimulationState = state;

        // Run the KCC simulation
        KinematicCharacterSystem.Simulate(delta, _motorList, _physicsMovers);

        // Get the state back after simulation
        state = _currentSimulationState;
        
        // Update position/rotation/velocity from motor
        GetUnityState(ref state);
        
        // Store the final state for public accessors
        _lastSimulatedState = state;
        
        // if (enableDebugLogs && isOwner)
        // {
        //     Debug.Log($"[Simulate Result] Pos={state.Position}, Vel={state.Velocity.magnitude:F2}, Grounded={state.Grounded}, Stance={state.Stance}");
        // }
    }

    protected override PlayerState GetInitialState()
    {
        return new PlayerState
        {
            Position = motor.transform.position,
            Rotation = motor.transform.rotation,
            Velocity = motor.Velocity,
            Grounded = motor.GroundingStatus.IsStableOnGround,
            Stance = Stance.Stand,
            PreviousStance = Stance.Stand,
            TimeSinceLeftWall = wallCoyoteTime,
        };
    }
    #endregion
    
    #region KCC Interface Loop

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        var input = _currentSimulationInput;
        var forward = Vector3.ProjectOnPlane(input.Look * Vector3.forward, motor.CharacterUp);
        if (forward.sqrMagnitude > 0f)
            currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        var state = _currentSimulationState;
        var input = _currentSimulationInput;

        // Update timers
        state.TimeSinceUngrounded += deltaTime;
        state.TimeSinceLeftWall += deltaTime;
        if (state.JumpCooldownTimer > 0f) state.JumpCooldownTimer -= deltaTime;
        if (state.WallJumpInputLockoutTimer > 0f) state.WallJumpInputLockoutTimer -= deltaTime;

        // --- CAMERA-RELATIVE MOVEMENT ---
        Vector3 camForward = cameraHelper != null 
            ? Vector3.ProjectOnPlane(cameraHelper.transform.forward, motor.CharacterUp).normalized
            : Vector3.ProjectOnPlane(transform.forward, motor.CharacterUp).normalized;
        Vector3 camRight = cameraHelper != null
            ? Vector3.ProjectOnPlane(cameraHelper.transform.right, motor.CharacterUp).normalized
            : Vector3.ProjectOnPlane(transform.right, motor.CharacterUp).normalized;
        Vector3 requestedMovement = camForward * input.Move.y + camRight * input.Move.x;

        // --- HANDLE MOVEMENT BASED ON STANCE/GROUNDED ---
        if (motor.GroundingStatus.IsStableOnGround)
        {
            HandleGroundedMovement(ref currentVelocity, requestedMovement, deltaTime, ref state);
        }
        else
        {
            HandleAirborneMovement(ref currentVelocity, requestedMovement, deltaTime, ref state);
        }

        // --- HANDLE JUMPING ---
        HandleJumping(ref currentVelocity, input, deltaTime, ref state);

        // --- STORE BACK UPDATED STATE ---
        _currentSimulationState = state;
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        var state = _currentSimulationState;
        if (state.Stance == Stance.Crouch || state.Stance == Stance.Slide)
        {
            if (motor.Capsule.height != crouchHeight)
                motor.SetCapsuleDimensions(motor.Capsule.radius, crouchHeight, crouchHeight * 0.5f);
        }
        else
        {
            if (motor.Capsule.height != standHeight)
                motor.SetCapsuleDimensions(motor.Capsule.radius, standHeight, standHeight * 0.5f);
        }
        _currentSimulationState = state;
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        var state = _currentSimulationState;
        var prevStance = state.PreviousStance;

        if (state.PreviousStance != Stance.Stand && state.Stance == Stance.Stand)
        {
            if (motor.CharacterOverlap(motor.TransientPosition, motor.TransientRotation, _uncrouchOverlapResults, motor.CollidableLayers, QueryTriggerInteraction.Ignore) > 0)
            {
                motor.SetCapsuleDimensions(motor.Capsule.radius, crouchHeight, crouchHeight * 0.5f);
                state.Stance = Stance.Crouch;
            }
        }
    
        state.Grounded = motor.GroundingStatus.IsStableOnGround;
        if (state.Grounded)
        {
            state.TimeSinceUngrounded = 0f;
            state.NumJumpsUsed = 0;
            state.UngroundedDueToJump = false;
        }

        state.Position = motor.TransientPosition;
        state.Rotation = motor.TransientRotation;
        state.Velocity = motor.BaseVelocity;
        state.PreviousStance = state.Stance;

        // if (enableDebugLogs && prevStance != state.Stance)
        // {
        //     Debug.Log($"[AfterCharacterUpdate] Stance changed {prevStance} -> {state.Stance}, Grounded={state.Grounded}");
        // }
    
        _currentSimulationState = state;
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        if (!motor.GroundingStatus.IsStableOnGround && !hitStabilityReport.IsStable && Mathf.Abs(Vector3.Dot(hitNormal, motor.CharacterUp)) < 0.7f)
        {
            var state = _currentSimulationState;
            state.TimeSinceLeftWall = 0f;
            state.WallJumpNormal = hitNormal;
            _currentSimulationState = state;
        }
    }
    
    public void PostGroundingUpdate(float deltaTime)
    {
        var state = _currentSimulationState;
        if (!motor.GroundingStatus.IsStableOnGround && state.Stance == Stance.Slide)
        {
            state.Stance = Stance.Crouch;
        }
        if (motor.GroundingStatus.IsStableOnGround && state.Stance == Stance.WallRun)
        {
            state.Stance = Stance.Stand;
        }
        _currentSimulationState = state;
    }
    
    public void UpdateBody(float deltaTime, PlayerState state)
    {
        var currentHeight = motor.Capsule.height;
        var normalizedHeight = currentHeight / standHeight;
        var cameraTargetHeight = currentHeight * 
                                 (state.Stance == Stance.Stand ? standCameraTargetHeight : crouchCameraTargetHeight);
        var lerpFactor = 1f - Mathf.Exp(-crouchHeightResponse * deltaTime);
    
        cameraTarget.localPosition = Vector3.Lerp(
            cameraTarget.localPosition, 
            new Vector3(0f, cameraTargetHeight, 0f), 
            lerpFactor);

        root.localScale = Vector3.Lerp(
            root.localScale, 
            new Vector3(1f, normalizedHeight, 1f), 
            lerpFactor);
    }

    public bool IsColliderValidForCollisions(Collider coll) => true;
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {}
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) {}
    public void OnDiscreteCollisionDetected(Collider hitCollider) {}

    #endregion

    #region Movement Handlers

    private void HandleGroundedMovement(ref Vector3 currentVelocity, Vector3 requestedMovement, float deltaTime, ref PlayerState state)
    {
        state.WallRunTime = 0f;
        if (state.Stance == Stance.WallRun)
        {
            state.Stance = Stance.Stand;
        }

        var groundedMovement = motor.GetDirectionTangentToSurface(requestedMovement, motor.GroundingStatus.GroundNormal) * requestedMovement.magnitude;

        bool isTryingToSlide = state.Stance == Stance.Crouch && (state.PreviousStance == Stance.Stand || !state.Grounded);
        if (isTryingToSlide && groundedMovement.sqrMagnitude > 0f)
        {
            HandleSlideMovement(ref currentVelocity, requestedMovement, deltaTime, ref state, true);
        }
        else if (state.Stance == Stance.Slide)
        {
            HandleSlideMovement(ref currentVelocity, requestedMovement, deltaTime, ref state, false);
        }
        else
        {
            HandleWalkCrouchMovement(ref currentVelocity, groundedMovement, deltaTime, ref state);
        }
    }

    private void HandleAirborneMovement(ref Vector3 currentVelocity, Vector3 requestedMovement, float deltaTime, ref PlayerState state)
    {
        var input = _currentSimulationInput;
        CheckForDirectionalWallJump(requestedMovement, ref state);
        CheckForWallRun(ref state);

        float horizontalInputDot = Vector3.Dot(requestedMovement, transform.right);
        bool wantsToRunLeft = horizontalInputDot < -0.1f;
        bool wantsToRunRight = horizontalInputDot > 0.1f;
        
        bool isMovingForward = Vector3.Dot(requestedMovement, transform.forward) > 0.1f;

        bool canWallRun = false;
        if (isWallRunEnabled && currentVelocity.y < 5f && isMovingForward)
        {
            if (state.IsWallOnLeft && wantsToRunLeft)
            {
                state.WallRunNormal = state.LeftWallNormal;
                canWallRun = true;
            }
            else if (state.IsWallOnRight && wantsToRunRight)
            {
                state.WallRunNormal = state.RightWallNormal;
                canWallRun = true;
            }
        }

        if (canWallRun)
        {
            HandleWallRun(ref currentVelocity, deltaTime, ref state);
        }
        else
        {
            if (state.Stance == Stance.WallRun)
            {
                state.Stance = Stance.Stand;
            }
            HandleStandardAirMovement(ref currentVelocity, requestedMovement, deltaTime, ref state);
            ApplyGravity(ref currentVelocity, input.JumpSustain, deltaTime, ref state);
        }
    }

    private void HandleJumping(ref Vector3 currentVelocity, PlayerInput input, float deltaTime, ref PlayerState state)
    {
        if (!input.Jump || state.JumpCooldownTimer > 0f)
        {
            if(!input.Jump) state.TimeSinceJumpRequest = 0f;
            return;
        }

        var canCoyoteJump = state.TimeSinceUngrounded < coyoteTime && !state.UngroundedDueToJump;
        var canExtraJump = isDoubleJumpEnabled && (state.NumJumpsUsed < maxJumps);
        var canWallJump = isWallJumpEnabled && state.TimeSinceLeftWall < wallCoyoteTime;

        bool jumpedThisFrame = false;

        if (state.Stance == Stance.WallRun)
        {
            state.Stance = Stance.Stand;
            Vector3 horizontalDir = Vector3.ProjectOnPlane(state.WallRunNormal, motor.CharacterUp).normalized;
            currentVelocity = Vector3.ProjectOnPlane(currentVelocity, state.WallRunNormal);
            currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);
            currentVelocity += horizontalDir * wallJumpHorizontalSpeed;
            currentVelocity += motor.CharacterUp * wallJumpVerticalSpeed;
            state.WallJumpInputLockoutTimer = wallJumpInputLockoutDuration;
            state.NumJumpsUsed++;
            jumpedThisFrame = true;
        }
        else if (canWallJump)
        {
            state.Stance = Stance.Stand;
            Vector3 horizontalDir = Vector3.ProjectOnPlane(state.WallJumpNormal, motor.CharacterUp).normalized;
            currentVelocity = Vector3.ProjectOnPlane(currentVelocity, state.WallJumpNormal);
            currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);
            currentVelocity += horizontalDir * wallJumpHorizontalSpeed;
            currentVelocity += motor.CharacterUp * wallJumpVerticalSpeed;
            state.WallJumpInputLockoutTimer = wallJumpInputLockoutDuration;
            state.NumJumpsUsed++;
            jumpedThisFrame = true;
        }
        else if (state.Grounded || canCoyoteJump || canExtraJump)
        {
            Vector3 jumpDirection = motor.GroundingStatus.FoundAnyGround && !motor.GroundingStatus.IsStableOnGround
                ? motor.GroundingStatus.GroundNormal
                : motor.CharacterUp;
            currentVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            currentVelocity += jumpDirection * jumpSpeed;
            state.NumJumpsUsed = (state.Grounded || canCoyoteJump) ? 1 : state.NumJumpsUsed + 1;
            jumpedThisFrame = true;
        }

        if (jumpedThisFrame)
        {
            state.JumpCooldownTimer = jumpCooldown;
            motor.ForceUnground(0.1f);
            state.UngroundedDueToJump = true;
        }
        else
        {
            state.TimeSinceJumpRequest += deltaTime;
        }
    }
    
    #endregion
    
    #region Specific Movement Logic

    protected override void UpdateView(PlayerState viewState, PlayerState? verified)
    {
        base.UpdateView(viewState, verified);
        UpdateBody(Time.deltaTime, viewState);
    }

    private void HandleWalkCrouchMovement(ref Vector3 currentVelocity, Vector3 groundedMovement, float deltaTime, ref PlayerState state)
    {
        var speed = state.Stance == Stance.Stand ? walkSpeed : crouchSpeed;
        var response = state.Stance == Stance.Stand ? walkResponse : crouchResponse;
        var targetVelocity = groundedMovement * speed;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1f - Mathf.Exp(-response * deltaTime));
    }

    private void HandleWallRun(ref Vector3 currentVelocity, float deltaTime, ref PlayerState state)
    {
        if (state.Stance != Stance.WallRun)
        {
            state.WallRunTime = 0f;
            state.NumJumpsUsed = 0;
            state.Stance = Stance.WallRun;

            Vector3 horizontalVel = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);
            Vector3 wallForward = Vector3.Cross(state.WallRunNormal, motor.CharacterUp);
            if (Vector3.Dot(horizontalVel, wallForward) < 0)
            {
                wallForward = -wallForward;
            }
            float horizontalSpeed = Mathf.Max(horizontalVel.magnitude, wallRunStartSpeed + wallRunInitialSpeedBoost);
            float verticalSpeed = currentVelocity.y * wallRunVerticalMomentumPreservation;
            currentVelocity = (wallForward * horizontalSpeed) + (motor.CharacterUp * verticalSpeed);
        }

        currentVelocity += state.WallRunNormal * -1 * wallStickForce * deltaTime;
        currentVelocity -= currentVelocity.normalized * (wallRunSpeedDecayRate * state.WallRunTime * deltaTime);
        currentVelocity += motor.CharacterUp * (wallRunGravity * deltaTime);
        state.WallRunTime += deltaTime;

        if (state.WallRunTime > wallRunMaxTime)
        {
            state.Stance = Stance.Stand;
        }
    }

    private void HandleStandardAirMovement(ref Vector3 currentVelocity, Vector3 requestedMovement, float deltaTime, ref PlayerState state)
    {
        if (state.WallJumpInputLockoutTimer > 0f) return;
        if (requestedMovement.sqrMagnitude <= 0f) return;
        
        var planarMovement = Vector3.ProjectOnPlane(requestedMovement, motor.CharacterUp);
        var currentPlanarVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);
        var movementForce = planarMovement * (airAcceleration * deltaTime);
        
        if (currentPlanarVelocity.magnitude < airSpeed)
        {
            var target = Vector3.ClampMagnitude(currentPlanarVelocity + movementForce, airSpeed);
            movementForce = target - currentPlanarVelocity;
        }
        else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
        {
            movementForce = Vector3.ProjectOnPlane(movementForce, currentPlanarVelocity.normalized);
        }
        currentVelocity += movementForce;
    }

    private void ApplyGravity(ref Vector3 currentVelocity, bool jumpSustain, float deltaTime, ref PlayerState state)
    {
        var effectiveGravity = gravity;
        if (jumpSustain && Vector3.Dot(currentVelocity, motor.CharacterUp) > 0f)
        {
            effectiveGravity *= jumpSustainGravity;
        }
        currentVelocity += motor.CharacterUp * (effectiveGravity * deltaTime);
    }
    
    private void HandleSlideMovement(ref Vector3 currentVelocity, Vector3 requestedMovement, float deltaTime, ref PlayerState state, bool isStartingSlide)
    {
        state.Stance = Stance.Slide;
        if (isStartingSlide)
        {
            if (!state.Grounded)
            {
                currentVelocity = Vector3.ProjectOnPlane(state.Velocity, motor.GroundingStatus.GroundNormal);
            }
            var startSpeed = Mathf.Max(slideStartSpeed, currentVelocity.magnitude);
            currentVelocity = currentVelocity.normalized * startSpeed;
        }
        else
        {
            currentVelocity -= currentVelocity * (slideFriction * deltaTime);
            var slopeForce = Vector3.ProjectOnPlane(-motor.CharacterUp, motor.GroundingStatus.GroundNormal) * slideGravity;
            currentVelocity -= slopeForce * deltaTime;
            
            var groundedMovement = motor.GetDirectionTangentToSurface(requestedMovement, motor.GroundingStatus.GroundNormal) * requestedMovement.magnitude;
            var currentSpeed = currentVelocity.magnitude;
            var targetVelocity = groundedMovement * currentSpeed;
            var steerForce = (targetVelocity - currentVelocity) * (slideSteerAcceleration * deltaTime);
            
            currentVelocity = Vector3.ClampMagnitude(currentVelocity + steerForce, currentSpeed);
            if (currentSpeed < slideEndSpeed)
            {
                state.Stance = Stance.Crouch;
            }
        }
    }

    private void CheckForDirectionalWallJump(Vector3 requestedMovement, ref PlayerState state)
    {
        if (requestedMovement.sqrMagnitude <= 0f) return;

        Vector3 origin = motor.TransientPosition + motor.CharacterUp * (standHeight * 0.5f);
        Vector3 direction = requestedMovement.normalized;
        if (Physics.SphereCast(origin, motor.Capsule.radius * 0.9f, direction, out RaycastHit hit, wallJumpCastDistance, motor.CollidableLayers, QueryTriggerInteraction.Ignore))
        {
            if (Mathf.Abs(Vector3.Dot(hit.normal, motor.CharacterUp)) < 0.7f)
            {
                state.TimeSinceLeftWall = 0f;
                state.WallJumpNormal = hit.normal;
            }
        }
    }
    
    private void CheckForWallRun(ref PlayerState state)
    {
        Vector3 origin = motor.TransientPosition + motor.CharacterUp * (standHeight * 0.5f);
        state.IsWallOnLeft = Physics.SphereCast(origin, motor.Capsule.radius * 0.9f, -transform.right, out RaycastHit leftHit, wallRunCheckDistance, motor.CollidableLayers, QueryTriggerInteraction.Ignore);
        if (state.IsWallOnLeft)
        {
            state.LeftWallNormal = leftHit.normal;
        }

        state.IsWallOnRight = Physics.SphereCast(origin, motor.Capsule.radius * 0.9f, transform.right, out RaycastHit rightHit, wallRunCheckDistance, motor.CollidableLayers, QueryTriggerInteraction.Ignore);
        if (state.IsWallOnRight)
        {
            state.RightWallNormal = rightHit.normal;
        }
    }
    
    #endregion

    #region Debug State Comparison

    private void CompareStates(PlayerState predicted, PlayerState verified, string context)
    {
        // Define thresholds to avoid logging tiny floating-point inaccuracies
        const float posThreshold = 0.01f;
        const float velThreshold = 1f;
    
        // --- Position Check ---
        float posDiff = Vector3.Distance(predicted.Position, verified.Position);
        if (posDiff > posThreshold)
        {
            Debug.LogWarning($"[{context}] Position Mismatch! Difference: {posDiff:F4}. Predicted: {predicted.Position}, Verified: {verified.Position}");
        }

        // --- Velocity Check ---
        float velDiff = Vector3.Distance(predicted.Velocity, verified.Velocity);
        if (velDiff > velThreshold)
        {
            Debug.LogWarning($"[{context}] Velocity Mismatch! Difference: {velDiff:F4}. Predicted: {predicted.Velocity}, Verified: {verified.Velocity}");
        }

        // --- Stance Check ---
        if (predicted.Stance != verified.Stance)
        {
            Debug.LogWarning($"[{context}] Stance Mismatch! Predicted: {predicted.Stance}, Verified: {verified.Stance}");
        }

        // --- Grounded Check ---
        if (predicted.Grounded != verified.Grounded)
        {
            Debug.LogWarning($"[{context}] Grounded Mismatch! Predicted: {predicted.Grounded}, Verified: {verified.Grounded}");
        }

        // --- Jumps Used Check ---
        if (predicted.NumJumpsUsed != verified.NumJumpsUsed)
        {
            Debug.LogWarning($"[{context}] NumJumpsUsed Mismatch! Predicted: {predicted.NumJumpsUsed}, Verified: {verified.NumJumpsUsed}");
        }
    }

    #endregion

    #region Public Getters/Setters

    public Transform GetCameraTarget() => cameraTarget;
    public PlayerState GetCurrentState() => _lastSimulatedState;
    public bool CanWallJump => _lastSimulatedState.TimeSinceLeftWall < wallCoyoteTime;
    public bool CanDoubleJump => _lastSimulatedState.NumJumpsUsed < maxJumps;
    public int NumJumpsUsed => _lastSimulatedState.NumJumpsUsed;
    public bool IsWallOnLeft => _lastSimulatedState.IsWallOnLeft;
    public bool IsWallOnRight => _lastSimulatedState.IsWallOnRight;
    public int MaxJumps => maxJumps;
    public bool IsWallJumpEnabled { get => isWallJumpEnabled; set => isWallJumpEnabled = value; }
    public bool IsDoubleJumpEnabled { get => isDoubleJumpEnabled; set => isDoubleJumpEnabled = value; }
    public bool IsWallRunEnabled { get => isWallRunEnabled; set => isWallRunEnabled = value; }
    public PurrnetCrouchMode GetCrouchMode() => _crouchMode;

    public Stance GetPlayerStance() => GetCurrentState().Stance;
    
    public Vector3 GetWallNormal()
    {
        var currentState = _lastSimulatedState;
        if (currentState.Stance == Stance.WallRun) return currentState.WallRunNormal;
        return CanWallJump ? currentState.WallJumpNormal : Vector3.zero;
    }

    #endregion
}