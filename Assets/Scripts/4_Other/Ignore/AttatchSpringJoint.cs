using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttatchSpringJoint : MonoBehaviour
{
    public Transform player;
    public Rigidbody rb;
    public Transform grappleObj;
    public LineRenderer lr;

    Vector3 grapplePoint;

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        lr.positionCount = 2;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, grapplePoint);

        if (Input.GetKeyDown(KeyCode.P)) AttachJoint();

        Vector3 lookAt = player.position + rb.linearVelocity;
        player.LookAt(new Vector3(lookAt.x, player.position.y, lookAt.z));
    }

    private void AttachJoint()
    {
        GetComponent<Rigidbody>().useGravity = true;
        lr.enabled = true;

        grapplePoint = grappleObj.transform.position;
        SpringJoint joint;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = grapplePoint;

        float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

        //The distance grapple will try to keep from grapple point. 
        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        //Adjust these values to fit your game.
        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;
    }
}
