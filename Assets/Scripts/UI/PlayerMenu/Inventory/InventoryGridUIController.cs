using System.Collections.Generic;
using TMPro;
using UndeadSurvivalGame.Gameplay;
using UndeadSurvivalGame.PlayerSystems;
using UnityEngine;
using UnityEngine.UI;

namespace UndeadSurvivalGame.UI
{
    public class InventoryGridUIController : MonoBehaviour
    {
        public InventorySlotUI SelectedSlot { get; private set; }
        public InventorySlotUI CurrentFocusedSlot { get; private set; }

        [SerializeField] private Inventory Inventory;

        [SerializeField] private List<InventorySlotUI> InventorySlots;

        [SerializeField] private SelectedItemNameUI SelectedItemNameUI;

        [SerializeField] private SelectedItemTypeUI SelectedItemTypeUI;

        [SerializeField] private SelectedItemDescriptionUI SelectedItemDescriptionUI;

        [SerializeField] private PlayerWeaponManager weaponManager;

        [SerializeField] private ContextMenuController contextMenuController;

        [SerializeField] private GridLayoutGroup InventoryGridLayoutGroup;

        void Awake()
        {
            SetupPlayerWeaponManager();
            SetupInventorySlots();
            SetupSelectedItemNameUI();
            SetupSelectedItemTypeUI();
            SetupSelectedItemDescriptionUI();
            SetupContextMenuController();
            SetupInventoryGridLayoutGroup();
        }

        private void SetupPlayerWeaponManager()
        {
            if (weaponManager == null)
            {
                Player player = FindFirstObjectByType<Player>();

                if (player != null)
                {
                    weaponManager = player.GetComponent<PlayerWeaponManager>();

                    if (weaponManager == null)
                    {
                        Debug.LogWarning($"[{gameObject.name}] SetupPlayerWeaponManager(): PlayerWeaponManager component not found on Player.");
                    }
                }
            }
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

        private void SetupContextMenuController()
        {
            if (contextMenuController == null)
            {
                contextMenuController = transform.Find("ContextMenu").GetComponent<ContextMenuController>();

                if (contextMenuController == null)
                {
                    Debug.LogWarning("InventoryGridUIController requires a ContextMenu in the parent.");
                }
            }
        }

        private void SetupInventoryGridLayoutGroup()
        {
            if (InventoryGridLayoutGroup == null)
            {
                InventoryGridLayoutGroup = GetComponent<GridLayoutGroup>();

                if (InventoryGridLayoutGroup == null)
                {
                    Debug.LogWarning("InventoryGridUIController requires a GridLayoutGroup component on the same GameObject.");
                }
            }
        }

        void OnEnable()
        {
            Inventory.OnInventoryChanged += RefreshGrid;
            RefreshGrid();
            FocusFirstItem();
        }

        void OnDisable()
        {
            Inventory.OnInventoryChanged -= RefreshGrid;
            CurrentFocusedSlot = null;
            SelectedSlot = null;
        }

        private void FocusFirstItem()
        {
            if (InventorySlots.Count > 0)
            {
                FocusSlot(InventorySlots[0]);
            }
            else
            {
                Debug.LogWarning("No inventory slots available to display item info.");
            }
        }

        public void SelectSlot(InventorySlotUI selectedSlot)
        {
            foreach (var inventorySlot in InventorySlots)
            {
                inventorySlot.SetSelected(inventorySlot == selectedSlot);
            }

            SelectedSlot = selectedSlot;


            // if (ContextMenu != null)
            // {
            //     PositionContextMenuUtility.ShowAtNextCellRight(
            //         ContextMenu,
            //         SelectedSlot.GetComponent<RectTransform>(),
            //         InventoryGridLayoutGroup,
            //         padX: 0f, padY: 0f
            //     );
            //     ContextMenu.gameObject.SetActive(true);
            // }

            if (contextMenuController != null && SelectedSlot != null)
            {
                int nextIndex = SelectedSlot.GetIndex() + 1;
                InventorySlotUI nextSlot = InventorySlots[nextIndex];
                contextMenuController.ShowAtAttachPoint(nextSlot.ContextMenuAttachPoint);
            }
        }

        public void FocusSlot(InventorySlotUI slot)
        {
            if (CurrentFocusedSlot != null && CurrentFocusedSlot != slot)
            {
                CurrentFocusedSlot.StopFadingAlphaHoverBackground();
            }

            int index = slot.GetIndex();

            if (index < Inventory.ItemStacks.Count)
            {
                if (CurrentFocusedSlot != null && CurrentFocusedSlot != slot)
                {
                    CurrentFocusedSlot.ClickFeedback.PlayFeedbacks();
                }

                ItemStack itemStack = Inventory.ItemStacks[index];
                SetSelectedItemName(itemStack.item.itemName);
                ToggleEquippedText(itemStack.item);
                SetSelectedItemType(itemStack.item.itemType.ToString());
                SetSelectedItemDescription(itemStack.item.description);
            }

            slot.FadeAlphaHoverBackground();
            CurrentFocusedSlot = slot;
        }

        private void SetSelectedItemName(string itemName)
        {
            if (SelectedItemNameUI != null)
            {
                SelectedItemNameUI.SetItemName(itemName);
            }
        }

        private void ToggleEquippedText(Item item)
        {
            if (SelectedItemNameUI != null && weaponManager != null)
            {
                bool isEquipped = weaponManager.IsItemEquipped(item);
                SelectedItemNameUI.ToggleEquippedText(isEquipped);
            }
        }

        private void DisplayEquippedText(bool isEquipped)
        {
            if (SelectedItemNameUI != null && SelectedItemNameUI.equippedText != null)
            {
                SelectedItemNameUI.equippedText.SetActive(isEquipped);
            }
        }

        private void SetSelectedItemType(string itemType)
        {
            if (SelectedItemTypeUI != null)
            {
                SelectedItemTypeUI.SetItemType(itemType.ToString());
            }
        }

        private void SetSelectedItemDescription(string itemDescription)
        {
            if (SelectedItemDescriptionUI != null)
            {
                SelectedItemDescriptionUI.SetItemDescription(itemDescription);
            }
        }

        /// <summary>
        /// Update slot visuals based on Inventory data.
        /// </summary>
        private void RefreshGrid()
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
                        slot.BottomRightCornerBackground.SetActive(true);
                        slot.ItemCount.SetActive(true);
                        slot.ItemCount.GetComponent<TextMeshProUGUI>().text = itemStack.quantity.ToString();
                    }
                    else
                    {
                        slot.BottomRightCornerBackground.SetActive(false);
                        slot.ItemCount.SetActive(false);
                    }

                    // Show equipped icon if the item is equipped
                    bool isEquipped = weaponManager != null && weaponManager.CurrentWeaponItem != null && itemStack.item == weaponManager.CurrentWeaponItem;
                    slot.DisplayEquippedIcon(isEquipped);
                }
                else
                {
                    slot.BottomRightCornerBackground.SetActive(false);
                    slot.ItemIcon.SetActive(false);
                    slot.ItemCount.SetActive(false);
                    slot.SetEmpty(true);
                    slot.DisplayEquippedIcon(false);
                }
            }
        }

        public void DropSelectedItem()
        {
            if (SelectedSlot == null)
            {
                Debug.LogWarning("No slot is selected to drop an item from.");
                return;
            }

            int selectedIndex = SelectedSlot.GetIndex();

            if (selectedIndex < 0 || selectedIndex >= Inventory.ItemStacks.Count)
            {
                Debug.LogWarning("Selected slot index is out of range of the inventory item stacks.");
                return;
            }

            ItemStack selectedItemStack = Inventory.ItemStacks[selectedIndex];

            if (selectedItemStack == null)
            {
                Debug.LogWarning("Selected slot does not contain a valid item stack to drop.");
                return;
            }

            Inventory.DropItemStack(selectedItemStack);
        }

        public bool IsItemEquipped(Item item)
        {
            return false;
        }
        
        public List<InventorySlotUI> GetInventorySlots()
        {
            return InventorySlots;
        }
    }
}
