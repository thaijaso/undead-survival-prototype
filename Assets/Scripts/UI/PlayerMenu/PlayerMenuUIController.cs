using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMenuUIController : MonoBehaviour
{
    public GameObject playerMenu;
    public event Action<bool> OnPlayerMenuToggled; // true = open, false = closed

    [SerializeField]
    private InputActionAsset inputActions;

    private InputActionMap playerMap;
    private InputActionMap uiMap;

    private InputAction closePlayerMenuAction;

    private void Awake()
    {
        if (playerMenu == null)
        {
            Debug.LogWarning("Player menu is not assigned in the PlayerMenuController.");
        }

        SetupInputActionsAsset();
        SetupInputActionMaps();
        SetupInputActions();
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

    public void TogglePlayerMenu()
    {
        if (playerMenu == null)
        {
            Debug.LogWarning("Player menu is not assigned in the PlayerMenuController.");
            return;
        }

        bool isMenuActive = !playerMenu.activeSelf;
        playerMenu.SetActive(isMenuActive);

        ToggleCursor(isMenuActive);
        ToggleActionMap(isMenuActive);

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
}
