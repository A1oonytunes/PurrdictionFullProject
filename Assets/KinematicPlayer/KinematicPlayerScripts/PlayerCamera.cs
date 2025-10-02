using System;
using Unity.Cinemachine;
using UnityEngine;

public struct CameraInput
{
    public Vector2 Look;
}

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.1f;
    
    private Vector3 _eulerAngles;
    
    
    

    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.eulerAngles = _eulerAngles = target.eulerAngles;
    }

    public void UpdateRotation(CameraInput input)
    {
        // Apply input to accumulated rotation
        _eulerAngles += new Vector3(-input.Look.y, input.Look.x) * sensitivity;
        
        // Clamp the X rotation (pitch) to prevent camera flipping
        _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, -89.9f, 89.9f);
        
        // Apply the clamped rotation to the transform
        transform.eulerAngles = _eulerAngles;
    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }
}