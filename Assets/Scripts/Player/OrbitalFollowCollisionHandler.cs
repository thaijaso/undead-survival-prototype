using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("")]
public class OrbitalFollowCollision : CinemachineExtension
{
    [Header("Debug")]
    public bool showDebug = true;
    public bool showHUD = true; // ✅ Toggle for the on-screen debug overlay

    [Header("Collision")]
    public LayerMask collisionMask;
    [Range(0.05f, 1f), HideInInspector] public float sphereCastRadius = 0.341f;
    public bool useSphereCast = true;

    [Header("Boom Settings")]
    public float minBoom = 0.5f;
    public float maxBoom = 2f;
    [Tooltip("Smooth speed for boom contraction/expansion.")]
    public float boomSmooth = 14f;
    [Tooltip("How far the camera stays off walls.")]
    public float wallBackoff = 0.2f;

    private CinemachineOrbitalFollow orbitalFollow;
    private Vector3 desiredPosition;
    private Vector3 correctedPosition;

    private Vector3 previousDirection;
    private bool didAnyProbesHit;
    private Vector3 pivotPosition;
    private float currentBoom;
    private bool initialized;
    private bool hadContact;

    [Header("Offsets")]
    [SerializeField, Range(0f, 0.3f)] private float startSkin = 0.05f;
    [SerializeField, Range(0f, 0.5f)] private float backoffStep = 0.1f;

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
        correctedPosition = transform.position; // ✅ safe fallback
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
            Vector3 camPos   = state.GetFinalPosition();

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
        bool nearbyCollision = false;

        Vector3 probeDir = (desiredPosition - pivotPosition).normalized;

        // ✅ Phase 1: Early proximity probe (detect nearby geometry)
        if (Physics.CheckSphere(pivotPosition + probeDir * (maxBoom * 0.5f),
            sphereCastRadius * 1.25f, collisionMask, QueryTriggerInteraction.Ignore))
        {
            nearbyCollision = true;
            didAnyProbesHit = true;
        }

        bool pivotInside = Physics.CheckSphere(pivotPosition, sphereCastRadius * 0.75f,
            collisionMask, QueryTriggerInteraction.Ignore);
        if (pivotInside)
            nearbyCollision = true;

        // 🧭 Debug visualization
        if (showDebug)
        {
            Color c = nearbyCollision ? (pivotInside ? Color.red : Color.yellow) : Color.gray;
            Debug.DrawRay(pivotPosition, probeDir * maxBoom, c);
        }

        // 🚨 Case 1: Pivot inside geometry
        if (pivotInside)
        {
            correctedPosition = pivotPosition - probeDir * (sphereCastRadius + wallBackoff);
            currentBoom = minBoom;
            UpdateBoomState();
            return;
        }

        // 🟢 Case 2: No nearby collision — free space
        if (!nearbyCollision)
        {
            float desiredLen = Mathf.Clamp(Vector3.Distance(pivotPosition, desiredPosition), minBoom, maxBoom);
            currentBoom = Mathf.Lerp(currentBoom, desiredLen, deltaTime * boomSmooth * 0.5f);
            correctedPosition = pivotPosition + probeDir * currentBoom;
            Debug.DrawLine(pivotPosition, correctedPosition, Color.blue);
            UpdateBoomState();
            return;
        }

        // 🟡 Case 3: Probing (nearby collision but not yet in contact)
        // Keep origin hugging the pivot while probing; don't offset it yet
        Vector3 dir = probeDir;
        float startOffset = sphereCastRadius + startSkin + backoffStep;
        Vector3 baselineOrigin = pivotPosition - dir * (sphereCastRadius * 0.5f);
        lastCastOrigin = baselineOrigin;

        float castDist = maxBoom + startOffset + 0.05f;
        RaycastHit hit;

        bool gotHit = useSphereCast
            ? Physics.SphereCast(lastCastOrigin, sphereCastRadius, dir, out hit, castDist, collisionMask, QueryTriggerInteraction.Ignore)
            : Physics.Raycast(lastCastOrigin, dir, out hit, castDist, collisionMask, QueryTriggerInteraction.Ignore);

        if (gotHit)
        {
            // 🟣 Only now — when we have a hit — push the origin back slightly
            lastCastOrigin = pivotPosition - dir * startOffset;

            Debug.DrawRay(hit.point, hit.normal * 0.3f, Color.magenta);
            if (showDebug)
                Debug.Log($"[Cast HIT] {hit.collider.name} dist={hit.distance:0.###}");

            Vector3 contactPoint = hit.point + hit.normal * (sphereCastRadius + wallBackoff);
            float contactDist = Mathf.Max(minBoom, Vector3.Distance(pivotPosition, contactPoint));
            float contractedTarget = Mathf.Clamp(contactDist, minBoom, maxBoom);

            currentBoom = Mathf.Lerp(currentBoom, contractedTarget, deltaTime * boomSmooth * 4f);
            hadContact = true;
            lastHadHit = true;
            lastHitPoint = hit.point;
            lastHitNormal = hit.normal;
        }
        else
        {
            // 🟢 No hit — stay in probing mode but origin remains near pivot
            lastHadHit = false;
            float expandedTarget = Mathf.Clamp(maxBoom, minBoom, maxBoom);
            currentBoom = Mathf.Lerp(currentBoom, expandedTarget, deltaTime * boomSmooth * 0.5f);
        }

        currentBoom = Mathf.Clamp(currentBoom, minBoom, maxBoom);
        correctedPosition = pivotPosition + dir * currentBoom;
        Debug.DrawLine(pivotPosition, correctedPosition, hadContact ? Color.red : Color.yellow);

        UpdateBoomState();
    }

    private void UpdateBoomState()
    {
        // --- Update boom state ---
        float diff = currentBoom - prevBoom;
        float eps = 0.001f; // small tolerance

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

        // --- Contact (only if hit this frame) ---
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
