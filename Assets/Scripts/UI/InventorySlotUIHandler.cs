using System;
using UndeadSurvivalGame.Gameplay;
using UndeadSurvivalGame.PlayerSystems;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UndeadSurvivalGame.UI
{ 
    public class InventorySlotUIHandler : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
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

        [SerializeField]
        private PlayerWeaponManager playerWeaponManager;

        public Action<InventorySlotUI> OnPointerEnteredSlot; 

        void Awake()
        {
            SetupInventory();
            SetupPlayerWeaponManager();
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
                Inventory = FindFirstObjectByType<Inventory>();
                if (Inventory == null)
                {
                    Debug.LogWarning("InventorySlotUIHandler requires an Inventory in the parent hierarchy.");
                }
            }
        }

        private void SetupPlayerWeaponManager()
        {
            if (playerWeaponManager == null)
            {
                playerWeaponManager = FindFirstObjectByType<PlayerWeaponManager>();
                if (playerWeaponManager == null)
                {
                    Debug.LogWarning("InventorySlotUIHandler requires a PlayerWeaponManager in the scene.");
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
                    Debug.LogWarning("InventorySlotUIHandler requires an InventoryGridUIController in the parent hierarchy.");
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
                    Debug.LogWarning("InventorySlotUIHandler requires an InventorySelectedItemNameUI in the parent hierarchy.");
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
                    Debug.LogWarning("InventorySlotUIHandler requires a SelectedItemTypeUI in the parent hierarchy.");
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
                    Debug.LogWarning("InventorySlotUIHandler requires a SelectedItemDescriptionUI in the parent hierarchy.");
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
                    Debug.LogWarning("InventorySlotUIHandler requires an InventorySlotUI on the same GameObject.");
                }
            }
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log($"[{gameObject.name}] InventorySlotUIHandler.OnPointerEnter(): Pointer entered on {gameObject.name}");

            if (InventorySlotUI == null)
            {
                Debug.LogWarning("InventorySlotUIHandler requires an InventorySlotUI on the same GameObject.");
                return;
            }

            // if (InventoryGridUIController == null)
            // {
            //     Debug.LogWarning("InventorySlotUIHandler requires an InventoryGridUIController in the parent hierarchy.");
            //     return;
            // }

            // if (InventorySlotUI.GetIndex() < Inventory.ItemStacks.Count)
            // {
            //     InventoryGridUIController.FocusSlot(InventorySlotUI);
            // }
            OnPointerEnteredSlot?.Invoke(InventorySlotUI);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[{gameObject.name}] InventorySlotUIHandler.OnPointerClick(): Pointer clicked on {gameObject.name}");

            if (eventData.button == PointerEventData.InputButton.Left)
            {                
                if (!InventorySlotUI.IsEmpty())
                {
                    HandleSlotSelection();
                    //PlayClickFeedback(); TODO: Play different sound feedback
                }
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

                Item item = Inventory.itemStacks[selectedIndex].item;
                string itemName = item.itemName;
                string itemType = item.itemType.ToString();
                string itemDesc = item.description;
                bool isItemEquipped = playerWeaponManager.IsItemEquipped(item);

                SelectedItemNameUI.SetItemName(itemName);
                SelectedItemNameUI.ToggleEquippedText(isItemEquipped);
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
            if (InventorySlotUI.ClickFeedback != null)
            {
                InventorySlotUI.ClickFeedback.PlayFeedbacks();
            }
            else
            {
                Debug.LogWarning("ClickPlayerFeedback is not set in InventorySlotUI.");
            }
        }
    }
}
