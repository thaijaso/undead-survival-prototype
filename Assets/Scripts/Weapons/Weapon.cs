using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

// Weapon.cs
// This MonoBehaviour represents a runtime weapon instance in the scene. It handles firing, effects, and references to visual/audio components.
// It references a WeaponData ScriptableObject for all static configuration and stats.

public class Weapon : MonoBehaviour
{
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
    public WeaponData weaponData;       // Reference to ScriptableObject with all weapon stats/config
    public Transform bulletHitTarget;   // Optional: world target for bullet direction (e.g., aim point)

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

        if (bulletRb != null && weaponData != null)
        {
            Vector3 direction = muzzleTransform.forward;
            if (bulletHitTarget != null)
            {
                direction = (bulletHitTarget.position - muzzleTransform.position).normalized;
            }
            bulletRb.linearVelocity = direction * weaponData.bulletSpeed;
            bulletScript.impactForce = weaponData.impactForce;
            bulletScript.damage = weaponData.damage;
            bulletScript.weaponData = weaponData;
            Debug.Log($"Firing bullet with impact force: {weaponData.impactForce} and damage: {weaponData.damage} toward {(bulletHitTarget != null ? bulletHitTarget.position.ToString() : "forward")}");
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
            muzzleEffect.Stop();
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
        if (weaponData == null)
            Debug.LogWarning($"[Weapon] WeaponData is not assigned on {gameObject.name}.");
        if (bulletHitTarget == null)
            Debug.LogWarning($"[Weapon] BulletHitTarget is not assigned on {gameObject.name}.");
    }

#if UNITY_EDITOR
    [BoxGroup("Auto-Setup"), PropertyOrder(-1)]
    [ToggleLeft]
    [ShowInInspector]
    [LabelText("Overwrite Existing References")]
    private bool overwriteReferences = false;

    [BoxGroup("Auto-Setup")]
    [Button("Auto-Setup References", ButtonSizes.Large)]
    private void AutoSetupReferences()
    {
        WeaponAutoSetupUtility.AutoSetupReferences(this, overwriteReferences);
    }
#endif
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
