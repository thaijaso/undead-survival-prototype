using UnityEngine;

public class ChildInteractionTrigger : MonoBehaviour
{
    public TriggerType triggerType;

    public PlayerInteractionSensor parentSensor;

    void Awake()
    {
        SetupParentSensor();
        SetupCollider();
    }

    private void SetupParentSensor()
    {
        if (parentSensor == null)
        {
            parentSensor = GetComponentInParent<PlayerInteractionSensor>();
        }

        if (parentSensor == null)
        {
            Debug.LogError($"[{name}] Parent PlayerInteractionSensor not found!");
        }
    }

    private void SetupCollider()
    {
        SphereCollider collider = GetComponent<SphereCollider>();

        if (collider == null)
        {
            collider = gameObject.AddComponent<SphereCollider>();
        }

        collider.isTrigger = true;

        if (triggerType == TriggerType.Arrow)
        {
            collider.radius = 3f;
        }
        else if (triggerType == TriggerType.Button)
        {
            collider.radius = .5f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            // Show interaction UI
            Debug.Log($"[{name}] Detected interactable: {other.gameObject.name}");
            parentSensor.OnChildTriggerEnter(this, other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            // Hide interaction UI
            Debug.Log($"[{name}] Lost interactable: {other.gameObject.name}");
            parentSensor.OnChildTriggerExit(this, other);
        }
    }
}

public enum TriggerType
{
    Arrow,
    Button
}
