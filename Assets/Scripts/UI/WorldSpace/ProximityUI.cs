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
            if (arrow == null)
            {
                SetupArrow();
            }

            if (arrow != null)
            {
                arrow.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"ProximityUI.ShowArrowIndicator(): {name} has no Arrow assigned or found in children.");
            }
        }

        public void HideArrowIndicator()
        {
            if (arrow == null)
            {
                SetupArrow();
            }

            if (arrow != null)
            {
                arrow.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"ProximityUI.HideArrowIndicator(): {name} has no Arrow assigned or found in children.");
            }
        }

        public void ShowPickupButton()
        {
            if (button == null)
            {
                SetupButton();
            }

            if (button != null)
            {
                button.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"ProximityUI.ShowPickupButton(): {name} has no Button assigned or found in children.");
            }
        }

        public void HidePickupButton()
        {
            if (button == null)
            {
                SetupButton();
            }

            if (button != null)
            {
                button.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"ProximityUI.HidePickupButton(): {name} has no Button assigned or found in children.");
            }
        }

        public void ShowTextBackground()
        {
            if (content == null)
            {
                SetupContent();
            }

            if (content != null)
            {
                content.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"ProximityUI.ShowTextBackground(): {name} has no Content assigned or found in children.");
            }
        }

        public void HideContent()
        {
            if (content == null)
            {
                SetupContent();
            }

            if (content != null)
            {
                content.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"ProximityUI.HideContent(): {name} has no Content assigned or found in children.");
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
            if (this == null)
            {
                Debug.LogWarning("ProximityUI.HideAllPrompts(): this is null.");
                return;
            }

            HideArrowIndicator();
            HidePickupButton();
            HideContent();
        }
    }
}