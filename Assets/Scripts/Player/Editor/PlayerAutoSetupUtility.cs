using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

public static class PlayerAutoSetupUtility
{
    public static void AutoSetupReferences(Player player, bool overwriteExisting = true)
    {
        if (player == null)
        {
            Debug.LogError("PlayerAutoSetupUtility: Player reference is null. Aborting auto-setup.");
            return;
        }

        SetupPlayerComponentReferences(player);
        SetupPlayerTemplate(player);
        SetupAnimator(player, overwriteExisting);
        SetupCameraTargetsAndSettings(player, overwriteExisting);
        SetupCrosshairController(player);
        SetupPlayerInput(player, overwriteExisting);
        SetupCharacterControllerFromTemplate(player, overwriteExisting);
        SetupPlayerWeaponManager(player, overwriteExisting);
        SetupPlayerWeaponHand(player);

        // Set layer to Player for this GameObject and all children
        if (player.gameObject != null)
            SetLayerRecursively(player.gameObject, LayerMask.NameToLayer("Player"));

        Debug.Log($"[{player.gameObject.name}] Auto-setup complete.");
        EditorUtility.SetDirty(player);
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

    private static void SetupPlayerTemplate(Player player)
    {
        if (player.playerTemplate == null)
        {
            string assetPath = "Assets/ScriptableObjects/Player/PlayerTemplate.asset";
            var loadedTemplate = AssetDatabase.LoadAssetAtPath<PlayerTemplate>(assetPath);
            if (loadedTemplate != null)
            {
                player.playerTemplate = loadedTemplate;
                Debug.Log($"[{player.gameObject.name}] AutoSetupReferences: Assigned PlayerTemplate from {assetPath}.");
            }
            else
            {
                Debug.LogWarning($"[{player.gameObject.name}] AutoSetupReferences: Could not find PlayerTemplate at {assetPath}.");
            }
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
            type.GetProperty("PlayerIKController")?.SetValue(player, playerIKController);
        var weaponManager = player.GetComponent<PlayerWeaponManager>();
        if (weaponManager != null)
            type.GetProperty("WeaponManager")?.SetValue(player, weaponManager);
        var healthManager = player.GetComponent<HealthManager>();
        if (healthManager != null)
            type.GetProperty("HealthManager")?.SetValue(player, healthManager);
        var recoil = player.GetComponent<IKRecoil>();
        if (recoil != null)
            type.GetProperty("Recoil")?.SetValue(player, recoil);
        var bulletHitscan = player.GetComponent<BulletHitscan>();
        if (bulletHitscan != null)
            type.GetProperty("BulletHitscan")?.SetValue(player, bulletHitscan);
        var bulletDecalManager = player.GetComponent<BulletDecalManager>();
        if (bulletDecalManager != null)
            type.GetProperty("BulletDecalManager")?.SetValue(player, bulletDecalManager);
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
                Debug.Log($"[AutoSetup] AnimatorController after: {animator.runtimeAnimatorController.name}");
            }
            else if (animator.runtimeAnimatorController == null)
            {
                animator.runtimeAnimatorController = player.playerTemplate.animatorController;
            }
        }
    }

    private static void SetupPlayerInput(Player player, bool overwriteExisting = true)
    {
        if (player == null || player.playerTemplate == null || player.PlayerInput == null)
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
        if (weaponManager != null)
        {
            // Try to load the MP40WeaponData asset from a known path
            var mp40WeaponData = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/ScriptableObjects/Weapons/MP40WeaponData.asset");
            if (mp40WeaponData != null)
            {
                // Set the private serialized field 'currentWeaponData' via reflection
                var currentWeaponDataField = weaponManager.GetType().GetField("currentWeaponData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (currentWeaponDataField != null)
                {
                    if (overwriteExisting)
                    {
                        currentWeaponDataField.SetValue(weaponManager, mp40WeaponData);
                        Debug.Log($"[AutoSetup] PlayerWeaponManager.currentWeaponData set to MP40WeaponData for {player.gameObject.name} (overwrite: true).");
                    }
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] Could not find currentWeaponData field on PlayerWeaponManager for {player.gameObject.name}.");
                }
            }
            else
            {
                Debug.LogWarning($"[AutoSetup] Could not find MP40WeaponData asset at Assets/ScriptableObjects/Weapons/MP40WeaponData.asset");
            }
        }
        else
        {
            Debug.LogWarning($"[AutoSetup] PlayerWeaponManager component not found on {player.gameObject.name}.");
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
            if (player.playerTemplate.forwardFollowTargetPrefab != null) // Replace with weaponHandPrefab if available
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

    private static void SetupCameraTargetsAndSettings(Player player, bool overwriteExisting = true)
    {
        if (player == null || player.PlayerCameraController == null)
            return;

        // Always create new targets instead of searching for existing ones
        Transform aimFollowTarget = FindDirectChildByName(player.transform, "AimFollowTarget");
        Transform forwardFollowTarget = FindDirectChildByName(player.transform, "ForwardFollowTarget");
        Transform aimTarget = FindDirectChildByName(player.transform, "AimTarget");

        aimFollowTarget = GetOrCreateCameraTarget(player, aimFollowTarget, "AimFollowTarget", player.playerTemplate?.aimFollowTargetPrefab, new Vector3(0.36f, 1.6f, -0.013f));
        forwardFollowTarget = GetOrCreateCameraTarget(player, forwardFollowTarget, "ForwardFollowTarget", player.playerTemplate?.forwardFollowTargetPrefab, new Vector3(0f, 1.775f, -0.009f));
        aimTarget = GetOrCreateCameraTarget(player, aimTarget, "AimTarget", player.playerTemplate?.aimTargetPrefab, Vector3.zero);

        SetCinemachineFollow(player, forwardFollowTarget, overwriteExisting);
        SetCameraControllerTargets(player, forwardFollowTarget, aimTarget, overwriteExisting);
        SetPlayerCameraField(player, overwriteExisting);
        SetCameraFOVAndZoom(player, overwriteExisting);

        if (aimFollowTarget == null) Debug.LogWarning("AimFollowTarget not found as child of Player.");
        if (forwardFollowTarget == null) Debug.LogWarning("ForwardFollowTarget not found as child of Player.");
        if (aimTarget == null) Debug.LogWarning("AimTarget not found as child of Player.");
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
            }
            else
            {
                newTarget = new GameObject(name);
                newTarget.transform.SetParent(player.transform);
                newTarget.transform.localPosition = defaultPosition;
                newTarget.transform.localRotation = Quaternion.identity;
                newTarget.transform.localScale = Vector3.one;
            }
            Debug.Log($"Created {name} as child of Player.");
            return newTarget.transform;
        }
        else
        {
            Debug.Log($"Found existing {name} as child of Player.");
            return existing;
        }
    }

    private static void SetCinemachineFollow(Player player, Transform forwardFollowTarget, bool overwriteExisting)
    {
        var playerCameraGO = GameObject.Find("Cameras/PlayerCamera");
        if (playerCameraGO != null && forwardFollowTarget != null)
        {
            var cinemachineCamera = playerCameraGO.GetComponent<Unity.Cinemachine.CinemachineCamera>();
            if (cinemachineCamera != null)
            {
                var before = cinemachineCamera.Follow;
                if (overwriteExisting)
                {
                    Debug.Log($"[AutoSetup] CinemachineCamera.Follow before: {(before != null ? before.name : "null")} (ID: {(before != null ? before.GetInstanceID().ToString() : "null")}), template: {forwardFollowTarget.name} (ID: {forwardFollowTarget.GetInstanceID()}), overwrite: {overwriteExisting}");
                    if (before != forwardFollowTarget)
                        Debug.Log($"[AutoSetup] Overwriting CinemachineCamera.Follow: {(before != null ? before.name : "null")} -> {forwardFollowTarget.name}");
                    cinemachineCamera.Follow = forwardFollowTarget;
                    var after = cinemachineCamera.Follow;
                    Debug.Log($"[AutoSetup] CinemachineCamera.Follow after: {after.name} (ID: {after.GetInstanceID()})");
                }
                else if (cinemachineCamera.Follow == null)
                {
                    cinemachineCamera.Follow = forwardFollowTarget;
                }
            }
        }
    }

    private static void SetCameraControllerTargets(Player player, Transform forwardFollowTarget, Transform aimTarget, bool overwriteExisting)
    {
        var forwardsFollowTargetField = player.PlayerCameraController.GetType().GetField("forwardsFollowTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (forwardsFollowTargetField != null && forwardFollowTarget != null)
        {
            var before = forwardsFollowTargetField.GetValue(player.PlayerCameraController) as Transform;
            if (overwriteExisting)
            {
                Debug.Log($"[AutoSetup] PlayerCameraController.forwardsFollowTarget before: {(before != null ? before.name : "null")} (ID: {(before != null ? before.GetInstanceID().ToString() : "null")}), template: {forwardFollowTarget.name} (ID: {forwardFollowTarget.GetInstanceID()}), overwrite: {overwriteExisting}");
                if (before != forwardFollowTarget)
                    Debug.Log($"[AutoSetup] Overwriting PlayerCameraController.forwardsFollowTarget: {(before != null ? before.name : "null")} -> {forwardFollowTarget.name}");
                forwardsFollowTargetField.SetValue(player.PlayerCameraController, forwardFollowTarget);
            }
            else if (before == null)
            {
                forwardsFollowTargetField.SetValue(player.PlayerCameraController, forwardFollowTarget);
            }
            if (overwriteExisting)
            {
                var after = forwardsFollowTargetField.GetValue(player.PlayerCameraController) as Transform;
                Debug.Log($"[AutoSetup] PlayerCameraController.forwardsFollowTarget after: {(after != null ? after.name : "null")} (ID: {(after != null ? after.GetInstanceID().ToString() : "null")})");
            }
        }
        var aimTargetField = player.PlayerCameraController.GetType().GetField("aimTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (aimTargetField != null && aimTarget != null)
        {
            var before = aimTargetField.GetValue(player.PlayerCameraController) as Transform;
            if (overwriteExisting)
            {
                Debug.Log($"[AutoSetup] PlayerCameraController.aimTarget before: {(before != null ? before.name : "null")} (ID: {(before != null ? before.GetInstanceID().ToString() : "null")}), template: {aimTarget.name} (ID: {aimTarget.GetInstanceID()}), overwrite: {overwriteExisting}");
                if (before != aimTarget)
                    Debug.Log($"[AutoSetup] Overwriting PlayerCameraController.aimTarget: {(before != null ? before.name : "null")} -> {aimTarget.name}");
                aimTargetField.SetValue(player.PlayerCameraController, aimTarget);
            }
            else if (before == null)
            {
                aimTargetField.SetValue(player.PlayerCameraController, aimTarget);
            }
            if (overwriteExisting)
            {
                var after = aimTargetField.GetValue(player.PlayerCameraController) as Transform;
                Debug.Log($"[AutoSetup] PlayerCameraController.aimTarget after: {(after != null ? after.name : "null")} (ID: {(after != null ? after.GetInstanceID().ToString() : "null")})");
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

    private static void SetCameraFOVAndZoom(Player player, bool overwriteExisting)
    {
        if (player.playerTemplate != null)
        {
            var followFOVField = player.PlayerCameraController.GetType().GetField("followFOV", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (followFOVField != null)
            {
                var before = (float)followFOVField.GetValue(player.PlayerCameraController);
                if (overwriteExisting)
                {
                    Debug.Log($"[AutoSetup] PlayerCameraController.followFOV before: {before}, template: {player.playerTemplate.followFOV}, overwrite: {overwriteExisting}");
                    if (before != player.playerTemplate.followFOV)
                        Debug.Log($"[AutoSetup] Overwriting PlayerCameraController.followFOV: {before} -> {player.playerTemplate.followFOV}");
                    followFOVField.SetValue(player.PlayerCameraController, player.playerTemplate.followFOV);
                    var after = (float)followFOVField.GetValue(player.PlayerCameraController);
                    Debug.Log($"[AutoSetup] PlayerCameraController.followFOV after: {after}");
                }
                else if (before == default(float))
                {
                    followFOVField.SetValue(player.PlayerCameraController, player.playerTemplate.followFOV);
                }
            }
            var aimFOVField = player.PlayerCameraController.GetType().GetField("aimFOV", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (aimFOVField != null)
            {
                var before = (float)aimFOVField.GetValue(player.PlayerCameraController);
                if (overwriteExisting)
                {
                    Debug.Log($"[AutoSetup] PlayerCameraController.aimFOV before: {before}, template: {player.playerTemplate.aimFOV}, overwrite: {overwriteExisting}");
                    if (overwriteExisting && before != player.playerTemplate.aimFOV)
                        Debug.Log($"[AutoSetup] Overwriting PlayerCameraController.aimFOV: {before} -> {player.playerTemplate.aimFOV}");
                    aimFOVField.SetValue(player.PlayerCameraController, player.playerTemplate.aimFOV);
                    var after = (float)aimFOVField.GetValue(player.PlayerCameraController);
                    Debug.Log($"[AutoSetup] PlayerCameraController.aimFOV after: {after}");
                }
                else if (before == default(float))
                {
                    aimFOVField.SetValue(player.PlayerCameraController, player.playerTemplate.aimFOV);
                }
            }
            var zoomSpeedField = player.PlayerCameraController.GetType().GetField("zoomSpeed", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (zoomSpeedField != null)
            {
                var before = (float)zoomSpeedField.GetValue(player.PlayerCameraController);
                if (overwriteExisting)
                {
                    Debug.Log($"[AutoSetup] PlayerCameraController.zoomSpeed before: {before}, template: {player.playerTemplate.zoomSpeed}, overwrite: {overwriteExisting}");
                    if (overwriteExisting && before != player.playerTemplate.zoomSpeed)
                        Debug.Log($"[AutoSetup] Overwriting PlayerCameraController.zoomSpeed: {before} -> {player.playerTemplate.zoomSpeed}");
                    zoomSpeedField.SetValue(player.PlayerCameraController, player.playerTemplate.zoomSpeed);
                    var after = (float)zoomSpeedField.GetValue(player.PlayerCameraController);
                    Debug.Log($"[AutoSetup] PlayerCameraController.zoomSpeed after: {after}");
                }
                else if (before == default(float))
                {
                    zoomSpeedField.SetValue(player.PlayerCameraController, player.playerTemplate.zoomSpeed);
                }
            }
            if (overwriteExisting)
            {
                Debug.Log($"[{player.gameObject.name}] AutoSetupReferences: Set camera FOV and zoom speed from PlayerTemplate. Overwrite: {overwriteExisting}");
            }
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
                var crosshairField = player.GetType().GetField("crosshairController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (crosshairField != null)
                {
                    crosshairField.SetValue(player, crosshair);
                }
                Debug.Log($"[{player.gameObject.name}] AutoSetupReferences: Assigned CrosshairController automatically.");
            }
            else
            {
                Debug.LogWarning($"[{player.gameObject.name}] AutoSetupReferences: Could not find CrosshairController in the scene.");
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
}

#if UNITY_EDITOR
[CustomEditor(typeof(Player))]
public class PlayerAutoSetupEditor : Editor
{
    private bool overwriteExisting = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        overwriteExisting = EditorGUILayout.ToggleLeft("Overwrite Existing Values", overwriteExisting);
        EditorGUILayout.HelpBox("If checked, all values will be overwritten with those from the PlayerTemplate asset.", MessageType.Info);
        // Make the button larger and more visually prominent
        GUIStyle bigButton = new GUIStyle(GUI.skin.button);
        bigButton.fontSize = 16;
        bigButton.fontStyle = FontStyle.Bold;
        bigButton.fixedHeight = 40;
        bigButton.margin = new RectOffset(0, 0, 10, 10);
        if (GUILayout.Button("Auto Setup References", bigButton))
        {
            var player = (Player)target;
            PlayerAutoSetupUtility.AutoSetupReferences(player, overwriteExisting);
        }
    }
}
#endif
