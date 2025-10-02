using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Dave MovementLab - Grappling
///
// Content:
/// - swinging ability
/// - grappling ability
/// 
// Note:
/// This script handles starting and stopping the swinging and grappling ability, as well as moving the player
/// The grappling rope is drawn and animated by the GrapplingRope_MLab script
/// 
/// If you don't understand the difference between swinging and grappling, please read the documentation
/// 
// Also, the swinging ability is based on Danis tutorial
// Credits: https://youtu.be/Xgh4v1w5DxU



/// single, or dual swinging
/// 
/// grappling left or right -> cancels any active swings and grapples
/// no grappling left/right twice in a row
/// swinging -> cancels any active grapples, exit limited state!
/// 
/// This implies that swinging and grappling can never be active at the same time, neither can there be 2 active grapples


public class Grappling_MLab: MonoBehaviour
{
    [Header("ToggleAbilites")]
    public bool EnableSwingingWithForces = true;
    public GrappleMode grappleMode = GrappleMode.Precise;

    [Header("References")]
    public Transform orientation;

    [Header("Swinging")]
    public LayerMask whatIsGrappleable; // you can grapple & swing on all objects that are in this layermask
    public List<Transform> gunTips;
    public Transform cam;
    public float maxSwingDistance = 25f; // max distance you're able hit objects for swinging ability
    public float swingSpherecastRadius = 3f;

    private List<SpringJoint> joints; // for swining we use Unitys SpringJoint component
    public float spring = 4.5f; // spring of the SpringJoint component
    public float damper = 7f; // damper of the SpringJoint component
    public float massScale = 4.5f; // massScale of the SpringJoint component

    [Header("Grappling")]
    public float maxGrappleDistance = 25f; // max distance you're able to grapple onto objects
    public float grappleDelayTime = 0.5f; // the time you freeze in the air before grappling
    public float grappleForce = 20f;
    public float grappleUpwardForce = 5f;
    public float grappleDistanceMultiplier = 0.1f; // how much more force you gain when grappling toward objects that are further away

    public float grapplingCd = 2.5f; // cooldown of your grappling ability
    private float grapplingCdTimer;

    public float overshootYAxis = 2f; // adjust the trajectory hight of the player when grappling (only in precise mode)

    public enum GrappleMode
    {
        Basic,
        Precise
    }

    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;
    public KeyCode swingKey2 = KeyCode.Mouse1;

    private Rigidbody rb;

    private List<Vector3> grapplePoints; // the point you're grappling to / swinging on

    private bool tracking;

    private bool grappleExecuted;

    private PlayerMovement_MLab pm;

    [HideInInspector] public List<bool> grapplesActive;
    [HideInInspector] public List<bool> swingsActive;

    [Header("Swinging predictions")]
    public int amountOfSwingPoints = 1;
    public List<Transform> predictionPoints;
    public List<Transform> pointAimers;
    private List<bool> hooksActive;
    private List<RaycastHit> predictionHits;

    public bool debuggingEnabled;

    private void Start()
    {
        // if you don't set whatIsGrappleable to anything, it's automatically set to Default
        if (whatIsGrappleable.value == 0)
            whatIsGrappleable = LayerMask.GetMask("Default");

        // get references
        pm = GetComponent<PlayerMovement_MLab>();
        rb = GetComponent<Rigidbody>();

        ListSetup();
    }

    private void ListSetup()
    {
        hooksActive = new List<bool>();
        predictionHits = new List<RaycastHit>();

        grapplePoints = new List<Vector3>();
        joints = new List<SpringJoint>();

        grapplesActive = new List<bool>();
        swingsActive = new List<bool>();

        for (int i = 0; i < amountOfSwingPoints; i++)
        {
            hooksActive.Add(false);
            predictionHits.Add(new RaycastHit());
            joints.Add(null);
            grapplePoints.Add(Vector3.zero);
            grapplesActive.Add(false);
            swingsActive.Add(false);
        }
    }

    private void Update()
    {
        // cooldown timer
        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;

        // make sure MyInput() is called every frame
        MyInput();

        if (EnableSwingingWithForces && joints[0] != null || joints[1] != null) OdmGearMovement();

        CheckForSwingPoints();
    }

    private void MyInput()
    {
        // stopping is always possible
        if (Input.GetKeyUp(swingKey)) TryStopGrapple(0);
        if (Input.GetKeyUp(swingKey2)) TryStopGrapple(1);
        if (Input.GetKeyUp(swingKey)) StopSwing(0);
        if (Input.GetKeyUp(swingKey2)) StopSwing(1);

        // starting swings or grapples depends on whether or not shift is pressed
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(swingKey)) StartGrapple(0);
            if (Input.GetKeyDown(swingKey2)) StartGrapple(1);
        }
        else
        {
            if (Input.GetKeyDown(swingKey)) StartSwing(0);
            if (Input.GetKeyDown(swingKey2)) StartSwing(1);
        }
    }

    #region Swinging

    private void CheckForSwingPoints()
    {
        for (int i = 0; i < amountOfSwingPoints; i++)
        {
            // active swings don't need new checks
            if (hooksActive[i]) { /* Do Nothing */ }
            else
            {
                RaycastHit hit = predictionHits[i];
                Physics.SphereCast(pointAimers[i].position, swingSpherecastRadius, pointAimers[i].forward, out hit, maxSwingDistance, whatIsGrappleable);

                // check if direct hit is available
                RaycastHit directHit;
                Physics.Raycast(cam.position, cam.forward, out directHit, maxSwingDistance, whatIsGrappleable);

                Vector3 realHitPoint = Vector3.zero;

                // Option 1 - Direct Hit
                if (directHit.point != Vector3.zero)
                    realHitPoint = directHit.point;

                // Option 2 - Indirect (predicted) Hit
                else if (hit.point != Vector3.zero)
                    realHitPoint = hit.point;

                // Option 3 - Miss
                else
                    realHitPoint = Vector3.zero;

                // realHitPoint found
                if (realHitPoint != Vector3.zero)
                {
                    predictionPoints[i].gameObject.SetActive(true);
                    predictionPoints[i].position = realHitPoint;
                }
                // realHitPoint not found
                else
                {
                    predictionPoints[i].gameObject.SetActive(false);
                    predictionPoints[i].position = Vector3.zero;
                }

                print("hit: " + hit.point);

                predictionHits[i] = directHit.point == Vector3.zero ? hit : directHit;
            }
        }
    }

    private Transform grappleObject;
    public void StartSwing(int swingIndex)
    {
        if (!pm.IsStateAllowed(PlayerMovement_MLab.MovementMode.swinging))
            return;

        // no swinging point can be found
        if (!TargetPointFound(swingIndex)) return;

        // cancel all active grapples
        CancelActiveGrapples();
        pm.ResetRestrictions();

        // this will cause the PlayerMovement script to enter MovementMode.swinging
        pm.swinging = true;

        // the grappleObject is the object the raycast hit
        grappleObject = predictionHits[swingIndex].transform;
        tracking = true;

        // the exact point where you swing on
        grapplePoints[swingIndex] = predictionHits[swingIndex].point;

        // add a springJoint component to your player
        joints[swingIndex] = gameObject.AddComponent<SpringJoint>();
        joints[swingIndex].autoConfigureConnectedAnchor = false;

        // set the anchor of the springJoint
        joints[swingIndex].connectedAnchor = grapplePoints[swingIndex];

        // calculate the distance to the grapplePoint
        float distanceFromPoint = Vector3.Distance(transform.position, grapplePoints[swingIndex]);

        // the distance grapple will try to keep from grapple point.
        joints[swingIndex].maxDistance = distanceFromPoint * 0.8f;
        joints[swingIndex].minDistance = distanceFromPoint * 0.25f;

        // adjust these values to fit your game
        joints[swingIndex].spring = spring;
        joints[swingIndex].damper = damper;
        joints[swingIndex].massScale = massScale;

        swingsActive[swingIndex] = true;
        UpdateHooksActive();
    }

    public void StopSwing(int swingIndex)
    {
        pm.swinging = false;
        swingsActive[swingIndex] = false;

        tracking = false;

        UpdateHooksActive();

        // destroy the SpringJoint again after you stopped swinging 
        Destroy(joints[swingIndex]);
    }

    #endregion

    #region Odm Gear

    [Header("OdmGear")]
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;
    private Vector3 pullPoint;
    private void OdmGearMovement()
    {
        if (swingsActive[0] && !swingsActive[1]) pullPoint = grapplePoints[0];
        if (swingsActive[1] && !swingsActive[0]) pullPoint = grapplePoints[1];
        // get midpoint if both swing points are active
        if (swingsActive[0] && swingsActive[1])
        {
            Vector3 dirToGrapplePoint1 = grapplePoints[1] - grapplePoints[0];
            pullPoint = grapplePoints[0] + dirToGrapplePoint1 * 0.5f;
        }

        // right
        if (Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        // left
        if (Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
        // forward
        if (Input.GetKey(KeyCode.W)) rb.AddForce(orientation.forward * forwardThrustForce * Time.deltaTime);
        // backward
        /// if (Input.GetKey(KeyCode.S)) rb.AddForce(-orientation.forward * forwardThrustForce * Time.deltaTime);
        // shorten cable
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = pullPoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            // calculate the distance to the grapplePoint
            float distanceFromPoint = Vector3.Distance(transform.position, pullPoint);

            // the distance grapple will try to keep from grapple point
            UpdateJoints(distanceFromPoint);
        }
        // extend cable
        if (Input.GetKey(KeyCode.S))
        {
            // calculate the distance to the grapplePoint
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, pullPoint) + extendCableSpeed;

            // the distance grapple will try to keep from grapple point
            UpdateJoints(extendedDistanceFromPoint);
        }
    }

    private void UpdateJoints(float distanceFromPoint)
    {
        for (int i = 0; i < joints.Count; i++)
        {
            if (joints[i] != null)
            {
                joints[i].maxDistance = distanceFromPoint * 0.8f;
                joints[i].minDistance = distanceFromPoint * 0.25f;
            }
        }
    }

    #endregion

    /// Here you'll find all of the code specificly needed for the grappling ability
    #region Grappling

    public void StartGrapple(int grappleIndex)
    {
        // in cooldown
        if (grapplingCdTimer > 0) return;

        // cancel active swings and grapples
        CancelActiveSwings();
        CancelAllGrapplesExcept(grappleIndex);

        // Case 1 - target point found
        if (TargetPointFound(grappleIndex))
        {
            print("grapple: target found");

            // set cooldown
            grapplingCdTimer = grapplingCd;

            // this will cause the PlayerMovement script to change to MovemementMode.freeze
            /// -> therefore the player will freeze mid-air for some time before grappling
            pm.freeze = true;

            // same stuff as in StartSwing() function
            grappleObject = predictionHits[grappleIndex].transform;
            tracking = true;

            grapplePoints[grappleIndex] = predictionHits[grappleIndex].point;

            grapplesActive[grappleIndex] = true;
            UpdateHooksActive();

            // call the ExecuteGrapple() function after the grappleDelayTime is over
            StartCoroutine(ExecuteGrapple(grappleIndex));
        }
        // Case 2 - target point not found
        else
        {
            print("grapple: target missed");

            // we still want to freeze the player for a bit
            pm.freeze = true;

            // set cooldown
            grapplingCdTimer = grapplingCd;

            // the grapple point is now just a point in the air
            /// calculated by taking your cameras position + the forwardDirection times your maxGrappleDistance
            grapplePoints[grappleIndex] = cam.position + cam.forward * maxGrappleDistance;

            // call the StopGrapple() function after the grappleDelayTime is over
            StartCoroutine(StopGrapple(grappleIndex, grappleDelayTime));
        }
    }

    public IEnumerator ExecuteGrapple(int grappleIndex)
    {
        yield return new WaitForSeconds(grappleDelayTime);

        // make sure that the player can move again
        pm.freeze = false;

        if(grappleMode == GrappleMode.Precise)
        {
            // find the lowest point of the player
            Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

            // calculate how much higher the grapple point is relative to the player
            float grapplePointRelativeYPos = grapplePoints[grappleIndex].y - lowestPoint.y;
            // calculate the highest y position that the player should reach when grappling
            float highestPointOfArc = grapplePointRelativeYPos + overshootYAxis;

            // no upwards force when point is below player
            if (grapplePointRelativeYPos < 0) highestPointOfArc = overshootYAxis;

            print("trying to grapple to " + grapplePointRelativeYPos + " which arc " + highestPointOfArc);

            pm.JumpToPosition(grapplePoints[grappleIndex], highestPointOfArc, default, 3f);
        }

        if(grappleMode == GrappleMode.Basic)
        {
            // calculate the direction from the player to the grapplePoint
            Vector3 direction = (grapplePoints[grappleIndex] - transform.position).normalized;

            // reset the y velocity of your rigidbody
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // the further the grapple point is away, the higher the distanceBoost should be
            float distanceBoost = Vector3.Distance(transform.position, grapplePoints[grappleIndex]) * grappleDistanceMultiplier;

            // apply force to your rigidbody in the direction towards the grapplePoint
            rb.AddForce(direction * grappleForce, ForceMode.Impulse);
            // also apply upwards force that scales with the distanceBoost
            rb.AddForce(Vector3.up * grappleUpwardForce * distanceBoost, ForceMode.Impulse);
            /// -> make sure to use ForceMode.Impulse because you're only applying force once
        }

        // Stop grapple after a second, (by this time you'll already have travelled most of the distance anyway)
        // StartCoroutine(StopGrapple(grappleIndex, 1f));

        grappleExecuted = true;
    }

    private void TryStopGrapple(int grappleIndex)
    {
        // can't stop grapple if not even executed
        if (!grappleExecuted) return;

        StartCoroutine(StopGrapple(grappleIndex));
    }

    private IEnumerator StopGrapple(int grappleIndex, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        // make sure player can move
        if(pm.freeze) pm.freeze = false;

        pm.ResetRestrictions();

        // reset the grappleExecuted bool
        grappleExecuted = false;

        grapplesActive[grappleIndex] = false;
        UpdateHooksActive();

        print("grapple: stop " + grappleIndex);
    }

    private void CancelActiveGrapples()
    {
        StartCoroutine(StopGrapple(0));
        StartCoroutine(StopGrapple(1));
    }

    private void CancelAllGrapplesExcept(int grappleIndex)
    {
        for (int i = 0; i < amountOfSwingPoints;  i++)
            if (i != grappleIndex) StartCoroutine(StopGrapple(i));
    }

    private void CancelActiveSwings()
    {
        StopSwing(0);
        StopSwing(1);
    }

    private void UpdateHooksActive()
    {
        for (int i = 0; i < grapplePoints.Count; i++)
            hooksActive[i] = grapplesActive[i] || swingsActive[i];
    }

    public void OnObjectTouch()
    {
        if (grappleExecuted)
        {
            print("grapple: objecttouch");
            CancelActiveGrapples();
        }
    }

    #endregion

    #region Tracking Objects

    // Important Note: function currently not being used, I'll implement that soon
    private void TrackObject()
    {
        ///Calculate direction
        Vector3 direction = transform.position - grappleObject.position;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, maxSwingDistance))
        {
            grapplePoints[0] = hit.point;
            joints[0].connectedAnchor = grapplePoints[0];
        }
    }

    #endregion

    #region Getters

    private Vector3 currentGrapplePosition;

    private bool TargetPointFound(int index)
    {
        return predictionHits[index].point != Vector3.zero;
    }

    // a bool to check if we're currently swinging or grappling
    /// function needed and called from the GrapplingRope_MLab script
    public bool IsGrappling(int index)
    {
        return hooksActive[index];
    }

    // a Vetor3 to quickly access the grapple point
    /// function needed and called from the GrapplingRope_MLab script
    public Vector3 GetGrapplePoint(int index)
    {
        return grapplePoints[index];
    }

    public Vector3 GetGunTipPosition(int index)
    {
        return gunTips[index].position;
    }

    #endregion

    /// just to visualize the maxGrappleRange inside of Unity
    #region Gizmos Visualisation

    private void OnDrawGizmosSelected()
    {
        if (!debuggingEnabled) return;

        Gizmos.color = Color.blue;
        if (grapplePoints == null)
        {
            Vector3 direction = (grapplePoints[0] - transform.position).normalized;
            Gizmos.DrawRay(transform.position, direction * maxGrappleDistance);
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < amountOfSwingPoints; i++)
        {
            Gizmos.DrawRay(pointAimers[i].position, pointAimers[i].forward * maxGrappleDistance);
        }
    }

    #endregion
}
