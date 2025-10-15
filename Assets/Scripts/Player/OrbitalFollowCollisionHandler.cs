using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("")]
public class OrbitalFollowCollision_Fixed : CinemachineExtension
{
    [Header("Collision")]
    public LayerMask collisionMask;
    [Range(0.05f, 1f)] public float sphereCastRadius = 0.35f;
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
    [Range(0, 6)] public int sweepSamples = 3;

    private CinemachineOrbitalFollow orbitalFollow;
    private float currentBoom;
    private bool initialized;
    private float insideCooldown;

    private Vector3 prevDir;
    private const int bufferSize = 4;
    private readonly float[] distBuffer = new float[bufferSize];
    private int bufferIndex;
    private bool bufferFilled;

    private float prevDist = 0f;
    private float smoothedTarget = 0f;

    // Contact stability (persist across frames)
    private Vector3 lastNormal;
    private float lastPlaneD;
    private bool hadContact;

    protected override void Awake()
    {
        base.Awake();
        orbitalFollow = GetComponentInChildren<CinemachineOrbitalFollow>(true);
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body || orbitalFollow == null)
            return;

        Vector3 pivotPos = orbitalFollow.FollowTargetPosition;
        Vector3 desiredPos = state.RawPosition;

        // Include CameraOffset if present
        var offsetComp = vcam.GetComponent<CinemachineCameraOffset>();
        if (offsetComp != null)
            desiredPos += state.RawOrientation * offsetComp.Offset;

        Vector3 rawDir = (desiredPos - pivotPos).sqrMagnitude > 1e-6f
            ? (desiredPos - pivotPos).normalized
            : (prevDir == Vector3.zero ? Vector3.back : prevDir);

        if (!initialized)
        {
            currentBoom = maxBoom;
            prevDir = rawDir;
            initialized = true;
        }

        // --- sweep for fast rotation coverage ---
        bool insideHit = Physics.CheckSphere(pivotPos, sphereCastRadius, collisionMask);

        bool anyHit = false;
        float nearestDist = float.PositiveInfinity;
        Vector3 useDir = rawDir;

        void ProbeDir(Vector3 dir, Color debugColor)
        {
            if (dir == Vector3.zero) return;
            RaycastHit hit;
            bool hitSomething = useSphereCast
                ? Physics.SphereCast(pivotPos, sphereCastRadius, dir, out hit, maxBoom, collisionMask, QueryTriggerInteraction.Ignore)
                : Physics.Raycast(pivotPos, dir, out hit, maxBoom, collisionMask, QueryTriggerInteraction.Ignore);

            Debug.DrawRay(pivotPos, dir * maxBoom, debugColor);

            if (hitSomething)
            {
                Debug.DrawLine(pivotPos, hit.point, Color.cyan);
                anyHit = true;
                if (hit.distance < nearestDist)
                {
                    nearestDist = hit.distance;
                    useDir = dir;
                }
            }
        }

        // current, mids, previous
        ProbeDir(rawDir, Color.green);
        if (sweepSamples > 0)
        {
            Vector3 from = (prevDir == Vector3.zero) ? rawDir : prevDir;
            for (int i = 1; i <= sweepSamples; i++)
            {
                float t = (float)i / (sweepSamples + 1);
                Vector3 mid = Vector3.Slerp(from, rawDir, t).normalized;
                ProbeDir(mid, Color.yellow);
            }
        }
        if (prevDir != Vector3.zero && Vector3.Dot(prevDir, rawDir) < 0.9995f)
            ProbeDir(prevDir, Color.red);

        prevDir = rawDir;

        // --- cooldown ---
        float dt = Mathf.Max(Time.deltaTime, 0.001f);
        if (insideHit || anyHit) insideCooldown = 0.12f;
        else if (insideCooldown > 0f) insideCooldown = Mathf.Max(0f, insideCooldown - dt);
        bool effectiveHit = insideHit || anyHit || insideCooldown > 0f;

        // --- low-pass + temporal damp on raw nearestDist ---
        if (nearestDist < float.PositiveInfinity)
        {
            if (prevDist <= 0f) prevDist = nearestDist;
            nearestDist = Mathf.Lerp(prevDist, nearestDist, 0.25f);
            prevDist = nearestDist;

            if (smoothedTarget <= 0f) smoothedTarget = nearestDist;
            smoothedTarget = Mathf.Lerp(smoothedTarget, nearestDist, 0.15f);
            nearestDist = smoothedTarget;
        }

        // --- boom logic ---
        if (anyHit)
        {
            // Contact-plane projection (compile-safe)
            RaycastHit hit;
            bool gotHit = useSphereCast
                ? Physics.SphereCast(pivotPos, sphereCastRadius, useDir, out hit, maxBoom, collisionMask, QueryTriggerInteraction.Ignore)
                : Physics.Raycast(pivotPos, useDir, out hit, maxBoom, collisionMask, QueryTriggerInteraction.Ignore);

            // Initialize to a valid value so it's always assigned
            Vector3 stablePos = desiredPos;

            if (gotHit)
            {
                float planeD = Vector3.Dot(hit.normal, hit.point);

                if (hadContact && Vector3.Dot(hit.normal, lastNormal) > 0.9f)
                {
                    // project desiredPos onto previous plane along useDir
                    float denom = Vector3.Dot(lastNormal, useDir);
                    if (Mathf.Abs(denom) > 1e-4f)
                    {
                        float t = (lastPlaneD - Vector3.Dot(lastNormal, desiredPos)) / denom;
                        stablePos = desiredPos + useDir * t;
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

            float stableDist = Vector3.Distance(pivotPos, stablePos);

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
            float desiredLen = Mathf.Clamp(Vector3.Distance(pivotPos, desiredPos), minBoom, maxBoom);
            currentBoom = Mathf.Lerp(currentBoom, desiredLen, deltaTime * (boomSmooth * 0.5f));
        }

        currentBoom = Mathf.Clamp(currentBoom, minBoom, maxBoom);

        // --- final camera position ---
        Vector3 correctedPos = pivotPos + useDir * currentBoom;

        // safety depenetration
        if (Physics.CheckSphere(correctedPos, sphereCastRadius, collisionMask))
        {
            if (Physics.SphereCast(pivotPos, sphereCastRadius, useDir, out var hit2, maxBoom, collisionMask, QueryTriggerInteraction.Ignore))
                correctedPos = pivotPos + useDir * Mathf.Max(minBoom, hit2.distance - wallBackoff);
        }

        // micro dead-zone
        if ((correctedPos - state.RawPosition).sqrMagnitude > 0.0001f)
            state.RawPosition = correctedPos;
    }
}
