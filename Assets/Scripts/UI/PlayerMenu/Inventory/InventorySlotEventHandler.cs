using System;
using UndeadSurvivalGame.Gameplay;
using UndeadSurvivalGame.PlayerSystems;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UndeadSurvivalGame.UI
{ 
    public class InventorySlotEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField]
        private Inventory Inventory;

        [SerializeField]
        private InventoryGridUIController InventoryGridUIController;

        [SerializeField]
        private SelectedItemNameUI SelectedItemNameUI;

        [SerializeField]
        private SelectedItemTypeUI SelectedItemTypeUI;

        [SerializeField]
        private SelectedItemDescriptionUI SelectedItemDescriptionUI;

        [SerializeField]
        private InventorySlotUI InventorySlotUI;

        [SerializeField]
        private PlayerWeaponManager playerWeaponManager;

        public event Action<InventorySlotUI> OnPointerEnteredSlot;
        public event Action<InventorySlotUI, PointerEventData.InputButton> OnPointerClickedSlot;

        public bool IsPointerOver { get; private set; }

        void Awake()
        {
            SetupInventory();
            SetupPlayerWeaponManager();
            SetupInventoryGridUIController();
            SetupInventorySelectedItemNameUI();
            SetupSelectedItemTypeUI();
            SetupSelectedItemDescriptionUI();
            SetupInventorySlotUI();
        }

        private void SetupInventory()
        {
            if (Inventory == null)
            {
                Inventory = FindFirstObjectByType<Inventory>();
                if (Inventory == null)
                {
                    Debug.LogWarning("InventorySlotUIHandler requires an Inventory in the parent hierarchy.");
                }
            }
        }

        private void SetupPlayerWeaponManager()
        {
            if (playerWeaponManager == null)
            {
                playerWeaponManager = FindFirstObjectByType<PlayerWeaponManager>();
                if (playerWeaponManager == null)
                {
                    Debug.LogWarning("InventorySlotUIHandler requires a PlayerWeaponManager in the scene.");
                }
            }
        }

        private void SetupInventoryGridUIController()
        {
            if (InventoryGridUIController == null)
            {
                InventoryGridUIController = GetComponentInParent<InventoryGridUIController>();
                if (InventoryGridUIController == null)
                {
                    Debug.LogWarning("InventorySlotUIHandler requires an InventoryGridUIController in the parent hierarchy.");
                }
            }
        }

        private void SetupInventorySelectedItemNameUI()
        {
            if (SelectedItemNameUI == null)
            {
                SelectedItemNameUI = transform.parent.parent.GetComponentInChildren<SelectedItemNameUI>();
                if (SelectedItemNameUI == null)
                {
                    Debug.LogWarning("InventorySlotUIHandler requires an InventorySelectedItemNameUI in the parent hierarchy.");
                }
            }
        }

        private void SetupSelectedItemTypeUI()
        {
            if (SelectedItemTypeUI == null)
            {
                SelectedItemTypeUI = transform.parent.parent.GetComponentInChildren<SelectedItemTypeUI>();
                if (SelectedItemTypeUI == null)
                {
                    Debug.LogWarning("InventorySlotUIHandler requires a SelectedItemTypeUI in the parent hierarchy.");
                }
            }
        }

        private void SetupSelectedItemDescriptionUI()
        {
            if (SelectedItemDescriptionUI == null)
            {
                SelectedItemDescriptionUI = transform.parent.parent.GetComponentInChildren<SelectedItemDescriptionUI>();
                if (SelectedItemDescriptionUI == null)
                {
                    Debug.LogWarning("InventorySlotUIHandler requires a SelectedItemDescriptionUI in the parent hierarchy.");
                }
            }
        }

        private void SetupInventorySlotUI()
        {
            if (InventorySlotUI == null)
            {
                InventorySlotUI = GetComponent<InventorySlotUI>();
                if (InventorySlotUI == null)
                {
                    Debug.LogWarning("InventorySlotUIHandler requires an InventorySlotUI on the same GameObject.");
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log($"[{gameObject.name}] InventorySlotUIHandler.OnPointerEnter(): Pointer entered on {gameObject.name}");

            if (InventorySlotUI == null)
            {
                Debug.LogWarning("InventorySlotUIHandler requires an InventorySlotUI on the same GameObject.");
                return;
            }

            IsPointerOver = true;
            OnPointerEnteredSlot?.Invoke(InventorySlotUI);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log($"[{gameObject.name}] InventorySlotUIHandler.OnPointerExit(): Pointer exited on {gameObject.name}");
            IsPointerOver = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[{gameObject.name}] InventorySlotUIHandler.OnPointerClick(): Pointer clicked on {gameObject.name}");


            //HandleSlotSelection();
            //PlayClickFeedback(); TODO: Play different sound feedback

            if (!InventorySlotUI.IsEmpty())
            {
                OnPointerClickedSlot?.Invoke(InventorySlotUI, eventData.button);
            }
        }
    }
}
