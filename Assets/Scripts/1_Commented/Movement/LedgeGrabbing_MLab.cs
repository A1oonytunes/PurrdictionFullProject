using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Dave MovementLab - LedgeGrabbing
///
// Content:
/// - detecting and moving towards ledges
/// - holding onto ledges
/// - jumping away from ledges
///
// Note:
/// This script is an extension of the WallRunning_MLab script, I did it this way, because 
/// the WallRunning script is already like 700 lines long
/// 

public class LedgeGrabbing_MLab : MonoBehaviour
{
    [Header("References")]
    private WallRunning_MLab main; // this script is an extension of the main wallrunning script
    private PlayerMovement_MLab pm;
    public Transform orientation;
    private Rigidbody rb;


    [Header("Ledge Grabbing")]
    public Transform cam;
    public KeyCode jumpKey = KeyCode.Space;

    public float moveToLedgeSpeed;
    public float ledgeJumpForwardForce;
    public float ledgeJumpUpForce;
    public float maxLedgeJumpUpSpeed;
    public float maxLedgeGrabDistance;

    public float minTimeOnLedge;
    private float timeOnLedge;

    private bool holding;

    [Header("Ledge Detection")]
    public float ledgeDetectionLength;
    public float ledgeSphereCastRadius;
    public LayerMask whatIsLedge;

    private Transform lastLedge;
    public Transform currLedge;

    private RaycastHit ledgeHit;
    private Vector3 directionToLedge;
    private float distanceToLedge;

    public bool exitingLedge;
    public float exitLedgeTime;
    private float exitLedgeTimer = 0.2f;


    private void Start()
    {
        // get references
        pm = GetComponent<PlayerMovement_MLab>();
        rb = GetComponent<Rigidbody>();
        main = GetComponent<WallRunning_MLab>();
    }

    private void Update()
    {
        LedgeDetection();
        SubStateMachine();
    }

    // a very simple state machine which takes care of the ledge grabbing state
    private void SubStateMachine()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector2 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // SubState 1 - Holding onto ledge
        if (holding)
        {
            FreezeRigidbodyOnLedge();

            if (timeOnLedge > minTimeOnLedge && inputDirection != Vector2.zero) ExitLedgeHold();

            timeOnLedge += Time.deltaTime;
        }

        // SubState 2 - Exiting Ledge
        else if(exitingLedge)
        {
            if (exitLedgeTimer > 0) exitLedgeTimer -= Time.deltaTime;
            else exitingLedge = false;
        }

        if (Input.GetKeyDown(jumpKey) && holding) LedgeJump();
    }

    private void LedgeDetection()
    {
        bool ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.forward, out ledgeHit, ledgeDetectionLength, whatIsLedge);

        if (ledgeHit.transform == null) return;

        directionToLedge = ledgeHit.transform.position - transform.position;
        distanceToLedge = directionToLedge.magnitude;

        if (lastLedge != null && ledgeHit.transform == lastLedge) return;

        if (ledgeDetected && distanceToLedge < maxLedgeGrabDistance && !holding) EnterLedgeHold();
    }

    private void LedgeJump()
    {
        print("ledge jump");

        ExitLedgeHold();

        Invoke(nameof(DelayedForce), 0.05f);
    }

    private void DelayedForce()
    {
        Vector3 forceToAdd = cam.forward * ledgeJumpForwardForce + orientation.up * ledgeJumpUpForce;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(forceToAdd, ForceMode.Impulse);
    }

    private void EnterLedgeHold()
    {
        if (exitingLedge) return;

        print("entered ledge hold");

        main.ledgegrabbing = true;
        holding = true;

        pm.restricted = true;
        pm.unlimitedSpeed = true;

        currLedge = ledgeHit.transform;
        lastLedge = ledgeHit.transform;

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
    }

    bool touchingLedge;
    private void FreezeRigidbodyOnLedge()
    {
        rb.useGravity = false;

        Vector3 directionToLedge = currLedge.position - transform.position;

        if (directionToLedge.magnitude > maxLedgeGrabDistance && holding) ExitLedgeHold();

        // Move player towards ledge
        if (directionToLedge.magnitude > 1f)
        {
            // Vector3 directionToLedge = ledgeHit.transform.position - transform.position;
            // rb.velocity = directionToLedge.normalized * moveToLedgeSpeed;

            if (rb.linearVelocity.magnitude < moveToLedgeSpeed)
                rb.AddForce(directionToLedge.normalized * moveToLedgeSpeed * 1000f * Time.deltaTime);

            /// The current problem is that I can't set the velocity from here, I can only add force
            /// -> but then the force is mainly upwards :D

            print("moving to ledge");
        }

        // Hold onto ledge
        else
        {
            if (pm.unlimitedSpeed) pm.unlimitedSpeed = false;
            if (!pm.freeze) pm.freeze = true;
            ///rb.velocity = Vector3.zero;
            print("hanging on ledge");
        }
    }

    private void ExitLedgeHold()
    {
        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;

        main.ledgegrabbing = false;
        holding = false;
        timeOnLedge = 0;

        pm.freeze = false;
        pm.unlimitedSpeed = false;
        pm.restricted = false;

        rb.useGravity = true;

        StopAllCoroutines();
        Invoke(nameof(ResetLastLedge), 1f);
    }

    private void ResetLastLedge()
    {
        lastLedge = null;
    }


    // checking with collisionEnter an Exit if the ledge has been reached (touched)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Ledge")
        {
            touchingLedge = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.tag == "Ledge")
        {
            touchingLedge = false;
        }
    }
}
