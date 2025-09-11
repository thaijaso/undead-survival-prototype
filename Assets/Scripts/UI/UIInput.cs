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
        private InputAction rightClick;

        public event Action OnDropItem;
        public event Action OnRightClick;

        private void Start()
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
            //SetupDropItemAction();
            SetupRightClickAction();
        }

        private void SetupRightClickAction()
        {
            rightClick = uiMap.FindAction("RightClick");

            if (rightClick == null)
            {
                Debug.LogWarning("UIInput: No RightClick action found in UI InputActionMap.");
            }

            if (rightClick != null)
            {
                rightClick.performed += ctx =>
                {
                    Debug.Log($"RightClick action performed by {gameObject.name} ({GetInstanceID()})");
                    OnRightClick?.Invoke();
                };
            }
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
    }
}
