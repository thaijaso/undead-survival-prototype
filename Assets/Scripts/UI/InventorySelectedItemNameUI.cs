using TMPro;
using UnityEngine;

public class InventorySelectedItemNameUI : MonoBehaviour
{
    public Inventory inventory;
    public TextMeshProUGUI itemName;

    public void SetItemName(string itemName)
    {
        this.itemName.text = itemName;
    }
}
