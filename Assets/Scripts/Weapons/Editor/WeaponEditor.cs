#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UndeadSurvivalGame.Editor;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

#if ODIN_INSPECTOR
[CustomEditor(typeof(Weapon))]
public class WeaponEditor : OdinEditor
#else
[CustomEditor(typeof(Weapon))]
public class WeaponEditor : Editor
#endif
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Weapon weapon = (Weapon)target;
        if (GUILayout.Button("Auto-Setup Prefab References"))
        {
            WeaponAutoSetupUtility.AutoSetupPrefabReferences(weapon, true);
        }
    }
}
#endif
