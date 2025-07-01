using UnityEngine;

public class BulletHitscan : MonoBehaviour
{
    [SerializeField] private LayerMask hitMask;

    public bool Fire(Vector3 origin, Vector3 direction, float range, out RaycastHit hit)
    {
        if (Physics.Raycast(origin, direction, out hit, range, hitMask))
        {
            Debug.Log($"[{gameObject.name}] BulletHitscan.Fire(): Hit {hit.collider.name}");
            return true;
        }

        Debug.Log($"[{gameObject.name}] BulletHitscan.Fire(): No hit detected");
        return false;
    }
}
