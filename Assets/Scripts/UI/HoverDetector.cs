using UnityEngine;
using UnityEngine.EventSystems;

public class HoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private Inventory Inventory;

    [SerializeField]
    private InventoryGridUIController InventoryGridUIController;

    [SerializeField]
    private SelectedItemNameUI SelectedItemNameUI;

    [SerializeField]
    private SelectedItemTypeUI SelectedItemTypeUI;

    [SerializeField]
    private SelectedItemDescriptionUI SelectedItemDescriptionUI;

    [SerializeField]
    private InventorySlotUI InventorySlotUI;

    void Awake()
    {
        SetupInventory();
        SetupInventoryGridUIController();
        SetupInventorySelectedItemNameUI();
        SetupSelectedItemTypeUI();
        SetupSelectedItemDescriptionUI();
        SetupInventorySlotUI();
    }

    private void SetupInventory()
    {
        if (Inventory == null)
        {
            Inventory = GetComponentInParent<Inventory>();
            if (Inventory == null)
            {
                Debug.LogWarning("HoverDetector requires an Inventory in the parent hierarchy.");
            }
        }
    }

    private void SetupInventoryGridUIController()
    {
        if (InventoryGridUIController == null)
        {
            InventoryGridUIController = GetComponentInParent<InventoryGridUIController>();
            if (InventoryGridUIController == null)
            {
                Debug.LogWarning("HoverDetector requires an InventoryGridUIController in the parent hierarchy.");
            }
        }
    }

    private void SetupInventorySelectedItemNameUI()
    {
        if (SelectedItemNameUI == null)
        {
            SelectedItemNameUI = transform.parent.parent.GetComponentInChildren<SelectedItemNameUI>();
            if (SelectedItemNameUI == null)
            {
                Debug.LogWarning("HoverDetector requires an InventorySelectedItemNameUI in the parent hierarchy.");
            }
        }
    }

    private void SetupSelectedItemTypeUI()
    {
        if (SelectedItemTypeUI == null)
        {
            SelectedItemTypeUI = transform.parent.parent.GetComponentInChildren<SelectedItemTypeUI>();
            if (SelectedItemTypeUI == null)
            {
                Debug.LogWarning("HoverDetector requires a SelectedItemTypeUI in the parent hierarchy.");
            }
        }
    }

    private void SetupSelectedItemDescriptionUI()
    {
        if (SelectedItemDescriptionUI == null)
        {
            SelectedItemDescriptionUI = transform.parent.parent.GetComponentInChildren<SelectedItemDescriptionUI>();
            if (SelectedItemDescriptionUI == null)
            {
                Debug.LogWarning("HoverDetector requires a SelectedItemDescriptionUI in the parent hierarchy.");
            }
        }
    }

    private void SetupInventorySlotUI()
    {
        if (InventorySlotUI == null)
        {
            InventorySlotUI = GetComponent<InventorySlotUI>();
            if (InventorySlotUI == null)
            {
                Debug.LogWarning("HoverDetector requires an InventorySlotUI on the same GameObject.");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[{gameObject.name}] HoverDetector.OnPointerEnter(): Pointer entered on {gameObject.name}");

        if (!InventorySlotUI.IsEmpty())
        {
            InventorySlotUI.HoverBackground.enabled = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[{gameObject.name}] HoverDetector.OnPointerExit(): Pointer exited from {gameObject.name}");
        InventorySlotUI inventorySlot = GetComponent<InventorySlotUI>();
        inventorySlot.HoverBackground.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[{gameObject.name}] HoverDetector.OnPointerClick(): Pointer clicked on {gameObject.name}");

        if (!InventorySlotUI.IsEmpty())
        {
            HandleSlotSelection();
            PlayClickFeedback();
        }
    }

    private void HandleSlotSelection()
    {
        InventorySlotUI inventorySlot = GetComponent<InventorySlotUI>();
        if (InventoryGridUIController != null && inventorySlot != null)
        {
            InventoryGridUIController.SelectSlot(inventorySlot);
            int selectedIndex = inventorySlot.GetIndex();
            Debug.Log($"Selected slot index: {selectedIndex}");

            string itemName = Inventory.itemStacks[selectedIndex].item.itemName;
            string itemType = Inventory.itemStacks[selectedIndex].item.itemType.ToString();
            string itemDesc = Inventory.itemStacks[selectedIndex].item.description;

            SelectedItemNameUI.SetItemName(itemName);
            SelectedItemTypeUI.SetItemType(itemType);
            SelectedItemDescriptionUI.SetItemDescription(itemDesc);
        }
        else
        {
            Debug.LogWarning("InventoryGridUIController or InventorySlotUI is not set.");
        }
    }

    private void PlayClickFeedback()
    {
        if (InventorySlotUI.ClickPlayerFeedback != null)
        {
            InventorySlotUI.ClickPlayerFeedback.PlayFeedbacks();
        }
        else
        {
            Debug.LogWarning("ClickPlayerFeedback is not set in InventorySlotUI.");
        }
    }
}
