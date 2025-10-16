using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("")]
public class OrbitalFollowCollision : CinemachineExtension
{
    [Header("Debug")]
    public bool showDebug = false;

    [Header("Collision")]
    public LayerMask collisionMask;

    [Range(0.05f, 1f)] 
    public float sphereCastRadius = 0.35f;
    public bool useSphereCast = true;

    [Header("Boom Settings")]
    public float minBoom = 0.5f;
    public float maxBoom = 2f;
    
    [Tooltip("Smooth speed for boom contraction/expansion.")]
    public float boomSmooth = 14f;
    
    [Tooltip("How far the camera stays off walls.")]
    public float wallBackoff = 0.2f;

    [Header("Arc Sweep")]
    [Tooltip("Extra directions sampled between last and current aim to catch fast rotations.")]
    [Range(0, 12)] 
    public int sweepSamples = 3;

    private CinemachineOrbitalFollow orbitalFollow;
    private CinemachineCameraOffset cameraOffset;

    private Vector3 desiredPosition; // The ideal camera position before collision adjustment
    private Vector3 correctedPosition; // The final camera position after collision adjustment
    private Vector3 useDirection; // The direction from the pivot to the final camera position
    private Vector3 previousDirection; // The previous frame's direction used for the final camera position
    private bool didAnyProbesHit = false; // Did any of the probes hit something?
    private float nearestDistance = float.PositiveInfinity; // The raw physics distance to the nearest wall
    private Vector3 pivotPosition; // the player position
    private float currentBoom;
    private bool initialized;
    private float insideCooldown;


    private const int bufferSize = 4;
    private readonly float[] distBuffer = new float[bufferSize];
    private int bufferIndex;
    private bool bufferFilled;

    private float previousSmoothedHitDistance = 0f; // Previous smoothed hit distance
    private float smoothedTargetHitDistance = 0f;

    // Contact stability (persist across frames)
    private Vector3 lastNormal;
    private float lastPlaneD;
    private bool hadContact;

    protected override void Awake()
    {
        base.Awake();
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        cameraOffset = GetComponent<CinemachineCameraOffset>();
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body)
        {
            return;
        }

        if (orbitalFollow == null)
        {
            Debug.LogWarning("OrbitalFollowCollision requires CinemachineOrbitalFollow component on the same GameObject.");
            return;
        }

        // If Cinemachine passes a negative dt, it’s a re-init tick: reset smoothing.
        if (deltaTime < 0f)
        {
            initialized = false;
            hadContact = false;
            previousDirection = Vector3.zero;
            previousSmoothedHitDistance = 0f;
            smoothedTargetHitDistance = 0f;
            bufferFilled = false;
            bufferIndex = 0;
            return;
        }

        Vector3 offset = GetCameraOffset();
        pivotPosition = orbitalFollow.FollowTargetPosition;
        desiredPosition = state.RawPosition + (state.RawOrientation * offset);
        Vector3 probeDirection = GetProbeDirection();

        if (!initialized)
        {
            currentBoom = maxBoom;
            previousDirection = probeDirection;
            initialized = true;
        }

        // --- sweep for fast rotation coverage ---
        bool insideHit = Physics.CheckSphere(pivotPosition, sphereCastRadius, collisionMask);

        didAnyProbesHit = false;
        nearestDistance = float.PositiveInfinity;                                                           // The raw physics distance to the nearest wall
        useDirection = probeDirection;

        ProbeDirection(probeDirection, Color.green);                                                        // probe current direction
        ProbeSweep(probeDirection);                                                                         // probe intermediate directions between previous probe direction and current probe direction
        if (previousDirection != Vector3.zero && Vector3.Dot(previousDirection, probeDirection) < 0.9995f)
            ProbeDirection(previousDirection, Color.red);                                                   // probe previous direction if significantly different

        previousDirection = probeDirection;

        // --- cooldown ---
        deltaTime = Mathf.Max(deltaTime, 0.001f);
        if (insideHit || didAnyProbesHit) insideCooldown = 0.12f;
        else if (insideCooldown > 0f) insideCooldown = Mathf.Max(0f, insideCooldown - deltaTime);
        bool effectiveHit = insideHit || didAnyProbesHit || insideCooldown > 0f;

        SmoothHitDistance();
        HandleBoom(deltaTime, effectiveHit, insideHit);
        
        // safety depenetration
        if (Physics.CheckSphere(correctedPosition, sphereCastRadius, collisionMask))
        {
            if (Physics.SphereCast(pivotPosition, sphereCastRadius, useDirection, out var hit2, maxBoom, collisionMask, QueryTriggerInteraction.Ignore))
                correctedPosition = pivotPosition + useDirection * Mathf.Max(minBoom, hit2.distance - wallBackoff);
        }

        // micro dead-zone
        if ((correctedPosition - state.RawPosition).sqrMagnitude > 0.0001f)
            state.RawPosition = correctedPosition;

        if (showDebug)
        {
            Debug.Log($"nearestDistance: {nearestDistance:F2}, prevDistance: {previousSmoothedHitDistance:F2}, smoothedTarget: {smoothedTargetHitDistance:F2}, insideHit: {insideHit}, cooldown: {insideCooldown:F2}, currentBoom: {currentBoom:F2})");
        }
    }

    private Vector3 GetCameraOffset()
    {
        var cameraOffset = GetComponent<CinemachineCameraOffset>();
        if (cameraOffset != null)
            return cameraOffset.Offset;
        return Vector3.zero;
    }

    private Vector3 GetProbeDirection()
    {
        Vector3 probeDirection = (desiredPosition - pivotPosition).sqrMagnitude > 1e-6f
            ? (desiredPosition - pivotPosition).normalized
            : (previousDirection == Vector3.zero ? Vector3.back : previousDirection);
        return probeDirection;
    }

    private void ProbeSweep(Vector3 probeDirection)
    {
        if (sweepSamples > 0)
        {
            Vector3 from = (previousDirection == Vector3.zero) ? probeDirection : previousDirection;
            for (int sweepSample = 1; sweepSample <= sweepSamples; sweepSample++)
            {
                float t = (float)sweepSample / (sweepSamples + 1);
                Vector3 mid = Vector3.Slerp(from, probeDirection, t).normalized;
                ProbeDirection(mid, Color.yellow);
            }
        }
    }

    private void ProbeDirection(Vector3 direction, Color debugColor)
    {
        if (direction == Vector3.zero) return;
        bool hitSomething = useSphereCast
            ? Physics.SphereCast(pivotPosition, sphereCastRadius, direction, out RaycastHit hit, maxBoom, collisionMask, QueryTriggerInteraction.Ignore)
            : Physics.Raycast(pivotPosition, direction, out hit, maxBoom, collisionMask, QueryTriggerInteraction.Ignore);

        Debug.DrawRay(pivotPosition, direction * maxBoom, debugColor);

        if (hitSomething)
        {
            Debug.DrawLine(pivotPosition, hit.point, Color.cyan);
            didAnyProbesHit = true;
            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                useDirection = direction;
            }
        }
    }

    // --- low-pass + temporal damp on raw nearestDist ---
    private void SmoothHitDistance()
    {
        if (nearestDistance < float.PositiveInfinity)
        {
            // smooth the previous filtered measurements and the current measurement:
            if (previousSmoothedHitDistance <= 0f) previousSmoothedHitDistance = nearestDistance;
            nearestDistance = Mathf.Lerp(previousSmoothedHitDistance, nearestDistance, 0.25f);
            previousSmoothedHitDistance = nearestDistance;

            // smooth the target distance: 
            if (smoothedTargetHitDistance <= 0f) smoothedTargetHitDistance = nearestDistance;
            smoothedTargetHitDistance = Mathf.Lerp(smoothedTargetHitDistance, nearestDistance, 0.15f);
            nearestDistance = smoothedTargetHitDistance;
        }
    }

    // <summary>
    // Decides the final camera distance (the “boom”) from the player, based on collision results, smooths it, and ensures stability against wall jitter.
    // <param name="deltaTime">The current frame delta time. used for smoothing</param>
    // <param name="effectiveHit">True if any probe or cooldown says “stay short”</param>
    // <param name="insideHit">True if the camera pivot is currently inside a wall</param>
    /// </summary>
    private void HandleBoom(float deltaTime, bool effectiveHit, bool insideHit)
    {
        if (didAnyProbesHit)
        {
            // Contact-plane projection (compile-safe)
            RaycastHit hit;
            bool gotHit = useSphereCast
                ? Physics.SphereCast(pivotPosition, sphereCastRadius, useDirection, out hit, maxBoom, collisionMask, QueryTriggerInteraction.Ignore)
                : Physics.Raycast(pivotPosition, useDirection, out hit, maxBoom, collisionMask, QueryTriggerInteraction.Ignore);

            // Initialize to a valid value so it's always assigned
            Vector3 stablePos = desiredPosition;

            // Project onto contact plane for stability
            if (gotHit)
            {
                float planeD = Vector3.Dot(hit.normal, hit.point);

                if (hadContact && Vector3.Dot(hit.normal, lastNormal) > 0.9f)
                {
                    // project desiredPos onto previous plane along useDir
                    float denom = Vector3.Dot(lastNormal, useDirection);
                    if (Mathf.Abs(denom) > 1e-4f)
                    {
                        float t = (lastPlaneD - Vector3.Dot(lastNormal, desiredPosition)) / denom;
                        stablePos = desiredPosition + useDirection * t;
                    }
                    else
                    {
                        // fallback if nearly parallel: back off along normal
                        stablePos = hit.point - hit.normal * wallBackoff;
                    }
                }
                else
                {
                    // new surface contact
                    stablePos = hit.point - hit.normal * wallBackoff;
                    lastNormal = hit.normal;
                    lastPlaneD = planeD;
                    hadContact = true;
                }
            }
            else
            {
                hadContact = false;
            }

            float stableDist = Vector3.Distance(pivotPosition, stablePos);

            // buffer average
            distBuffer[bufferIndex] = stableDist;
            bufferIndex = (bufferIndex + 1) % bufferSize;
            if (bufferIndex == 0) bufferFilled = true;

            int count = bufferFilled ? bufferSize : bufferIndex;
            float avgDist = 0f;
            for (int i = 0; i < count; i++) avgDist += distBuffer[i];
            avgDist /= Mathf.Max(1, count);

            float contractedTarget = Mathf.Clamp(avgDist, minBoom, maxBoom); // already offset by normal backoff in stablePos
            float targetBoom = Mathf.Min(contractedTarget, currentBoom);     // contract-only on hit
            currentBoom = Mathf.Lerp(currentBoom, targetBoom, deltaTime * boomSmooth);
        }
        else if (insideHit)
        {
            float targetBoom = minBoom + 0.05f;
            currentBoom = Mathf.Lerp(currentBoom, targetBoom, deltaTime * boomSmooth * 2f);
        }
        else if (effectiveHit)
        {
            // hold during cooldown
        }
        else
        {
            hadContact = false;
            float desiredLen = Mathf.Clamp(Vector3.Distance(pivotPosition, desiredPosition), minBoom, maxBoom);
            currentBoom = Mathf.Lerp(currentBoom, desiredLen, deltaTime * (boomSmooth * 0.5f));
        }

        currentBoom = Mathf.Clamp(currentBoom, minBoom, maxBoom);

        // --- final camera position ---
        correctedPosition = pivotPosition + useDirection * currentBoom;
    }

#if UNITY_EDITOR
    // ───────────────────────────── Gizmos ─────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (!enabled) return;
        if (orbitalFollow == null) orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow == null) return;

        var vcam = GetComponent<CinemachineCamera>();
        if (vcam == null) return;

        Vector3 pivotPos = orbitalFollow.FollowTargetPosition;
        Vector3 castOrigin = pivotPos + useDirection;
        float castDist = currentBoom;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(pivotPos, 0.03f);
        UnityEditor.Handles.Label(pivotPos, "Pivot");

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(castOrigin, sphereCastRadius);
        UnityEditor.Handles.Label(castOrigin, $"Cast Origin (r={sphereCastRadius:0.00})");

        Gizmos.color = Color.white;
        Gizmos.DrawLine(castOrigin, castOrigin + useDirection * castDist);

        bool startsInside = Physics.CheckSphere(castOrigin, sphereCastRadius, collisionMask, QueryTriggerInteraction.Ignore);
        if (startsInside)
        {   
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(castOrigin, sphereCastRadius);
            Gizmos.color = new Color(1f, 0f, 0f, 0.35f); // subtle red halo
            Gizmos.DrawWireSphere(castOrigin, sphereCastRadius * 1.05f);
            UnityEditor.Handles.Label(castOrigin + Vector3.up * 0.1f, "Starts INSIDE collider!");
        }

        if (Physics.SphereCast(castOrigin, sphereCastRadius, useDirection, out var hit, castDist, collisionMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 hitCenter = castOrigin + useDirection * hit.distance;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(hitCenter, sphereCastRadius);
            Gizmos.DrawSphere(hit.point, 0.02f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(hit.point, hit.point + hit.normal * 0.25f);
            UnityEditor.Handles.Label(hit.point + hit.normal * 0.1f, $"HIT d={hit.distance:0.###}");
        }

        // Draw current boom line
        Gizmos.color = Color.blue;
        Vector3 cameraPos = pivotPos + useDirection * currentBoom;
        Gizmos.DrawLine(pivotPos, cameraPos);
        Gizmos.DrawWireSphere(cameraPos, 0.05f);
        UnityEditor.Handles.Label(cameraPos, $"Current Boom {currentBoom:F2}");
    }
#endif
}
