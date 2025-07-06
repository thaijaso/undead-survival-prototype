using UnityEditor;
using UnityEngine;

public class RemoveMissingScripts
{
    [MenuItem("Tools/Remove Missing Scripts in Scene")]
    static void RemoveAllMissingScripts()
    {
        int count = 0;
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0) count += removed;
        }
        Debug.Log($"Removed {count} missing scripts.");
    }
}