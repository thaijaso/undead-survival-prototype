using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("")]
public class CinemachineOrbitalCollisionHandler : CinemachineExtension
{
    [Header("Collision")]
    public LayerMask collisionMask;
    [Range(0.05f, 1f)] public float whiskerRadius = 0.15f;   // was 0.1; 0.15-0.2 detects edges earlier
    [Range(3, 9)] public int whiskerCount = 5;
    [Range(10f, 120f)] public float whiskerArc = 90f;

    [Header("Boom Settings")]
    public float minBoom = 0.2f;
    public float maxBoom = 2f;
    [Tooltip("Smooth speed for boom contraction/expansion")]
    public float boomSmooth = 14f;

    [Header("Offset Settings")]
    [Tooltip("Maximum shoulder offset when boom is fully extended")]
    public float maxOffsetX = 0.5f;
    [Tooltip("Minimum shoulder offset when boom is fully contracted")]
    public float minOffsetX = 0.3f;
    [Tooltip("How fast shoulder offset transitions")]
    public float offsetSmooth = 20f;

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

        // Apply shoulder offset directly to position
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

        if (showDebug) Debug.DrawLine(_pivotPos, _pivotPos + dir * currentBoom, Color.yellow);

        // Fan whiskers in an arc (Y-up), biased toward the camera's offset side
        for (int i = 0; i < whiskerCount; i++)
        {
            float t = (whiskerCount == 1) ? 0.5f : (i / (float)(whiskerCount - 1));

            // Bias toward camera side; flipped sign to lean toward the visible shoulder
            float sideBiasDeg = -Mathf.Sign(currentOffsetX) * 10f; // tweak 5–15° to taste
            float angle = (t - 0.5f) * whiskerArc + sideBiasDeg;

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
        // Nonlinear scaling: keep wide longer, collapse faster near close walls
        float proximity = Mathf.Pow(Mathf.InverseLerp(maxBoom, minBoom, currentBoom), 2f);

        float targetOffsetX = Mathf.Lerp(maxOffsetX, minOffsetX, proximity);
        currentOffsetX = Mathf.Lerp(currentOffsetX, targetOffsetX, dt * offsetSmooth);

        Vector3 dir = GetDir(pivotPos, desiredPos);
        Vector3 right = Vector3.Cross(dir, Vector3.up);

        correctedPosition += right * currentOffsetX;

        if (showDebug)
        {
            Debug.DrawLine(pivotPos, correctedPosition, Color.cyan);
            Debug.Log($"Boom={currentBoom:0.00}  proximity={proximity:0.00}  targetOffset={targetOffsetX:0.00}");
        }
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
