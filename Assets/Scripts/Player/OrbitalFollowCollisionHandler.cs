using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

// -----------------------------------------------------------------------------
//  CinemachineOrbitalCollisionHandler — FINAL ULTRA-STABLE BUILD
//  • Snappy contraction (MoveTowards)
//  • Smooth expansion (Lerp)
//  • Noise-resistant target filtering + dead-zone
//  • No GUI calls outside OnDrawGizmos
// -----------------------------------------------------------------------------
[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("")]
public class CinemachineOrbitalCollisionHandler : CinemachineExtension
{
    [Header("Debug")]
    public bool showDebug = true;
    public bool showHUD = true;
    public bool verboseLogs = false;

    [Header("Collision")]
    public LayerMask collisionMask;

    [Header("Boom Settings")]
    public float minBoom = 0.5f;
    public float maxBoom = 2f;

    [Header("Speed Settings")]
    [Tooltip("Speed (m/s) for contracting toward wall.")]
    public float contractionSpeed = 25f;
    [Tooltip("Lerp smoothing for expansion away from wall.")]
    public float expansionSmooth = 6f;

    [Header("Whisker Settings")]
    public float wallBackoff = 0.3f;
    [SerializeField, Range(0f, 60f)] private float spreadAngle = 25f;
    [SerializeField] private float whiskerRadius = 0.04f;

    [Header("Camera Offset")]
    public float maxOffsetX = 0.5f;
    public float minOffsetX = 0.25f;
    public float offsetSmooth = 8f;

    [Header("Stability")]
    public int missFrameHold = 8;
    public float distanceEpsilon = 0.03f;
    public float hysteresis = 0.1f;

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
        lastLog = new LogSnap { state = (BoomState)999, boom = -999f, target = maxBoom, nearest = float.PositiveInfinity, miss = 0 };
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

        float rayRange = maxBoom + wallBackoff + 0.15f;

        // --- Whisker setup ---
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

        foreach (var wdir in whiskerDirs)
        {
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

        // --- NEW: stability filters ---
        if (Mathf.Abs(target - currentBoom) < 0.01f)
            target = currentBoom;                               // ignore micro changes
        target = Mathf.Lerp(lastLog.target, target, deltaTime * 20f); // smooth target

        float before = currentBoom;

        if (state == BoomState.Contracting)
        {
            currentBoom = Mathf.MoveTowards(currentBoom, target, deltaTime * contractionSpeed);
            if (currentBoom < target) currentBoom = target; // prevent overshoot
        }
        else
        {
            currentBoom = Mathf.Lerp(currentBoom, target, deltaTime * expansionSmooth);
        }

        currentBoom = Mathf.Clamp(currentBoom, minBoom, maxBoom);
        correctedPosition = pivotPosition + dir * currentBoom;

        // --- Logging & snapshot ---
        bool changed =
            Mathf.Abs(currentBoom - lastLog.boom) > 0.01f ||
            Mathf.Abs(target - lastLog.target) > 0.01f ||
            state != lastLog.state ||
            missFrames != lastLog.miss;

        if (verboseLogs && changed)
        {
            Debug.Log($"[CineDiag f={Time.frameCount}] state={state} gotHit={(gotHit?1:0)} raw={nearestRaw:0.000} filt={lastNearestHitDist:0.000} target={target:0.000} boom(before/after)={before:0.000}/{currentBoom:0.000}");
        }

        lastLog = new LogSnap { state = state, boom = currentBoom, target = target, nearest = lastNearestHitDist, miss = missFrames };

        // side-offset smoothing
        float proximity = Mathf.InverseLerp(maxBoom, minBoom, currentBoom);
        float targetOffsetX = Mathf.Lerp(maxOffsetX, minOffsetX, proximity);
        currentOffsetX = Mathf.Lerp(currentOffsetX, targetOffsetX, deltaTime * offsetSmooth);
        if (cameraOffset != null) cameraOffset.Offset.x = currentOffsetX;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showHUD || !Application.isPlaying) return;

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
//  Vector3 helper
// -----------------------------------------------------------------------------
public static class Vector3Extensions
{
    public static bool IsFinite(this Vector3 v)
        => float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
}
