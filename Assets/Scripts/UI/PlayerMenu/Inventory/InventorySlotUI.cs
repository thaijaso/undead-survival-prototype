using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

namespace UndeadSurvivalGame.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        private int index;
        public Image BorderBackground;
        public Color BorderBackgroundSelectedColor = Color.white;
        public Color BorderBackgroundUnselectedColor = new(0.4352941f, 0.4352941f, 0.4352941f, 1f); // TODO: create color util class
        public Color BorderBackgroundEmptyColor = new(0.9607843f, 0.0f, 0.0f, 1f);
        public Image HoverBackground;
        public GameObject ItemIcon;
        public GameObject ItemCount;
        public GameObject EquippedIcon;
        public GameObject BottomRightCornerBackground;
        public MMF_Player HoverFeedback;
        public MMF_Player ClickFeedback;

        private bool isSelected;
        private bool isEmpty;

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
            Debug.Log($"InventorySlotUI.SetSelected() {gameObject.name} isSelected: {isSelected}");

            this.isSelected = isSelected;
            UpdateBorderColor();
        }

        public void SetEmpty(bool isEmpty)
        {
            Debug.Log($"InventorySlotUI.SetEmpty(): {gameObject.name} isEmpty: " + isEmpty);

            this.isEmpty = isEmpty;
            UpdateBorderColor();
        }

        public bool IsEmpty()
        {
            return isEmpty;
        }

        private void UpdateBorderColor()
        {
            if (isEmpty)
            {
                BorderBackground.color = BorderBackgroundEmptyColor;
            }
            else if (isSelected)
            {
                BorderBackground.color = BorderBackgroundSelectedColor;
            }
            else
            {
                BorderBackground.color = BorderBackgroundUnselectedColor;
            }
        }

        public void DisplayEquippedIcon(bool isEquipped)
        {
            //EquippedIcon.SetActive(isEquipped);
        }
    }
}
