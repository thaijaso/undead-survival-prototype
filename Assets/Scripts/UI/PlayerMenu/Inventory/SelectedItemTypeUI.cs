using TMPro;
using UnityEngine;

namespace UndeadSurvivalGame.UI
{
    public class SelectedItemTypeUI : MonoBehaviour
    {
        public TextMeshProUGUI itemTypeText;

        private void Awake()
        {
            if (itemTypeText == null)
            {
                itemTypeText = GetComponentInChildren<TextMeshProUGUI>();
                if (itemTypeText == null)
                {
                    Debug.LogWarning("SelectedItemTypeUI requires a TextMeshProUGUI component in the children.");
                }
            }
        }

        public void SetItemType(string itemType)
        {
            itemTypeText.text = itemType;
        }
    }
}
