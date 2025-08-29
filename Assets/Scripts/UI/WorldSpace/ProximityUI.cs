using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

public class ProximityUI : MonoBehaviour
{
    public string itemPickupTemplate = $"Pickup {{itemName}} x{{quantity}}";
    public string inventoryFullTemplate = "Inventory Full";

    [SerializeField]
    private GameObject arrow;

    [SerializeField]
    private GameObject button;

    [SerializeField]
    private GameObject textBackground;

    [SerializeField]
    private TextMeshProUGUI textMeshPro;

    [SerializeField]
    private MMF_Player InventoryFullFeedback;

    private ItemPickupInteractable itemPickupInteractable;


    void Awake()
    {
        SetupArrow();
        SetupButton();
        SetupTextBackground();
        SetupItemPickupInteractable();
        SetupInventoryFullFeedback();
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

    private void SetupItemPickupInteractable()
    {
        itemPickupInteractable = GetComponent<ItemPickupInteractable>();

        if (itemPickupInteractable != null)
        {
            itemPickupInteractable.OnPickupAllFailed += SetPickupText;
            itemPickupInteractable.OnInventoryFull += HandleInventoryFullFeedbacks;
        }
    }

    private void SetupInventoryFullFeedback()
    {
        if (InventoryFullFeedback == null)
        {
            InventoryFullFeedback = GetComponentInChildren<MMF_Player>(true);
        }

        if (InventoryFullFeedback == null)
        {
            Debug.LogWarning($"ProximityUI.SetupInventoryFullFeedback(): {name} has no InventoryFullFeedback assigned or found in children.");
        }
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

    public void SetPickupText(string itemName, int quantity)
    {
        string text = itemPickupTemplate
            .Replace("{itemName}", itemName)
            .Replace("{quantity}", quantity.ToString());
        SetText(text);
    }

    private void SetText(string text)
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = text;
        }
    }

    private void HandleInventoryFullFeedbacks()
    {
        PlayInventoryFullFeedback();
        SetInventoryFullText();
    }

    private void PlayInventoryFullFeedback()
    {
        if (InventoryFullFeedback != null)
        {
            InventoryFullFeedback.PlayFeedbacks();
        }
    }

    private void SetInventoryFullText()
    {
        SetText(inventoryFullTemplate);
    }   
}