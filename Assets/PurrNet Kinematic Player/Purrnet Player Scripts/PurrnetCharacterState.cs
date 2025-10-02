using System;
using PurrNet.Prediction;
using UnityEngine;

public enum CharacterStance
{
    Standing,
    Crouching,
    Sliding,
    WallRunning
}

[Serializable]
public struct PurrnetCharacterState : IPredictedData<PurrnetCharacterState>
{
    // Transform data (critical for CSP)
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Velocity;
    
    // Movement state
    public bool IsGrounded;
    public CharacterStance Stance;
    public float CurrentHeight;
    
    // Jump state
    public int JumpsUsed;
    public float TimeSinceUngrounded;
    public float TimeSinceJumpRequest;
    public bool UngroundedDueToJump;
    
    // Wall mechanics
    public bool IsWallOnLeft;
    public bool IsWallOnRight;
    public Vector3 WallNormal;
    public float WallRunTime;
    public float TimeSinceLeftWall;
    
    // Cooldown timers
    public float JumpCooldownTimer;
    public float WallJumpLockoutTimer;
    
    // Sliding
    public bool IsSliding;
    public float SlideSpeed;
    
    public void Dispose() { }
}