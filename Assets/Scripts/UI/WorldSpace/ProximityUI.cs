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
        private GameObject textBackground;

        [SerializeField]
        private TextMeshProUGUI textMeshPro;

        [SerializeField]
        private MMF_Player InventoryFullFeedback;

        private ItemPickupInteractable itemPickupInteractable;

        void Awake()
        {
            SetupArrow();
            SetupButton();
            SetupTextBackground();
            SetupItemPickupInteractable();
            SetupInventoryFullFeedback();
        }

        private void SetupArrow()
        {
            if (arrow == null)
            {
                arrow = transform.Find("Arrow").gameObject;
            }

            arrow.SetActive(false);
        }

        private void SetupButton()
        {
            if (button == null)
            {
                button = transform.Find("PCButtonWhite").gameObject;
            }

            button.SetActive(false);
        }

        private void SetupTextBackground()
        {
            if (textBackground == null)
            {
                textBackground = transform.Find("TextBackground").gameObject;
            }

            textBackground.SetActive(false);
        }

        private void SetupItemPickupInteractable()
        {
            itemPickupInteractable = GetComponent<ItemPickupInteractable>();

            if (itemPickupInteractable != null)
            {
                itemPickupInteractable.OnPickupAllFailed += DisplayPickupPrompt;
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
            if (textBackground != null)
            {
                textBackground.SetActive(true);
            }
        }

        public void HideTextBackground()
        {
            if (textBackground != null)
            {
                textBackground.SetActive(false);
            }
        }

        public void DisplayPickupPrompt(string itemName, int quantity)
        {
            string text = itemPickupTemplate
                .Replace("{itemName}", itemName)
                .Replace("{quantity}", quantity.ToString());
            DisplayPromptText(text);
        }

        private void DisplayPromptText(string text)
        {
            if (textMeshPro != null)
            {
                textMeshPro.text = text;
            }
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
            DisplayPromptText(inventoryFullTemplate);
        }

        public void HideAllPrompts()
        {
            HideArrowIndicator();
            HidePickupButton();
            HideTextBackground();
        }
    }
}