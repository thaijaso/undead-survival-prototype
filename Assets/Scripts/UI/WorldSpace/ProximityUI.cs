using UnityEngine;

public class ProximityUI : MonoBehaviour
{
    [SerializeField]
    private GameObject arrow;

    [SerializeField]
    private GameObject button;

    void Awake()
    {
        if (arrow == null)
        {
            arrow = transform.Find("Arrow").gameObject;
        }

        if (button == null)
        {
            button = transform.Find("PCButtonWhite").gameObject;
        }
    }

    void Reset()
    {
        arrow = transform.Find("Arrow").gameObject;
        button = transform.Find("PCButtonWhite").gameObject; // TODO: implement controller support
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
}