using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this freeFlyCam exists to create clips of your player from all kinds of perspectives

public class FreeFlyCam : MonoBehaviour
{
    public PlayerMovement_MLab pm;
    public PlayerCam_MLab playerCam;

    public float flySpeed;
    public float sensX = 10f;
    public float sensY = 10f;

    private Camera cam;

    public float mouseX;
    public float mouseY;
    private float multiplier = 0.01f;
    private float xRotation;
    private float yRotation;

    private void Start()
    {
        cam = GetComponent<Camera>();
        cam.enabled = false;
    }

    private void Update()
    {
        Movement();
        RotateCamera();

        // enable, disable camera
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            cam.enabled = true;
            pm.enabled = false;
            playerCam.enabled = false;
            FindObjectOfType<Canvas>().enabled = false;
        }
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            cam.enabled = false;
            pm.enabled = true;
            playerCam.enabled = true;
            FindObjectOfType<Canvas>().enabled = true;
        }
    }

    private void Movement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        float inputY = 0f;
        if (Input.GetKey(KeyCode.Space)) inputY = 1f;
        else if (Input.GetKey(KeyCode.LeftShift)) inputY = -1f;

        Vector3 inputDirection = transform.forward * inputZ + transform.right * inputX + transform.up * inputY;

        transform.Translate(inputDirection * flySpeed * Time.deltaTime, Space.World);
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY;

        // calculate rotation
        yRotation += mouseX * sensX * multiplier;
        xRotation -= mouseY * sensY * multiplier;

        xRotation = Mathf.Clamp(xRotation, -89f, 89f);

        // rotate cam
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 10f);
    }
}
