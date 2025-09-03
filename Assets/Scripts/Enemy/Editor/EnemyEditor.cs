using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace UndeadSurvivalGame.Editor
{
    [CustomEditor(typeof(UndeadSurvivalGame.EnemySystems.Enemy))]
    public class EnemyEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UndeadSurvivalGame.EnemySystems.Enemy enemy = (UndeadSurvivalGame.EnemySystems.Enemy)target;
            GUILayout.Space(10);

            // Static lock toggle
            if (!EditorPrefs.HasKey("EnemyAutoSetup_Locked"))
                EditorPrefs.SetBool("EnemyAutoSetup_Locked", false);
            bool autoSetupLocked = EditorPrefs.GetBool("EnemyAutoSetup_Locked");

            // Overwrite toggle
            bool overwriteExisting = EditorPrefs.GetBool("EnemyAutoSetup_Overwrite");
            overwriteExisting = EditorGUILayout.ToggleLeft("Overwrite Existing Values", overwriteExisting);
            EditorPrefs.SetBool("EnemyAutoSetup_Overwrite", overwriteExisting);

            EditorGUILayout.HelpBox("If checked, all values will be overwritten with those from the EnemyTemplate asset.", MessageType.Info);
            GUILayout.Space(5);

            // Lock toggle
            autoSetupLocked = EditorGUILayout.ToggleLeft("\U0001F512 Lock Auto Setup Button (prevent accidental press)", autoSetupLocked);
            EditorPrefs.SetBool("EnemyAutoSetup_Locked", autoSetupLocked);
            GUILayout.Space(5);

            // Big button style
            GUIStyle bigButton = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                fixedHeight = 40,
                margin = new RectOffset(0, 0, 10, 10)
            };

            EditorGUI.BeginDisabledGroup(autoSetupLocked);
            if (GUILayout.Button(autoSetupLocked ? "Auto Setup Enemy (Locked)" : "Auto Setup Enemy", bigButton))
            {
                EnemyAutoSetupUtility.AutoSetupReferences(enemy, overwriteExisting);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}