using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    public ItemStack itemStack;

    public void ShowPrompt()
    {
        // Implement prompt display logic here
        Debug.Log($"[{gameObject.name}] ItemPickup.ShowPrompt(): Press 'F' to pick up {itemStack.item.name}");
    }

    public void HidePrompt()
    {
        // Implement prompt hide logic here
        Debug.Log($"[{gameObject.name}] ItemPickup.HidePrompt()");
    }
}
