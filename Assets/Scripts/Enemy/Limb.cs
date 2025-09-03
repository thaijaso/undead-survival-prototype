using RootMotion.Dynamics;
using UndeadSurvivalGame.PlayerSystems;
using UndeadSurvivalGame.Gameplay;
using UnityEngine;

namespace UndeadSurvivalGame.EnemySystems
{
    public enum LimbType
    {
        Head,
        Torso,
        Stomach,
        UpperArm,
        LowerArm,
        UpperLeg,
        LowerLeg,
        Foot,
        Hand
    }

    public class Limb : MonoBehaviour
    {
        [Header("Dismemberment (Only needed if limb is dismemerable)")]
        public LimbType LimbType;

        [Tooltip("Prefab to spawn when this limb is dismembered. If not set, the limb will not be dismembered.")]
        public GameObject Prefab;

        [Tooltip("Instance of the limb prefab in the scene. Used to hide the limb when dismembered.")]
        public GameObject Instance;

        [Tooltip("Transform where the prefab will be spawned when dismembered. Usually the bone transform of the limb.")]
        public Transform Bone; // Where to spawn the prefab

        [Header("Needed for damage proccessing")]
        public int Health = 100;

        [Header("Ragdoll")]
        public Rigidbody ragdollRigidbody;

        public Collider ragdollCollider;

        public LimbTemplate Template;

        private HealthManager HealthManager;

        void Awake()
        {
            if (HealthManager == null)
                HealthManager = GetComponent<HealthManager>();
        }

        void Start()
        {
            // Initialize HealthManager with template data if available
            if (HealthManager != null && Template != null)
            {
                HealthManager.Initialize(Template.maxHealth);
                Debug.Log($"[{gameObject.name}] Limb HealthManager initialized with template maxHealth: {Template.maxHealth}");
            }
            else if (HealthManager != null)
            {
                // Fallback to the Health field if no template is available
                HealthManager.Initialize(Health);
                Debug.Log($"[{gameObject.name}] Limb HealthManager initialized with fallback Health: {Health}");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Limb has no HealthManager component!");
            }
        }

        public void TakeDamage(int damage)
        {
            if (HealthManager == null)
            {
                Debug.LogError("HealthManager is not assigned on " + name);
                return;
            }

            HealthManager.TakeDamage(damage);
            Debug.Log($"{name} took {damage} damage. Remaining health: {HealthManager.currentHealth}");

            if (Template.canBeDismembered && HealthManager.currentHealth <= 0)
            {
                Dismember();
            }
        }

        public void Dismember()
        {
            // Hide the body part instance
            if (Instance != null)
            {
                Instance.SetActive(false);
                GetComponent<Collider>().enabled = false;
            }

            // Spawn the severed limb prefab at the hit position
            if (Prefab != null)
            {
                Debug.Log("Spawning severed limb at: " + Bone.position);

                GameObject limb = Instantiate(
                    Prefab,
                    Bone.position,
                    Bone.rotation
                );
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsPlayerRagdollCollision(collision))
                return;

            if (LimbType == LimbType.Hand)
                TryProcessHandCollision(collision);
        }

        private bool IsPlayerRagdollCollision(Collision collision)
        {
            return collision.gameObject.layer == LayerMask.NameToLayer("PlayerRagdoll");
        }

        private void TryProcessHandCollision(Collision collision)
        {
            Debug.Log($"{LimbType} limb collided with {collision.gameObject.name}");

            PuppetMaster playerPuppetMaster = GetPlayerPuppetMaster(collision);
            if (playerPuppetMaster == null) return;

            Player player = GetPlayerFromPuppetMaster(playerPuppetMaster, collision);
            if (player == null) return;

            PuppetMaster enemyPuppetMaster = GetComponentInParent<PuppetMaster>();
            if (enemyPuppetMaster == null)
            {
                Debug.Log($"Limb.OnCollisionEnter(): No PuppetMaster found on {gameObject.name}");
                return;
            }

            Debug.Log($"Limb.OnCollisionEnter(): Enemy PuppetMaster found on {gameObject.name}");
            Enemy enemy = enemyPuppetMaster.targetRoot.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.Log($"Limb.OnCollisionEnter(): No Enemy component found on PuppetMaster for {gameObject.name}");
                return;
            }

            Debug.Log($"Limb.OnCollisionEnter(): Player state: {player.stateMachine.currentState}, Enemy state: {enemy.stateMachine.currentState}");

            if (ShouldTriggerHitReaction(player, enemy, out var hitReaction))
            {
                Debug.Log($"Limb.OnCollisionEnter(): Triggering hit reaction for player {player.name} from enemy {enemy.name}");
                hitReaction.OnHandCollided();
            }
            else
            {
                Debug.Log($"Limb.OnCollisionEnter(): No hit reaction triggered for player {player.name} from enemy {enemy.name}");
            }
        }

        private bool ShouldTriggerHitReaction(Player player, Enemy enemy, out PlayerSystems.HitReactionState hitReaction)
        {
            hitReaction = (PlayerSystems.HitReactionState)player.hitReaction;
            if (
                enemy.stateMachine.currentState is AttackState
                && player.stateMachine.currentState is not PlayerSystems.HitReactionState
                && player.stateMachine.currentState is not PlayerSystems.DeathState
            )
            {
                return true;
            }

            hitReaction = null;
            return false;
        }

        private PuppetMaster GetPlayerPuppetMaster(Collision collision)
        {
            PuppetMaster puppetMaster = collision.gameObject.GetComponentInParent<PuppetMaster>();
            if (puppetMaster == null)
            {
                Debug.Log($"Limb.OnCollisionEnter(): No PuppetMaster found on {collision.gameObject.name}");
                return null;
            }
            Debug.Log($"Limb.OnCollisionEnter(): PuppetMaster found on collision: {collision.gameObject.name}");
            return puppetMaster;
        }

        private Player GetPlayerFromPuppetMaster(PuppetMaster puppetMaster, Collision collision)
        {
            Player player = puppetMaster.targetRoot.GetComponent<Player>();
            if (player == null)
            {
                Debug.Log($"Limb.OnCollisionEnter(): No Player component found on PuppetMaster targetRoot for {collision.gameObject.name}");
                return null;
            }
            return player;
        }
    }
}
