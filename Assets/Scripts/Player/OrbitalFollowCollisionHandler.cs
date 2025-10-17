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

    //--------------------------------------------------------------------
    // 1️⃣ Direction: pivot → camera
    //--------------------------------------------------------------------
    Vector3 offset = desiredPosition - pivotPosition;
    Vector3 dir = offset.sqrMagnitude > 1e-6f
        ? offset.normalized
        : (previousDirection == Vector3.zero ? -transform.forward : previousDirection);
    previousDirection = dir;

    //--------------------------------------------------------------------
    // 2️⃣ Define cast origin (behind pivot, toward camera)
    //--------------------------------------------------------------------
    float originBackoff = Mathf.Max(sphereCastRadius * 0.5f, sphereCastRadius + startSkin + backoffStep);
    lastCastOrigin = pivotPosition + dir * originBackoff; // ✅ behind pivot now

    bool nearbyCollision = false;

    //--------------------------------------------------------------------
    // 3️⃣ Quick proximity probes
    //--------------------------------------------------------------------
    if (Physics.CheckSphere(pivotPosition + dir * (maxBoom * 0.5f),
        sphereCastRadius * 1.25f, collisionMask, QueryTriggerInteraction.Ignore))
    {
        nearbyCollision = true;
        didAnyProbesHit = true;
    }

    bool pivotInside = Physics.CheckSphere(pivotPosition, sphereCastRadius * 0.75f,
        collisionMask, QueryTriggerInteraction.Ignore);
    if (pivotInside) nearbyCollision = true;

    if (showDebug)
    {
        Color c = nearbyCollision ? (pivotInside ? Color.red : Color.yellow) : Color.gray;
        Debug.DrawRay(pivotPosition, dir * maxBoom, c);
    }

    //--------------------------------------------------------------------
    // 4️⃣ If pivot starts inside geometry
    //--------------------------------------------------------------------
    if (pivotInside)
    {
        correctedPosition = pivotPosition - dir * (sphereCastRadius + wallBackoff);
        currentBoom = minBoom;
        UpdateBoomState();
        return;
    }

    //--------------------------------------------------------------------
    // 5️⃣ Free space: expand or stabilize
    //--------------------------------------------------------------------
    if (!nearbyCollision)
    {
        float desiredLen = Mathf.Clamp(Vector3.Distance(pivotPosition, desiredPosition), minBoom, maxBoom);
        currentBoom = Mathf.Lerp(currentBoom, desiredLen, deltaTime * boomSmooth * 0.5f);
        correctedPosition = pivotPosition + dir * currentBoom;
        Debug.DrawLine(pivotPosition, correctedPosition, Color.blue);
        UpdateBoomState();
        return;
    }

    //--------------------------------------------------------------------
    // 6️⃣ Main collision cast (origin hugs pivot)
    //--------------------------------------------------------------------
    float startOffset = sphereCastRadius * 0.5f;   // ✅ keep cast origin very close to pivot
    lastCastOrigin = pivotPosition - dir * startOffset;  // ✅ now hugs pivot (pivot → camera direction)
    float castDist = maxBoom + startOffset + 0.05f;

    // If the origin happens to start inside geometry, nudge it slightly backward
    for (int i = 0; i < 3; i++)
    {
        if (!Physics.CheckSphere(lastCastOrigin, sphereCastRadius, collisionMask, QueryTriggerInteraction.Ignore))
            break;
        lastCastOrigin -= dir * (sphereCastRadius * 0.25f);
    }

    //--------------------------------------------------------------------
    // 7️⃣ Sphere cast for obstacles
    //--------------------------------------------------------------------
    RaycastHit hit;
    bool gotHit = useSphereCast
        ? Physics.SphereCast(lastCastOrigin, sphereCastRadius, dir, out hit, castDist, collisionMask, QueryTriggerInteraction.Ignore)
        : Physics.Raycast(lastCastOrigin, dir, out hit, castDist, collisionMask, QueryTriggerInteraction.Ignore);

    if (gotHit)
    {
        Debug.DrawRay(hit.point, hit.normal * 0.3f, Color.magenta);

        Vector3 contactPoint = hit.point + hit.normal * (sphereCastRadius + wallBackoff);
        float contactDist = Mathf.Max(minBoom, Vector3.Distance(pivotPosition, contactPoint));
        float contractedTarget = Mathf.Clamp(contactDist, minBoom, maxBoom);

        if (showDebug)
            Debug.Log($"[Boom Contract] hit={hit.collider.name} contactDist={contactDist:0.###} target={contractedTarget:0.###}");

        currentBoom = Mathf.Lerp(currentBoom, contractedTarget, deltaTime * boomSmooth * 4f);
        hadContact = true;
        lastHitPoint = hit.point;
        lastHitNormal = hit.normal;
        lastHadHit = true;
    }
    else
    {
        lastHadHit = false;
        if (showDebug)
            Debug.Log("[Cast MISS]");
    }

    //--------------------------------------------------------------------
    // 8️⃣ Expansion (if no collision)
    //--------------------------------------------------------------------
    if (!gotHit)
    {
        float expandedTarget = Mathf.Clamp(maxBoom, minBoom, maxBoom);
        currentBoom = Mathf.Lerp(currentBoom, expandedTarget, deltaTime * boomSmooth * 0.5f);
    }

    //--------------------------------------------------------------------
    // 9️⃣ Final clamp + position update
    //--------------------------------------------------------------------
    currentBoom = Mathf.Clamp(currentBoom, minBoom, maxBoom);
    correctedPosition = pivotPosition + dir * currentBoom;
    Debug.DrawLine(pivotPosition, correctedPosition, hadContact ? Color.red : Color.blue);

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

    private Color GetDebugColor()
    {
        if (lastHadHit) return Color.red;
        if (hadContact) return Color.yellow;
        if (didAnyProbesHit) return Color.white;
        return Color.gray;
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

        Color fadeCol = GetDebugColor();
        Gizmos.color = fadeCol;
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

        // ✅ HUD Overlay (Scene View only)
        if (showHUD)
        {
            string state = boomState;
            Color textColor =
                state == "Contracting" ? Color.red :
                state == "Expanding"   ? Color.green :
                Color.cyan;


            Vector3 hudPos = pivotPos + Vector3.up * 0.25f;
            float percent = (currentBoom / maxBoom) * 100f;
            string hudText = $"State: {state}\n" +
                             $"Boom: {currentBoom:0.00}/{maxBoom:0.00} ({percent:0.#}%)\n";

            if (lastHadHit)
                hudText += $"Hit: {lastHitPoint.magnitude:0.00} → {lastHitPoint}";

            UnityEditor.Handles.color = textColor;
            UnityEditor.Handles.Label(hudPos, hudText);
        }
    }
#endif
}
