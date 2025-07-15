using UnityEngine;

namespace UndeadSurvivalGame.Editor
{
    public static class EnemyAutoSetupUtility
    {
        public static void AutoSetupReferences(Enemy enemy, bool overwriteExisting = false)
        {
            Debug.Log($"[{enemy.gameObject.name}] Auto-Setting up references...");
        }

        private static void SetupEnemyTemplate(Enemy enemy)
        {
            
        }

        private static void SetupAnimator()
        {

        }
    }
}