using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector]
    public float impactForce = 10f; // Force applied on impact
    private bool hasHit = false; // Prevent multiple hits

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        Debug.Log($"Bullet collided with: {collision.gameObject.name}");

        // Check if the bullet hits a rigidbody object
        Rigidbody rb = collision.rigidbody;
        if (rb != null)
        {
            Vector3 direction = GetComponent<Rigidbody>().linearVelocity.normalized;

            rb.AddForceAtPosition(
                direction * impactForce,
                collision.GetContact(0).point,
                ForceMode.Impulse
            );
        }

        if (BulletDecalManager.Instance == null)
        {
            Debug.LogWarning("BulletDecalManager is not assigned.");
            return;
        }

        // Spawn bullet decal based on the hit surface
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")
            && collision.gameObject.layer != LayerMask.NameToLayer("Ragdoll"))
        {
            BulletDecalManager.Instance.SpawnBulletDecal(collision.GetContact(0));
        }

        // Destroy the bullet after impact
        Destroy(gameObject);
    }
}
