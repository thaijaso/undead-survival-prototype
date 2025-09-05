using UnityEngine;
using System;

namespace UndeadSurvivalGame.UI
{
    [CreateAssetMenu(fileName = "ColorPalette", menuName = "ScriptableObjects/ColorPalette")]
    public class ColorPalette : ScriptableObject
    {
        public Color Background = new Color32(0, 0, 0, 178); // semi-transparent black
        public Color Foreground = new Color32(198, 198, 198, 255); // white
        public Color Accent = new Color32(255, 213, 13, 255); // yellow
        public Color Disabled = new Color32(109, 109, 109, 250); // dark gray
        public Color ForegroundVariant = new Color32(0, 0, 0, 255); // black
        public Color Success = new Color32(89, 202, 77, 255); // green
        public Color Error = new Color32(235, 102, 107, 255); // red

        public Color Get(ColorRole role) => role switch
        {
            ColorRole.Background => Background,
            ColorRole.Foreground => Foreground,
            ColorRole.Accent => Accent,
            ColorRole.Disabled => Disabled,
            ColorRole.ForegroundVariant => ForegroundVariant,
            ColorRole.Success => Success,
            ColorRole.Error => Error,
            _ => Color.magenta
        };

        public event Action<ColorPalette> Changed;

#if UNITY_EDITOR
        void OnValidate() => Changed?.Invoke(this); // fires when you edit the asset in Inspector
#endif

        // Call this at runtime if you change values via code
        public void RaiseChanged() => Changed?.Invoke(this);
    }
}