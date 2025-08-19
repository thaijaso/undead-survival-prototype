using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class AlignHudToInventoryMargins : MonoBehaviour
{
    [Header("Source (inventory on main canvas)")]
    public RectTransform inventoryPanel;   // inventory window root/background

    [Header("Target (your HUD canvas)")]
    public Canvas hudCanvas;               // HUD canvas (Screen Space - Overlay)
    public CanvasScaler hudScaler;         // Constant Pixel Size scaler

    [Header("Bottom margin mode")]
    public bool bottomMatchesInventoryBottom = false;  // true: use inventory’s bottom margin
    public bool bottomMirrorsInventoryTop   = true;    // true: use inventory’s TOP margin for HUD BOTTOM
    public float fixedBottomPx = 80f;                  // used if both flags are false

    [Header("Extras")]
    public float extraRightPx  = 0f;
    public float extraBottomPx = 0f;

    RectTransform hudRectTransform;

    void OnEnable()
    {
        hudRectTransform = (RectTransform)transform;
        Canvas.willRenderCanvases += Apply;
    }

    void OnDisable() { Canvas.willRenderCanvases -= Apply; }

    void Apply()
    {
        if (!hudRectTransform || !inventoryPanel || !hudCanvas || !hudScaler) return;

        // Require bottom-right anchoring on the HUD panel
        hudRectTransform.anchorMin = hudRectTransform.anchorMax = new Vector2(1f, 0f);
        hudRectTransform.pivot     = new Vector2(1f, 0f);

        // Inventory rect -> screen pixels
        var inventoryWorldCorners = new Vector3[4];                 // 0=BL,1=TL,2=TR,3=BR
        inventoryPanel.GetWorldCorners(inventoryWorldCorners);
        Vector2 inventoryBottomLeft = RectTransformUtility.WorldToScreenPoint(null, inventoryWorldCorners[0]);
        Vector2 inventoryTopLeft = RectTransformUtility.WorldToScreenPoint(null, inventoryWorldCorners[1]);
        Vector2 inventoryTopRight = RectTransformUtility.WorldToScreenPoint(null, inventoryWorldCorners[2]);

        Rect screenRect = hudCanvas.pixelRect;

        // RIGHT margin = screen right - inv right
        float rightMarginPixels = Mathf.Max(0, screenRect.xMax - inventoryTopRight.x) + extraRightPx;

        // BOTTOM margin: choose one
        float bottomMarginPixels;
        if (bottomMatchesInventoryBottom)
            bottomMarginPixels = Mathf.Max(0, inventoryBottomLeft.y - screenRect.yMin) + extraBottomPx;            // match inventory bottom
        else if (bottomMirrorsInventoryTop)
            bottomMarginPixels = Mathf.Max(0, screenRect.yMax - inventoryTopLeft.y) + extraBottomPx;            // mirror inventory TOP
        else
            bottomMarginPixels = fixedBottomPx + extraBottomPx;                                // fixed value

        // Convert screen pixels -> HUD canvas units (Constant Pixel Size)
        float scaleFactor = Mathf.Max(0.0001f, hudScaler.scaleFactor);
        hudRectTransform.anchoredPosition = new Vector2(-rightMarginPixels / scaleFactor, bottomMarginPixels / scaleFactor);
    }
}