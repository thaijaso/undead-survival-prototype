using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("")]
public class OrbitalFollowCollision : CinemachineExtension
{
    [Header("Debug")]
    public bool showDebug = true;
    public bool showHUD = true;

    [Header("Collision")]
    public LayerMask collisionMask;
    [Range(0.05f, 1f)] public float sphereCastRadius = 0.4f;

    [Header("Boom Settings")]
    public float minBoom = 0.5f;
    public float maxBoom = 2f;
    [Tooltip("Smooth speed for boom contraction/expansion.")]
    public float boomSmooth = 14f;
    [Tooltip("How far the camera stays off walls.")]
    public float wallBackoff = 0.3f;
    [SerializeField, Range(0f, 60f), Tooltip("Whisker spread angle in degrees (tune: 10-25º typical).")]
    private float spreadAngle = 25f;

    private CinemachineOrbitalFollow orbitalFollow;
    private Vector3 desiredPosition;
    private Vector3 correctedPosition;

    private Vector3 previousDirection;
    private bool didAnyProbesHit;
    private Vector3 pivotPosition;
    private float currentBoom;
    private bool initialized;
    private bool hadContact;

    // Debug fields
    public Vector3 lastCastOrigin;
    public Vector3 lastHitPoint;
    public Vector3 lastHitNormal;
    public bool lastHadHit;

    private float prevBoom;
    private string boomState = "Free";

    protected override void Awake()
    {
        base.Awake();
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        correctedPosition = transform.position; // safe fallback
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body)
            return;

        if (orbitalFollow == null)
        {
            Debug.LogWarning("OrbitalFollowCollision requires CinemachineOrbitalFollow on the same GameObject.");
            return;
        }

        if (deltaTime < 0f)
        {
            initialized = false;
            hadContact = false;
            previousDirection = Vector3.zero;
            return;
        }

        if (stage == CinemachineCore.Stage.Body)
        {
            Vector3 pivotPos = orbitalFollow.FollowTargetPosition;
            Vector3 camPos = state.GetFinalPosition();
            Debug.DrawLine(pivotPos, camPos, Color.green);
        }

        float dt = Mathf.Max(0.0001f, deltaTime);
        pivotPosition = orbitalFollow.FollowTargetPosition;
        desiredPosition = state.RawPosition;

        Vector3 probeDirection = (desiredPosition - pivotPosition).sqrMagnitude > 1e-6f
            ? (desiredPosition - pivotPosition).normalized
            : (previousDirection == Vector3.zero ? Vector3.back : previousDirection);

        if (!initialized)
        {
            currentBoom = maxBoom;
            prevBoom = currentBoom;
            previousDirection = probeDirection;
            initialized = true;
        }

        didAnyProbesHit = false;
        previousDirection = probeDirection;

        HandleBoom(dt);

        state.RawPosition = correctedPosition;
    }

    // -----------------------------------------------------------------------
    // MAIN LOGIC
    // -----------------------------------------------------------------------
    private void HandleBoom(float deltaTime)
    {
        hadContact = false;
        didAnyProbesHit = false;

        // -------- Setup --------
        Vector3 dir = (desiredPosition - pivotPosition).normalized;
        if (dir.sqrMagnitude < 1e-8f)
            dir = previousDirection == Vector3.zero ? Vector3.back : previousDirection;
        previousDirection = dir;

        float maxLen = maxBoom;
        float nearestHit = maxLen;
        bool gotHit = false;

        // Early proximity check (cheap skip)
        bool nearbyCollision = Physics.CheckSphere(
            pivotPosition + dir * (maxLen * 0.5f),
            sphereCastRadius,
            collisionMask,
            QueryTriggerInteraction.Ignore
        );

        if (!nearbyCollision)
        {
            float desiredLen = Mathf.Clamp(Vector3.Distance(pivotPosition, desiredPosition), minBoom, maxBoom);
            currentBoom = Mathf.Lerp(currentBoom, desiredLen, deltaTime * boomSmooth * 0.5f);
            correctedPosition = pivotPosition + dir * currentBoom;
            if (showDebug)
                Debug.DrawLine(pivotPosition, correctedPosition, Color.blue);
            UpdateBoomState();
            return;
        }

    // -------- Build whisker directions (center + 8) --------
    // uses serialized `spreadAngle` (degrees)
        Vector3 right = Vector3.Cross(Vector3.up, dir);
        if (right.sqrMagnitude < 1e-6f)
            right = Vector3.Cross(Vector3.forward, dir);
        right.Normalize();
        Vector3 up = Vector3.Cross(dir, right);

        List<Vector3> whiskerDirs = new List<Vector3>(9) { dir };
        for (int x = -1; x <= 1; x++)
        for (int y = -1; y <= 1; y++)
        {
            if (x == 0 && y == 0) continue;
            Quaternion rot = Quaternion.AngleAxis(x * spreadAngle, up) * Quaternion.AngleAxis(y * spreadAngle, right);
            whiskerDirs.Add((rot * dir).normalized);
        }

        Vector3 origin = pivotPosition;
        float rayRange = maxLen + wallBackoff;

        // Track nearest hit
        bool hasNearest = false;
        RaycastHit nearestInfo = default;

        // -------- Fire whiskers --------
        foreach (var d in whiskerDirs)
        {
            if (showDebug)
                Debug.DrawRay(origin, d * rayRange, new Color(0f, 1f, 1f, 0.25f));

            if (Physics.Raycast(origin, d, out RaycastHit hit, rayRange, collisionMask, QueryTriggerInteraction.Ignore))
            {
                gotHit = true;
                didAnyProbesHit = true;

                if (hit.distance < nearestHit)
                {
                    nearestHit = hit.distance;
                    nearestInfo = hit;
                    hasNearest = true;
                }

                if (showDebug)
                    Debug.Log($"[WHISKER HIT] {hit.collider.name} dist={hit.distance:F3}");
            }
        }

        // -------- Apply results + magenta marker for nearest --------
        if (gotHit && hasNearest)
        {
            float targetDist = Mathf.Clamp(nearestHit - wallBackoff, minBoom, maxBoom);
            currentBoom = Mathf.Lerp(currentBoom, targetDist, deltaTime * boomSmooth * 4f);
            correctedPosition = pivotPosition + dir * currentBoom;
            hadContact = true;
            lastHadHit = true;
            lastHitPoint = nearestInfo.point;
            lastHitNormal = nearestInfo.normal;

            if (showDebug)
            {
                Debug.DrawLine(pivotPosition, correctedPosition, Color.red);
                Debug.Log($"[CONTRACT] nearest={nearestHit:F3}, boom={currentBoom:F3}");

                // 🟣 Draw small magenta cross + normal at nearest hit
                float size = 0.05f;
                Vector3 p = nearestInfo.point;

                Debug.DrawRay(p, nearestInfo.normal * 0.25f, Color.magenta, 0.5f); // normal
                Debug.DrawLine(p + Vector3.up * size, p - Vector3.up * size, Color.magenta, 0.5f);
                Debug.DrawLine(p + Vector3.right * size, p - Vector3.right * size, Color.magenta, 0.5f);
                Debug.DrawLine(p + Vector3.forward * size, p - Vector3.forward * size, Color.magenta, 0.5f);
            }
        }
        else
        {
            lastHadHit = false;
            float expandedTarget = Mathf.Clamp(maxBoom, minBoom, maxBoom);
            currentBoom = Mathf.Lerp(currentBoom, expandedTarget, deltaTime * boomSmooth * 0.5f);
            correctedPosition = pivotPosition + dir * currentBoom;

            if (showDebug)
            {
                Debug.DrawLine(pivotPosition, correctedPosition, Color.yellow);
                Debug.Log("[FREE] expanding to full boom");
            }
        }

        currentBoom = Mathf.Clamp(currentBoom, minBoom, maxBoom);
        UpdateBoomState();
    }

    private void UpdateBoomState()
    {
        float diff = currentBoom - prevBoom;
        float eps = 0.001f;

        if (Mathf.Abs(diff) <= eps)
            boomState = "Stable";
        else if (diff < 0f)
            boomState = "Contracting";
        else
            boomState = "Expanding";

        prevBoom = currentBoom;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!enabled) return;

        if (orbitalFollow == null)
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow == null) return;

        Vector3 pivotPos = orbitalFollow.FollowTargetPosition;

        // --- Pivot ---
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(pivotPos, 0.025f);
        UnityEditor.Handles.Label(pivotPos, "Pivot");

        // --- Cast origin ---
        bool showOrigin = didAnyProbesHit || hadContact || lastHadHit;
        if (showOrigin)
        {
            Color originCol =
                lastHadHit ? Color.yellow :
                hadContact ? Color.yellow :
                didAnyProbesHit ? new Color(1f, 1f, 1f, 0.7f) :
                new Color(0.8f, 0.8f, 0.8f, 0.4f);

            Gizmos.color = originCol;
            Gizmos.DrawWireSphere(lastCastOrigin, sphereCastRadius);
            UnityEditor.Handles.Label(lastCastOrigin, "Cast Origin");
        }

        // --- Contact ---
        if (lastHadHit)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(lastHitPoint, 0.05f);
            Gizmos.DrawRay(lastHitPoint, lastHitNormal * 0.3f);
            UnityEditor.Handles.Label(lastHitPoint, "Contact");
        }

        // --- Final camera position ---
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(correctedPosition, 0.03f);
        UnityEditor.Handles.Label(correctedPosition, "Camera Position");

        // --- Boom line ---
        Gizmos.color = hadContact ? Color.red :
                       didAnyProbesHit ? Color.yellow :
                       Color.gray;
        Gizmos.DrawLine(pivotPos, correctedPosition);

        // --- HUD ---
        if (showHUD)
        {
            string stateText = hadContact ? "Contracting" :
                               didAnyProbesHit ? "Probing" : "Free";
            float pct = (maxBoom > 1e-6f) ? (currentBoom / maxBoom) * 100f : 0f;

            string hud = $"State: {stateText}\n" +
                         $"Boom: {currentBoom:0.00}/{maxBoom:0.00} ({pct:0.#}%)";

            if (lastHadHit)
                hud += $"\nHit: {Vector3.Distance(pivotPos, lastHitPoint):0.00} m";

            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(pivotPos + Vector3.up * 0.25f, hud);
        }
    }
#endif
}
