using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Dave MovementLab - MovingObject
///
// Content:
/// - moveObjects from position A to B and back
/// - rotate objects over time
/// 
// Note:
/// This code is being used to create the flying-block-parcour in the MovementLab Scene
/// Feel free to use it to create your own cool parcours with moving blocks


public class MovingObject_MLab : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;

    [Header("Settings")]
    public MovementMode movementMode;
    public enum MovementMode
    {
        Transform,
        Rigidbody
    }

    [Header("Timing")]
    public float startDelay; // for example: if set to 1f, the object will start moving 1 second after the start of the game

    [Header("Moving")]
    public bool movementEnabled = true;
    public Vector3 startPos;
    public Vector3 endPos;
    public float moveTime; // how long it takes the object to move from the startPos to the endPos
    private float distanceThreshold = 0.1f; // don't change

    private float moveSpeed; // how fast the object moves (don't change, it's being calculated using the moveTime variable)
    private bool movingToEnd = true; // if the object is moving to the end or to the start

    [Header("Rotating")]
    public bool rotationEnabled = false;
    public Vector3 rotation; // the rotation that is being applied every second

    private bool started;

    private void Start()
    {
        // at the beginning the object should move to the endPos
        movingToEnd = true;

        // calculate start and endPos
        startPos = transform.position + startPos;
        endPos = transform.position + endPos;

        // calculate the distance between start and end pos
        float fullDistance = Vector3.Distance(startPos, endPos);

        // calculate how fast your objet needs to move in order to reach the target in the desired moveTime
        moveSpeed = fullDistance / moveTime;

        // start the movement once the startDelay is over
        Invoke(nameof(StartMovement), startDelay);
    }

    private void Update()
    {
        // if the movement hasn't started, just return (stop) the function
        if (!started) return;

        if (movementEnabled) MoveObject();
        if (rotationEnabled) RotateObject();
    }

    #region Moving & Rotating

    private void StartMovement()
    {
        started = true;
    }

    private void MoveObject()
    {
        // if moving to end, calculate the direction from the current position to the endPos
        // if not moving to end, calculate the direction from the current position to the startPos
        Vector3 direction = movingToEnd ? endPos - transform.position : startPos - transform.position;

        // distance to end/startPos
        float distance = direction.magnitude;

        if(movementMode == MovementMode.Transform || rb == null)
        {
            // move the object in the correct direction multiplied by your moveSpeed (over time)
            transform.Translate(direction.normalized * moveSpeed * Time.deltaTime);
        }
        else if(movementMode == MovementMode.Rigidbody)
        {
            rb.linearVelocity = direction.normalized * moveSpeed * Time.deltaTime * 100;
        }

        // if the distance is smaller than the distanceThreshold (0.1f), the target has been reached
        // change movingToEnd to !movingToEnd or the other way around
        /// -> this will cause the object to switch targets and move back to the other position
        if (distance <= distanceThreshold)
            movingToEnd = !movingToEnd;
    }

    private void RotateObject()
    {
        // rotate object over time
        transform.Rotate(rotation * Time.deltaTime);
    }

    #endregion

    #region PlayerMovement Handling

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            collision.transform.SetParent(transform);
            collision.transform.GetComponent<PlayerMovement_MLab>().AssignPlatform(rb);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.transform.SetParent(null);
            collision.transform.GetComponent<PlayerMovement_MLab>().UnassignPlatform();
        }
    }

    #endregion

    /// This code visualizes in Unity from where to where the object is going to move
    #region Gizmo Visualisation

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        if(!started)
            Gizmos.DrawLine(transform.position + startPos, transform.position + endPos);

        else
            Gizmos.DrawLine(startPos, endPos);
    }

    #endregion
}