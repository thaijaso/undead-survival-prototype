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

        private void OnEnable()
        {
            // Ensure actions and map are ready and enabled when the object becomes active
            SetupInputActions();
            SetupUIMap();
            if (uiMap != null)
            {
                uiMap.Enable();
            }
        }

        private void OnDisable()
        {
            // Remove handlers and disable map to avoid duplicated callbacks
            if (rightClick != null)
                rightClick.performed -= OnRightClickPerformed;
            if (dropItem != null)
                dropItem.performed -= OnDropItemPerformed;

            if (uiMap != null)
                uiMap.Disable();
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
                return;
            }

            // Ensure we don't double-subscribe
            rightClick.performed -= OnRightClickPerformed;
            rightClick.performed += OnRightClickPerformed;
        }

        private void SetupDropItemAction()
        {
            dropItem = uiMap.FindAction("DropItem");

            if (dropItem == null)
            {
                Debug.LogWarning("UIInput: No DropItem action found in UI InputActionMap.");
                return;
            }

            dropItem.performed -= OnDropItemPerformed;
            dropItem.performed += OnDropItemPerformed;
        }

        private void OnRightClickPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            Debug.Log($"RightClick action performed by {gameObject.name} ({GetInstanceID()})");
            OnRightClick?.Invoke();
        }

        private void OnDropItemPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            Debug.Log("DropItem action performed.");
            OnDropItem?.Invoke();
        }
    }
}
