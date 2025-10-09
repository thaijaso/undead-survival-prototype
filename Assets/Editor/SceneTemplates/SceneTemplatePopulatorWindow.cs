using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTemplatePopulatorWindow : EditorWindow
{
    SceneTemplate template;
    bool replaceExisting = true;

    [MenuItem("Tools/Scene Template Populator")]
    static void Open() => GetWindow<SceneTemplatePopulatorWindow>("Scene Populator");

    void OnGUI()
    {
        EditorGUILayout.LabelField("Minimal Scene Template Populator", EditorStyles.boldLabel);
        template = (SceneTemplate)EditorGUILayout.ObjectField("Template", template, typeof(SceneTemplate), false);
        replaceExisting = EditorGUILayout.Toggle("Replace existing", replaceExisting);

        if (template == null)
        {
            EditorGUILayout.HelpBox("Select a SceneTemplate asset to populate the scene.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Populate Scene"))
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "Cannot populate while in Play Mode.", "OK");
            }
            else
            {
                PopulateScene();
            }
        }

        if (GUILayout.Button("Clear Instances from Template"))
        {
            ClearInstances();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Ping Template Asset"))
            EditorGUIUtility.PingObject(template);
    }

    void PopulateScene()
    {
        // If requested, remove previous instances created from this template
        if (replaceExisting)
        {
            var existing = Object.FindObjectsByType<SceneTemplateInstance>(FindObjectsSortMode.None);
            for (int i = existing.Length - 1; i >= 0; i--)
            {
                if (existing[i].template == template)
                {
                    Undo.DestroyObjectImmediate(existing[i].gameObject);
                }
            }
        }

        foreach (var prefab in template.prefabs)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"Template '{template.name}' contains a null prefab reference. Skipping.");
                continue;
            }

            var goObj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (goObj == null)
            {
                Debug.LogWarning($"Failed to instantiate prefab {prefab.name}");
                continue;
            }

            // leave at scene root (no parent)
            Undo.RegisterCreatedObjectUndo(goObj, "Populate Template");

            var marker = goObj.GetComponent<SceneTemplateInstance>();
            if (marker == null) marker = goObj.AddComponent<SceneTemplateInstance>();
            marker.template = template;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    void ClearInstances()
    {
        var all = Object.FindObjectsByType<SceneTemplateInstance>(FindObjectsSortMode.None);
        int removed = 0;
        foreach (var t in all)
        {
            if (t.template == template)
            {
                Undo.DestroyObjectImmediate(t.gameObject);
                removed++;
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Clear Instances", $"Removed {removed} objects for template '{template.name}'.", "OK");
    }
}
