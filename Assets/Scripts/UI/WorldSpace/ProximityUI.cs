using TMPro;
using UnityEngine;

public class ProximityUI : MonoBehaviour
{
    [SerializeField]
    private GameObject arrow;

    [SerializeField]
    private GameObject button;

    [SerializeField]
    private GameObject textBackground;

    [SerializeField]
    private TextMeshProUGUI textMeshPro;

    void Awake()
    {
        SetupArrow();
        SetupButton();
        SetupTextBackground();
    }

    private void SetupArrow()
    {
        if (arrow == null)
        {
            arrow = transform.Find("Arrow").gameObject;
        }

        arrow.SetActive(false);
    }

    private void SetupButton()
    {
        if (button == null)
        {
            button = transform.Find("PCButtonWhite").gameObject;
        }

        button.SetActive(false);
    }

    private void SetupTextBackground()
    {
        if (textBackground == null)
        {
            textBackground = transform.Find("TextBackground").gameObject;
        }

        textBackground.SetActive(false);
    }

    public void EnableArrow()
    {
        if (arrow != null)
        {
            arrow.SetActive(true);
        }
    }

    public void DisableArrow()
    {
        if (arrow != null)
        {
            arrow.SetActive(false);
        }
    }

    public void EnableButton()
    {
        if (button != null)
        {
            button.SetActive(true);
        }
    }

    public void DisableButton()
    {
        if (button != null)
        {
            button.SetActive(false);
        }
    }

    public void EnableTextBackground()
    {
        if (textBackground != null)
        {
            textBackground.SetActive(true);
        }
    }

    public void DisableTextBackground()
    {
        if (textBackground != null)
        {
            textBackground.SetActive(false);
        }
    }

    public void SetText(string text)
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = text;
        }
    }
}