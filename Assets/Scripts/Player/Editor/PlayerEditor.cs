using UnityEditor;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace UndeadSurvivalGame.Editor
{
    [CustomEditor(typeof(Player))]
    public class PlayerEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Player player = (Player)target;
            GUILayout.Space(10);
            if (GUILayout.Button("Auto-Setup References", GUILayout.Height(32)))
            {
                PlayerAutoSetupUtility.AutoSetupReferences(player);
            }
        }
    }
}
