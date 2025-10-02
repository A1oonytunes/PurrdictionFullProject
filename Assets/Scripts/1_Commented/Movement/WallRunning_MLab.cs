using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Dave;


// Dave MovementLab - WallRunning
///
// Content:
/// - wallrunning ability
/// - walljumping
/// - climbing ability
/// - climb-jumping
/// - other wallhandling stuff
/// 
// Note:
/// This is probably the longest and most complex script of my player controller,
/// since it handles all wallmovement including jumping etc.
/// 
/// Similar to the PlayerMovement script it has it's own StateMachine for different wall states
/// 
/// I would highly recommend you to open my MovementLab documentation (-> 10 Day Learning Plan) to follow along and
/// understand all of these functions in the correct order. If you do that, it won't be complicated.
/// 
/// -> In the 10 Day Learning Plan you'll also find an explanation of how my wallmovement generally works (highly recommended!)


public class WallRunning_MLab : MonoBehaviour
{
    [Header("Toggle Abilites")]
    public bool EnablePrecisionMode = true;

    [Header("References")]
    public Transform orientation;

    [Header("Wall Running")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce = 200f; // forward wallRunForce, Note: maxSpeed while wallrunning is defined in PlayerMovement_MLab
    public float wallJumpSideForce = 15f; // sidewards force of your wall jump (pushes you away from the wall when jumping)
    public float wallJumpUpForce = 12f; // upward force of your wall jump
    public float pushToWallForce = 100f; // the force that keeps you on the wall
    public float maxWallRunTime = 1f;
    public float wallrunClimbSpeed; // how fast you can move on the y axis when wallrunning diagonally

    public float wallJumpDuration = 0.5f; // the duration of the wallJump, afterwards speed gained decreases quickly back to normal

    public bool useGravity;
    public float gravityCounterForce = 0f; // the higher the value, the lower the effect of gravity while wallrunning

    private float wallRunTimer;

    [Header("Climbing")]
    public float climbForce = 200f; // upward force while climbing
    public float climbJumpUpForce = 15f; // upward force of your climb jump
    public float climbJumpBackForce = 15f; // backwards force of your climb jump (pushes you away from the wall)
    public float maxClimbYSpeed = 5f; // max upward speed while climbing
    public float maxClimbTime = 0.75f;
    public float maxWallLookAngle = 30f; // if you look at the wall with an angle of let's say 45 degrees, you can't climb

    private float wallLookAngle;
    private float climbTimer;
    private bool readyToClimb; // is true if player hits a new wall or has sucessfully exited the old one

    public float minFrontWallAngle = 80; // how steep the wall needs to be
    private float frontWallAngle;

    [Header("BackWallMovement")]
    public float backWallJumpUpForce = 5f;
    public float backWallJumpForwardForce = 12f;

    [Header("Limitations")]
    public bool doJumpOnEndOfTimer = false; // when active, wall jump happens automatically when timer runs out
    public bool resetDoubleJumpsOnNewWall = true; // when active, double jumps get resetted when player hits a new wall
    public bool resetDoubleJumpsOnEveryWall = false; // when active, double jumps get resetted when player hits a wall (always!)
    public int allowedWallJumps = 1; // wall jumps allowed (resets on new wall)
    public int allowedClimbJumps = 1; // climb jumps allowed (resets on new wall)

    [Header("Input")]
    public KeyCode wallJumpKey = KeyCode.Space;
    public KeyCode upwardsRunKey = KeyCode.LeftShift; // the key to press in order to wall run diagonally upwards
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    // this entire section is for defining how long the raycasts forward, sideways and backwards are
    /// these values should work just fine for your game, but if you still want to change them
    /// just set them to public and change them inside of Unity
    private float doubleRayCheckDistance = 0.1f;
    private float wallDistanceSide = 0.7f;
    private float wallDistanceFront = 1f;
    private float wallDistanceBack = 1f;

    private float minJumpHeight = 2f; // the minimal height the player needs to have in order to wallRun

    private float exitWallTime = 0.2f; // just a variable needed to exit walls correctly
    private float exitWallTimer;

    public float minWallNormalAngleChange = 15f; // the minimum angle change that is needed to count a surface as a new wall

    [Header("Gravity")]
    public float customGravity = 0f; // apply custom gravity while wallrunning

    [Header("References")]
    private PlayerMovement_MLab pm;
    private PlayerCam_MLab cam;
    private LedgeGrabbing_MLab lg;
    private Detector_MLab dt;

    [Header("Vaulting")]
    public float vaultDetectionLength;
    public bool topReached;
    public float maxVaultClimbYSpeed;
    public float vaultJumpForwardForce;
    public float vaultJumpUpForce;
    public float vaultCooldown;

    bool vaultClimbStarted;
    bool readyToVault;
    bool vaultPerformed;
    bool midCheck;
    bool feetCheck;

    // RaycastHit variables for all of the raycasts that get performed
    /// These variables store the information of where objects were hit

    private RaycastHit leftWallHit;
    private RaycastHit leftWallHit2;
    private RaycastHit rightWallHit;
    private RaycastHit rightWallHit2;

    private RaycastHit frontWallHit;
    private RaycastHit backWallHit;

    // booleans that get activated when the raycasts hit something

    private bool wallLeft;
    private bool wallLeft2;
    private bool wallRight;
    private bool wallRight2;

    [HideInInspector] public bool wallFront;
    [HideInInspector] public bool wallBack;

    private bool exitingWall; // needed to exit walls correctly

    private bool wallRemembered;
    private Transform lastWall; // the transform of the wall the player previously touched
    private Vector3 lastWallNormal; // the normal of the wall the prlayer previously touched

    private int wallJumpsDone;
    private int climbJumpsDone;

    private Rigidbody rb;

    // different states for wallmovement

    private State state; // this variable stores the current State
    private enum State
    {
        ledgegrabbing,
        wallrunning,
        climbing,
        sliding,
        exiting,
        none
    }

    public bool ledgegrabbing;

    public TextMeshProUGUI text_wallState; // displaying text ingame

    private void Start()
    {
        // if the layermasks are not set to anything, set them to "Default"
        if (whatIsWall.value == 0)
            whatIsWall = LayerMask.GetMask("Default");

        if (whatIsGround.value == 0)
            whatIsGround = LayerMask.GetMask("Default");

        // get the references
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement_MLab>();
        cam = GetComponent<PlayerCam_MLab>();
        lg = GetComponent<LedgeGrabbing_MLab>();
        dt = GetComponent<Detector_MLab>();
    }

    private void Update()
    {
        // make sure to call these funcitons every frame
        CheckForWall();
        StateMachine();
        MyInput();

        // if grounded, next wall should be considered a new one
        if (pm.grounded && lastWall != null && !pm.climbing)
            lastWall = null; // by setting the lastWall to null -> next wall will definitely be seen as a new one

        // just setting the Ui text, ignore
        if(text_wallState != null)
            text_wallState.SetText(state.ToString());
    }

    private void FixedUpdate()
    {
        // if wallrunning has started continously call WallRunningMovement()
        if (pm.wallrunning && !exitingWall)
            WallRunningMovement();

        // if climbing has started continuously call ClimbingMovment()
        if (pm.climbing && !exitingWall)
            ClimbingMovement();
    }

    #region Input & Wallchecks

    private void MyInput()
    {
        // get W,A,S,D input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // upwards and downwards running
        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        // if you're pressing the jump key while wallrunning, or if there's a wall in front or back of you
        if (Input.GetKeyDown(wallJumpKey) && (pm.wallrunning || wallFront || wallBack) && !lg.exitingLedge)
        {
            pm.maxYSpeed = -1; // make sure the players Y speed is unlimited
            WallJump(); // perform a wall jump
        }

        // when climbing, then stopping before the climbTimer runs out, then reentering
        // -> Start climbing again
        /// Note: This is just for reentering, the main way to start climbing is executed by the StateHandler
        if (!pm.climbing && verticalInput > 1 && wallFront && climbTimer > 0 && !exitingWall)
            StartClimbing();
    }

    // this function seems complex, but it's really just a bunch of simple raycasts
    // to better understand it, check out the explanation in the 10 Day Learning Plan
    private void CheckForWall()
    {
        float difference = doubleRayCheckDistance * 0.5f;
        Vector3 differenceV = orientation.forward * difference;

        wallLeft = Physics.Raycast(transform.position - differenceV, -orientation.right, out leftWallHit, wallDistanceSide, whatIsWall);
        wallLeft2 = Physics.Raycast(transform.position + differenceV, -orientation.right, out leftWallHit2, wallDistanceSide, whatIsWall);

        wallRight = Physics.Raycast(transform.position - differenceV, orientation.right, out rightWallHit, wallDistanceSide, whatIsWall);
        wallRight2 = Physics.Raycast(transform.position + differenceV, orientation.right, out rightWallHit2, wallDistanceSide, whatIsWall);

        // wallFront = Physics.Raycast(transform.position, orientation.forward, out frontWallHit, wallDistanceFront, whatIsWall);
        wallFront = Physics.SphereCast(transform.position, 0.25f, orientation.forward, out frontWallHit, wallDistanceFront, whatIsWall);
        wallBack = Physics.Raycast(transform.position, -orientation.forward, out backWallHit, wallDistanceBack, whatIsWall);

        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        // reset readyToClimb and wallJumps whenever player hits a new wall
        if (wallLeft || wallRight || wallFront || wallBack)
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

        // vaulting
        midCheck = Physics.Raycast(transform.position, orientation.forward, vaultDetectionLength, whatIsWall);

        RaycastHit feetHit;
        feetCheck = Physics.Raycast(transform.position + new Vector3(0, -0.9f, 0), orientation.forward, out feetHit, vaultDetectionLength, whatIsWall);

        frontWallAngle = Vector3.Angle(orientation.up, feetHit.normal); // calculate how steep the wall is

        // feetCheck = Physics.SphereCast(transform.position + new Vector3(0, -0.9f, 0), 0.1f, orientation.forward, out feetHit, vaultDetectionLength, whatIsWall);

        topReached = feetCheck && !midCheck;
    }

    /// a bool called to check if player has hit a new wall
    private bool NewWallHit()
    {
        // if lastWall is null, next one is definitely a new wall -> return true
        if (lastWall == null)
            return true;

        // the following statements just check if the new wall (stored in the WallHit variables) is equal to the lastWall
        // if not -> new Wall has been hit -> return true

        if (wallLeft && (leftWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, leftWallHit.normal)) > minWallNormalAngleChange))
            return true;

        else if (wallRight && (rightWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, rightWallHit.normal)) > minWallNormalAngleChange))
            return true;

        else if (wallFront && (frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange))
            return true;

        else if (wallBack && (backWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, backWallHit.normal)) > minWallNormalAngleChange))
            return true;

        // if everything above did not happen, no new wall has been hit -> return false
        return false;
    }

    /// This function is called in WallRunningMovement() and ClimbingMovement()
    /// It simply stores the Transform and Normal of the current wall
    private void RememberLastWall()
    {
        if (wallLeft)
        {
            lastWall = leftWallHit.transform;
            lastWallNormal = leftWallHit.normal;
        }

        if (wallRight)
        {
            lastWall = rightWallHit.transform;
            lastWallNormal = rightWallHit.normal;
        }

        if (wallFront)
        {
            lastWall = frontWallHit.transform;
            lastWallNormal = frontWallHit.normal;
        }

        if (wallBack)
        {
            lastWall = backWallHit.transform;
            lastWallNormal = backWallHit.normal;
        }
    }

    private bool CanWallRun()
    {
        // cast a ray down using the minJumpHeight as distance
        // if this ray hits something, the player is not high enough in the air to wallrun
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    #endregion

    #region Wallrunning

    private void StartWallRun()
    {
        if (!pm.IsStateAllowed(PlayerMovement_MLab.MovementMode.wallrunning))
            return;

        // this will cause the PlayerMovement script to enter the MovementState.wallrunning
        pm.wallrunning = true;

        // limit the y speed while wallrunning
        pm.maxYSpeed = maxClimbYSpeed;

        wallRemembered = false;

        // increase camera fov
        cam.DoFov(100f);

        RememberLastWall();

        // set camera tilt
        if (wallRight) cam.DoTilt(5f);
        if(wallLeft) cam.DoTilt(-5f);
    }

    private void WallRunningMovement()
    {
        // make sure normal gravity is turned off while wallrunning
        rb.useGravity = useGravity;

        // choose the correct wall normal (direction away from the wall)
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        // calculate the forward direction of the wall
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        /// sometimes you need to switch it
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // upwards/downwards force
        if (upwardsRunning)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallrunClimbSpeed, rb.linearVelocity.z);
        if (downwardsRunning)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -wallrunClimbSpeed, rb.linearVelocity.z);

        // push to wall force
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);

        // weaken gravity
        if (useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);

        // remember the last wall
        if (!wallRemembered)
        {
            RememberLastWall();
            wallRemembered = true;
            /// Note: The wallRemembered bool might seem unnecessary but it isn't
            /// Without it, the wall would be remembered every single frame, but this way, it's only called once for every new wallrun
        }
    }

    /// called when wallRunTimer runs out, player performs a walljump or player exits the wall on another way
    private void StopWallRun()
    {
        // activate gravity again
        rb.useGravity = true;

        // leave the MovementMove.wallrunning
        pm.wallrunning = false;

        // this will make sure that the y speed of the player is unlimited again
        pm.maxYSpeed = -1;

        // reset camera fov and tilt
        cam.ResetFov();
        cam.ResetTilt();
    }

    #endregion

    #region Climbing

    private void StartClimbing()
    {
        if (!pm.IsStateAllowed(PlayerMovement_MLab.MovementMode.climbing))
            return;

        // this will cause the PlayerMovement script to enter MovementState.climbing
        pm.climbing = true;

        // limit the players y speed to your maxClimbSpeed, or vaultClimbSpeed
        // pm.maxYSpeed = topReached ? maxVaultClimbYSpeed : maxClimbYSpeed;
        // pm.maxYSpeed = maxClimbYSpeed;

        if (topReached) vaultClimbStarted = true;

        // disable gravity
        rb.useGravity = false;

        wallRemembered = false;

        RememberLastWall();

        // shake the camera, just a cool visual effect
        cam.DoShake(1, 1);
    }

    private void ClimbingMovement()
    {
        // make sure that gravity stays off
        if (rb.useGravity != false)
            rb.useGravity = false;

        float speed = topReached ? maxVaultClimbYSpeed : maxClimbYSpeed;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, speed, rb.linearVelocity.z);

        /* calculate directions

        Vector3 upwardsDirection = Vector3.up;

        // calculate the direction from the player to the wall
        Vector3 againstWallDirection = (frontWallHit.point - orientation.position).normalized;

        // add upwards force
        rb.AddForce(upwardsDirection * climbForce, ForceMode.Force);

        // push player to wall
        // rb.AddForce(againstWallDirection * pushToWallForce, ForceMode.Force);

        // remember the last wall
        if (!wallRemembered)
        {
            RememberLastWall();
            wallRemembered = true;
        }
        */
    }

    private void StopClimbing()
    {
        // activate the gravity again
        rb.useGravity = true;

        // exit MovementMode.climbing
        pm.climbing = false;

        // make sure the players y speed is no longer limited
        pm.maxYSpeed = -1;

        // no longer readyToClimb until the wall is sucessfully exited
        readyToClimb = false;

        // reset cam shake
        cam.ResetShake();

        // reset cam fov and tilt
        cam.ResetFov();
        cam.ResetTilt();

        vaultClimbStarted = false;
    }

    /// called when the player has sucessfully exited a wall
    /// makes climbing possible again
    private void ResetReadyToClimb()
    {
        readyToClimb = true;
    }

    #endregion

    #region Walljumping

    /// Inside of this function the decision between normal or precise wall/climb jumping is made
    public void WallJump()
    {
        if (!pm.IsStateAllowed(PlayerMovement_MLab.MovementMode.walljumping))
            return;

        if (!EnablePrecisionMode)
        {
            NormalWallJump();
            return;
        }

        bool sideWall = wallLeft || wallRight;

        if (sideWall && dt.precisionTargetFound) PreciseWallJump();
        else NormalWallJump();
    }

    private void NormalWallJump()
    {
        if (ledgegrabbing) return;

        /// once again, the full explanation how wall jumping works can be found in the 10 Day Learning Plan

        bool applyUpwardForce = true;

        exitingWall = true;
        exitWallTimer = exitWallTime;

        // create a new vector to store the forceToApply
        Vector3 forceToApply = new Vector3();

        // I'll explain the next if, else if statements all at once here:
        ///
        /// 1. calculate the force to apply by using transform.up and
        /// the normal of the wall hit (which is the direction away from the wall!)
        /// 
        /// 2. if you still have allowed jumps on this wall, set applyUpwardForce to true
        /// 
        /// 3. count up the jumps done

        if (wallLeft)
        {
            forceToApply = transform.up * wallJumpUpForce + leftWallHit.normal * wallJumpSideForce;

            applyUpwardForce = wallJumpsDone < allowedWallJumps;
            wallJumpsDone++;
        }

        else if(wallRight)
        {
            forceToApply = transform.up * wallJumpUpForce + rightWallHit.normal * wallJumpSideForce;

            applyUpwardForce = wallJumpsDone < allowedWallJumps;
            wallJumpsDone++;
        }

        else if (wallFront)
        {
            Vector3 againstWallDirection = (frontWallHit.point - orientation.position).normalized;

            forceToApply = Vector3.up * climbJumpUpForce + -againstWallDirection * climbJumpBackForce;

            applyUpwardForce = climbJumpsDone < allowedClimbJumps;
            climbJumpsDone++;
        }

        else if (wallBack)
        {
            // remember the last wall
            lastWall = backWallHit.transform;

            Vector3 againstWallDirection = (backWallHit.point - orientation.position).normalized;

            forceToApply = Vector3.up * backWallJumpUpForce + -againstWallDirection * backWallJumpForwardForce;

            applyUpwardForce = true;
        }

        else
        {
            print("WallJump was called, but there is no wall in range");
        }

        // if your jump is "allowed", apply the full jump force, including upward force
        if (applyUpwardForce)
        {
            // also reset the y velocity first (as always when jumping)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            delayedForceToApply = forceToApply;
            Invoke(nameof(DelayedForce), 0.05f);

            pm.walljumping = true;
            Invoke(nameof(ResetWallJumpingState), wallJumpDuration);
        }

        // if you try to jump, but you aren't "allowed" to anymore, push yourself away from the wall, but without upward force
        /// For example: you already performed a wallJump on this wall and you're only allowed to do one
        else
        {
            // also reset the y velocity first
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // remove upward force (y axis) from forceToApply
            Vector3 noUpwardForce = new Vector3(forceToApply.x, 0f, forceToApply.z);

            // apply force without upward component
            rb.AddForce(noUpwardForce, ForceMode.Impulse);
        }

        // stop wallRun and climbing immediately
        StopWallRun();
        StopClimbing();
    }

    private Vector3 delayedForceToApply;
    private void DelayedForce()
    {
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetWallJumpingState()
    {
        pm.walljumping = false;
    }

    /// called whenever player hits a new wall
    private void ResetWallJumpsDone()
    {
        wallJumpsDone = 0;
        climbJumpsDone = 0;
    }

    private void PreciseWallJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        // before jumping off, make sure that the last wall is remembered
        RememberLastWall();

        Vector3 midPoint = orientation.position;

        float markerSphereRelativeYPos = dt.markerSphere.position.y - midPoint.y;
        float highestPointOfArc = markerSphereRelativeYPos + pm.maxJumpHeight;

        // no upwards force when point is below player
        if (markerSphereRelativeYPos < 0) highestPointOfArc = 1;

        print("predicted wall jump " + markerSphereRelativeYPos + " " + highestPointOfArc);

        pm.JumpToPosition(dt.markerSphere.position, highestPointOfArc, midPoint);

        // rb.AddForce(PhysicsExtension.CalculateJumpVelocity(lowestPoint, jumpTarget.position, maxJumpHeight), ForceMode.Impulse);
        /// rb.velocity = PhysicsExtension.CalculateJumpVelocity(midPoint, dt.markerSphere.position, highestPointOfArc);

        wallJumpsDone++;

        // stop wallRun and climbing immediately
        StopWallRun();
        StopClimbing();
    }

    #endregion

    #region StateMachine

    // and here's the state machine that handles all wallmovement states
    private void StateMachine()
    {
        // I just defined the bools up here because I'm going to use them multiple times in this StateMachine

        bool leftWall = wallLeft && wallLeft2;
        bool rightWall = wallRight && wallRight2;
        bool sideWall = leftWall || rightWall;
        bool noInput = horizontalInput == 0 && horizontalInput == 0;

        bool climbing = wallFront && verticalInput > 0;


        // State 0 - LedgeGrabbing
        if (ledgegrabbing)
        {
            state = State.ledgegrabbing;

            // everything else in this state gets handled by the LedgeGrabbing_MLab script

            if (pm.climbing) StopClimbing();
        }

        // State 1 - Climbing
        /// Enter state when: there's a wall in front, you're pressing W,
        /// you're ready to climb and not currently exiting a wall
        /// also the wall needs to be steep enough
        else if ((wallFront || topReached) && verticalInput > 0 && wallLookAngle < maxWallLookAngle && !exitingWall && frontWallAngle > minFrontWallAngle)
        {
            print("climbing...");

            state = State.climbing;

            // start climbing if not already started
            if (!pm.climbing && climbTimer > 0) StartClimbing();

            // restart climbing if top has been reached (changes climbspeed)
            if (!vaultClimbStarted && topReached) StartClimbing();

            // count down climbTimer
            if (climbTimer > 0) climbTimer -= Time.deltaTime;

            // exit wall once the timer runs out
            if ((climbTimer < 0) && pm.climbing)
            {
                climbTimer = -1;

                exitingWall = true;
                exitWallTimer = exitWallTime;

                StopClimbing();
            }
        }


        // State 2 - Wallrunning
        /// Enter state when: there's a wall left or right, you're pressing W, 
        /// you're high enough over the ground and not exiting a wall
        else if (sideWall && verticalInput > 0 && CanWallRun() && !exitingWall)
        {
            state = State.wallrunning;

            // startwallrun
            if (!pm.wallrunning) StartWallRun();

            // count down the wallRunTimer
            wallRunTimer -= Time.deltaTime;

            // what happens when the wallRunTimer runs out
            if (wallRunTimer < 0 && pm.wallrunning)
            {
                wallRunTimer = 0;

                // if needed, perform a wall jump
                if (doJumpOnEndOfTimer)
                    WallJump();

                else
                {
                    exitingWall = true; // this will set the state to State.exiting
                    exitWallTimer = exitWallTime; // set the exitWallTimer
                    StopWallRun(); // stop the wallrun
                }
            }
        }


        // State 3 - Sliding
        /// Enter state when: wall back + S Input, sidewalls with A/D but no W input,
        /// or climbing but the climbTimer ran out
        else if ((wallBack && verticalInput < 0) || (((leftWall && horizontalInput < 0) || (rightWall && horizontalInput > 0)) && verticalInput <= 0) || (climbing && climbTimer <= 0))
        {
            state = State.sliding;

            // bug fix
            if (pm.wallrunning)
                StopWallRun();
        }


        // State 4 - Exiting
        /// Enter state when: exitingWall was set to true by other functions
        else if (exitingWall)
        {
            state = State.exiting;

            // make sure the player can't move for a quick time
            pm.restricted = true;

            // exit out of wall run or climb when active
            if (pm.wallrunning)
                StopWallRun();

            if (pm.climbing)
                StopClimbing();

            // handle wall-exiting
            // make sure the timer counts down
            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;

            // what happens if the exitWallTimer runs out
            if (exitWallTimer <= 0 && exitingWall)
            {
                // set extiWall to false again -> state will now change to Stat.none
                exitingWall = false;

                // reset readyToClimb when player has sucessfully exited the wall
                ResetReadyToClimb();
            }
        }


        // State 5 - None
        else
        {
            state = State.none;

            // exit out of wall run or climb when active
            if (pm.wallrunning)
                StopWallRun();

            if (pm.climbing)
                StopClimbing();
        }

        // when no longer exiting the wall, allow normal player movement again
        if (state != State.exiting && pm.restricted)
            pm.restricted = false;
    }

    #endregion

    #region Gizmos (Debugging & Visualizing)

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

    #endregion
}