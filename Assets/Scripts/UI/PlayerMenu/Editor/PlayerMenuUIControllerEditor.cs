using UndeadSurvivalGame.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UndeadSurvivalGame.UI
{ 
    [CustomEditor(typeof(PlayerMenuUIController))]
    public class PlayerMenuUIControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PlayerMenuUIController controller = (PlayerMenuUIController)target;

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
                var so = new SerializedObject(target);

                var playerMenuUI = GameObject.Find("PlayerMenu/RightContainer/PlayerMenuUI");
                if (playerMenuUI != null)
                {
                    AssignObjectReference(so, "PlayerMenuUI", playerMenuUI);
                    AssignObjectReference(so, "menuCanvasGroup", GetOrWarn<CanvasGroup>(playerMenuUI, "CanvasGroup"));
                    AssignObjectReference(so, "uiInput", GetOrWarn<UIInput>(GameObject.Find("UIInput"), "UIInput"));
                    AssignObjectReference(so, "inventory", GetOrWarn<Inventory>(GameObject.Find("Player/Cowboy"), "Inventory"));
                    AssignObjectReference(so, "inventoryGridUIController", GetOrWarnInChildren<InventoryGridUIController>(playerMenuUI, "InventoryGridUIController"));
                    AssignObjectReference(so, "contextMenuController", GetOrWarnInChildren<ContextMenuController>(playerMenuUI, "ContextMenuController"));
                    AssignObjectReference(so, "inputActions", Resources.Load<InputActionAsset>("InputSystem_Actions"));

                    so.ApplyModifiedProperties();
                    Debug.Log("PlayerMenuUI and related references assigned automatically.");
                }
                else
                {
                    Debug.LogWarning("Could not find PlayerMenu/RightContainer/PlayerMenuUI in the scene.");
                }
            }
        }

        private void AssignObjectReference(SerializedObject so, string propertyName, Object value)
        {
            var prop = so.FindProperty(propertyName);
            prop.objectReferenceValue = value;
        }

        private T GetOrWarn<T>(GameObject go, string label) where T : Component
        {
            if (go == null)
            {
                Debug.LogWarning($"No GameObject found for {label}.");
                return null;
            }
            var comp = go.GetComponent<T>();
            if (comp == null)
                Debug.LogWarning($"No {typeof(T).Name} found on {go.name}.");
            return comp;
        }

        private T GetOrWarnInChildren<T>(GameObject go, string label) where T : Component
        {
            if (go == null)
            {
                Debug.LogWarning($"No GameObject found for {label}.");
                return null;
            }
            var comp = go.GetComponentInChildren<T>(true);
            if (comp == null)
                Debug.LogWarning($"No {typeof(T).Name} found in {go.name}'s children.");
            return comp;
        }
    }
}