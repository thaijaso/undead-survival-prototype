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

    void Reset()
    {
        arrow = transform.Find("Arrow").gameObject;
        button = transform.Find("PCButtonWhite").gameObject; // TODO: implement controller support
        textBackground = transform.Find("TextBackground").gameObject;
    }

    public void EnableArrow()
    {
        arrow.SetActive(true);
    }

    public void DisableArrow()
    {
        arrow.SetActive(false);
    }

    public void EnableButton()
    {
        button.SetActive(true);
    }

    public void DisableButton()
    {
        button.SetActive(false);
    }

    public void EnableTextBackground()
    {
        textBackground.SetActive(true);
    }

    public void DisableTextBackground()
    {
        textBackground.SetActive(false);
    }

    public void SetText(string text)
    {
        textMeshPro.text = text;
    }
}