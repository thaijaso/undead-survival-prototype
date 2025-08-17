using TMPro;
using UnityEngine;

public class SelectedItemNameUI : MonoBehaviour
{
    public Inventory inventory;
    public TextMeshProUGUI itemName;

    private void Awake()
    {
        SetupInventory();
        SetupItemName();
    }

    private void SetupInventory()
    {
        if (inventory == null)
        {
            inventory = transform.parent.GetComponentInChildren<Inventory>();
            if (inventory == null)
            {
                Debug.LogWarning("SelectedItemNameUI requires an Inventory component in the parent.");
            }
        }
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
