using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Dave MovementLab - Sliding
///
// Content:
/// - sliding ability
/// 
// Note:
/// Sliding is basically crouching while moving
/// Crouching is handled by the PlayerMovement script


public class Sliding_MLab : MonoBehaviour
{
    [Header("References")]
    public Transform orientation; // orientation object inside the player
    /// public Transform playerObj; // player Object with the blue capsule and collider on it
    private Rigidbody rb;
    private PlayerMovement_MLab pm; // script reference to the PlayerMovement script

    [Header("Sliding")]
    public float minSlideTime = 0.2f;
    public float maxSlideTime = 0.75f; // how long the slide maximally lasts
    public float slideForce = 200f;
    private float slideTimer;
    public float slideCooldown = 0.5f;

    public float slideYScale = 0.5f; // how tall the playerObj is while sliding
    private float startYScale;

    private Vector3 startInputDirection;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    public bool allowSlideHoldDown;
    private bool bufferSlide;
    private bool readyToSlide = true;
    private bool stopSlideAsap;

    private void Start()
    {
        // get references
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement_MLab>();

        // save the normal scale of the player
        startYScale = transform.localScale.y;

        readyToSlide = true;
    }

    private void Update()
    {
        // get the W,A,S,D keyboard inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // if you press down the slide key while moving -> StartSlide
        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
        {
            if (allowSlideHoldDown) bufferSlide = true;
            else if (pm.grounded && readyToSlide) bufferSlide = true;
        }

        // if you release the slide key while sliding -> StopSlide
        if (Input.GetKeyUp(slideKey))
        {
            if (allowSlideHoldDown) bufferSlide = false;

            if (pm.sliding) stopSlideAsap = true;
        }

        // slide buffering
        if (bufferSlide && pm.grounded && readyToSlide)
        {
            StartSlide();
            bufferSlide = false;
        }

        // unslide if slide key was released and minSlideTime exceeded
        if (stopSlideAsap && maxSlideTime - slideTimer > minSlideTime)
        {
            stopSlideAsap = false;
            StopSlide();
        }

        // unsliding if no longer grounded
        if (pm.sliding && !pm.grounded)
            StopSlide();
    }

    private void FixedUpdate()
    {
        // make sure that sliding movement is continuously called while sliding
        if (pm.sliding) SlidingMovement();
    }

    public void StartSlide()
    {
        if (!pm.IsStateAllowed(PlayerMovement_MLab.MovementMode.sliding))
            return;

        if (!pm.grounded) return;

        // this causes the player to change to MovementMode.sliding
        pm.sliding = true;
        readyToSlide = false;

        // shrink the player down
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);

        // after shrinking, you'll be a bit in the air, so add downward force to hit the ground again
        /// you don't really notice this while playing
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        // set the slideTimer
        slideTimer = maxSlideTime;

        // idk, feels weird but the idea would be ok I guess
        // startInputDirection = orientation.forward * pm.verticalInput + orientation.right * pm.horizontalInput;
    }

    private void SlidingMovement()
    {
        // calculate the direction of your keyboard input relative to the players orientation (where the player is looking)
        Vector3 inputDirection = Vector3.Normalize(orientation.forward * verticalInput + orientation.right * horizontalInput);

        // Mode 1 - Sliding Normal
        /// slide time is limited
        if (!pm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            // add force in the direction of your keyboard input
            rb.AddForce(inputDirection * slideForce, ForceMode.Force);

            // count down timer
            slideTimer -= Time.deltaTime;
        }

        // Mode 2 - Sliding down slopes
        /// can slide for as long as the slope lasts
        else
        {
            // add force in the direction of your keyboard input
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        // stop sliding again if the timer runs out
        if (slideTimer <= 0) StopSlide();
    }

    public void StopSlide()
    {
        pm.sliding = false;

        // reset the player scale back to normal again
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

        Invoke(nameof(ResetSlide), slideCooldown);
    }

    private void ResetSlide()
    {
        readyToSlide = true;
    }
}