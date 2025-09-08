using UnityEngine;

public class BottomBarUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject primaryActionContainer;

    [SerializeField]
    private GameObject secondaryActionContainer;

    [SerializeField]
    private GameObject tertiaryActionContainer;

    private void Awake()
    {
        SetupActionContainers();
    }

    private void SetupActionContainers()
    {
        if (primaryActionContainer == null)
        {
            Transform transform = this.transform.Find("PrimaryActionContainer");

            if (transform == null)
            {
                Debug.LogWarning("BottomBarUIController: PrimaryActionContainer not found in children.");
                return;
            }

            primaryActionContainer = transform.gameObject;
        }

        if (secondaryActionContainer == null)
        {
            Transform transform = this.transform.Find("SecondaryActionContainer");

            if (transform == null)
            {
                Debug.LogWarning("BottomBarUIController: SecondaryActionContainer not found.");
                return;
            }

            secondaryActionContainer = transform.gameObject;
        }

        if (tertiaryActionContainer == null)
        {
            Transform transform = this.transform.Find("TertiaryActionContainer");

            if (transform == null)
            {
                Debug.LogWarning("BottomBarUIController: TertiaryActionContainer not found.");
                return;
            }

            tertiaryActionContainer = transform.gameObject;
        }
    }
}
