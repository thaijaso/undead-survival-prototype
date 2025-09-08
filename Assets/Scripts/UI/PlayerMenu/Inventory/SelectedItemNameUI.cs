using TMPro;
using UndeadSurvivalGame.Gameplay;
using UnityEngine;

namespace UndeadSurvivalGame.UI
{
    public class SelectedItemNameUI : MonoBehaviour
    {
        public Inventory inventory;
        public TextMeshProUGUI itemName;
        public GameObject equippedText;

        private void Awake()
        {
            SetupItemName();
            SetupEquippedText();
        }

        private void SetupItemName()
        {
            if (itemName == null)
            {
                itemName = GetComponentInChildren<TextMeshProUGUI>();
                if (itemName == null)
                {
                    Debug.LogWarning("SelectedItemNameUI requires a TextMeshProUGUI component in the children.");
                }
            }
        }

        private void SetupEquippedText()
        {
            if (equippedText == null)
            {
                equippedText = transform.Find("EquippedText").gameObject;

                if (equippedText == null)
                {
                    Debug.LogWarning("SelectedItemNameUI requires a child GameObject named 'EquippedText' for displaying equipped status.");
                }
                else
                {
                    equippedText.SetActive(false);
                }
            }
        }

        public void SetItemName(string itemName)
        {
            this.itemName.text = itemName;
        }

        public void ToggleEquippedText(bool isEquipped)
        {
            if (equippedText != null)
            {
                equippedText.SetActive(isEquipped);
            }
            else
            {
                Debug.LogWarning("SelectedItemNameUI.ToggleEquippedText(): EquippedText GameObject is not assigned.");
            }
        }
    }
}
