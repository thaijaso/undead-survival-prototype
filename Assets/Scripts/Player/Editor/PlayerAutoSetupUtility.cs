using Pathfinding;
using RootMotion;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using UndeadSurvivalGame.Gameplay;
using UndeadSurvivalGame.Effects;
using UndeadSurvivalGame.PlayerSystems;
using UndeadSurvivalGame.UI;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UndeadSurvivalGame.Editor
{
    public static class PlayerAutoSetupUtility
    {
        public static void AutoSetupReferences(Player player, bool overwriteExisting = true)
        {
            if (player == null)
            {
                Debug.LogError("PlayerAutoSetupUtility: Player reference is null. Aborting auto-setup.");
                return;
            }

            SetupPlayerTemplate(player);

            if (player.playerTemplate == null)
            {
                Debug.LogError($"[{player.gameObject.name}] AutoSetup: PlayerTemplate is not assigned. Aborting auto-setup.");
                return;
            }

            SetupPlayerCharacterController(player);
            SetupAnimator(player, overwriteExisting);
            SetupPlayerFollowTarget(player);
            SetupPlayerBulletHitTarget(player);
            SetupPlayerWeaponHand(player);
            SetupPlayerAimIKTarget(player);
            SetupPlayerLeftHandIKTarget(player, overwriteExisting);
            var cameraTargets = SetupCameraTargetsAndSettings(player, overwriteExisting);
            SetupCrosshairController(player);
            SetupOverlayUIController(player);
            SetupCurrentWeaponUIController(player);
            SetupHealthUIController(player);
            SetupPlayerMenuUIController(player);
            SetupPlayerInput(player, overwriteExisting);
            SetupCharacterControllerFromTemplate(player, overwriteExisting);
            SetupPlayerWeaponManager(player, overwriteExisting);
            SetupAimIK(player, overwriteExisting, cameraTargets.aimIKTarget);
            SetupLeftHandElbowBendGoal(player, overwriteExisting);
            SetupFBBIK(player, overwriteExisting);
            SetupRecoilIK(player, overwriteExisting);
            SetupBulletDecalManager(player);
            SetupHealthManager(player);
            SetupBulletHitscan(player);
            SetupPlayerDebugger(player);
            SetupPlayerAnimatorEvents(player);
            SetupPlayerComponentReferences(player);
            SetupBipedRagdollCreator(player, overwriteExisting);
            SetupPlayerInventory(player);
            SetupInventoryBootstrap(player);
            SetupAimPoseLayerWeightController(player, overwriteExisting);
            SetupAimPitchLayerWeightController(player, overwriteExisting);
            SetupUpperBodyLayerWeightController(player, overwriteExisting);
            SetupAlphaCutoffController(player, overwriteExisting);
            SetupCenterZoneOverlapCalculator(player, overwriteExisting);
            SetupPlayerInteractionSensor(player, overwriteExisting);
            SetupWallDetector(player, overwriteExisting);
            SetupStairDetector(player, overwriteExisting);
            SetupFadeCollider(player, overwriteExisting);
            AssignPlayerToEnemies(player);
            
            if (player.gameObject != null)
                SetLayerRecursively(player.gameObject, LayerMask.NameToLayer("Player"));

            Debug.Log($"[{player.gameObject.name}] Auto-setup complete.");
            EditorUtility.SetDirty(player);
        }

        private static void SetupPlayerTemplate(Player player)
        {
            if (player.playerTemplate == null)
            {
                // Try to find any PlayerTemplate asset in the project
                string[] guids = AssetDatabase.FindAssets("t:PlayerTemplate");
                if (guids != null && guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var mainAssembly = typeof(Player).Assembly;
                    var playerTemplateType = mainAssembly.GetType("PlayerTemplate");
                    var loadedTemplate = AssetDatabase.LoadAssetAtPath(path, playerTemplateType);
                    if (loadedTemplate != null)
                    {
                        var playerTemplateProp = typeof(Player).GetProperty("playerTemplate");
                        if (playerTemplateProp != null && playerTemplateProp.CanWrite)
                        {
                            playerTemplateProp.SetValue(player, loadedTemplate);
                            Debug.Log($"[{player.gameObject.name}] AutoSetupReferences: Assigned PlayerTemplate from {path}.");
                        }
                        else
                        {
                            var playerTemplateField = typeof(Player).GetField("playerTemplate");
                            if (playerTemplateField != null)
                            {
                                playerTemplateField.SetValue(player, loadedTemplate);
                                Debug.Log($"[{player.gameObject.name}] AutoSetupReferences: Assigned PlayerTemplate from {path} (via field).");
                            }
                            else
                            {
                                Debug.LogWarning($"[{player.gameObject.name}] AutoSetupReferences: Could not assign PlayerTemplate to player (no property or field found).");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[{player.gameObject.name}] AutoSetupReferences: Could not load PlayerTemplate at {path}.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[{player.gameObject.name}] AutoSetupReferences: No PlayerTemplate asset found in project.");
                }
            }
        }

        private static void SetupPlayerDebugger(Player player)
        {
            if (player == null) return;
            var playerDebugger = player.GetComponent<PlayerDebugger>();
            if (playerDebugger == null)
            {
                playerDebugger = player.gameObject.AddComponent<PlayerDebugger>();
                Debug.Log($"[AutoSetup] PlayerDebugger component added to {player.gameObject.name}.");
            }
        }

        private static void SetupAnimator(Player player, bool overwriteExisting = true)
        {
            if (player == null) return;
            var animator = player.GetComponent<Animator>();
            if (animator != null && player.playerTemplate != null && player.playerTemplate.animatorController != null)
            {
                var before = animator.runtimeAnimatorController;
                if (overwriteExisting)
                {
                    Debug.Log($"[AutoSetup] AnimatorController before: {(before != null ? before.name : "null")}, template: {player.playerTemplate.animatorController.name}, overwrite: {overwriteExisting}");
                    if (before != player.playerTemplate.animatorController)
                        Debug.Log($"[AutoSetup] Overwriting Animator.runtimeAnimatorController: {(before != null ? before.name : "null")} -> {player.playerTemplate.animatorController.name}");
                    animator.runtimeAnimatorController = player.playerTemplate.animatorController;
                    animator.applyRootMotion = false; // Disable root motion if overwriting
                    Debug.Log($"[AutoSetup] AnimatorController after: {animator.runtimeAnimatorController.name}, applyRootMotion: {animator.applyRootMotion}");
                }
                else if (animator.runtimeAnimatorController == null)
                {
                    animator.runtimeAnimatorController = player.playerTemplate.animatorController;
                }
            }
        }

        private static void SetupPlayerInput(Player player, bool overwriteExisting = true)
        {
            if (player == null || player.playerTemplate == null)
                return;

            // Ensure PlayerInput component exists
            if (player.PlayerInput == null)
            {
                var playerInput = player.GetComponent<PlayerInput>();
                if (playerInput == null)
                {
                    playerInput = player.gameObject.AddComponent<PlayerInput>();
                    Debug.Log($"[AutoSetup] PlayerInput component added to {player.gameObject.name}.");
                }
                else
                {
                    Debug.Log($"[AutoSetup] PlayerInput component already exists on {player.gameObject.name}.");
                }
                // Explicitly set the property on Player for robustness
                var prop = typeof(Player).GetProperty("PlayerInput");
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(player, playerInput);
                    Debug.Log($"[AutoSetup] Explicitly set Player.PlayerInput property after ensuring component exists.");
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] Could not set PlayerInput property on Player {player.gameObject.name} (property not found or not writable).");
                }
            }
            if (player.PlayerInput == null)
                return;

            var type = player.PlayerInput.GetType();
            var inputObj = player.PlayerInput;

            SetPlayerInputThreshold(type, inputObj, player.playerTemplate.movementThreshold, "movementThreshold", overwriteExisting);
            SetPlayerInputThreshold(type, inputObj, player.playerTemplate.animationSmoothTime, "animationSmoothTime", overwriteExisting);
            SetPlayerInputThreshold(type, inputObj, player.playerTemplate.maxInputThreshold, "maxInputThreshold", overwriteExisting);

            if (overwriteExisting)
            {
                Debug.Log($"[{player.gameObject.name}] AutoSetupReferences: Set PlayerInput thresholds from PlayerTemplate. Overwrite: {overwriteExisting}");
            }
        }

        private static void SetupCharacterControllerFromTemplate(Player player, bool overwriteExisting = true)
        {
            if (player == null)
            {
                Debug.LogWarning("[AutoSetup] Player is null in SetupCharacterControllerFromTemplate.");
                return;
            }
            if (player.playerTemplate == null)
            {
                Debug.LogWarning($"[AutoSetup] PlayerTemplate is null for {player.gameObject?.name} in SetupCharacterControllerFromTemplate.");
                return;
            }
            if (player.PlayerCharacterController == null)
            {
                Debug.LogWarning($"[AutoSetup] PlayerCharacterController is null for {player.gameObject?.name} in SetupCharacterControllerFromTemplate.");
                return;
            }
            var cc = player.PlayerCharacterController.CharacterController;
            if (cc == null)
            {
                cc = player.gameObject.GetComponent<CharacterController>();
                if (cc == null)
                {
                    Debug.LogWarning($"[AutoSetup] CharacterController is null for {player.gameObject?.name} in SetupCharacterControllerFromTemplate, and could not be found on the GameObject.");
                    return;
                }
                else
                {
                    Debug.Log($"[AutoSetup] CharacterController was not set on PlayerCharacterController, but was found on the GameObject and will be used.");
                }
            }

            var template = player.playerTemplate;

            if (overwriteExisting)
            {
                if (cc.slopeLimit != template.slopeLimit)
                    Debug.Log($"[AutoSetup] Overwriting CharacterController.slopeLimit: {cc.slopeLimit} -> {template.slopeLimit}");
                cc.slopeLimit = template.slopeLimit;
                if (cc.stepOffset != template.stepOffset)
                    Debug.Log($"[AutoSetup] Overwriting CharacterController.stepOffset: {cc.stepOffset} -> {template.stepOffset}");
                cc.stepOffset = template.stepOffset;
                if (cc.skinWidth != template.skinWidth)
                    Debug.Log($"[AutoSetup] Overwriting CharacterController.skinWidth: {cc.skinWidth} -> {template.skinWidth}");
                cc.skinWidth = template.skinWidth;
                if (cc.minMoveDistance != template.minMoveDistance)
                    Debug.Log($"[AutoSetup] Overwriting CharacterController.minMoveDistance: {cc.minMoveDistance} -> {template.minMoveDistance}");
                cc.minMoveDistance = template.minMoveDistance;
                if (cc.center != template.center)
                    Debug.Log($"[AutoSetup] Overwriting CharacterController.center: {cc.center} -> {template.center}");
                cc.center = template.center;
                if (cc.radius != template.radius)
                    Debug.Log($"[AutoSetup] Overwriting CharacterController.radius: {cc.radius} -> {template.radius}");
                cc.radius = template.radius;
                if (cc.height != template.height)
                    Debug.Log($"[AutoSetup] Overwriting CharacterController.height: {cc.height} -> {template.height}");
                cc.height = template.height;
            }
            else
            {
                if (cc.slopeLimit == default) cc.slopeLimit = template.slopeLimit;
                if (cc.stepOffset == default) cc.stepOffset = template.stepOffset;
                if (cc.skinWidth == default) cc.skinWidth = template.skinWidth;
                if (cc.minMoveDistance == default) cc.minMoveDistance = template.minMoveDistance;
                if (cc.center == default) cc.center = template.center;
                if (cc.radius == default) cc.radius = template.radius;
                if (cc.height == default) cc.height = template.height;
            }

            Debug.Log($"[{player.gameObject.name}] AutoSetupReferences: Set CharacterController values from PlayerTemplate. Overwrite: {overwriteExisting}");
        }

        private static void SetPlayerInputThreshold(System.Type type, object inputObj, float templateValue, string fieldName, bool overwriteExisting)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                float current = (float)field.GetValue(inputObj);
                if (overwriteExisting)
                {
                    Debug.Log($"[AutoSetup] {fieldName} (field) before: {current}, template: {templateValue}, overwrite: {overwriteExisting}");
                    if (current != templateValue)
                        Debug.Log($"[AutoSetup] Overwriting PlayerInput.{fieldName} (field): {current} -> {templateValue}");
                    field.SetValue(inputObj, templateValue);
                    Debug.Log($"[AutoSetup] {fieldName} (field) after: {(float)field.GetValue(inputObj)}");
                }
                else if (current == default)
                {
                    field.SetValue(inputObj, templateValue);
                }
            }
            else
            {
                SetPlayerInputThresholdProperty(type, inputObj, templateValue, fieldName, overwriteExisting);
            }
        }

        private static void SetPlayerInputThresholdProperty(System.Type type, object inputObj, float templateValue, string propertyName, bool overwriteExisting)
        {
            var prop = type.GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                float current = (float)prop.GetValue(inputObj);
                if (overwriteExisting)
                {
                    Debug.Log($"[AutoSetup] {propertyName} (prop) before: {current}, template: {templateValue}, overwrite: {overwriteExisting}");
                    if (current != templateValue)
                        Debug.Log($"[AutoSetup] Overwriting PlayerInput.{propertyName} (prop): {current} -> {templateValue}");
                    prop.SetValue(inputObj, templateValue);
                    Debug.Log($"[AutoSetup] {propertyName} (prop) after: {(float)prop.GetValue(inputObj)}");
                }
                else if (current == default(float))
                {
                    prop.SetValue(inputObj, templateValue);
                }
            }
            else
            {
                Debug.LogWarning($"[AutoSetup] {propertyName}: No field or writable property found on PlayerInput.");
            }
        }

        private static void SetupPlayerWeaponManager(Player player, bool overwriteExisting = true)
        {
            if (player == null)
                return;
            // Try to find the PlayerWeaponManager component
            var weaponManager = player.GetComponent<PlayerWeaponManager>();
            if (weaponManager == null)
            {
                weaponManager = player.gameObject.AddComponent<PlayerWeaponManager>();
                Debug.Log($"[AutoSetup] PlayerWeaponManager component added to {player.gameObject.name}.");
            }
            // Try to load the RevolverWeaponConfig asset from a known path
            var revolverWeaponConfig = AssetDatabase.LoadAssetAtPath<WeaponConfig>("Assets/ScriptableObjects/Weapons/RevolverWeaponConfig.asset");
            if (revolverWeaponConfig != null)
            {
                // Try to set CurrentWeaponConfig via its public property (may have a non-public setter),
                // fall back to the compiler-generated backing field or an explicitly declared private field.
                var wmType = weaponManager.GetType();
                var prop = wmType.GetProperty("CurrentWeaponConfig", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                bool setSucceeded = false;
                if (prop != null)
                {
                    var currentValue = prop.GetValue(weaponManager) as WeaponConfig;
                    if (overwriteExisting || currentValue == null)
                    {
                        // Try invoking the setter even if it's non-public
                        var setMethod = prop.GetSetMethod(true);
                        if (setMethod != null)
                        {
                            setMethod.Invoke(weaponManager, new object[] { revolverWeaponConfig });
                            Debug.Log($"[AutoSetup] PlayerWeaponManager.CurrentWeaponConfig (property) set to RevolverWeaponConfig for {player.gameObject.name} (overwrite: {overwriteExisting}).");
                            setSucceeded = true;
                        }
                        else
                        {
                            // Try the auto-property backing field generated by the compiler
                            var backing = wmType.GetField("<CurrentWeaponConfig>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (backing != null)
                            {
                                backing.SetValue(weaponManager, revolverWeaponConfig);
                                Debug.Log($"[AutoSetup] PlayerWeaponManager.CurrentWeaponConfig (backing field) set to RevolverWeaponConfig for {player.gameObject.name} (overwrite: {overwriteExisting}).");
                                setSucceeded = true;
                            }
                        }
                    }
                    else
                    {
                        // nothing to do, already set and not overwriting
                        setSucceeded = true;
                    }
                }

                if (!setSucceeded)
                {
                    // Last-resort: try to find a private field named CurrentWeaponConfig (older code or explicit field)
                    var currentWeaponDataField = wmType.GetField("CurrentWeaponConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (currentWeaponDataField != null)
                    {
                        var currentValue = currentWeaponDataField.GetValue(weaponManager) as WeaponConfig;
                        if (overwriteExisting || currentValue == null)
                        {
                            currentWeaponDataField.SetValue(weaponManager, revolverWeaponConfig);
                            Debug.Log($"[AutoSetup] PlayerWeaponManager.CurrentWeaponConfig (private field) set to RevolverWeaponConfig for {player.gameObject.name} (overwrite: {overwriteExisting}).");
                            setSucceeded = true;
                        }
                    }
                }

                if (!setSucceeded)
                {
                    Debug.LogWarning($"[AutoSetup] Could not set CurrentWeaponConfig on PlayerWeaponManager for {player.gameObject.name}.");
                }
            }
            else
            {
                Debug.LogWarning($"[AutoSetup] Could not find RevolverWeaponConfig asset at Assets/ScriptableObjects/Weapons/RevolverWeaponConfig.asset");
            }
        }

        private static void SetupPlayerWeaponHand(Player player)
        {
            if (player == null || player.playerTemplate == null)
                return;

            // Find the hand bone (hand.r) in the player's hierarchy
            Transform handBone = FindChildRecursive(player.transform, "hand.r");
            if (handBone == null)
            {
                Debug.LogWarning($"[AutoSetup] Could not find hand.r bone on {player.gameObject.name}.");
                return;
            }

            // Check if WeaponHand already exists as a child of hand.r
            Transform weaponHand = FindDirectChildByName(handBone, "WeaponHand");
            if (weaponHand == null)
            {
                if (player.playerTemplate.followTargetPrefab != null) // Replace with weaponHandPrefab if available
                {
                    // If you have a weaponHandPrefab in PlayerTemplate, use it here instead of forwardFollowTargetPrefab
                    GameObject prefab = player.playerTemplate.weaponHandPrefab; // <-- Make sure this exists in PlayerTemplate
                    if (prefab != null)
                    {
                        GameObject newWeaponHand = (GameObject)PrefabUtility.InstantiatePrefab(prefab, handBone);
                        newWeaponHand.name = "WeaponHand";
                        weaponHand = newWeaponHand.transform;
                        Debug.Log($"Created WeaponHand as child of hand.r for {player.gameObject.name}.");
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoSetup] weaponHandPrefab is not assigned in PlayerTemplate for {player.gameObject.name}.");
                    }
                }
                else
                {
                    // Fallback: create an empty WeaponHand if prefab is missing
                    GameObject newWeaponHand = new GameObject("WeaponHand");
                    newWeaponHand.transform.SetParent(handBone);
                    newWeaponHand.transform.localPosition = Vector3.zero;
                    newWeaponHand.transform.localRotation = Quaternion.identity;
                    newWeaponHand.transform.localScale = Vector3.one;
                    weaponHand = newWeaponHand.transform;
                    Debug.Log($"Created empty WeaponHand as child of hand.r for {player.gameObject.name}.");
                }
            }
            else
            {
                Debug.Log($"Found existing WeaponHand as child of hand.r for {player.gameObject.name}.");
            }

            // Assign the Player.WeaponHand reference (private field) via reflection
            if (weaponHand != null)
            {
                var weaponHandField = typeof(Player).GetField("weaponHand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (weaponHandField != null)
                {
                    weaponHandField.SetValue(player, weaponHand);
                    Debug.Log($"[AutoSetup] Assigned Player.weaponHand reference for {player.gameObject.name}.");
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] Could not find private field 'weaponHand' on Player for {player.gameObject.name}.");
                }
            }
        }

        private static void SetupPlayerBulletHitTarget(Player player)
        {
            if (player == null || player.playerTemplate == null)
                return;

            // Find the BulletHitTarget in the player's hierarchy
            Transform bulletHitTarget = FindDirectChildByName(player.transform, "BulletHitTarget");
            if (bulletHitTarget == null)
            {
                GameObject prefab = player.playerTemplate.bulletHitTargetPrefab;
                if (prefab == null)
                {
                    // Try to find a prefab in the project named "BulletHitTarget" or of type GameObject
                    string[] guids = AssetDatabase.FindAssets("BulletHitTarget t:Prefab");
                    if (guids != null && guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        Debug.Log($"[AutoSetup] Fallback: Found BulletHitTarget prefab at {path}.");
                    }
                }
                if (prefab != null)
                {
                    GameObject newBulletHitTarget = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
                    newBulletHitTarget.name = "BulletHitTarget";
                    bulletHitTarget = newBulletHitTarget.transform;
                    Debug.Log($"Created BulletHitTarget from prefab as child of Player for {player.gameObject.name}.");
                }
                else
                {
                    GameObject newBulletHitTarget = new GameObject("BulletHitTarget");
                    newBulletHitTarget.transform.SetParent(player.transform);
                    newBulletHitTarget.transform.localPosition = Vector3.zero;
                    newBulletHitTarget.transform.localRotation = Quaternion.identity;
                    newBulletHitTarget.transform.localScale = Vector3.one;
                    bulletHitTarget = newBulletHitTarget.transform;
                    Debug.Log($"Created empty BulletHitTarget as child of Player for {player.gameObject.name} (no prefab assigned or found).");
                }
            }
            else
            {
                Debug.Log($"Found existing BulletHitTarget as child of Player for {player.gameObject.name}.");
            }
        }

        private static void SetupPlayerFollowTarget(Player player)
        {
            if (player == null || player.playerTemplate == null)
                return;

            // Find the FollowTarget in the player's hierarchy
            Transform followTarget = FindDirectChildByName(player.transform, "FollowTarget");
            if (followTarget == null)
            {
                GameObject prefab = player.playerTemplate.followTargetPrefab;
                if (prefab == null)
                {
                    // Try to find a prefab in the project named "FollowTarget" or of type GameObject
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("FollowTarget t:Prefab");
                    if (guids != null && guids.Length > 0)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                        prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        Debug.Log($"[AutoSetup] Fallback: Found FollowTarget prefab at {path}.");
                    }
                }
                if (prefab != null)
                {
                    GameObject newFollowTarget = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
                    newFollowTarget.name = "FollowTarget";
                    followTarget = newFollowTarget.transform;
                    Debug.Log($"Created FollowTarget from prefab as child of Player for {player.gameObject.name}.");
                }
                else
                {
                    GameObject newFollowTarget = new GameObject("FollowTarget");
                    newFollowTarget.transform.SetParent(player.transform);
                    newFollowTarget.transform.localPosition = new Vector3(0f, 1.775f, -0.009f);
                    newFollowTarget.transform.localRotation = Quaternion.identity;
                    newFollowTarget.transform.localScale = Vector3.one;
                    followTarget = newFollowTarget.transform;
                    Debug.Log($"Created empty FollowTarget as child of Player for {player.gameObject.name} (no prefab assigned or found).");
                }
            }
            else
            {
                Debug.Log($"Found existing FollowTarget as child of Player for {player.gameObject.name}.");
            }
        }

        private static void SetupPlayerAimIKTarget(Player player)
        {
            if (player == null || player.playerTemplate == null)
                return;

            // Find the AimIKTarget in the player's hierarchy
            Transform aimIKTarget = FindDirectChildByName(player.transform, "AimIKTarget");
            if (aimIKTarget == null)
            {
                GameObject prefab = player.playerTemplate.aimIKTargetPrefab;
                if (prefab == null)
                {
                    // Try to find a prefab in the project named "AimIKTarget" or of type GameObject
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("AimIKTarget t:Prefab");
                    if (guids != null && guids.Length > 0)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                        prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        Debug.Log($"[AutoSetup] Fallback: Found AimIKTarget prefab at {path}.");
                    }
                }
                if (prefab != null)
                {
                    GameObject newAimIKTarget = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
                    newAimIKTarget.name = "AimIKTarget";
                    aimIKTarget = newAimIKTarget.transform;
                    Debug.Log($"Created AimIKTarget from prefab as child of Player for {player.gameObject.name}.");
                }
                else
                {
                    GameObject newAimIKTarget = new GameObject("AimIKTarget");
                    newAimIKTarget.transform.SetParent(player.transform);
                    newAimIKTarget.transform.localPosition = Vector3.zero;
                    newAimIKTarget.transform.localRotation = Quaternion.identity;
                    newAimIKTarget.transform.localScale = Vector3.one;
                    aimIKTarget = newAimIKTarget.transform;
                    Debug.Log($"Created empty AimIKTarget as child of Player for {player.gameObject.name} (no prefab assigned or found).");
                }
            }
            else
            {
                Debug.Log($"Found existing AimIKTarget as child of Player for {player.gameObject.name}.");
            }
        }

        private static void SetupPlayerLeftHandIKTarget(Player player, bool overwriteExisting = true)
        {
            if (player == null || player.playerTemplate == null)
                return;

            // 1. Search for existing child
            Transform leftHandIKTarget = FindDirectChildByName(player.transform, "LeftHandIKTarget");
            if (leftHandIKTarget != null)
            {
                if (overwriteExisting)
                {
                    // Destroy the existing LeftHandIKTarget before creating a new one
                    Debug.Log($"[AutoSetup] Overwriting existing LeftHandIKTarget for {player.gameObject.name}.");
#if UNITY_EDITOR
                    Object.DestroyImmediate(leftHandIKTarget.gameObject);
#else
                    Object.Destroy(leftHandIKTarget.gameObject);
#endif
                }
                else
                {
                    Debug.Log($"Found existing LeftHandIKTarget as child of Player for {player.gameObject.name}.");
                    return;
                }
            }

            // 2. Try to instantiate from PlayerTemplate prefab
            GameObject prefab = player.playerTemplate.leftHandIKTargetPrefab;
            if (prefab != null)
            {
                GameObject newLeftHandIKTarget = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
                newLeftHandIKTarget.name = "LeftHandIKTarget";
                Debug.Log($"Created LeftHandIKTarget from prefab as child of Player for {player.gameObject.name}.");
                return;
            }

            // 3. Fallback: create empty
            GameObject emptyLeftHandIKTarget = new GameObject("LeftHandIKTarget");
            emptyLeftHandIKTarget.transform.SetParent(player.transform);
            emptyLeftHandIKTarget.transform.localPosition = Vector3.zero;
            emptyLeftHandIKTarget.transform.localRotation = Quaternion.identity;
            emptyLeftHandIKTarget.transform.localScale = Vector3.one;
            Debug.Log($"Created empty LeftHandIKTarget as child of Player for {player.gameObject.name} (no prefab assigned or found).");
            return;
        }

        // Helper to find a child recursively by name
        private static Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                var found = FindChildRecursive(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        // Helper to find a direct child by name
        private static Transform FindDirectChildByName(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
            }
            return null;
        }

        private static (Transform followTarget, Transform aimIKTarget, Transform bulletHitTarget) SetupCameraTargetsAndSettings(Player player, bool overwriteExisting = true)
        {
            if (player == null)
                return (null, null, null);

            // Ensure PlayerCameraController exists
            if (player.PlayerCameraController == null)
            {
                var pcc = player.GetComponent<PlayerCameraController>();
                if (pcc == null)
                {
                    pcc = player.gameObject.AddComponent<PlayerCameraController>();
                    Debug.Log($"[AutoSetup] PlayerCameraController component added to {player.gameObject.name}.");
                }
                else
                {
                    Debug.Log($"[AutoSetup] PlayerCameraController component already exists on {player.gameObject.name}.");
                }
                // Explicitly set the property on Player for robustness
                var prop = typeof(Player).GetProperty("PlayerCameraController");
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(player, pcc);
                    Debug.Log($"[AutoSetup] Explicitly set Player.PlayerCameraController property after ensuring component exists.");
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] Could not set PlayerCameraController property on Player {player.gameObject.name} (property not found or not writable).");
                }
            }
            if (player.PlayerCameraController == null)
            {
                Debug.LogWarning($"[AutoSetup] PlayerCameraController is still null for {player.gameObject?.name} after attempting to add it.");
                return (null, null, null);
            }

            // Only create FollowTarget and AimTarget
            Transform followTarget = FindDirectChildByName(player.transform, "FollowTarget");
            Transform aimIKTarget = FindDirectChildByName(player.transform, "AimIKTarget");
            Transform bulletHitTarget = FindDirectChildByName(player.transform, "BulletHitTarget");

            followTarget = GetOrCreateCameraTarget(player, followTarget, "FollowTarget", player.playerTemplate?.followTargetPrefab, new Vector3(0f, 1.775f, -0.009f));
            aimIKTarget = GetOrCreateCameraTarget(player, aimIKTarget, "AimIKTarget", player.playerTemplate?.aimIKTargetPrefab, Vector3.zero);
            bulletHitTarget = GetOrCreateCameraTarget(player, bulletHitTarget, "BulletHitTarget", player.playerTemplate?.bulletHitTargetPrefab, Vector3.zero);

            // Always assign the new followTarget to the CinemachineCamera.Follow, regardless of overwriteExisting
            var playerCameraGO = GameObject.Find("Cameras/PlayerCamera");
            if (playerCameraGO != null && followTarget != null)
            {
                var cinemachineCamera = playerCameraGO.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                if (cinemachineCamera != null)
                {
                        var before = cinemachineCamera.Follow;
                        // Record undo and mark object dirty so the change persists across domain reload / entering play mode
                        Undo.RecordObject(cinemachineCamera, "Assign Cinemachine Follow Target");
                        cinemachineCamera.Follow = followTarget;
                        EditorUtility.SetDirty(cinemachineCamera);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(cinemachineCamera);

                        Debug.Log($"[AutoSetup] CinemachineCamera.Follow forcibly set to {followTarget.name} (ID: {followTarget.GetInstanceID()}) for player {player.gameObject.name}.");
                }
            }

            SetCameraControllerTargets(player, followTarget, aimIKTarget, bulletHitTarget, overwriteExisting);
            SetPlayerCameraField(player, overwriteExisting);
            SetPlayerMenuUIControllerReferenceForPlayerCamera(player, overwriteExisting);
            SetCameraSettings(player, overwriteExisting);

            if (followTarget == null) Debug.LogWarning("FollowTarget not found as child of Player.");
            if (aimIKTarget == null) Debug.LogWarning("AimIKTarget not found as child of Player.");
            return (followTarget, aimIKTarget, bulletHitTarget);
        }

        private static Transform GetOrCreateCameraTarget(Player player, Transform existing, string name, GameObject prefab, Vector3 defaultPosition)
        {
            if (existing == null)
            {
                GameObject newTarget;
                if (player.playerTemplate != null && prefab != null)
                {
                    newTarget = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
                    newTarget.name = name;
                    Debug.Log($"[AutoSetup] Instantiated {name} from prefab '{prefab.name}' as child of Player {player.gameObject.name}.");
                }
                else
                {
                    newTarget = new GameObject(name);
                    newTarget.transform.SetParent(player.transform);
                    newTarget.transform.localPosition = defaultPosition;
                    newTarget.transform.localRotation = Quaternion.identity;
                    newTarget.transform.localScale = Vector3.one;
                    Debug.Log($"[AutoSetup] Created empty {name} as child of Player {player.gameObject.name}.");
                }
                return newTarget.transform;
            }
            else
            {
                Debug.Log($"[AutoSetup] Found existing {name} as child of Player {player.gameObject.name}.");
                return existing;
            }
        }

        private static void SetCinemachineFollow(Player player, Transform followTarget, bool overwriteExisting)
        {
            var playerCameraGO = GameObject.Find("Cameras/PlayerCamera");
            if (playerCameraGO != null && followTarget != null)
            {
                var cinemachineCamera = playerCameraGO.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                if (cinemachineCamera != null)
                {
                    var before = cinemachineCamera.Follow;
                    if (overwriteExisting)
                    {
                        Debug.Log($"[AutoSetup] CinemachineCamera.Follow before: {(before != null ? before.name : "null")} (ID: {(before != null ? before.GetInstanceID().ToString() : "null")}), template: {followTarget.name} (ID: {followTarget.GetInstanceID()}), overwrite: {overwriteExisting}");
                        if (before != followTarget)
                            Debug.Log($"[AutoSetup] Overwriting CinemachineCamera.Follow: {(before != null ? before.name : "null")} -> {followTarget.name}");
#if UNITY_EDITOR
                        UnityEditor.Undo.RecordObject(cinemachineCamera, "Assign Cinemachine Follow Target");
#endif
                        cinemachineCamera.Follow = followTarget;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(cinemachineCamera);
                        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(cinemachineCamera);
#endif
                        var after = cinemachineCamera.Follow;
                        Debug.Log($"[AutoSetup] CinemachineCamera.Follow after: {after.name} (ID: {after.GetInstanceID()})");
                    }
                    else if (cinemachineCamera.Follow == null)
                    {
#if UNITY_EDITOR
                        UnityEditor.Undo.RecordObject(cinemachineCamera, "Assign Cinemachine Follow Target");
#endif
                        cinemachineCamera.Follow = followTarget;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(cinemachineCamera);
                        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(cinemachineCamera);
#endif
                    }
                }
            }
        }

        private static void SetCameraControllerTargets(Player player, Transform followTarget, Transform aimIKTarget, Transform bulletHitTarget, bool overwriteExisting)
        {
            SetCameraControllerFollowTarget(player, followTarget, overwriteExisting);
            SetCameraControllerAimIKTarget(player, aimIKTarget, overwriteExisting);
            SetCameraBulletHitTarget(player, bulletHitTarget, overwriteExisting);
        }

        private static void SetCameraControllerFollowTarget(Player player, Transform followTarget, bool overwriteExisting)
        {
            var followTargetField = player.PlayerCameraController.GetType().GetField("followTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (followTargetField != null)
            {
                if (followTarget != null)
                {
                    var before = followTargetField.GetValue(player.PlayerCameraController) as Transform;
                    if (overwriteExisting)
                    {
                        Debug.Log($"[AutoSetup] PlayerCameraController.followTarget before: {(before != null ? before.name : "null")} (ID: {(before != null ? before.GetInstanceID().ToString() : "null")}), template: {followTarget.name} (ID: {followTarget.GetInstanceID()}), overwrite: {overwriteExisting}");
                        if (before != followTarget)
                            Debug.Log($"[AutoSetup] Overwriting PlayerCameraController.followTarget: {(before != null ? before.name : "null")} -> {followTarget.name}");
                        followTargetField.SetValue(player.PlayerCameraController, followTarget);
                        // Persist the change so it survives entering Play Mode / domain reload
                        EditorUtility.SetDirty(player.PlayerCameraController);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(player.PlayerCameraController);
                    }
                    else if (before == null)
                    {
                        followTargetField.SetValue(player.PlayerCameraController, followTarget);
                    }
                    if (overwriteExisting)
                    {
                        var after = followTargetField.GetValue(player.PlayerCameraController) as Transform;
                        Debug.Log($"[AutoSetup] PlayerCameraController.followTarget after: {(after != null ? after.name : "null")} (ID: {(after != null ? after.GetInstanceID().ToString() : "null")})");
                    }
                }
                else
                {
                    Debug.LogWarning("[AutoSetup] PlayerCameraController.followTarget: No follow target reference was set.");
                }
            }
        }

        private static void SetCameraControllerAimIKTarget(Player player, Transform aimIKTarget, bool overwriteExisting)
        {
            var aimIKTargetField = player.PlayerCameraController.GetType().GetField("aimIKTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (aimIKTargetField != null)
            {
                if (aimIKTarget != null)
                {
                    var before = aimIKTargetField.GetValue(player.PlayerCameraController) as Transform;
                    if (overwriteExisting)
                    {
                        Debug.Log($"[AutoSetup] PlayerCameraController.aimIKTarget before: {(before != null ? before.name : "null")} (ID: {(before != null ? before.GetInstanceID().ToString() : "null")}), template: {aimIKTarget.name} (ID: {aimIKTarget.GetInstanceID()}), overwrite: {overwriteExisting}");
                        if (before != aimIKTarget)
                            Debug.Log($"[AutoSetup] Overwriting PlayerCameraController.aimIKTarget: {(before != null ? before.name : "null")} -> {aimIKTarget.name}");
                        aimIKTargetField.SetValue(player.PlayerCameraController, aimIKTarget);
                        // Persist the change so it survives entering Play Mode / domain reload
                        EditorUtility.SetDirty(player.PlayerCameraController);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(player.PlayerCameraController);
                    }
                    else if (before == null)
                    {
                        aimIKTargetField.SetValue(player.PlayerCameraController, aimIKTarget);
                    }
                    if (overwriteExisting)
                    {
                        var after = aimIKTargetField.GetValue(player.PlayerCameraController) as Transform;
                        Debug.Log($"[AutoSetup] PlayerCameraController.aimIKTarget after: {(after != null ? after.name : "null")} (ID: {(after != null ? after.GetInstanceID().ToString() : "null")})");
                    }
                }
                else
                {
                    Debug.LogWarning("[AutoSetup] PlayerCameraController.aimIKTarget: No aim IK target reference was set.");
                }
            }
        }

        private static void SetCameraBulletHitTarget(Player player, Transform bulletHitTarget, bool overwriteExisting)
        {
            var bulletHitTargetField = player.PlayerCameraController.GetType().GetField("bulletHitTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (bulletHitTargetField != null)
            {
                if (bulletHitTarget != null)
                {
                    var before = bulletHitTargetField.GetValue(player.PlayerCameraController) as Transform;
                    if (overwriteExisting)
                    {
                        Debug.Log($"[AutoSetup] PlayerCameraController.bulletHitTarget before: {(before != null ? before.name : "null")} (ID: {(before != null ? before.GetInstanceID().ToString() : "null")}), template: {bulletHitTarget.name} (ID: {bulletHitTarget.GetInstanceID()}), overwrite: {overwriteExisting}");
                        if (before != bulletHitTarget)
                            Debug.Log($"[AutoSetup] Overwriting PlayerCameraController.bulletHitTarget: {(before != null ? before.name : "null")} -> {bulletHitTarget.name}");
                        bulletHitTargetField.SetValue(player.PlayerCameraController, bulletHitTarget);
                        // Persist the change so it survives entering Play Mode / domain reload
                        EditorUtility.SetDirty(player.PlayerCameraController);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(player.PlayerCameraController);
                    }
                    else if (before == null)
                    {
                        bulletHitTargetField.SetValue(player.PlayerCameraController, bulletHitTarget);
                    }
                    if (overwriteExisting)
                    {
                        var after = bulletHitTargetField.GetValue(player.PlayerCameraController) as Transform;
                        Debug.Log($"[AutoSetup] PlayerCameraController.bulletHitTarget after: {(after != null ? after.name : "null")} (ID: {(after != null ? after.GetInstanceID().ToString() : "null")})");
                    }
                }
                else
                {
                    Debug.LogWarning("[AutoSetup] PlayerCameraController.bulletHitTarget: No bullet hit target reference was set.");
                }
            }
        }

        private static void SetPlayerCameraField(Player player, bool overwriteExisting)
        {
            var playerCameraGO = GameObject.Find("Cameras/PlayerCamera");
            if (playerCameraGO != null)
            {
                var cinemachineCamera = playerCameraGO.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                if (cinemachineCamera != null)
                {
                    var cameraField = player.PlayerCameraController.GetType().GetField("playerCamera", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (cameraField != null)
                    {
                        var before = cameraField.GetValue(player.PlayerCameraController);
                        if (overwriteExisting)
                        {
                            Debug.Log($"[AutoSetup] PlayerCameraController.playerCamera before: {(before != null ? before.ToString() : "null")}, template: {cinemachineCamera.name}, overwrite: {overwriteExisting}");
                            if (before != (object)cinemachineCamera)
                                Debug.Log($"[AutoSetup] Overwriting PlayerCameraController.playerCamera: {(before != null ? before.ToString() : "null")} -> {cinemachineCamera.name}");
                            cameraField.SetValue(player.PlayerCameraController, cinemachineCamera);
                            // Persist the change to the component so it isn't lost on entering Play Mode
                            EditorUtility.SetDirty(player.PlayerCameraController);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(player.PlayerCameraController);
                            Debug.Log($"[AutoSetup] PlayerCameraController.playerCamera after: {cameraField.GetValue(player.PlayerCameraController)}");
                        }
                        else if (before == null)
                        {
                            cameraField.SetValue(player.PlayerCameraController, cinemachineCamera);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("PlayerCamera field not found in PlayerCameraController.");
                    }
                }
                else
                {
                    Debug.LogWarning("CinemachineCamera component not found on PlayerCamera GameObject.");
                }
            }
            else
            {
                Debug.LogWarning("PlayerCamera GameObject not found at path Cameras/PlayerCamera.");
            }
        }

        private static void SetPlayerMenuUIControllerReferenceForPlayerCamera(Player player, bool overwriteExisting)
        {
            var playerMenuUIController = Object.FindFirstObjectByType<PlayerMenuUIController>();

            if (playerMenuUIController == null)
            {
                Debug.LogWarning("PlayerMenuUIController not found in the scene.");
                return;
            }

            var playerMenuUIControllerField = player.PlayerCameraController.GetType().GetField("playerMenuUIController", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (playerMenuUIControllerField != null)
            {
                playerMenuUIControllerField.SetValue(player.PlayerCameraController, playerMenuUIController);
                Debug.Log($"[AutoSetup] PlayerCameraController.playerMenuUIController set to: {playerMenuUIController}");
                // Persist the change so it survives entering Play Mode / domain reload
                EditorUtility.SetDirty(player.PlayerCameraController);
                PrefabUtility.RecordPrefabInstancePropertyModifications(player.PlayerCameraController);
            }
            else
            {
                Debug.LogWarning("PlayerMenuUIController field not found in PlayerCameraController.");
            }
        }

        private static void SetCameraSettings(Player player, bool overwriteExisting)
        {
            if (player.playerTemplate != null)
            {
                SetCameraSettingFloat(player, "followFOV", player.playerTemplate.followFOV, overwriteExisting);
                SetCameraSettingFloat(player, "aimFOV", player.playerTemplate.aimFOV, overwriteExisting);
                SetCameraSettingFloat(player, "zoomSpeed", player.playerTemplate.zoomSpeed, overwriteExisting);
                SetCameraSettingFloat(player, "aimCamOffsetX", player.playerTemplate.aimCamOffsetX, overwriteExisting);
                SetCameraSettingFloat(player, "offsetLerpSpeed", player.playerTemplate.offsetLerpSpeed, overwriteExisting);
                if (overwriteExisting)
                {
                    Debug.Log($"[{player.gameObject.name}] AutoSetupReferences: Set camera FOV and zoom speed from PlayerTemplate. Overwrite: {overwriteExisting}");
                }
            }
        }

        private static void SetCameraSettingFloat(Player player, string fieldName, float templateValue, bool overwriteExisting)
        {
            var field = player.PlayerCameraController.GetType().GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var before = (float)field.GetValue(player.PlayerCameraController);
                if (overwriteExisting)
                {
                    Debug.Log($"[AutoSetup] PlayerCameraController.{fieldName} before: {before}, template: {templateValue}, overwrite: {overwriteExisting}");
                    if (before != templateValue)
                        Debug.Log($"[AutoSetup] Overwriting PlayerCameraController.{fieldName}: {before} -> {templateValue}");
                    field.SetValue(player.PlayerCameraController, templateValue);
                    var after = (float)field.GetValue(player.PlayerCameraController);
                    Debug.Log($"[AutoSetup] PlayerCameraController.{fieldName} after: {after}");
                }
                else if (before == default(float))
                {
                    field.SetValue(player.PlayerCameraController, templateValue);
                }
            }
            else
            {
                Debug.LogWarning($"PlayerCameraController.{fieldName} field not found.");
            }
        }

        private static void SetupCrosshairController(Player player)
        {
            if (player == null) return;

            // Auto-assign CrosshairController if not already set
            if (player.CrosshairController == null)
            {
                var crosshair = Object.FindFirstObjectByType<CrosshairController>();
                if (crosshair != null)
                {
                    var crosshairField = typeof(Player).GetField("crosshairController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (crosshairField != null)
                    {
                        crosshairField.SetValue(player, crosshair);
                        // Persist the change so it survives entering Play Mode / domain reload
                        EditorUtility.SetDirty(player);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(player);
                    }
                    Debug.Log($"[{player.gameObject.name}] AutoSetupReferences: Assigned CrosshairController automatically.");
                }
                else
                {
                    Debug.LogWarning($"[{player.gameObject.name}] AutoSetupReferences: Could not find CrosshairController in the scene.");
                }
            }
        }

        private static void SetupOverlayUIController(Player player)
        {
            if (player == null) return;

            // Auto-assign OverlayController if not already set
            if (player.OverlayUIController == null)
            {
                var overlayUIController = Object.FindFirstObjectByType<OverlayUIController>();
                if (overlayUIController != null)
                {
                    var overlayField = typeof(Player).GetField("overlayUIController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (overlayField != null)
                    {
                        overlayField.SetValue(player, overlayUIController);
                        EditorUtility.SetDirty(player);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(player);
                    }
                    Debug.Log($"[{player.gameObject.name}] AutoSetupReferences: Assigned OverlayUIController automatically.");
                }
                else
                {
                    Debug.LogWarning($"[{player.gameObject.name}] AutoSetupReferences: Could not find OverlayUIController in the scene.");
                }
            }
        }

        private static void SetupCurrentWeaponUIController(Player player)
        {
            if (player == null) return;

            // Auto-assign CurrentWeaponUIController if not already set
            if (player.CurrentWeaponUIController == null)
            {
                var currentWeaponUIController = Object.FindFirstObjectByType<CurrentWeaponUIController>();
                if (currentWeaponUIController != null)
                {
                    var currentWeaponField = typeof(Player).GetField("currentWeaponUIController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (currentWeaponField != null)
                    {
                        currentWeaponField.SetValue(player, currentWeaponUIController);
                        EditorUtility.SetDirty(player);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(player);
                    }
                    Debug.Log($"[{player.gameObject.name}] AutoSetupReferences: Assigned CurrentWeaponUIController automatically.");
                }
                else
                {
                    Debug.LogWarning($"[{player.gameObject.name}] AutoSetupReferences: Could not find CurrentWeaponUIController in the scene.");
                }
            }
        }

        private static void SetupHealthUIController(Player player)
        {
            if (player == null) return;

            if (player.HealthUIController == null)
            {
                var healthUIController = Object.FindFirstObjectByType<HealthUIController>();
                if (healthUIController != null)
                {
                    var healthField = typeof(Player).GetField("healthUIController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (healthField != null)
                    {
                        healthField.SetValue(player, healthUIController);
                        EditorUtility.SetDirty(player);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(player);
                    }
                    Debug.Log($"[{player.gameObject.name}] AutoSetupReferences: Assigned HealthUIController automatically.");
                }
                else
                {
                    Debug.LogWarning($"[{player.gameObject.name}] AutoSetupReferences: Could not find HealthUIController in the scene.");
                }
            }
        }

        private static void SetupPlayerMenuUIController(Player player)
        {
            if (player == null) return;

            // Auto-assign PlayerMenuUIController if not already set
            if (player.PlayerMenuUIController == null)
            {
                var playerMenuUIController = Object.FindFirstObjectByType<PlayerMenuUIController>();
                if (playerMenuUIController != null)
                {
                    var playerMenuField = typeof(Player).GetField("playerMenuUIController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (playerMenuField != null)
                    {
                        playerMenuField.SetValue(player, playerMenuUIController);
                        EditorUtility.SetDirty(player);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(player);
                    }
                    Debug.Log($"[{player.gameObject.name}] AutoSetupReferences: Assigned PlayerMenuUIController automatically.");
                }
                else
                {
                    Debug.LogWarning($"[{player.gameObject.name}] AutoSetupReferences: Could not find PlayerMenuUIController in the scene.");
                }
            }
        }

        private static void SetLayerRecursively(GameObject obj, int layer)
        {
            if (obj == null) return;
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                if (child != null && child.gameObject != null)
                {
                    child.gameObject.layer = layer;
                    SetLayerRecursively(child.gameObject, layer);
                }
            }
        }

        private static void SetupAimIK(Player player, bool overwriteExisting = true, Transform aimIKTarget = null)
        {
            if (player == null)
                return;

            var aimIK = player.GetComponent<AimIK>();
            if (aimIK == null)
            {
                aimIK = player.gameObject.AddComponent<AimIK>();
                Debug.Log($"[AutoSetup] AimIK component added to {player.gameObject.name}.");
            }
            // Always disable AimIK after setup
            aimIK.enabled = false;

            // Assign targets
            aimIK.solver.target = aimIKTarget != null ? aimIKTarget : FindChildRecursive(player.transform, "AimIKTarget");
            aimIK.solver.axis = new Vector3(0, 0, 1);
            aimIK.solver.poleAxis = new Vector3(0, 1, 0);
            aimIK.solver.IKPositionWeight = 0f; // Ensure IK position weight is set
            aimIK.solver.poleWeight = 0f;
            aimIK.solver.tolerance = 0f;
            aimIK.solver.maxIterations = 4;
            aimIK.solver.clampWeight = 0.1f;
            aimIK.solver.clampSmoothing = (int)2f;
            aimIK.solver.useRotationLimits = true;
            // aimIK.solver.fixTransforms = true; // Not present on solver
            // If your version supports clamp, set it here:
            // aimIK.solver.clamp = IKSolverAim.Clamp.Absolute;

            // Set up the bones array with correct references and weights
            aimIK.solver.bones = new IKSolver.Bone[] {
                new(FindChildRecursive(player.transform, "spine_01.x"), .2f),
                new(FindChildRecursive(player.transform, "spine_02.x"), .4f),
                new(FindChildRecursive(player.transform, "spine_03.x"), .6f),
            };

            // Mark AimIK as dirty so changes persist
            EditorUtility.SetDirty(aimIK);
            PrefabUtility.RecordPrefabInstancePropertyModifications(aimIK);

            Debug.Log($"[AutoSetup] AimIK component set up for {player.gameObject.name} (disabled by default). Target: {aimIK.solver.target?.name}");
        }

        private static void SetupLeftHandElbowBendGoal(Player player, bool overwriteExisting)
        {
            if (player == null || player.playerTemplate == null)
                return;

            Transform leftElbowBendGoal = FindChildRecursive(player.transform, "LeftElbowBendGoal");

            if (leftElbowBendGoal == null)
            {
                GameObject prefab = player.playerTemplate.leftElbowBendGoalPrefab;
                if (prefab == null)
                {
                    // Try to find a prefab named "LeftElbowBendGoal" in the project
                    string[] guids = AssetDatabase.FindAssets("LeftElbowBendGoal t:Prefab");
                    if (guids != null && guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    }
                }
                if (prefab != null)
                {
                    GameObject leftElbowBendGoalInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
                    leftElbowBendGoalInstance.name = "LeftElbowBendGoal";
                    Debug.Log($"[AutoSetup] Instantiated LeftElbowBendGoal from prefab '{prefab.name}' as child of Player {player.gameObject.name}.");
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] Could not find LeftElbowBendGoal prefab to instantiate for Player {player.gameObject.name}.");
                }
            }
        }

        private static void SetAimIKBoneWeight(AimIK aimIK, string boneName, float weight)
        {
            if (aimIK?.solver?.bones == null) return;
            foreach (var bone in aimIK.solver.bones)
            {
                if (bone != null && bone.transform != null && bone.transform.name == boneName)
                {
                    bone.weight = weight;
                    return;
                }
            }
            Debug.LogWarning($"[AutoSetup] Could not find bone '{boneName}' in AimIK.bones array.");
        }

        private static void SetupRecoilIK(Player player, bool overwriteExisting = true)
        {
            if (player == null)
                return;

            var recoilIK = player.GetComponent<RecoilIK>();
            if (recoilIK == null)
            {
                recoilIK = player.gameObject.AddComponent<RecoilIK>();
                Debug.Log($"[AutoSetup] IKRecoil component added to {player.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] IKRecoil component already exists on {player.gameObject.name}.");
            }

            AssignAimIKToRecoilIK(player, recoilIK);
            AssignFBBIKToRecoilIK(player, recoilIK);
            AssignRecoilIKSettingsFromWeaponManager(player, recoilIK);

            // Mark RecoilIK as dirty so changes persist
            EditorUtility.SetDirty(recoilIK);
            PrefabUtility.RecordPrefabInstancePropertyModifications(recoilIK);
        }

        private static void AssignAimIKToRecoilIK(Player player, RecoilIK recoilIK)
        {
            var aimIK = player.GetComponent<AimIK>();
            if (aimIK != null)
            {
                var aimIKField = recoilIK.GetType().GetField("aimIK", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (aimIKField != null)
                {
                    aimIKField.SetValue(recoilIK, aimIK);
                    Debug.Log($"[AutoSetup] Assigned AimIK reference to RecoilIK on {player.gameObject.name}.");
                }
                else
                {
                    var aimIKProp = recoilIK.GetType().GetProperty("aimIK", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (aimIKProp != null && aimIKProp.CanWrite)
                    {
                        aimIKProp.SetValue(recoilIK, aimIK);
                        Debug.Log($"[AutoSetup] Assigned AimIK property to RecoilIK on {player.gameObject.name}.");
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoSetup] Could not find 'aimIK' field or property on RecoilIK for {player.gameObject.name}.");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[AutoSetup] AimIK component not found on {player.gameObject.name}, cannot assign to RecoilIK.");
            }
        }

        private static void AssignFBBIKToRecoilIK(Player player, RecoilIK recoilIK)
        {
            var fbbik = player.GetComponent<FullBodyBipedIK>();
            if (fbbik != null)
            {
                // Walk up the inheritance chain to find the 'ik' field, logging each step
                System.Type type = recoilIK.GetType();
                System.Reflection.FieldInfo ikField = null;
                while (type != null && ikField == null)
                {
                    Debug.Log($"[AutoSetup] Checking for 'ik' field in type: {type.FullName}");
                    ikField = type.GetField("ik", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (ikField != null)
                    {
                        Debug.Log($"[AutoSetup] Found 'ik' field in type: {type.FullName}");
                    }
                    type = type.BaseType;
                }
                if (ikField != null)
                {
                    ikField.SetValue(recoilIK, fbbik);
                    Debug.Log($"[AutoSetup] Assigned FBBIK reference to 'ik' field (found in {ikField.DeclaringType.Name}) of RecoilIK on {player.gameObject.name}.");
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] Could not find 'ik' field in RecoilIK or its base classes for {player.gameObject.name}.");
                }
            }
            else
            {
                Debug.LogWarning($"[AutoSetup] FullBodyBipedIK component not found on {player.gameObject.name}, cannot assign to RecoilIK.");
            }
        }

        private static void AssignRecoilIKSettingsFromWeaponManager(Player player, RecoilIK recoilIK)
        {
            var pwm = player.GetComponent<PlayerWeaponManager>();
            if (pwm != null)
            {
                var weaponData = pwm.CurrentWeaponConfig;
                if (weaponData == null)
                {
                    // Try to auto-assign a WeaponData asset if one exists
                    string[] guids = AssetDatabase.FindAssets("t:WeaponData");
                    if (guids != null && guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        var defaultWeaponData = AssetDatabase.LoadAssetAtPath<WeaponConfig>(path);
                        if (defaultWeaponData != null)
                        {
                            var currentWeaponDataField = pwm.GetType().GetField("currentWeaponData", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (currentWeaponDataField != null)
                            {
                                currentWeaponDataField.SetValue(pwm, defaultWeaponData);
                                weaponData = defaultWeaponData;
                            }
                        }
                    }
                }
                if (weaponData != null)
                {
                    // Always assign a deep copy of offsets from WeaponData to RecoilIK
                    if (weaponData.offsets != null && weaponData.offsets.Length > 0)
                    {
                        recoilIK.offsets = new RecoilIK.RecoilOffset[weaponData.offsets.Length];
                        for (int i = 0; i < weaponData.offsets.Length; i++)
                        {
                            var src = weaponData.offsets[i];
                            var dst = new RecoilIK.RecoilOffset();
                            dst.offset = src.offset;
                            dst.additivity = src.additivity;
                            dst.maxAdditiveOffsetMag = src.maxAdditiveOffsetMag;
                            if (src.effectorLinks != null && src.effectorLinks.Length > 0)
                            {
                                dst.effectorLinks = new RecoilIK.RecoilOffset.EffectorLink[src.effectorLinks.Length];
                                for (int j = 0; j < src.effectorLinks.Length; j++)
                                {
                                    var srcLink = src.effectorLinks[j];
                                    var dstLink = new RecoilIK.RecoilOffset.EffectorLink();
                                    dstLink.effector = srcLink.effector;
                                    dstLink.weight = srcLink.weight;
                                    dst.effectorLinks[j] = dstLink;
                                }
                            }
                            else
                            {
                                dst.effectorLinks = null;
                            }
                            recoilIK.offsets[i] = dst;
                        }
                    }
                    else
                    {
                        recoilIK.offsets = null;
                        Debug.LogWarning($"[AutoSetup] No offsets found in WeaponData for {player.gameObject.name}.");
                    }
                }
            }
        }

        // Adds or assigns FullBodyBipedIK to the player if missing
        private static void SetupFBBIK(Player player, bool overwriteExisting = true)
        {
            if (player == null) return;

            var fbbik = player.GetComponent<FullBodyBipedIK>();
            if (fbbik == null)
            {
                fbbik = player.gameObject.AddComponent<FullBodyBipedIK>();
                Debug.Log($"[AutoSetup] FullBodyBipedIK component added to {player.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] FullBodyBipedIK component already exists on {player.gameObject.name}.");
            }

            fbbik.enabled = false;

            var refs = new BipedReferences
            {
                root = player.transform,
                pelvis = FindChildRecursive(player.transform, "root.x"),
                spine = new Transform[] {
                FindChildRecursive(player.transform, "spine_01.x"),
                FindChildRecursive(player.transform, "spine_02.x"),
                FindChildRecursive(player.transform, "spine_03.x"),
            },
                head = FindChildRecursive(player.transform, "head.x"),

                leftThigh = FindChildRecursive(player.transform, "thigh_stretch.l"),
                leftCalf = FindChildRecursive(player.transform, "leg_stretch.l"),
                leftFoot = FindChildRecursive(player.transform, "foot.l"),

                rightThigh = FindChildRecursive(player.transform, "thigh_stretch.r"),
                rightCalf = FindChildRecursive(player.transform, "leg_stretch.r"),
                rightFoot = FindChildRecursive(player.transform, "foot.r"),

                leftUpperArm = FindChildRecursive(player.transform, "arm_stretch.l"),
                leftForearm = FindChildRecursive(player.transform, "forearm_stretch.l"),
                leftHand = FindChildRecursive(player.transform, "hand.l"),

                rightUpperArm = FindChildRecursive(player.transform, "arm_stretch.r"),
                rightForearm = FindChildRecursive(player.transform, "forearm_stretch.r"),
                rightHand = FindChildRecursive(player.transform, "hand.r")
            };

            if (refs.isFilled)
            {
                fbbik.SetReferences(refs, rootNode: FindChildRecursive(player.transform, "spine_01.x"));
                Debug.Log($"[AutoSetup] FBBIK references successfully assigned for {player.gameObject.name}.");
            }
            else
            {
                Debug.LogWarning($"[AutoSetup] Failed to assign some FBBIK references for {player.gameObject.name}. Check bone names or rig.");
            }

            // Set LeftHandElbowBendGoal reference
            fbbik.solver.leftArmChain.bendConstraint.bendGoal = FindChildRecursive(player.transform, "LeftElbowBendGoal");
            fbbik.solver.leftArmChain.bendConstraint.weight = .3f;

            EditorUtility.SetDirty(fbbik);
            PrefabUtility.RecordPrefabInstancePropertyModifications(fbbik);
        }

        // Adds or assigns BulletDecalManager to the player if missing
        private static void SetupBulletDecalManager(Player player)
        {
            if (player == null)
                return;

            var bulletDecalManager = player.GetComponent<BulletDecalManager>();
            if (bulletDecalManager == null)
            {
                bulletDecalManager = player.gameObject.AddComponent<BulletDecalManager>();
                Debug.Log($"[AutoSetup] BulletDecalManager component added to {player.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] BulletDecalManager component already exists on {player.gameObject.name}.");
            }
            EditorUtility.SetDirty(bulletDecalManager);
            PrefabUtility.RecordPrefabInstancePropertyModifications(bulletDecalManager);
        }

        private static void SetupPlayerCharacterController(Player player)
        {
            if (player == null)
                return;
            var pcc = player.GetComponent<PlayerCharacterController>();
            if (pcc == null)
            {
                pcc = player.gameObject.AddComponent<PlayerCharacterController>();
                Debug.Log($"[AutoSetup] PlayerCharacterController component added to {player.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] PlayerCharacterController component already exists on {player.gameObject.name}.");
            }
            // Optionally, set as property if needed
            var type = typeof(Player);
            var prop = type.GetProperty("PlayerCharacterController");
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(player, pcc);
            }
        }

        private static void SetupHealthManager(Player player)
        {
            if (player == null)
                return;

            // Ensure HealthManager component exists
            if (player.HealthManager == null)
            {
                var healthManager = player.GetComponent<HealthManager>();
                if (healthManager == null)
                {
                    healthManager = player.gameObject.AddComponent<HealthManager>();
                    Debug.Log($"[AutoSetup] HealthManager component added to {player.gameObject.name}.");
                }
                else
                {
                    Debug.Log($"[AutoSetup] HealthManager component already exists on {player.gameObject.name}.");
                }
                // Explicitly set the property on Player for robustness
                var prop = typeof(Player).GetProperty("HealthManager");
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(player, healthManager);
                    Debug.Log($"[AutoSetup] Explicitly set Player.HealthManager property after ensuring component exists.");
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] Could not set HealthManager property on Player {player.gameObject.name} (property not found or not writable).");
                }
            }
        }

        private static void SetupBulletHitscan(Player player)
        {
            if (player == null)
                return;

            // Ensure BulletHitscan component exists
            var bulletHitscan = player.GetComponent<BulletHitscan>();
            if (bulletHitscan == null)
            {
                bulletHitscan = player.gameObject.AddComponent<BulletHitscan>();
                Debug.Log($"[AutoSetup] BulletHitscan component added to {player.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] BulletHitscan component already exists on {player.gameObject.name}.");
            }

            // Assign to Player property/field for robustness
            var prop = typeof(Player).GetProperty("BulletHitscan");
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(player, bulletHitscan);
                Debug.Log($"[AutoSetup] Explicitly set Player.BulletHitscan property after ensuring component exists.");
            }
            else
            {
                var field = typeof(Player).GetField("bulletHitscan", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(player, bulletHitscan);
                    Debug.Log($"[AutoSetup] Assigned Player.bulletHitscan field for {player.gameObject.name}.");
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] Could not set BulletHitscan property or field on Player {player.gameObject.name} (not found or not writable).");
                }
            }

            // Mark as dirty for persistence
            EditorUtility.SetDirty(bulletHitscan);
            PrefabUtility.RecordPrefabInstancePropertyModifications(bulletHitscan);
        }

        private static void SetupPlayerAnimatorEvents(Player player)
        {
            if (player == null) return;
            var animatorEvents = player.GetComponent<PlayerAnimatorEvents>();
            if (animatorEvents == null)
            {
                animatorEvents = player.gameObject.AddComponent<PlayerAnimatorEvents>();
                Debug.Log($"[AutoSetup] PlayerAnimatorEvents component added to {player.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] PlayerAnimatorEvents component already exists on {player.gameObject.name}.");
            }
        }

        private static void SetupPlayerComponentReferences(Player player)
        {
            var type = typeof(Player);
            var playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
                type.GetProperty("PlayerInput")?.SetValue(player, playerInput);
            var playerCharacterController = player.GetComponent<PlayerCharacterController>();
            if (playerCharacterController != null)
                type.GetProperty("PlayerCharacterController")?.SetValue(player, playerCharacterController);
            var playerCameraController = player.GetComponent<PlayerCameraController>();
            if (playerCameraController != null)
                type.GetProperty("PlayerCameraController")?.SetValue(player, playerCameraController);
            var playerIKController = player.GetComponent<PlayerIKController>();
            if (playerIKController != null)
            {
                type.GetProperty("PlayerIKController")?.SetValue(player, playerIKController);
                EditorUtility.SetDirty(playerIKController);
                PrefabUtility.RecordPrefabInstancePropertyModifications(playerIKController);
                // Assign LeftHandIKTarget if it exists
                var leftHandIKTarget = FindDirectChildByName(player.transform, "LeftHandIKTarget");
                if (leftHandIKTarget != null)
                {
                    var leftHandIKTargetField = playerIKController.GetType().GetField("leftHandIKTarget", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (leftHandIKTargetField != null)
                    {
                        leftHandIKTargetField.SetValue(playerIKController, leftHandIKTarget);
                        Debug.Log($"[AutoSetup] Assigned LeftHandIKTarget to PlayerIKController for {player.gameObject.name}.");
                    }
                    else
                    {
                        var leftHandIKTargetProp = playerIKController.GetType().GetProperty("LeftHandIKTarget", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (leftHandIKTargetProp != null && leftHandIKTargetProp.CanWrite)
                        {
                            leftHandIKTargetProp.SetValue(playerIKController, leftHandIKTarget);
                            Debug.Log($"[AutoSetup] Assigned LeftHandIKTarget property to PlayerIKController for {player.gameObject.name}.");
                        }
                        else
                        {
                            Debug.LogWarning($"[AutoSetup] Could not find field or writable property 'LeftHandIKTarget' on PlayerIKController for {player.gameObject.name}.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] Could not find LeftHandIKTarget in Player hierarchy for {player.gameObject.name}.");
                }
                // Assign RecoilIK reference from player to PlayerIKController if possible
                var recoilIK = player.GetComponent<RecoilIK>();
                if (recoilIK != null)
                {
                    var recoilField = playerIKController.GetType().GetField("recoil", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (recoilField != null)
                    {
                        recoilField.SetValue(playerIKController, recoilIK);
                        Debug.Log($"[AutoSetup] Assigned RecoilIK reference to PlayerIKController (field 'recoil') for {player.gameObject.name}.");
                    }
                    else
                    {
                        var recoilProp = playerIKController.GetType().GetProperty("recoil", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (recoilProp != null && recoilProp.CanWrite)
                        {
                            recoilProp.SetValue(playerIKController, recoilIK);
                            Debug.Log($"[AutoSetup] Assigned RecoilIK property to PlayerIKController (property 'recoil') for {player.gameObject.name}.");
                        }
                        else
                        {
                            Debug.LogWarning($"[AutoSetup] Could not find 'recoil' field or property on PlayerIKController for {player.gameObject.name}.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] RecoilIK component not found on {player.gameObject.name}, cannot assign to PlayerIKController.");
                }
            }
            var weaponManager = player.GetComponent<PlayerWeaponManager>();
            if (weaponManager != null)
            {
                type.GetProperty("WeaponManager")?.SetValue(player, weaponManager);
                EditorUtility.SetDirty(weaponManager);
                PrefabUtility.RecordPrefabInstancePropertyModifications(weaponManager);
            }
            var healthManager = player.GetComponent<HealthManager>();
            if (healthManager != null)
                type.GetProperty("HealthManager")?.SetValue(player, healthManager);
            var recoil = player.GetComponent<RecoilIK>();
            if (recoil != null)
                type.GetProperty("Recoil")?.SetValue(player, recoil);
            var bulletHitscan = player.GetComponent<BulletHitscan>();
            if (bulletHitscan != null)
                type.GetProperty("BulletHitscan")?.SetValue(player, bulletHitscan);
            var bulletDecalManager = player.GetComponent<BulletDecalManager>();
            if (bulletDecalManager != null)
                type.GetProperty("BulletDecalManager")?.SetValue(player, bulletDecalManager);

            // Assign the player reference for PlayerDebugger
            var playerDebugger = player.GetComponent<PlayerDebugger>();
            if (playerDebugger != null)
            {
                var playerField = playerDebugger.GetType().GetField("player", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (playerField != null)
                {
                    playerField.SetValue(playerDebugger, player);
                    Debug.Log($"[AutoSetup] Assigned player reference to PlayerDebugger for {player.gameObject.name}.");
                }
                else
                {
                    var playerProp = playerDebugger.GetType().GetProperty("player", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (playerProp != null && playerProp.CanWrite)
                    {
                        playerProp.SetValue(playerDebugger, player);
                        Debug.Log($"[AutoSetup] Assigned player property to PlayerDebugger for {player.gameObject.name}.");
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoSetup] Could not find field or writable property 'player' on PlayerDebugger for {player.gameObject.name}.");
                    }
                }
            }
        }

        // Sets all AiDestinationSetter.target fields on Enemy GameObjects to the player
        private static void AssignPlayerToEnemies(Player player)
        {
            if (player == null || player.gameObject == null)
                return;

            // Find the Enemies root GameObject
            var enemiesRoot = GameObject.Find("Enemies");
            if (enemiesRoot == null)
            {
                Debug.LogWarning("[AutoSetup] Could not find GameObject named 'Enemies'.");
                return;
            }

            int setCount = 0;
            // Find all Enemy components in children (recursively)
            var enemyComponents = enemiesRoot.GetComponentsInChildren<UndeadSurvivalGame.EnemySystems.Enemy>(true);
            foreach (var enemyComponent in enemyComponents)
            {
                if (enemyComponent == null) continue;
                enemyComponent.PlayerTransform = player.transform;
                EditorUtility.SetDirty(enemyComponent);
                PrefabUtility.RecordPrefabInstancePropertyModifications(enemyComponent);

                var aiDestinationSetter = enemyComponent.GetComponent<AIDestinationSetter>();
                if (aiDestinationSetter != null)
                {
                    aiDestinationSetter.target = player.transform;
                    EditorUtility.SetDirty(aiDestinationSetter);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(aiDestinationSetter);
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] AIDestinationSetter component not found on {enemyComponent.gameObject.name}. Cannot set target.");
                }

                var lookAtIK = enemyComponent.GetComponent<LookAtIK>();
                if (lookAtIK != null)
                {
                    // Assign the player's head bone (head.x) as the target for LookAtIK
                    var headTransform = FindChildRecursive(player.transform, "head.x");
                    if (headTransform != null)
                    {
                        lookAtIK.solver.target = headTransform;
                        EditorUtility.SetDirty(lookAtIK);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(lookAtIK);
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoSetup] Could not find head.x bone on player {player.gameObject.name}. LookAtIK target not set.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] LookAtIK component not found on {enemyComponent.gameObject.name}. Cannot set target.");
                }
                setCount++;
            }
            Debug.Log($"[AutoSetup] SetupEnemyPlayerReference: Set Player Transform for {setCount} Enemy components.");
        }

        private static void SetupBipedRagdollCreator(Player player, bool overwriteExisting = true)
        {
            if (player == null) return;

            // Check for RagdollEditor component to determine ragdoll status
            var ragdollEditor = player.GetComponent<RagdollEditor>();
            var bipedRagdollCreator = player.GetComponent<BipedRagdollCreator>();
            if (!overwriteExisting && ragdollEditor != null)
            {
                Debug.Log($"[AutoSetup] Skipping BipedRagdollCreator: RagdollEditor component found and overwriteExisting is false.");
                return;
            }

            if (bipedRagdollCreator == null)
            {
                bipedRagdollCreator = player.gameObject.AddComponent<BipedRagdollCreator>();
                Debug.Log($"[AutoSetup] BipedRagdollCreator component added to {player.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] BipedRagdollCreator component already exists on {player.gameObject.name}.");
            }

            // Mark as dirty for persistence
            EditorUtility.SetDirty(bipedRagdollCreator);
            PrefabUtility.RecordPrefabInstancePropertyModifications(bipedRagdollCreator);
        }

        private static void SetupPlayerInventory(Player player)
        {
            if (player == null)
                return;

            var inventory = player.GetComponent<Inventory>();
            if (inventory == null)
            {
                inventory = player.gameObject.AddComponent<Inventory>();
                Debug.Log($"[AutoSetup] Inventory component added to {player.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] Inventory component already exists on {player.gameObject.name}.");
            }

            // Mark as dirty for persistence
            EditorUtility.SetDirty(inventory);
            PrefabUtility.RecordPrefabInstancePropertyModifications(inventory);
        }

        private static void SetupInventoryBootstrap(Player player)
        {
            if (player == null)
                return;

            var inventoryBootstrap = player.GetComponent<InventoryBootstrap>();
            if (inventoryBootstrap == null)
            {
                inventoryBootstrap = player.gameObject.AddComponent<InventoryBootstrap>();
                Debug.Log($"[AutoSetup] InventoryBootstrap component added to {player.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] InventoryBootstrap component already exists on {player.gameObject.name}.");
            }

            // Assign Inventory reference to InventoryBootstrap
            var inventory = player.GetComponent<Inventory>();
            if (inventory != null)
            {
                var inventoryField = inventoryBootstrap.GetType().GetField("inventory", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (inventoryField != null)
                {
                    inventoryField.SetValue(inventoryBootstrap, inventory);
                    Debug.Log($"[AutoSetup] Assigned Inventory reference to InventoryBootstrap for {player.gameObject.name}.");
                }
                else
                {
                    var inventoryProp = inventoryBootstrap.GetType().GetProperty("inventory", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (inventoryProp != null && inventoryProp.CanWrite)
                    {
                        inventoryProp.SetValue(inventoryBootstrap, inventory);
                        Debug.Log($"[AutoSetup] Assigned Inventory property to InventoryBootstrap for {player.gameObject.name}.");
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoSetup] Could not find field or writable property 'inventory' on InventoryBootstrap for {player.gameObject.name}.");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[AutoSetup] Inventory component not found on {player.gameObject.name}, cannot assign to InventoryBootstrap.");
            }

            // Assign InventoryPreset
            var inventoryPreset = FindFirstInventoryPreset();
            if (inventoryPreset != null)
            {
                var presetField = inventoryBootstrap.GetType().GetField("preset", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (presetField != null)
                {
                    presetField.SetValue(inventoryBootstrap, inventoryPreset);
                    Debug.Log($"[AutoSetup] Assigned InventoryPreset reference to InventoryBootstrap for {player.gameObject.name}.");
                }
                else
                {
                    var presetProp = inventoryBootstrap.GetType().GetProperty("preset", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (presetProp != null && presetProp.CanWrite)
                    {
                        presetProp.SetValue(inventoryBootstrap, inventoryPreset);
                        Debug.Log($"[AutoSetup] Assigned InventoryPreset property to InventoryBootstrap for {player.gameObject.name}.");
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoSetup] Could not find field or writable property 'preset' on InventoryBootstrap for {player.gameObject.name}.");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[AutoSetup] InventoryPreset asset not found, cannot assign to InventoryBootstrap.");
            }

            // Mark as dirty for persistence
            EditorUtility.SetDirty(inventoryBootstrap);
            PrefabUtility.RecordPrefabInstancePropertyModifications(inventoryBootstrap);
        }

        private static InventoryPreset FindFirstInventoryPreset()
        {
            // Finds assets of type InventoryPreset and returns the first one (or null)
            string[] guids = AssetDatabase.FindAssets("t:InventoryPreset");
            if (guids == null || guids.Length == 0) return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (string.IsNullOrEmpty(path)) return null;

            var preset = AssetDatabase.LoadAssetAtPath<InventoryPreset>(path);
            return preset;
        }

        private static void SetupAimPoseLayerWeightController(Player player, bool overwriteExisting)
        {
            if (player == null) return;

            var aimPoseLayerWeightController = player.GetComponent<AimPoseLayerWeightController>();
            if (aimPoseLayerWeightController == null)
            {
                aimPoseLayerWeightController = player.gameObject.AddComponent<AimPoseLayerWeightController>();
                Debug.Log($"[AutoSetup] AimPoseLayerWeightController component added to {player.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] AimPoseLayerWeightController component already exists on {player.gameObject.name}.");
            }

            // Persist the change so it survives entering Play Mode / domain reload
            EditorUtility.SetDirty(aimPoseLayerWeightController);
            PrefabUtility.RecordPrefabInstancePropertyModifications(aimPoseLayerWeightController);
        }

        private static void SetupAimPitchLayerWeightController(Player player, bool overwriteExisting)
        {
            if (player == null) 
                return;

            var aimPitchLayerWeightController = player.GetComponent<AimPitchLayerWeightController>();
            if (aimPitchLayerWeightController == null)
            {
                aimPitchLayerWeightController = player.gameObject.AddComponent<AimPitchLayerWeightController>();
                Debug.Log($"[AutoSetup] AimPitchLayerWeightController component added to {player.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] AimPitchLayerWeightController component already exists on {player.gameObject.name}.");
            }

            EditorUtility.SetDirty(aimPitchLayerWeightController);
            PrefabUtility.RecordPrefabInstancePropertyModifications(aimPitchLayerWeightController);
        }

        private static void SetupUpperBodyLayerWeightController(Player player, bool overwriteExisting)
        {
            if (player == null)
                return;

            var upperBodyLayerWeightController = player.GetComponent<UpperBodyLayerWeightController>();
            if (upperBodyLayerWeightController == null)
            {
                upperBodyLayerWeightController = player.gameObject.AddComponent<UpperBodyLayerWeightController>();
                Debug.Log($"[AutoSetup] UpperBodyLayerWeightController component added to {player.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] UpperBodyLayerWeightController component already exists on {player.gameObject.name}.");
            }

            EditorUtility.SetDirty(upperBodyLayerWeightController);
            PrefabUtility.RecordPrefabInstancePropertyModifications(upperBodyLayerWeightController);
        }

        private static void SetupAlphaCutoffController(Player player, bool overwriteExisting)
        {
            if (player == null)
                return;

            var alphaCutoffController = player.GetComponent<AlphaCutoffController>();
            if (alphaCutoffController == null)
            {
                alphaCutoffController = player.gameObject.AddComponent<AlphaCutoffController>();
                Debug.Log($"[AutoSetup] AlphaCutoffController component added to {player.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] AlphaCutoffController component already exists on {player.gameObject.name}.");
            }

            // Assign mainCamera if it's missing or if we're allowed to overwrite existing reference
            if (alphaCutoffController != null)
            {
                if (overwriteExisting || alphaCutoffController.mainCamera == null)
                {
                    // Try to find a GameObject named "MainCamera" and get its Camera component
                    Camera foundCam = null;
                    var camGO = GameObject.Find("MainCamera");
                    if (camGO != null)
                        foundCam = camGO.GetComponent<Camera>();

                    // Fallback to Camera.main (tag-based) if name-based lookup failed
                    if (foundCam == null)
                        foundCam = Camera.main;

                    if (foundCam != null)
                    {
                        alphaCutoffController.mainCamera = foundCam;
                        Debug.Log($"[AutoSetup] Assigned AlphaCutoffController.mainCamera on {player.gameObject.name} to {foundCam.gameObject.name}.");
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoSetup] Could not find a GameObject named 'MainCamera' with a Camera component (or Camera.main). AlphaCutoffController.mainCamera remains unset on {player.gameObject.name}.");
                    }
                }
            }

            EditorUtility.SetDirty(alphaCutoffController);
            PrefabUtility.RecordPrefabInstancePropertyModifications(alphaCutoffController);
        }
        
        private static void SetupCenterZoneOverlapCalculator(Player player, bool overwriteExisting)
        {
            if (player == null)
                return;

            var centerZoneOverlapCalculator = player.GetComponent<CenterZoneOverlapCalculator>();
            if (centerZoneOverlapCalculator == null)
            {
                centerZoneOverlapCalculator = player.gameObject.AddComponent<CenterZoneOverlapCalculator>();
                Debug.Log($"[AutoSetup] CenterZoneOverlapCalculator component added to {player.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] CenterZoneOverlapCalculator component already exists on {player.gameObject.name}.");
            }

            centerZoneOverlapCalculator.renderCam = Camera.main;
            centerZoneOverlapCalculator.playerFadeMask = LayerMask.GetMask("FadeCollider");
            centerZoneOverlapCalculator.sphereRadius = .1f;
            centerZoneOverlapCalculator.maxDistance = 3f;
            centerZoneOverlapCalculator.backOffset = 1f;

            EditorUtility.SetDirty(centerZoneOverlapCalculator);
            PrefabUtility.RecordPrefabInstancePropertyModifications(centerZoneOverlapCalculator);
        }

        private static void SetupPlayerInteractionSensor(Player player, bool overwriteExisting)
        {
            if (player == null || player.playerTemplate == null)
                return;

            // Find the PlayerInteractionSensor in the player's hierarchy
            Transform sensor = FindDirectChildByName(player.transform, "PlayerInteractionSensor");
            if (sensor == null)
            {
                GameObject prefab = player.playerTemplate.playerInteractionSensorPrefab;
                if (prefab == null)
                {
                    // Try to find a prefab named "PlayerInteractionSensor" in the project
                    string[] guids = AssetDatabase.FindAssets("PlayerInteractionSensor t:Prefab");
                    if (guids != null && guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        Debug.Log($"[AutoSetup] Fallback: Found PlayerInteractionSensor prefab at {path}.");
                    }
                }
                if (prefab != null)
                {
                    GameObject sensorInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
                    sensorInstance.name = "PlayerInteractionSensor";
                    Debug.Log($"[AutoSetup] PlayerInteractionSensor prefab instantiated and added to {player.gameObject.name}.");
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] PlayerInteractionSensor prefab not found. Cannot add to {player.gameObject.name}.");
                }
            }
        }

        private static void SetupWallDetector(Player player, bool overwriteExisting)
        {
            if (player == null || player.playerTemplate == null)
                return;

            // Find the WallDetector in the player's hierarchy
            Transform detector = FindDirectChildByName(player.transform, "WallDetector");
            if (detector == null)
            {
                GameObject prefab = player.playerTemplate.wallDetectorPrefab;
                if (prefab == null)
                {
                    // Try to find a prefab named "WallDetector" in the project
                    string[] guids = AssetDatabase.FindAssets("WallDetector t:Prefab");
                    if (guids != null && guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        Debug.Log($"[AutoSetup] Fallback: Found WallDetector prefab at {path}.");
                    }
                }
                if (prefab != null)
                {
                    GameObject detectorInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
                    detectorInstance.name = "WallDetector";
                    Debug.Log($"[AutoSetup] WallDetector prefab instantiated and added to {player.gameObject.name}.");
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] WallDetector prefab not found. Cannot add to {player.gameObject.name}.");
                }
            }
        }

        private static void SetupStairDetector(Player player, bool overwriteExisting)
        {
            if (player == null || player.playerTemplate == null)
                return;

            // Find the StairDetector in the player's hierarchy
            Transform detector = FindDirectChildByName(player.transform, "StairDetector");
            if (detector == null)
            {
                GameObject prefab = player.playerTemplate.stairDetectorPrefab;
                if (prefab == null)
                {
                    // Try to find a prefab named "StairDetector" in the project
                    string[] guids = AssetDatabase.FindAssets("StairDetector t:Prefab");
                    if (guids != null && guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        Debug.Log($"[AutoSetup] Fallback: Found StairDetector prefab at {path}.");
                    }
                }
                if (prefab != null)
                {
                    GameObject detectorInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
                    detectorInstance.name = "StairDetector";
                    Debug.Log($"[AutoSetup] StairDetector prefab instantiated and added to {player.gameObject.name}.");
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] StairDetector prefab not found. Cannot add to {player.gameObject.name}.");
                }
            }
        }

        private static void SetupFadeCollider(Player player, bool overwriteExisting)
        {
            if (player == null || player.playerTemplate == null)
                return;

            // Find the FadeCollider in the player's hierarchy
            Transform collider = FindDirectChildByName(player.transform, "FadeCollider");
            if (collider == null)
            {
                GameObject prefab = player.playerTemplate.fadeColliderPrefab;
                if (prefab == null)
                {
                    // Try to find a prefab named "FadeCollider" in the project
                    string[] guids = AssetDatabase.FindAssets("FadeCollider t:Prefab");
                    if (guids != null && guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        Debug.Log($"[AutoSetup] Fallback: Found FadeCollider prefab at {path}.");
                    }
                }
                if (prefab != null)
                {
                    GameObject colliderInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
                    colliderInstance.name = "FadeCollider";
                    Debug.Log($"[AutoSetup] FadeCollider prefab instantiated and added to {player.gameObject.name}.");
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] FadeCollider prefab not found. Cannot add to {player.gameObject.name}.");
                }
            }
        }
    }
}
