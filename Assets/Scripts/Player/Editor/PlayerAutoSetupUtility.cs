using RootMotion.FinalIK;
using UnityEngine;
using UnityEditor;
using System.Linq;

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
        SetupPlayerAimIK(player, overwriteExisting);
        SetupRecoilIK(player, overwriteExisting);
        SetupFBBIK(player, overwriteExisting);
        SetupBulletDecalManager(player);

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
        {
            type.GetProperty("PlayerIKController")?.SetValue(player, playerIKController);
            EditorUtility.SetDirty(playerIKController);
            PrefabUtility.RecordPrefabInstancePropertyModifications(playerIKController);
            // If auto setup is pressed and overwrite is true, set debugOverrideIKWeight to false
            var debugOverrideField = playerIKController.GetType().GetField("debugOverrideIKWeight", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (debugOverrideField != null)
            {
                debugOverrideField.SetValue(playerIKController, false);
                Debug.Log($"[AutoSetup] PlayerIKController.debugOverrideIKWeight set to false for {player.gameObject.name}.");
            }
            else
            {
                Debug.LogWarning($"[AutoSetup] Could not find 'debugOverrideIKWeight' field on PlayerIKController for {player.gameObject.name}.");
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

        // Only create FollowTarget and AimTarget
        Transform followTarget = FindDirectChildByName(player.transform, "FollowTarget");
        Transform aimIKTarget = FindDirectChildByName(player.transform, "AimIKTarget");
        Transform bulletHitTarget = FindDirectChildByName(player.transform, "BulletHitTarget");

        followTarget = GetOrCreateCameraTarget(player, followTarget, "FollowTarget", player.playerTemplate?.followTargetPrefab, new Vector3(0f, 1.775f, -0.009f));
        aimIKTarget = GetOrCreateCameraTarget(player, aimIKTarget, "AimIKTarget", player.playerTemplate?.aimIKTargetPrefab, Vector3.zero);
        bulletHitTarget = GetOrCreateCameraTarget(player, bulletHitTarget, "BulletHitTarget", player.playerTemplate?.bulletHitTargetPrefab, Vector3.zero);

        SetCinemachineFollow(player, followTarget, overwriteExisting);
        SetCameraControllerTargets(player, followTarget, aimIKTarget, bulletHitTarget, overwriteExisting);
        SetPlayerCameraField(player, overwriteExisting);
        SetCameraSettings(player, overwriteExisting);

        if (followTarget == null) Debug.LogWarning("FollowTarget not found as child of Player.");
        if (aimIKTarget == null) Debug.LogWarning("AimIKTarget not found as child of Player.");
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
                    cinemachineCamera.Follow = followTarget;
                    var after = cinemachineCamera.Follow;
                    Debug.Log($"[AutoSetup] CinemachineCamera.Follow after: {after.name} (ID: {after.GetInstanceID()})");
                }
                else if (cinemachineCamera.Follow == null)
                {
                    cinemachineCamera.Follow = followTarget;
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

    private static void SetupPlayerAimIK(Player player, bool overwriteExisting = true)
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
        aimIK.solver.target = FindChildRecursive(player.transform, "AimTarget");
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
            new(FindChildRecursive(player.transform, "spine_01.x"), 0f),
            new(FindChildRecursive(player.transform, "spine_02.x"), 0f),
            new(FindChildRecursive(player.transform, "spine_03.x"), 0.769f),
            new(FindChildRecursive(player.transform, "arm_stretch.r"), 1f),
            new(FindChildRecursive(player.transform, "forearm_stretch.r"), 1f),
            new(FindChildRecursive(player.transform, "hand.r"), 1f)
        };

        // Mark AimIK as dirty so changes persist
        EditorUtility.SetDirty(aimIK);
        PrefabUtility.RecordPrefabInstancePropertyModifications(aimIK);

        Debug.Log($"[AutoSetup] AimIK component set up for {player.gameObject.name} (disabled by default).");
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

        // Assign AimIK reference if available
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

        // --- NEW: Assign RecoilIK settings from PlayerWeaponManager's CurrentWeaponData ---
        var pwm = player.GetComponent<PlayerWeaponManager>();
        if (pwm != null)
        {
            var weaponData = pwm.CurrentWeaponData;
            if (weaponData == null)
            {
                // Try to auto-assign a WeaponData asset if one exists
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:WeaponData");
                if (guids != null && guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    var defaultWeaponData = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                    if (defaultWeaponData != null)
                    {
                        var currentWeaponDataField = pwm.GetType().GetField("currentWeaponData", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (currentWeaponDataField != null)
                        {
                            currentWeaponDataField.SetValue(pwm, defaultWeaponData);
                            weaponData = defaultWeaponData;
                            Debug.Log($"[AutoSetup] Auto-assigned default WeaponData '{defaultWeaponData.name}' to PlayerWeaponManager for {player.gameObject.name}.");
                            EditorUtility.SetDirty(pwm);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(pwm);
                        }
                    }
                }
            }
            if (weaponData != null)
            {
                // Map WeaponData fields to RecoilIK fields
                var recoil = recoilIK;
                var data = weaponData;
                recoil.ikRecoilWeight = data.ikRecoilWeight;
                recoil.aimIKSolvedLast = data.aimIKSolvedLast;
                recoil.handedness = (RecoilIK.Handedness)data.handedness;
                recoil.twoHanded = data.twoHanded;
                recoil.recoilWeight = data.recoilWeight;
                recoil.magnitudeRandom = data.magnitudeRandom;
                recoil.rotationRandom = data.rotationRandom;
                recoil.handRotationOffset = data.handRotationOffset;
                recoil.blendTime = data.blendTime;
                recoil.offsetSettings = new RecoilIK.OffsetSettings {
                    offset = data.offsets.offset,
                    additivity = data.offsets.additivity,
                    maxAdditiveOffsetMag = data.offsets.maxAdditiveOffsetMag
                };
                // Map EffectorLinks
                if (data.effectorLinks != null)
                {
                    recoil.effectorLinks = new RecoilIK.EffectorLink[data.effectorLinks.Length];
                    for (int i = 0; i < data.effectorLinks.Length; i++)
                    {
                        var src = data.effectorLinks[i];
                        recoil.effectorLinks[i] = new RecoilIK.EffectorLink {
                            effector = src.effector,
                            weight = src.weight
                        };
                    }
                }
                Debug.Log($"[AutoSetup] RecoilIK settings assigned from PlayerWeaponManager.CurrentWeaponData for {player.gameObject.name}.");
            }
            else
            {
                Debug.LogWarning($"[AutoSetup] CurrentWeaponData is null on PlayerWeaponManager for {player.gameObject.name} (even after attempting auto-assign).");
            }
        }
        else
        {
            Debug.LogWarning($"[AutoSetup] PlayerWeaponManager not found on {player.gameObject.name}.");
        }

        // Mark RecoilIK as dirty so changes persist
        EditorUtility.SetDirty(recoilIK);
        PrefabUtility.RecordPrefabInstancePropertyModifications(recoilIK);
    }

    private static void SetupFBBIK(Player player, bool overwriteExisting = true)
    {
        if (player == null)
            return;

        var fbbik = player.GetComponent<FullBodyBipedIK>();
        if (fbbik == null)
        {
            fbbik = player.gameObject.AddComponent<FullBodyBipedIK>();
            Debug.Log($"[AutoSetup] FullBodyBipedIK component added to {player.gameObject.name}.");
        }

        var references = fbbik.references;
        bool needsSetup = overwriteExisting || !references.isFilled;
        if (needsSetup)
        {
            // Set references.root to spine_01.x for the Root Node field
            var spine01 = FindChildRecursive(player.transform, "spine_01.x");
            try {
                references.root = spine01;
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.root for {player.gameObject.name}: {ex.Message}");
            }
            if (references.root == null)
                Debug.LogWarning($"[AutoSetup] FBBIK: spine_01.x not found for root on {player.gameObject.name}");
            // Also set RootNode property directly if it exists (for some FinalIK versions)
            var rootNodeProp = fbbik.GetType().GetProperty("RootNode", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (rootNodeProp != null && rootNodeProp.CanWrite)
            {
                try {
                    rootNodeProp.SetValue(fbbik, spine01);
                    Debug.Log($"[AutoSetup] FBBIK RootNode property set to spine_01.x for {player.gameObject.name}.");
                } catch (System.Exception ex) {
                    Debug.LogError($"[AutoSetup] FBBIK: Failed to set RootNode property for {player.gameObject.name}: {ex.Message}");
                }
            }
            // Also set solver.rootNode directly if property not found
            if ((rootNodeProp == null || !rootNodeProp.CanWrite) && fbbik.solver != null)
            {
                try {
                    fbbik.solver.rootNode = spine01;
                    Debug.Log($"[AutoSetup] FBBIK.solver.rootNode set to spine_01.x for {player.gameObject.name}.");
                } catch (System.Exception ex) {
                    Debug.LogError($"[AutoSetup] FBBIK: Failed to set solver.rootNode for {player.gameObject.name}: {ex.Message}");
                }
            }
            try {
                references.pelvis = FindChildRecursive(player.transform, "root.x");
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.pelvis for {player.gameObject.name}: {ex.Message}");
            }
            // Setup spine array (spine_01.x, spine_02.x, spine_03.x)
            var spineList = new System.Collections.Generic.List<Transform>();
            var s1 = spine01;
            var s2 = FindChildRecursive(player.transform, "spine_02.x");
            var s3 = FindChildRecursive(player.transform, "spine_03.x");
            if (s1 != null) spineList.Add(s1); else Debug.LogWarning($"[AutoSetup] FBBIK: spine_01.x not found on {player.gameObject.name}");
            if (s2 != null) spineList.Add(s2); else Debug.LogWarning($"[AutoSetup] FBBIK: spine_02.x not found on {player.gameObject.name}");
            if (s3 != null) spineList.Add(s3); else Debug.LogWarning($"[AutoSetup] FBBIK: spine_03.x not found on {player.gameObject.name}");
            try {
                references.spine = spineList.ToArray();
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.spine for {player.gameObject.name}: {ex.Message}");
            }
            try {
                references.head = FindChildRecursive(player.transform, "head.x");
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.head for {player.gameObject.name}: {ex.Message}");
            }
            if (references.head == null) Debug.LogWarning($"[AutoSetup] FBBIK: head.x not found on {player.gameObject.name}");
            try {
                references.leftThigh = FindChildRecursive(player.transform, "thigh_stretch.l");
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.leftThigh for {player.gameObject.name}: {ex.Message}");
            }
            if (references.leftThigh == null) Debug.LogWarning($"[AutoSetup] FBBIK: thigh_stretch.l not found on {player.gameObject.name}");
            try {
                references.leftCalf = FindChildRecursive(player.transform, "leg_stretch.l");
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.leftCalf for {player.gameObject.name}: {ex.Message}");
            }
            if (references.leftCalf == null) Debug.LogWarning($"[AutoSetup] FBBIK: leg_stretch.l not found on {player.gameObject.name}");
            try {
                references.leftFoot = FindChildRecursive(player.transform, "foot.l");
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.leftFoot for {player.gameObject.name}: {ex.Message}");
            }
            if (references.leftFoot == null) Debug.LogWarning($"[AutoSetup] FBBIK: foot.l not found on {player.gameObject.name}");
            try {
                references.rightThigh = FindChildRecursive(player.transform, "thigh_stretch.r");
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.rightThigh for {player.gameObject.name}: {ex.Message}");
            }
            if (references.rightThigh == null) Debug.LogWarning($"[AutoSetup] FBBIK: thigh_stretch.r not found on {player.gameObject.name}");
            try {
                references.rightCalf = FindChildRecursive(player.transform, "leg_stretch.r");
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.rightCalf for {player.gameObject.name}: {ex.Message}");
            }
            if (references.rightCalf == null) Debug.LogWarning($"[AutoSetup] FBBIK: leg_stretch.r not found on {player.gameObject.name}");
            try {
                references.rightFoot = FindChildRecursive(player.transform, "foot.r");
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.rightFoot for {player.gameObject.name}: {ex.Message}");
            }
            if (references.rightFoot == null) Debug.LogWarning($"[AutoSetup] FBBIK: foot.r not found on {player.gameObject.name}");
            try {
                references.leftUpperArm = FindChildRecursive(player.transform, "arm_stretch.l");
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.leftUpperArm for {player.gameObject.name}: {ex.Message}");
            }
            if (references.leftUpperArm == null) Debug.LogWarning($"[AutoSetup] FBBIK: arm_stretch.l not found on {player.gameObject.name}");
            try {
                references.leftForearm = FindChildRecursive(player.transform, "forearm_stretch.l");
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.leftForearm for {player.gameObject.name}: {ex.Message}");
            }
            if (references.leftForearm == null) Debug.LogWarning($"[AutoSetup] FBBIK: forearm_stretch.l not found on {player.gameObject.name}");
            try {
                references.leftHand = FindChildRecursive(player.transform, "hand.l");
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.leftHand for {player.gameObject.name}: {ex.Message}");
            }
            if (references.leftHand == null) Debug.LogWarning($"[AutoSetup] FBBIK: hand.l not found on {player.gameObject.name}");
            try {
                references.rightUpperArm = FindChildRecursive(player.transform, "arm_stretch.r");
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.rightUpperArm for {player.gameObject.name}: {ex.Message}");
            }
            if (references.rightUpperArm == null) Debug.LogWarning($"[AutoSetup] FBBIK: arm_stretch.r not found on {player.gameObject.name}");
            try {
                references.rightForearm = FindChildRecursive(player.transform, "forearm_stretch.r");
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.rightForearm for {player.gameObject.name}: {ex.Message}");
            }
            if (references.rightForearm == null) Debug.LogWarning($"[AutoSetup] FBBIK: forearm_stretch.r not found on {player.gameObject.name}");
            try {
                references.rightHand = FindChildRecursive(player.transform, "hand.r");
            } catch (System.Exception ex) {
                Debug.LogError($"[AutoSetup] FBBIK: Failed to set references.rightHand for {player.gameObject.name}: {ex.Message}");
            }
            if (references.rightHand == null) Debug.LogWarning($"[AutoSetup] FBBIK: hand.r not found on {player.gameObject.name}");
            fbbik.references = references;
            // Log all assigned references for debugging
            Debug.Log($"[AutoSetup] FBBIK references for {player.gameObject.name}:\n" +
                $"root: {references.root?.name}\n" +
                $"pelvis: {references.pelvis?.name}\n" +
                $"spine: {string.Join(", ", references.spine.Select(t => t?.name))}\n" +
                $"head: {references.head?.name}\n" +
                $"leftThigh: {references.leftThigh?.name}\n" +
                $"leftCalf: {references.leftCalf?.name}\n" +
                $"leftFoot: {references.leftFoot?.name}\n" +
                $"rightThigh: {references.rightThigh?.name}\n" +
                $"rightCalf: {references.rightCalf?.name}\n" +
                $"rightFoot: {references.rightFoot?.name}\n" +
                $"leftUpperArm: {references.leftUpperArm?.name}\n" +
                $"leftForearm: {references.leftForearm?.name}\n" +
                $"leftHand: {references.leftHand?.name}\n" +
                $"rightUpperArm: {references.rightUpperArm?.name}\n" +
                $"rightForearm: {references.rightForearm?.name}\n" +
                $"rightHand: {references.rightHand?.name}");
        }

        fbbik.enabled = false;
        EditorUtility.SetDirty(fbbik);
        PrefabUtility.RecordPrefabInstancePropertyModifications(fbbik);
    }

    private static void SetupBulletDecalManager(Player player)
    {
        if (player == null) return;
        var bulletDecalManager = player.GetComponent<BulletDecalManager>();
        if (bulletDecalManager == null) return;
        var defaultDecal = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Effects/BulletDecals/SM_Prop_BulletHoles_01.prefab");
        if (defaultDecal != null)
        {
            var field = bulletDecalManager.GetType().GetField("defaultDecal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(bulletDecalManager, defaultDecal);
                Debug.Log($"[AutoSetup] BulletDecalManager.defaultDecal set to SM_Prop_BulletHoles_01.prefab for {player.gameObject.name}.");
            }
            else
            {
                Debug.LogWarning($"[AutoSetup] Could not find 'defaultDecal' field on BulletDecalManager for {player.gameObject.name}.");
            }
        }
        else
        {
            Debug.LogWarning($"[AutoSetup] Could not load BulletDecal prefab at Assets/Prefabs/Effects/BulletDecals/SM_Prop_BulletHoles_01.prefab");
        }

        // Setup Material Decals
        var materialDecalsField = bulletDecalManager.GetType().GetField("materialDecals", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (materialDecalsField != null)
        {
            // Load all 3 bullet hole prefabs
            var decal1 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Effects/BulletDecals/SM_Prop_BulletHoles_01.prefab");
            var decal2 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Effects/BulletDecals/SM_Prop_BulletHoles_02.prefab");
            var decal3 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Effects/BulletDecals/SM_Prop_BulletHoles_03.prefab");
            var bulletDecalPrefabs = new GameObject[] { decal1, decal2, decal3 };
            var materials = UnityEditor.AssetDatabase.FindAssets("t:PhysicsMaterial", new[] { "Assets/PhysicsMaterials" });
            var materialDecalsList = new System.Collections.Generic.List<object>();
            foreach (var guid in materials)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var mat = UnityEditor.AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
                if (mat != null)
                {
                    // Try to create a material decal entry (assumes a struct/class with 'material' and 'bulletDecalPrefabs' fields)
                    var decalType = materialDecalsField.FieldType.GetElementType() ?? materialDecalsField.FieldType.GetGenericArguments()[0];
                    var decalEntry = System.Activator.CreateInstance(decalType);
                    var matField = decalType.GetField("material");
                    var prefabsField = decalType.GetField("bulletDecalPrefabs");
                    if (matField != null && prefabsField != null)
                    {
                        matField.SetValue(decalEntry, mat);
                        prefabsField.SetValue(decalEntry, bulletDecalPrefabs);
                        materialDecalsList.Add(decalEntry);
                    }
                }
            }
            // Assign the array/list to the field
            var elementType = materialDecalsField.FieldType.IsArray
                ? materialDecalsField.FieldType.GetElementType()
                : materialDecalsField.FieldType.GetGenericArguments()[0];
            if (materialDecalsField.FieldType.IsArray)
            {
                var typedArray = System.Array.CreateInstance(elementType, materialDecalsList.Count);
                for (int i = 0; i < materialDecalsList.Count; i++)
                    typedArray.SetValue(materialDecalsList[i], i);
                materialDecalsField.SetValue(bulletDecalManager, typedArray);
            }
            else
            {
                materialDecalsField.SetValue(bulletDecalManager, materialDecalsList);
            }
            Debug.Log($"[AutoSetup] BulletDecalManager.materialDecals set up with {materialDecalsList.Count} entries for {player.gameObject.name}.");
        }
        else
        {
            Debug.LogWarning($"[AutoSetup] Could not find 'materialDecals' field on BulletDecalManager for {player.gameObject.name}.");
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Player))]
public class PlayerAutoSetupEditor : Editor
{
    private bool overwriteExisting = false;
    private static bool autoSetupLocked = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        overwriteExisting = EditorGUILayout.ToggleLeft("Overwrite Existing Values", overwriteExisting);
        EditorGUILayout.HelpBox("If checked, all values will be overwritten with those from the PlayerTemplate asset.", MessageType.Info);
        EditorGUILayout.Space();
        // Lock toggle
        autoSetupLocked = EditorGUILayout.ToggleLeft("\U0001F512 Lock Auto Setup Button (prevent accidental press)", autoSetupLocked);
        EditorGUILayout.Space();
        // Make the button larger and more visually prominent
        GUIStyle bigButton = new GUIStyle(GUI.skin.button);
        bigButton.fontSize = 16;
        bigButton.fontStyle = FontStyle.Bold;
        bigButton.fixedHeight = 40;
        bigButton.margin = new RectOffset(0, 0, 10, 10);
        EditorGUI.BeginDisabledGroup(autoSetupLocked);
        if (GUILayout.Button(autoSetupLocked ? "Auto Setup Player (Locked)" : "Auto Setup Player", bigButton))
        {
            var player = (Player)target;
            PlayerAutoSetupUtility.AutoSetupReferences(player, overwriteExisting);
        }
        EditorGUI.EndDisabledGroup();
    }
}
#endif
