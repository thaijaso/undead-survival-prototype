using UnityEngine;

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

    // Collision detection for zombie attacks
    private void OnTriggerEnter(Collider other)
    {
        // Only process collisions for arm limbs during attacks
        if (LimbType == LimbType.UpperArm || LimbType == LimbType.LowerArm || LimbType == LimbType.Hand)
        {
            // Check if we hit the player's CharacterController
            CharacterController playerController = other.GetComponent<CharacterController>();
            if (playerController != null)
            {
                // Get the player component to confirm it's the player
                Player player = other.GetComponent<Player>();
                if (player != null)
                {
                    Debug.Log($"Zombie {LimbType} hit Player's CharacterController!");

                    // Optionally, we can also get the Enemy component to check if it's in attack state
                    Enemy enemy = GetComponentInParent<Enemy>();
                    if (enemy != null && enemy.stateMachine.currentState == enemy.Attack)
                    {
                        Debug.Log($"Confirmed: Zombie attack hit detected during Attack state!");
                        OnZombieAttackHit(player, enemy);
                    }
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Alternative collision detection using OnCollisionEnter if using solid colliders
        if (LimbType == LimbType.UpperArm || LimbType == LimbType.LowerArm || LimbType == LimbType.Hand)
        {
            CharacterController playerController = collision.gameObject.GetComponent<CharacterController>();
            if (playerController != null)
            {
                Player player = collision.gameObject.GetComponent<Player>();
                if (player != null)
                {
                    Debug.Log($"Zombie {LimbType} collided with Player's CharacterController!");

                    Enemy enemy = GetComponentInParent<Enemy>();
                    if (enemy != null && enemy.stateMachine.currentState == enemy.Attack)
                    {
                        Debug.Log($"Confirmed: Zombie attack collision detected during Attack state!");
                        OnZombieAttackHit(player, enemy);
                    }
                }
            }
        }
    }

    private void OnZombieAttackHit(Player player, Enemy enemy)
    {
        // This method can be expanded to handle the actual attack logic
        Debug.Log($"Processing zombie attack hit from {enemy.name}'s {LimbType} on {player.name}");

        // Here you could:
        // - Deal damage to the player
        // - Apply knockback
        // - Trigger attack animations/effects
        // - Play sound effects
        // - Update attack state machine

        // Example damage dealing (uncomment if you want to implement):
        // player.HealthManager.TakeDamage(10); // Adjust damage as needed
    }
}
