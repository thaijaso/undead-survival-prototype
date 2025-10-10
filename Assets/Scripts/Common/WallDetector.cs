using UnityEngine;

/// <summary>
/// Casts a ray forward from the GameObject and detects colliders on the "Wall" layer.
/// Exposes distance and LayerMask for tuning. Provides a read-only property to check detection.
/// </summary>
public class WallDetector : MonoBehaviour
{
    [Tooltip("Maximum distance for wall detection (meters)")]
    public float maxDistance = 1.5f;

    [Tooltip("Layers considered as walls. By default set to the layer named 'Wall' if it exists.")]
    public LayerMask wallLayerMask;

    [Tooltip("Optional offset from the transform position to start the raycast (local space)")]
    public Vector3 originOffset = Vector3.zero;

    [Tooltip("If enabled, draws debug rays in the Scene and Game views")]
    public bool drawDebug = true;

    // Public read-only state (use serialized backing fields so values show in the Inspector)
    [SerializeField, Tooltip("Read-only (runtime)")]
    private bool isWallDetected;
    public bool IsWallDetected => isWallDetected;

    // Inspector-friendly last-hit info (RaycastHit isn't nicely serialized for display)
    [SerializeField, Tooltip("Read-only (runtime) - last hit point")]
    private Vector3 lastHitPoint;

    [SerializeField, Tooltip("Read-only (runtime) - last hit collider name")]
    private string lastHitColliderName = string.Empty;

    // Keep full RaycastHit for runtime access, but it's not serialized for the inspector
    public RaycastHit LastHit { get; private set; }

    void Reset()
    {
        // Prefer using LayerMask.GetMask which accepts layer names and returns a safe mask (0 if not found)
        int mask = LayerMask.GetMask("Wall");
        if (mask != 0)
        {
            wallLayerMask = mask;
        }
        else
        {
            // Fallback: if a layer named "Wall" exists, use its index safely; otherwise default to everything
            int wallLayer = LayerMask.NameToLayer("Wall");
            if (wallLayer >= 0)
                wallLayerMask = 1 << wallLayer;
            else
                wallLayerMask = ~0; // default to everything if the layer doesn't exist
        }
    }

    void Update()
    {
        Vector3 origin = transform.TransformPoint(originOffset);
        Vector3 direction = transform.forward;

        isWallDetected = Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, wallLayerMask.value);

        if (isWallDetected)
        {
            LastHit = hit;
            lastHitPoint = hit.point;
            lastHitColliderName = hit.collider ? hit.collider.name : string.Empty;
        }
        else
        {
            LastHit = default;
            lastHitPoint = Vector3.zero;
            lastHitColliderName = string.Empty;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!drawDebug) return;
        Vector3 origin = transform.TransformPoint(originOffset);
        Vector3 dir = transform.forward;
        Gizmos.color = isWallDetected ? Color.red : Color.green;
        Gizmos.DrawLine(origin, origin + dir * maxDistance);
        float hitDistance = isWallDetected ? Vector3.Distance(origin, lastHitPoint) : maxDistance;
        Gizmos.DrawWireSphere(origin + dir * Mathf.Min(maxDistance, hitDistance), 0.05f);
    }
}
