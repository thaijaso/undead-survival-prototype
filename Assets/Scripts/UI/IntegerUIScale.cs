using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(CanvasScaler), typeof(Canvas))]
public class IntegerUIScale : MonoBehaviour
{
    public int referenceHeight = 1080;
    public float step = 0.5f;             // 1.0 = integer only, 0.5 = half steps
    public float minScale = 0.75f, maxScale = 3f;

    CanvasScaler scaler;
    Canvas canvas;

    void OnEnable() {
        scaler = GetComponent<CanvasScaler>();
        canvas = GetComponent<Canvas>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        Canvas.willRenderCanvases += Apply;   // fires in edit & play
    }

    void OnDisable() {
        Canvas.willRenderCanvases -= Apply;
    }

    void OnRectTransformDimensionsChange() {
        Apply();   // also catch editor resize events
    }

    void Apply() {
        if (scaler == null || canvas == null) return; // Prevent null reference errors
        float h = canvas ? canvas.pixelRect.height : Screen.height;   // works in editor
        float raw = h / referenceHeight;
        float snapped = Mathf.Round(raw / step) * step;
        snapped = Mathf.Clamp(snapped, minScale, maxScale);
        if (Mathf.Abs(scaler.scaleFactor - snapped) > 0.001f)
            scaler.scaleFactor = snapped;
    }
}
