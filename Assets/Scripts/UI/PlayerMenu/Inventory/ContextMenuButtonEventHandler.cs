using MoreMountains.Feedbacks;
using System;
using UndeadSurvivalGame.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UndeadSurvivalGame.UI
{

    public class ContextMenuButtonEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField]
        private ContextMenuButtonUI buttonUI;

        public event Action<ContextMenuButtonUI> OnButtonEnter;
        public event Action OnButtonClicked;

        private void Awake()
        {
            if (buttonUI == null)
            {
                buttonUI = GetComponent<ContextMenuButtonUI>();
            }

            if (buttonUI == null)
            {
                Debug.LogError($"ContextMenuButtonEventHandler.Awake() - No ContextMenuButtonUI found on {gameObject.name} or assigned in the inspector.");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log($"ContextMenuButtonEventHandler.OnPointerEnter() - Pointer entered on {gameObject.name}");
            OnButtonEnter?.Invoke(buttonUI);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"ContextMenuButtonEventHandler.OnPointerClick() - Pointer clicked on {gameObject.name}");
            OnButtonClicked?.Invoke();
        }
    }
}
