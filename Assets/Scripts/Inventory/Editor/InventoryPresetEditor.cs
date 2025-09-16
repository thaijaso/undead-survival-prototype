using UnityEditor;
using UndeadSurvivalGame.Gameplay;

[CustomEditor(typeof(InventoryPreset))]
public class InventoryPresetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var preset = (InventoryPreset)target;
        if (preset.startingItems != null)
        {
            foreach (var stack in preset.startingItems)
            {
                if (stack != null && stack.quantity < 1)
                {
                    stack.quantity = 1;
                    EditorUtility.SetDirty(preset);
                }
            }
        }
    }
}