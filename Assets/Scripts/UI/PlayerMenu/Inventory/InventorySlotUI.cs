using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

namespace UndeadSurvivalGame.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("Color Palette")]
        public ColorPalette Palette;

        public ColorRole BorderBackgroundSelectedRole = ColorRole.Foreground;
        public ColorRole BorderBackgroundUnselectedRole = ColorRole.Disabled;
        public ColorRole BorderBackgroundEmptyRole = ColorRole.Disabled;

        private int index;
        public Image BorderBackground;
        public Image HoverBackground;
        public GameObject ItemIcon;
        public GameObject ItemCount;
        public GameObject EquippedIcon;
        public GameObject BottomRightCornerBackground;
        public MMF_Player HoverFeedback;
        public MMF_Player ClickFeedback;

        private bool isSelected;
        private bool isFocused;
        private bool isEmpty;

        private void Awake()
        {
            if (Palette == null)
            {
                Palette = Resources.Load<ColorPalette>("ColorPalette");
            }

            if (Palette == null)
            {
                Debug.LogError($"[{gameObject.name}] InventorySlotUI: No ColorPalette assigned or found in Resources!");
            }
        }

        private void OnEnable()
        {
            if (Palette != null)
            {
                Palette.Changed += OnPaletteChanged;
            }
        }

        private void OnDisable()
        {
            if (Palette != null)
            {
                Palette.Changed -= OnPaletteChanged;
            }

            if (HoverBackground != null)
            {
                HoverBackground.enabled = false;
            }
        }

        private void OnPaletteChanged(ColorPalette palette)
        {
            UpdateBorderColor();
        }

        public void SetIndex(int index)
        {
            this.index = index;
        }

        public int GetIndex()
        {
            return index;
        }

        public void SetSelected(bool isSelected)
        {
            Debug.Log($"InventorySlotUI.SetSelected() {gameObject.name} isSelected: {isSelected}");

            this.isSelected = isSelected;
            UpdateBorderColor();
        }

        public void SetEmpty(bool isEmpty)
        {
            Debug.Log($"InventorySlotUI.SetEmpty(): {gameObject.name} isEmpty: " + isEmpty);

            this.isEmpty = isEmpty;
            UpdateBorderColor();
        }

        public bool IsEmpty()
        {
            return isEmpty;
        }

        private void UpdateBorderColor()
        {
            // if (isEmpty)
            // {
            //     BorderBackground.color = Palette.Get(BorderBackgroundEmptyRole);
            // }
            // else if (isSelected)
            // {
            //     BorderBackground.color = Palette.Get(BorderBackgroundSelectedRole);
            // }
            // else
            // {
            //     BorderBackground.color = Palette.Get(BorderBackgroundUnselectedRole);
            // }
        }

        public void DisplayEquippedIcon(bool isEquipped)
        {
            EquippedIcon.SetActive(isEquipped);
        }

        public void FadeAlphaHoverBackground()
        {
            if (HoverBackground == null)
            {
                Debug.LogWarning($"[{gameObject.name}] InventorySlotUI.FadeAlphaHoverBackground(): HoverBackground is not assigned.");
                return;
            }

            HoverBackground.enabled = true;
            AnimateAlpha animateAlpha = HoverBackground.GetComponent<AnimateAlpha>();

            if (animateAlpha != null)
            {
                animateAlpha.StartContinuousFade();
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] InventorySlotUI.FadeAlphaHoverBackground(): No AnimateAlpha component found on HoverBackground.");
            }
        }

        public void StopFadingAlphaHoverBackground()
        {
            if (HoverBackground == null)
            {
                Debug.LogWarning($"[{gameObject.name}] InventorySlotUI.StopFadingAlphaHoverBackground(): HoverBackground is not assigned.");
                return;
            }

            AnimateAlpha animateAlpha = HoverBackground.GetComponent<AnimateAlpha>();

            if (animateAlpha != null)
            {
                animateAlpha.StopContinuousFade();
                HoverBackground.enabled = false;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] InventorySlotUI.StopFadingAlphaHoverBackground(): No AnimateAlpha component found on HoverBackground.");
            }
        }
    }
}
