using UnityEngine;
using RootMotion.Dynamics;
using System.Collections;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    [HideInInspector]
    public float impactForce = 10f; // Force applied on impact
    private bool hasHit = false; // Prevent multiple hits
    
    [HideInInspector]
    public int damage = 1; // Damage to apply
    
    [HideInInspector]
    public WeaponData weaponData; // Reference to weapon data for damage calculations

    // Static dictionary to track original muscle properties
    private static Dictionary<Muscle, MuscleOriginalValues> originalMuscleValues = new Dictionary<Muscle, MuscleOriginalValues>();

    private struct MuscleOriginalValues
    {
        public float pinWeight;
        public float muscleWeight;
        public float muscleDamper;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        Debug.Log($"[Bullet] OnCollisionEnter(): {gameObject.name} collided with: {collision.gameObject.name}");
        
        ContactPoint contact = collision.GetContact(0);
        Vector3 hitPoint = contact.point;
        Vector3 hitNormal = contact.normal;
        
        // Handle enemy damage if this is a hitbox collision
        HandleEnemyHitboxImpact(collision.collider, hitPoint, hitNormal);

        // Apply physics force to any rigidbody (limbs, props, etc.)
        ApplyImpactForce(collision, contact);

        if (BulletDecalManager.Instance == null)
        {
            Debug.LogWarning("[Bullet] OnCollisionEnter(): BulletDecalManager instance is not assigned.");
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

    private void ApplyImpactForce(Collision collision, ContactPoint contact)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb != null)
        {
            Vector3 direction = GetComponent<Rigidbody>().linearVelocity.normalized;
            Vector3 force = direction * impactForce;

            // Check if this is part of a PuppetMaster
            PuppetMaster puppetMaster = collision.collider.GetComponentInParent<PuppetMaster>();
            
            if (puppetMaster != null)
            {
                // Apply force through PuppetMaster system for more natural results
                ApplyForceToMuscle(puppetMaster, rb, force, contact.point);
            }
            else
            {
                // Regular rigidbody (props, debris, etc.)
                rb.AddForceAtPosition(force, contact.point, ForceMode.Impulse);
            }
        }
    }

    private void HandleEnemyHitboxImpact(Collider hitCollider, Vector3 hitPoint, Vector3 hitNormal)
    {
        // Check if the hit object is on the Hitbox layer
        if (hitCollider.gameObject.layer != LayerMask.NameToLayer("EnemyRagdoll"))
        {
            Debug.Log("[Bullet] HandleEnemyHitboxImpact(): Hit object is not on the EnemyRagdoll layer.");
            return;
        }

        // Try to get the Enemy and Limb components
        PuppetMaster puppetMaster = hitCollider.GetComponentInParent<PuppetMaster>();

        if (puppetMaster == null)
        {
            Debug.LogWarning("[Bullet] HandleEnemyHitboxImpact(): PuppetMaster component not found in parent hierarchy.");
            return;
        }

        Enemy enemy = puppetMaster.targetRoot.GetComponent<Enemy>();

        Debug.Log($"[Bullet] HandleEnemyHitboxImpact(): Collision #{Time.frameCount} - Current enemy state: {enemy.stateMachine.currentState?.GetType().Name}");

        if (enemy == null)
        {
            Debug.LogWarning("[Bullet] HandleEnemyHitboxImpact(): Enemy component not found in parent hierarchy.");
            return;
        }

        Limb limb = hitCollider.gameObject.GetComponent<Limb>();

        if (limb == null)
        {
            limb = hitCollider.GetComponentInParent<Limb>();

            if (limb == null)
            {
                Debug.LogWarning("[Bullet] HandleEnemyHitboxImpact(): No Limb component found in current or parent hierarchy.");
            }
        }

        // Do damage to the enemy and limb:
        enemy.ProcessHit(damage, limb);

        // Spawn blood effect regardless of body part presence
        SpawnBloodEffect(hitPoint, hitNormal, enemy);

        // Handle state transitions based on bullet impact
        HandleEnemyStateTransition(enemy, limb);
    }
    
    private void HandleEnemyStateTransition(Enemy enemy, Limb limb)
    {
        // Only force state transitions if the enemy is not in debug mode
        if (!enemy.DebugModeEnabled)
        {
            var currentState = enemy.stateMachine.currentState;
            var hitReactionState = enemy.HitReaction as HitReactionState;
            // hitReactionState?.SetHitLimb(limb);

            // if (currentState == enemy.Death || currentState == enemy.GetUp)
            // {
            //     Debug.Log($"[Bullet] HandleEnemyStateTransition(): Enemy is dead or getting up - no state change needed");
            //     return;
            // }

            hitReactionState.OnHit(limb);
        }
        else
        {
            Debug.Log($"[Bullet] HandleEnemyStateTransition(): Enemy in debug mode - not forcing state transitions");
        }
    }

    private void SpawnBloodEffect(Vector3 hitPoint, Vector3 hitNormal, Enemy enemy)
    {
        GameObject bloodEffect = Instantiate(
            enemy.enemyTemplate.bloodEffectPrefab,
            hitPoint,
            Quaternion.LookRotation(hitNormal)
        );

        Destroy(bloodEffect, 2f);
    }

    private void ApplyForceToMuscle(PuppetMaster puppetMaster, Rigidbody hitRigidbody, Vector3 force, Vector3 position)
    {
        Limb hitLimb = hitRigidbody.GetComponent<Limb>();
        Debug.Log($"[Bullet] ApplyForceToMuscle(): Applying force to muscle for hit limb: {hitLimb.LimbType}");

        // Find the muscle that owns this rigidbody
        foreach (Muscle muscle in puppetMaster.muscles)
        {
            if (muscle.rigidbody == hitRigidbody)
            {
                // Store original values only if this is the first time hitting this muscle
                if (!originalMuscleValues.ContainsKey(muscle))
                {
                    MuscleOriginalValues originalValues = new MuscleOriginalValues();
                    originalValues.pinWeight = muscle.props.pinWeight;
                    originalValues.muscleWeight = muscle.props.muscleWeight;
                    originalValues.muscleDamper = muscle.props.muscleDamper;

                    originalMuscleValues[muscle] = originalValues;
                }

                // TODO: get these values from template for each limb for more control (feet move too much)
                // Set specific weights for bullet impact effect
                muscle.props.pinWeight = 0.6f;
                muscle.props.muscleWeight = 0.7f;
                muscle.props.muscleDamper = 0.5f; // Reduce damping for more dramatic movement

                // Then apply force through the muscle system for maximum effect
                muscle.rigidbody.AddForceAtPosition(force, position, ForceMode.Impulse);

                // Schedule weight restoration through the PuppetMaster (won't be destroyed)
                puppetMaster.StartCoroutine(RestoreMusclePropertiesDelayed(muscle, 0.5f));

                Debug.Log($"[Bullet] ApplyForceToMuscle(): Applied force {force.magnitude} to muscle: {muscle.target.name}");
                break;
            }
        }
    }

    private static IEnumerator RestoreMusclePropertiesDelayed(Muscle muscle, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (muscle != null && muscle.props != null && originalMuscleValues.ContainsKey(muscle))
        {
            MuscleOriginalValues originalValues = originalMuscleValues[muscle];
            
            // Restore original PuppetMaster properties
            muscle.props.pinWeight = originalValues.pinWeight;
            muscle.props.muscleWeight = originalValues.muscleWeight;
            muscle.props.muscleDamper = originalValues.muscleDamper;
            
            // Remove from tracking dictionary
            originalMuscleValues.Remove(muscle);
            
            Debug.Log($"[Bullet] RestoreMusclePropertiesDelayed(): Restored muscle properties for: {muscle.target.name}");
        }
    }
}
