using UnityEngine;

public abstract class LayerWeightController : MonoBehaviour
{
    private Animator animator;
    private float currentWeight = 0f;
    private float targetWeight = 0f;

    protected abstract float blendSpeed { get; }
    protected abstract string layerName { get; }
    protected int layerIndex = -1;

    protected virtual void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();

            if (animator == null)
            {
                Debug.LogError($"[{gameObject.name}] LayerWeightController.Start(): No Animator component found on this GameObject.");
            }
        }

        layerIndex = animator.GetLayerIndex(layerName);

        if (layerIndex == -1)
        {
            Debug.LogError($"[{gameObject.name}] LayerWeightController.Awake(): No layer named '{layerName}' found in the Animator.");
        }
    }

    public void SetWeight(float weight)
    {
        targetWeight = weight;
    }

    void Update()
    {
        if (layerIndex != -1)
        {
            currentWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * blendSpeed);
            animator.SetLayerWeight(layerIndex, currentWeight);
        }
    }
}
