using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling_MLab_Uncommented : MonoBehaviour
{
    [Header("Swinging")]
    public LayerMask whatIsGrappleable;
    public Transform gunTip, cam;
    public float maxDistance;

    [Header("Grappling")]
    public float maxGrappleDistance = 25f;
    public float grappleDelayTime = 0.5f;
    public float grappleForce = 20f;
    public float grappleUpwardForce = 5f;
    public float grappleDistanceMultiplier = 0.1f;

    public float grapplingCd = 2.5f;
    private float grapplingCdTimer;

    private SpringJoint joint;
    public float spring = 4.5f;
    public float damper = 7f;
    public float massScale = 4.5f;

    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;
    public KeyCode grappleKey = KeyCode.Mouse1;

    //public LineRenderer lr;
    private Rigidbody rb;

    private Vector3 grapplePoint;

    //Fixing the problem of joint staying after gun switch
    private bool tracking;

    private bool grappleExecuted;

    private PlayerMovement_MLab_Uncommented pm;

    private void Start()
    {
        if (whatIsGrappleable.value == 0)
            whatIsGrappleable = LayerMask.GetMask("Default");

        pm = GetComponent<PlayerMovement_MLab_Uncommented>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // cooldown timer
        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;

        MyInput();
    }

    private void MyInput()
    {
        if (Input.GetKeyDown(swingKey)) StartSwing();
        if (Input.GetKeyUp(swingKey)) StopSwing();

        if (Input.GetKeyDown(grappleKey)) StartGrapple();
        if (Input.GetKeyUp(grappleKey)) TryStopGrapple();
    }

    private Transform grappleObject;
    public void StartSwing()
    {
        pm.swinging = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxDistance, whatIsGrappleable))
        {
            grappleObject = hit.transform;
            Debug.Log(hit.transform.name);
            tracking = true;

            grapplePoint = hit.point;
            joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

            //The distance grapple will try to keep from grapple point. 
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            //Adjust these values to fit your game.
            joint.spring = spring;
            joint.damper = damper;
            joint.massScale = massScale;
        }
    }

    public void StopSwing()
    {
        pm.swinging = false;

        tracking = false;
        Destroy(joint);
    }
    private void TrackObject()
    {
        //Use a ray cast to track the object, then get the hit.point
        //You can't just use grappleObject.position, because that's too inacurate :D

        ///Calculate direction
        Vector3 direction = transform.position - grappleObject.position;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, maxDistance))
        {
            grapplePoint = hit.point;
            joint.connectedAnchor = grapplePoint;
        }
    }

    private Vector3 currentGrapplePosition;

    public bool IsGrappling()
    {
        return joint != null || pm.grappling;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }

    #region Grappling

    public void StartGrapple()
    {
        // in cooldown
        if (grapplingCdTimer > 0) return;

        pm.grappling = true;

        // set cooldown
        grapplingCdTimer = grapplingCd;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            pm.freeze = true;

            grappleObject = hit.transform;
            Debug.Log(hit.transform.name);
            tracking = true;

            grapplePoint = hit.point;

            float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

            Invoke(nameof(ExcectueGrapple), grappleDelayTime);
        }
        else
        {
            pm.freeze = true;

            grapplePoint = cam.position + cam.forward * maxGrappleDistance;

            Invoke(nameof(StopGrapple), grappleDelayTime);
        }
    }

    public void ExcectueGrapple()
    {
        pm.freeze = false;
        pm.grappling = true;

        Vector3 direction = (grapplePoint - transform.position).normalized;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float distanceBoost = Vector3.Distance(transform.position, grapplePoint) * grappleDistanceMultiplier;

        rb.AddForce(direction * grappleForce, ForceMode.Impulse);
        rb.AddForce(Vector3.up * grappleUpwardForce * distanceBoost, ForceMode.Impulse);

        Invoke(nameof(StopGrapple), 1f);

        grappleExecuted = true;
    }

    /// called on right click release
    public void TryStopGrapple()
    {
        if (!grappleExecuted) return;

        StopGrapple();
    }

    private void StopGrapple()
    {
        pm.freeze = false;

        pm.grappling = false;

        rb.useGravity = true;

        grappleExecuted = false;
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 direction = (grapplePoint - transform.position).normalized;
        Gizmos.DrawRay(transform.position, direction * maxGrappleDistance);
    }
}
