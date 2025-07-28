using Pathfinding;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UndeadSurvivalGame.Editor
{
    public static class EnemyAutoSetupUtility
    {
        // Maps bone name (string) to LimbType
        private static readonly Dictionary<string, LimbType> boneLimbTypeMap = new Dictionary<string, LimbType>()
        {
            { "root.x", LimbType.Torso },
            { "thigh_stretch.l", LimbType.UpperLeg },
            { "leg_stretch.l", LimbType.LowerLeg },
            { "foot.l", LimbType.Foot },
            { "thigh_stretch.r", LimbType.UpperLeg },
            { "leg_stretch.r", LimbType.LowerLeg },
            { "foot.r", LimbType.Foot },
            { "spine_02.x", LimbType.Stomach },
            { "head.x", LimbType.Head },
            { "arm_stretch.l", LimbType.UpperArm },
            { "forearm_stretch.l", LimbType.LowerArm },
            { "hand.l", LimbType.Hand },
            { "arm_stretch.r", LimbType.UpperArm },
            { "forearm_stretch.r", LimbType.LowerArm },
            { "hand.r", LimbType.Hand }
        };

        private static readonly Dictionary<string, LimbTemplate> boneLimbTemplateMap = new Dictionary<string, LimbTemplate>()
        {
            { "root.x", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieTorsoTemplate.asset")},
            { "thigh_stretch.l", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieUpperLegTemplate.asset")},
            { "leg_stretch.l", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieLowerLegTemplate.asset")},
            { "foot.l", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieFootTemplate.asset")},
            { "thigh_stretch.r", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieUpperLegTemplate.asset")},
            { "leg_stretch.r", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieLowerLegTemplate.asset")},
            { "foot.r", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieFootTemplate.asset")},
            { "spine_02.x", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieStomachTemplate.asset")},
            { "head.x", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieHeadTemplate.asset")},
            { "arm_stretch.l", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieUpperArmTemplate.asset")},
            { "forearm_stretch.l", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieLowerArmTemplate.asset")},
            { "hand.l", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieHandTemplate.asset")},
            { "arm_stretch.r", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieUpperArmTemplate.asset")},
            { "forearm_stretch.r", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieLowerArmTemplate.asset")},
            { "hand.r", AssetDatabase.LoadAssetAtPath<LimbTemplate>("Assets/ScriptableObjects/Enemies/ZombieHandTemplate.asset")}
        };

        public static void AutoSetupReferences(UndeadSurvivalGame.Enemy.Enemy enemy, bool overwriteExisting = false)
        {
            Debug.Log($"[{enemy.gameObject.name}] Auto-Setting up references...");

            if (enemy == null)
            {
                Debug.LogError("Enemy reference is null. Cannot auto-setup.");
                return;
            }

            SetupEnemyTemplate(enemy);

            if (enemy.enemyTemplate == null)
            {
                Debug.LogError($"[{enemy.gameObject.name}] EnemyTemplate is not assigned. Cannot auto-setup.");
                return;
            }

            SetupPuppetMasterReference(enemy, overwriteExisting);
            SetupAnimator(enemy, overwriteExisting);
            SetupEnemyAnimatorEvents(enemy, overwriteExisting);
            SetupFollowerEntity(enemy, overwriteExisting);
            SetupAIDestinationSetter(enemy, overwriteExisting);
            SetupHealthManagerForEnemy(enemy, overwriteExisting);
            SetupLookAtIK(enemy, overwriteExisting);
            SetupEnemyDebugger(enemy, overwriteExisting);
            SetupBipedRagdollCreator(enemy, overwriteExisting);
            SetLayerRecursively(enemy.gameObject, LayerMask.NameToLayer("Enemy"), overwriteExisting);
            SetupLimbs(enemy, overwriteExisting);
        }

        private static void SetupEnemyTemplate(UndeadSurvivalGame.Enemy.Enemy enemy)
        {
            if (enemy == null)
                return;
            if (enemy.enemyTemplate == null)
            {
                // Try to find any ZombieEnemyTemplate asset in the project
                string[] guids = AssetDatabase.FindAssets("ZombieEnemyTemplate t:ScriptableObject");
                if (guids != null && guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var mainAssembly = typeof(UndeadSurvivalGame.Enemy.Enemy).Assembly;
                    var enemyTemplateType = mainAssembly.GetType("EnemyTemplate");
                    var loadedTemplate = AssetDatabase.LoadAssetAtPath(path, enemyTemplateType);
                    if (loadedTemplate != null)
                    {
                        var templateProp = typeof(UndeadSurvivalGame.Enemy.Enemy).GetProperty("enemyTemplate");
                        if (templateProp != null && templateProp.CanWrite)
                        {
                            templateProp.SetValue(enemy, loadedTemplate);
                            EditorUtility.SetDirty(enemy);
                            Debug.Log($"[{enemy.gameObject.name}] AutoSetupReferences: Assigned ZombieEnemyTemplate from {path}.");
                        }
                        else
                        {
                            var templateField = typeof(UndeadSurvivalGame.Enemy.Enemy).GetField("enemyTemplate");
                            if (templateField != null)
                            {
                                templateField.SetValue(enemy, loadedTemplate);
                                EditorUtility.SetDirty(enemy);
                                Debug.Log($"[{enemy.gameObject.name}] AutoSetupReferences: Assigned ZombieEnemyTemplate from {path} (via field).");
                            }
                            else
                            {
                                Debug.LogWarning($"[{enemy.gameObject.name}] AutoSetupReferences: Could not assign ZombieEnemyTemplate to enemy (no property or field found).");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[{enemy.gameObject.name}] AutoSetupReferences: Could not load ZombieEnemyTemplate at {path}.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[{enemy.gameObject.name}] AutoSetupReferences: No ZombieEnemyTemplate asset found in project.");
                }
            }
        }

        private static void SetupPuppetMasterReference(UndeadSurvivalGame.Enemy.Enemy enemy, bool overwriteExisting = true)
        {
            if (enemy == null)
                return;

            // Try to find a sibling with a PuppetMaster component
            var parent = enemy.transform.parent;
            if (parent == null)
            {
                Debug.LogWarning($"[SetupPuppetMasterReference] {enemy.gameObject.name} has no parent, cannot search for PuppetMaster sibling.");
                return;
            }

            PuppetMaster foundPuppetMaster = null;
            foreach (Transform sibling in parent)
            {
                if (sibling == enemy.transform)
                    continue;
                var puppetMaster = sibling.GetComponent<PuppetMaster>();
                if (puppetMaster != null)
                {
                    foundPuppetMaster = puppetMaster;
                    Debug.Log($"[SetupPuppetMasterReference] Found PuppetMaster on sibling '{sibling.gameObject.name}' for Enemy '{enemy.gameObject.name}'.");
                    break;
                }
            }

            if (foundPuppetMaster == null)
            {
                Debug.LogWarning($"[SetupPuppetMasterReference] No PuppetMaster found among siblings for Enemy '{enemy.gameObject.name}'.");
                return;
            }

            // Try to assign the PuppetMaster reference to the Enemy
            var type = enemy.GetType();
            var pmField = type.GetField("puppetMasterReference", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (pmField != null)
            {
                pmField.SetValue(enemy, foundPuppetMaster);
                Debug.Log($"[SetupPuppetMasterReference] Assigned PuppetMaster to field 'puppetMasterReference' on Enemy '{enemy.gameObject.name}'.");
            }
            else
            {
                var pmProp = type.GetProperty("puppetMasterReference", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (pmProp != null && pmProp.CanWrite)
                {
                    pmProp.SetValue(enemy, foundPuppetMaster);
                    Debug.Log($"[SetupPuppetMasterReference] Assigned PuppetMaster to property 'puppetMasterReference' on Enemy '{enemy.gameObject.name}'.");
                }
                else
                {
                    Debug.LogWarning($"[SetupPuppetMasterReference] Could not assign PuppetMaster to Enemy '{enemy.gameObject.name}' (no field or writable property named 'puppetMasterReference').");
                }
            }
        }

        private static void SetupAnimator(UndeadSurvivalGame.Enemy.Enemy enemy, bool overwriteExisting = true)
        {
            if (enemy == null) return;
            var animator = enemy.GetComponent<Animator>();
            if (animator != null && enemy.enemyTemplate != null)
            {
                // If your EnemyTemplate has an animatorController field, use it here
                var templateType = enemy.enemyTemplate.GetType();
                var animatorControllerField = templateType.GetField("animatorController");
                var animatorControllerProp = templateType.GetProperty("animatorController");
                RuntimeAnimatorController templateController = null;
                if (animatorControllerField != null)
                {
                    templateController = animatorControllerField.GetValue(enemy.enemyTemplate) as UnityEngine.RuntimeAnimatorController;
                }
                else if (animatorControllerProp != null && animatorControllerProp.CanRead)
                {
                    templateController = animatorControllerProp.GetValue(enemy.enemyTemplate) as UnityEngine.RuntimeAnimatorController;
                }
                if (templateController != null)
                {
                    var before = animator.runtimeAnimatorController;
                    if (overwriteExisting)
                    {
                        Debug.Log($"[AutoSetup] AnimatorController before: {(before != null ? before.name : "null")}, template: {templateController.name}, overwrite: {overwriteExisting}");
                        if (before != templateController)
                            Debug.Log($"[AutoSetup] Overwriting Animator.runtimeAnimatorController: {(before != null ? before.name : "null")} -> {templateController.name}");
                        animator.runtimeAnimatorController = templateController;
                        animator.applyRootMotion = false; // Disable root motion if overwriting
                        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                        Debug.Log($"[AutoSetup] AnimatorController after: {animator.runtimeAnimatorController.name}, applyRootMotion: {animator.applyRootMotion}");
                    }
                    else if (animator.runtimeAnimatorController == null)
                    {
                        animator.runtimeAnimatorController = templateController;
                    }
                }
            }
        }

        private static void SetupEnemyAnimatorEvents(UndeadSurvivalGame.Enemy.Enemy enemy, bool overwriteExisting = true)
        {
            if (enemy == null) return;

            var animatorEvents = enemy.GetComponent<EnemyAnimatorEvents>();
            if (animatorEvents == null)
            {
                animatorEvents = enemy.gameObject.AddComponent<EnemyAnimatorEvents>();
                Debug.Log($"[AutoSetup] EnemyAnimatorEvents component added to {enemy.gameObject.name}.");
            }
            else if (!overwriteExisting)
            {
                Debug.Log($"[AutoSetup] EnemyAnimatorEvents already exists on {enemy.gameObject.name}, skipping setup.");
                return;
            }

            EditorUtility.SetDirty(animatorEvents);
        }

        private static void SetupFollowerEntity(UndeadSurvivalGame.Enemy.Enemy enemy, bool overwriteExisting = true)
        {
            if (enemy == null || enemy.enemyTemplate == null)
                return;

            var follower = enemy.GetComponent<FollowerEntity>();

            if (follower == null)
            {
                follower = enemy.gameObject.AddComponent<FollowerEntity>();
                Debug.Log($"[AutoSetup] FollowerEntity component added to {enemy.gameObject.name}.");
            }

            follower.enabled = false;
            var template = enemy.enemyTemplate;

            // Shape
            follower.radius = template.followerRadius;
            follower.height = template.followerHeight;
            follower.orientation = (Pathfinding.OrientationMode)template.followerOrientation;

            // Movement
            var move = follower.movementSettings;
            move.follower.speed = template.followerSpeed;
            move.follower.rotationSpeed = template.followerRotationSpeed;
            move.follower.maxRotationSpeed = template.followerMaxRotationSpeed;
            move.follower.allowRotatingOnSpot = template.followerAllowRotatingOnTheSpot;
            follower.positionSmoothing = template.followerPositionSmoothing;
            follower.rotationSmoothing = template.followerRotationSmoothing;
            move.follower.slowdownTime = template.followerSlowdownTime;
            move.stopDistance = template.followerStopDistance;
            Debug.Log($"[AutoSetup] FollowerEntity.stopDistance set to {move.stopDistance} for {enemy.gameObject.name}.");
            move.follower.leadInRadiusWhenApproachingDestination = template.followerLeadInRadius;
            move.follower.desiredWallDistance = template.followerDesiredWallDistance;
            move.groundMask = LayerMask.GetMask(template.followerRaycastGroundMask);
            follower.movementSettings = move;

            // Pathfinding
            // Pathfinding settings can be set here if needed

            // Debug
            // Debug flags can be set here if needed

            EditorUtility.SetDirty(follower);
            Debug.Log($"[AutoSetup] FollowerEntity settings applied from EnemyTemplate to {enemy.gameObject.name}.");
        }

        private static void SetupAIDestinationSetter(UndeadSurvivalGame.Enemy.Enemy enemy, bool overwriteExisting = true)
        {
            if (enemy == null || enemy.enemyTemplate == null)
                return;

            var aiDestinationSetter = enemy.GetComponent<AIDestinationSetter>();

            if (aiDestinationSetter == null)
            {
                aiDestinationSetter = enemy.gameObject.AddComponent<AIDestinationSetter>();
                Debug.Log($"[AutoSetup] AIDestinationSetter component added to {enemy.gameObject.name}.");
            }

            aiDestinationSetter.enabled = false;
            aiDestinationSetter.target = enemy.PlayerTransform;

            if (aiDestinationSetter.target == null)
            {
                Debug.LogWarning($"[{enemy.gameObject.name}] AIDestinationSetter: PlayerTransform is not assigned. Cannot set target.");
            }
            else
            {
                Debug.Log($"[{enemy.gameObject.name}] AIDestinationSetter: Target set to {aiDestinationSetter.target.name}.");
            }

            // Optionally set other properties from the template if needed
        }

        private static void SetupHealthManagerForEnemy(UndeadSurvivalGame.Enemy.Enemy enemy, bool overwriteExisting = true)
        {
            if (enemy == null || enemy.enemyTemplate == null)
                return;

            var healthManager = enemy.GetComponent<HealthManager>();
            bool added = false;
            if (healthManager == null)
            {
                healthManager = enemy.gameObject.AddComponent<HealthManager>();
                added = true;
                Debug.Log($"[AutoSetup] HealthManager component added to {enemy.gameObject.name}.");
            }

            if (added || overwriteExisting)
            {
                // Set maxHealth from template
                var maxHealthField = healthManager.GetType().GetField("maxHealth");
                if (maxHealthField != null)
                {
                    maxHealthField.SetValue(healthManager, enemy.enemyTemplate.maxHealth);
                    Debug.Log($"[AutoSetup] HealthManager.maxHealth set to {enemy.enemyTemplate.maxHealth} for {enemy.gameObject.name}.");
                }
                else
                {
                    var maxHealthProp = healthManager.GetType().GetProperty("maxHealth");
                    if (maxHealthProp != null && maxHealthProp.CanWrite)
                    {
                        maxHealthProp.SetValue(healthManager, enemy.enemyTemplate.maxHealth);
                        Debug.Log($"[AutoSetup] HealthManager.maxHealth property set to {enemy.enemyTemplate.maxHealth} for {enemy.gameObject.name}.");
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoSetup] Could not set maxHealth on HealthManager for {enemy.gameObject.name} (no field or writable property found).");
                    }
                }
                UnityEditor.EditorUtility.SetDirty(healthManager);
            }
        }

        private static void SetupLookAtIK(UndeadSurvivalGame.Enemy.Enemy enemy, bool overwriteExisting = true)
        {
            if (enemy == null)
                return;

            // Try to get or add LookAtIKComponent
            var lookAtIK = enemy.GetComponent<LookAtIK>();
            bool added = false;
            if (lookAtIK == null)
            {
                lookAtIK = enemy.gameObject.AddComponent<LookAtIK>();
                added = true;
                Debug.Log($"[AutoSetup] LookAtIKComponent added to {enemy.gameObject.name}.");
            }

            lookAtIK.enabled = false; // Disable it initially
            lookAtIK.solver.headWeight = .8f;

            // Only set the head bone if just added or overwriteExisting is true
            if (added || overwriteExisting)
            {
                var animator = enemy.GetComponent<Animator>();
                if (animator != null && animator.avatar != null && animator.isHuman)
                {
                    // For humanoid rigs, use HumanBodyBones.Head
                    var headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
                    // Find the root bone by name 'root' (case-insensitive)
                    Transform rootTransform = null;
                    var transforms = enemy.GetComponentsInChildren<Transform>(true);
                    foreach (var t in transforms)
                    {
                        if (t.name.Equals("root", System.StringComparison.OrdinalIgnoreCase))
                        {
                            rootTransform = t;
                            break;
                        }
                    }
                    if (headTransform != null)
                    {
                        lookAtIK.solver.SetChain(null, headTransform, null, rootTransform);
                        Debug.Log($"[AutoSetup] LookAtIKComponent.Head set to humanoid head bone and root set to '{(rootTransform != null ? rootTransform.name : "null")}' for {enemy.gameObject.name}.");
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoSetup] Could not find humanoid head bone for {enemy.gameObject.name}.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] Animator is missing or not humanoid for {enemy.gameObject.name}. LookAtIK setup skipped.");
                }
                EditorUtility.SetDirty(lookAtIK);
            }
        }

        private static void SetupEnemyDebugger(UndeadSurvivalGame.Enemy.Enemy enemy, bool overwriteExisting = true)
        {
            if (enemy == null)
                return;

            var debugger = enemy.GetComponent<EnemyDebugger>();
            if (debugger == null)
            {
                debugger = enemy.gameObject.AddComponent<EnemyDebugger>();
                Debug.Log($"[AutoSetup] EnemyDebugger component added to {enemy.gameObject.name}.");
                EditorUtility.SetDirty(enemy.gameObject);
            }
            // Set the enemy reference on the EnemyDebugger component (try public and non-public fields/properties)
            var type = debugger.GetType();
            var enemyField = type.GetField("enemy", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (enemyField != null)
            {
                enemyField.SetValue(debugger, enemy);
            }
            else
            {
                var enemyProp = type.GetProperty("enemy", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (enemyProp != null && enemyProp.CanWrite)
                {
                    enemyProp.SetValue(debugger, enemy);
                }
                else
                {
                    Debug.LogWarning($"[AutoSetup] Could not set enemy reference on EnemyDebugger for {enemy.gameObject.name} (no field or writable property found, tried all visibilities).");
                }
            }
        }

        private static void SetupBipedRagdollCreator(UndeadSurvivalGame.Enemy.Enemy enemy, bool overwriteExisting = true)
        {
            if (enemy == null) return;

            // Check for RagdollEditor component to determine ragdoll status
            var ragdollEditor = enemy.GetComponent<RagdollEditor>();
            var bipedRagdollCreator = enemy.GetComponent<BipedRagdollCreator>();
            if (!overwriteExisting && ragdollEditor != null)
            {
                Debug.Log($"[AutoSetup] Skipping BipedRagdollCreator: RagdollEditor component found and overwriteExisting is false.");
                return;
            }

            if (bipedRagdollCreator == null)
            {
                bipedRagdollCreator = enemy.gameObject.AddComponent<BipedRagdollCreator>();
                Debug.Log($"[AutoSetup] BipedRagdollCreator component added to {enemy.gameObject.name}.");
            }
            else
            {
                Debug.Log($"[AutoSetup] BipedRagdollCreator component already exists on {enemy.gameObject.name}.");
            }

            // Mark as dirty for persistence
            EditorUtility.SetDirty(bipedRagdollCreator);
            PrefabUtility.RecordPrefabInstancePropertyModifications(bipedRagdollCreator);
        }

        private static void SetLayerRecursively(GameObject obj, int layer, bool overwriteExisting = true)
        {
            if (obj == null) return;
            if (!overwriteExisting) return;

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

        private static void SetupLimbs(UndeadSurvivalGame.Enemy.Enemy enemy, bool overwriteExisting = true)
        {
            if (enemy == null || enemy.transform.parent == null)
                return;

            var parent = enemy.transform.parent;
            foreach (Transform sibling in parent)
            {
                if (sibling == enemy.transform)
                    continue;
                var puppetMaster = sibling.GetComponent<PuppetMaster>();
                if (puppetMaster != null)
                {
                    Debug.Log($"[SetupLimbs] Found sibling GameObject '{sibling.gameObject.name}' with PuppetMaster component for Enemy '{enemy.gameObject.name}'.");
                    // Find child named 'root' and log its children's names
                    Transform rootChild = null;
                    foreach (Transform child in sibling)
                    {
                        if (child.name.Equals("root.x", System.StringComparison.OrdinalIgnoreCase))
                        {
                            rootChild = child;
                            break;
                        }
                    }
                    if (rootChild != null)
                    {
                        AddLimbsRecursive(rootChild, $"{sibling.gameObject.name}");
                    }
                    else
                    {
                        Debug.Log($"[SetupLimbs] No child named 'root.x' found under '{sibling.gameObject.name}'.");
                    }
                }
                else
                {
                    // Suppress log for known non-PuppetMaster siblings like 'Behaviours'
                    if (!sibling.gameObject.name.Equals("Behaviours", System.StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogWarning($"[SetupLimbs] Sibling '{sibling.gameObject.name}' does not have PuppetMaster component, skipping. Did you setup PuppetMaster yet?");
                    }
                }
            }
        }



        // Recursively add all limbs to children from a given Transform
        private static void AddLimbsRecursive(Transform parent, string parentPath, bool overwriteExisting = true)
        {
            if (boneLimbTypeMap.ContainsKey(parent.name))
            {
                var limbComponent = GetOrAddLimbComponent(parent, parentPath);
                bool wasAdded = limbComponent != null && limbComponent.ragdollRigidbody == null && limbComponent.ragdollCollider == null;
                if (overwriteExisting || wasAdded)
                {
                    AssignRigidbodyToLimb(parent, limbComponent, parentPath);
                    AssignColliderToLimb(parent, limbComponent, parentPath);
                    AssignLimbTypeAndTemplate(parent, limbComponent, parentPath);
                    AssignHealthManagerToLimb(parent, limbComponent, parentPath);
                }
            }
            foreach (Transform child in parent)
            {
                AddLimbsRecursive(child, $"{parentPath}/{child.gameObject.name}");
            }
        }

        private static Limb GetOrAddLimbComponent(Transform parent, string parentPath)
        {
            var limbComponent = parent.GetComponent<Limb>();
            if (limbComponent != null)
            {
                Debug.Log($"[SetupLimbs] Limb component found on '{parent.name}' under {parentPath}.");
            }
            else
            {
                limbComponent = parent.gameObject.AddComponent<Limb>();
                Debug.Log($"[SetupLimbs] Limb component ADDED to '{parent.name}' under {parentPath}.");
            }
            return limbComponent;
        }

        private static void AssignRigidbodyToLimb(Transform parent, Limb limbComponent, string parentPath)
        {
            var rb = parent.GetComponent<Rigidbody>();
            if (rb != null)
            {
                limbComponent.ragdollRigidbody = rb;
                // If this is an arm bone, set collision detection mode to ContinuousSpeculative
                if (parent.name.Equals("arm_stretch.l", System.StringComparison.OrdinalIgnoreCase) ||
                    parent.name.Equals("forearm_stretch.l", System.StringComparison.OrdinalIgnoreCase) ||
                    parent.name.Equals("hand.l", System.StringComparison.OrdinalIgnoreCase) ||
                    parent.name.Equals("arm_stretch.r", System.StringComparison.OrdinalIgnoreCase) ||
                    parent.name.Equals("forearm_stretch.r", System.StringComparison.OrdinalIgnoreCase) ||
                    parent.name.Equals("hand.r", System.StringComparison.OrdinalIgnoreCase))
                {
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    Debug.Log($"[SetupLimbs] Set CollisionDetectionMode.ContinuousSpeculative for arm bone '{parent.name}' under {parentPath}.");
                }
                Debug.Log($"[SetupLimbs] Rigidbody found and assigned to Limb on '{parent.name}' under {parentPath}.");
            }
            else
            {
                Debug.LogWarning($"[SetupLimbs] No Rigidbody found on '{parent.name}' under {parentPath}. Limb.ragdollRigidbody not set.");
            }
        }

        private static void AssignColliderToLimb(Transform parent, Limb limbComponent, string parentPath)
        {
            Collider foundCollider = null;
            if (parent.name.Equals("foot.l") || parent.name.Equals("foot.r"))
            {
                foreach (Transform child in parent)
                {
                    var col = child.GetComponent<Collider>();
                    if (col != null)
                    {
                        foundCollider = col;
                        Debug.Log($"[SetupLimbs] Collider found on child '{child.name}' of foot '{parent.name}' under {parentPath}.");
                        break;
                    }
                }
                if (foundCollider == null)
                {
                    Debug.LogWarning($"[SetupLimbs] No collider found on any child of foot '{parent.name}' under {parentPath}.");
                }
            }
            else
            {
                foundCollider = parent.GetComponent<Collider>();
                if (foundCollider != null)
                {
                    Debug.Log($"[SetupLimbs] Collider found on '{parent.name}' under {parentPath}.");
                }
                else
                {
                    Debug.LogWarning($"[SetupLimbs] No collider found on '{parent.name}' under {parentPath}.");
                }
            }
            limbComponent.ragdollCollider = foundCollider;
        }

        private static void AssignLimbTypeAndTemplate(Transform parent, Limb limbComponent, string parentPath)
        {
            limbComponent.LimbType = boneLimbTypeMap[parent.name];
            limbComponent.Template = boneLimbTemplateMap[parent.name];
            Debug.Log($"[SetupLimbs] LimbType set to '{boneLimbTypeMap[parent.name]}' for '{parent.name}' under {parentPath}.");
        }

        private static void AssignHealthManagerToLimb(Transform parent, Limb limbComponent, string parentPath)
        {
            if (parent == null || limbComponent == null)
                return;

            var healthManager = parent.GetComponent<HealthManager>();
            if (healthManager == null)
            {
                healthManager = parent.gameObject.AddComponent<HealthManager>();
                Debug.Log($"[SetupLimbs] HealthManager component ADDED to '{parent.name}' under {parentPath}.");
            }
            else
            {
                Debug.Log($"[SetupLimbs] HealthManager component already exists on '{parent.name}' under {parentPath}.");
            }
            // Optionally, you can link the limb to the healthManager if needed
        }
    }
}