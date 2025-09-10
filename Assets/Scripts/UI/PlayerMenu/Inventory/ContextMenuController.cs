using UnityEngine;
using UnityEngine.UI;

public class ContextMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;       // the CanvasGroup on this object
    [SerializeField] private RectTransform menuRoot;        // this ContextMenu RectTransform
    [SerializeField] private GridLayoutGroup grid;          // your Inventory GridLayoutGroup
    [SerializeField] private Vector2 padding = new Vector2(6f, 6f);

    private RectTransform parentRect;

    private void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!menuRoot) menuRoot = GetComponent<RectTransform>();
        parentRect = menuRoot.parent as RectTransform;

        Hide();
    }

    public void ShowAtAttachPoint(RectTransform attachPoint)
    {
        // World → Screen → Parent local
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, attachPoint.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screen, null, out var local);
        menuRoot.anchoredPosition = local;

        LayoutRebuilder.ForceRebuildLayoutImmediate(menuRoot);

        // Enable menu
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        menuRoot.gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        menuRoot.gameObject.SetActive(false);
    }
}
