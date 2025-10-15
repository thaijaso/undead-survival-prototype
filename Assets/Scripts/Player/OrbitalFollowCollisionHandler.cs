using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("")]
public class OrbitalFollowCollision_Fixed : CinemachineExtension
{
    [Header("Collision")]
    public LayerMask collisionMask;
    [Range(0.05f, 1f)] public float sphereCastRadius = 0.3f;
    public bool useSphereCast = true;

    [Header("Boom Settings")]
    public float minBoom = 0.5f;
    public float maxBoom = 2f;
    [Tooltip("Smoothing speed for boom changes")]
    public float boomSmooth = 10f;
    [Tooltip("How far off the wall to keep the camera")]
    public float wallBackoff = 0.20f;

    private CinemachineOrbitalFollow orbitalFollow;

    // boom smoothing (we keep our own boom, don't touch OrbitalFollow.Radius)
    private float currentBoom;
    private bool initialized = false;

    // state cooldown to prevent hit/miss flapping
    private float insideCooldown = 0f;

    // distance buffer (temporal averaging)
    private const int bufferSize = 5;
    private readonly float[] distBuffer = new float[bufferSize];
    private int bufferIndex = 0;
    private bool bufferFilled = false;

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
        // 🔑 Do collision at Body stage so Aim/Noise apply AFTER our correction
        if (stage != CinemachineCore.Stage.Body || orbitalFollow == null)
            return;

        // --- read current orbit ---
        Vector3 pivotPos   = orbitalFollow.FollowTargetPosition;  // follow target/pivot
        Vector3 desiredPos = state.RawPosition;                   // camera pos computed this frame by OrbitalFollow (Body)
        // Optional: account for CameraOffset (if one exists on the same vcam)
        var offsetComp = vcam.GetComponent<CinemachineCameraOffset>();
        if (offsetComp != null)
        {
            Vector3 localOffset = new Vector3(offsetComp.Offset.x, offsetComp.Offset.y, offsetComp.Offset.z);
            desiredPos += state.RawOrientation * localOffset; // apply local offset in world space
        }
        Vector3 camDir     = (desiredPos - pivotPos).normalized;
        float   desiredLen = Vector3.Distance(pivotPos, desiredPos);

        if (!initialized)
        {
            currentBoom = maxBoom; // start fully extended
            initialized = true;
        }

        float castLength = desiredLen;
        Vector3 castOrigin = pivotPos;

        // --- collision checks ---
        bool insideHit = Physics.CheckSphere(castOrigin, sphereCastRadius, collisionMask);

        bool sweepHit = false;
        RaycastHit hitInfo = new RaycastHit();

        if (!insideHit)
        {
            if (useSphereCast)
                sweepHit = Physics.SphereCast(
                    castOrigin, sphereCastRadius, camDir, out hitInfo,
                    castLength, collisionMask, QueryTriggerInteraction.Ignore);
            else
                sweepHit = Physics.Raycast(
                    castOrigin, camDir, out hitInfo,
                    castLength, collisionMask, QueryTriggerInteraction.Ignore);
        }

        // ✅ safer cooldown decay that handles 0 deltaTime frames
        float dt = Mathf.Max(Time.deltaTime, 0.001f);
        if (insideHit || sweepHit)
            insideCooldown = 0.12f;
        else if (insideCooldown > 0f)
            insideCooldown = Mathf.Max(0f, insideCooldown - dt);
        else
            insideCooldown = 0f;


        bool effectiveHit = insideHit || sweepHit || insideCooldown > 0f;

        // --- debug (optional) ---
        Color lineColor = effectiveHit ? Color.green : Color.red;
        Debug.DrawRay(castOrigin, camDir * castLength, lineColor);
        if (sweepHit) Debug.DrawLine(castOrigin, hitInfo.point, Color.cyan);
        else if (insideHit) Debug.DrawRay(castOrigin, camDir * 0.1f, Color.yellow);

        // --- boom solve (buffered + contract-only) ---
        if (sweepHit)
        {
            float rawDist = hitInfo.distance;

            // ring buffer
            distBuffer[bufferIndex] = rawDist;
            bufferIndex = (bufferIndex + 1) % bufferSize;
            if (bufferIndex == 0) bufferFilled = true;

            int count = bufferFilled ? bufferSize : bufferIndex;
            float avgDist = 0f; for (int i = 0; i < count; i++) avgDist += distBuffer[i];
            avgDist /= Mathf.Max(1, count);

            float contractedTarget = Mathf.Clamp(avgDist - wallBackoff, minBoom, maxBoom);
            float targetBoom = Mathf.Min(contractedTarget, currentBoom); // contract-only while colliding
            currentBoom = Mathf.Lerp(currentBoom, targetBoom, deltaTime * boomSmooth);
        }
        else if (insideHit)
        {
            float targetBoom = minBoom + 0.05f;                     // push out of geometry
            currentBoom = Mathf.Lerp(currentBoom, targetBoom, deltaTime * (boomSmooth * 2f));
        }
        else if (effectiveHit)
        {
            // during cooldown: hold boom, no expansion yet
        }
        else
        {
            // free: expand smoothly
            currentBoom = Mathf.Lerp(currentBoom, Mathf.Clamp(desiredLen, minBoom, maxBoom), deltaTime * (boomSmooth * 0.5f));
        }

        currentBoom = Mathf.Clamp(currentBoom, minBoom, maxBoom);



        // --- write corrected camera position into the state (NOT into OrbitalFollow.Radius) ---
        Vector3 correctedPos = pivotPos + camDir * currentBoom;
        state.RawPosition = correctedPos;

        // optional log
        Debug.Log($"[Boom] insideHit={insideHit}, sweepHit={sweepHit}, cooldown={insideCooldown:F2}, len={desiredLen:F2}, boom={currentBoom:F2}");
    }
}
