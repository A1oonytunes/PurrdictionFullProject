using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Dave MovementLab - Dashing
///
// Content:
/// - dashing ability
/// 
// Note:
/// The script looks a bit complex because you can decide wheter the player is allowed to dash forward, sideways or backwards,
/// but actually the script is quite simple, just read through it and you'll probably get it


public class Dashing_MLab : MonoBehaviour
{
    public Transform orientation; // the players orientation (where he's looking)
    public Transform playerCam;

    [Header("References")]
    private Rigidbody rb;
    private PlayerMovement_MLab pm;
    private PlayerCam_MLab cam;

    [Header("Settings")]
    // how much force is added when dashing forward
    /// Note: the actual maxSpeed you can reach while dashing is defined in the PlayerMovement script
    public float dashForce = 70f;
    public float dashUpwardForce = 2f; // how much upward force is added when dashing
    public float maxUpwardVel = -1; // limit the upwardVelocity if needed (if you keep it on -1, the upwardsVelocity is unlimited)
    public float dashDuration = 0.4f;

    // here are a few settings to customize your dash

    public bool useCameraForward = true;  // when active, the player dashes in the forward direction of the camera (upwards if you look up)
    public bool allowForwardDirection = true; // defines if the player is allowed to dash forwards
    public bool allowBackDirection = true; // defines if the player is allowed to dash backwards
    public bool allowSidewaysDirection = true; // defines if the player is allowed to dash sideways
    public bool disableGravity = false; // when active, gravity is disabled while dashing
    public bool resetYVel = true; // when active, y velocity is resetted before dashing
    public bool resetVel = true; // when active, full velocity reset before dashing

    [Header("Effects")]
    public float dashFov = 95f;

    [Header("Cooldown")]
    public float dashCd = 1.5f; // cooldown of your dash ability
    private float dashCdTimer;

    [Header("Input")]
    public KeyCode dashKey = KeyCode.E;


    private void Start()
    {
        // get all references

        if(playerCam == null)
            playerCam = Camera.main.transform;

        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement_MLab>();
        cam = GetComponent<PlayerCam_MLab>();
    }

    private void Update()
    {
        // if you press the dash key -> call Dash() function
        if (Input.GetKeyDown(dashKey))
            Dash();

        // cooldown timer
        if (dashCdTimer > 0)
            dashCdTimer -= Time.deltaTime;
    }

    private void Dash()
    {
        if (!pm.IsStateAllowed(PlayerMovement_MLab.MovementMode.dashing))
            return;

        // cooldown implementation
        if (dashCdTimer > 0) return;
        else dashCdTimer = dashCd;

        pm.ResetRestrictions();

        // if maxUpwardVel set to default (-1), don't limit the players upward velocity
        if (maxUpwardVel == -1)
            pm.maxYSpeed = -1;

        else
            pm.maxYSpeed = maxUpwardVel;

        // this will cause the PlayerMovement script to change to MovementMode.dashing
        pm.dashing = true;

        // increase the fov of the camera (graphical effect)
        cam.DoFov(dashFov, .2f);

        Transform forwardT;

        // decide wheter you want to use the playerCam or the playersOrientation as forward direction
        if (useCameraForward)
            forwardT = playerCam;
        else
            forwardT = orientation;

        // call the GetDirection() function below to calculate the direction
        Vector3 direction = GetDirection(forwardT);

        // calculate the forward and upward force
        Vector3 force = direction * dashForce + orientation.up * dashUpwardForce;

        // disable gravity of the players rigidbody if needed
        if (disableGravity)
            rb.useGravity = false;

        // add the dash force (deayed)
        delayedForceToApply = force;
        Invoke(nameof(DelayedDashForce), 0.025f);

        // make sure the dash stops after the dashDuration is over
        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    {
        // reset velocity based on settings
        if (resetVel)
            rb.linearVelocity = Vector3.zero;
        else if (resetYVel)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.y);

        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        pm.dashing = false;

        // make sure players maxYSpeed is no longer limited
        pm.maxYSpeed = -1;

        // reset the fov of your camera
        cam.ResetFov();

        // if you disabled it before, activate the gravity of the rigidbody again
        if (disableGravity)
            rb.useGravity = true;
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        // get the W,A,S,D input
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // 2 Vector3 for the forward and right velocity
        Vector3 forwardV = Vector3.zero;
        Vector3 rightV = Vector3.zero;

        // forward
        /// if W is pressed and you're allowed to dash forwards, activate the forwardVelocity
        if (z > 0 && allowForwardDirection)
            forwardV = forwardT.forward;

        // back
        /// if S is pressed and you're allowed to dash backwards, activate the backwardVelocity
        if (z < 0 && allowBackDirection)
            forwardV = -forwardT.forward;

        // right
        /// if D is pressed and you're allowed to dash sideways, activate the right velocity
        if (x > 0 && allowSidewaysDirection)
            rightV = forwardT.right;

        // left
        /// if A is pressed and you're allowed to dash sideways, activate the left velocity
        if (x < 0 && allowSidewaysDirection)
            rightV = -forwardT.right;

        // no input (forward)
        /// If there's no input but dashing forward is allowed, activate the forwardVelocity
        if (x == 0 && z == 0 && allowForwardDirection)
             forwardV = forwardT.forward;

        // forward only allowed direction
        /// if forward is the only allowed direction, activate the forwardVelocity
        if (allowForwardDirection && !allowBackDirection && !allowSidewaysDirection)
            forwardV = forwardT.forward;

        // return the forward and right velocity
        /// if for example both have been activated, the player will now dash forward and to the right -> diagonally
        /// this works for all 8 directions
        return (forwardV + rightV).normalized;
    }
}