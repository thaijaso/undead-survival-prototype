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

        // Only force state transitions if the enemy is not in debug mode
        if (!enemy.DebugModeEnabled)
        {
            // Determine appropriate state based on current state and aggro history
            if (!enemy.HasAggroed)
            {
                // First time being hit - go to Aggro state for initial reaction
                enemy.stateMachine.SetState(enemy.Aggro);
                Debug.Log($"[Bullet] HandleEnemyHitboxImpact(): First aggro - transitioning to Aggro state");
            }
            else
            {
                // Already aggroed before - behavior depends on current state
                var currentState = enemy.stateMachine.currentState;
                
                if (currentState == enemy.Idle || currentState == enemy.Patrol || currentState == enemy.Alert)
                {
                    // Re-engaging from a calm state - go to Chase (skip aggro animation)
                    enemy.stateMachine.SetState(enemy.Chase);
                    Debug.Log($"[Bullet] HandleEnemyHitboxImpact(): Re-engaging from {currentState.GetType().Name} - transitioning to Chase");
                }
                else if (currentState == enemy.Attack)
                {
                    // Already attacking - stay in attack, let natural transitions handle it
                    Debug.Log($"[Bullet] HandleEnemyHitboxImpact(): Enemy already attacking - no state change needed");
                }
                else
                {
                    // In Aggro or Chase - go to Chase to ensure active pursuit
                    enemy.stateMachine.SetState(enemy.Chase);
                    Debug.Log($"[Bullet] HandleEnemyHitboxImpact(): Ensuring Chase state from {currentState.GetType().Name}");
                }
            }
        }
        else
        {
            Debug.Log($"[Bullet] HandleEnemyHitboxImpact(): Enemy in debug mode - not forcing state transitions");
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

    // this is causing the zombies head to spin LOL
    private void ApplyHitReaction(PuppetMaster puppetMaster, Limb hitLimb)
    {
        // Store original muscle weights for restoration
        float[] originalWeights = new float[puppetMaster.muscles.Length];

        for (int i = 0; i < puppetMaster.muscles.Length; i++)
        {
            originalWeights[i] = puppetMaster.muscles[i].props.muscleWeight;

            // If we hit a specific limb, reduce its muscle weight more
            if (hitLimb != null && puppetMaster.muscles[i].rigidbody == hitLimb.GetComponent<Rigidbody>())
            {
                puppetMaster.muscles[i].props.muscleWeight *= 0.2f; // Less extreme for hit limb
            }
            else
            {
                puppetMaster.muscles[i].props.muscleWeight *= 0.6f; // More conservative reduction for other muscles
            }
        }

        // Start coroutine to restore muscle strength
        StartCoroutine(RestoreMuscleStrength(puppetMaster, originalWeights, 0.5f)); // Faster recovery
    }
    
    private System.Collections.IEnumerator RestoreMuscleStrength(PuppetMaster puppetMaster, float[] originalWeights, float duration)
    {
        float elapsed = 0f;
        
        // Store the reduced weights to interpolate from
        float[] reducedWeights = new float[puppetMaster.muscles.Length];
        for (int i = 0; i < puppetMaster.muscles.Length; i++)
        {
            reducedWeights[i] = puppetMaster.muscles[i].props.muscleWeight;
        }
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            // Smoothly interpolate back to original strength
            for (int i = 0; i < puppetMaster.muscles.Length; i++)
            {
                if (puppetMaster.muscles[i] != null) // Safety check
                {
                    puppetMaster.muscles[i].props.muscleWeight = Mathf.Lerp(reducedWeights[i], originalWeights[i], t);
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we're back to original values
        for (int i = 0; i < puppetMaster.muscles.Length; i++)
        {
            if (puppetMaster.muscles[i] != null)
            {
                puppetMaster.muscles[i].props.muscleWeight = originalWeights[i];
            }
        }
    }
}
