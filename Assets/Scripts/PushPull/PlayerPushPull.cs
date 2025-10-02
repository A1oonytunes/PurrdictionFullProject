using UnityEngine;
using System.Collections.Generic;

public class PlayerPushPull : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private KeyCode pushKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode pullKey = KeyCode.Mouse1;

    [Header("Detection Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask detectionLayerMask = -1;
    [SerializeField] private int coneRayCount = 10; // More rays = smoother cone targeting
    [SerializeField] private float coneAngle = 5f; // How wide the targeting cone is

    [Header("Push Settings")]
    [SerializeField] private float pushMaxRange = 25f;
    [SerializeField] private float pushBaseForce = 600f;
    [SerializeField] private float pushExtraForce = 300f;
    [SerializeField] private AnimationCurve pushDistanceForceMultiplier = AnimationCurve.EaseInOut(0f, 2.5f, 1f, 0.3f);
    [SerializeField] private float pushMaxVelocity = 18f;

    [Header("Pull Settings")]
    [SerializeField] private float pullMaxRange = 20f;
    [SerializeField] private float pullBaseForce = 400f;
    [SerializeField] private float pullExtraForce = 150f;
    [SerializeField] private AnimationCurve pullDistanceForceMultiplier = AnimationCurve.EaseInOut(0f, 1.5f, 1f, 0.8f);
    [SerializeField] private float pullMaxVelocity = 12f;

    [Header("Impulse Settings")]
    [SerializeField] private bool useImpulsePush = true;
    [SerializeField] private bool useImpulsePull = false; // Usually pull is fine as continuous
    [SerializeField] private float pushImpulseInterval = 0.1f; // Apply impulse every 0.1 seconds
    [SerializeField] private float pullImpulseInterval = 0.1f;
    [SerializeField] private float maxUpwardPushRatio = 0.05f; // Maximum 5% of push force can be upward
    [SerializeField] private float maxUpwardPullRatio = 0.1f; // Maximum 10% of pull force can be upward

    [Header("Lock-on Settings")]
    [SerializeField] private bool enableLockOn = true;
    [SerializeField] private float lockOnAngle = 60f;
    [SerializeField] private float pushLockOnBreakDistance = 30f;
    [SerializeField] private float pullLockOnBreakDistance = 25f;

    [Header("Player Movement")]
    [SerializeField] private float playerMaxSpeed = 20f;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLines = true;
    [SerializeField] private Color pushColor = Color.red;
    [SerializeField] private Color pullColor = Color.blue;

    // Private variables
    private Rigidbody playerRigidbody;
    private PushPullObject currentTarget;
    private PushPullObject lastKnownTarget; // To remember the last target
    private bool isPushing = false;
    private bool isPulling = false;
    private Vector3 lastKnownTargetPosition;

    // Impulse timing
    private float lastPushTime = 0f;
    private float lastPullTime = 0f;

    // Debug visualization variables
    private List<Vector3> coneDirections;

    // Events for extensibility
    public System.Action<PushPullObject> OnTargetAcquired;
    public System.Action OnTargetLost;
    public System.Action<PushPullObject, float> OnPush;
    public System.Action<PushPullObject, float> OnPull;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            Debug.LogError("PlayerPushPull requires a Rigidbody component!");
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("No camera assigned and no main camera found!");
            }
        }

        GenerateConeDirections();
    }

    private void GenerateConeDirections()
    {
        coneDirections = new List<Vector3>();
        float alphaRad = coneAngle * Mathf.Deg2Rad;
        float cosAlpha = Mathf.Cos(alphaRad);

        for (int i = 0; i < coneRayCount; i++)
        {
            float u = Random.value;
            float v = Random.value;

            // Sample uniform in spherical cap
            float phi = Mathf.Acos(1f - u * (1f - cosAlpha)); // Polar angle
            float theta = v * 2f * Mathf.PI; // Azimuthal angle

            float x = Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = Mathf.Sin(phi) * Mathf.Sin(theta);
            float z = Mathf.Cos(phi);

            coneDirections.Add(new Vector3(x, y, z).normalized);
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateTarget();

        if (showDebugLines)
        {
            DrawDebugLines();
        }
    }

    private void FixedUpdate()
    {
        if (currentTarget != null)
        {
            if (isPushing)
                ApplyPushForce();
            else if (isPulling)
                ApplyPullForce();
        }

        if (playerRigidbody.linearVelocity.magnitude > playerMaxSpeed)
        {
            playerRigidbody.linearVelocity = playerRigidbody.linearVelocity.normalized * playerMaxSpeed;
        }
    }

    private void HandleInput()
    {
        bool pushInput = Input.GetKey(pushKey);
        bool pullInput = Input.GetKey(pullKey);

        if (pushInput && !isPushing && !isPulling)
        {
            isPushing = true;
        }
        else if (!pushInput && isPushing)
        {
            isPushing = false;
        }

        if (pullInput && !isPulling && !isPushing)
        {
            isPulling = true;
        }
        else if (!pullInput && isPulling)
        {
            isPulling = false;
        }
    }

    // Handles acquiring, releasing, and re-acquiring targets with stability
    private void UpdateTarget()
    {
        bool hasInput = isPushing || isPulling;

        // --- Target Release Logic ---
        if (currentTarget != null)
        {
            // If we release the button OR the target becomes invalid (out of lock-on range/angle), release the lock-on
            if (!hasInput || !IsTargetStillValid(currentTarget))
            {
                OnTargetLost?.Invoke();
                lastKnownTarget = currentTarget; // Remember this target before letting go
                currentTarget = null;            // Break the lock-on
                return; // Early exit to avoid flickering
            }

            // STABLE SWITCHING: While holding input and target is valid, check for a better target in the cone
            if (hasInput)
            {
                PushPullObject betterTarget = FindBetterTargetInCone(currentTarget);
                if (betterTarget != null && betterTarget != currentTarget)
                {
                    OnTargetLost?.Invoke();
                    lastKnownTarget = currentTarget;
                    currentTarget = betterTarget;
                    lastKnownTargetPosition = currentTarget.transform.position;
                    OnTargetAcquired?.Invoke(currentTarget);
                }
            }
        }

        // --- Target Acquisition Logic (only if no current target) ---
        if (hasInput && currentTarget == null)
        {
            // Priority 1: Find a new target directly in our view.
            PushPullObject newTarget = FindTargetInCone();

            if (newTarget != null)
            {
                currentTarget = newTarget;
                lastKnownTargetPosition = currentTarget.transform.position;
                OnTargetAcquired?.Invoke(currentTarget);
            }
            // Priority 2: If no new target, try to re-acquire the last one only if it's in the targeting cone and valid.
            else if (lastKnownTarget != null && IsInTargetingCone(lastKnownTarget) && IsTargetStillValid(lastKnownTarget))
            {
                currentTarget = lastKnownTarget;
                lastKnownTargetPosition = currentTarget.transform.position;
                OnTargetAcquired?.Invoke(currentTarget);
            }
        }
    }

    private bool IsInTargetingCone(PushPullObject target)
    {
        if (target == null) return false;

        Vector3 directionToTarget = (target.transform.position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToTarget);
        return angle <= coneAngle;
    }

    // Finds a better target in the cone compared to the current one (smaller angle to forward)
    private PushPullObject FindBetterTargetInCone(PushPullObject current)
    {
        float maxRange = isPushing ? pushMaxRange : pullMaxRange;

        Vector3 camPos = playerCamera.transform.position;
        Vector3 camForward = playerCamera.transform.forward;

        HashSet<PushPullObject> potentials = new HashSet<PushPullObject>();

        // Helper to cast rays and collect all valid objects along the ray
        System.Action<Vector3> collectHits = (direction) => {
            RaycastHit[] hits = Physics.RaycastAll(camPos, direction, maxRange, detectionLayerMask);
            foreach (RaycastHit hit in hits)
            {
                PushPullObject obj = hit.collider.GetComponent<PushPullObject>();
                if (obj != null && obj.CanBeUsedFrom(transform.position) && !potentials.Contains(obj))
                {
                    potentials.Add(obj);
                }
            }
        };

        // Center ray
        collectHits(camForward);

        // Cone rays
        for (int i = 0; i < coneRayCount; i++)
        {
            if (i >= coneDirections.Count) break;
            Vector3 localDir = coneDirections[i];
            Vector3 worldDir = playerCamera.transform.rotation * localDir;
            collectHits(worldDir);
        }

        if (potentials.Count == 0) return null;

        // Calculate current target's angle
        Vector3 dirToCurrent = (current.transform.position - camPos).normalized;
        float currentAngle = Vector3.Angle(camForward, dirToCurrent);

        // Find the potential with the smallest angle; switch only if strictly better (smaller angle)
        float minAngle = float.MaxValue;
        PushPullObject bestTarget = null;
        foreach (PushPullObject obj in potentials)
        {
            if (obj == current) continue; // Don't consider current as "better"

            Vector3 dirToObj = (obj.transform.position - camPos).normalized;
            float angle = Vector3.Angle(camForward, dirToObj);

            if (angle < minAngle)
            {
                minAngle = angle;
                bestTarget = obj;
            }
        }

        // Switch only if a strictly better (smaller angle) target is found
        return (bestTarget != null && minAngle < currentAngle) ? bestTarget : null;
    }
    
    private PushPullObject FindTargetInCone()
    {
        float maxRange = isPushing ? pushMaxRange : pullMaxRange;

        Vector3 camPos = playerCamera.transform.position;
        Vector3 camForward = playerCamera.transform.forward;

        HashSet<PushPullObject> potentials = new HashSet<PushPullObject>();

        // Helper to cast rays and collect all valid objects along the ray
        System.Action<Vector3> collectHits = (direction) => {
            RaycastHit[] hits = Physics.RaycastAll(camPos, direction, maxRange, detectionLayerMask);
            foreach (RaycastHit hit in hits)
            {
                PushPullObject obj = hit.collider.GetComponent<PushPullObject>();
                if (obj != null && obj.CanBeUsedFrom(transform.position) && !potentials.Contains(obj))
                {
                    potentials.Add(obj);
                }
            }
        };

        // Center ray
        collectHits(camForward);

        // Cone rays
        for (int i = 0; i < coneRayCount; i++)
        {
            if (i >= coneDirections.Count) break;
            Vector3 localDir = coneDirections[i];
            Vector3 worldDir = playerCamera.transform.rotation * localDir;
            collectHits(worldDir);
        }

        if (potentials.Count == 0) return null;

        // Select the target with the smallest angle to the center; for ties, pick the furthest one
        float minAngle = float.MaxValue;
        PushPullObject bestTarget = null;
        float maxDistForTie = 0f;
        foreach (PushPullObject obj in potentials)
        {
            Vector3 dirToObj = (obj.transform.position - camPos).normalized;
            float angle = Vector3.Angle(camForward, dirToObj);
            float dist = Vector3.Distance(transform.position, obj.transform.position);

            if (angle < minAngle || (Mathf.Approximately(angle, minAngle) && dist > maxDistForTie))
            {
                minAngle = angle;
                maxDistForTie = dist;
                bestTarget = obj;
            }
        }

        return bestTarget;
    }

    private bool IsTargetStillValid(PushPullObject target)
    {
        if (target == null) return false;

        float breakDistance = isPushing ? pushLockOnBreakDistance : pullLockOnBreakDistance;
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > breakDistance) return false;

        Vector3 directionToTarget = (target.transform.position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToTarget);
        return angle <= lockOnAngle;
    }

    private void ApplyPushForce()
    {
        if (currentTarget == null) return;

        Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        float normalizedDistance = Mathf.Clamp01(distance / pushMaxRange);
        float forceMultiplier = pushDistanceForceMultiplier.Evaluate(normalizedDistance);
        float totalForce = Mathf.Min((pushBaseForce + pushExtraForce) * forceMultiplier, currentTarget.maxForce);

        Vector3 pushForce = directionToTarget * totalForce;

        if (useImpulsePush)
        {
            // Apply impulses at intervals instead of continuous force
            if (Time.fixedTime - lastPushTime >= pushImpulseInterval)
            {
                // Limit upward component
                if (pushForce.y > 0)
                {
                    float maxUpwardForce = totalForce * maxUpwardPushRatio;
                    pushForce.y = Mathf.Min(pushForce.y, maxUpwardForce);
                }
                
                // Scale force by interval to maintain consistent strength
                currentTarget.ApplyForce(pushForce * (pushImpulseInterval * 10f), playerRigidbody);
                lastPushTime = Time.fixedTime;
                OnPush?.Invoke(currentTarget, totalForce);
            }
        }
        else
        {
            // Original continuous force approach with vertical limitation
            if (pushForce.y > 0)
            {
                float maxUpwardForce = totalForce * maxUpwardPushRatio;
                pushForce.y = Mathf.Min(pushForce.y, maxUpwardForce);
            }
            
            currentTarget.ApplyForce(pushForce, playerRigidbody);
            OnPush?.Invoke(currentTarget, totalForce);
        }
    }

    private void ApplyPullForce()
    {
        if (!currentTarget) return;

        Vector3 directionToPlayer = (transform.position - currentTarget.transform.position).normalized;
        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        float normalizedDistance = Mathf.Clamp01(distance / pullMaxRange);
        float forceMultiplier = pullDistanceForceMultiplier.Evaluate(normalizedDistance);
        float totalForce = Mathf.Min((pullBaseForce + pullExtraForce) * forceMultiplier, currentTarget.maxForce);

        Vector3 pullForce = directionToPlayer * totalForce;

        if (useImpulsePull)
        {
            // Apply impulses at intervals instead of continuous force
            if (Time.fixedTime - lastPullTime >= pullImpulseInterval)
            {
                // Limit upward component
                if (pullForce.y > 0)
                {
                    float maxUpwardForce = totalForce * maxUpwardPullRatio;
                    pullForce.y = Mathf.Min(pullForce.y, maxUpwardForce);
                }
                
                // Scale force by interval to maintain consistent strength
                currentTarget.ApplyForce(pullForce * (pullImpulseInterval * 10f), playerRigidbody);
                lastPullTime = Time.fixedTime;
                OnPull?.Invoke(currentTarget, totalForce);
            }
        }
        else
        {
            // Original continuous force approach with vertical limitation
            if (pullForce.y > 0)
            {
                float maxUpwardForce = totalForce * maxUpwardPullRatio;
                pullForce.y = Mathf.Min(pullForce.y, maxUpwardForce);
            }
            
            currentTarget.ApplyForce(pullForce, playerRigidbody);
            OnPull?.Invoke(currentTarget, totalForce);
        }
    }

    private void DrawDebugLines()
    {
        if (!playerCamera || coneDirections == null) return;

        float maxRange = isPushing ? pushMaxRange : pullMaxRange;
        Vector3 camPos = playerCamera.transform.position;
        Vector3 camForward = playerCamera.transform.forward;

        // Draw the center ray (Scene view)
        Debug.DrawRay(camPos, camForward * maxRange, Color.green * 0.5f);

        // Draw cone rays for visualization (Scene view)
        for (int i = 0; i < coneRayCount && i < coneDirections.Count; i++)
        {
            Vector3 localDir = coneDirections[i];
            Vector3 worldDir = playerCamera.transform.rotation * localDir;
            Debug.DrawRay(camPos, worldDir * maxRange, Color.yellow * 0.5f);
        }

        // Draw line to current target, if any (Scene view)
        if (currentTarget != null)
        {
            Color lineColor = isPushing ? pushColor : pullColor;
            Debug.DrawLine(transform.position, currentTarget.transform.position, lineColor);
        }
    }

    // Public getters
    public bool HasTarget => currentTarget != null;
    public PushPullObject CurrentTarget => currentTarget;
    public bool IsPushing => isPushing;
    public bool IsPulling => isPulling;
    public float CurrentMaxRange => isPushing ? pushMaxRange : (isPulling ? pullMaxRange : pushMaxRange);

    public void ForceReleaseTarget()
    {
        isPushing = false;
        isPulling = false;
        if (currentTarget != null)
        {
            OnTargetLost?.Invoke();
        }
        currentTarget = null;
        lastKnownTarget = null; // Also clear the last known target on a forced release
    }
}