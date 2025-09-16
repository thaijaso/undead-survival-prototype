using MoreMountains.Feedbacks;
using System;
using UndeadSurvivalGame.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextMenuButtonEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private Image hoverBackgroundImage;

    [SerializeField]
    private AnimateAlpha animateAlpha;

    [SerializeField]
    private MMF_Player hoverSoundFeedback;

    public event Action OnButtonClicked;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"ContextMenuButtonEventHandler.OnPointerEnter() - Pointer entered on {gameObject.name}");

        if (animateAlpha == null)
        {
            Debug.LogWarning("AnimateAlpha component is not assigned.");
            return;
        }

        if (hoverBackgroundImage == null)
        {
            Debug.LogWarning("Hover background Image is not assigned.");
            return;
        }

        if (hoverSoundFeedback == null)
        {
            Debug.LogWarning("Hover sound feedback is not assigned.");
            return;
        }

        hoverSoundFeedback.PlayFeedbacks();
        hoverBackgroundImage.enabled = true;
        animateAlpha.StartContinuousFade();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"ContextMenuButtonEventHandler.OnPointerExit() - Pointer exited from {gameObject.name}");

        if (animateAlpha == null)
        {
            Debug.LogWarning("AnimateAlpha component is not assigned.");
            return;
        }

        if (hoverBackgroundImage == null)
        {
            Debug.LogWarning("Hover background Image is not assigned.");
            return;
        }

        hoverBackgroundImage.enabled = false;
        animateAlpha.StopContinuousFade();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"ContextMenuButtonEventHandler.OnPointerClick() - Pointer clicked on {gameObject.name}");
        OnButtonClicked?.Invoke();
    }
}
