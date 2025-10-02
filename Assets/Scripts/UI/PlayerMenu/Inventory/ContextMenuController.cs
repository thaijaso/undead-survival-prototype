using System.Collections.Generic;
using UndeadSurvivalGame.Gameplay;
using UnityEngine;
using UnityEngine.UI;


namespace UndeadSurvivalGame.UI
{
    /// <summary>
    /// Manages the context menu UI for inventory items. 
    /// It controls the visibility, positioning, and button configuration of the menu based on the selected item's type. 
    /// The menu is positioned at a specified attach point by converting its world position to a local position within the parent UI. 
    /// The controller enables or disables relevant buttons (equip, use, combine, shortcut, drop) depending on the item type, and provides methods to show or hide the menu. 
    /// It also maintains a list of currently enabled buttons for further interaction logic.
    /// </summary>
    public class ContextMenuController : MonoBehaviour
    {
        public ContextMenuButtonUI EquipButton => equipButton;
        public ContextMenuButtonUI UnequipButton => unequipButton;
        public ContextMenuButtonUI UseButton => useButton;
        public ContextMenuButtonUI CombineButton => combineButton;
        public ContextMenuButtonUI ShortcutButton => shortcutButton;
        public ContextMenuButtonUI DropButton => dropButton;

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
        private ContextMenuButtonUI unequipButton;

        [SerializeField]
        private ContextMenuButtonUI useButton;

        [SerializeField]
        private ContextMenuButtonUI combineButton;

        [SerializeField]
        private ContextMenuButtonUI shortcutButton;

        [SerializeField]
        private ContextMenuButtonUI dropButton;

        public bool IsVisible => canvasGroup.alpha > 0f;

        private RectTransform parentRect;

        private List<ContextMenuButtonUI> enabledButtons = new();

        private ContextMenuButtonUI currentFocusedButtonUI;

        private void Awake()
        {
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
            if (!menuRoot) menuRoot = GetComponent<RectTransform>();
            parentRect = menuRoot.parent as RectTransform;

            SetupButtons();
            HideMenu();
        }

        private void SetupButtons()
        {
            SetupEquipButton();
            SetupUnequipButton();
            SetupUseButton();
            SetupCombineButton();
            SetupShortcutButton();
            SetupDropButton();
        }

        private void SetupEquipButton()
        {
            if (equipButton == null)
            {
                equipButton = transform.Find("EquipButton")?.GetComponent<ContextMenuButtonUI>();
            }
            if (equipButton == null)
            {
                Debug.LogError("ContextMenuController: EquipButton reference is missing and could not be found in children.");
            }
        }

        private void SetupUnequipButton()
        {
            if (unequipButton == null)
            {
                unequipButton = transform.Find("UnequipButton")?.GetComponent<ContextMenuButtonUI>();
            }
            if (unequipButton == null)
            {
                Debug.LogError("ContextMenuController: UnequipButton reference is missing and could not be found in children.");
            }
        }

        private void SetupUseButton()
        {
            if (useButton == null)
            {
                useButton = transform.Find("UseButton")?.GetComponent<ContextMenuButtonUI>();
            }
            if (useButton == null)
            {
                Debug.LogError("ContextMenuController: UseButton reference is missing and could not be found in children.");
            }
        }

        private void SetupCombineButton()
        {
            if (combineButton == null)
            {
                combineButton = transform.Find("CombineButton")?.GetComponent<ContextMenuButtonUI>();
            }
            if (combineButton == null)
            {
                Debug.LogError("ContextMenuController: CombineButton reference is missing and could not be found in children.");
            }
        }

        private void SetupShortcutButton()
        {
            if (shortcutButton == null)
            {
                shortcutButton = transform.Find("ShortcutButton")?.GetComponent<ContextMenuButtonUI>();
            }
            if (shortcutButton == null)
            {
                Debug.LogError("ContextMenuController: ShortcutButton reference is missing and could not be found in children.");
            }
        }

        private void SetupDropButton()
        {
            if (dropButton == null)
            {
                dropButton = transform.Find("DropButton")?.GetComponent<ContextMenuButtonUI>();
            }
            if (dropButton == null)
            {
                Debug.LogError("ContextMenuController: DropButton reference is missing and could not be found in children.");
            }
        }

        private void Start()
        {
            SubscribeToButtonEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromButtonEvents();
        }

        private void SubscribeToButtonEvents()
        {
            if (equipButton != null) equipButton.EventHandler.OnButtonEnter += OnButtonEntered;
            if (unequipButton != null) unequipButton.EventHandler.OnButtonEnter += OnButtonEntered;
            if (useButton != null) useButton.EventHandler.OnButtonEnter += OnButtonEntered;
            if (combineButton != null) combineButton.EventHandler.OnButtonEnter += OnButtonEntered;
            if (shortcutButton != null) shortcutButton.EventHandler.OnButtonEnter += OnButtonEntered;
            if (dropButton != null) dropButton.EventHandler.OnButtonEnter += OnButtonEntered;
        }

        private void UnsubscribeFromButtonEvents()
        {
            if (equipButton != null) equipButton.EventHandler.OnButtonEnter -= OnButtonEntered;
            if (unequipButton != null) unequipButton.EventHandler.OnButtonEnter -= OnButtonEntered;
            if (useButton != null) useButton.EventHandler.OnButtonEnter -= OnButtonEntered;
            if (combineButton != null) combineButton.EventHandler.OnButtonEnter -= OnButtonEntered;
            if (shortcutButton != null) shortcutButton.EventHandler.OnButtonEnter -= OnButtonEntered;
            if (dropButton != null) dropButton.EventHandler.OnButtonEnter -= OnButtonEntered;
        }

        private void OnButtonEntered(ContextMenuButtonUI button)
        {
            if (currentFocusedButtonUI != button)
            {
                currentFocusedButtonUI.StopHoverAnimation();
                button.PlayHoverAnimation();
                button.PlayHoverSound();
                currentFocusedButtonUI = button;
            }
        }


        public void ShowAtAttachPoint(RectTransform attachPoint, ItemType itemType)
        {
            ConfigureButtons(itemType);
            AddEnabledButtonsToList();
            PositionMenuAtAttachPoint(attachPoint);
            LayoutRebuilder.ForceRebuildLayoutImmediate(menuRoot);
            ShowMenu();
            FocusFirstButton();
        }

        private void ShowMenu()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        public void HideMenu()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            if (currentFocusedButtonUI != null)
            {
                currentFocusedButtonUI.StopHoverAnimation();
                currentFocusedButtonUI = null;
            }
        }

        private void ConfigureButtons(ItemType itemType)
        {
            equipButton.gameObject.SetActive(itemType == ItemType.Weapon || itemType == ItemType.Clothing);
            unequipButton.gameObject.SetActive(itemType == ItemType.Weapon || itemType == ItemType.Clothing);
            useButton.gameObject.SetActive(itemType == ItemType.Consumable);
            combineButton.gameObject.SetActive(itemType == ItemType.Consumable);
            shortcutButton.gameObject.SetActive(itemType == ItemType.Weapon);
        }

        /// <summary>
        /// The PositionMenuAtAttachPoint method positions the context menu UI at a specific inventory slot by converting the slot's world position to a local position within the parent UI RectTransform. 
        /// It first transforms the attach point's world position to screen space, then converts that screen position to the local coordinate space of the parent UI, and finally sets the menu's anchored position accordingly.
        /// This ensures the context menu appears aligned with the selected inventory slot in the UI.
        /// </summary>
        /// <param name="attachPoint"> is a position in world space at the top left corner of an inventory slot</param>
        private void PositionMenuAtAttachPoint(RectTransform attachPoint)
        {
            // World → Screen → Parent local
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, attachPoint.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, null, out var local);
            menuRoot.anchoredPosition = local;
        }

        private void AddEnabledButtonsToList()
        {
            enabledButtons.Clear();

            if (equipButton.gameObject.activeSelf) enabledButtons.Add(equipButton);
            if (useButton.gameObject.activeSelf) enabledButtons.Add(useButton);
            if (combineButton.gameObject.activeSelf) enabledButtons.Add(combineButton);
            if (shortcutButton.gameObject.activeSelf) enabledButtons.Add(shortcutButton);
            if (dropButton.gameObject.activeSelf) enabledButtons.Add(dropButton);
        }

        private void FocusFirstButton()
        {
            if (enabledButtons.Count > 0)
            {
                currentFocusedButtonUI = enabledButtons[0];
                currentFocusedButtonUI.PlayHoverAnimation();
            }
        }
    }
}
