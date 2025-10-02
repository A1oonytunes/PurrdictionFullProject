using System;
using System.Collections.Generic;
using KinematicCharacterController;
using PurrNet.Prediction;
using UnityEngine;
using UnityEngine.InputSystem;

    public enum PurrnetCrouchInput
    {
        None,
        Toggle,
        Crouch,
        Uncrouch
    }

    public enum PurrnetCrouchMode
    {
        Hold,
        Toggle
    }

    public enum PurrnetStance
    {
        Stand,
        Crouch,
        Slide,
        WallRun
    }
public class PurrnetKCCController :
    PredictedIdentity<PurrnetKCCController.PlayerInput, PurrnetKCCController.PlayerState>,
    ICharacterController
{
    [Header("Components")] [SerializeField]
    private KinematicCharacterMotor motor;

    [SerializeField] private Transform root;

    [SerializeField] private Transform cameraTarget;

    [Header("Movement - Ground")] [SerializeField]
    private float walkSpeed = 20f;

    [SerializeField] private float crouchSpeed = 7f;
    [SerializeField] private float walkResponse = 25f;
    [SerializeField] private float crouchResponse = 20f;

    [Header("Movement - Air")] [SerializeField]
    private float airSpeed = 15f;

    [SerializeField] private float airAcceleration = 70f;

    [SerializeField] private float gravity = -90f;

    [Header("Jumping")] [SerializeField] private float jumpSpeed = 20f;

    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private bool isDoubleJumpEnabled = false;
    [SerializeField] private float jumpCooldown = 0.2f;

    [SerializeField] private int maxJumps = 2;
    [Range(0f, 1f)] [SerializeField] private float jumpSustainGravity = 0.4f;
    [SerializeField] private bool isSustainedJumpEnabled = false;

    [Header("Sliding")] [SerializeField] private float slideStartSpeed = 25f;
    [SerializeField] private float slideEndSpeed = 15f;
    [SerializeField] private float slideFriction = 0.8f;
    [SerializeField] private float slideSteerAcceleration = 5f;
    [SerializeField] private float slideGravity = -90f;

    [Header("Crouching")] [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchHeightResponse = 15f;
    [SerializeField] private bool holdToCrouch = false;
    [Range(0f, 1f)] [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0f, 1f)] [SerializeField] private float crouchCameraTargetHeight = 0.7f;
    [SerializeField] private float wallRunCheckDistance = 0.7f;
    [SerializeField] private float wallJumpCastDistance = 0.7f;
    [SerializeField] private float wallRunGravity = -30f;
    [SerializeField] private float wallRunMaxTime = 3f;

    [Header("Wall Mechanics")] [SerializeField]
    private bool isWallJumpEnabled = false;

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

// Private state variables
    private CharacterState _state;
    private CharacterState _lastState;
    private CharacterState _tempState;
    private PurrnetCrouchMode _crouchMode;

// Private input variables
    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedSustainedJump;
    private bool _requestedCrouch;
    private bool _requestedCrouchInAir;

// Timers and counters
    private float _timeSinceUngrounded;
    private float _timeSinceJumpRequest;
    private bool _ungroundedDueToJump;
    private int _numJumpsUsed;

// Wall-related state
    private Vector3 _wallJumpNormal;
    private float _jumpCooldownTimer;
    private float _wallJumpInputLockoutTimer;
    private float _timeSinceLeftWall;

    private Collider[] _uncrouchOverlapResults;
    private Vector3 _wallRunNormal;
    private float _wallRunTime;
    private bool _isWallOnLeft, _isWallOnRight;
    private Vector3 _leftWallNormal, _rightWallNormal;

    private KCCSettings _kccSettings;
    
    private UnityEngine.InputSystem.PlayerInput _playerInput;
    private List<KinematicCharacterMotor> _motorList = new List<KinematicCharacterMotor>();
    private List<PhysicsMover> _physicsList = new List<PhysicsMover>();

    private bool _initialized;


    public struct PlayerInput : IPredictedData
    {
        public Quaternion Look;
        public Vector3 Move;
        public bool Jump;
        public bool JumpSustain;
        public PurrnetCrouchInput Crouch;

        public void Dispose()
        {

        }
    } 

    public struct PlayerState : IPredictedData<PlayerState>
    {
        // === CRITICAL: Transform Data ===
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 Acceleration;
    
        // === Movement State ===
        public bool Grounded;
        public Stance Stance;  // Stand, Crouch, Slide, WallRun
    
        // === Jump State ===
        public float TimeSinceUngrounded;
        public float TimeSinceJumpRequest;
        public bool UngroundedDueToJump;
        public int NumJumpsUsed;
    
        // === Wall Mechanics State ===
        public float TimeSinceLeftWall;
        public Vector3 WallJumpNormal;
        public Vector3 WallRunNormal;
        public float WallRunTime;
        public bool IsWallOnLeft;
        public bool IsWallOnRight;
        public Vector3 LeftWallNormal;
        public Vector3 RightWallNormal;
    
        // === Cooldown Timers ===
        public float JumpCooldownTimer;
        public float WallJumpInputLockoutTimer;
    
        public void Dispose() { }
    }

    #region Initialization and Input

    public void Initialize()
    {
        _initialized = true;
        _crouchMode = holdToCrouch ? PurrnetCrouchMode.Hold : PurrnetCrouchMode.Toggle;
        _state.Stance = Stance.Stand;
        _lastState = _state;

        _uncrouchOverlapResults = new Collider[8];
        motor.CharacterController = this;
        _timeSinceLeftWall = wallCoyoteTime;

        _kccSettings = ScriptableObject.CreateInstance<KCCSettings>();
        _kccSettings.AutoSimulation = false;
        _kccSettings.Interpolate = false;
        
        _motorList.Add(motor);
        KinematicCharacterSystem.Settings = _kccSettings;
    }
    

    protected override void GetFinalInput(ref PlayerInput playerInput)
    {
        // Hand back the cached "last frame’s raw input"
        // This is what CSP consumes
        playerInput.Look = _requestedRotation;
        playerInput.Move = _requestedMovement;
        playerInput.Jump = _requestedJump;
        playerInput.JumpSustain = _requestedSustainedJump;

        // Convert stance back into an enum for CSP
        playerInput.Crouch = _requestedCrouch
            ? PurrnetCrouchInput.Crouch
            : PurrnetCrouchInput.Uncrouch;

        // Clear one-frame booleans so they don’t repeat
        _requestedJump = false;
    }


    public void UpdateInput(PlayerInput playerInput)
    {
        _requestedRotation = playerInput.Look;
        _requestedMovement = Vector3.ClampMagnitude(
            playerInput.Look * new Vector3(playerInput.Move.x, 0f, playerInput.Move.y), 1f);

        // Jump handling
        var wasRequestingJump = _requestedJump;
        _requestedJump = _requestedJump || playerInput.Jump;
        if (_requestedJump && !wasRequestingJump)
            _timeSinceJumpRequest = 0f;

        _requestedSustainedJump = isSustainedJumpEnabled && playerInput.JumpSustain;

        // Crouch handling
        var wasRequestingCrouch = _requestedCrouch;
        _requestedCrouch = playerInput.Crouch switch
        {
            PurrnetCrouchInput.Toggle   => !_requestedCrouch,
            PurrnetCrouchInput.Crouch   => true,
            PurrnetCrouchInput.Uncrouch => false,
            _                           => _requestedCrouch
        };

        if (_requestedCrouch && !wasRequestingCrouch)
            _requestedCrouchInAir = !_state.Grounded;
        else if (!_requestedCrouch && wasRequestingCrouch)
            _requestedCrouchInAir = false;
    }


    protected override void Simulate(PlayerInput playerInput, ref PlayerState playerState, float delta)
    {
        if (!_initialized) return;
    
        // 1. Load state into working variables
        SetUnityState(playerState);
    
        // 3. Run KCC simulation
        KinematicCharacterSystem.Simulate(delta, _motorList, _physicsList);
    
        // 4. Capture resulting state
        GetUnityState(ref playerState);
    }

    protected override void GetUnityState(ref PlayerState playerState)
    {
        // Read FROM Unity/KCC components INTO the state
        playerState.Position = motor.TransientPosition;
        playerState.Rotation = motor.TransientRotation;
        playerState.Velocity = motor.BaseVelocity;
        playerState.Grounded = motor.GroundingStatus.IsStableOnGround;
        playerState.Stance = _state.Stance;
        playerState.Acceleration = _state.Acceleration;
    
        // Any other runtime state that affects simulation
        playerState.TimeSinceUngrounded = _timeSinceUngrounded;
        playerState.TimeSinceLeftWall = _timeSinceLeftWall;
        playerState.NumJumpsUsed = _numJumpsUsed;
        playerState.WallRunTime = _wallRunTime;
        playerState.UngroundedDueToJump = _ungroundedDueToJump;
    }

    protected override void SetUnityState(PlayerState playerState)
    {
        // Write FROM state TO Unity/KCC components
        motor.SetPositionAndRotation(playerState.Position, playerState.Rotation);
        motor.BaseVelocity = playerState.Velocity;
    
        // Restore internal state
        _state.Grounded = playerState.Grounded;
        _state.Stance = playerState.Stance;
        _state.Velocity = playerState.Velocity;
        _state.Acceleration = playerState.Acceleration;
    
        _timeSinceUngrounded = playerState.TimeSinceUngrounded;
        _timeSinceLeftWall = playerState.TimeSinceLeftWall;
        _numJumpsUsed = playerState.NumJumpsUsed;
        _wallRunTime = playerState.WallRunTime;
        _ungroundedDueToJump = playerState.UngroundedDueToJump;
    }
    

    #endregion
    

    #region Main Update Loop (ICharacterController)

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        var forward = Vector3.ProjectOnPlane(_requestedRotation * Vector3.forward, motor.CharacterUp);
        if (forward != Vector3.zero)
            currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        _state.Acceleration = Vector3.zero;
        _timeSinceLeftWall += deltaTime;
        if (_jumpCooldownTimer > 0f)
        {
            _jumpCooldownTimer -= deltaTime;
        }

        if (_wallJumpInputLockoutTimer > 0f)
        {
            _wallJumpInputLockoutTimer -= deltaTime;
        }

        if (motor.GroundingStatus.IsStableOnGround)
        {
            HandleGroundedMovement(ref currentVelocity, deltaTime);
        }
        else
        {
            HandleAirborneMovement(ref currentVelocity, deltaTime);
        }

        HandleJumping(ref currentVelocity, deltaTime);
    }

    #endregion

    #region State & Collision Handling

    public void BeforeCharacterUpdate(float deltaTime)
    {
        _tempState = _state;

        if (_requestedCrouch && _state.Stance == Stance.Stand)
        {
            _state.Stance = Stance.Crouch;
            motor.SetCapsuleDimensions(motor.Capsule.radius, crouchHeight, crouchHeight * 0.5f);
        }
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        if (!_requestedCrouch && _state.Stance != Stance.Stand)
        {
            motor.SetCapsuleDimensions(motor.Capsule.radius, standHeight, standHeight * 0.5f);
            var pos = motor.TransientPosition;
            var rot = motor.TransientRotation;
            var mask = motor.CollidableLayers;
            if (motor.CharacterOverlap(pos, rot, _uncrouchOverlapResults, mask, QueryTriggerInteraction.Ignore) > 0)
            {
                motor.SetCapsuleDimensions(motor.Capsule.radius, crouchHeight, crouchHeight * 0.5f);
                _requestedCrouch = true;
            }
            else
            {
                if (_state.Stance != Stance.WallRun)
                    _state.Stance = Stance.Stand;
            }
        }

        _state.Grounded = motor.GroundingStatus.IsStableOnGround;
        _state.Velocity = motor.Velocity;
        var totalAcceleration = (_state.Velocity - _lastState.Velocity) / deltaTime;
        _state.Acceleration = Vector3.ClampMagnitude(_state.Acceleration, totalAcceleration.magnitude);
        _lastState = _tempState;
    }

    #endregion

    #region Movement Handlers

    private void HandleGroundedMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        _timeSinceUngrounded = 0f;
        _ungroundedDueToJump = false;
        _numJumpsUsed = 0;
        _wallRunTime = 0f;

        if (_state.Stance == Stance.WallRun)
        {
            _state.Stance = Stance.Stand;
        }

        var groundedMovement = motor.GetDirectionTangentToSurface(
            _requestedMovement, motor.GroundingStatus.GroundNormal
        ) * _requestedMovement.magnitude;

        bool isTryingToSlide = _state.Stance == Stance.Crouch &&
                               (_lastState.Stance == Stance.Stand || !_lastState.Grounded);
        if (isTryingToSlide && groundedMovement.sqrMagnitude > 0f)
        {
            HandleSlideMovement(ref currentVelocity, deltaTime, true);
        }
        else if (_state.Stance == Stance.Slide)
        {
            HandleSlideMovement(ref currentVelocity, deltaTime, false);
        }
        else
        {
            HandleWalkCrouchMovement(ref currentVelocity, deltaTime, groundedMovement);
        }
    }

    private void HandleAirborneMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        _timeSinceUngrounded += deltaTime;

        CheckForDirectionalWallJump();
        CheckForWallRun();

        float horizontalInputDot = Vector3.Dot(_requestedMovement, transform.right);
        bool wantsToRunLeft = horizontalInputDot < -0.1f;
        bool wantsToRunRight = horizontalInputDot > 0.1f;

        // --- FIX --- : Check for forward input before allowing a wall run to start.
        // A dot product greater than 0 means the player is holding a "forward" key relative to the camera.
        bool isMovingForward = Vector3.Dot(_requestedMovement, transform.forward) > 0.1f;

        bool canWallRun = false;
        if (isWallRunEnabled && currentVelocity.y < 5f && isMovingForward) // Added the isMovingForward check here
        {
            if (_isWallOnLeft && wantsToRunLeft)
            {
                _wallRunNormal = _leftWallNormal;
                canWallRun = true;
            }
            else if (_isWallOnRight && wantsToRunRight)
            {
                _wallRunNormal = _rightWallNormal;
                canWallRun = true;
            }
        }

        if (canWallRun)
        {
            HandleWallRun(ref currentVelocity, deltaTime);
        }
        else
        {
            if (_state.Stance == Stance.WallRun)
            {
                _state.Stance = Stance.Stand;
            }

            HandleStandardAirMovement(ref currentVelocity, deltaTime);
            ApplyGravity(ref currentVelocity, deltaTime);
        }
    }

    private void HandleJumping(ref Vector3 currentVelocity, float deltaTime)
    {
        if (!_requestedJump || _jumpCooldownTimer > 0f)
        {
            return;
        }

        var grounded = motor.GroundingStatus.IsStableOnGround;
        var canCoyoteJump = _timeSinceUngrounded < coyoteTime && !_ungroundedDueToJump;
        var canExtraJump = isDoubleJumpEnabled && (_numJumpsUsed < maxJumps);
        var canWallJump = isWallJumpEnabled && _timeSinceLeftWall < wallCoyoteTime;

        bool jumpedThisFrame = false;

        // Wall Run Jump
        if (_state.Stance == Stance.WallRun)
        {
            _state.Stance = Stance.Stand;

            Vector3 horizontalDir = Vector3.ProjectOnPlane(_wallRunNormal, motor.CharacterUp).normalized;

            currentVelocity = Vector3.ProjectOnPlane(currentVelocity, _wallRunNormal);
            currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);

            currentVelocity += horizontalDir * wallJumpHorizontalSpeed;
            currentVelocity += motor.CharacterUp * wallJumpVerticalSpeed;

            _wallJumpInputLockoutTimer = wallJumpInputLockoutDuration;

            _numJumpsUsed++;
            jumpedThisFrame = true;
        }
        // Standard Wall Jump
        else if (canWallJump)
        {
            _state.Stance = Stance.Stand;

            Vector3 horizontalDir = Vector3.ProjectOnPlane(_wallJumpNormal, motor.CharacterUp).normalized;

            currentVelocity = Vector3.ProjectOnPlane(currentVelocity, _wallJumpNormal);
            currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);

            currentVelocity += horizontalDir * wallJumpHorizontalSpeed;
            currentVelocity += motor.CharacterUp * wallJumpVerticalSpeed;

            _wallJumpInputLockoutTimer = wallJumpInputLockoutDuration;

            _numJumpsUsed++;
            jumpedThisFrame = true;
        }
        // Grounded, Coyote, or Double Jump
        else if (grounded || canCoyoteJump || canExtraJump)
        {
            Vector3 jumpDirection = motor.CharacterUp;
            if (motor.GroundingStatus.FoundAnyGround && !motor.GroundingStatus.IsStableOnGround)
            {
                jumpDirection = motor.GroundingStatus.GroundNormal;
            }

            currentVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            currentVelocity += jumpDirection * jumpSpeed;

            if (!(grounded || canCoyoteJump))
            {
                _numJumpsUsed++;
            }
            else
            {
                _numJumpsUsed = 1;
            }

            jumpedThisFrame = true;
        }

        if (jumpedThisFrame)
        {
            _jumpCooldownTimer = jumpCooldown;
            motor.ForceUnground(0.1f);
            _ungroundedDueToJump = true;
            _requestedJump = false;
        }
        else
        {
            _timeSinceJumpRequest += deltaTime;
            if (_timeSinceJumpRequest >= coyoteTime)
            {
                _requestedJump = false;
            }
        }
    }

    #endregion

    #region Specific Movement Logic

    private void HandleWalkCrouchMovement(ref Vector3 currentVelocity, float deltaTime, Vector3 groundedMovement)
    {
        var speed = _state.Stance == Stance.Stand ? walkSpeed : crouchSpeed;
        var response = _state.Stance == Stance.Stand ? walkResponse : crouchResponse;
        var targetVelocity = groundedMovement * speed;
        var moveVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1f - Mathf.Exp(-response * deltaTime));
        _state.Acceleration = (moveVelocity - currentVelocity) / deltaTime;
        currentVelocity = moveVelocity;
    }

    private void HandleWallRun(ref Vector3 currentVelocity, float deltaTime)
    {
        if (_state.Stance != Stance.WallRun)
        {
            _wallRunTime = 0f;
            _numJumpsUsed = 0;
            _state.Stance = Stance.WallRun;

            Vector3 horizontalVel = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);
            Vector3 wallForward = Vector3.Cross(_wallRunNormal, motor.CharacterUp);

            if (Vector3.Dot(horizontalVel, wallForward) < 0)
            {
                wallForward = -wallForward;
            }

            float horizontalSpeed = horizontalVel.magnitude;
            if (horizontalSpeed < wallRunStartSpeed)
            {
                horizontalSpeed = wallRunStartSpeed + wallRunInitialSpeedBoost;
            }

            float verticalSpeed = currentVelocity.y * wallRunVerticalMomentumPreservation;

            currentVelocity = (wallForward * horizontalSpeed) + (motor.CharacterUp * verticalSpeed);
        }

        currentVelocity += _wallRunNormal * -1 * wallStickForce * deltaTime;
        currentVelocity -= currentVelocity.normalized * (wallRunSpeedDecayRate * _wallRunTime * deltaTime);
        currentVelocity += motor.CharacterUp * (wallRunGravity * deltaTime);
        _wallRunTime += deltaTime;

        if (_wallRunTime > wallRunMaxTime)
        {
            _state.Stance = Stance.Stand;
        }
    }

    private void HandleStandardAirMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        if (_wallJumpInputLockoutTimer > 0f)
        {
            return;
        }

        if (_requestedMovement.sqrMagnitude <= 0f) return;
        var planarMovement = Vector3.ProjectOnPlane(_requestedMovement, motor.CharacterUp);
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

        if (motor.GroundingStatus.FoundAnyGround && Vector3.Dot(currentVelocity + movementForce, movementForce) > 0f)
        {
            if (!motor.GroundingStatus.IsStableOnGround)
            {
                movementForce = Vector3.ProjectOnPlane(movementForce, motor.GroundingStatus.GroundNormal);
            }
        }

        currentVelocity += movementForce;
    }

    private void ApplyGravity(ref Vector3 currentVelocity, float deltaTime)
    {
        var effectiveGravity = gravity;
        if (_requestedSustainedJump && Vector3.Dot(currentVelocity, motor.CharacterUp) > 0f)
        {
            effectiveGravity *= jumpSustainGravity;
        }

        currentVelocity += motor.CharacterUp * (effectiveGravity * deltaTime);
    }

    private void HandleSlideMovement(ref Vector3 currentVelocity, float deltaTime, bool isStartingSlide)
    {
        _state.Stance = Stance.Slide;
        if (isStartingSlide)
        {
            if (!_lastState.Grounded)
            {
                currentVelocity = Vector3.ProjectOnPlane(_lastState.Velocity, motor.GroundingStatus.GroundNormal);
            }

            var startSpeed = Mathf.Max(slideStartSpeed, currentVelocity.magnitude);
            currentVelocity = currentVelocity.normalized * startSpeed;
        }
        else
        {
            currentVelocity -= currentVelocity * (slideFriction * deltaTime);
            var slopeForce = Vector3.ProjectOnPlane(-motor.CharacterUp, motor.GroundingStatus.GroundNormal) *
                             slideGravity;
            currentVelocity -= slopeForce * deltaTime;
            var groundedMovement =
                motor.GetDirectionTangentToSurface(_requestedMovement, motor.GroundingStatus.GroundNormal) *
                _requestedMovement.magnitude;
            var currentSpeed = currentVelocity.magnitude;
            var targetVelocity = groundedMovement * currentSpeed;
            var steerForce = (targetVelocity - currentVelocity) * (slideSteerAcceleration * deltaTime);
            currentVelocity = Vector3.ClampMagnitude(currentVelocity + steerForce, currentSpeed);
            if (currentSpeed < slideEndSpeed)
            {
                _state.Stance = Stance.Crouch;
            }
        }
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        if (!motor.GroundingStatus.IsStableOnGround && _state.Stance == Stance.Slide)
        {
            _state.Stance = Stance.Crouch;
        }

        if (motor.GroundingStatus.IsStableOnGround && _state.Stance == Stance.WallRun)
        {
            _state.Stance = Stance.Stand;
        }
    }

    public bool IsColliderValidForCollisions(Collider coll) => true;

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
        if (!motor.GroundingStatus.IsStableOnGround && !hitStabilityReport.IsStable)
        {
            if (Mathf.Abs(Vector3.Dot(hitNormal, motor.CharacterUp)) < 0.7f)
            {
                _timeSinceLeftWall = 0f;
                _wallJumpNormal = hitNormal;
            }
        }
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
    }

    private void CheckForDirectionalWallJump()
    {
        if (_requestedMovement.sqrMagnitude <= 0f) return;

        Vector3 origin = motor.TransientPosition + motor.CharacterUp * (standHeight * 0.5f);
        float sphereRadius = motor.Capsule.radius * 0.9f;
        Vector3 direction = _requestedMovement.normalized;

        if (Physics.SphereCast(origin, sphereRadius, direction, out RaycastHit hit, wallJumpCastDistance,
                motor.CollidableLayers, QueryTriggerInteraction.Ignore))
        {
            if (Mathf.Abs(Vector3.Dot(hit.normal, motor.CharacterUp)) < 0.7f)
            {
                _timeSinceLeftWall = 0f;
                _wallJumpNormal = hit.normal;
            }
        }
    }

    #endregion

    #region Helper Methods & Callbacks

    private void CheckForWallRun()
    {
        Vector3 origin = motor.TransientPosition + motor.CharacterUp * (standHeight * 0.5f);
        float sphereRadius = motor.Capsule.radius * 0.9f;

        _isWallOnLeft = Physics.SphereCast(origin, sphereRadius, -transform.right, out RaycastHit leftHit,
            wallRunCheckDistance, motor.CollidableLayers, QueryTriggerInteraction.Ignore);
        if (_isWallOnLeft)
        {
            _leftWallNormal = leftHit.normal;
        }

        _isWallOnRight = Physics.SphereCast(origin, sphereRadius, transform.right, out RaycastHit rightHit,
            wallRunCheckDistance, motor.CollidableLayers, QueryTriggerInteraction.Ignore);
        if (_isWallOnRight)
        {
            _rightWallNormal = rightHit.normal;
        }
    }


    public void UpdateBody(float deltaTime)
    {
        var currentHeight = motor.Capsule.height;
        var normalizedHeight = currentHeight / standHeight;
        var cameraTargetHeight = currentHeight *
                                 (_state.Stance == Stance.Stand ? standCameraTargetHeight : crouchCameraTargetHeight);
        var lerpFactor = 1f - Mathf.Exp(-crouchHeightResponse * deltaTime);
        cameraTarget.localPosition =
            Vector3.Lerp(cameraTarget.localPosition, new Vector3(0f, cameraTargetHeight, 0f), lerpFactor);
        root.localScale = Vector3.Lerp(root.localScale, new Vector3(1f, normalizedHeight, 1f), lerpFactor);
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    #endregion

    #region Public Getters/Setters

    public Transform GetCameraTarget() => cameraTarget;

    public CharacterState GetState() => _state;

    public CharacterState GetLastState() => _lastState;

    public void SetPosition(Vector3 position, bool killVelocity = true)
    {
        motor.SetPosition(position);
        if (killVelocity)
            motor.BaseVelocity = Vector3.zero;
    }

    public PurrnetCrouchMode GetCrouchMode() => _crouchMode;
    public bool GetRequestingToCrouch() => _requestedCrouch;

    public bool IsWallJumpEnabled
    {
        get => isWallJumpEnabled;
        set => isWallJumpEnabled = value;
    }


    public bool IsDoubleJumpEnabled
    {
        get => isDoubleJumpEnabled;
        set => isDoubleJumpEnabled = value;
    }

    public bool IsWallRunEnabled
    {
        get => isWallRunEnabled;
        set => isWallRunEnabled = value;
    }

    public bool CanWallJump => _timeSinceLeftWall < wallCoyoteTime;

    public bool CanDoubleJump => _numJumpsUsed < maxJumps;

    public int NumJumpsUsed => _numJumpsUsed;
    public int MaxJumps => maxJumps;
    public bool IsWallOnLeft => _isWallOnLeft;
    public bool IsWallOnRight => _isWallOnRight;

    public Vector3 GetWallNormal()
    {
        if (_state.Stance == Stance.WallRun)
            return _wallRunNormal;
        return CanWallJump ? _wallJumpNormal : Vector3.zero;
    }

    public Vector3 GetRequestedMovement() => _requestedMovement;


    #endregion
}