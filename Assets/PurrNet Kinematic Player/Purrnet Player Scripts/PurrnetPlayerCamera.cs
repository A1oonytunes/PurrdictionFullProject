using System;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Simple camera helper with direct camera effects
/// </summary>
public class PurrnetCameraHelper : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    
    [Header("Target")]
    [SerializeField] private Vector3 followOffset = new Vector3(0, 1.6f, -5f);
    [SerializeField] private float followSmoothTime = 0.1f;
    
    [Header("Wall Run Lean")]
    [SerializeField] private bool enableWallRunLean = true;
    [SerializeField] private float wallRunLeanAngle = 15f;
    [SerializeField] private float wallRunLeanSpeed = 8f;
    
    [Header("Camera Shake")]
    [SerializeField] private bool enableCameraShake = false;
    [SerializeField] private float shakeDecay = 5f;
    
    [Header("Camera Bob")]
    [SerializeField] private bool enableCameraBob = false;
    [SerializeField] private float bobAmount = 0.02f;
    [SerializeField] private float bobSpeed = 10f;
    
    [Header("FOV")]
    [SerializeField] private bool enableFOVChange = false;
    [SerializeField] private float defaultFOV = 80f;
    [SerializeField] private float fovChangeSpeed = 5f;
    
    private PurrnetPlayerMovement _characterController;
    private Transform _followTarget;
    
    private float _verticalRotation;
    private float _horizontalRotation;
    private Vector3 _followVelocity;
    private bool _initialized;
    
    // Effect state
    private float _currentLeanAngle = 0f;
    private float _targetLeanAngle = 0f;
    private float _shakeTrauma = 0f;
    private float _shakeTimeOffset;
    private float _bobTimer = 0f;
    private float _targetFOV;

    // Input cache
    private float _mouseX;
    private float _mouseY;
    
    public Quaternion GetLookRotation() => Quaternion.Euler(0, _horizontalRotation, 0);
    public Vector3 GetLookForward() => GetLookRotation() * Vector3.forward;
    public Vector3 GetLookRight() => GetLookRotation() * Vector3.right;

    public void Awake()
    {
        cinemachineCamera.Priority.Value = -1;
        _shakeTimeOffset = UnityEngine.Random.Range(0f, 1000f);
        _targetFOV = defaultFOV;
    }

    public void Init(PurrnetPlayerMovement playerMovement, Transform cameraTarget)
    {
        _initialized = true;
        _characterController = playerMovement;
        _followTarget = cameraTarget;
        cinemachineCamera.Priority.Value = 10;
        
        if (enableFOVChange && cinemachineCamera != null)
        {
            cinemachineCamera.Lens.FieldOfView = defaultFOV;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        if (!_initialized) return;
        
        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform ?? transform;
            
        Vector3 currentEuler = transform.eulerAngles;
        _horizontalRotation = currentEuler.y;
        _verticalRotation = currentEuler.x;
        
        if (_verticalRotation > 180f)
            _verticalRotation -= 360f;
    }
    
    private void Update()
    {
        if (!_initialized) return;

        // Collect input only
        _mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        _mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Update effect logic (but not rotation)
        UpdateWallRunLean();
        UpdateShake();
        UpdateBob();
        UpdateFOV();
    }
    
    private void LateUpdate()
    {
        if (!_initialized || _followTarget == null || cameraTransform == null) return;

        // === Apply rotation using cached input ===
        _horizontalRotation += _mouseX;
        _verticalRotation -= _mouseY;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -maxLookAngle, maxLookAngle);
        transform.rotation = Quaternion.Euler(_verticalRotation, _horizontalRotation, 0);

        // Clear input after applying
        _mouseX = 0f;
        _mouseY = 0f;

        // === Position and rotation handling ===
        Vector3 targetPosition = _followTarget.position;
        Vector3 basePosition = targetPosition + transform.rotation * followOffset;
        Quaternion baseRotation = transform.rotation;

        // Smooth follow
        Vector3 finalPosition = Vector3.SmoothDamp(
            cameraTransform.position, 
            basePosition, 
            ref _followVelocity, 
            followSmoothTime
        );
        Quaternion finalRotation = baseRotation;
        
        // Apply lean
        if (enableWallRunLean && Mathf.Abs(_currentLeanAngle) > 0.01f)
        {
            finalRotation *= Quaternion.Euler(0f, 0f, _currentLeanAngle);
        }
        
        // Apply shake
        if (enableCameraShake && _shakeTrauma > 0.01f)
        {
            float shake = _shakeTrauma * _shakeTrauma;
            float time = Time.time + _shakeTimeOffset;
            
            float offsetX = (Mathf.PerlinNoise(time * 25f, 0f) - 0.5f) * 2f * shake;
            float offsetY = (Mathf.PerlinNoise(0f, time * 25f) - 0.5f) * 2f * shake;
            float offsetZ = (Mathf.PerlinNoise(time * 25f, time * 25f) - 0.5f) * 2f * shake;
            
            finalPosition += finalRotation * new Vector3(offsetX, offsetY, offsetZ) * 0.1f;
            finalRotation *= Quaternion.Euler(offsetX * 2f, offsetY * 2f, offsetZ * 2f);
        }
        
        // Apply bob
        if (enableCameraBob)
        {
            float verticalBob = Mathf.Sin(_bobTimer) * bobAmount;
            finalPosition += finalRotation * new Vector3(0f, verticalBob, 0f);
        }
        
        cameraTransform.position = finalPosition;
        cameraTransform.rotation = finalRotation;
    }
    
    private void UpdateWallRunLean()
    {
        if (!enableWallRunLean || _characterController == null) return;
        
        bool isWallRunning = _characterController.GetPlayerStance() is PurrnetPlayerMovement.Stance.WallRun;
        
        if (isWallRunning)
        {
            if (_characterController.IsWallOnRight)
                _targetLeanAngle = wallRunLeanAngle;
            else if (_characterController.IsWallOnLeft)
                _targetLeanAngle = -wallRunLeanAngle;
        }
        else
        {
            _targetLeanAngle = 0f;
        }
        
        _currentLeanAngle = Mathf.Lerp(_currentLeanAngle, _targetLeanAngle, Time.deltaTime * wallRunLeanSpeed);
    }
    
    private void UpdateShake()
    {
        if (!enableCameraShake) return;
        _shakeTrauma = Mathf.Max(0f, _shakeTrauma - shakeDecay * Time.deltaTime);
    }
    
    private void UpdateBob()
    {
        if (!enableCameraBob || _characterController == null) return;
        
        float speed = _characterController.GetCurrentState().Velocity.magnitude;
        
        if (speed > 1f)
        {
            _bobTimer += Time.deltaTime * bobSpeed;
        }
        else
        {
            _bobTimer = 0f;
        }
    }
    
    private void UpdateFOV()
    {
        if (!enableFOVChange || cinemachineCamera == null) return;
        
        float currentFOV = cinemachineCamera.Lens.FieldOfView;
        cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(currentFOV, _targetFOV, Time.deltaTime * fovChangeSpeed);
    }
    
    // Public API
    public void TriggerShake(float intensity)
    {
        if (enableCameraShake)
            _shakeTrauma = Mathf.Clamp01(_shakeTrauma + intensity);
    }
    
    public void SetFOV(float fov)
    {
        if (enableFOVChange)
            _targetFOV = fov;
    }
    
    public void ResetFOV()
    {
        SetFOV(defaultFOV);
    }
}
