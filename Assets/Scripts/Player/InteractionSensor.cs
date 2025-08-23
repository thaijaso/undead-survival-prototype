using UnityEngine;

public class InteractionSensor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            Debug.Log($"[{gameObject.name}] InteractionSensor.OnTriggerEnter(): Player is near an interactable object: " + other.name);

            IInteractable interactable = other.GetComponent<IInteractable>();
            interactable?.ShowPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            Debug.Log($"[{gameObject.name}] InteractionSensor.OnTriggerExit(): Player is no longer near an interactable object: " + other.name);

            IInteractable interactable = other.GetComponent<IInteractable>();
            interactable?.HidePrompt();
        }
    }
}
