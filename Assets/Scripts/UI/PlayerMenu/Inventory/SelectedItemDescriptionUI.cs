using TMPro;
using UnityEngine;

public class SelectedItemDescriptionUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI ItemDescriptionText;

    private void Awake()
    {
        if (ItemDescriptionText == null)
        {
            ItemDescriptionText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public void SetItemDescription(string description)
    {
        if (ItemDescriptionText != null)
        {
            ItemDescriptionText.text = description;
        }
    }
}
