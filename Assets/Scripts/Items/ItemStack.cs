using System;

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

    public bool IsEmpty => item == null;
}
