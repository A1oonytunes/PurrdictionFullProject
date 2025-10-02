using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dashing_MLab_Uncommented : MonoBehaviour
{
    public Transform orientation;
    public Transform playerCam;

    [Header("References")]
    private Rigidbody rb;
    private PlayerMovement_MLab pm;
    private PlayerCam_MLab cam;

    [Header("Settings")]
    public float dashForce = 70f;
    public float dashUpwardForce = 2f;
    public float maxUpwardVel = 0f;
    [Tooltip("how long the dash lasts (in seconds)")]
    public float dashDuration = 0.4f;
    [Tooltip("when active -> forward force is applied in direction of camera.forward")]
    public bool useCameraForward = false;
    public bool allowForwardDirection = true;
    public bool allowBackDirection = true;
    public bool allowSidewaysDirection = true;
    public bool disableGravity = false;
    public bool resetYVel = true;

    [Header("Cooldown")]
    public float dashCd = 1.5f;
    private float dashCdTimer;

    [Header("Input")]
    public KeyCode dashKey = KeyCode.E;


    private void Start()
    {
        if(playerCam == null)
            playerCam = Camera.main.transform;

        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement_MLab>();
        cam = GetComponent<PlayerCam_MLab>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(dashKey))
            Dash();

        // cooldown timer
        if (dashCdTimer > 0)
            dashCdTimer -= Time.deltaTime;
    }

    private void Dash()
    {
        // cooldown implementation
        if (dashCdTimer > 0) return;
        else dashCdTimer = dashCd;

        if (maxUpwardVel == 0)
            pm.maxYSpeed = -1;

        pm.dashing = true;

        cam.DoFov(95, .2f);

        Transform forwardT;

        if (useCameraForward)
            forwardT = playerCam;
        else
            forwardT = orientation;

        Vector3 direction = GetDirection(forwardT);

        Vector3 force = direction * dashForce + orientation.up * dashUpwardForce;

        if (disableGravity)
            rb.useGravity = false;

        if(resetYVel)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.y);

        rb.AddForce(force, ForceMode.Impulse);

        if(maxUpwardVel != 0)
            pm.maxYSpeed = maxUpwardVel;

        Invoke(nameof(ResetDash), dashDuration);
    }

    private void ResetDash()
    {
        pm.dashing = false;

        pm.maxYSpeed = -1;

        cam.ResetFov();

        if (disableGravity)
            rb.useGravity = true;
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 forwardV = Vector3.zero;
        Vector3 rightV = Vector3.zero;

        // allowed directions indexes: 0 forward, 1 back, 2 sideways

        // forward
        if (z > 0 && allowForwardDirection)
            forwardV = forwardT.forward;

        // back
        if (z < 0 && allowBackDirection)
            forwardV = -forwardT.forward;

        // right
        if (x > 0 && allowSidewaysDirection)
            rightV = forwardT.right;

        // left
        if (x < 0 && allowSidewaysDirection)
            rightV = -forwardT.right;

        // no input (forward)
        if (x == 0 && z == 0 && allowForwardDirection)
            forwardV = forwardT.forward;

        // forward only allowed direction
        if (allowForwardDirection && allowBackDirection && allowSidewaysDirection)
            forwardV = forwardT.forward;

        return (forwardV + rightV).normalized;
    }
}