using UnityEngine;

public class PlayerInteractionSensor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnChildTriggerEnter(ChildInteractionTrigger trigger, Collider other)
    {
        Debug.Log($"[{name}] triggerType: {trigger.triggerType} detected interactable: {other.gameObject.name}");

        if (trigger.triggerType == TriggerType.Arrow)
        {
            // Show arrow UI
            Debug.Log($"[{name}] Show Arrow UI");
            ProximityUI proximityUI = other.GetComponent<ProximityUI>();

            if (proximityUI != null)
            {
                proximityUI.EnableArrow();
            }
            else
            {
                Debug.LogWarning($"[{name}] No ProximityUI component found on {other.gameObject.name}");
            }
        }
    }
    
    public void OnChildTriggerExit(ChildInteractionTrigger trigger, Collider other)
    {
        Debug.Log($"[{name}] triggerType: {trigger.triggerType} lost interactable: {other.gameObject.name}");

        if (trigger.triggerType == TriggerType.Arrow)
        {
            // Hide arrow UI
            Debug.Log($"[{name}] Hide Arrow UI");
            ProximityUI proximityUI = other.GetComponent<ProximityUI>();

            if (proximityUI != null)
            {
                proximityUI.DisableArrow();
            }
            else
            {
                Debug.LogWarning($"[{name}] No ProximityUI component found on {other.gameObject.name}");
            }
        }
    }
}
