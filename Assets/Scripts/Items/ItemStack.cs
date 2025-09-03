using System;
using UnityEngine;

namespace UndeadSurvivalGame.Gameplay
{
    [Serializable]
    public class ItemStack
    {
        public Item item;
        public int quantity = 1;
        

        public ItemStack(Item item, int quantity = 1)
        {
            this.item = item;
            this.quantity = quantity;
        }

        public void AddQuantity(int amount)
        {
            quantity += amount;
        }

        public void RemoveQuantity(int amount)
        {
            quantity -= amount;
            if (quantity <= 0)
            {
                item = null; // Clear item if quantity is zero or less
            }
        }

        public bool IsEmpty => quantity == 0;

        public void DecrementQuantity()
        {
            quantity--;
            if (quantity <= 0)
            {
                Debug.Log($"[{GetType().Name}] DecrementQuantity(): Item '{item?.name}' is depleted.");
            }
        }
    }
}
