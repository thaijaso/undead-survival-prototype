using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    public ItemStack itemStack;

    private void Awake()
    {
        if (itemStack == null || itemStack.item == null)
        {
            Debug.LogError($"ItemPickup.Awake(): {name} has no ItemStack or Item assigned.");
            return;
        }

        ProximityUI proximityUI = GetComponent<ProximityUI>();

        if (proximityUI == null)
        {
            Debug.LogError($"ItemPickup.Awake(): {name} has no ProximityUI component.");
            return;
        }

        proximityUI.SetText($"Pickup {itemStack.item.itemName} x{itemStack.quantity}");
    }

    public void Interact()
    {
        Debug.Log($"ItemPickup.Interact(): {name} picked up {itemStack}");
    }
}
