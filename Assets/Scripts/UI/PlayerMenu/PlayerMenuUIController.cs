using System;
using UnityEngine;

public class PlayerMenuUIController : MonoBehaviour
{
    public GameObject playerMenu;
    public event Action<bool> OnPlayerMenuToggled; // true = open, false = closed

    private void Awake()
    {
        if (playerMenu == null)
        {
            Debug.LogWarning("Player menu is not assigned in the PlayerMenuController.");
        }
        else
        {
            playerMenu.SetActive(false); // Ensure menu is initially closed
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

        if (isMenuActive)
            CursorUtils.ShowCursor();
        else
            CursorUtils.HideCursor();

        OnPlayerMenuToggled?.Invoke(isMenuActive);
    }
}
