using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject_MLab_Uncommented : MonoBehaviour
{
    [Header("Timing")]
    public float startDelay;

    [Header("Moving")]
    public bool movementEnabled = true;
    public Vector3 startPos;
    public Vector3 endPos;
    public float moveTime;
    public float distanceThreshold = 0.1f;

    private float moveSpeed;
    private bool movingToEnd = true;

    [Header("Rotating")]
    public bool rotationEnabled = false;
    public Vector3 rotation;

    private bool started;

    private void Start()
    {
        movingToEnd = true;

        startPos = transform.position + startPos;
        endPos = transform.position + endPos;

        float fullDistance = (endPos - startPos).magnitude;
        moveSpeed = fullDistance / moveTime;

        Invoke(nameof(StartMovement), startDelay);
    }

    private void Update()
    {
        if (!started) return;

        if (movementEnabled) MoveObject();
        if (rotationEnabled) RotateObject();
    }

    private void StartMovement()
    {
        started = true;
    }

    private void MoveObject()
    {
        Vector3 direction = movingToEnd ? endPos - transform.position : startPos - transform.position;

        float distance = direction.magnitude;

        transform.Translate(direction.normalized * moveSpeed * Time.deltaTime);

        if (distance <= distanceThreshold)
            movingToEnd = !movingToEnd;
    }

    private void RotateObject()
    {
        transform.Rotate(rotation * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        if(!started)
            Gizmos.DrawLine(transform.position + startPos, transform.position + endPos);

        else
            Gizmos.DrawLine(startPos, endPos);
    }
}