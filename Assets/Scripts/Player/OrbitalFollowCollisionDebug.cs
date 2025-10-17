using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("")]
public class OrbitalFollowCollisionDebug : CinemachineExtension
{
    public LayerMask collisionMask;
    public float sphereCastRadius = 0.3f;
    public float maxBoom = 2f;
    public bool useSphereCast = true;

    private CinemachineOrbitalFollow orbitalFollow;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize)
            return;

        if (!orbitalFollow)
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        if (!orbitalFollow)
            return;

        Vector3 pivot = orbitalFollow.FollowTargetPosition;
        Vector3 dir = -state.ReferenceUp; // or your actual camera-back direction
        dir.Normalize();

        Vector3 castOrigin = pivot;
        float castDist = maxBoom;
        RaycastHit hit;
        bool hasHit = Physics.SphereCast(castOrigin, sphereCastRadius, dir, out hit, castDist, collisionMask);

        // Draw the pivot
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(pivot, 0.05f);
        UnityEditor.Handles.Label(pivot, "Pivot");

        if (hasHit)
        {
            // 1️⃣ Sphere center at moment of contact
            Vector3 hitCenter = castOrigin + dir * hit.distance;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitCenter, sphereCastRadius);
            UnityEditor.Handles.Label(hitCenter, $"Hit Center (hit.distance={hit.distance:0.00})");

            // 2️⃣ Actual camera position (edge flush with real wall)
            Vector3 finalCamPos = castOrigin + dir * (hit.distance - sphereCastRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(finalCamPos, sphereCastRadius);
            UnityEditor.Handles.Label(finalCamPos, "Final Cam Pos");

            // 3️⃣ Debug line from pivot → hit
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pivot, hitCenter);
        }
        else
        {
            // No hit: show desired camera position
            Vector3 desired = castOrigin + dir * castDist;
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(desired, sphereCastRadius);
            UnityEditor.Handles.Label(desired, "No hit (desired)");
            Gizmos.DrawLine(pivot, desired);
        }
    }
}
