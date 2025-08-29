using System;
using MoreMountains.Tools;
using UndeadSurvivalGame.Player;
using UnityEngine;

public class ItemPickupInteractable : MonoBehaviour, IInteractable
{
    public ItemStack itemStack;

    public event Action<string, int> OnPickupAllFailed;
    public event Action OnInventoryFull;

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

        proximityUI.DisplayPickupPrompt(itemStack.item.itemName, itemStack.quantity);
    }

    public void Interact(Player player)
    {
        if (player.PlayerInventory != null)
        {
            int remaining = player.PlayerInventory.TryAdd(itemStack.item, itemStack.quantity);

            if (remaining == 0)
            {
                Debug.Log($"ItemPickupInteractable.Interact(): picked up {itemStack.item.itemName}");

                MMSoundManager.Instance.PlaySound(
                    itemStack.item.pickupAllSound,
                    MMSoundManager.MMSoundManagerTracks.Sfx,
                    transform.position
                );

                ProximityUI proximityUI = GetComponent<ProximityUI>();
                player.InteractionSensor.RemoveProximityUIRefs(proximityUI);
                Destroy(gameObject);
            }
            else if (remaining < itemStack.quantity)
            {
                Debug.Log($"ItemPickupInteractable.Interact(): {name} could not pick up entire {itemStack.item.itemName}. Inventory full.");

                MMSoundManager.Instance.PlaySound(
                    itemStack.item.pickupSomeSound,
                    MMSoundManager.MMSoundManagerTracks.Sfx,
                    transform.position
                );
                itemStack.quantity = remaining;
                OnPickupAllFailed?.Invoke(itemStack.item.itemName, itemStack.quantity);
            }
            else if (remaining == itemStack.quantity)
            {
                Debug.Log($"ItemPickupInteractable.Interact(): {name} could not pickup any of {itemStack.item.itemName}.");

                // Play error sound?
                OnInventoryFull?.Invoke();
            }
            else
            {
                Debug.LogError($"ItemPickupInteractable.Interact(): {name} encountered an error while trying to pick up {itemStack}.");
            }
        }
    }
}
