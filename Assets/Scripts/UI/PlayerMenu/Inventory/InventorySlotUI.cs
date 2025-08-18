using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    private int index;
    public Image BorderBackground;
    public string BorderBackgroundSelectedColor = "#FFFFFFFA";
    public string BorderBackgroundUnselectedColor = "#6F6F6FFA";
    public Image HoverBackground;

    public GameObject ItemIcon;
    public GameObject ItemCount;

    public MMF_Player ClickPlayerFeedback;

    public void SetIndex(int index)
    {
        this.index = index;
    }

    public int GetIndex()
    {
        return index;
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            BorderBackground.color = ColorUtility.TryParseHtmlString(BorderBackgroundSelectedColor, out Color selectedColor) ? selectedColor : Color.white;
        }
        else
        {
            BorderBackground.color = ColorUtility.TryParseHtmlString(BorderBackgroundUnselectedColor, out Color unselectedColor) ? unselectedColor : Color.white;
        }
    }
}
