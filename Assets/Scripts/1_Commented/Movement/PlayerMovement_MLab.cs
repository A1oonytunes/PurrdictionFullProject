using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Dave;
using static UnityEngine.Rendering.DebugUI;
using System.Security.Cryptography;


// Dave MovementLab - PlayerMovement
///
// Content:
/// - basic player movement (x, z axis)
/// - slope movement
/// - jumping & double jumping
/// - crouching & walking
/// - full state handler
///
// Note:
/// The PlayerMovement script keeps track of the movementState the player is currently in.
/// For example, as soon as the player starts wallRunning through the WallRunning_MLab script, 
/// the state of the player here will be set to MovementState.wallrunning.
/// The PlayerMovement script also handles all speed limitations (maxSpeed of the player), depending on which state the player currently is in.
/// 
// I also created a tutorial on playerMovement, so if you struggle to understand this script just watch it
// My Tutorial: https://youtu.be/f473C43s8nE


public class PlayerMovement_MLab : MonoBehaviour
{
    public float playerHeight = 2f;

    /// this is an empty gameObject inside the player, it is rotated by the camera
    /// -> keeps track of where the player is looking -> orientation.forward is the direction you're looking in
    public Transform orientation; 

    /// public Transform playerObj; // your player object with the collider on it

    [Header("Movement")]
    public float moveForce = 12f;
    /// how much air control you have
    /// for example: airMultiplier = 0.5f -> you can only move half as fast will being in the air
    public float airMultiplier = 0.4f;

    public float groundDrag = 5f;

    public float jumpForce = 13f;
    public float jumpCooldown = 0.25f;

    public float crouchYScale = 0.5f; // how tall your player is while crouching (0.5f -> half as tall as normal)
    private float startYScale;
    private bool crouchStarted;

    [Header("Special Movement")]
    public int doubleJumps = 1;
    private int doubleJumpsLeft;

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode walkKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    private bool readyToJump = true;

    [Header("Speed handling")]
    // these variables define how fast your player can move while being in the specific movemt mode
    public float walkMaxSpeed = 4f;
    public float sprintMaxSpeed = 7f;
    public float crouchMaxSpeed = 2f;
    public float slopeSlideMaxSpeed = 30f;
    public float wallJumpMaxSpeed = 12f;
    public float climbMaxSpeed = 3f;
    public float dashMaxSpeed = 15f;
    public float swingMaxSpeed = 17f;
    public float airMaxSpeed = 7f;

    public float limitedMaxSpeed = 20f; // changes based on how fast the player needs to go

    private float maxSpeed; // this variable changes depending on which movement mode you are in
    private float desiredMaxSpeed; // needed to smoothly change between speed limitations
    private float desiredMaxSpeedLastFrame; // the previous desired max speed

    public float speedIncreaseMultiplier = 1.5f; // how fast the maxSpeed changes
    public float slopeIncreaseMultiplier = 2.5f; // how fast the maxSpeed changes on a slope

    /// how fast your player can maximally move on the y axis
    /// if set to -1, y speed will not be limited
    [HideInInspector] public float maxYSpeed;

    [Header("Ground Detection")]
    public LayerMask whatIsGround;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public float maxSlopeAngle = 40f; // how steep the slopes you walk on can be

    [Header("Jump Prediction")]
    // this is needed for precise jumping and walljumping
    public float maxJumpRange;
    public float maxJumpHeight;

    [Header("References")]
    // all script references are assigned in void Start
    private PlayerCam_MLab cam;
    private WallRunning_MLab wr;

    [Header("Momentum")]
    private MomentumExtension momentumExtension;
    private bool momentumExtensionEnabled;

    [Header("Movement Modes")] 
    [HideInInspector] public MovementMode mm; // this variable stores the current movement mode of the player
    public enum MovementMode // here are all movement modes defined
    {
        unlimited, // players speed is not being limited at all
        limited, // limit speed to a specific value using EnableLimitedSpeed()
        freeze, // player can't move at all
        dashing,
        sliding,
        crouching,
        sprinting,
        walking,
        wallrunning,
        walljumping,
        climbing,
        swinging,
        air
    };

    // these bools are activated from different scripts
    // if for example the wallrunning bool is set to true, the movement mode will change to MovementMode.wallrunning#

    [HideInInspector] public bool freeze;
    [HideInInspector] public bool unlimitedSpeed;
    [HideInInspector] public bool restricted;
    [HideInInspector] private bool tierTwoRestricted;
    [HideInInspector] public bool sprinting;
    [HideInInspector] public bool climbing;
    [HideInInspector] public bool sliding;
    [HideInInspector] public bool dashing;
    [HideInInspector] public bool crouching;
    [HideInInspector] public bool swinging;
    [HideInInspector] public bool wallrunning;
    [HideInInspector] public bool walljumping;

    // these bools are changed using specific functions
    [HideInInspector] private bool limitedSpeed;

    // other variables
    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float verticalInput;

    [HideInInspector] public bool grounded;

    private Vector3 moveDirection;

    private Rigidbody rb; // the players rigidbody

    RaycastHit slopeHit; // variable needed for slopeCheck


    // text variables needed to display the speed and movement state ingame

    [Header("Debug")]
    public TextMeshProUGUI text_speed;
    public TextMeshProUGUI text_ySpeed;
    public TextMeshProUGUI text_moveState;
    public TextMeshProUGUI text_speedChangeFactor;

    private void Start()
    {
        // if the player has not yet assigned a groundMask, just set it to "Default"
        if (whatIsGround.value == 0)
            whatIsGround = LayerMask.GetMask("Default");

        // assign references
        cam = GetComponent<PlayerCam_MLab>();
        wr = GetComponent<WallRunning_MLab>();
        rb = GetComponent<Rigidbody>();

        // freeze all rotation on the rigidbody, otherwise the player falls over
        /// (like you would expect from a capsule with round surface)
        rb.freezeRotation = true;

        // if maxYSpeed is set to -1, the y speed of the player will be unlimited
        /// I only limit it while climbing or wallrunning
        maxYSpeed = -1;

        startYScale = transform.localScale.y;

        readyToJump = true;

        if (GetComponent<MomentumExtension>() != null)
        {
            momentumExtension = GetComponent<MomentumExtension>();
            momentumExtensionEnabled = true;
        }
    }

    private void Update()
    {
        print("slope" + OnSlope());

        // make sure to call all functions every frame
        MyInput();
        LimitVelocity();
        HandleDrag();
        StateHandler();

        // shooting a raycast down from the middle of the player and checking if it hits the ground
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        // if you hit the ground again after double jumping, reset your double jumps
        if (grounded && doubleJumpsLeft != doubleJumps)
            ResetDoubleJumps();

        if (Input.GetKeyDown(KeyCode.J))
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 50f, whatIsGround))
            {
                JumpToPosition(hit.point, 10f);
                print("trying to jump to " + hit.point);
            }
        }

        DebugText();
    }

    /// functions that directly move the player should be called in FixedUpdate()
    /// this way your movement is not dependent on how many FPS you have
    /// if you call it in void Update, a player with 120FPS could move twice as fast as someone with just 60FPS
    private void FixedUpdate()
    {
        // if you're walking, sprinting, crouching or in the air, the MovePlayer function, which takes care of all basic movement, should be active
        /// this also makes sure that you can't move left or right while dashing for example
        if (mm == MovementMode.walking || mm == MovementMode.sprinting || mm == MovementMode.crouching || mm == MovementMode.air)
            MovePlayer();

        else
            LimitVelocity();
    }

    #region Input, Movement & Velocity Limiting

    private void MyInput()
    {
        // get your W,A,S,D inputs from your keyboard
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // whenever you press the jump key, you're grounded and readyToJump (which means jumping is not in cooldown),
        // you want to call the Jump() function
        if(Input.GetKey(jumpKey) && grounded && readyToJump)
        {
            readyToJump = false;

            Jump();

            // This will set readyToJump to true again after the cooldown is over
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // if you press the jump key while being in the air -> perform a double jump
        else if(Input.GetKeyDown(jumpKey) && (mm == MovementMode.air || mm == MovementMode.walljumping))
        {
            DoubleJump();
        }

        // if you press the crouch key while not pressing W,A,S or D -> start crouching
        /// Note: if you are pressing W,A,S or D, the sliding script will start a slide instead
        if (Input.GetKeyDown(crouchKey) && horizontalInput == 0 && verticalInput == 0)
            StartCrouch();

        // uncrouch again when you release the crouch key
        if (Input.GetKeyUp(crouchKey) && crouching)
            StopCrouch();

        // whenever you press the walk key, walking should be true
        sprinting = Input.GetKey(walkKey);
    }

    /// entire function only called when mm == walking, sprinting crouching or air
    private void MovePlayer()
    {
        if (restricted || tierTwoRestricted) return;

        // calculate the direction you need to move in
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // To Add the movement force, just use Rigidbody.AddForce (with ForceMode.Force, because you are adding force continuously)

        // movement on a slope
        if (OnSlope())
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveForce * 7.5f, ForceMode.Force);

        // movement on ground
        else if(grounded)
            rb.AddForce(moveDirection.normalized * moveForce * 10f, ForceMode.Force);

        // movement in air
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveForce * 10f * airMultiplier, ForceMode.Force);
    }

    /// this function is always called
    private void LimitVelocity()
    {
        // get the velocity of your rigidbody without the y axis
        Vector3 rbFlatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float currYVel = rb.linearVelocity.y;

        // if you move faster over the x/z axis than you are allowed...
        if (rbFlatVelocity.magnitude > maxSpeed)
        {
            // ...then first calculate what your maximal velocity would be
            Vector3 limitedFlatVelocity = rbFlatVelocity.normalized * maxSpeed;

            // and then apply this velocity to your rigidbody
            rb.linearVelocity = new Vector3(limitedFlatVelocity.x, rb.linearVelocity.y, limitedFlatVelocity.z);
        }
        
        // if you move faster over the y axis than you are allowed...
        if(maxYSpeed != -1 && currYVel > maxYSpeed)
        {
            // special case for swinging
            if (mm == MovementMode.swinging)
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxSpeed * 0.5f, rb.linearVelocity.z);

            // ...just set your rigidbodys y velocity to you maxYSpeed, while leaving the x and z axis untouched
            else
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxYSpeed, rb.linearVelocity.z);
        }
    }

    /// function called the entire time
    private void HandleDrag()
    {
        // if you're walking or sprinting, apply drag to your rigidbody in order to prevent slippery movement
        if (mm == MovementMode.walking || mm == MovementMode.sprinting)
            rb.linearDamping = groundDrag;

        // in any other case you don't want any drag
        else
            rb.linearDamping = 0;
    }

    #endregion

    #region Jump Abilities

    /// called when jumpKeyPressed, readyToJump and grounded
    public void Jump()
    {
        // while dashing you shouldn't be able to jump
        if (dashing) return;

        // reset the y velocity of your rigidbody, while leaving the x and z velocity untouched
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // add upward force to your rigidbody
        /// make sure to use ForceMode.Impulse, since you're only adding force once
        rb.AddForce(orientation.up * jumpForce, ForceMode.Impulse);
    }

    /// called when in air and jumpKey is pressed
    public void DoubleJump()
    {
        // if you don't have any double jumps left, stop the function
        if (doubleJumpsLeft <= 0) return;

        /// this is just for bug-fixing
        if (mm == MovementMode.wallrunning || mm == MovementMode.climbing) return;

        // get rb velocity without y axis
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        // find out how large this velocity is
        float flatVelMag = flatVel.magnitude;

        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // reset rb velocity in the correct direction while maintaing speed
        /// for example, you're jumping forward, then in the air, you turn around and quickly jump back
        /// you now want to take the speed you had in the forward direction and apply it to the backward direction
        /// otherwise you would try to jump against your old forward speed
        rb.linearVelocity = inputDirection.normalized * flatVelMag;

        // add jump force
        /// make sure to use ForceMode.Impulse, since you're only adding force once
        rb.AddForce(orientation.up * jumpForce, ForceMode.Impulse);

        doubleJumpsLeft--;
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    public void ResetDoubleJumps()
    {
        doubleJumpsLeft = doubleJumps;
    }

    // Uses Vector Maths to make the player jump exactly to a desired position
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight, Vector3 startPosition = new Vector3(), float maxRestrictedTime = 1f)
    {
        tierTwoRestricted = true;

        if (startPosition == Vector3.zero) startPosition = transform.position;

        Vector3 velocity = PhysicsExtension.CalculateJumpVelocity(startPosition, targetPosition, trajectoryHeight);

        // enter limited state
        Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);
        EnableLimitedState(flatVel.magnitude);

        velocityToSet = velocity;
        Invoke(nameof(SetVelocity), 0.05f);
        Invoke(nameof(EnableMovementNextTouchDelayed), 0.01f);

        Invoke(nameof(ResetRestrictions), maxRestrictedTime);
    }
    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        rb.linearVelocity = velocityToSet;
        cam.DoFov(100f);
    }
    private void EnableMovementNextTouchDelayed()
    {
        enableMovementOnNextTouch = true;
    }

    public void ResetRestrictions()
    {
        if (tierTwoRestricted)
        {
            tierTwoRestricted = false;
            cam.ResetFov();
        }

        DisableLimitedState();
    }

    #endregion

    #region Crouching

    /// called when crouchKey is pressed down
    private void StartCrouch()
    {
        if (!IsStateAllowed(MovementMode.crouching))
            return;

        // shrink the player down
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

        // after shrinking, you'll be a bit in the air, so add downward force to hit the ground again
        /// you don't really notice this while playing
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        crouching = true;
    }

    /// called when crouchKey is released
    private void StopCrouch()
    {
        // make sure your players size is the same as before
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

        crouching = false;
    }

    #endregion

    #region StateMachine

    // Now this is a giantic function I know, but don't worry, it's extremly simple
    // Basically it just decides in which movement mode the player is currently in and sets the maxSpeed accordingly
    // Also in a few states there needs to be done something extra (like in freeze), then I just added that code in there
    MovementMode movementModeLastFrame;
    MovementMode previousMovementMode;
    private void StateHandler()
    {
        // Mode - Freeze
        if (freeze)
        {
            mm = MovementMode.freeze;
            desiredMaxSpeed = 0f;

            // make sure the player can't move at all
            rb.linearVelocity = Vector3.zero;
        }

        // Mode - Unlimited
        else if (unlimitedSpeed)
        {
            mm = MovementMode.unlimited;

            // this way the player can go as fast as he wants
            desiredMaxSpeed = 1234.5678f;
        }

        // Mode - Limited
        else if (limitedSpeed)
        {
            mm = MovementMode.limited;
            desiredMaxSpeed = limitedMaxSpeed;
        }

        // Mode - Dashing
        else if (dashing)
        {
            mm = MovementMode.dashing;
            desiredMaxSpeed = dashMaxSpeed;
        }

        // SubMode - WallJumping
        else if (walljumping)
        {
            mm = MovementMode.walljumping;
            desiredMaxSpeed = wallJumpMaxSpeed;
        }

        // Mode - Wallrunning
        else if (wallrunning)
        { 
            mm = MovementMode.wallrunning;
            desiredMaxSpeed = sprintMaxSpeed;
        }

        // Mode - Climbing
        else if (climbing)
        {
            mm = MovementMode.climbing;
            desiredMaxSpeed = climbMaxSpeed;
        }

        // Mode - Sliding
        else if (sliding)
        {
            mm = MovementMode.sliding;

            if (OnSlope() && rb.linearVelocity.y < 0.2f)
            {
                desiredMaxSpeed = slopeSlideMaxSpeed;
            }
            else
                desiredMaxSpeed = sprintMaxSpeed;
        }

        // Mode - Crouching
        else if (crouching && grounded)
        {
            mm = MovementMode.crouching;
            desiredMaxSpeed = crouchMaxSpeed;
        }

        // Mode - Sprint
        else if (grounded && sprinting)
        {
            mm = MovementMode.sprinting;
            desiredMaxSpeed = sprintMaxSpeed;
        }

        // Mode - Walk
        else if (grounded)
        {
            mm = MovementMode.walking;
            desiredMaxSpeed = walkMaxSpeed;
        }

        // Mode - Swinging
        else if (swinging)
        {
            mm = MovementMode.swinging;
            desiredMaxSpeed = swingMaxSpeed;
        }

        // Mode - Air
        else
        {
            mm = MovementMode.air;

            if (desiredMaxSpeed < walkMaxSpeed)
                desiredMaxSpeed = sprintMaxSpeed;

            else
                desiredMaxSpeed = walkMaxSpeed;
        }

        bool desiredMaxSpeedHasChanged = desiredMaxSpeed != desiredMaxSpeedLastFrame;

        // minimum momentum
        if (momentumExtensionEnabled && maxSpeed < momentumExtension.minimalMomentum && mm != MovementMode.freeze)
            maxSpeed = momentumExtension.minimalMomentum;

        if (momentumExtensionEnabled)
            UpdateMomentumBasedMaxSpeed();
        else
            maxSpeed = desiredMaxSpeed;

        // movement mode switched
        if (movementModeLastFrame != mm)
        {
            previousMovementMode = movementModeLastFrame;

            // update increase and decrease speedChangeFactors
            if (momentumExtensionEnabled)
            {
                if (mm != MovementMode.air)
                    increaseSpeedChangeFactor = momentumExtension.GetIncreaseSpeedChangeFactor(mm);
                if (previousMovementMode != MovementMode.air)
                    decreaseSpeedChangeFactor = momentumExtension.GetDecreaseSpeedChangeFactor(previousMovementMode);
            }
        }

        desiredMaxSpeedLastFrame = desiredMaxSpeed;
        movementModeLastFrame = mm;

        // if you're walking or sprinting, the head bob movement of the camer should be enabled
        cam.hbEnabled = mm == MovementMode.walking || mm == MovementMode.sprinting;
    }

    private bool isIncreasingMaxSpeed;
    private float increaseSpeedChangeFactor;
    private float decreaseSpeedChangeFactor;
    private void UpdateMomentumBasedMaxSpeed()
    {
        if (!momentumExtensionEnabled)
        {
            Debug.LogError("Trying to update maxSpeed based on momentum but momentumExtension is not enabled");
            return;
        }

        if (maxSpeed == desiredMaxSpeed)
            return;

        isIncreasingMaxSpeed = desiredMaxSpeed > maxSpeed;
        float speedChangeFactor = isIncreasingMaxSpeed ? increaseSpeedChangeFactor : decreaseSpeedChangeFactor;

        if (!isIncreasingMaxSpeed)
            speedChangeFactor *= momentumExtension.GetSurfaceSpeedDecreaseFactor(grounded);

        if (speedChangeFactor == -1)
        {
            maxSpeed = desiredMaxSpeed;
            return;
        }

        if (isIncreasingMaxSpeed)
        {
            // only increase max speed if trying to reach it
            if (rb.linearVelocity.magnitude < maxSpeed - 1)
                return;

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f) * 2f;

                maxSpeed += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease * speedChangeFactor;
            }
            else
                maxSpeed += Time.deltaTime * speedIncreaseMultiplier * speedChangeFactor;
        }
        else
        {
            maxSpeed -= Time.deltaTime * speedIncreaseMultiplier * speedChangeFactor;
        }
    }

    public void EnableLimitedState(float speedLimit)
    {
        limitedMaxSpeed = speedLimit;
        limitedSpeed = true;
    }
    public void DisableLimitedState()
    {
        limitedSpeed = false;
    }

    #endregion

    #region Variables

    public bool OnSlope()
    {
        // shoot a raycast down to check if you hit something
        /// the "out slopeHit" bit makes sure that you store the information of the object you hit
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.5f))
        {
            // calculate the angle of the ground you're standing on (how steep it is)
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

            // check if the angle is smaller than your maxSlopeAngle
            /// -> that means you're standing on a slope -> return true
            return angle < maxSlopeAngle && angle != 0;
        }

        // if the raycast doesn't hit anything, just return false
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        // calcualte the direction you need to move relative to the slope you're standing on
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    #endregion

    #region Moving Platforms

    private Rigidbody movingPlatform;
    public void AssignPlatform(Rigidbody platform)
    {
        movingPlatform = platform;
    }
    public void UnassignPlatform()
    {
        movingPlatform = null;
    }

    #endregion

    #region Collision Detection

    private bool enableMovementOnNextTouch;
    private void OnCollisionEnter(Collision collision)
    {
        bool touch = false;
        print("Contact count" + collision.contactCount);
        // Note: What is ground layer means Layer 7!
        print("Contact Layer " + collision.collider.gameObject.layer + " / " + whatIsGround.value);
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.collider.gameObject.layer == 9 || collision.collider.gameObject.layer == 10)
                touch = true;
        }

        if (touch) print("GroundObjectTouched");

        print("event sucessfully called");

        if (enableMovementOnNextTouch && touch)
        {
            // I don't know anymore lol
            GetComponent<Grappling_MLab>().OnObjectTouch(); // this stops active grapples

            enableMovementOnNextTouch = false;
            ResetRestrictions();
        }
    }

    #endregion

    #region Getters

    public bool IsStateAllowed(MovementMode movementMode)
    {
        if (!momentumExtensionEnabled)
            return true;

        return momentumExtension.IsStateAllowed(movementMode, maxSpeed);
    }

    #endregion

    #region Text Displaying

    private void DebugText()
    {
        if (text_speed != null)
        {
            Vector3 rbFlatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            text_speed.SetText("Speed: " + Round(rbFlatVelocity.magnitude, 1) + "/" + Round(maxSpeed,0));
        }

        if (text_ySpeed != null)
            text_ySpeed.SetText("Y Speed: " + Round(rb.linearVelocity.y, 1));

        if (text_moveState != null)
            text_moveState.SetText(mm.ToString());

        if (!momentumExtensionEnabled)
            return;

        if (text_speedChangeFactor != null)
        {
            if (isIncreasingMaxSpeed)
                text_speedChangeFactor.SetText("Increase: " + increaseSpeedChangeFactor.ToString());
            else
            {
                text_speedChangeFactor.SetText("Decrease: " + (decreaseSpeedChangeFactor*momentumExtension.GetSurfaceSpeedDecreaseFactor(grounded)).ToString());
            }
        }
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }

    #endregion
}