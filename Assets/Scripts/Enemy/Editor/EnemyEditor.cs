using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace UndeadSurvivalGame.Editor
{
    [CustomEditor(typeof(Enemy))]
    public class EnemyEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Enemy enemy = (Enemy)target;
            GUILayout.Space(10);
            if (GUILayout.Button("Auto-Setup References", GUILayout.Height(32)))
            {
                EnemyAutoSetupUtility.AutoSetupReferences(enemy);
            }
        }
    }
}