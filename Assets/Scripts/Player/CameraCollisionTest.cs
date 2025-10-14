using UnityEngine;

public class CameraCollisionTest : MonoBehaviour
{
    public LayerMask mask;
    public float radius = 0.3f;
    public float distance = 5f;

    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 dir = transform.forward;

        if (Physics.SphereCast(origin, radius, dir, out var hit, distance, mask, QueryTriggerInteraction.Collide))
        {
            Debug.Log($"✅ Hit: {hit.collider.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            Debug.DrawLine(origin, hit.point, Color.green);
        }
        else
        {
            Debug.DrawLine(origin, origin + dir * distance, Color.red);
        }
    }
}
