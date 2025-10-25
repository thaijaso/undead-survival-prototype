using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("")]
public class CinemachineOrbitalCollisionHandler : CinemachineExtension
{
    [Header("Collision")]
    public LayerMask collisionMask;
    [Range(0.05f, 1f)] public float whiskerRadius = 0.15f;
    [Range(3, 9)] public int whiskerCount = 5;
    [Range(10f, 120f)] public float whiskerArc = 90f;

    [Header("Boom Settings")]
    [Tooltip("Minimum and maximum boom length (camera distance).")]
    public float minBoom = 0.2f;
    public float maxBoom = 2f;
    [Tooltip("Base smooth speed for boom contraction / expansion.")]
    public float boomSmooth = 14f;
    public float contractionMultiplier = 2.5f;
    public float expansionMultiplier = 1f;

    [Header("Offset Settings")]
    [Tooltip("Maximum shoulder offset when boom is fully extended.")]
    public float maxOffsetX = 0.5f;
    [Tooltip("Minimum shoulder offset when boom is fully contracted.")]
    public float minOffsetX = 0.3f;
    [Tooltip("How fast shoulder offset transitions.")]
    public float offsetSmooth = 20f;

    [Header("Debug")]
    public bool showDebug = true;
    public bool showHUD = true;

    // --- Runtime state ---
    private float currentBoom;
    private float targetBoom;
    private float currentOffsetX;
    private Vector3 pivotPos;
    private Vector3 desiredPos;
    private float lastNearest = Mathf.Infinity;

    // --- Optional diagnostic data ---
    private Vector3 lastDir;
    private float angularVelocity;

    private enum BoomState { Expanding, Contracting }
    private BoomState state = BoomState.Expanding;

    // ----------------------------------------------------------------------

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState stateRef,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize || vcam.Follow == null)
            return;

        if (deltaTime <= 0f) deltaTime = Time.deltaTime;

        pivotPos = vcam.Follow.position;
        desiredPos = stateRef.GetFinalPosition();

        // Track angular velocity (deg/sec) – useful for future tuning or debugging.
        Vector3 currentDir = GetDir(pivotPos, desiredPos);
        if (lastDir != Vector3.zero)
        {
            float angle = Vector3.Angle(lastDir, currentDir);
            angularVelocity = Mathf.Lerp(angularVelocity, angle / deltaTime, deltaTime * 10f);
        }
        lastDir = currentDir;

        if (currentBoom <= 0f)
            currentBoom = Mathf.Clamp(Vector3.Distance(pivotPos, desiredPos), minBoom, maxBoom);

        HandleBoom(deltaTime);

        // Final corrected position along boom
        Vector3 corrected = pivotPos + currentDir * currentBoom;

        // Apply shoulder offset
        ApplyCameraOffset(ref corrected, pivotPos, desiredPos, deltaTime);

        stateRef.RawPosition = corrected;
    }

    // ----------------------------------------------------------------------

    private void HandleBoom(float deltaTime)
    {
        Vector3 dir = GetDir(pivotPos, desiredPos);
        bool gotHit = false;
        float nearest = Mathf.Infinity;

        Vector3 origin = pivotPos + dir * 0.1f;
        if (showDebug) Debug.DrawLine(pivotPos, pivotPos + dir * currentBoom, Color.yellow);

        // Whisker probes in an arc
        for (int i = 0; i < whiskerCount; i++)
        {
            float arcFraction = (whiskerCount == 1) ? 0.5f : (i / (float)(whiskerCount - 1));
            float sideBiasDeg = -Mathf.Sign(currentOffsetX) * 10f;
            float angle = (arcFraction - 0.5f) * whiskerArc + sideBiasDeg;
            Vector3 rayDir = Quaternion.AngleAxis(angle, Vector3.up) * dir;

            if (Physics.SphereCast(origin, whiskerRadius, rayDir, out RaycastHit hit, maxBoom,
                collisionMask, QueryTriggerInteraction.Ignore))
            {
                gotHit = true;
                if (hit.distance < nearest) nearest = hit.distance;

                if (showDebug)
                {
                    Debug.DrawLine(origin, hit.point, Color.red);
                    Debug.DrawRay(hit.point, hit.normal * 0.2f, Color.magenta);
                }
            }
            else if (showDebug)
            {
                Debug.DrawLine(origin, origin + rayDir * maxBoom, Color.white);
            }
        }

        // --- Boom state update ---
        if (gotHit)
        {
            lastNearest = nearest;
            targetBoom = Mathf.Clamp(nearest, minBoom, maxBoom);
            state = BoomState.Contracting;
        }
        else
        {
            targetBoom = maxBoom;
            state = BoomState.Expanding;
        }

        // --- Smooth boom interpolation ---
        float smoothSpeed = (state == BoomState.Contracting)
            ? boomSmooth * contractionMultiplier
            : boomSmooth * expansionMultiplier;

        float blendFactor = 1f - Mathf.Exp(-deltaTime * smoothSpeed);
        currentBoom = Mathf.Lerp(currentBoom, targetBoom, blendFactor);
        currentBoom = Mathf.Clamp(currentBoom, minBoom, maxBoom);

        if (showDebug)
        {
            Debug.Log(
                $"[CineDiag f={Time.frameCount}] state={state} boom={currentBoom:0.000}/{maxBoom:0.000} " +
                $"target={targetBoom:0.000} nearest={lastNearest:0.000} gotHit={gotHit} angVel={angularVelocity:0.0}"
            );
        }
    }

    // ----------------------------------------------------------------------

    private void ApplyCameraOffset(ref Vector3 correctedPosition, Vector3 pivotPos, Vector3 desiredPos, float dt)
    {
        float proximity = Mathf.Pow(Mathf.InverseLerp(maxBoom, minBoom, currentBoom), 2f);
        float targetOffsetX = Mathf.Lerp(maxOffsetX, minOffsetX, proximity);
        currentOffsetX = Mathf.Lerp(currentOffsetX, targetOffsetX, dt * offsetSmooth);

        Vector3 dir = GetDir(pivotPos, desiredPos);
        Vector3 right = Vector3.Cross(dir, Vector3.up);
        correctedPosition += right * currentOffsetX;

        if (showDebug)
            Debug.DrawLine(pivotPos, correctedPosition, Color.cyan);
    }

    // ----------------------------------------------------------------------

    private static Vector3 GetDir(Vector3 from, Vector3 to)
    {
        Vector3 d = to - from;
        if (d.sqrMagnitude < 1e-8f) return Vector3.forward;
        d.Normalize();
        return d;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showHUD || !Application.isPlaying) return;

        string nearestTxt = float.IsInfinity(lastNearest) ? "∞" : lastNearest.ToString("0.000");
        UnityEditor.Handles.Label(
            pivotPos + Vector3.up * 0.25f,
            $"State: {state}\nBoom: {currentBoom:0.00}/{maxBoom:0.00}\nNearest: {nearestTxt}\nAngularVel: {angularVelocity:0.0}\nOffsetX: {currentOffsetX:0.000}"
        );
    }
#endif
}
