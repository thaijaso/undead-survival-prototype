using UnityEngine;

public static class CursorUtils
{
    public static void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log($"CursorUtils.HideCursor() called. lockState: {Cursor.lockState}, visible: {Cursor.visible}");
    }
}
