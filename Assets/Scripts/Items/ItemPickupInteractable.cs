using UndeadSurvivalGame.Player;
using UnityEngine;

public class ItemPickupInteractable : MonoBehaviour, IInteractable
{
    public ItemStack itemStack;

    private void Awake()
    {
        if (itemStack == null || itemStack.item == null)
        {
            Debug.LogError($"ItemPickupInteractable.Awake(): {name} has no ItemStack or Item assigned.");
            return;
        }

        ProximityUI proximityUI = GetComponent<ProximityUI>();

        if (proximityUI == null)
        {
            Debug.LogError($"ItemPickupInteractable.Awake(): {name} has no ProximityUI component.");
            return;
        }

        proximityUI.SetText($"Pickup {itemStack.item.itemName} x{itemStack.quantity}");
    }

    public void Interact(Player player)
    {
        if (player.PlayerInventory != null)
        {
            int remaining = player.PlayerInventory.TryAdd(itemStack.item, itemStack.quantity);

            if (remaining == 0)
            {
                Debug.Log($"ItemPickupInteractable.Interact(): {name} picked up {itemStack}");
                ProximityUI proximityUI = GetComponent<ProximityUI>();
                player.InteractionSensor.RemoveProximityUIRefs(proximityUI);
                Destroy(gameObject);
            }
            else
            {
                Debug.Log($"ItemPickupInteractable.Interact(): {name} could not pick up {itemStack}. Inventory full.");
                itemStack.quantity = remaining;
                // TODO: invoke event for ui to update
            }
        }
    }
}
