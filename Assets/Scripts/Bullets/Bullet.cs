using UnityEngine;
using RootMotion.Dynamics;

public class Bullet : MonoBehaviour
{
    [HideInInspector]
    public float impactForce = 10f; // Force applied on impact
    private bool hasHit = false; // Prevent multiple hits
    
    [HideInInspector]
    public int damage = 1; // Damage to apply
    
    [HideInInspector]
    public WeaponData weaponData; // Reference to weapon data for damage calculations

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        Debug.Log($"Bullet collided with: {collision.gameObject.name}");
        
        ContactPoint contact = collision.GetContact(0);
        Vector3 hitPoint = contact.point;
        Vector3 hitNormal = contact.normal;
        
        // Handle enemy damage if this is a hitbox collision
        HandleEnemyHitboxImpact(collision.collider, hitPoint, hitNormal);

         // Apply physics force to any rigidbody (limbs, props, etc.)
        Rigidbody rb = collision.rigidbody;
        if (rb != null)
        {
            Vector3 direction = GetComponent<Rigidbody>().linearVelocity.normalized;
            
            // Check if this is a ragdoll limb and adjust force accordingly
            float adjustedForce = impactForce;
            PuppetMaster puppetMaster = rb.GetComponentInParent<PuppetMaster>();
            
            if (puppetMaster != null)
            {
                // Reduce force on dead ragdolls to prevent over-reaction
                if (puppetMaster.state == RootMotion.Dynamics.PuppetMaster.State.Dead)
                {
                    adjustedForce *= 0.3f; // Reduce force by 70% for dead ragdolls
                }
                // Increase force on alive ragdolls to overcome muscle resistance
                else if (puppetMaster.state == RootMotion.Dynamics.PuppetMaster.State.Alive)
                {
                    adjustedForce *= 2.0f; // Increase force by 100% for alive ragdolls
                }
            }

            rb.AddForceAtPosition(
                direction * adjustedForce,
                contact.point,
                ForceMode.Impulse
            );
            
            Debug.Log($"Applied force {adjustedForce} to {rb.gameObject.name} (PuppetMaster state: {puppetMaster?.state})");
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
            BulletDecalManager.Instance.SpawnBulletDecal(contact);
        }

        // Destroy the bullet after impact
        Destroy(gameObject);
    }

    private void HandleEnemyHitboxImpact(Collider hitCollider, Vector3 hitPoint, Vector3 hitNormal)
    {
        // Check if the hit object is on the Hitbox layer
        if (hitCollider.gameObject.layer != LayerMask.NameToLayer("Ragdoll"))
            return;

        // Try to get the Enemy and Limb components
        PuppetMaster puppetMaster = hitCollider.GetComponentInParent<PuppetMaster>();

        if (puppetMaster == null)
        {
            Debug.LogWarning("PuppetMaster component not found in parent hierarchy.");
            return;
        }

        Enemy enemy = puppetMaster.targetRoot.GetComponent<Enemy>();
        Limb limb = hitCollider.gameObject.GetComponent<Limb>();

        Debug.Log($"Bullet collision #{Time.frameCount} - Current enemy state: {enemy.stateMachine.currentState?.GetType().Name}");

        if (enemy == null)
        {
            Debug.LogWarning("Enemy component not found in parent hierarchy.");
            return;
        }

        if (limb == null)
        {
            Debug.LogWarning("Limb component not found in parent hierarchy.");
        }

        // Do damage to the enemy and limb:
        enemy.ProcessHit(damage, limb);

        // Spawn blood effect regardless of body part presence
        SpawnBloodEffect(hitPoint, hitNormal, enemy);

        enemy.stateMachine.SetState(enemy.Aggro);
    }
    
    private void SpawnBloodEffect(Vector3 hitPoint, Vector3 hitNormal, Enemy enemy)
    {
        GameObject bloodEffect = Object.Instantiate(
            enemy.template.bloodEffectPrefab,
            hitPoint,
            Quaternion.LookRotation(hitNormal)
        );
        Object.Destroy(bloodEffect, 2f);
    }
}
