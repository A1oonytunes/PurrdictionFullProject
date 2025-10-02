using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingRope_MLab_Uncommented : MonoBehaviour
{
    public int ropeIndex;

    [Header("References")]
    public Grappling_MLab grappling;
    public PlayerMovement_MLab pm;

    [Header("Settings")]
    public int quality = 500;
    public float damper = 14;
    public float strength = 800;
    public float velocity = 15;
    public float waveCount = 3;
    public float waveHeight = 1;
    public AnimationCurve affectCurve;

    private Spring_MLab spring;
    private LineRenderer lr;
    private Vector3 currentGrapplePosition;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        spring = new Spring_MLab();
        spring.SetTarget(0);
    }

    //Called after Update
    private void LateUpdate()
    {
        DrawRope();
    }

    void DrawRope()
    {
        //If not grappling, don't draw rope
        if (!grappling.IsGrappling(ropeIndex))
        {
            currentGrapplePosition = grappling.GetGunTipPosition(ropeIndex);
            spring.Reset();

            if (lr.positionCount > 0)
                lr.positionCount = 0;

            return;
        }

        if(lr.positionCount == 0)
        {
            spring.SetVelocity(velocity);
            lr.positionCount = quality + 1;
        }

        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        Vector3 grapplePoint = grappling.GetGrapplePoint(ropeIndex);
        Vector3 gunTipPosition = grappling.GetGunTipPosition(ropeIndex);
        Vector3 up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);

        for (int i = 0; i < quality + 1; i++)
        {
            float delta = i / (float)quality;
            Vector3 offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value * affectCurve.Evaluate(delta);

            lr.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
        }
    }
}
