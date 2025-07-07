using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class Weapon : MonoBehaviour
{
    public ParticleSystem muzzleEffect;

    public AudioSource gunshot;

    public GameObject bulletPrefab;
    public ParticleSystem bulletTracer;

    public Transform muzzleTransform;

    public WeaponData weaponData;

    public Transform bulletHitTarget; // Added aimTarget Transform reference

    private void Awake()
    {
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
