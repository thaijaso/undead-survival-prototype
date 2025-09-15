using UnityEngine;

public class ContextMenuButtonUI : MonoBehaviour
{
    [SerializeField]
    private ContextMenuButtonEventHandler contextMenuButtonEventHandler;

    public ContextMenuButtonEventHandler ContextMenuButtonEventHandler => contextMenuButtonEventHandler;
}
