using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement_MLab_Uncommented : MonoBehaviour
{
    public float playerHeight = 2f;

    public Transform orientation;

    public Transform playerObj;

    [Header("Movement")]
    public float moveSpeed = 12f;
    public float moveMultiplier = 10f;
    public float airMultiplier = 0.4f;

    public float groundDrag = 5f;

    public float jumpForce = 13f;
    public float jumpCooldown = 0.25f;

    public float crouchYScale = 0.5f;
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
    public float walkMaxSpeed = 4f;
    public float sprintMaxSpeed = 7f;
    public float crouchMaxSpeed = 2f;
    public float dashMaxSpeed = 15f;
    public float swingMaxSpeed = 17f;
    public float airMaxSpeed = 7f;
    private float maxSpeed;

    // if set to -1, y speed will not be affected
    [HideInInspector] public float maxYSpeed;

    [Header("Ground Detection")]
    public LayerMask whatIsGround;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("References")]
    private PlayerCam_MLab cam;
    private WallRunning_MLab wr;

    [Header("Movement Modes")]
    [HideInInspector] public MovementMode mm;
    public enum MovementMode
    {
        unlimited,
        restricted,
        freeze,
        dashing,
        sliding,
        crouching,
        sprinting,
        walking,
        wallrunning,
        climbing,
        swinging,
        air
    };

    [HideInInspector] public bool freeze;
    [HideInInspector] public bool unlimitedSpeed;
    [HideInInspector] public bool restricted; // no basic movment allowed (x,z)
    [HideInInspector] public bool dashing;
    [HideInInspector] public bool grappling;
    [HideInInspector] public bool walking;
    [HideInInspector] public bool wallrunning;
    [HideInInspector] public bool climbing;
    [HideInInspector] public bool crouching;
    [HideInInspector] public bool sliding;
    [HideInInspector] public bool swinging;


    // other variables
    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float verticalInput;

    [HideInInspector] public bool grounded;

    private Vector3 moveDirection;

    private Rigidbody rb;

    RaycastHit slopeHit;

    public TextMeshProUGUI text_speed;
    public TextMeshProUGUI text_ySpeed;
    public TextMeshProUGUI text_moveState;

    private void Start()
    {
        if (whatIsGround.value == 0)
            whatIsGround = LayerMask.GetMask("Default");

        cam = GetComponent<PlayerCam_MLab>();
        wr = GetComponent<WallRunning_MLab>();

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        maxYSpeed = -1;

        startYScale = transform.localScale.y;

        readyToJump = true;
    }

    private void Update()
    {
        MyInput();
        LimitVelocity();
        HandleDrag();
        StateHandler();

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        if (grounded && doubleJumpsLeft != doubleJumps)
            ResetDoubleJumps();

        if(text_speed != null)
        {
            Vector3 rbFlatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            text_speed.SetText("Speed: " + rbFlatVelocity.magnitude);
        }

        if (text_ySpeed != null)
            text_ySpeed.SetText("Y Speed: " + rb.linearVelocity.y);

        if(text_moveState != null)
            text_moveState.SetText(mm.ToString());
    }

    private void FixedUpdate()
    {
        if ((mm == MovementMode.walking || mm == MovementMode.sprinting || mm == MovementMode.crouching || mm == MovementMode.air) && mm != MovementMode.restricted)
            MovePlayer();

        else
            LimitVelocity();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        else if(Input.GetKeyDown(jumpKey) && mm == MovementMode.air)
        {
            DoubleJump();
        }

        if (Input.GetKeyDown(crouchKey) && horizontalInput == 0 && verticalInput == 0)
            crouching = true;

        if (Input.GetKeyUp(crouchKey) && crouching)
            crouching = false;

        walking = Input.GetKey(walkKey);
    }

    /// entire function only called when mm == walking, sprinting crouching or air
    private void MovePlayer()
    {
        float x = horizontalInput;
        float y = verticalInput;

        Debug.Log(x.ToString());

        moveDirection = orientation.forward * y + orientation.right * x;

        // on slope
        if (OnSlope())
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * moveMultiplier, ForceMode.Force);

        // on ground
        else if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * moveMultiplier, ForceMode.Force);

        // in air
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * moveMultiplier * airMultiplier, ForceMode.Force);
    }

    /// always called
    private void LimitVelocity()
    {
        // get rb velocity
        Vector3 rbFlatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float currYVel = rb.linearVelocity.y;

        // check what limits to apply
        bool limitY = false;
        bool limitFlat = false;

        if (rbFlatVelocity.magnitude > maxSpeed)
            limitFlat = true;

        if(maxYSpeed != -1 && currYVel > maxYSpeed)
            limitY = true;

        // calculate limited flatVel
        Vector3 limitedFlatVelocity = rbFlatVelocity.normalized * maxSpeed;

        // only limit flat vel
        if (limitFlat && !limitY)
            rb.linearVelocity = new Vector3(limitedFlatVelocity.x, rb.linearVelocity.y, limitedFlatVelocity.z);
        
        // only limit y vel
        else if(limitY && !limitFlat)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxYSpeed, rb.linearVelocity.z);

        // limit flat and y vel
        else if(limitY && limitFlat)
            rb.linearVelocity = new Vector3(limitedFlatVelocity.x, maxYSpeed, limitedFlatVelocity.x);
    }

    private void HandleDrag()
    {
        if (mm == MovementMode.walking || mm == MovementMode.sprinting)
            rb.linearDamping = groundDrag;

        else
            rb.linearDamping = 0;
    }

    public void Jump()
    {
        // cases in which you can't jump
        if (dashing) return;

        // reset rb y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(orientation.up * jumpForce, ForceMode.Impulse);
    }

    public void DoubleJump()
    {
        if (doubleJumpsLeft <= 0) return;

        if (wr.wallFront || wr.wallBack) return;

        // get flat velocity
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float flatVelMag = flatVel.magnitude;

        // reset rb velocity in correct direction
        rb.linearVelocity = orientation.forward * flatVelMag;
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

    private void StartCrouch()
    {
        playerObj.localScale = new Vector3(playerObj.localScale.x, crouchYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        crouchStarted = true;
    }

    private void StateHandler()
    {
        // Mode - Freeze
        if (freeze)
        {
            mm = MovementMode.freeze;
            maxSpeed = 0f;

            rb.linearVelocity = Vector3.zero;
        }

        // Mode - Unlimited
        if (unlimitedSpeed)
        {
            mm = MovementMode.unlimited;
            maxSpeed = Mathf.Infinity;
        }

        // Mode - Restricted (no input)
        else if (restricted)
        {
            mm = MovementMode.restricted;
            maxSpeed = sprintMaxSpeed;
        }

        // Mode - Dashing and Grappling
        else if (dashing || grappling)
        {
            mm = MovementMode.dashing;
            maxSpeed = dashMaxSpeed;
        }

        // Mode - Wallrunning
        else if (wallrunning)
        {
            mm = MovementMode.wallrunning;
            maxSpeed = sprintMaxSpeed;
        }

        // Mode - Climbing
        else if (climbing)
        {
            mm = MovementMode.climbing;
            maxSpeed = sprintMaxSpeed;
        }

        // Mode - Sliding
        else if (sliding)
        {
            mm = MovementMode.sliding;
            maxSpeed = sprintMaxSpeed;
        }

        // Mode - Crouching
        else if (crouching && grounded)
        {
            mm = MovementMode.crouching;
            maxSpeed = crouchMaxSpeed;

            if (!crouchStarted) StartCrouch();
        }

        // Mode - Walk
        else if (grounded && walking)
        {
            mm = MovementMode.walking;
            maxSpeed = walkMaxSpeed;
        }

        // Mode - Sprint
        else if (grounded)
        {
            mm = MovementMode.sprinting;
            maxSpeed = sprintMaxSpeed;
        }

        // Mode - Swinging / Grappling
        else if (swinging || grappling)
        {
            mm = MovementMode.swinging;
            maxSpeed = swingMaxSpeed;
        }

        // Mode - Air
        else
        {
            mm = MovementMode.air;

            if (walking)
                maxSpeed = walkMaxSpeed;

            else
                maxSpeed = sprintMaxSpeed;
        }

        // handle head bob movement
        cam.hbEnabled = mm == MovementMode.walking || mm == MovementMode.sprinting ? true : false;

        // uncrouch
        if (crouchStarted && mm != MovementMode.crouching)
        {
            playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
            crouchStarted = false;
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
                return true;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}