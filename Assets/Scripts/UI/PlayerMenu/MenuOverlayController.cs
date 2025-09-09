using UnityEngine;
using UnityEngine.UI;

public class MenuOverlayController : MonoBehaviour
{
    public Image OverlayImage;

    private void Awake()
    {
        if (OverlayImage == null)
        {
            OverlayImage = GetComponent<Image>();
        }

        if (OverlayImage == null)
        {
            Debug.LogWarning("MenuOverlayController: No Image component found on the GameObject.");
            return;
        }

        OverlayImage.enabled = false;
    }
}
