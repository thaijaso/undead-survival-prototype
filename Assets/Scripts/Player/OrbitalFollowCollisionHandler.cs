using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("")]
public class CinemachineOrbitalCollisionHandler : CinemachineExtension
{
    [Header("Collision")]
    public LayerMask collisionMask;
    [Range(0.05f, 1f)] public float whiskerRadius = 0.3f;
    [Range(3, 9)] public int whiskerCount = 5;
    [Range(10f, 90f)] public float whiskerArc = 45f;

    [Header("Boom Settings")]
    public float minBoom = 0.3f;
    public float maxBoom = 2f;
    [Tooltip("Smooth speed for boom contraction/expansion")]
    public float boomSmooth = 8f;

    [Header("Offset Settings")]
    [Tooltip("Maximum shoulder offset when boom is fully extended")]
    public float maxOffsetX = 0.3f;
    [Tooltip("Minimum shoulder offset when boom is fully contracted")]
    public float minOffsetX = 0f;
    [Tooltip("How fast shoulder offset transitions")]
    public float offsetSmooth = 8f;

    [Header("Debug")]
    public bool showDebug = true;
    public bool showHUD = true;

    // runtime state
    private float currentBoom;
    private float targetBoom;
    private float currentOffsetX;

    private enum BoomState { Free, Contracting, Hold }
    private BoomState _state = BoomState.Free;

    private Vector3 _pivotPos;
    private Vector3 _desiredPos;
    private float _lastNearest = Mathf.Infinity;
    private int _hitMemory = 0;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState stateRef,
        float deltaTime)
    {
        // Do everything here; we set the final position directly.
        if (stage != CinemachineCore.Stage.Finalize || vcam.Follow == null)
            return;

        if (deltaTime <= 0f) deltaTime = Time.deltaTime;

        _pivotPos   = vcam.Follow.position;
        _desiredPos = stateRef.GetFinalPosition();

        if (currentBoom <= 0f)
            currentBoom = Mathf.Clamp(Vector3.Distance(_pivotPos, _desiredPos), minBoom, maxBoom);

        HandleBoom(deltaTime);

        // Final corrected position along boom
        Vector3 dir = GetDir(_pivotPos, _desiredPos);
        Vector3 corrected = _pivotPos + dir * currentBoom;

        // Apply shoulder offset directly to position (no CameraOffset dependency)
        ApplyCameraOffset(ref corrected, _pivotPos, _desiredPos, deltaTime);

        stateRef.RawPosition = corrected;
    }

    private void HandleBoom(float dt)
    {
        Vector3 dir = GetDir(_pivotPos, _desiredPos);
        bool gotHit = false;
        float nearest = Mathf.Infinity;

        // Whisker origin: pivot-based, nudged forward to avoid inside-player casts
        Vector3 origin = _pivotPos + dir * 0.1f;

        // Draw boom for debugging (runtime-safe)
        if (showDebug) Debug.DrawLine(_pivotPos, _pivotPos + dir * currentBoom, Color.yellow);

        // Fan whiskers in an arc around dir (Y-up)
        for (int i = 0; i < whiskerCount; i++)
        {
            float t = (whiskerCount == 1) ? 0.5f : (i / (float)(whiskerCount - 1));
            float angle = (t - 0.5f) * whiskerArc;
            Vector3 rayDir = Quaternion.AngleAxis(angle, Vector3.up) * dir;

            if (Physics.SphereCast(origin, whiskerRadius, rayDir, out RaycastHit hit, maxBoom,
                collisionMask, QueryTriggerInteraction.Ignore))
            {
                gotHit = true;
                if (hit.distance < nearest) nearest = hit.distance;
                if (showDebug) Debug.DrawLine(origin, hit.point, Color.red);
            }
            else
            {
                if (showDebug) Debug.DrawLine(origin, origin + rayDir * maxBoom, Color.white);
            }
        }

        if (gotHit)
        {
            _lastNearest = nearest;
            targetBoom = Mathf.Clamp(nearest, minBoom, maxBoom);
            _state = BoomState.Contracting;
            _hitMemory = Mathf.Min(_hitMemory + 1, 10);
        }
        else
        {
            _hitMemory = Mathf.Max(_hitMemory - 1, 0);
            if (_hitMemory == 0)
            {
                targetBoom = maxBoom;
                _state = BoomState.Free;
            }
            else
            {
                _state = BoomState.Hold;
            }
        }

        currentBoom = Mathf.Lerp(currentBoom, targetBoom, dt * boomSmooth);
        
        if (showDebug)
        {
            Debug.Log($"[CineDiag f={Time.frameCount}] state={_state} " +
                    $"boom={currentBoom:0.000}/{maxBoom:0.000} " +
                    $"target={targetBoom:0.000} " +
                    $"offsetX={currentOffsetX:0.000} " +
                    $"nearest={_lastNearest:0.000}");
        }
    }

    private void ApplyCameraOffset(ref Vector3 correctedPosition, Vector3 pivotPos, Vector3 desiredPos, float dt)
    {
        float proximity = Mathf.InverseLerp(maxBoom, minBoom, currentBoom);
        Debug.Log($"Boom={currentBoom:0.00}  proximity={proximity:0.00}  " +
          $"targetOffset={Mathf.Lerp(maxOffsetX, minOffsetX, proximity):0.00}");

        float targetOffsetX = Mathf.Lerp(maxOffsetX, minOffsetX, proximity);
        currentOffsetX = Mathf.Lerp(currentOffsetX, targetOffsetX, dt * offsetSmooth);

        Vector3 dir = GetDir(pivotPos, desiredPos);
        Vector3 right = Vector3.Cross(dir, Vector3.up);

        correctedPosition += right * currentOffsetX;

        if (showDebug) Debug.DrawLine(pivotPos, correctedPosition, Color.cyan);
    }

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

        string nearestTxt = float.IsInfinity(_lastNearest) ? "∞" : _lastNearest.ToString("0.000");
        UnityEditor.Handles.Label(
            _pivotPos + Vector3.up * 0.25f,
            $"State: {_state}\nBoom: {currentBoom:0.00}/{maxBoom:0.00}\nNearest: {nearestTxt}\nHitMemory: {_hitMemory}\nOffsetX: {currentOffsetX:0.000}"
        );
    }
#endif
}
