using System;
using System.ComponentModel;
using Unity.Cinemachine;
using UnityEngine;

[ExecuteAlways]
public class OrbitalFollowCollisionHandler : CinemachineExtension
{
    [Header("Collision Settings")]
    [SerializeField]
    private LayerMask collisionMask;

    [SerializeField]
    [Min(0.1f)]
    private float minRadius = 0.8f;

    [SerializeField]
    [Min(0.1f)]
    private float maxRadius = 2f;

    [Min(0.1f)]
    private float smoothSpeed = 8f;

    [SerializeField] 
    private float hitSmooth = 10f; // <-- new: filter strength (higher = smoother)

    [SerializeField]
    private float collisionPadding = .15f;

    private CinemachineOrbitalFollow orbitalFollow;
    private CinemachineCamera playerCamera;

    [SerializeField, ReadOnly(true)]
    private float currentRadius = 1f;
    private float smoothedHitDist = 0f; // <-- new: filtered hit distance

    protected override void OnEnable()
    {
        base.OnEnable();
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow != null)
            currentRadius = orbitalFollow.Radius;
        playerCamera = GetComponent<CinemachineCamera>();
        if (playerCamera == null)
            Debug.LogError("OrbitalFollowCollisionHandler requires a CinemachineCamera component on the same GameObject.");
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize) return;

        // 1️⃣ always measure from pivot to the IDEAL camera position
        Vector3 pivotPos = orbitalFollow.FollowTargetPosition;
        Vector3 desiredPos = state.GetFinalPosition();   // full boom
        Vector3 camDir = (desiredPos - pivotPos).normalized;
        float maxBoom = Vector3.Distance(pivotPos, desiredPos);
        float near = state.Lens.NearClipPlane;

        // 2️⃣ cast along the ideal direction, not from the moved camera
        bool isHit = Physics.Linecast(pivotPos, desiredPos, out var hitInfo, collisionMask, QueryTriggerInteraction.Ignore);

        if (isHit)
        {
            // low-pass filter the raw hit distance
            if (smoothedHitDist <= 0f)
            {
                smoothedHitDist = hitInfo.distance;
            }
            else
            {
                smoothedHitDist = Mathf.Lerp(smoothedHitDist, hitInfo.distance, deltaTime * hitSmooth);
            }

            float targetRadius = Mathf.Clamp(smoothedHitDist - (near + collisionPadding), minRadius, maxRadius);

            if (targetRadius < currentRadius)
            {
                currentRadius = targetRadius;
            }
            else
            {
                currentRadius = Mathf.Lerp(currentRadius, targetRadius, deltaTime * smoothSpeed);
            }

            Debug.DrawLine(pivotPos, hitInfo.point, Color.green);
        }
        else
        {
            smoothedHitDist = 0f;
            
            float targetRadius = Mathf.Clamp(maxBoom, minRadius, maxRadius);
            currentRadius = Mathf.Lerp(currentRadius, targetRadius, deltaTime * smoothSpeed);
            
            Debug.DrawLine(pivotPos, desiredPos, Color.red);
        }

        // 3️⃣ compute final camera pos from pivot + direction * radius
        orbitalFollow.Radius = currentRadius;
    }
    
    

#if UNITY_EDITOR
    public bool showGizmos = true;

    void OnDrawGizmosSelected()  // only when the vcam is selected
    {
        if (!showGizmos || playerCamera == null || playerCamera.Follow == null) return;

        var vcam = playerCamera;

        Vector3 pivotPos = vcam.Follow.position;                  // orbit center / pivot
        Vector3 camPos   = vcam.State.GetFinalPosition();         // camera world pos
        Vector3 camDir   = (camPos - pivotPos).normalized;        // pivot -> camera
        Vector3 start    = pivotPos + camDir * minRadius;         // cast origin

        Gizmos.color = Color.red;
        Gizmos.DrawLine(start, start + camDir * (maxRadius - minRadius));
    }
#endif
}
