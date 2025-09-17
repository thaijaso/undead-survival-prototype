using UndeadSurvivalGame.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace UndeadSurvivalGame.UI
{
    public class ContextMenuController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private CanvasGroup canvasGroup;       // the CanvasGroup on this object

        [SerializeField]
        private RectTransform menuRoot;        // this ContextMenu RectTransform

        [SerializeField]
        private GridLayoutGroup grid;          // your Inventory GridLayoutGroup

        [SerializeField]
        private ContextMenuButtonUI equipButton;

        [SerializeField]
        private ContextMenuButtonUI useButton;

        [SerializeField]
        private ContextMenuButtonUI combineButton;

        [SerializeField]
        private ContextMenuButtonUI shortcutButton;

        [SerializeField]
        private ContextMenuButtonUI dropButton;

        public bool IsVisible => menuRoot.gameObject.activeSelf;

        private RectTransform parentRect;

        private void Awake()
        {
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
            if (!menuRoot) menuRoot = GetComponent<RectTransform>();
            parentRect = menuRoot.parent as RectTransform;

            SetupButtons();
            Hide();
        }

        private void SetupButtons()
        {
            if (equipButton == null)
            {
                equipButton = transform.Find("EquipButton").GetComponent<ContextMenuButtonUI>();
            }

            if (equipButton == null)
            {
                Debug.LogError("ContextMenuController: EquipButton reference is missing and could not be found in children.");
            }

            if (useButton == null)
            {
                useButton = transform.Find("UseButton").GetComponent<ContextMenuButtonUI>();
            }

            if (useButton == null)
            {
                Debug.LogError("ContextMenuController: UseButton reference is missing and could not be found in children.");
            }

            if (combineButton == null)
            {
                combineButton = transform.Find("CombineButton").GetComponent<ContextMenuButtonUI>();
            }

            if (combineButton == null)
            {
                Debug.LogError("ContextMenuController: CombineButton reference is missing and could not be found in children.");
            }

            if (shortcutButton == null)
            {
                shortcutButton = transform.Find("ShortcutButton").GetComponent<ContextMenuButtonUI>();
            }

            if (shortcutButton == null)
            {
                Debug.LogError("ContextMenuController: ShortcutButton reference is missing and could not be found in children.");
            }

            if (dropButton == null)
            {
                dropButton = transform.Find("DropButton").GetComponent<ContextMenuButtonUI>();
            }

            if (dropButton == null)
            {
                Debug.LogError("ContextMenuController: DropButton reference is missing and could not be found in children.");
            }
        }

        public ContextMenuButtonUI EquipButton => equipButton;
        public ContextMenuButtonUI UseButton => useButton;
        public ContextMenuButtonUI CombineButton => combineButton;
        public ContextMenuButtonUI ShortcutButton => shortcutButton;
        public ContextMenuButtonUI DropButton => dropButton;

        public void ShowAtAttachPoint(RectTransform attachPoint, ItemType itemType)
        {
            // Configure buttons
            equipButton.gameObject.SetActive(itemType == ItemType.Weapon || itemType == ItemType.Clothing);
            useButton.gameObject.SetActive(itemType == ItemType.Consumable);
            combineButton.gameObject.SetActive(itemType == ItemType.Consumable);
            shortcutButton.gameObject.SetActive(itemType == ItemType.Weapon);

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
}
