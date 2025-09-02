using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerMenuUIController))]
public class PlayerMenuUIControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerMenuUIController controller = (PlayerMenuUIController)target;

        // Show warning if PlayerMenuUI is not set
        if (controller.PlayerMenuUI == null)
        {
            EditorGUILayout.HelpBox(
                "PlayerMenuUI GameObject is not assigned! " +
                "You can assign it manually or use the Auto Setup button below.",
                MessageType.Warning
            );
        }

        if (GUILayout.Button("Auto Setup PlayerMenuUI"))
        {
            var found = GameObject.Find("PlayerMenuContainer/RightContainer/PlayerMenuUI");
            if (found != null)
            {
                controller.PlayerMenuUI = found;
                Debug.Log("PlayerMenuUI assigned automatically.");
            }
            else
            {
                Debug.LogWarning("PlayerMenuUI GameObject not found at path 'PlayerMenuContainer/RightContainer/PlayerMenuUI'.");
            }
        }
    }
}