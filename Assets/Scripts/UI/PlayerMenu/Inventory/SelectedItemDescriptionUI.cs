using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace UndeadSurvivalGame.UI
{ 
    public class SelectedItemDescriptionUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI ItemDescriptionText;

        private void Awake()
        {
            SetupItemDescriptionText();
        }

        private void OnEnable()
        {
            SetupItemDescriptionText();
        }

        private void SetupItemDescriptionText()
        {
            if (ItemDescriptionText == null)
            {
                ItemDescriptionText = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (ItemDescriptionText == null)
            {
                Debug.LogWarning($"[{gameObject.name}] SelectedItemDescriptionUI: No TextMeshProUGUI component found in children.");
            }
        }   

        public void SetItemDescription(string description)
        {
            if (ItemDescriptionText != null)
            {
                ItemDescriptionText.text = description;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] SelectedItemDescriptionUI: Cannot set item description because ItemDescriptionText is not assigned.");
            }
        }
    }
}
