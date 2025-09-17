using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UndeadSurvivalGame.Gameplay;
using UndeadSurvivalGame.PlayerSystems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UndeadSurvivalGame.UI
{
    public class InventoryGridUIController : MonoBehaviour
    {
        public InventorySlotUI SelectedSlot { get; private set; }
        public InventorySlotUI CurrentFocusedSlot { get; private set; }

        [SerializeField]
        private Inventory inventory;

        [SerializeField]
        private List<InventorySlotUI> inventorySlots;

        [SerializeField]
        private SelectedItemNameUI selectedItemNameUI;

        [SerializeField]
        private SelectedItemTypeUI selectedItemTypeUI;

        [SerializeField]
        private SelectedItemDescriptionUI selectedItemDescriptionUI;

        [SerializeField]
        private PlayerWeaponManager weaponManager;

        [SerializeField]
        private ContextMenuController contextMenuController;

        [SerializeField]
        private GridLayoutGroup inventoryGridLayoutGroup;

        [SerializeField]
        private MMF_Player hoverSoundFeedback;

        [SerializeField]
        private MMF_Player selectSoundFeedback;

        void Start()
        {
            SetupPlayerWeaponManager();
            SetupInventorySlots();
            SetupSelectedItemNameUI();
            SetupSelectedItemTypeUI();
            SetupSelectedItemDescriptionUI();
            SetupContextMenuController();
            SetupInventoryGridLayoutGroup();

            inventory.OnInventoryChanged += RefreshGrid;
            RefreshGrid();
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
            if (inventorySlots == null || inventorySlots.Count == 0)
            {
                inventorySlots = new List<InventorySlotUI>(GetComponentsInChildren<InventorySlotUI>());
            }

            if (inventorySlots.Count == 0)
            {
                Debug.LogWarning("No InventorySlots found in children.");
            }

            // Subscribe to slot click events
            foreach (var slot in inventorySlots)
            {
                slot.InventorySlotEventHandler.OnPointerClickedSlot += HandleSlotSelection;
            }
        }

        private void HandleSlotSelection(InventorySlotUI clickedSlot, PointerEventData.InputButton button)
        {
            if (button != PointerEventData.InputButton.Left)
                return;

            // Prevent selecting if context menu is open (optional, depending on your UX)
            if (contextMenuController != null && contextMenuController.IsVisible)
                return;

            if (clickedSlot == null)
            {
                Debug.LogWarning("HandleSlotSelection: clickedSlot is null.");
                return;
            }

            SelectSlot(clickedSlot);
        }

        private void SelectSlot(InventorySlotUI slotToSelect)
        {
            if (slotToSelect == null)
            {
                Debug.LogWarning("SelectSlot: slotToSelect is null.");
                return;
            }

            PlaySelectSound();
            DeselectPreviousSlot(slotToSelect);
            StopPreviousFocusFade();

            SelectedSlot = slotToSelect;

            UpdateSelectedItemUI(slotToSelect);
            ShowContextMenuAtSlot(slotToSelect);
        }

        private void PlaySelectSound()
        {
            if (selectSoundFeedback != null)
            {
                selectSoundFeedback.PlayFeedbacks();
            }
            else
            {
                Debug.LogWarning("Select sound feedback is not assigned.");
            }
        }

        private void DeselectPreviousSlot(InventorySlotUI slotToSelect)
        {
            foreach (var slot in inventorySlots)
                slot.SetSelected(slot == slotToSelect);
        }

        private void StopPreviousFocusFade()
        {
            if (CurrentFocusedSlot != null)
                CurrentFocusedSlot.StopFadingAlphaHoverBackground();
        }

        private void SetupSelectedItemNameUI()
        {
            if (selectedItemNameUI == null)
            {
                selectedItemNameUI = transform.parent.GetComponentInChildren<SelectedItemNameUI>();
                if (selectedItemNameUI == null)
                {
                    Debug.LogWarning("InventoryGridUIController requires a SelectedItemNameUI in the children.");
                }
            }
        }

        private void SetupSelectedItemTypeUI()
        {
            if (selectedItemTypeUI == null)
            {
                selectedItemTypeUI = transform.parent.GetComponentInChildren<SelectedItemTypeUI>();
                if (selectedItemTypeUI == null)
                {
                    Debug.LogWarning("InventoryGridUIController requires a SelectedItemTypeUI in the children.");
                }
            }
        }

        private void UpdateSelectedItemUI(InventorySlotUI slotToSelect)
        {
            int selectedIndex = slotToSelect.GetIndex();
            if (selectedIndex >= 0 && selectedIndex < inventory.ItemStacks.Count && inventory.ItemStacks[selectedIndex] != null)
            {
                Item item = inventory.ItemStacks[selectedIndex].item;
                selectedItemNameUI.SetItemName(item.ItemName);
                selectedItemNameUI.ToggleEquippedText(weaponManager.IsItemEquipped(item));
                selectedItemTypeUI.SetItemType(item.ItemType.ToString());
                selectedItemDescriptionUI.SetItemDescription(item.Description);
            }
            else
            {
                selectedItemNameUI.SetItemName(string.Empty);
                selectedItemTypeUI.SetItemType(string.Empty);
                selectedItemDescriptionUI.SetItemDescription(string.Empty);
                selectedItemNameUI.ToggleEquippedText(false);
            }
        }

        private void ShowContextMenuAtSlot(InventorySlotUI slotToSelect)
        {
            if (contextMenuController != null)
            {
                int currentIndex = slotToSelect.GetIndex();
                ItemType itemType = inventory.ItemStacks[currentIndex].item.ItemType;

                int nextIndex = slotToSelect.GetIndex() + 1;
                InventorySlotUI nextSlot = inventorySlots[nextIndex];
                
                contextMenuController.ShowAtAttachPoint(nextSlot.ContextMenuAttachPoint, itemType);
            }
        }

        private void SetupSelectedItemDescriptionUI()
        {
            if (selectedItemDescriptionUI == null)
            {
                selectedItemDescriptionUI = transform.parent.GetComponentInChildren<SelectedItemDescriptionUI>();
                if (selectedItemDescriptionUI == null)
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
            if (inventoryGridLayoutGroup == null)
            {
                inventoryGridLayoutGroup = GetComponent<GridLayoutGroup>();

                if (inventoryGridLayoutGroup == null)
                {
                    Debug.LogWarning("InventoryGridUIController requires a GridLayoutGroup component on the same GameObject.");
                }
            }
        }

        void OnDisable()
        {
            inventory.OnInventoryChanged -= RefreshGrid;
            CurrentFocusedSlot = null;
            SelectedSlot = null;
        }

        public void FocusFirstAvailableItem()
        {
            if (!inventory.IsEmpty())
            {
                FocusSlot(inventorySlots[0]);
            }
            else
            {
                Debug.Log("No inventory items available to display item info.");
                selectedItemNameUI.SetItemName(string.Empty);
                selectedItemNameUI.ToggleEquippedText(false);
                selectedItemTypeUI.SetItemType(string.Empty);
                selectedItemDescriptionUI.SetItemDescription(string.Empty);
            }
        }

        public void FocusSlot(InventorySlotUI slot)
        {
            if (slot == null)
            {
                Debug.LogWarning("InventoryGridUIController.FocusSlot() - slot is null.");
                return;
            }

            if (inventory.ItemStacks == null)
            {
                Debug.LogWarning("InventoryGridUIController.FocusSlot() - Inventory's ItemStacks is null.");
                return;
            }

            int index = slot.GetIndex();

            if (inventory.ItemStacks[index] == null)
            {
                Debug.Log("InventoryGridUIController.FocusSlot() - Focused slot has no item.");
                return;
            }

            if (index < inventory.ItemStacks.Count)
            {

                ItemStack itemStack = inventory.ItemStacks[index];

                if (itemStack != null)
                {
                    if (CurrentFocusedSlot == null)
                    {
                        slot.FadeAlphaHoverBackground();
                    }
                    else if (CurrentFocusedSlot != null && CurrentFocusedSlot != slot)
                    {
                        CurrentFocusedSlot.StopFadingAlphaHoverBackground();
                        PlayHoverSound();
                        slot.FadeAlphaHoverBackground();
                    }
                    
                    SetSelectedItemName(itemStack.item.ItemName);
                    ToggleEquippedText(itemStack.item);
                    SetSelectedItemType(itemStack.item.ItemType.ToString());
                    SetSelectedItemDescription(itemStack.item.Description);
                    CurrentFocusedSlot = slot;
                }
                else
                {
                    Debug.Log("InventoryGridUIController.FocusSlot() - Focused slot has no item.");
                }
            }
        }

        private void PlayHoverSound()
        {
            if (hoverSoundFeedback != null)
            {
                hoverSoundFeedback.PlayFeedbacks();
            }
            else
            {
                Debug.LogWarning("Hover sound feedback is not assigned.");
            }
        }

        public void ClearSelection()
        {
            if (SelectedSlot != null)
            {
                SelectedSlot.SetSelected(false);
                SelectedSlot = null;
            }

            // Only resume fading if pointer is still over the focused slot
            if (CurrentFocusedSlot != null && CurrentFocusedSlot.InventorySlotEventHandler.IsPointerOver)
            {
                CurrentFocusedSlot.FadeAlphaHoverBackground();
            }

            contextMenuController.HideMenu();
        }

        private void SetSelectedItemName(string itemName)
        {
            if (selectedItemNameUI != null)
            {
                selectedItemNameUI.SetItemName(itemName);
            }
        }

        private void ToggleEquippedText(Item item)
        {
            if (selectedItemNameUI != null && weaponManager != null)
            {
                bool isEquipped = weaponManager.IsItemEquipped(item);
                selectedItemNameUI.ToggleEquippedText(isEquipped);
            }
        }

        private void DisplayEquippedText(bool isEquipped)
        {
            if (selectedItemNameUI != null && selectedItemNameUI.equippedText != null)
            {
                selectedItemNameUI.equippedText.SetActive(isEquipped);
            }
        }

        private void SetSelectedItemType(string itemType)
        {
            if (selectedItemTypeUI != null)
            {
                selectedItemTypeUI.SetItemType(itemType.ToString());
            }
        }

        private void SetSelectedItemDescription(string itemDescription)
        {
            if (selectedItemDescriptionUI != null)
            {
                selectedItemDescriptionUI.SetItemDescription(itemDescription);
            }
        }

        /// <summary>
        /// Update slot visuals based on Inventory data.
        /// </summary>
        public void RefreshGrid()
        {
            Debug.Log("Refreshing inventory grid UI...");

            if (weaponManager == null)
            {
                Debug.LogWarning("InventoryGridUIController.RefreshGrid(): PlayerWeaponManager reference is not set.");
                return;
            }

            if (inventory == null)
            {
                Debug.LogWarning("InventoryGridUIController.RefreshGrid(): Inventory reference is not set.");
                return;
            }

            if (inventory.ItemStacks == null)
            {
                Debug.LogWarning("InventoryGridUIController.RefreshGrid(): Inventory's ItemStacks is null.");
                return;
            }

            if (inventorySlots == null || inventorySlots.Count == 0)
            {
                Debug.LogWarning("InventorySlots reference is not set or is empty in InventoryGridUIController.");
                return;
            }

            if (inventory.ItemStacks.Count > inventorySlots.Count)
            {
                Debug.LogWarning("Not enough InventorySlots for all ItemStacks. Some items will not be displayed.");
                return;
            }

            for (int index = 0; index < inventorySlots.Count; index++)
            {
                ItemStack itemStack = inventory.ItemStacks[index];
                UpdateInventorySlotUI(inventorySlots[index], itemStack, index);
            }

            FocusFirstAvailableItem();
        }

        private void UpdateInventorySlotUI(InventorySlotUI slot, ItemStack itemStack, int index)
        {
            Debug.Log($"InventoryGridUIController.UpdateInventorySlotUI() - Updating slot at index {index}...");
            slot.SetIndex(index);

            if (itemStack != null && itemStack.item != null)
            {
                // Slot has item
                slot.SetEmpty(false);

                // Display icon
                slot.ItemIconImage.enabled = true;
                slot.ItemIconImage.sprite = itemStack.item.ItemIcon;

                // Display count if stackable
                if (itemStack.item.IsStackable)
                {
                    slot.ItemCountText.gameObject.SetActive(true);
                    slot.ItemCountText.text = itemStack.quantity.ToString();
                }
                else
                {
                    slot.ItemCountText.gameObject.SetActive(false);
                }

                // Show equipped icon if the item is equipped
                bool isEquipped = weaponManager != null && weaponManager.CurrentWeaponItem != null && itemStack.item == weaponManager.CurrentWeaponItem;
                slot.DisplayEquippedIcon(isEquipped);
            }
            else
            {
                slot.ItemIconImage.enabled = false;
                slot.ItemCountText.gameObject.SetActive(false);
                slot.SetEmpty(true);
                slot.DisplayEquippedIcon(false);
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

            if (selectedIndex < 0 || selectedIndex >= inventory.ItemStacks.Count)
            {
                Debug.LogWarning("Selected slot index is out of range of the inventory item stacks.");
                return;
            }

            ItemStack selectedItemStack = inventory.ItemStacks[selectedIndex];

            if (selectedItemStack == null)
            {
                Debug.LogWarning("Selected slot does not contain a valid item stack to drop.");
                return;
            }

            inventory.DropItemStack(selectedIndex);
            if (weaponManager.IsItemEquipped(selectedItemStack.item))
            {
                weaponManager.DespawnWeaponInWeaponHand();
            }
        }

        public List<InventorySlotUI> GetInventorySlots()
        {
            return inventorySlots;
        }
    }
}
