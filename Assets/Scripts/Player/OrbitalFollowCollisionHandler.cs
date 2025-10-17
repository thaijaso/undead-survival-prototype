using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("")]
public class OrbitalFollowCollision : CinemachineExtension
{
    [Header("Debug")]
    public bool showDebug = true;

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
    [Range(0, 12)] public int sweepSamples = 3;

    private CinemachineOrbitalFollow orbitalFollow;
    private Vector3 desiredPosition;
    private Vector3 correctedPosition;
    private Vector3 useDirection;
    private Vector3 previousDirection;
    private bool didAnyProbesHit;
    private float nearestDistance = float.PositiveInfinity;
    private Vector3 pivotPosition;
    private float currentBoom;
    private bool initialized;
    private float insideCooldown;

    private const int bufferSize = 4;
    private readonly float[] distBuffer = new float[bufferSize];
    private int bufferIndex;
    private bool bufferFilled;

    private Vector3 lastNormal;
    private bool hadContact;

    [Header("Offsets")]
    [SerializeField, Range(0f, 0.3f)] private float startSkin = 0.05f;
    [SerializeField, Range(0f, 0.5f)] private float backoffStep = 0.1f;

    // Debug fields
    public Vector3 lastCastOrigin;
    public Vector3 lastHitPoint;
    public Vector3 lastHitNormal;
    public bool lastHadHit;

    bool dbgGrazeActive;
    Vector3 dbgGrazePos;
    float dbgGrazeRadius;
    Vector3 dbgGrazeLineA, dbgGrazeLineB;

    protected override void Awake()
    {
        base.Awake();
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
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
            bufferFilled = false;
            bufferIndex = 0;
            return;
        }

        pivotPosition = orbitalFollow.FollowTargetPosition;
        desiredPosition = state.RawPosition;
        Vector3 probeDirection = (desiredPosition - pivotPosition).sqrMagnitude > 1e-6f
            ? (desiredPosition - pivotPosition).normalized
            : (previousDirection == Vector3.zero ? Vector3.back : previousDirection);

        if (!initialized)
        {
            currentBoom = maxBoom;
            previousDirection = probeDirection;
            initialized = true;
        }

        bool insideHit = Physics.CheckSphere(pivotPosition, sphereCastRadius, collisionMask);
        didAnyProbesHit = false;

        useDirection = probeDirection;
        previousDirection = probeDirection;

        HandleBoom(deltaTime, false, insideHit);

        // Apply final position
        state.RawPosition = correctedPosition;
    }

    // -----------------------------------------------------------------------
    // MAIN LOGIC
    // -----------------------------------------------------------------------
    private void HandleBoom(float deltaTime, bool effectiveHit, bool insideHit)
    {
        dbgGrazeActive = false;
        hadContact = false;

        // --------------------------------------------------------------------
        // --- PHASE 1: Quick probe before main collision logic ---------------
        // --------------------------------------------------------------------
        didAnyProbesHit = false;
        bool nearbyCollision = false;

        Vector3 probeDir = (desiredPosition - pivotPosition).normalized;
        float probeRange = maxBoom + sphereCastRadius;

        if (Physics.CheckSphere(pivotPosition + probeDir * (maxBoom * 0.5f),
            sphereCastRadius * 1.25f, collisionMask, QueryTriggerInteraction.Ignore))
        {
            nearbyCollision = true;
            didAnyProbesHit = true;
        }

        bool pivotInside = Physics.CheckSphere(pivotPosition, sphereCastRadius * 0.75f, collisionMask, QueryTriggerInteraction.Ignore);
        if (pivotInside)
        {
            insideHit = true;
            nearbyCollision = true;
        }

        if (showDebug)
        {
            Color c = nearbyCollision ? (pivotInside ? Color.red : Color.yellow) : Color.gray;
            Debug.DrawRay(pivotPosition, probeDir * maxBoom, c);
            Debug.Log($"[ProbePhase] nearby={nearbyCollision} inside={pivotInside} didAnyProbesHit={didAnyProbesHit}");
        }

        if (!nearbyCollision)
        {
            float desiredLen = Mathf.Clamp(Vector3.Distance(pivotPosition, desiredPosition), minBoom, maxBoom);
            currentBoom = Mathf.Lerp(currentBoom, desiredLen, deltaTime * boomSmooth * 0.5f);
            correctedPosition = pivotPosition + probeDir * currentBoom;
            Debug.DrawLine(pivotPosition, correctedPosition, Color.blue);
            return;
        }

        // --------------------------------------------------------------------
        // --- PHASE 2: Main collision handling -------------------------------
        // --------------------------------------------------------------------

        Vector3 dir = probeDir;
        float startOffset = sphereCastRadius + startSkin + backoffStep;
        lastCastOrigin = pivotPosition - dir * startOffset;
        float castDist = maxBoom + startOffset + 0.05f;

        // Ensure start not inside
        for (int i = 0; i < 3; i++)
        {
            if (!Physics.CheckSphere(lastCastOrigin, sphereCastRadius, collisionMask, QueryTriggerInteraction.Ignore))
                break;
            lastCastOrigin -= dir * (sphereCastRadius * 0.25f);
        }

        // --- Perform main cast ---
        bool gotHit = useSphereCast
            ? Physics.SphereCast(lastCastOrigin, sphereCastRadius, dir, out RaycastHit hit, castDist, collisionMask, QueryTriggerInteraction.Ignore)
            : Physics.Raycast(lastCastOrigin, dir, out hit, castDist, collisionMask, QueryTriggerInteraction.Ignore);

        if (gotHit)
        {
            Debug.DrawRay(hit.point, hit.normal * 0.3f, Color.magenta);
            Debug.Log($"[Cast HIT] {hit.collider.name} dist={hit.distance:0.###} normal={hit.normal}");

            // Compute target distance with radius and wallBackoff
            Vector3 contactPoint = hit.point + hit.normal * (sphereCastRadius + wallBackoff);
            float contactDist = Mathf.Max(minBoom, Vector3.Distance(pivotPosition, contactPoint));
            float contractedTarget = Mathf.Clamp(contactDist, minBoom, maxBoom);

            Debug.Log($"[Boom Contract] contactDist={contactDist:0.###} → target={contractedTarget:0.###}");

            currentBoom = Mathf.Lerp(currentBoom, contractedTarget, deltaTime * boomSmooth * 4f);
            hadContact = true;
            lastHitPoint = hit.point;
            lastHitNormal = hit.normal;
            lastHadHit = true;
        }
        else
        {
            lastHadHit = false;
            Debug.Log("[Cast MISS]");
        }

        // --- Expansion logic ---
        if (!gotHit)
        {
            float expandedTarget = Mathf.Clamp(maxBoom, minBoom, maxBoom);
            currentBoom = Mathf.Lerp(currentBoom, expandedTarget, deltaTime * boomSmooth * 0.5f);
        }

        currentBoom = Mathf.Clamp(currentBoom, minBoom, maxBoom);
        correctedPosition = pivotPosition + dir * currentBoom;
        Debug.DrawLine(pivotPosition, correctedPosition, Color.blue);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!enabled) return;
        if (orbitalFollow == null)
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow == null) return;

        Vector3 pivotPos = orbitalFollow.FollowTargetPosition;
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(pivotPos, 0.025f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lastCastOrigin, sphereCastRadius);
        UnityEditor.Handles.Label(lastCastOrigin, "Cast Origin");

        if (lastHadHit)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(lastHitPoint, 0.05f);
            Gizmos.DrawRay(lastHitPoint, lastHitNormal * 0.3f);
            UnityEditor.Handles.Label(lastHitPoint, "Contact");
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(correctedPosition, 0.03f);
        UnityEditor.Handles.Label(correctedPosition, "Camera Position");
    }
#endif
}
