using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UndeadSurvivalGame.Gameplay;

namespace UndeadSurvivalGame.UI
{
    public class ProximityUI : MonoBehaviour
    {
        public string itemPickupTemplate = $"{{itemName}} ({{quantity}})";
        public string inventoryFullTemplate = "Inventory Full";

        [SerializeField]
        private GameObject arrow;

        [SerializeField]
        private GameObject button;

        [SerializeField]
        private GameObject content;

        [SerializeField]
        private TextMeshProUGUI itemNameText;

        [SerializeField]
        private MMF_Player InventoryFullFeedback;

        private ItemPickupInteractable itemPickupInteractable;

        void Awake()
        {
            Debug.Log($"ProximityUI.Awake() called on {name}, instanceID: {GetInstanceID()}");
            SetupArrow();
            SetupButton();
            SetupContent();
            SetupItemNameText();
            SetupItemPickupInteractable();
            SetupInventoryFullFeedback();
        }

        private void SetupArrow()
        {
            if (arrow == null)
            {
                arrow = transform.Find("PickupUI/Arrow").gameObject;
            }

            if (arrow == null)
            {
                Debug.LogWarning($"ProximityUI.SetupArrow(): {name} has no Arrow assigned or found in children.");
                return;
            }

            arrow.SetActive(false);
        }

        private void SetupButton()
        {
            if (button == null)
            {
                button = transform.Find("PickupUI/HUDItemPickupInfo/Content/Info/InputActionTop/InputButtonContainer/PCButtonWhite").gameObject;
            }

            if (button == null)
            {
                Debug.LogWarning($"ProximityUI.SetupButton(): {name} has no Button assigned or found in children.");
                return;
            }

            button.SetActive(false);
        }

        private void SetupContent()
        {
            if (content == null)
            {
                content = transform.Find("PickupUI/HUDItemPickupInfo/Content").gameObject;
            }

            content.SetActive(false);
        }

        private void SetupItemNameText()
        {
            if (itemNameText == null)
            {
                itemNameText = transform.Find("PickupUI/HUDItemPickupInfo/Content/Info/LabelItemName")?.GetComponent<TextMeshProUGUI>();
            }

            if (itemNameText == null)
            {
                Debug.LogWarning($"ProximityUI.SetupItemNameText(): {name} has no ItemNameText assigned or found in children.");
            }
        }

        private void SetupItemPickupInteractable()
        {
            itemPickupInteractable = GetComponent<ItemPickupInteractable>();

            if (itemPickupInteractable != null)
            {
                itemPickupInteractable.OnPickupAllFailed += SetPickupPrompt;
                itemPickupInteractable.OnInventoryFull += ShowInventoryFullFeedback;
            }
        }

        private void SetupInventoryFullFeedback()
        {
            if (InventoryFullFeedback == null)
            {
                InventoryFullFeedback = GetComponentInChildren<MMF_Player>(true);
            }

            if (InventoryFullFeedback == null)
            {
                Debug.LogWarning($"ProximityUI.SetupInventoryFullFeedback(): {name} has no InventoryFullFeedback assigned or found in children.");
            }
        }

        public void ShowArrowIndicator()
        {
            if (arrow != null)
            {
                arrow.SetActive(true);
            }
        }

        public void HideArrowIndicator()
        {
            if (arrow != null)
            {
                arrow.SetActive(false);
            }
        }

        public void ShowPickupButton()
        {
            if (button != null)
            {
                button.SetActive(true);
            }
        }

        public void HidePickupButton()
        {
            if (button != null)
            {
                button.SetActive(false);
            }
        }

        public void ShowTextBackground()
        {
            if (content != null)
            {
                content.SetActive(true);
            }
        }

        public void HideContent()
        {
            if (content != null)
            {
                content.SetActive(false);
            }
        }

        public void SetPickupPrompt(string itemName, int quantity)
        {
            string text = itemPickupTemplate
                .Replace("{itemName}", itemName)
                .Replace("{quantity}", quantity.ToString());
            SetPromptText(text);
        }

        private void SetPromptText(string text)
        {
            if (itemNameText == null)
            {
                Debug.LogWarning($"ProximityUI.DisplayPromptText(): {name} has no ItemNameText assigned.");
                return;
            }

            itemNameText.text = text;
        }

        private void ShowInventoryFullFeedback()
        {
            PlayInventoryFullEffect();
            DisplayInventoryFullText();
        }

        private void PlayInventoryFullEffect()
        {
            if (InventoryFullFeedback != null)
            {
                InventoryFullFeedback.PlayFeedbacks();
            }
        }

        private void DisplayInventoryFullText()
        {
            SetPromptText(inventoryFullTemplate);
        }

        public void HideAllPrompts()
        {
            HideArrowIndicator();
            HidePickupButton();
            HideContent();
        }
    }
}