using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum ObjectType
{
    Coin,           // Moveable object
    Anchor,         // Fixed object that moves the player
    Environmental   // Other interactive objects
}

public class PushPullObject : MonoBehaviour
{
    [Header("Object Properties")]
    public ObjectType objectType = ObjectType.Coin;
    public bool isAnchor = false;
    [SerializeField] private float _resistance = 1f;
    [SerializeField] private float _maxForce = 1000f;
    
    [Header("Physics Settings")]
    [SerializeField] private bool respectGrounding = true;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayerMask = 1; // Default layer
    [SerializeField] private float airborneForceMultiplier = 1.2f;
    [SerializeField] private float maxSpeed = 15f; // Velocity clamping
    
    [Header("Audio/Visual Feedback")]
    [SerializeField] private AudioClip pushSound;
    [SerializeField] private AudioClip pullSound;
    [SerializeField] private ParticleSystem forceEffect;
    
    [Header("Special Behaviors")]
    [SerializeField] private bool consumeOnUse = false; // For coins that disappear
    [SerializeField] private float maxUsageDistance = 30f;
    [SerializeField] private bool requiresLineOfSight = true;
    
    [Header("Visual Settings")]
    [SerializeField] private Color groundedColor = Color.green;
    [SerializeField] private Color airborneColor = Color.red;
    
    [Header("Object Type Colors")]
    [SerializeField] private Color coinColor = Color.yellow;
    [SerializeField] private Color anchorColor = Color.blue;
    [SerializeField] private Color environmentalColor = Color.gray;

    // Private components and variables
    private Rigidbody objectRigidbody;
    private AudioSource audioSource;
    private MeshRenderer meshRenderer;
    private bool isGrounded = true;
    private Vector3 lastForceDirection;
    private float lastForceAmount;
    private Color originalColor;
    
    // Events for extensibility
    public System.Action<Vector3, float> OnForceApplied;
    public System.Action<PushPullObject> OnObjectConsumed;
    public System.Action<bool> OnGroundedStateChanged;
    
    // Properties with validation
    public float resistance
    {
        get => _resistance;
        set => _resistance = Mathf.Max(0.1f, value);
    }
    
    public float maxForce
    {
        get => _maxForce;
        set => _maxForce = Mathf.Max(1f, value);
    }
    
    private void Awake()
    {
        InitializeComponents();
        ValidateConfiguration();
        SetObjectTypeColor();
    }
    
    private void InitializeComponents()
    {
        objectRigidbody = GetComponent<Rigidbody>();
        if (objectRigidbody == null && !isAnchor)
        {
            Debug.LogWarning($"PushPullObject '{gameObject.name}' has no Rigidbody but is not marked as anchor. Adding Rigidbody.");
            objectRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (pushSound != null || pullSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        meshRenderer = GetComponent<MeshRenderer>();
    }
    
    private void ValidateConfiguration()
    {
        // Auto-set anchor based on object type
        if (objectType == ObjectType.Anchor)
        {
            isAnchor = true;
        }
        
        // Ensure anchors don't have rigidbodies or make them kinematic
        if (isAnchor && objectRigidbody != null)
        {
            objectRigidbody.isKinematic = true;
        }
        
        // Validate resistance and maxForce
        resistance = _resistance;
        maxForce = _maxForce;
    }
    
    private void SetObjectTypeColor()
    {
        if (meshRenderer != null)
        {
            Color typeColor = objectType switch
            {
                ObjectType.Coin => coinColor,
                ObjectType.Anchor => anchorColor,
                ObjectType.Environmental => environmentalColor,
                _ => Color.white
            };
            
            originalColor = typeColor;
            meshRenderer.material.color = typeColor;
        }
    }
    
    private void FixedUpdate()
    {
        if (respectGrounding)
        {
            CheckGroundedState();
        }
        
        // Apply velocity clamping for moveable objects
        if (!isAnchor && objectRigidbody != null && objectRigidbody.linearVelocity.magnitude > maxSpeed)
        {
            objectRigidbody.linearVelocity = objectRigidbody.linearVelocity.normalized * maxSpeed;
        }
    }
    
    private void CheckGroundedState()
    {
        bool wasGrounded = isGrounded;
        
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            isGrounded = false;
        }
        else if (col is BoxCollider boxCol)
        {
            Vector3 localCenter = boxCol.center;
            localCenter.y -= (boxCol.size.y / 2f) + (groundCheckDistance / 2f);
            Vector3 checkCenter = transform.TransformPoint(localCenter);
            Vector3 halfExtents = boxCol.size / 2f;
            halfExtents.y = groundCheckDistance / 2f;
            Quaternion orientation = transform.rotation;
            isGrounded = Physics.CheckBox(checkCenter, halfExtents, orientation, groundLayerMask);
        }
        else
        {
            // Fallback to raycast for non-box colliders
            Bounds bounds = col.bounds;
            Vector3 rayOrigin = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundLayerMask);
        }
        
        // Trigger event if grounded state changed
        if (wasGrounded != isGrounded)
        {
            OnGroundedStateChanged?.Invoke(isGrounded);
            if (meshRenderer != null)
            {
                // Use grounded/airborne colors when respectGrounding is enabled
                // Otherwise use the object type color
                if (respectGrounding)
                {
                    meshRenderer.material.color = isGrounded ? groundedColor : airborneColor;
                }
                else
                {
                    meshRenderer.material.color = originalColor;
                }
            }
        }
    }
    
    public void ApplyForce(Vector3 force, Rigidbody playerRigidbody)
    {
        // Store force information for feedback
        lastForceDirection = force.normalized;
        lastForceAmount = force.magnitude;
        
        // Apply grounding modifier
        if (respectGrounding && !isGrounded)
        {
            force *= airborneForceMultiplier;
        }
        
        // Determine if this is a push or pull
        Vector3 playerPos = playerRigidbody != null ? playerRigidbody.position : transform.position;
        Vector3 dirToTarget = (transform.position - playerPos).normalized;
        bool isPush = Vector3.Dot(force.normalized, dirToTarget) > 0;
        
        // Determine if we should act as anchor
        bool actAsAnchor = isAnchor || (objectType == ObjectType.Coin && isGrounded && isPush);
        
        
        // Apply force based on object type using ForceMode.Acceleration for smooth motion
        if (actAsAnchor)
        {
            // For anchors, apply force to the player instead
            if (playerRigidbody != null)
            {
                // Invert the force direction for logical push/pull behavior
                Vector3 playerForce = -force / resistance;
                playerRigidbody.AddForce(playerForce, ForceMode.Acceleration);
            }
        }
        else
        {
            // For moveable objects, apply force to this object and player
            if (objectRigidbody && playerRigidbody)
            {
                Vector3 adjustedForce = force / resistance;
                objectRigidbody.AddForce(adjustedForce, ForceMode.Acceleration);
                playerRigidbody.AddForce(-adjustedForce, ForceMode.Acceleration);
            }
        }
        
        // Trigger effects and events
        PlayForceEffects(force);
        OnForceApplied?.Invoke(force, lastForceAmount);
        
        // Handle consumption (for coins)
        if (consumeOnUse)
        {
            ConsumeObject();
        }
    }
    
    private void PlayForceEffects(Vector3 force)
    {
        // Play audio
        if (audioSource != null)
        {
            AudioClip clipToPlay = force.magnitude > 0 ? 
                (Vector3.Dot(force.normalized, transform.forward) > 0 ? pushSound : pullSound) : null;
            
            if (clipToPlay != null)
            {
                audioSource.pitch = Random.Range(0.8f, 1.2f); // Add some variation
                audioSource.PlayOneShot(clipToPlay);
            }
        }
        
        // Play particle effects
        if (forceEffect != null)
        {
            // Orient particles based on force direction
            forceEffect.transform.rotation = Quaternion.LookRotation(-force.normalized);
            forceEffect.Play();
        }
    }
    
    private void ConsumeObject()
    {
        OnObjectConsumed?.Invoke(this);
        
        // Add some visual flair before destroying
        if (forceEffect != null)
        {
            // Detach the particle system so it can finish playing
            forceEffect.transform.SetParent(null);
            forceEffect.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            Destroy(forceEffect.gameObject, 2f);
        }
        
        // Destroy the object
        Destroy(gameObject);
    }
    
    public bool CanBeUsedFrom(Vector3 playerPosition)
    {
        // Check distance
        float distance = Vector3.Distance(transform.position, playerPosition);
        if (distance > maxUsageDistance)
        {
            return false;
        }
        
        // Check line of sight if required
        if (requiresLineOfSight)
        {
            Vector3 direction = (transform.position - playerPosition).normalized;
            if (Physics.Raycast(playerPosition, direction, out RaycastHit hit, distance))
            {
                // If we hit this object first, line of sight is clear
                return hit.collider.gameObject == gameObject;
            }
        }
        
        return true;
    }
    
    public bool IsGrounded()
    {
        return isGrounded;
    }
    
    public Vector3 GetLastForceDirection()
    {
        return lastForceDirection;
    }
    
    public float GetLastForceAmount()
    {
        return lastForceAmount;
    }
    
    // Method to change object type and update color
    public void SetObjectType(ObjectType newType)
    {
        objectType = newType;
        ValidateConfiguration();
        SetObjectTypeColor();
    }
    
    // Static factory methods for easy setup
    public static PushPullObject SetupAsCoin(GameObject go, float resistance = 1f, float maxForce = 800f)
    {
        PushPullObject ppo = go.GetComponent<PushPullObject>() ?? go.AddComponent<PushPullObject>();
        ppo.objectType = ObjectType.Coin;
        ppo.isAnchor = false;
        ppo.resistance = resistance;
        ppo.maxForce = maxForce;
        ppo.SetObjectTypeColor();
        return ppo;
    }
    
    public static PushPullObject SetupAsAnchor(GameObject go, float maxForce = 1200f)
    {
        PushPullObject ppo = go.GetComponent<PushPullObject>() ?? go.AddComponent<PushPullObject>();
        ppo.objectType = ObjectType.Anchor;
        ppo.isAnchor = true;
        ppo.resistance = 1f; // Anchors don't need resistance since they don't move
        ppo.maxForce = maxForce;
        ppo.SetObjectTypeColor();
        return ppo;
    }
    
    public static PushPullObject SetupAsEnvironmental(GameObject go, float resistance = 2f, float maxForce = 600f)
    {
        PushPullObject ppo = go.GetComponent<PushPullObject>() ?? go.AddComponent<PushPullObject>();
        ppo.objectType = ObjectType.Environmental;
        ppo.isAnchor = false;
        ppo.resistance = resistance;
        ppo.maxForce = maxForce;
        ppo.SetObjectTypeColor();
        return ppo;
    }
    
    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        // Draw max usage distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxUsageDistance);
        
        // Draw ground check
        if (respectGrounding)
        {
            Collider col = GetComponent<Collider>();
            if (col is BoxCollider boxCol)
            {
                Vector3 localCenter = boxCol.center;
                localCenter.y -= (boxCol.size.y / 2f) + (groundCheckDistance / 2f);
                Vector3 checkCenter = transform.TransformPoint(localCenter);
                Vector3 halfExtents = boxCol.size / 2f;
                halfExtents.y = groundCheckDistance / 2f;
                Quaternion orientation = transform.rotation;
                Gizmos.matrix = Matrix4x4.TRS(checkCenter, orientation, halfExtents * 2f);
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                Gizmos.matrix = Matrix4x4.identity;
            }
            else
            {
                Bounds bounds = col != null ? col.bounds : new Bounds(transform.position, Vector3.one);
                Vector3 groundCheckStart = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawRay(groundCheckStart, Vector3.down * groundCheckDistance);
            }
        }
        
        // Draw last force direction
        if (lastForceAmount > 0)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, lastForceDirection * (lastForceAmount / 100f));
        }
    }
}