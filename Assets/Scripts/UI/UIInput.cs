using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UndeadSurvivalGame.UI
{ 
    public class UIInput : MonoBehaviour
    {
        [SerializeField]
        private InputActionAsset inputActions;
        private InputActionMap uiMap;
        private InputAction dropItem;

        public event Action OnDropItem;

        private void Awake()
        {
            SetupInputActions();
            SetupUIMap();
        }

        private void SetupInputActions()
        {
            if (inputActions == null)
            {
                inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            }

            if (inputActions == null)
            {
                Debug.LogWarning("UIInput: No InputActionAsset found.");
            }
        }

        private void SetupUIMap()
        {
            if (uiMap == null)
            {
                uiMap = inputActions.FindActionMap("UI");
            }

            if (uiMap == null)
            {
                Debug.LogWarning("UIInput: No UI InputActionMap found in InputActionAsset.");
            }

            // Setup UI input actions here
            SetupDropItemAction();
        }

        private void SetupDropItemAction()
        {

            dropItem = uiMap.FindAction("DropItem");

            if (dropItem == null)
            {
                Debug.LogWarning("UIInput: No DropItem action found in UI InputActionMap.");
            }

            if (dropItem != null)
            {
                dropItem.performed += ctx =>
                {
                    Debug.Log("DropItem action performed.");
                    OnDropItem?.Invoke();
                };
            }
        }

        public void EnableUIInput()
        {
            uiMap.Enable();
        }

        public void DisableUIInput()
        {
            uiMap.Disable();
        }
    }
}
