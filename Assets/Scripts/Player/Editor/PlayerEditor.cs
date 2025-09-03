using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace UndeadSurvivalGame.Editor
{
    [CustomEditor(typeof(PlayerSystems.Player))]
    public class PlayerEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PlayerSystems.Player player = (PlayerSystems.Player)target;
            GUILayout.Space(10);

            // Static lock toggle
            if (!EditorPrefs.HasKey("PlayerAutoSetup_Locked"))
                EditorPrefs.SetBool("PlayerAutoSetup_Locked", false);
            bool autoSetupLocked = EditorPrefs.GetBool("PlayerAutoSetup_Locked");

            // Overwrite toggle
            bool overwriteExisting = EditorPrefs.GetBool("PlayerAutoSetup_Overwrite");
            overwriteExisting = EditorGUILayout.ToggleLeft("Overwrite Existing Values", overwriteExisting);
            EditorPrefs.SetBool("PlayerAutoSetup_Overwrite", overwriteExisting);

            EditorGUILayout.HelpBox("If checked, all values will be overwritten with those from the PlayerTemplate asset.", MessageType.Info);
            GUILayout.Space(5);

            // Lock toggle
            autoSetupLocked = EditorGUILayout.ToggleLeft("\U0001F512 Lock Auto Setup Button (prevent accidental press)", autoSetupLocked);
            EditorPrefs.SetBool("PlayerAutoSetup_Locked", autoSetupLocked);
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
            if (GUILayout.Button(autoSetupLocked ? "Auto Setup Player (Locked)" : "Auto Setup Player", bigButton))
            {
                PlayerAutoSetupUtility.AutoSetupReferences(player, overwriteExisting);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
