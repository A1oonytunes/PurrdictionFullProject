using System;
using PurrNet.Prediction;
using UnityEngine;

[Serializable]
public struct PurrnetCharacterInput : IPredictedData
{
    // Movement input
    public Vector2 MoveInput;
    public Quaternion LookRotation;
    
    // Action inputs
    public bool Jump;
    public bool JumpHeld;
    public bool CrouchToggle;
    public bool Sprint;
    
    public void Dispose() { }
}