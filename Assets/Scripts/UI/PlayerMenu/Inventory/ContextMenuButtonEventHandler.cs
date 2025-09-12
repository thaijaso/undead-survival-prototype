using UndeadSurvivalGame.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextMenuButtonEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image hoverBackgroundImage;

    [SerializeField]
    private AnimateAlpha animateAlpha;

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
}
