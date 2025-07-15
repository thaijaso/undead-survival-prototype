using UnityEngine;

namespace UndeadSurvivalGame.Editor
{
    public static class EnemyAutoSetupUtility
    {
        public static void AutoSetupReferences(Enemy enemy, bool overwriteExisting = false)
        {
            Debug.Log($"[{enemy.gameObject.name}] Auto-Setting up references...");

            if (enemy == null)
            {
                Debug.LogError("Enemy reference is null. Cannot auto-setup.");
                return;
            }

            SetupEnemyTemplate(enemy);

            if (enemy.enemyTemplate == null)
            {
                Debug.LogError($"[{enemy.gameObject.name}] EnemyTemplate is not assigned. Cannot auto-setup.");
                return;
            }
        }

        private static void SetupEnemyTemplate(Enemy enemy)
        {
            if (enemy == null)
                return;
            if (enemy.enemyTemplate == null)
            {
                // Try to find any ZombieEnemyTemplate asset in the project
                string[] guids = UnityEditor.AssetDatabase.FindAssets("ZombieEnemyTemplate t:ScriptableObject");
                if (guids != null && guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    var mainAssembly = typeof(Enemy).Assembly;
                    var enemyTemplateType = mainAssembly.GetType("EnemyTemplate");
                    var loadedTemplate = UnityEditor.AssetDatabase.LoadAssetAtPath(path, enemyTemplateType);
                    if (loadedTemplate != null)
                    {
                        var templateProp = typeof(Enemy).GetProperty("enemyTemplate");
                        if (templateProp != null && templateProp.CanWrite)
                        {
                            templateProp.SetValue(enemy, loadedTemplate);
                            UnityEditor.EditorUtility.SetDirty(enemy);
                            Debug.Log($"[{enemy.gameObject.name}] AutoSetupReferences: Assigned ZombieEnemyTemplate from {path}.");
                        }
                        else
                        {
                            var templateField = typeof(Enemy).GetField("enemyTemplate");
                            if (templateField != null)
                            {
                                templateField.SetValue(enemy, loadedTemplate);
                                UnityEditor.EditorUtility.SetDirty(enemy);
                                Debug.Log($"[{enemy.gameObject.name}] AutoSetupReferences: Assigned ZombieEnemyTemplate from {path} (via field).");
                            }
                            else
                            {
                                Debug.LogWarning($"[{enemy.gameObject.name}] AutoSetupReferences: Could not assign ZombieEnemyTemplate to enemy (no property or field found).");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[{enemy.gameObject.name}] AutoSetupReferences: Could not load ZombieEnemyTemplate at {path}.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[{enemy.gameObject.name}] AutoSetupReferences: No ZombieEnemyTemplate asset found in project.");
                }
            }
        }

        private static void SetupAnimator()
        {

        }
    }
}