using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector;

public static class WeaponAutoSetupUtility
{
    public static void AutoSetupReferences(Weapon weapon, bool overwriteReferences)
    {
        // Try to find and assign muzzleTransform (search all children, including inactive, in prefab and scene context)
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
        // Try to find and assign muzzleEffect from prefab if null
        if (overwriteReferences || weapon.muzzleEffect == null)
        {
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
                        weapon.muzzleEffect = ps;
                        Debug.Log($"[Weapon] Auto-assigned muzzleEffect from prefab: {assetPath}");
                        break;
                    }
                }
            }
            if (weapon.muzzleEffect == null)
            {
                Debug.LogWarning($"[Weapon] Could not auto-assign muzzleEffect: No ParticleSystem found in any prefab under {muzzleEffectsPath}.");
            }
        }
        // Try to find and assign bulletHitTarget (scene search only)
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
        // Check for AudioSource on this GameObject, add if missing, and assign a gunshot clip if found
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
        // Always assign a gunshot AudioClip if missing
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
