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

    public void AddForce(Vector3 direction, float force)
    {
        if (ragdollRigidbody != null)
        {
            ragdollRigidbody.AddForce(direction * force, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Ragdoll Rigidbody is not assigned on " + name);
        }
    }
}
