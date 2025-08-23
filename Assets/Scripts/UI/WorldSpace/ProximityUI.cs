using UnityEngine;

public class ProximityUI : MonoBehaviour
{
    [SerializeField]
    private GameObject arrow;

    [SerializeField]
    private GameObject button;

    [SerializeField]
    private Transform player;  // Assign your Player root transform 
    public float showButtonRadius = 3f;
    public float showArrowRadius = 5f;

    void Awake()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.transform;
        }

        if (!player)  Debug.LogWarning($"[{name}] No Player found. Tag your player 'Player' or assign Transform.");
    }

    void Reset()
    {
        arrow = transform.Find("Arrow").gameObject;
        button = transform.Find("PCButtonWhite").gameObject; // TODO: implement controller support
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);

        // Enable if within range, disable otherwise
        bool shouldShowButton = dist <= showButtonRadius;
        bool shouldShowArrow = dist <= showArrowRadius;

        button.SetActive(shouldShowButton);
        arrow.SetActive(shouldShowArrow);
    }
}