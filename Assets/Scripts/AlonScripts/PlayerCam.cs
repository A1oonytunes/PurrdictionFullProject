using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;
    
    public Transform orientation;

    public InputActionMap inputActionMap;

    private InputAction _mLookAction;
    private Vector2 _mLookAmt;
    
    private float _xRotation;
    private float _yRotation;

    //Enable and disable the player input map
    private void OnEnable()
    {
        inputActionMap.FindAction("Player").Enable();
    }

    private void OnDisable()
    {
        inputActionMap.FindAction("Player").Disable();
    }
    
    //set up the look action
    private void Awake()
    {
        _mLookAction = InputSystem.actions.FindAction("Look");
    }

    
    //lock + hide cursor on start
    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
    //apply rotation to camera
    void Update()
    {
        _mLookAmt =  _mLookAction.ReadValue<Vector2>();
        float mouseX = _mLookAmt.x * Time.deltaTime * sensX;
        float mouseY = _mLookAmt.y*Time.deltaTime * sensY;;
        
        _yRotation += mouseX;
        _xRotation -= mouseY;
        
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, _yRotation, 0);
        
    }

   
}
