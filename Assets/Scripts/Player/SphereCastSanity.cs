using UnityEngine;

public class SphereCastSanity : MonoBehaviour
{
    public LayerMask mask;
    public Transform target;
    public float radius = 0.3f;
    public float distance = 5f;

    void Update()
    {
        Vector3 dir = (transform.position - target.position).normalized;
        Vector3 origin = target.position;
        bool hit = Physics.SphereCast(origin, radius, dir, out RaycastHit info, distance, mask, QueryTriggerInteraction.Collide);

        Debug.DrawLine(origin, origin + dir * distance, hit ? Color.green : Color.red);
        if (hit) Debug.Log($"✅ Hit {info.collider.name} (layer {LayerMask.LayerToName(info.collider.gameObject.layer)})");
    }
}