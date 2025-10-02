using UnityEngine;
using DG.Tweening;

public class CameraLean : MonoBehaviour
{
    [SerializeField] private float attackDamping = 0.5f;
    [SerializeField] private float decayDamping = 0.3f;
    [SerializeField] private float walkStrength = 0.075f;
    [SerializeField] private float slideStrength = 0.2f;
    [SerializeField] private float strengthResponse = 5f;

    [Header("WallRun Effects")] 
    [SerializeField] private float wallRunTilt = 5f;
    
    
    
    private Vector3 _dampedAcceleration;
    private Vector3 _dampedAccelerationVel;

    private float _smoothStrength;
    private float _wallRunExitDelay = 0f;
    
    public void Initialize()
    {
        _smoothStrength = walkStrength;
    }
    

    public void UpdateLean(float deltaTime, bool sliding, Vector3 acceleration, Vector3 up)
    {
        if (_wallRunExitDelay > 0f)
        {
            _wallRunExitDelay -= Time.deltaTime;
            return;
        }
        var planarAcceleration = Vector3.ProjectOnPlane(acceleration, up);
        var damping = planarAcceleration.magnitude > _dampedAcceleration.magnitude
            ? attackDamping
            : decayDamping;

        _dampedAcceleration = Vector3.SmoothDamp
        (
            current: _dampedAcceleration,
            target: planarAcceleration,
            currentVelocity: ref _dampedAccelerationVel,
            smoothTime: damping,
            maxSpeed: float.PositiveInfinity,
            deltaTime: deltaTime
        );
        
        //Get the rotation axis based on the acceleration vector
        var leanAxis = Vector3.Cross(_dampedAcceleration.normalized, up).normalized;
        
        //Reset the rotation to that of its parent
        transform.localRotation = Quaternion.identity;
        
        //Rotate around the lean axis
        var targetStrength = sliding 
            ? slideStrength 
            :walkStrength;

        _smoothStrength = Mathf.Lerp(_smoothStrength, targetStrength, 1f - Mathf.Exp(-strengthResponse * deltaTime));
        
        transform.rotation = Quaternion.AngleAxis(_dampedAcceleration.magnitude * _smoothStrength, leanAxis) * transform.rotation;
    }

    public void WallRunLean(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0f, 0f, zTilt), 0.25f);
    }

    public void ResetWallRunLean()
    {
        _wallRunExitDelay = 0.3f;
        transform.DOLocalRotate(Vector3.zero, 0.25f);
    }

    public float WallRunTilt => wallRunTilt;
}
