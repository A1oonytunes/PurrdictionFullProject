using System;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Simple camera helper to provide look rotation for the character controller.
/// Attach this to your camera or a separate GameObject.
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
     private PurrnetPlayerMovement _characterController;
     private Transform _followTarget;
    
    private float _verticalRotation;
    private float _horizontalRotation;
    private Vector3 _followVelocity;
    private bool _initialized;
    
    /// <summary>
    /// Returns the horizontal look rotation (yaw only, no pitch)
    /// This is what the character should face
    /// </summary>
    public Quaternion GetLookRotation()
    {
        return Quaternion.Euler(0, _horizontalRotation, 0);
    }
    

    /// <summary>
    /// Returns the forward direction the character should move towards
    /// </summary>
    public Vector3 GetLookForward()
    {
        return Quaternion.Euler(0, _horizontalRotation, 0) * Vector3.forward;
    }

    /// <summary>
    /// Returns the right direction relative to camera look
    /// </summary>
    public Vector3 GetLookRight()
    {
        return Quaternion.Euler(0, _horizontalRotation, 0) * Vector3.right;
    }

    public void Awake()
    {
        cinemachineCamera.Priority.Value = -1;
    }

    public void Init(PurrnetPlayerMovement playerMovement, Transform cameraTarget)
    {
        _initialized = true;
        _characterController = playerMovement;
        _followTarget = cameraTarget;
        cinemachineCamera.Priority.Value = 10;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        if (!_initialized)
        {
            return;
        };
        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform ?? transform;
            
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize rotation to match current transform
        Vector3 currentEuler = transform.eulerAngles;
        _horizontalRotation = currentEuler.y;
        _verticalRotation = currentEuler.x;
        
        // Normalize vertical rotation to -180 to 180 range
        if (_verticalRotation > 180f)
            _verticalRotation -= 360f;
    }
    
    private void Update()
    {
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        _horizontalRotation += mouseX;
        _verticalRotation -= mouseY;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -maxLookAngle, maxLookAngle);
        
        // Apply rotation to camera pivot
        transform.rotation = Quaternion.Euler(_verticalRotation, _horizontalRotation, 0);
    }
    
    private void LateUpdate()
    {
        if (!_initialized) return;
        if (_followTarget != null && cameraTransform != null)
        {
            // Follow the target smoothly
            Vector3 targetPosition = _followTarget.position;
            Vector3 desiredPosition = targetPosition + transform.rotation * followOffset;
            
            cameraTransform.position = Vector3.SmoothDamp(
                cameraTransform.position, 
                desiredPosition, 
                ref _followVelocity, 
                followSmoothTime
            );
            
            cameraTransform.rotation = transform.rotation;
        }
    }
}