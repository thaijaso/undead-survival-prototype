using TMPro;
using UndeadSurvivalGame.Gameplay;
using UnityEngine;

namespace UndeadSurvivalGame.UI
{ 
    public class SelectedItemNameUI : MonoBehaviour
    {
        public Inventory inventory;
        public TextMeshProUGUI itemName;

        private void Awake()
        {
            SetupItemName();
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

        public void SetItemName(string itemName)
        {
            this.itemName.text = itemName;
        }
    }
}
