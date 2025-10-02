using UnityEngine;
using TMPro;

public class PurrnetSimpleHUD : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private TMP_Text stanceText;
    [SerializeField] private TMP_Text velocityText;
    [SerializeField] private TMP_Text requestingToCrouchText;
    [SerializeField] private TMP_Text canWallJumpText;   
    [SerializeField] private TMP_Text canDoubleJumpText;
    
    // Wall run debug texts
    [SerializeField] private TMP_Text isTouchingWallText;
    [SerializeField] private TMP_Text isGroundedText;
    [SerializeField] private TMP_Text wallNormalText;
    [SerializeField] private TMP_Text movementInputText;
    [SerializeField] private TMP_Text pushingTowardWallText; // Repurposed for Movement Intent
    [SerializeField] private TMP_Text wallAlignmentText;     // Repurposed for Jump Count

    void Start()
    {
        // This helper function reduces the repetition from your original Start method
        SetupTextElement(stanceText, new Vector2(10, 200));
        SetupTextElement(velocityText, new Vector2(10, 170));
        SetupTextElement(isGroundedText, new Vector2(10, 140));
        SetupTextElement(isTouchingWallText, new Vector2(10, 110)); // Will now show L/R spherecast results
        SetupTextElement(wallNormalText, new Vector2(10, 80));
        SetupTextElement(movementInputText, new Vector2(10, 50));
        SetupTextElement(pushingTowardWallText, new Vector2(10, 20)); // Repurposed
        SetupTextElement(wallAlignmentText, new Vector2(10, -10));     // Repurposed
        
        SetupTextElement(canWallJumpText, new Vector2(-10, 80), TextAlignmentOptions.BottomRight, new Vector2(1,0));
        SetupTextElement(canDoubleJumpText, new Vector2(-10, 45), TextAlignmentOptions.BottomRight, new Vector2(1,0));
        SetupTextElement(requestingToCrouchText, new Vector2(-10, 10), TextAlignmentOptions.BottomRight, new Vector2(1,0));
    }

    // Helper to avoid repeating the same setup code for each text element
    private void SetupTextElement(TMP_Text text, Vector2 anchoredPosition, TextAlignmentOptions alignment = TextAlignmentOptions.BottomLeft, Vector2? anchor = null)
    {
        Vector2 anchorPos = anchor ?? new Vector2(0, 0);
        text.rectTransform.anchorMin = anchorPos;
        text.rectTransform.anchorMax = anchorPos;
        text.rectTransform.pivot = anchorPos;
        text.rectTransform.anchoredPosition = anchoredPosition;
        text.enableWordWrapping = false;
        text.alignment = alignment;
        text.rectTransform.sizeDelta = new Vector2(500, 100);
    }

    void Update()
    {
        var state = playerCharacter.GetState();
        stanceText.text = $"Stance: {state.Stance}";
        velocityText.text = $"Velocity: {state.Velocity.magnitude:F1}";
        requestingToCrouchText.text = $"requestedToCrouch: {playerCharacter.GetRequestingToCrouch()}";

        // Update jump info with color coding
        canWallJumpText.text = $"Can Wall Jump: {(playerCharacter.CanWallJump ? "<color=green>TRUE</color>" : "<color=red>FALSE</color>")}";
        canDoubleJumpText.text = $"Can Double Jump: {(playerCharacter.CanDoubleJump ? "<color=green>TRUE</color>" : "<color=red>FALSE</color>")}";

        // Ground and Wall Detection
        isGroundedText.text = $"Grounded: {(state.Grounded ? "<color=green>TRUE</color>" : "<color=red>FALSE</color>")}";

        bool wallOnLeft = playerCharacter.IsWallOnLeft;
        bool wallOnRight = playerCharacter.IsWallOnRight;
        isTouchingWallText.text = $"Wall Detect: L:{(wallOnLeft ? "<color=green>T</color>" : "<color=red>F</color>")} | R:{(wallOnRight ? "<color=green>T</color>" : "<color=red>F</color>")}";
        
        Vector3 normal = playerCharacter.GetWallNormal();
        if (normal != Vector3.zero)
        {
            wallNormalText.text = $"Wall Normal: ({normal.x:F2}, {normal.y:F2}, {normal.z:F2})";
        }
        else
        {
            wallNormalText.text = "Wall Normal: <color=gray>N/A</color>";
        }

        // Movement Input and Intent
        Vector3 movementInput = playerCharacter.GetRequestedMovement();
        movementInputText.text = $"Movement Input: ({movementInput.x:F2}, {movementInput.z:F2})";

        if (!state.Grounded)
        {
            float horizontalInputDot = Vector3.Dot(movementInput, playerCharacter.transform.right);
            string intent = "Forward";
            if (horizontalInputDot < -0.1f) intent = "<color=orange>Left</color>";
            if (horizontalInputDot > 0.1f) intent = "<color=cyan>Right</color>";
            pushingTowardWallText.text = $"Movement Intent: {intent}"; // REPURPOSED

            int jumps = playerCharacter.NumJumpsUsed;
            int maxJumps = playerCharacter.MaxJumps;
            wallAlignmentText.text = $"Jumps Used: {jumps} / {maxJumps}"; // REPURPOSED
        }
        else
        {
            pushingTowardWallText.text = "Movement Intent: <color=gray>N/A</color>";
            wallAlignmentText.text = $"Jumps Used: {playerCharacter.NumJumpsUsed} / {playerCharacter.MaxJumps}";
        }
    }
}