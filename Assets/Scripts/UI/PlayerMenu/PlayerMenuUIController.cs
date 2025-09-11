using System;
using UndeadSurvivalGame.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UndeadSurvivalGame.UI
{
    public class PlayerMenuUIController : MonoBehaviour
    {
        public GameObject PlayerMenuUI;
        public event Action<bool> OnPlayerMenuToggled; // true = open, false = closed

        [SerializeField] private CanvasGroup menuCanvasGroup;

        [SerializeField] private UIInput uiInput;

        [SerializeField] private Inventory inventory;

        [SerializeField] private InventoryGridUIController inventoryGridUIController;

        [SerializeField] private ContextMenuController contextMenuController;

        [SerializeField] private InputActionAsset inputActions;

        private InputActionMap playerMap;
        private InputActionMap uiMap;

        private InputAction togglePlayerMenuAction;

        private void OnEnable()
        {
            if (PlayerMenuUI == null)
            {
                Debug.LogWarning("Player menu is not assigned in the PlayerMenuController.");
            }

            SetupPlayerMenuUI();
            SetupCanvasGroup();
            SetupInputActionsAsset();
            SetupInputActionMaps();
            SetupInputActions();
            SetupInventory();
            SetupInventoryGridUIController();
            SetupContextMenuController();
        }

        private void SetupPlayerMenuUI()
        {
            if (PlayerMenuUI == null)
            {
                PlayerMenuUI = GameObject.Find("PlayerMenuUI");
            }

            if (PlayerMenuUI == null)
            {
                Debug.LogWarning("PlayerMenuUIController requires a PlayerMenuUI GameObject in the scene.");
            }

            SetMenuVisible(false);
        }

        private void SetupCanvasGroup()
        {
            if (menuCanvasGroup == null)
            {
                menuCanvasGroup = PlayerMenuUI.GetComponent<CanvasGroup>();
            }

            if (menuCanvasGroup == null)
            {
                Debug.LogWarning("PlayerMenuUIController requires a CanvasGroup component on the PlayerMenuUI GameObject.");
            }
        }

        private void SetMenuVisible(bool isVisible)
        {
            if (menuCanvasGroup == null)
            {
                Debug.LogWarning("CanvasGroup is not assigned.");
                return;
            }

            menuCanvasGroup.alpha = isVisible ? 1 : 0;
            menuCanvasGroup.interactable = isVisible;
            menuCanvasGroup.blocksRaycasts = isVisible;
        }

        private void SetupInputActionsAsset()
        {
            if (inputActions == null)
            {
                inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            }

            if (inputActions == null)
            {
                Debug.LogWarning("InputActionAsset 'InputSystem_Actions' not found in Resources.");
                return;
            }
        }

        private void SetupInputActionMaps()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("InputActionAsset is not assigned.");
                return;
            }

            playerMap = inputActions.FindActionMap("Player");

            if (playerMap == null)
            {
                Debug.LogWarning("Player action map not found in InputActionAsset.");
                return;
            }

            uiMap = inputActions.FindActionMap("UI");

            if (uiMap == null)
            {
                Debug.LogWarning("UI action map not found in InputActionAsset.");
                return;
            }
        }

        private void SetupInputActions()
        {
            togglePlayerMenuAction = uiMap.FindAction("TogglePlayerMenu");

            if (togglePlayerMenuAction != null)
            {
                togglePlayerMenuAction.performed += ctx => {
                    Debug.Log($"{gameObject.name} TogglePlayerMenu action performed. Phase: {ctx.phase}");
                    TogglePlayerMenu();
                };
            }
        }

        private void SetupInventory()
        { 
            if (inventory == null)
            {
                inventory = FindFirstObjectByType<Inventory>();
                if (inventory == null)
                {
                    Debug.LogWarning("PlayerMenuUIController requires an Inventory in the scene.");
                }
            }
        }

        private void SetupInventoryGridUIController()
        {
            if (inventoryGridUIController == null)
            {
                inventoryGridUIController = GetComponentInChildren<InventoryGridUIController>();

                if (inventoryGridUIController == null)
                {
                    Debug.LogWarning("PlayerMenuUIController requires an InventoryGridUIController in the children.");
                }
            }
        }

        private void SetupContextMenuController()
        {
            if (contextMenuController == null)
            {
                contextMenuController = GetComponentInChildren<ContextMenuController>();

                if (contextMenuController == null)
                {
                    Debug.LogWarning("PlayerMenuUIController requires a ContextMenuController in the children.");
                }
            }
        }

        public void TogglePlayerMenu()
        {
            Debug.Log($"{gameObject.name} PlayerMenuUIController.TogglePlayerMenu() called.");

            if (PlayerMenuUI == null)
            {
                Debug.LogWarning("PlayerMenuUI is not assigned in the PlayerMenuController.");
                return;
            }

            bool isMenuVisible = menuCanvasGroup.alpha < 0.5f;
            SetMenuVisible(isMenuVisible);

            ToggleCursor(isMenuVisible);
            TogglePlayerActionMap(isMenuVisible);

            if (isMenuVisible)
            {
                SubscribeToUIInputEvents();
                SubscribeToInventorySlotUIHandlerEvents();
                inventoryGridUIController.RefreshGrid();
            }
            else
            {
                UnsubscribeFromUIInputEvents();
                UnsubscribeToInventorySlotUIHandlerEvents();
            }

            OnPlayerMenuToggled?.Invoke(isMenuVisible);
        }

        private void ToggleCursor(bool isMenuVisible)
        {
            if (isMenuVisible)
                CursorUtils.ShowCursor();
            else
                CursorUtils.HideCursor();
        }

        private void TogglePlayerActionMap(bool isMenuVisible)
        {
            if (isMenuVisible)
            {
                playerMap.Disable();
            }
            else
            {
                playerMap.Enable();
            }
        }

        private void SubscribeToUIInputEvents()
        {
            if (uiInput == null)
            {
                SetupUIInput();
            }

            if (uiInput != null)
            {
                uiInput.OnRightClick += HandleRightClick;
            }
        }

        private void SetupUIInput()
        {
            uiInput = GameObject.Find("/UIInput").GetComponent<UIInput>();

            if (uiInput == null)
            {
                Debug.LogWarning("UIInput component not found.");
            }
        }

        private void UnsubscribeFromUIInputEvents()
        {
            if (uiInput != null)
            {
                uiInput.OnRightClick -= HandleRightClick;
            }
        }

        private void HandleRightClick()
        {
            Debug.Log($"{gameObject.name} RightClick event received.");

            if (contextMenuController == null)
            {
                Debug.LogWarning("ContextMenuController is not assigned.");
                return;
            }

            if (inventoryGridUIController == null)
            {
                Debug.LogWarning("InventoryGridUIController is not assigned.");
                return;
            }

            if (contextMenuController.IsVisible)
            {
                contextMenuController.Hide();
                return;
            }
            else
            {
                TogglePlayerMenu();
            }
        }

        private void HandleDropItem()
        {
            Debug.Log("DropItem event received.");

            if (inventoryGridUIController != null)
            {
                inventoryGridUIController.DropSelectedItem();
            }
        }

        private void SubscribeToInventorySlotUIHandlerEvents()
        {
            if (inventoryGridUIController == null)
            {
                Debug.LogWarning("InventoryGridUIController is not assigned.");
                return;
            }

            foreach (var slot in inventoryGridUIController.GetInventorySlots())
            {
                if (slot == null || slot.InventorySlotUIHandler == null)
                {
                    Debug.LogWarning("One of the inventory slots or its handler is not assigned.");
                }

                slot.InventorySlotUIHandler.OnPointerEnteredSlot += HandlePointerEnteredInventorySlot;
            }
        }

        private void UnsubscribeToInventorySlotUIHandlerEvents()
        {
            if (inventoryGridUIController == null)
            {
                return;
            }

            foreach (var slot in inventoryGridUIController.GetInventorySlots())
            {
                slot.InventorySlotUIHandler.OnPointerEnteredSlot -= HandlePointerEnteredInventorySlot;
            }
        }

        private void HandlePointerEnteredInventorySlot(InventorySlotUI slot)
        {
            if (slot == null)
            {
                Debug.LogWarning("HandlePointerEnteredInventorySlot received a null slot.");
                return;
            }

            if (inventory == null)
            {
                Debug.LogWarning("Inventory is not assigned.");
                return;
            }

            if (contextMenuController == null)
            {
                Debug.LogWarning("ContextMenuController is not assigned.");
                return;
            }

            if (inventoryGridUIController == null)
            {
                Debug.LogWarning("InventoryGridUIController is not assigned.");
                return;
            }

            if (contextMenuController.IsVisible)
            {
                return;
            }

            if (slot.GetIndex() < inventory.ItemStacks.Count)
            {
                inventoryGridUIController.FocusSlot(slot);
            }
        }
    }
}
