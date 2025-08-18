using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryGridUIController : MonoBehaviour
{
    public InventorySlotUI SelectedSlot { get; private set; }

    [SerializeField]
    private Inventory Inventory;

    [SerializeField]
    private List<InventorySlotUI> InventorySlots;

    [SerializeField]
    private SelectedItemNameUI SelectedItemNameUI;

    [SerializeField]
    private SelectedItemTypeUI SelectedItemTypeUI;

    [SerializeField]
    private SelectedItemDescriptionUI SelectedItemDescriptionUI;

    void Awake()
    {
        SetupInventorySlots();
        SetupSelectedItemNameUI();
        SetupSelectedItemTypeUI();
        SetupSelectedItemDescriptionUI();
    }

    private void SetupInventorySlots()
    {
        if (InventorySlots == null || InventorySlots.Count == 0)
        {
            InventorySlots = new List<InventorySlotUI>(GetComponentsInChildren<InventorySlotUI>());
        }

        if (InventorySlots.Count == 0)
        {
            Debug.LogWarning("No InventorySlots found in children.");
        }
    }

    private void SetupSelectedItemNameUI()
    {
        if (SelectedItemNameUI == null)
        {
            SelectedItemNameUI = transform.parent.GetComponentInChildren<SelectedItemNameUI>();
            if (SelectedItemNameUI == null)
            {
                Debug.LogWarning("InventoryGridUIController requires a SelectedItemNameUI in the children.");
            }
        }
    }

    private void SetupSelectedItemTypeUI()
    {
        if (SelectedItemTypeUI == null)
        {
            SelectedItemTypeUI = transform.parent.GetComponentInChildren<SelectedItemTypeUI>();
            if (SelectedItemTypeUI == null)
            {
                Debug.LogWarning("InventoryGridUIController requires a SelectedItemTypeUI in the children.");
            }
        }
    }

    private void SetupSelectedItemDescriptionUI()
    {
        if (SelectedItemDescriptionUI == null)
        {
            SelectedItemDescriptionUI = transform.parent.GetComponentInChildren<SelectedItemDescriptionUI>();
            if (SelectedItemDescriptionUI == null)
            {
                Debug.LogWarning("InventoryGridUIController requires a SelectedItemDescriptionUI in the children.");
            }
        }
    }

    void OnEnable()
    {
        Inventory.OnInventoryChanged += RefreshGrid;
        RefreshGrid();
    }

    void OnDisable()
    {
        Inventory.OnInventoryChanged -= RefreshGrid;
    }

    void Start()
    {
        if (InventorySlots.Count > 0)
        {
            SelectSlot(InventorySlots[0]);
            ItemStack firstItemStack = Inventory.ItemStacks[0];
            SetSelectedItemName(firstItemStack.item.itemName);
            SetSelectedItemType(firstItemStack.item.itemType);
        }
    }

    public void SelectSlot(InventorySlotUI selectedSlot)
    {
        foreach (var inventorySlot in InventorySlots)
        {
            inventorySlot.SetSelected(inventorySlot == selectedSlot);
        }

        SelectedSlot = selectedSlot;
    }

    private void SetSelectedItemName(string itemName)
    {
        if (SelectedSlot != null && SelectedItemNameUI != null)
        {
            SelectedItemNameUI.SetItemName(itemName);
        }
    }

    private void SetSelectedItemType(ItemType itemType)
    {
        if (SelectedSlot != null && SelectedItemTypeUI != null)
        {
            SelectedItemTypeUI.SetItemType(itemType.ToString());
        }
    }

    /// <summary>
    /// Update slot visuals based on Inventory data.
    /// </summary>
    void RefreshGrid()
    {
        Debug.Log("Refreshing inventory grid UI...");

        if (InventorySlots == null || InventorySlots.Count == 0)
        {
            Debug.LogWarning("InventorySlots reference is not set or is empty in InventoryGridUIController.");
            return;
        }

        if (Inventory.ItemStacks.Count > InventorySlots.Count)
        {
            Debug.LogWarning("Not enough InventorySlots for all ItemStacks. Some items will not be displayed.");
        }

        for (int index = 0; index < InventorySlots.Count; index++)
        {
            InventorySlotUI slot = InventorySlots[index];
            slot.SetIndex(index);

            if (Inventory != null && Inventory.ItemStacks != null && index < Inventory.ItemStacks.Count)
            {
                ItemStack itemStack = Inventory.ItemStacks[index];

                // Slot has item
                slot.SetEmpty(false);

                // Display icon
                slot.ItemIcon.SetActive(true);
                slot.ItemIcon.GetComponent<Image>().sprite = itemStack.item.itemIcon;

                // Display count if stackable
                if (itemStack.item.isStackable)
                {
                    slot.ItemCount.SetActive(true);
                    slot.ItemCount.GetComponent<TextMeshProUGUI>().text = itemStack.quantity.ToString();
                }
                else
                {
                    slot.ItemCountBackground.SetActive(false);
                    slot.ItemCount.SetActive(false);
                }
            }
            else
            {
                slot.ItemIcon.SetActive(false);
                slot.ItemCount.SetActive(false);
                slot.SetEmpty(true);
            }
        }
    }
}
