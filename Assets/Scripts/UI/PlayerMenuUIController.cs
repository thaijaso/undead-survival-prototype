using System;
using UnityEngine;

public class PlayerMenuUIController : MonoBehaviour
{
    public GameObject playerMenu;
    public event Action<bool> OnPlayerMenuToggled; // true = open, false = closed

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
