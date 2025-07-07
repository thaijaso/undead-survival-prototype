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

    public Transform aimTarget; // Added aimTarget Transform reference

    public void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab, muzzleTransform.position, muzzleTransform.rotation);
        Rigidbody bulletRb = bullet.GetComponentInChildren<Rigidbody>();
        Bullet bulletScript = bullet.GetComponentInChildren<Bullet>();

        if (bulletRb != null && weaponData != null)
        {
            Vector3 direction = muzzleTransform.forward;
            if (aimTarget != null)
            {
                direction = (aimTarget.position - muzzleTransform.position).normalized;
            }
            bulletRb.linearVelocity = direction * weaponData.bulletSpeed;
            bulletScript.impactForce = weaponData.impactForce;
            bulletScript.damage = weaponData.damage;
            bulletScript.weaponData = weaponData;
            Debug.Log($"Firing bullet with impact force: {weaponData.impactForce} and damage: {weaponData.damage} toward {(aimTarget != null ? aimTarget.position.ToString() : "forward")}");
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

#if UNITY_EDITOR
    [Button("Auto-Setup References")]
    private void AutoSetupReferences()
    {
        // Try to find and assign muzzleTransform (child search)
        if (muzzleTransform == null)
        {
            var foundMuzzle = transform.Find("MuzzleTransform");
            if (foundMuzzle != null)
            {
                muzzleTransform = foundMuzzle;
                Debug.Log($"[Weapon] Auto-assigned muzzleTransform to child named '{foundMuzzle.name}'.");
            }
            else
            {
                Debug.LogWarning("[Weapon] Could not auto-assign muzzleTransform: No child named 'Muzzle' or 'MuzzleTransform' found.");
            }
        }
        // Try to find and assign muzzleEffect from prefab if null
        if (muzzleEffect == null)
        {
#if UNITY_EDITOR
            string muzzleEffectsPath = "Assets/Prefabs/Effects/MuzzleEffects";
            var guids = UnityEditor.AssetDatabase.FindAssets("t:GameObject", new[] { muzzleEffectsPath });
            foreach (var guid in guids)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab != null)
                {
                    var ps = prefab.GetComponentInChildren<ParticleSystem>(true);
                    if (ps != null)
                    {
                        muzzleEffect = ps;
                        Debug.Log($"[Weapon] Auto-assigned muzzleEffect from prefab: {assetPath}");
                        break;
                    }
                }
            }
            if (muzzleEffect == null)
            {
                Debug.LogWarning($"[Weapon] Could not auto-assign muzzleEffect: No ParticleSystem found in any prefab under {muzzleEffectsPath}.");
            }
#endif
        }
        // Try to find and assign aimTarget (scene search only)
        if (aimTarget == null)
        {
            Transform foundAim = null;
            // Only search the scene, not the prefab
            Debug.Log("[Weapon] Scene mode: Searching all active objects for AimTarget.");
            foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (string.Equals(t.name, "AimTarget", System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"[Weapon] Found AimTarget at: {t.GetHierarchyPath()}");
                    foundAim = t;
                    break;
                }
            }
            if (foundAim != null)
            {
                aimTarget = foundAim;
                Debug.Log($"[Weapon] Auto-assigned aimTarget to '{foundAim.name}' in scene context.");
            }
            else
            {
                Debug.LogWarning("[Weapon] Could not auto-assign aimTarget: No 'AimTarget' found in scene context.");
            }
        }
        // Check for AudioSource on this GameObject, add if missing, and assign a gunshot clip if found
        if (gunshot == null)
        {
            gunshot = GetComponent<AudioSource>();
            if (gunshot == null)
            {
                gunshot = gameObject.AddComponent<AudioSource>();
                gunshot.playOnAwake = false;
                Debug.Log("[Weapon] AudioSource component was missing and has been added to the GameObject with Play On Awake disabled.");
            }
            else
            {
                Debug.Log("[Weapon] AudioSource component found and assigned.");
            }
        }
        // Always assign a gunshot AudioClip if missing
#if UNITY_EDITOR
        if (gunshot != null && gunshot.clip == null)
        {
            string gunshotSoundsPath = "Assets/Prefabs/Effects/SoundEffects/GunShotSoundEffects";
            var audioGuids = UnityEditor.AssetDatabase.FindAssets("t:AudioClip", new[] { gunshotSoundsPath });
            if (audioGuids.Length > 0)
            {
                string audioPath = UnityEditor.AssetDatabase.GUIDToAssetPath(audioGuids[0]);
                var audioClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
                if (audioClip != null)
                {
                    gunshot.clip = audioClip;
                    Debug.Log($"[Weapon] Auto-assigned gunshot AudioClip from: {audioPath}");
                }
            }
            else
            {
                Debug.LogWarning($"[Weapon] Could not auto-assign gunshot AudioClip: No AudioClip found in {gunshotSoundsPath}.");
            }
        }
#endif
    }

    // Recursively search for a child by name in the hierarchy, including the parent itself, with debug logs and case-insensitive
    private Transform FindInHierarchyInclusiveDebug(Transform parent, string childName)
    {
        if (string.Equals(parent.name, childName, System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log($"[Weapon] Found AimTarget at: {parent.GetHierarchyPath()}");
            return parent;
        }
        foreach (Transform child in parent)
        {
            Debug.Log($"[Weapon] Checking child: {child.GetHierarchyPath()}");
            var result = FindInHierarchyInclusiveDebug(child, childName);
            if (result != null)
                return result;
        }
        return null;
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
