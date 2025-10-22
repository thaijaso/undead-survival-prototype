using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

// -----------------------------------------------------------------------------
// CinemachineOrbitalCollisionHandler — ANTI-JITTER BUILD
//  - Adds re-entry hysteresis (prevents fast Free/Contracting toggles)
//  - Adds smooth fade when hits are lost (temporal stability)
//  - Optional soft low-pass on target
//  - GUI safely isolated in OnDrawGizmos()
// -----------------------------------------------------------------------------
[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("")]
public class CinemachineOrbitalCollisionHandler : CinemachineExtension
{
    [Header("Debug")]
    public bool showDebug = true;
    public bool showHUD = true;
    [Tooltip("Print event-driven logs when something meaningful changes.")]
    public bool verboseLogs = true;

    [Header("Collision")]
    public LayerMask collisionMask;

    [Header("Boom Settings")]
    public float minBoom = 0.5f;
    public float maxBoom = 2f;
    [Tooltip("Smooth speed for boom contraction/expansion.")]
    public float boomSmooth = 14f;

    [Header("Whisker Settings")]
    [Tooltip("How far the camera stays off walls.")]
    public float wallBackoff = 0.3f;
    [SerializeField, Range(0f, 60f)] private float spreadAngle = 25f;
    [SerializeField] private float whiskerRadius = 0.04f;

    [Header("Camera Offset")]
    public float maxOffsetX = 0.5f;
    public float minOffsetX = 0.25f;
    public float offsetSmooth = 8f;

    [Header("Stability")]
    [Tooltip("Frames to keep last hit alive before expanding when rays miss.")]
    public int missFrameHold = 6; // was 3
    [Tooltip("Ignore hit distance changes smaller than this (meters).")]
    public float distanceEpsilon = 0.03f;
    [Tooltip("Additional hysteresis on re-expansion (meters).")]
    public float hysteresis = 0.1f; // was 0.05f

    private float currentOffsetX;
    private float lastNearestHitDist = float.PositiveInfinity;

    private CinemachineOrbitalFollow orbitalFollow;
    private CinemachineCameraOffset cameraOffset;
    private Vector3 desiredPosition;
    private Vector3 correctedPosition;
    private Vector3 previousDirection;
    private Vector3 pivotPosition;

    private float currentBoom;
    private bool initialized;

    private enum BoomState { Free, Hold, Contracting }
    private BoomState state = BoomState.Free;
    private int missFrames = 0;

    private readonly float[] _hitBuf = new float[3];
    private int _hitCount = 0;

    private struct LogSnap { public BoomState state; public float boom; public float target; public float nearest; public int miss; }
    private LogSnap lastLog;

    protected override void Awake()
    {
        base.Awake();
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        cameraOffset = GetComponent<CinemachineCameraOffset>();
        correctedPosition = transform.position;
        if (cameraOffset != null) maxOffsetX = cameraOffset.Offset.x;
        lastLog = new LogSnap { state = (BoomState)999, boom = -999f, target = -999f, nearest = -999f, miss = -999 };
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState stateArg,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body) return;
        if (orbitalFollow == null) return;

        if (deltaTime < 0f)
        {
            initialized = false;
            previousDirection = Vector3.zero;
            state = BoomState.Free;
            missFrames = 0;
            lastNearestHitDist = float.PositiveInfinity;
            _hitCount = 0;
            return;
        }

        float dt = Mathf.Max(0.0001f, deltaTime);
        pivotPosition = orbitalFollow.FollowTargetPosition;
        desiredPosition = stateArg.RawPosition;

        Vector3 probeDir = (desiredPosition - pivotPosition).sqrMagnitude > 1e-6f
            ? (desiredPosition - pivotPosition).normalized
            : (previousDirection == Vector3.zero ? Vector3.back : previousDirection);
        if (!probeDir.IsFinite()) probeDir = Vector3.back;

        if (!initialized)
        {
            currentBoom = maxBoom;
            previousDirection = probeDir;
            initialized = true;
        }

        previousDirection = probeDir;
        HandleBoom(dt);

        if (!correctedPosition.IsFinite())
            correctedPosition = pivotPosition + previousDirection * maxBoom;

        stateArg.RawPosition = correctedPosition;
    }

    private void HandleBoom(float deltaTime)
    {
        Vector3 dir = (desiredPosition - pivotPosition).normalized;
        if (dir.sqrMagnitude < 1e-8f || !dir.IsFinite())
            dir = previousDirection == Vector3.zero ? Vector3.back : previousDirection;
        previousDirection = dir;

        float rayRange = maxBoom + wallBackoff;

        // Whisker fan setup
        Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
        Vector3 flatUp = Vector3.Cross(dir, right).normalized;
        List<Vector3> whiskerDirs = new()
        {
            dir,
            Quaternion.AngleAxis(-spreadAngle, flatUp) * dir,
            Quaternion.AngleAxis(spreadAngle,  flatUp) * dir,
            Quaternion.AngleAxis(-spreadAngle * 2f, flatUp) * dir,
            Quaternion.AngleAxis(spreadAngle * 2f,  flatUp) * dir,
        };

        Vector3 camRight = Vector3.Cross(dir, Vector3.up).normalized;
        Vector3 origin = pivotPosition + camRight * currentOffsetX * 0.5f;

        bool gotHit = false;
        float nearestRaw = float.PositiveInfinity;
        RaycastHit nearestInfo = default;

        for (int i = 0; i < whiskerDirs.Count; i++)
        {
            Vector3 wdir = whiskerDirs[i];
            if (Physics.SphereCast(origin, whiskerRadius, wdir, out RaycastHit hit, rayRange, collisionMask, QueryTriggerInteraction.Ignore))
            {
                gotHit = true;
                if (hit.distance < nearestRaw)
                {
                    nearestRaw = hit.distance;
                    nearestInfo = hit;
                }
                if (showDebug) Debug.DrawLine(origin, hit.point, Color.red);
            }
            else if (showDebug) Debug.DrawRay(origin, wdir * rayRange, Color.white);
        }

        float nearestFiltered = lastNearestHitDist;
        if (gotHit && float.IsFinite(nearestRaw))
        {
            PushHit(nearestRaw);
            nearestFiltered = Median3();
            if (Mathf.Abs(nearestFiltered - lastNearestHitDist) < distanceEpsilon)
                nearestFiltered = lastNearestHitDist;
            lastNearestHitDist = nearestFiltered;
        }

        float target = maxBoom;
        BoomState prevState = state;

        if (gotHit && float.IsFinite(nearestFiltered))
        {
            missFrames = 0;
            state = BoomState.Contracting;
            target = Mathf.Clamp(nearestFiltered - wallBackoff, minBoom, maxBoom);
        }
        else
        {
            // --- Anti-jitter: hold for hysteresis period before freeing ---
            missFrames++;

            float hitConfidence = Mathf.Clamp01(1f - missFrames / (float)missFrameHold);
            float blendedNearest = Mathf.Lerp(maxBoom, lastNearestHitDist, hitConfidence);

            if (state != BoomState.Free)
                state = BoomState.Hold;

            if (missFrames >= missFrameHold && currentBoom >= lastNearestHitDist + hysteresis)
            {
                state = BoomState.Free;
                target = maxBoom;
                _hitCount = 0;
                lastNearestHitDist = float.PositiveInfinity;
            }
            else
            {
                target = Mathf.Clamp(blendedNearest - wallBackoff, minBoom, maxBoom);
                if (!float.IsFinite(target)) target = currentBoom;
            }
        }

        // Optional low-pass for target (further smoothness)
        target = Mathf.Lerp(lastLog.target, target, deltaTime * 6f);

        float before = currentBoom;
        float lerpSpeed = (state == BoomState.Contracting) ? boomSmooth * 4f : boomSmooth * 0.5f;
        currentBoom = Mathf.Lerp(currentBoom, target, deltaTime * lerpSpeed);
        currentBoom = Mathf.Clamp(currentBoom, minBoom, maxBoom);

        correctedPosition = pivotPosition + dir * currentBoom;

        if (verboseLogs)
        {
            bool stateChanged = state != lastLog.state;
            bool targetChanged = Mathf.Abs(target - lastLog.target) > 0.01f;
            bool nearestChanged = Mathf.Abs(lastNearestHitDist - lastLog.nearest) > 0.01f;
            bool boomChanged = Mathf.Abs(currentBoom - lastLog.boom) > 0.01f;
            bool missChanged = missFrames != lastLog.miss;

            if (stateChanged || targetChanged || nearestChanged || boomChanged || missChanged)
            {
                Debug.Log($"[CineDiag f={Time.frameCount}] state={state} prev={prevState} gotHit={(gotHit?1:0)} missFrames={missFrames}/{missFrameHold} raw={nearestRaw:0.000} filt={lastNearestHitDist:0.000} target={target:0.000} boom(before/after)={before:0.000}/{currentBoom:0.000}");
                lastLog = new LogSnap { state = state, boom = currentBoom, target = target, nearest = lastNearestHitDist, miss = missFrames };
            }
        }

        // Smooth camera side-offset
        float proximity = Mathf.InverseLerp(maxBoom, minBoom, currentBoom);
        float targetOffsetX = Mathf.Lerp(maxOffsetX, minOffsetX, proximity);
        currentOffsetX = Mathf.Lerp(currentOffsetX, targetOffsetX, deltaTime * offsetSmooth);
        if (cameraOffset != null) cameraOffset.Offset.x = currentOffsetX;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showHUD) return;
        if (!Application.isPlaying) return;

        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(
            pivotPosition + Vector3.up * 0.25f,
            $"State: {state}\nBoom: {currentBoom:0.00}/{maxBoom:0.00}\nMissHold: {missFrames}/{missFrameHold}\nNearest: {(float.IsInfinity(lastNearestHitDist) ? "∞" : lastNearestHitDist.ToString("0.000"))}");
    }
#endif

    private void PushHit(float d)
    {
        if (_hitCount < 3) _hitCount++;
        _hitBuf[2] = _hitBuf[1];
        _hitBuf[1] = _hitBuf[0];
        _hitBuf[0] = d;
    }

    private float Median3()
    {
        if (_hitCount == 0) return float.PositiveInfinity;
        if (_hitCount == 1) return _hitBuf[0];
        if (_hitCount == 2) return 0.5f * (_hitBuf[0] + _hitBuf[1]);
        float a = _hitBuf[0], b = _hitBuf[1], c = _hitBuf[2];
        if (a > b) (a, b) = (b, a);
        if (b > c) (b, c) = (c, b);
        if (a > b) (a, b) = (b, a);
        return b;
    }
}

// -----------------------------------------------------------------------------
// Global helpers
// -----------------------------------------------------------------------------
public static class Vector3Extensions
{
    public static bool IsFinite(this Vector3 v)
        => float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
}
