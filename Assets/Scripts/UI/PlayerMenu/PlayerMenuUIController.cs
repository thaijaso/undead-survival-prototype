using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UndeadSurvivalGame.UI
{ 
    public class PlayerMenuUIController : MonoBehaviour
    {
        public GameObject PlayerMenuUI;
        public event Action<bool> OnPlayerMenuToggled; // true = open, false = closed

        [SerializeField]
        private UIInput uiInput;

        [SerializeField]
        private InventoryGridUIController inventoryGridUIController;

        [SerializeField]
        private GameObject bottomBarPrimaryActionContainer;

        [SerializeField]
        private GameObject bottomBarSecondaryActionContainer;

        [SerializeField]
        private GameObject bottomBarTertiaryActionContainer;

        [SerializeField]
        private InputActionAsset inputActions;

        private InputActionMap playerMap;
        private InputActionMap uiMap;

        private InputAction closePlayerMenuAction;

        private void OnEnable()
        {
            if (PlayerMenuUI == null)
            {
                Debug.LogWarning("Player menu is not assigned in the PlayerMenuController.");
            }

            SetupPlayerMenuUI();
            SetupInputActionsAsset();
            SetupInputActionMaps();
            SetupInputActions();
            SetupInventoryGridUIController();
            //SetupBottomBarActionContainers();
            SubscribeToUIInputEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromUIInputEvents();
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
            uiMap = inputActions.FindActionMap("UI");
        }

        private void SetupInputActions()
        {
            closePlayerMenuAction = uiMap.FindAction("ClosePlayerMenu");

            if (closePlayerMenuAction != null)
            {
                closePlayerMenuAction.performed += ctx => TogglePlayerMenu();
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

        public void TogglePlayerMenu()
        {
            if (PlayerMenuUI == null)
            {
                Debug.LogWarning("PlayerMenuUI is not assigned in the PlayerMenuController.");
                return;
            }

            bool isMenuActive = !PlayerMenuUI.activeSelf;
            PlayerMenuUI.SetActive(isMenuActive);

            ToggleCursor(isMenuActive);
            ToggleActionMap(isMenuActive);
            ToggleUIInput(isMenuActive);

            OnPlayerMenuToggled?.Invoke(isMenuActive);
        }

        private void ToggleCursor(bool isMenuActive)
        {
            if (isMenuActive)
                CursorUtils.ShowCursor();
            else
                CursorUtils.HideCursor();
        }

        private void ToggleActionMap(bool isMenuActive)
        {
            if (isMenuActive)
            {
                playerMap.Disable();
                uiMap.Enable();
            }
            else
            {
                uiMap.Disable();
                playerMap.Enable();
            }
        }

        private void ToggleUIInput(bool isMenuActive)
        {
            if (uiInput == null)
            {
                Debug.LogWarning("UIInput component is not assigned.");
                return;
            }

            if (isMenuActive)
                uiInput.EnableUIInput();
            else
                uiInput.DisableUIInput();
        }

        private void SubscribeToUIInputEvents()
        {
            if (uiInput == null)
            {
                SetupUIInput();
            }

            if (uiInput != null)
            {
                uiInput.OnDropItem += HandleDropItem;
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
                uiInput.OnDropItem -= HandleDropItem;
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

        private void SetupBottomBarActionContainers()
        {
            SetupPrimaryActionContainer();
            SetupSecondaryActionContainer();
            SetupTertiaryActionContainer();
        }

        private void SetupPrimaryActionContainer()
        {
            if (bottomBarPrimaryActionContainer == null)
            {
                bottomBarPrimaryActionContainer = PlayerMenuUI.transform.Find("Container/BottomBarContainer/PrimaryActionContainer").gameObject;
            }

            if (bottomBarPrimaryActionContainer == null)
            {
                Debug.LogWarning("PlayerMenuUIController requires a PrimaryActionContainer in the BottomBarContainer.");
            }
        }
        private void SetupSecondaryActionContainer()
        {
            if (bottomBarSecondaryActionContainer == null)
            {
                bottomBarSecondaryActionContainer = PlayerMenuUI.transform.Find("Container/BottomBarContainer/SecondaryActionContainer").gameObject;
            }

            if (bottomBarSecondaryActionContainer == null)
            {
                Debug.LogWarning("PlayerMenuUIController requires a SecondaryActionContainer in the BottomBarContainer.");
            }
        }

        private void SetupTertiaryActionContainer()
        {
            if (bottomBarTertiaryActionContainer == null)
            {
                bottomBarTertiaryActionContainer = PlayerMenuUI.transform.Find("Container/BottomBarContainer/TertiaryActionContainer").gameObject;
            }

            if (bottomBarTertiaryActionContainer == null)
            {
                Debug.LogWarning("PlayerMenuUIController requires a TertiaryActionContainer in the BottomBarContainer.");
            }
        }
    }
}
