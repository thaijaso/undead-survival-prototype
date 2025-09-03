using UnityEngine;

namespace UndeadSurvivalGame.Gameplay
{
    public class InventoryBootstrap : MonoBehaviour
    {
        public Inventory inventory;
        public InventoryPreset preset;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (inventory != null && preset != null)
            {
                foreach (var itemStack in preset.startingItems)
                {
                    inventory.TryAdd(itemStack.item, itemStack.quantity);
                }
            }
        }
    }
}
