using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    int capacity = 15;

    [SerializeField]
    public List<ItemStack> itemStacks = new();

    public event Action OnInventoryChanged;

    public IReadOnlyList<ItemStack> ItemStacks => itemStacks;

    /// <summary>
    /// Attempts to add the specified quantity of an item to the inventory.
    /// Returns the number of items that could not be added (remaining quantity).
    /// 
    /// Parameters:
    /// - item: The Item to add.
    /// - quantity: The number of items to add (default is 1).
    /// 
    /// Return:
    /// - int: The quantity that could not be added.
    ///  0 means all items were added, positive means some remaining items were not added, 
    /// and negative means there was an error in item or quantity was invalid.
    /// </summary>
    public int TryAdd(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
        {
            Debug.LogWarning("Invalid item or quantity.");
            return quantity;
        }

        if (item.isStackable)
        {
            quantity = AddStackableItem(item, quantity);
        }
        else
        {
            quantity = AddNonStackableItem(item, quantity);
        }

        return quantity;
    }

    /// <summary>
    /// Adds stackable items to existing stacks or creates new stacks.
    /// Returns the remaining quantity that could not be added.
    /// </summary>
    private int AddStackableItem(Item item, int quantity)
    {
        // Add to existing stacks
        foreach (var itemStack in itemStacks)
        {
            if (itemStack.item.itemID == item.itemID)
            {
                int spaceLeft = item.maxStack - itemStack.quantity;
                if (spaceLeft > 0)
                {
                    int amountToAdd = Math.Min(spaceLeft, quantity);
                    itemStack.AddQuantity(amountToAdd);
                    quantity -= amountToAdd;
                    OnInventoryChanged?.Invoke();
                    if (quantity <= 0) break;
                }
            }
        }

        // Add new stacks if there is still quantity left and there is capacity
        while (quantity > 0 && itemStacks.Count < capacity)
        {
            int amountToAdd = Math.Min(quantity, item.maxStack);
            itemStacks.Add(new ItemStack(item, amountToAdd));
            quantity -= amountToAdd;
            OnInventoryChanged?.Invoke();
        }

        if (quantity > 0)
        {
            Debug.Log("Inventory is full. Some items could not be added.");
        }

        return quantity;
    }

    /// <summary>
    /// Adds non-stackable items to the inventory.
    /// Returns the remaining quantity that could not be added.
    /// </summary>
    private int AddNonStackableItem(Item item, int quantity)
    {
        while (quantity > 0 && itemStacks.Count < capacity)
        {
            itemStacks.Add(new ItemStack(item, 1));
            quantity--;
            OnInventoryChanged?.Invoke();
        }

        if (quantity > 0)
        {
            Debug.Log("Inventory is full. Some items could not be added.");
        }

        return quantity;
    }
}
