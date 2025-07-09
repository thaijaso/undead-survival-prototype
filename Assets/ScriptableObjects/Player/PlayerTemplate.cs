using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

[CreateAssetMenu(fileName = "PlayerTemplate", menuName = "ScriptableObjects/PlayerTemplate")]
public partial class PlayerTemplate : ScriptableObject
{
    [TabGroup("Health")]
    [MinValue(1)]
    [SuffixLabel("HP")]
    public int maxHealth = 100;

    [TabGroup("Movement")]
    [MinValue(0.1f)]
    [SuffixLabel("units/sec")]
    public float strafeSpeed = 2f;
    
    [TabGroup("Movement")]
    [MinValue(0.1f)]
    [SuffixLabel("units/sec")]
    public float sprintSpeed = 5f;

    [TabGroup("Movement")]
    [Range(-20f, -5f)]
    [SuffixLabel("m/s²")]
    [InfoBox("Negative values pull the player downward. Standard Earth gravity is -9.81 m/s²")]
    public float gravity = -9.81f;

    [TabGroup("Animation")]
    [InfoBox("Assign the animator controller for this player template.")]
    public RuntimeAnimatorController animatorController;

    [TabGroup("References")]
    [InfoBox("Assign the prefab reference for the ForwardFollowTarget child object (not an in-game object).")]
    public GameObject followTargetPrefab;

    [TabGroup("References")]
    [InfoBox("Assign the prefab reference for the BulletHitTarget child object (not an in-game object).")]
    public GameObject bulletHitTargetPrefab;

    [TabGroup("References")]
    [InfoBox("Assign the prefab reference for the Player's WeaponHand (not an in-game object).")]
    public GameObject weaponHandPrefab;

    [TabGroup("References")]
    [InfoBox("Assign the prefab reference for the AimIKTarget child object (not an in-game object).")]
    public GameObject aimIKTargetPrefab;

    [TabGroup("References")]
    [InfoBox("Assign the prefab reference for LeftHandIKTarget child object (not an in-game object).")]
    public GameObject leftHandIKTargetPrefab;

    [TabGroup("Camera")]
    [MinValue(1f)]
    [SuffixLabel("deg")] public float followFOV = 40f;
    [TabGroup("Camera")]
    [MinValue(1f)]
    [SuffixLabel("deg")] public float aimFOV = 28.7f;
    [TabGroup("Camera")]
    [MinValue(0.01f)]
    [SuffixLabel("units/sec")] public float zoomSpeed = 5f;
    [TabGroup("Camera")]
    [SuffixLabel("units")] public float aimCamOffsetX = 0.5f;
    [TabGroup("Camera")]
    [SuffixLabel("units/sec")] public float offsetLerpSpeed = 5f;

    [TabGroup("PlayerInput")]
    [InfoBox("Player input settings. Tune input thresholds for this player template.\n- movementThreshold: Minimum input magnitude to register movement.\n- animationSmoothTime: Smoothing time for input-driven animation blending.\n- maxInputThreshold: Maximum input magnitude for full movement response.")]
    public float movementThreshold = 0.2f;
    [TabGroup("PlayerInput")]
    public float animationSmoothTime = 0.05f;
    [TabGroup("PlayerInput")]
    public float maxInputThreshold = 0.6f;

    [TabGroup("CharacterController")]
    [MinValue(0f)]
    [SuffixLabel("deg")]
    public float slopeLimit = 45f;
    [TabGroup("CharacterController")]
    [MinValue(0f)]
    public float stepOffset = 0.3f;
    [TabGroup("CharacterController")]
    [MinValue(0f)]
    public float skinWidth = 0.08f;
    [TabGroup("CharacterController")]
    [MinValue(0f)]
    public float minMoveDistance = 0.001f;
    [TabGroup("CharacterController")]
    public Vector3 center = new Vector3(0f, 1f, 0f);
    [TabGroup("CharacterController")]
    [MinValue(0f)]
    public float radius = 0.3f;
    [TabGroup("CharacterController")]
    [MinValue(0f)]
    public float height = 1.8f;

    [TabGroup("CharacterController")]
    public int layerOverridePriority = 0;
    [TabGroup("CharacterController")]
    public LayerMask includeLayers = 0;
    [TabGroup("CharacterController")]
    public LayerMask excludeLayers = 0;

    [TabGroup("Debug")]
    [Button("Validate Settings")]
    [InfoBox("Validates template settings and checks for potential configuration issues")]
    private void ValidateSettings()
    {
        Debug.Log($"[PlayerTemplate] Player Settings for {name}:");
        Debug.Log($"[PlayerTemplate]   Max Health: {maxHealth} HP");
        Debug.Log($"[PlayerTemplate]   Strafe Speed: {strafeSpeed} units/sec");
        Debug.Log($"[PlayerTemplate]   Sprint Speed: {sprintSpeed} units/sec");
        Debug.Log($"[PlayerTemplate]   Gravity: {gravity} m/s²");
        Debug.Log($"[PlayerTemplate]   Follow FOV: {followFOV} deg");
        Debug.Log($"[PlayerTemplate]   Aim FOV: {aimFOV} deg");
        Debug.Log($"[PlayerTemplate]   Zoom Speed: {zoomSpeed} units/sec");
        Debug.Log($"[PlayerTemplate]   BulletHitTarget Prefab: {(bulletHitTargetPrefab != null ? bulletHitTargetPrefab.name : "null")}");

        // --- Reference validation ---
        if (animatorController == null)
            Debug.LogWarning($"[PlayerTemplate] ⚠️ AnimatorController is not assigned!");
        if (followTargetPrefab == null)
            Debug.LogWarning($"[PlayerTemplate] ⚠️ FollowTargetPrefab is not assigned!");
        if (bulletHitTargetPrefab == null)
            Debug.LogWarning($"[PlayerTemplate] ⚠️ BulletHitTargetPrefab is not assigned!");
        if (weaponHandPrefab == null)
            Debug.LogWarning($"[PlayerTemplate] ⚠️ WeaponHandPrefab is not assigned!");
        if (aimIKTargetPrefab == null)
            Debug.LogWarning($"[PlayerTemplate] ⚠️ AimIKTargetPrefab is not assigned!");
        // --- End reference validation ---
        
        if (sprintSpeed <= strafeSpeed)
            Debug.LogWarning("[PlayerTemplate] ⚠️ Sprint speed should be faster than strafe speed!");
            
        if (gravity > -5f)
            Debug.LogWarning("[PlayerTemplate] ⚠️ Gravity seems too weak (should be more negative)!");
            
        if (gravity < -20f)
            Debug.LogWarning("[PlayerTemplate] ⚠️ Gravity seems too strong (player will fall too fast)!");
    }

#if UNITY_EDITOR
    [TabGroup("References")]
    [Button("Auto-Setup References")]
    [InfoBox("Automatically finds and assigns prefab references for this PlayerTemplate asset.")]
    [ShowInInspector]
    public void AutoSetupReferences()
    {
        var unityObj = this as UnityEngine.Object;
        string assetName = unityObj != null ? unityObj.name : "(unknown)";
        Debug.Log($"[PlayerTemplate] Auto-Setup References button pressed for {assetName}.");
        bool changed = false;
        changed |= TryAssignPrefabByName("followTargetPrefab", "FollowTarget");
        changed |= TryAssignPrefabByName("bulletHitTargetPrefab", "BulletHitTarget");
        changed |= TryAssignPrefabByName("weaponHandPrefab", "WeaponHand");
        changed |= TryAssignPrefabByName("aimIKTargetPrefab", "AimIKTarget");
        changed |= TryAssignPrefabByName("leftHandIKTargetPrefab", "LeftHandIKTarget");
        // Add more as needed
        if (changed)
        {
            EditorUtility.SetDirty(unityObj);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PlayerTemplate] Auto-Setup complete for {assetName}.");
        }
        else
        {
            Debug.LogWarning($"[PlayerTemplate] Auto-Setup found no assignable prefabs for {assetName}.");
        }
    }

    private bool TryAssignPrefabByName(string fieldName, string prefabName)
    {
        var field = this.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null) {
            Debug.LogWarning($"[PlayerTemplate] Field '{fieldName}' not found.");
            return false;
        }
        var current = field.GetValue(this) as GameObject;
        if (current != null) return false;

        // First, try to find prefab at Assets/Player/Prefab/{PrefabName}.prefab
        string directPath = $"Assets/Player/Prefab/{prefabName}.prefab";
        var directPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(directPath);
        if (directPrefab != null)
        {
            field.SetValue(this, directPrefab);
            Debug.Log($"[PlayerTemplate] Assigned {prefabName} prefab from direct path: {directPrefab.name} ({directPath})");
            return true;
        }

        // Fallback: search by name
        string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.name.ToLower().Contains(prefabName.ToLower()))
            {
                field.SetValue(this, prefab);
                Debug.Log($"[PlayerTemplate] Assigned {prefabName} prefab: {prefab.name} ({path})");
                return true;
            }
        }
        Debug.LogWarning($"[PlayerTemplate] Could not find prefab for {prefabName}.");
        return false;
    }
#endif
}
