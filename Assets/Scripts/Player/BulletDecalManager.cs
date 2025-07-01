using UnityEngine;

/// <summary>
/// This class manages spawning the correct decal based on the hit surface.
public class BulletDecalManager : MonoBehaviour
{
    public static BulletDecalManager Instance { get; private set; }

    [System.Serializable]
    public struct MaterialDecalPair
    {
        public PhysicsMaterial material;
        public GameObject[] bulletDecalPrefabs;
    }

    public MaterialDecalPair[] materialDecals;
    public GameObject defaultDecal;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[{gameObject.name}] BulletDecalManager.Awake(): Multiple BulletDecalManager instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SpawnBulletDecal(RaycastHit hit)
    {
        string matName = hit.collider.sharedMaterial ? hit.collider.sharedMaterial.name : "";
        GameObject prefab = defaultDecal;

        foreach (var entry in materialDecals)
        {
            if (entry.material && entry.material.name == matName)
            {
                // Randomly select a decal from the array
                if (entry.bulletDecalPrefabs.Length > 0)
                {
                    prefab = entry.bulletDecalPrefabs[Random.Range(0, entry.bulletDecalPrefabs.Length)];
                }
                break;
            }
        }

        if (prefab)
        {
            GameObject decal = Instantiate(prefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            decal.transform.SetParent(hit.collider.transform); // Optional: parent to surface
        }
    }

    public void SpawnBulletDecal(ContactPoint contact)
    {
        string matName = contact.otherCollider.sharedMaterial ? contact.otherCollider.sharedMaterial.name : "";
        GameObject prefab = defaultDecal;

        foreach (var entry in materialDecals)
        {
            if (entry.material && entry.material.name == matName)
            {
                // Randomly select a decal from the array
                if (entry.bulletDecalPrefabs.Length > 0)
                {
                    prefab = entry.bulletDecalPrefabs[Random.Range(0, entry.bulletDecalPrefabs.Length)];
                }
                break;
            }
        }

        if (prefab)
        {
            GameObject decal = Instantiate(prefab, contact.point, Quaternion.FromToRotation(Vector3.up, contact.normal));
            decal.transform.SetParent(contact.otherCollider.transform); // Optional: parent to surface
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
