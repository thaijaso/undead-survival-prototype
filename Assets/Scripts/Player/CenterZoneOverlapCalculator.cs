using UnityEngine;

[DisallowMultipleComponent]
public class CenterZoneOverlapCalculator : MonoBehaviour
{
    [Header("Camera")]
    public Camera renderCam; // if null, falls back to Camera.main

    [Header("Detection (Mask-Only)")]
    [Tooltip("Set this to a dedicated layer that only the player's fade-colliders use (e.g., 'PlayerFade').")]
    public LayerMask playerFadeMask;
    [Tooltip("Sphere radius in meters for the center-ray test.")]
    public float sphereRadius = 0.2f;
    [Tooltip("How far to check from the camera (use your boom length or a bit more).")]
    public float maxDistance = 3f;
    public QueryTriggerInteraction triggerMode = QueryTriggerInteraction.Ignore;

    [Header("Output")]
    [Tooltip("True if the center ray hits the player's fade layer within maxDistance.")]
    public bool isPlayerBlockingView;

    [Header("Ray Offsets")]
    [Tooltip("Offset the ray origin backwards along the view direction to avoid hitting very close objects.")]
    public float backOffset = .1f;

    [Header("Debug")]
    public bool showDebug = true;

    void Awake()
    {
        if (!renderCam) renderCam = Camera.main;
    }

    void LateUpdate()
    {
        isPlayerBlockingView = CheckIfPlayerBlocksCenter();
    }

    public bool CheckIfPlayerBlocksCenter()
    {
        if (!renderCam) return false;

        Vector3 center = new Vector3(0.5f, 0.5f, 0f);
        Ray ray = renderCam.ViewportPointToRay(center);
        ray.origin -= renderCam.transform.forward * backOffset;

        return Physics.SphereCast(ray, sphereRadius, out _, maxDistance, playerFadeMask, triggerMode);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;
        Camera cam = renderCam ? renderCam : Camera.main;
        if (!cam) return;

        Vector3 center = new Vector3(0.5f, 0.5f, 0f);
        Ray ray = renderCam.ViewportPointToRay(center);
        ray.origin -= renderCam.transform.forward * backOffset;
        float radius = Mathf.Max(0f, sphereRadius);
        float length = Mathf.Max(0f, maxDistance);

        bool hit = Physics.SphereCast(ray, radius, out RaycastHit hitInfo, length, playerFadeMask, triggerMode);

        // draw main ray
        Gizmos.color = hit ? Color.red : Color.green;
        Vector3 end = hit ? ray.origin + ray.direction * hitInfo.distance : ray.origin + ray.direction * length;
        Gizmos.DrawLine(ray.origin, end);

        // draw remaining clear segment if we hit
        if (hit)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.6f);
            Gizmos.DrawLine(end, ray.origin + ray.direction * length);

            // hit point + normal
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 1f);
            Gizmos.DrawWireSphere(hitInfo.point, Mathf.Max(0.03f, radius * 0.25f));
            Gizmos.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal * Mathf.Max(0.25f, radius));
        }

        // swept sphere volume (start/end caps + rails)
        DrawSweptSphereGizmo(ray.origin, ray.origin + ray.direction * length, ray.direction, radius, new Color(1f, 1f, 1f, 0.5f));
    }

    // helper: visualize spherecast path
    void DrawSweptSphereGizmo(Vector3 start, Vector3 end, Vector3 dir, float radius, Color col)
    {
        if (radius <= 0f) return;
        Gizmos.color = col;

        // endcap spheres
        Gizmos.DrawWireSphere(start, radius);
        Gizmos.DrawWireSphere(end, radius);

        // build basis around direction
        Vector3 d = dir.normalized;
        Vector3 up = Mathf.Abs(Vector3.Dot(d, Vector3.up)) > 0.95f ? Vector3.right : Vector3.up;
        Vector3 right = Vector3.Cross(d, up).normalized;
        Vector3 fwd   = Vector3.Cross(right, d).normalized;

        // rails
        Gizmos.DrawLine(start + right * radius, end + right * radius);
        Gizmos.DrawLine(start - right * radius, end - right * radius);
        Gizmos.DrawLine(start + fwd   * radius, end + fwd   * radius);
        Gizmos.DrawLine(start - fwd   * radius, end - fwd   * radius);
    }
#endif
}
