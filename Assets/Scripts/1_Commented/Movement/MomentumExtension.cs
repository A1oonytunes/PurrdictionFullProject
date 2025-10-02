using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MomentumExtension : MonoBehaviour
{
    private PlayerMovement_MLab playerMovement;

    [Header("Increase/Decrease")]
    [Range(0f,10f)] public float momentumIncreaseFactor = 1;
    [Range(0f, 10f)] public float momentumDecreaseFactorOnGround = 2;
    [Range(0f, 10f)] public float momentumDecreaseFactorInAir = 0.5f;

    [Header("Boundaries")]
    [Range(0f, 10f)] public float minimalMomentum = 3f;

    [Header("State Settings")]
    public List<MovementState> movementStates = new List<MovementState>()
    {
        new MovementState("Walking", PlayerMovement_MLab.MovementMode.walking, 1, 1),
        new MovementState("Sprinting",PlayerMovement_MLab.MovementMode.sprinting, 1, 1),
        new MovementState("Crouching", PlayerMovement_MLab.MovementMode.crouching, 1, 1),
        new MovementState("Swinging", PlayerMovement_MLab.MovementMode.swinging, 1, 1),
        new MovementState("Sliding", PlayerMovement_MLab.MovementMode.sliding, 2, 1),
        new MovementState("Wallrunning", PlayerMovement_MLab.MovementMode.wallrunning, 1, 1),
        new MovementState("Walljumping", PlayerMovement_MLab.MovementMode.walljumping, 1, 1),
        new MovementState("Climbing", PlayerMovement_MLab.MovementMode.climbing, 1, 1),
        new MovementState("Dashing", PlayerMovement_MLab.MovementMode.dashing, -1, 10),
    };

    public List<MovementState> hardcodedMovementStates = new List<MovementState>()
    {
        new MovementState("Unlimited", PlayerMovement_MLab.MovementMode.unlimited, -1, -1),
        new MovementState("Limited", PlayerMovement_MLab.MovementMode.limited, -1, 1),
        new MovementState("Freeze", PlayerMovement_MLab.MovementMode.freeze, -1, -1),
    };


    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement_MLab>();
    }

    public MovementState GetMovementState(PlayerMovement_MLab.MovementMode movementMode)
    {
        for (int i = 0; i < movementStates.Count; i++)
        {
            if (movementStates[i].movementMode == movementMode)
                return movementStates[i];
        }

        for (int i = 0; i < hardcodedMovementStates.Count; i++)
        {
            if (hardcodedMovementStates[i].movementMode == movementMode)
                return hardcodedMovementStates[i];
        }

        return null;
    }

    public float GetIncreaseSpeedChangeFactor(PlayerMovement_MLab.MovementMode movementMode)
    {
        float speedChangeFactor = 0f;
        MovementState movementState = GetMovementState(movementMode);

        if (movementState.speedBuildupFactor == -1)
            speedChangeFactor = -1;
        else
            speedChangeFactor = momentumIncreaseFactor * movementState.speedBuildupFactor;

        return speedChangeFactor;
    }

    public float GetDecreaseSpeedChangeFactor(PlayerMovement_MLab.MovementMode movementMode)
    {
        float speedChangeFactor = 0f;

        MovementState movementState = GetMovementState(movementMode);

        if (movementState.speedBuilddownFactor == -1)
            speedChangeFactor = -1;
        else
        {
            speedChangeFactor = movementState.speedBuilddownFactor;
        }

        return speedChangeFactor;
    }

    public float GetSurfaceSpeedDecreaseFactor(bool grounded)
    {
        return grounded ? momentumDecreaseFactorOnGround : momentumDecreaseFactorInAir;
    }

    public bool IsStateAllowed(PlayerMovement_MLab.MovementMode movementMode, float currMomentum)
    {
        MovementState movementState = GetMovementState(movementMode);
        bool stateAllowed = true;

        if(currMomentum < movementState.minNeededMomentum)
            stateAllowed = false;
        else if (currMomentum > movementState.maxAllowedMomentum)
            stateAllowed = false;

        Debug.Log($"CurrMomentum {currMomentum}, MinMomentum {movementState.minNeededMomentum}, MaxMomentum {movementState.maxAllowedMomentum} -> State {movementState.stateName} allowed -> {stateAllowed}");

        return stateAllowed;
    }
}

[Serializable]
public class MovementState
{
    public string stateName;

    public PlayerMovement_MLab.MovementMode movementMode;

    [Tooltip("-1 means instant speed change")]
    public float speedBuildupFactor = -1;
    [Tooltip("-1 means instant speed change")]
    public float speedBuilddownFactor = -1;

    [Range(0f, 100f)]
    public float minNeededMomentum = 0f;
    [Range(0f, 100f)]
    public float maxAllowedMomentum = 100f;

    public MovementState(string stateName, PlayerMovement_MLab.MovementMode movementMode, float speedBuildupFactor, float speedBuilddownFactor)
    {
        this.stateName = stateName;
        this.movementMode = movementMode;
        this.speedBuildupFactor = speedBuildupFactor;
        this.speedBuilddownFactor = speedBuilddownFactor;
    }

    public MovementState(PlayerMovement_MLab.MovementMode movementMode)
    {
        this.movementMode = movementMode;
    }
}