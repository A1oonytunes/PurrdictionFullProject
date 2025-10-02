using UnityEngine;
using UnityEngine.InputSystem;

// This component is now required to be on the same GameObject.
[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [Space]
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;

    // --- Private references to components and actions ---
    private PlayerInput _playerInput;
    private InputAction _mLookAction;
    private InputAction _mMoveAction;
    private InputAction _mJumpAction;
    private InputAction _mCrouchAction;

    private void Awake()
    {
        // Get the PlayerInput component attached to this GameObject.
        _playerInput = GetComponent<PlayerInput>();

        // Get action references from the PlayerInput component's action asset.
        // This is robust because the PlayerInput component guarantees they exist.
        _mLookAction = _playerInput.actions["Look"];
        _mMoveAction = _playerInput.actions["Move"];
        _mJumpAction = _playerInput.actions["Jump"];
        _mCrouchAction = _playerInput.actions["Crouch"];

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.GetCameraTarget());
        cameraSpring.Initialize();
        cameraLean.Initialize();
    }

    // NOTE: OnEnable and OnDisable are no longer needed here to manage the action map.
    // The PlayerInput component and the PauseMenuController now handle the active state.

    private void Update()
    {
        // This code only runs effectively when the "Player" action map is active.
        // When the Pause Menu switches to the "UI" map, ReadValue will return default/zero values.
        var cameraInput = new CameraInput { Look = _mLookAction.ReadValue<Vector2>() };
        playerCamera.UpdateRotation(cameraInput);

        var deltaTime = Time.deltaTime;

        CrouchInput crouchInput = CrouchInput.None;
        if (playerCharacter.GetCrouchMode() is CrouchMode.Hold)
        {
            if (_mCrouchAction.WasPressedThisFrame()) crouchInput = CrouchInput.Crouch;
            else if (_mCrouchAction.WasReleasedThisFrame()) crouchInput = CrouchInput.Uncrouch;
        }
        else
        {
            if (_mCrouchAction.WasPressedThisFrame()) crouchInput = CrouchInput.Toggle;
        }

        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = _mMoveAction.ReadValue<Vector2>(),
            Jump = _mJumpAction.WasPressedThisFrame(),
            JumpSustain = _mJumpAction.IsPressed(),
            Crouch = crouchInput
        };
        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);

#if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out var hit))
            {
                Teleport(hit.point);
            }
        }
#endif
    }

    void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var cameraTarget = playerCharacter.GetCameraTarget();
        var state = playerCharacter.GetState();

        playerCamera.UpdatePosition(cameraTarget);
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        if (state.Stance is not Stance.WallRun)
        {
            if (playerCharacter.GetLastState().Stance is Stance.WallRun)
            {
                cameraLean.ResetWallRunLean();
            }
            cameraLean.UpdateLean(deltaTime, state.Stance is Stance.Slide, state.Acceleration, cameraTarget.up);
        }
        else
        {
            cameraLean.WallRunLean(playerCharacter.IsWallOnLeft ? -cameraLean.WallRunTilt : cameraLean.WallRunTilt);
        }
    }

    public void Teleport(Vector3 position)
    {
        playerCharacter.SetPosition(position);
    }
}