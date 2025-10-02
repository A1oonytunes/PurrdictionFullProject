using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WallRunning_MLab_Uncommented : MonoBehaviour
{
    public Transform orientation;

    [Header("Wall Running")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce = 200f;
    public float wallJumpSideForce = 15f;
    public float wallJumpUpForce = 12f;
    public float pushToWallForce = 100f;
    public float maxWallRunTime = 1f;

    private float wallRunTimer;

    [Header("Climbing")]
    public float climbForce = 200f;
    public float climbJumpUpForce = 15f;
    public float climbJumpBackForce = 15f;
    public float maxClimbYSpeed = 5f;
    public float maxClimbTime = 0.75f;

    private float climbTimer;
    // is true if player hits a new wall or has sucessfully exited the old one
    private bool readyToClimb;

    [Header("BackWallMovement")]
    public float backWallJumpUpForce = 5f;
    public float backWallJumpForwardForce = 12f;

    [Header("Limitations")]
    public bool doJumpOnEndOfTimer = false;
    public bool resetDoubleJumpsOnNewWall = true;
    public bool resetDoubleJumpsOnEveryWall = false;
    public int allowedWallJumps = 1;
    public int allowedClimbJumps = 1;

    [Header("Input")]
    public KeyCode wallJumpKey = KeyCode.Space;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float doubleRayCheckDistance = 0.1f;
    public float wallDistanceSide = 0.7f;
    public float wallDistanceFront = 1f;
    public float wallDistanceBack = 1f;
    public float minJumpHeight = 2f;
    public float exitWallTime = 0.2f;

    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity = false;
    public float customGravity = 0f;
    public float yDrossleSpeed = 0.2f;

    [Header("References")]
    private PlayerMovement_MLab pm;
    private PlayerCam_MLab cam;

    private RaycastHit leftWallHit;
    private RaycastHit leftWallHit2;
    private RaycastHit rightWallHit;
    private RaycastHit rightWallHit2;

    private RaycastHit frontWallHit;
    private RaycastHit backWallHit;

    private bool wallLeft;
    private bool wallLeft2;
    private bool wallRight;
    private bool wallRight2;

    [HideInInspector] public bool wallFront;
    [HideInInspector] public bool wallBack;

    private bool exitingWall;

    private bool wallRemembered;
    private Transform lastWall;

    private int wallJumpsDone;
    private int climbJumpsDone;

    private Rigidbody rb;

    private State state;
    private enum State
    {
        wallrunning,
        climbing,
        sliding,
        exiting,
        none
    }

    public TextMeshProUGUI text_wallState;

    private void Start()
    {
        if (whatIsWall.value == 0)
            whatIsWall = LayerMask.GetMask("Default");

        if (whatIsGround.value == 0)
            whatIsGround = LayerMask.GetMask("Default");

        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement_MLab>();
        cam = GetComponent<PlayerCam_MLab>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
        MyInput();

        if (wallRunTimer < 0 && pm.wallrunning)
        {
            wallRunTimer = 0;

            if(doJumpOnEndOfTimer)
                WallJump();

            else
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
                StopWallRun();
            }
        }

        // handle wall-exiting
        if (exitWallTimer > 0)
        {
            exitWallTimer -= Time.deltaTime;
        }

        if(exitWallTimer <= 0 && exitingWall)
        {
            exitingWall = false;

            // reset readyToClimb when player has sucessfully exited the wall
            ResetReadyToClimb();
        }

        // if grounded, next wall is a new one
        if (pm.grounded && lastWall != null)
            lastWall = null;

        if(text_wallState != null)
            text_wallState.SetText(state.ToString());
    }

    private void FixedUpdate()
    {
        if (pm.wallrunning && !exitingWall)
            WallRunningMovement();

        if (pm.climbing && !exitingWall)
            ClimbingMovement();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(wallJumpKey) && (pm.wallrunning || wallFront || wallBack))
        {
            pm.maxYSpeed = -1;
            WallJump();
        }
    }

    /// do all of the raycasts
    private void CheckForWall()
    {
        float difference = doubleRayCheckDistance * 0.5f;
        Vector3 differenceV = orientation.forward * difference;

        wallLeft = Physics.Raycast(transform.position - differenceV, -orientation.right, out leftWallHit, wallDistanceSide, whatIsWall);
        wallLeft2 = Physics.Raycast(transform.position + differenceV, -orientation.right, out leftWallHit2, wallDistanceSide, whatIsWall);

        wallRight = Physics.Raycast(transform.position - differenceV, orientation.right, out rightWallHit, wallDistanceSide, whatIsWall);
        wallRight2 = Physics.Raycast(transform.position + differenceV, orientation.right, out rightWallHit2, wallDistanceSide, whatIsWall);

        wallFront = Physics.Raycast(transform.position, orientation.forward, out frontWallHit, wallDistanceFront, whatIsWall);

        wallBack = Physics.Raycast(transform.position, -orientation.forward, out backWallHit, wallDistanceBack, whatIsWall);

        // reset readyToClimb and wallJumps whenever player hits a new wall
        if(wallLeft || wallRight || wallFront || wallBack)
        {
            if (NewWallHit())
            {
                ResetReadyToClimb();
                ResetWallJumpsDone();

                if (resetDoubleJumpsOnNewWall)
                    pm.ResetDoubleJumps();

                wallRunTimer = maxWallRunTime;
                climbTimer = maxClimbTime;
            }

            if(resetDoubleJumpsOnEveryWall)
                pm.ResetDoubleJumps();
        }
    }

    private bool NewWallHit()
    {
        if (lastWall == null)
            return true;

        if (wallLeft && leftWallHit.transform != lastWall)
            return true;

        else if (wallRight && rightWallHit.transform != lastWall)
            return true;

        else if (wallFront && frontWallHit.transform != lastWall)
            return true;

        else if (wallBack && backWallHit.transform != lastWall)
            return true;

        return false;
    }

    private bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StartWallRun()
    {
        pm.wallrunning = true;

        pm.maxYSpeed = maxClimbYSpeed;

        rb.useGravity = useGravity;

        wallRemembered = false;

        cam.DoFov(100f);
        
        if(wallRight) cam.DoTilt(5f);
        if(wallLeft) cam.DoTilt(-5f);
    }

    private void WallRunningMovement()
    {  
        if(rb.useGravity) rb.useGravity = false;

        bool leftWall = wallLeft && wallLeft2;
        bool rightWall = wallRight && wallRight2;

        // calculate directions

        Vector3 wallForwardDirection = new Vector3();
        Vector3 againstWallDirection = new Vector3();

        if (leftWall)
        {
            wallForwardDirection = (leftWallHit2.point - leftWallHit.point).normalized;
            againstWallDirection = (leftWallHit.point - orientation.position).normalized;
        }

        if (rightWall)
        {
            wallForwardDirection = (rightWallHit2.point - rightWallHit.point).normalized;
            againstWallDirection = (rightWallHit.point - orientation.position).normalized;
        }

        // lerp upwards velocity of rb to 0 if gravity is turned off

        float velY = rb.linearVelocity.y;

        if (!useGravity)
        {
            if (velY > 0)
                velY -= yDrossleSpeed;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, velY, rb.linearVelocity.z);
        }

        // add forces

        // forward force
        rb.AddForce(wallForwardDirection * wallRunForce, ForceMode.Force);

        // upward force
        //if ((leftWall && input.x < 0) || (rightWall && input.x > 0))
        //    rb.AddForce(orientation.up * climbForce, ForceMode.Force);

        if ((leftWall && horizontalInput < 0) || (rightWall && horizontalInput > 0))
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxClimbYSpeed * 0.5f, rb.linearVelocity.z);

        if (!exitingWall)
            rb.AddForce(againstWallDirection * pushToWallForce, ForceMode.Force);

        if(customGravity != 0)
            rb.AddForce(-orientation.up * customGravity, ForceMode.Force);

        // remember the last wall

        if (!wallRemembered)
        {
            RememberLastWall();
            wallRemembered = true;
        }
    }

    private void StopWallRun()
    {
        rb.useGravity = true;

        pm.wallrunning = false;

        pm.maxYSpeed = -1;

        cam.ResetFov();
        cam.ResetTilt();
    }

    private void StartClimbing()
    {
        pm.climbing = true;

        pm.maxYSpeed = maxClimbYSpeed;

        rb.useGravity = false;

        wallRemembered = false;

        cam.DoShake(1, 1);
    }

    private void ClimbingMovement()
    {
        if (rb.useGravity != false)
            rb.useGravity = false;

        // calculate directions

        Vector3 upwardsDirection = Vector3.up;

        Vector3 againstWallDirection = (frontWallHit.point - orientation.position).normalized;

        // add forces

        rb.AddForce(upwardsDirection * climbForce, ForceMode.Force);

        if(!exitingWall)
            rb.AddForce(againstWallDirection * pushToWallForce, ForceMode.Force);

        // remember the last wall

        if (!wallRemembered)
        {
            RememberLastWall();
            wallRemembered = true;
        }
    }
    private void RememberLastWall()
    {
        if(wallLeft)
            lastWall = leftWallHit.transform;

        if (wallRight)
            lastWall = rightWallHit.transform;

        if (wallFront)
            lastWall = frontWallHit.transform;

        if (wallBack)
            lastWall = backWallHit.transform;
    }

    private void StopClimbing()
    {
        rb.useGravity = true;

        pm.climbing = false;

        // maxYSpeed is reseted when jumping as well
        pm.maxYSpeed = -1;

        readyToClimb = false;

        cam.ResetShake();

        cam.ResetFov();
        cam.ResetTilt();
    }

    private void ResetReadyToClimb()
    {
        readyToClimb = true;
        Debug.Log("ReadyToClimb resetted");
    }

    public void WallJump()
    {
        // idea: allow one full jump, the second one is without upward force

        bool firstJump = true;

        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = new Vector3();

        if (wallLeft)
        {
            lastWall = leftWallHit.transform;

            forceToApply = transform.up * wallJumpUpForce + leftWallHit.normal * wallJumpSideForce;

            firstJump = wallJumpsDone < allowedWallJumps;
            wallJumpsDone++;
        }

        else if(wallRight)
        {
            lastWall = rightWallHit.transform;

            forceToApply = transform.up * wallJumpUpForce + rightWallHit.normal * wallJumpSideForce;

            firstJump = wallJumpsDone < allowedWallJumps;
            wallJumpsDone++;
        }

        else if (wallFront)
        {
            lastWall = frontWallHit.transform;

            Vector3 againstWallDirection = (frontWallHit.point - orientation.position).normalized;

            forceToApply = Vector3.up * climbJumpUpForce + -againstWallDirection * climbJumpBackForce;

            firstJump = climbJumpsDone < allowedClimbJumps;
            climbJumpsDone++;
        }

        else if (wallBack)
        {
            lastWall = backWallHit.transform;

            Vector3 againstWallDirection = (backWallHit.point - orientation.position).normalized;

            forceToApply = Vector3.up * backWallJumpUpForce + -againstWallDirection * backWallJumpForwardForce;

            firstJump = true;
        }

        else
        {
            print("WallJump was called, but there is no wall in range");
        }

        // apply force
        if (firstJump)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(forceToApply, ForceMode.Impulse);
        }

        else
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            Vector3 noUpwardForce = new Vector3(forceToApply.x, 0f, forceToApply.z);

            rb.AddForce(noUpwardForce, ForceMode.Impulse);
        }

        // stop wallRun and climbing immediately
        StopWallRun();
        StopClimbing();
    }

    private void ResetWallJumpsDone()
    {
        wallJumpsDone = 0;
        climbJumpsDone = 0;
    }

    private void StateMachine()
    {
        bool leftWall = wallLeft && wallLeft2;
        bool rightWall = wallRight && wallRight2;
        bool sideWall = leftWall || rightWall;
        bool noInput = horizontalInput == 0 && horizontalInput == 0;

        bool climbing = wallFront && verticalInput > 0;

        // State 1 - Wallrunning
        if (sideWall && verticalInput > 0 && CanWallRun() && !exitingWall)
        {
            state = State.wallrunning;

            if (!pm.wallrunning) StartWallRun();

            wallRunTimer -= Time.deltaTime;
        }

        // State 2 - Climbing
        else if (climbing && readyToClimb && !exitingWall)
        {
            state = State.climbing;

            if (readyToClimb && !pm.climbing)
                StartClimbing();

            if (climbTimer > 0 && !exitingWall)
                StartClimbing();

            if (climbTimer > 0 && pm.climbing) climbTimer -= Time.deltaTime;

            if (climbTimer < 0 && pm.climbing)
            {
                climbTimer = -1;

                StopClimbing();
            }
        }

        // State 3 - Sliding
        // Ok, here in normal language:
        // wallback + back input, or sidewalls with specific side and no forward input, or wallfront without timer
        else if ((wallBack && verticalInput < 0) || (((leftWall && horizontalInput < 0) || (rightWall && horizontalInput > 0)) && verticalInput <= 0) || (climbing && climbTimer <= 0))
        {
            state = State.sliding;

            // bug fix
            if (pm.wallrunning)
                StopWallRun();
        }

        // State 4 - Exiting
        // no input
        else if (exitingWall)
        {
            state = State.exiting;

            pm.restricted = true;

            if (pm.wallrunning)
                StopWallRun();

            if (pm.climbing)
                StopClimbing();
        }

        else
        {
            state = State.none;

            // exit out of WallRun or Climb when active

            if (pm.wallrunning)
                StopWallRun();

            if (pm.climbing)
                StopClimbing();
        }

        if (state != State.exiting && pm.restricted)
            pm.restricted = false;
    }

    private void OnDrawGizmosSelected()
    {
        float difference = doubleRayCheckDistance * 0.5f;
        Vector3 differenceV = orientation.forward * difference;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position - differenceV, orientation.right * wallDistanceSide);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + differenceV, orientation.right * wallDistanceSide);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position - differenceV, -orientation.right * wallDistanceSide);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + differenceV, -orientation.right * wallDistanceSide);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, orientation.forward * wallDistanceFront);

        Gizmos.color = Color.grey;
        Gizmos.DrawRay(transform.position, -orientation.forward * wallDistanceBack);
    }
}