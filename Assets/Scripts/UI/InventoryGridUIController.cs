using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryGridUIController : MonoBehaviour
{
    public Inventory inventory;
    public List<InventorySlot> inventorySlots;

    void OnEnable()
    {
        inventory.OnInventoryChanged += RefreshGrid;
        RefreshGrid();
    }

    void OnDisable()
    {
        inventory.OnInventoryChanged -= RefreshGrid;
    }

    void RefreshGrid()
    {
        Debug.Log("Refreshing inventory grid UI...");

        for (int index = 0; index < inventory.ItemStacks.Count; index++)
        {
            InventorySlot slot = inventorySlots[index];
            ItemStack itemStack = inventory.ItemStacks[index];

            // Display icon
            slot.ItemIcon.SetActive(true);
            slot.ItemIcon.GetComponent<Image>().sprite = itemStack.item.itemIcon;

            // Display count if stackable
            if (itemStack.item.isStackable)
            {
                slot.ItemCount.SetActive(true);
                slot.ItemCount.GetComponent<TextMeshProUGUI>().text = itemStack.quantity.ToString();
            }
        }
    }
}
