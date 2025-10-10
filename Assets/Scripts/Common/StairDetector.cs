using UnityEngine;

// TODO: refactor WallDector and StairDector to share common base class LayerDetector
public class StairDetector : MonoBehaviour
{
    [Tooltip("Maximum distance for wall detection (meters)")]
    public float MaxDistance = 1.5f;

    [Tooltip("Layers considered as stairs. By default set to the layer named 'Stair' if it exists.")]
    public LayerMask StairLayerMask;

    [Tooltip("Optional offset from the transform position to start the raycast (local space)")]
    public Vector3 OriginOffset = Vector3.zero;

    [Tooltip("If enabled, draws debug rays in the Scene and Game views")]
    public bool DrawDebug = true;

    // Public read-only state (use serialized backing fields so values show in the Inspector)
    [SerializeField, Tooltip("Read-only (runtime)")]
    private bool isStairDetected;
    public bool IsStairDetected => isStairDetected;

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
        int mask = LayerMask.GetMask("Stair");
        if (mask != 0)
        {
            StairLayerMask = mask;
        }
        else
        {
            // Fallback: if a layer named "Stair" exists, use its index safely; otherwise default to everything
            int stairLayer = LayerMask.NameToLayer("Stair");
            if (stairLayer >= 0)
                StairLayerMask = 1 << stairLayer;
            else
                StairLayerMask = ~0; // default to everything if the layer doesn't exist
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 origin = transform.TransformPoint(OriginOffset);
        // Use the local "down" direction (negative of the transform's up vector)
        Vector3 direction = -transform.up;

        isStairDetected = Physics.Raycast(origin, direction, out RaycastHit hit, MaxDistance, StairLayerMask.value);

        if (isStairDetected)
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
        Vector3 origin = transform.TransformPoint(OriginOffset);
        Vector3 direction = -transform.up;
        Gizmos.color = isStairDetected ? Color.red : Color.green;
        Gizmos.DrawLine(origin, origin + direction * MaxDistance);
        float hitDistance = isStairDetected ? Vector3.Distance(origin, lastHitPoint) : MaxDistance;
        Gizmos.DrawWireSphere(origin + direction * Mathf.Min(MaxDistance, hitDistance), 0.05f);
    }
}
