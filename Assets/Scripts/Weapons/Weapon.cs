using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform aimTransform;
    public ParticleSystem muzzleEffect;

    public AudioSource gunshot;

    public GameObject bulletPrefab;
    public ParticleSystem bulletTracer;

    public Transform muzzleTransform;

    public WeaponData weaponData;

    public void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab, muzzleTransform.position, muzzleTransform.rotation);
        Rigidbody bulletRb = bullet.GetComponentInChildren<Rigidbody>();
        Bullet bulletScript = bullet.GetComponentInChildren<Bullet>();

        if (bulletRb != null && weaponData != null)
        {
            bulletRb.linearVelocity = muzzleTransform.forward * weaponData.bulletSpeed;
            bulletScript.impactForce = weaponData.impactForce;
            Debug.Log($"Firing bullet with impact force: {weaponData.impactForce}");
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
}
