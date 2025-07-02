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
                if (puppetMaster.state == PuppetMaster.State.Dead)
                {
                    adjustedForce *= 0.3f; // Reduce force by 70% for dead ragdolls
                }
                // Increase force on alive ragdolls to overcome muscle resistance
                else if (puppetMaster.state == PuppetMaster.State.Alive)
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

        Debug.Log($"Bullet collision #{Time.frameCount} - Current enemy state: {enemy.stateMachine.currentState?.GetType().Name}");

        if (enemy == null)
        {
            Debug.LogWarning("Enemy component not found in parent hierarchy.");
            return;
        }

        Limb limb = hitCollider.gameObject.GetComponent<Limb>();

        if (limb == null)
        {
            limb = hitCollider.GetComponentInParent<Limb>();

            if (limb == null)
            {
                Debug.LogWarning("No Limb component found in current or parent hierarchy.");
            }
        }

        // Do damage to the enemy and limb:
        enemy.ProcessHit(damage, limb);

        // Add hit reaction if enemy is alive
        if (puppetMaster.state == PuppetMaster.State.Alive)
        {
            //ApplyHitReaction(puppetMaster, limb);
        }

        // Spawn blood effect regardless of body part presence
        SpawnBloodEffect(hitPoint, hitNormal, enemy);

        // Handle state transitions based on bullet impact
        HandleEnemyStateTransition(enemy);
    }
    
    private void HandleEnemyStateTransition(Enemy enemy)
    {
        // Only force state transitions if the enemy is not in debug mode
        if (!enemy.DebugModeEnabled)
        {
            var currentState = enemy.stateMachine.currentState;
            
            // Determine appropriate state based on current state and aggro history
            if (!enemy.HasAggroed)
            {
                // First time being hit - go to Aggro state for initial reaction
                enemy.stateMachine.SetState(enemy.Aggro);
                Debug.Log($"[Bullet] HandleEnemyStateTransition(): First aggro - transitioning to Aggro state");
            }
            else if (currentState == enemy.Aggro)
            {
                // Currently in aggro state (first aggro animation) - don't interrupt it
                Debug.Log($"[Bullet] HandleEnemyStateTransition(): Enemy in Aggro state - not interrupting aggro animation");
            }
            else
            {
                // Already aggroed before and not currently in Aggro - behavior depends on current state
                if (currentState == enemy.Idle || currentState == enemy.Patrol || currentState == enemy.Alert)
                {
                    // Re-engaging from a calm state - go to Chase (skip aggro animation)
                    enemy.stateMachine.SetState(enemy.Chase);
                    Debug.Log($"[Bullet] HandleEnemyStateTransition(): Re-engaging from {currentState.GetType().Name} - transitioning to Chase");
                }
                else if (currentState == enemy.Attack)
                {
                    // Already attacking - stay in attack, let natural transitions handle it
                    Debug.Log($"[Bullet] HandleEnemyStateTransition(): Enemy already attacking - no state change needed");
                }
                else
                {
                    // In Chase - stay in chase, no need to force transition
                    Debug.Log($"[Bullet] HandleEnemyStateTransition(): In Chase state - maintaining pursuit");
                }
            }
        }
        else
        {
            Debug.Log($"[Bullet] HandleEnemyStateTransition(): Enemy in debug mode - not forcing state transitions");
        }
    }

    private void SpawnBloodEffect(Vector3 hitPoint, Vector3 hitNormal, Enemy enemy)
    {
        GameObject bloodEffect = Instantiate(
            enemy.template.bloodEffectPrefab,
            hitPoint,
            Quaternion.LookRotation(hitNormal)
        );

        Destroy(bloodEffect, 2f);
    }
}
