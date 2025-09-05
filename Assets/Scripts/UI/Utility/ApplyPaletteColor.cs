using TMPro;
using UndeadSurvivalGame.UI;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
public class ApplyPaletteColor : MonoBehaviour
{
    public ColorPalette Palette;
    public ColorRole Role = ColorRole.Foreground;

    [Tooltip("If left empty, will auto-find on this GameObject.")]
    public Graphic Graphic;   // Image, RawImage, Text (UGUI)
    public TMP_Text TMPText;  // TextMeshProUGUI

    [Header("Optional")]
    public bool ApplyToChildren = false;

    void OnEnable()  => Apply();
#if UNITY_EDITOR
    void OnValidate() => Apply();
#endif

    void Apply()
    {
        if (!Palette) return;

        if (Graphic == null && TMPText == null)
        {
            Graphic = GetComponent<Graphic>();
            TMPText = GetComponent<TMP_Text>();
        }

        var color = Palette.Get(Role);

        if (!ApplyToChildren)
        {
            if (Graphic) Graphic.color = color;
            if (TMPText) TMPText.color = color;
            return;
        }

        // Propagate to children
        foreach (var graphic in GetComponentsInChildren<Graphic>(true)) graphic.color = color;
        foreach (var tmpText in GetComponentsInChildren<TMP_Text>(true)) tmpText.color = color;
    }
}
