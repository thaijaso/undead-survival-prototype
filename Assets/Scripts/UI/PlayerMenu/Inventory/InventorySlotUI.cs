using MoreMountains.Feedbacks;
using TMPro;
using UndeadSurvivalGame.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace UndeadSurvivalGame.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField]
        private RectTransform contextMenuAttachPoint;

        [SerializeField]
        private InventorySlotUIHandler inventorySlotUIHandler;

        [SerializeField]
        private Image selectedBackgroundImage;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Image hoverBackgroundImage;

        [SerializeField]
        private Image itemIconImage;

        [SerializeField]
        private TextMeshProUGUI itemCountText;

        [SerializeField]
        private Image equippedIconImage;

        [SerializeField]
        private MMF_Player hoverFeedback;

        [SerializeField]
        private MMF_Player clickFeedback;

        public RectTransform ContextMenuAttachPoint => contextMenuAttachPoint;
        public InventorySlotUIHandler InventorySlotUIHandler => inventorySlotUIHandler;
        public Image SelectedBackgroundImage => selectedBackgroundImage;
        public Image BackgroundImage => backgroundImage;
        public Image HoverBackgroundImage => hoverBackgroundImage;
        public Image ItemIconImage => itemIconImage;
        public TextMeshProUGUI ItemCountText => itemCountText;
        public MMF_Player ClickFeedback => clickFeedback;

        private int index;
        private bool isSelected;
        private bool isFocused;
        private bool isEmpty;
        private void Awake()
        {
            SetupContextMenuAttachPoint();
            SetupInventorySlotUIHandler();
        }

        private void SetupContextMenuAttachPoint()
        {
            if (contextMenuAttachPoint == null)
            {
                // Try to find by name in all descendants
                var transforms = transform.GetComponentsInChildren<Transform>(true);
                foreach (var transform in transforms)
                {
                    if (transform.name == "ContextMenuAttachPoint")
                    {
                        contextMenuAttachPoint = transform.GetComponent<RectTransform>();
                        break;
                    }
                }

                if (contextMenuAttachPoint == null)
                {
                    Debug.LogWarning($"[{gameObject.name}] InventorySlotUI: No ContextMenuAttachPoint found in children or grandchildren.");
                }
            }
        }

        private void OnDisable()
        {
            if (HoverBackgroundImage != null)
            {
                HoverBackgroundImage.enabled = false;
            }
        }

        private void SetupInventorySlotUIHandler()
        {
            if (inventorySlotUIHandler == null)
            {
                inventorySlotUIHandler = GetComponent<InventorySlotUIHandler>();

                if (inventorySlotUIHandler == null)
                {
                    Debug.LogWarning($"[{gameObject.name}] InventorySlotUI requires an InventorySlotUIHandler on the same GameObject.");
                }
            }
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
        }

        public void SetEmpty(bool isEmpty)
        {
            Debug.Log($"InventorySlotUI.SetEmpty(): {gameObject.name} isEmpty: " + isEmpty);

            this.isEmpty = isEmpty;
        }

        public bool IsEmpty()
        {
            return isEmpty;
        }

        public void DisplayEquippedIcon(bool isEquipped)
        {
            equippedIconImage.enabled = isEquipped;
        }

        public void FadeAlphaHoverBackground()
        {
            if (hoverBackgroundImage == null)
            {
                Debug.LogWarning($"[{gameObject.name}] InventorySlotUI.FadeAlphaHoverBackground(): HoverBackground is not assigned.");
                return;
            }

            hoverBackgroundImage.enabled = true;
            AnimateAlpha animateAlpha = hoverBackgroundImage.GetComponent<AnimateAlpha>();

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
            if (hoverBackgroundImage == null)
            {
                Debug.LogWarning($"[{gameObject.name}] InventorySlotUI.StopFadingAlphaHoverBackground(): HoverBackground is not assigned.");
                return;
            }

            AnimateAlpha animateAlpha = hoverBackgroundImage.GetComponent<AnimateAlpha>();

            if (animateAlpha != null)
            {
                animateAlpha.StopContinuousFade();
                hoverBackgroundImage.enabled = false;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] InventorySlotUI.StopFadingAlphaHoverBackground(): No AnimateAlpha component found on HoverBackground.");
            }
        }

    }
}
