using UndeadSurvivalGame.UI;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

/// <summary>
/// This MonoBehaviour represents a runtime weapon instance in the scene.
/// It handles firing, effects, and references to visual/audio components.
/// It references a WeaponConfig ScriptableObject for all static configuration and stats.
/// </summary>
namespace UndeadSurvivalGame.Gameplay
{
    public class Weapon : MonoBehaviour
    {
    #if ODIN_INSPECTOR
        [BoxGroup("Prefab References", Order = 0)]
        public ParticleSystem muzzleEffect;

        [BoxGroup("Prefab References", Order = 0)]
        public AudioSource gunshot;

        [BoxGroup("Prefab References", Order = 0)]
        public AudioSource emptyGunClick;

        [BoxGroup("Prefab References", Order = 0)]
        public GameObject bulletPrefab;

        [BoxGroup("Prefab References", Order = 0)]
        public ParticleSystem bulletTracer;

        [BoxGroup("Prefab References", Order = 0)]
        public Transform muzzleTransform;

        [BoxGroup("Prefab References", Order = 0)]
        public Transform leftHandGripSource;

        [BoxGroup("Prefab References", Order = 0)]
        public WeaponConfig WeaponConfig;

        [BoxGroup("Scene References", Order = 1)]
        public Transform bulletHitTarget;

        [BoxGroup("Ammo", Order = 2)]
        public int currentLoadedAmmo = 0;

        [BoxGroup("Interaction Scripts", Order = 2)]
        public ItemPickupInteractable itemPickupInteractable;

        [BoxGroup("Interaction Scripts", Order = 2)]
        public ProximityUI proximityUI;

        [BoxGroup("Component References", Order = 3)]
        public Collider weaponCollider;

    #else
        // Visual and audio effects
        public ParticleSystem muzzleEffect; // Muzzle flash effect
        public AudioSource gunshot;         // Gunshot sound

        // Projectile and tracer
        public GameObject bulletPrefab;     // Prefab for the bullet projectile
        public ParticleSystem bulletTracer; // Optional: visual tracer for bullets

        // Muzzle and grip transforms
        public Transform muzzleTransform;   // Where bullets and effects spawn
        public Transform leftHandGripSource;// LeftHandIKTarget will use this as a world space reference

        // Data and targeting
        public WeaponConfig WeaponConfig;       // Reference to ScriptableObject with all weapon stats/config
        public Transform bulletHitTarget;   // Optional: world target for bullet direction (e.g., aim point)

        // Bullet count
        public int currentAmmo = 0;
#endif

        private void Awake()
        {
            // Assign BulletHitTarget at runtime if not set (scene search)
            if (bulletHitTarget == null)
            {
                foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
                {
                    if (string.Equals(t.name, "BulletHitTarget", System.StringComparison.OrdinalIgnoreCase))
                    {
                        bulletHitTarget = t;
                        Debug.Log($"[Weapon] Auto-assigned BulletHitTarget at runtime to '{t.GetHierarchyPath()}' on {gameObject.name}.");
                        break;
                    }
                }
            }
            ValidateReferences();
        }

        public void Fire()
        {
            GameObject bullet = Instantiate(bulletPrefab, muzzleTransform.position, muzzleTransform.rotation);
            Rigidbody bulletRb = bullet.GetComponentInChildren<Rigidbody>();
            Bullet bulletScript = bullet.GetComponentInChildren<Bullet>();

            if (bulletRb != null && WeaponConfig != null)
            {
                Vector3 direction = muzzleTransform.forward;
                // Use camera center for aiming
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // center of screen
                    Vector3 targetPoint;
                    if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
                    {
                        targetPoint = hit.point;
                    }
                    else
                    {
                        targetPoint = ray.origin + ray.direction * 1000f; // far point
                    }
                    direction = (targetPoint - muzzleTransform.position).normalized;
                }
                else if (bulletHitTarget != null)
                {
                    direction = (bulletHitTarget.position - muzzleTransform.position).normalized;
                }
                bulletRb.linearVelocity = direction * WeaponConfig.bulletSpeed;
                bulletScript.impactForce = WeaponConfig.impactForce;
                bulletScript.damage = WeaponConfig.damage;
                bulletScript.weaponData = WeaponConfig;
                Debug.Log($"Firing bullet with impact force: {WeaponConfig.impactForce} and damage: {WeaponConfig.damage} toward {direction}");
            }
            else
            {
                Debug.LogWarning("Bullet Rigidbody or WeaponData is not assigned.");
            }
        }

        public void PlayMuzzleEffect()
        {
            if (muzzleEffect != null)
            {
                muzzleEffect.Play();
            }
            else
            {
                Debug.LogWarning("Muzzle effect is not assigned.");
            }
        }

        public void StopMuzzleEffect()
        {
            if (muzzleEffect != null)
            {
                muzzleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            else
            {
                Debug.LogWarning("Muzzle effect is not assigned.");
            }
        }

        public void PlayGunshotSound()
        {
            if (gunshot != null)
            {
                gunshot.PlayOneShot(gunshot.clip);
            }
            else
            {
                Debug.LogWarning("Gunshot sound is not assigned.");
            }
        }

        public void PlayEmptyGunClick()
        {
            if (emptyGunClick != null)
            {
                emptyGunClick.PlayOneShot(emptyGunClick.clip);
            }
            else
            {
                Debug.LogWarning("Empty gun click sound is not assigned.");
            }
        }

        public void ValidateReferences()
        {
            if (muzzleEffect == null)
                Debug.LogWarning($"[Weapon] MuzzleEffect is not assigned on {gameObject.name}.");
            if (gunshot == null)
                Debug.LogWarning($"[Weapon] Gunshot AudioSource is not assigned on {gameObject.name}.");
            if (bulletPrefab == null)
                Debug.LogWarning($"[Weapon] BulletPrefab is not assigned on {gameObject.name}.");
            if (muzzleTransform == null)
                Debug.LogWarning($"[Weapon] MuzzleTransform is not assigned on {gameObject.name}.");
            if (WeaponConfig == null)
                Debug.LogWarning($"[Weapon] WeaponData is not assigned on {gameObject.name}.");
            if (bulletHitTarget == null)
                Debug.LogWarning($"[Weapon] BulletHitTarget is not assigned on {gameObject.name}.");
        }
    }

    // Helper extension to print full hierarchy path
    public static class TransformExtensions
    {
        public static string GetHierarchyPath(this Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
