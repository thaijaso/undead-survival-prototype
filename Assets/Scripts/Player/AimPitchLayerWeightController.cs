using UnityEngine;

public class AimPitchLayerWeightController : MonoBehaviour
{
    [SerializeField]
    private float blendSpeed = 12f;
    private Animator animator;
    private string layerName = "Aim Pitch Layer";
    private int layerIndex = -1;

    private float currentWeight = 0f;
    private float targetWeight = 0f;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();

            if (animator == null)
            {
                Debug.LogError($"[{gameObject.name}] AimPitchLayerWeightController.Start(): No Animator component found on this GameObject.");
            }
        }

        layerIndex = animator.GetLayerIndex(layerName);

        if (layerIndex == -1)
        {
            Debug.LogError($"[{gameObject.name}] AimPitchLayerWeightController.Start(): No layer named '{layerName}' found in the Animator.");
        }

        currentWeight = animator.GetLayerWeight(layerIndex);
        targetWeight = currentWeight;
    }

    public void SetWeight(float weight)
    {
        targetWeight = weight;
    }

    // Update is called once per frame
    void Update()
    {
        if (layerIndex != -1)
        {
            currentWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * blendSpeed);
            animator.SetLayerWeight(layerIndex, currentWeight);
        }
    }
}
