using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public InputActionMap inputActionMap;
    private InputAction _mMoveAction;
    private InputAction _mJumpAction;
    private InputAction _mSprintAction;
    private InputAction _mCrouchAction;
    
    private Vector2 _mMoveAmt;
    
    
    [Header("Movement")]
    private float _moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;
    
    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool _readyToJump;
    
    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float _startYScale;

    [Header("Ground Check")] 
    public float playerHeight;
    public LayerMask whatIsGround;
    private bool _isGrounded;
    
    public Transform orientation;

    private float _horizontalInput;
    private float _verticalInput;
    
    private Vector3 _moveDirection;
    
    private Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    private void OnEnable()
    {
        inputActionMap.FindAction("Player").Enable();
    }

    private void OnDisable()
    {
        inputActionMap.FindAction("Player").Disable();
    }
    
    private void Awake()
    {
        _mMoveAction = InputSystem.actions.FindAction("Move");
        _mJumpAction = InputSystem.actions.FindAction("Jump");
        _mSprintAction = InputSystem.actions.FindAction("Sprint");
        _mCrouchAction = InputSystem.actions.FindAction("Crouch");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        _readyToJump = true;
        
        _startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    private void Update()
    {
        //ground check
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        
        MyInput();
        SpeedControl();
        StateHandler();

        if (_isGrounded)
        {
            rb.linearDamping = groundDrag;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
    
    private void MyInput()
    {
        //move
        _horizontalInput = _mMoveAction.ReadValue<Vector2>().x;
        _verticalInput = _mMoveAction.ReadValue<Vector2>().y;
        
        //jump
        if (_mJumpAction.IsPressed() && _readyToJump && _isGrounded)
        {
            _readyToJump = false;
            Jump();
            
            StartCoroutine(ResetJumpCooldown());
        }
        
        //crouch
        if (_mCrouchAction.IsPressed())
        {
            transform.localScale = new Vector3(transform.localScale.x , crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        
        //uncrouch
        if (_mCrouchAction.WasReleasedThisFrame())
        {
            transform.localScale = new Vector3(transform.localScale.x , _startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        
        //crouch
        if (_mMoveAction.IsPressed())
        {
            state = MovementState.crouching;
            _moveSpeed = crouchSpeed;
        }
        
        //Sprint
        if (_isGrounded && _mSprintAction.IsPressed())
        {
            state = MovementState.sprinting;
            _moveSpeed = sprintSpeed;
        }
        
        //walk
        else if (_isGrounded)
        {
            state = MovementState.walking;
            _moveSpeed = walkSpeed;
        }
        
        //air
        else
        {
            state = MovementState.air;
            
        }
    }

    private void MovePlayer()
    {
        //calc movement direction
        _moveDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;
        
        float moveForce = _moveSpeed * 10f;
        
        if (_isGrounded)
        {
            rb.AddForce(_moveDirection.normalized * moveForce, ForceMode.Force);
        }else if (!_isGrounded)
        {
            rb.AddForce(_moveDirection.normalized * moveForce * airMultiplier, ForceMode.Force);
        }

    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > _moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * _moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        //reset y velocity to 0
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private IEnumerator ResetJumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        _readyToJump = true;
    }
}
