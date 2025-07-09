using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector;

public static class WeaponAutoSetupUtility
{
    public static void AutoSetupReferences(Weapon weapon, bool overwriteReferences)
    {
        TryAssignMuzzleTransform(weapon, overwriteReferences);
        TryAssignMuzzleEffect(weapon, overwriteReferences);
        TryAssignBulletHitTarget(weapon, overwriteReferences);
        TryAssignLeftHandGripTarget(weapon, overwriteReferences);
        TryAssignGunshotAudioSource(weapon, overwriteReferences);
        TryAssignGunshotAudioClip(weapon, overwriteReferences);
    }

    private static void TryAssignMuzzleTransform(Weapon weapon, bool overwriteReferences)
    {
        if (overwriteReferences || weapon.muzzleTransform == null)
        {
            Transform foundMuzzle = null;
            foreach (var t in weapon.GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(t.name, "MuzzleTransform", System.StringComparison.OrdinalIgnoreCase))
                {
                    foundMuzzle = t;
                    break;
                }
            }
            if (foundMuzzle != null)
            {
                weapon.muzzleTransform = foundMuzzle;
                Debug.Log($"[Weapon] Auto-assigned muzzleTransform to '{foundMuzzle.name}'.");
            }
            else
            {
                Debug.LogWarning("[Weapon] Could not auto-assign muzzleTransform: No child named 'MuzzleTransform' found in prefab or scene context.");
            }
        }
    }

    private static void TryAssignMuzzleEffect(Weapon weapon, bool overwriteReferences)
    {
        if (overwriteReferences || weapon.muzzleEffect == null)
        {
            ParticleSystem foundMuzzleEffect = null;
            if (weapon.muzzleTransform != null)
            {
                foundMuzzleEffect = weapon.muzzleTransform.GetComponentInChildren<ParticleSystem>(true);
            }
            if (foundMuzzleEffect == null)
            {
                foundMuzzleEffect = weapon.GetComponentInChildren<ParticleSystem>(true);
            }
            if (foundMuzzleEffect != null)
            {
                weapon.muzzleEffect = foundMuzzleEffect;
                // Disable Play On Awake by default
                var main = foundMuzzleEffect.main;
                main.playOnAwake = false;
                Debug.Log($"[Weapon] Auto-assigned muzzleEffect to '{foundMuzzleEffect.name}' from weapon hierarchy and set Play On Awake to false.");
            }
            else
            {
                Debug.LogWarning("[Weapon] Could not auto-assign muzzleEffect: No ParticleSystem found in weapon hierarchy.");
            }
        }
    }

    private static void TryAssignBulletHitTarget(Weapon weapon, bool overwriteReferences)
    {
        if (overwriteReferences || weapon.bulletHitTarget == null)
        {
            Transform foundTarget = null;
            Debug.Log("[Weapon] Scene mode: Searching all active objects for BulletHitTarget.");
            foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (string.Equals(t.name, "BulletHitTarget", System.StringComparison.OrdinalIgnoreCase))
                {
                    foundTarget = t;
                    break;
                }
            }
            if (foundTarget != null)
            {
                weapon.bulletHitTarget = foundTarget;
                Debug.Log($"[Weapon] Auto-assigned bulletHitTarget to '{foundTarget.name}' in scene context.");
            }
            else
            {
                Debug.LogWarning("[Weapon] Could not auto-assign bulletHitTarget: No 'BulletHitTarget' found in scene context.");
            }
        }
    }

    private static void TryAssignLeftHandGripTarget(Weapon weapon, bool overwriteReferences)
    {
        if (overwriteReferences || weapon.leftHandGripSource == null)
        {
            Transform foundGrip = null;
            foreach (var t in weapon.GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(t.name, "LeftHandGripTarget", System.StringComparison.OrdinalIgnoreCase))
                {
                    foundGrip = t;
                    break;
                }
            }
            if (foundGrip != null)
            {
                weapon.leftHandGripSource = foundGrip;
                Debug.Log($"[Weapon] Auto-assigned leftHandGripTarget to '{foundGrip.name}'.");
            }
            else
            {
                Debug.LogWarning("[Weapon] Could not auto-assign leftHandGripTarget: No child named 'LeftHandGripTarget' found in prefab or scene context.");
            }
        }
    }

    private static void TryAssignGunshotAudioSource(Weapon weapon, bool overwriteReferences)
    {
        if (overwriteReferences || weapon.gunshot == null)
        {
            weapon.gunshot = weapon.GetComponent<AudioSource>();
            if (weapon.gunshot == null)
            {
                weapon.gunshot = weapon.gameObject.AddComponent<AudioSource>();
                weapon.gunshot.playOnAwake = false;
                Debug.Log("[Weapon] AudioSource component was missing and has been added to the GameObject with Play On Awake disabled.");
            }
            else
            {
                Debug.Log("[Weapon] AudioSource component found and assigned.");
            }
        }
    }

    private static void TryAssignGunshotAudioClip(Weapon weapon, bool overwriteReferences)
    {
        if (weapon.gunshot != null && (overwriteReferences || weapon.gunshot.clip == null))
        {
            string gunshotSoundsPath = "Assets/Prefabs/Effects/SoundEffects/GunShotSoundEffects";
            var audioGuids = UnityEditor.AssetDatabase.FindAssets("t:AudioClip", new[] { gunshotSoundsPath });
            if (audioGuids.Length > 0)
            {
                string audioPath = UnityEditor.AssetDatabase.GUIDToAssetPath(audioGuids[0]);
                var audioClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
                if (audioClip != null)
                {
                    weapon.gunshot.clip = audioClip;
                    Debug.Log($"[Weapon] Auto-assigned gunshot AudioClip from: {audioPath}");
                }
            }
            else
            {
                Debug.LogWarning($"[Weapon] Could not auto-assign gunshot AudioClip: No AudioClip found in {gunshotSoundsPath}.");
            }
        }
    }
}
#endif
