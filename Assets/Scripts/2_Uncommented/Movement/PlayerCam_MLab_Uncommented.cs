using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCam_MLab_Uncommented : MonoBehaviour
{
    [Header("Sensitivity")]
    public float sensX = 10f;
    public float sensY = 10f;

    [Header("Assignables")]
    public Transform camT;
    public Camera cam;
    public Transform orientation;

    [Header("Effects")]
    public float baseFov = 90f;
    public float fovTransitionTime = 0.25f;
    public float tiltTransitionTime = 0.25f;

    [Header("Effects - HeadBob")]
    [HideInInspector] public bool hbEnabled;
    public float hbAmplitude = 0.5f;
    public float hbFrequency = 12f;

    private float hbToggleSpeed = 3f;
    private Vector3 hbStartPos;
    private Rigidbody rb;

    [HideInInspector] public float mouseX;
    [HideInInspector] public float mouseY;

    private float multiplier = 0.01f;

    private float xRotation;
    private float yRotation;

    private void Start()
    {
        hbStartPos = cam.transform.localPosition;

        camT = GameObject.Find("CameraHolder").transform;
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        RotateCamera();

        if (hbEnabled)
        {
            CheckMotion();
            cam.transform.LookAt(FocusTarget());
        }
    }

    public void RotateCamera()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY;

        // calculate rotation
        yRotation += mouseX * sensX * multiplier;
        xRotation -= mouseY * sensY * multiplier;

        xRotation = Mathf.Clamp(xRotation, -89f, 89f);

        // rotate cam and player
        camT.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }


    /// double click the field below to show all fov, tilt and cam shake code
    #region Fov, Tilt and CamShake

    public void DoFov(float endValue, float transitionTime = -1)
    {
        if(transitionTime == -1)
            cam.DOFieldOfView(endValue, fovTransitionTime);

        else
            cam.DOFieldOfView(endValue, transitionTime);
    }

    public void ResetFov()
    {
        cam.DOFieldOfView(baseFov, fovTransitionTime);
    }

    public void DoTilt(float zTilt)
    {
        cam.transform.DOLocalRotate(new Vector3(0, 0, zTilt), tiltTransitionTime);
    }

    public void ResetTilt()
    {
        cam.transform.DOLocalRotate(Vector3.zero, tiltTransitionTime);
    }

    private Tweener shakeTween;
    public void DoShake(float amplitude, float frequency)
    {
        shakeTween = cam.transform.DOShakePosition(1f, .4f, 1, 90).SetLoops(-1);
    }

    public void ResetShake()
    {
        StartCoroutine(ResetShakeRoutine());
    }
    public IEnumerator ResetShakeRoutine()
    {
        /// needs to be fixed!

        shakeTween.SetLoops(1);
        cam.transform.DOKill(); // not optimal, sometimes kills the tilt or fov stuff too...

        yield return shakeTween.WaitForCompletion();

        cam.transform.DOLocalMove(Vector3.zero, .2f);

        print("Shake reseted");
    }

    #endregion


    /// double click the field below to show all headBob code
    #region HeadBob

    private void CheckMotion()
    {
        float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

        ResetPosition();

        if (speed < hbToggleSpeed) return;

        PlayMotion(FootStepMotion());
    }

    private void PlayMotion(Vector3 motion)
    {
        cam.transform.localPosition += motion * Time.deltaTime;
    }

    private void ResetPosition()
    {
        if (cam.transform.localPosition == hbStartPos) return;
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, hbStartPos, 1 * Time.deltaTime);
    }

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * hbFrequency) * hbAmplitude;
        pos.x += Mathf.Cos(Time.time * hbFrequency * 0.5f) * hbAmplitude * 2f;
        return pos;
    }


    private Vector3 FocusTarget()
    {
        Vector3 pos = new Vector3(transform.position.x, camT.position.y, transform.position.z);
        pos += camT.forward * 15f;
        return pos;
    }

    #endregion
}