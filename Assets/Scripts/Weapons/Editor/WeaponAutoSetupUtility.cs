using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;

namespace UndeadSurvivalGame.Editor
{
    public static class WeaponAutoSetupUtility
    {
        public static void AutoSetupPrefabReferences(Weapon weapon, bool overwriteReferences)
        {
            TryAssignMuzzleTransform(weapon, overwriteReferences);
            TryAssignMuzzleEffect(weapon, overwriteReferences);
            TryAssignLeftHandGripSource(weapon, overwriteReferences);
            TryAssignGunshotAudioSource(weapon, overwriteReferences);
            TryAssignGunshotAudioClip(weapon, overwriteReferences);
            TryAssignBulletPrefab(weapon, overwriteReferences);
            TryAssignWeaponData(weapon, overwriteReferences);
        }

        public static void AutoSetupSceneReferences(Weapon weapon, bool overwriteReferences)
        {
            // Add scene-only reference assignment logic here if needed in the future
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

        private static void TryAssignLeftHandGripSource(Weapon weapon, bool overwriteReferences)
        {
            if (overwriteReferences || weapon.leftHandGripSource == null)
            {
                Transform foundGripSource = null;
                foreach (var t in weapon.GetComponentsInChildren<Transform>(true))
                {
                    if (string.Equals(t.name, "LeftHandGripSource", System.StringComparison.OrdinalIgnoreCase))
                    {
                        foundGripSource = t;
                        break;
                    }
                }
                if (foundGripSource != null)
                {
                    weapon.leftHandGripSource = foundGripSource;
                    Debug.Log($"[Weapon] Auto-assigned leftHandGripSource to '{foundGripSource.name}'.");
                }
                else
                {
                    Debug.LogWarning("[Weapon] Could not auto-assign leftHandGripSource: No child named 'LeftHandGripSource' found in prefab or scene context.");
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
                    Debug.LogWarning($"[Weapon] Could not auto-assigned gunshot AudioClip: No AudioClip found in {gunshotSoundsPath}.");
                }
            }
        }

        private static void TryAssignBulletPrefab(Weapon weapon, bool overwriteReferences)
        {
            if (overwriteReferences || weapon.bulletPrefab == null)
            {
                string bulletPrefabPath = "Assets/Prefabs/Bullets/SM_Bullet_05.prefab";
                var bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPrefabPath);
                if (bulletPrefab != null)
                {
                    weapon.bulletPrefab = bulletPrefab;
                    Debug.Log($"[Weapon] Auto-assigned bulletPrefab from: {bulletPrefabPath}");
                }
                else
                {
                    Debug.LogWarning($"[Weapon] Could not auto-assign bulletPrefab: No prefab found at {bulletPrefabPath}.");
                }
            }
        }

        private static void TryAssignWeaponData(Weapon weapon, bool overwriteReferences)
        {
            if (overwriteReferences || weapon.weaponData == null)
            {
                string prefabName = weapon.gameObject.name;
                string expectedName = prefabName + "WeaponData";
                string[] guids = AssetDatabase.FindAssets($"{expectedName} t:WeaponData");
                foreach (var guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var weaponData = AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);
                    if (weaponData != null && weaponData.name == expectedName)
                    {
                        weapon.weaponData = weaponData;
                        Debug.Log($"[Weapon] Auto-assigned weaponData '{weaponData.name}' from: {assetPath}");
                        return;
                    }
                }
                Debug.LogWarning($"[Weapon] Could not auto-assign weaponData: No WeaponData asset found matching expected name '{expectedName}'.");
            }
        }
    }
}
