#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;

public static class PlayerTemplateAutoSetupUtility
{
    public static void AutoSetupReferences(PlayerTemplate template)
    {
        var unityObj = template as UnityEngine.Object;
        string assetName = unityObj != null ? unityObj.name : "(unknown)";
        Debug.Log($"[PlayerTemplate] Auto-Setup References button pressed for {assetName}.");
        bool changed = false;
        changed |= TryAssignPrefabByName(template, "followTargetPrefab", "FollowTarget");
        changed |= TryAssignPrefabByName(template, "bulletHitTargetPrefab", "BulletHitTarget");
        changed |= TryAssignPrefabByName(template, "weaponHandPrefab", "WeaponHand");
        changed |= TryAssignPrefabByName(template, "aimIKTargetPrefab", "AimIKTarget");
        changed |= TryAssignPrefabByName(template, "leftHandIKTargetPrefab", "LeftHandIKTarget");
        // Add more as needed
        if (changed)
        {
            EditorUtility.SetDirty(unityObj);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PlayerTemplate] Auto-Setup complete for {assetName}.");
        }
        else
        {
            Debug.LogWarning($"[PlayerTemplate] Auto-Setup found no assignable prefabs for {assetName}.");
        }
    }

    private static bool TryAssignPrefabByName(PlayerTemplate template, string fieldName, string prefabName)
    {
        var field = template.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null) {
            Debug.LogWarning($"[PlayerTemplate] Field '{fieldName}' not found.");
            return false;
        }
        var current = field.GetValue(template) as GameObject;
        if (current != null) return false;

        // First, try to find prefab at Assets/Player/Prefab/{PrefabName}.prefab
        string directPath = $"Assets/Player/Prefab/{prefabName}.prefab";
        var directPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(directPath);
        if (directPrefab != null)
        {
            field.SetValue(template, directPrefab);
            Debug.Log($"[PlayerTemplate] Assigned {prefabName} prefab from direct path: {directPrefab.name} ({directPath})");
            return true;
        }

        // Fallback: search by name
        string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.name.ToLower().Contains(prefabName.ToLower()))
            {
                field.SetValue(template, prefab);
                Debug.Log($"[PlayerTemplate] Assigned {prefabName} prefab: {prefab.name} ({path})");
                return true;
            }
        }
        Debug.LogWarning($"[PlayerTemplate] Could not find prefab for {prefabName}.");
        return false;
    }
}
#endif
