using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


// Dave MovementLab - Detector
///
// Content:
/// - detection for jump predictions
///
// Note:
/// This script handles all kinds of extra detections and calculations needed as information for other scripts.
/// I made this extra script to keep all other scripts shorter and more understandable.


public class Detector_MLab : MonoBehaviour
{
    [Header("ToggleAbilities")]
    public bool ShowMarkerSphere = false;

    [Header("References")]
    public PlayerMovement_MLab pm;
    public Transform camT;
    public Transform camHolder;
    public LayerMask whatIsGround;

    [Header("Jump Predictions")]
    [HideInInspector] public bool precisionTargetFound;
    [HideInInspector] public bool precisionTargetIsWall;

    [Header("Debugging")]
    public bool debuggingEnabled;
    public MeshRenderer renderer_markerSphere;
    public Transform markerSphere;
    public Transform someSecondSphere;
    public TextMeshProUGUI text_predictionState;

    private void Start()
    {
        // if no ground layermask is selected, set it to "Default"
        if (whatIsGround.value == 0)
            whatIsGround = LayerMask.GetMask("Default");

        if (!debuggingEnabled)
        {
            renderer_markerSphere.enabled = false;
            someSecondSphere.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private void Update()
    {
        JumpPrediction();

        if(ShowMarkerSphere)
            renderer_markerSphere.enabled = precisionTargetFound;
    }

    /// This function tries to predict where the player wants to jump next.
    /// Needed for precise ground and wall jumping.
    private void JumpPrediction()
    {
        RaycastHit viewRayHit;
        string predictionState;

        if (Physics.Raycast(camT.position, camHolder.forward, out viewRayHit, pm.maxJumpRange, whatIsGround))
        {
            // Case 1 - raycast hits (in maxDistance)
            markerSphere.position = viewRayHit.point;

            predictionState = "in distance";

            precisionTargetFound = true;
        }

        else if (Physics.SphereCast(camT.position, 1f, camHolder.forward, out viewRayHit, 10f, whatIsGround))
        {
            // Case 2 - raycast hits (out of maxDistance)

            // calculate nearest possible point
            Vector3 maxRangePoint = camT.position + camHolder.forward * pm.maxJumpRange;

            RaycastHit wallHit;
            if (Physics.Raycast(maxRangePoint, -viewRayHit.normal, out wallHit, 4f, whatIsGround))
            {
                markerSphere.position = wallHit.point;
                predictionState = "out of distance, to wall";

                precisionTargetFound = true;
            }
            else
            {
                someSecondSphere.position = viewRayHit.point;

                if (Vector3.Distance(camT.position, viewRayHit.point) <= pm.maxJumpRange)
                {
                    predictionState = "out of distance, hitPoint";
                    markerSphere.position = viewRayHit.point;

                    precisionTargetFound = true;
                }
                else
                {
                    predictionState = "out of distance, can't predict point..."; // -> same as case 3
                    markerSphere.position = camT.position + camHolder.forward * pm.maxJumpRange;

                    precisionTargetFound = false;
                }
            }
        }

        else
        {
            // Case 3 - raycast completely misses
            // -> Normal Jump
            // Gizmos.DrawWireSphere(cam.transform.position + camHolder.forward * maxJumpRange, .5f);
            markerSphere.position = camT.position + camHolder.forward * pm.maxJumpRange;
            predictionState = "complete miss";

            precisionTargetFound = false;
        }

        if (precisionTargetFound)
            precisionTargetIsWall = viewRayHit.transform.gameObject.layer == 8;
        else
            precisionTargetIsWall = false;

        if (debuggingEnabled)
            text_predictionState.SetText(predictionState);
    }
}
