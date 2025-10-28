using UnityEngine;

[DisallowMultipleComponent]
public class AlphaCutoffController : MonoBehaviour
{
    [Header("Camera")]
    public Camera mainCamera;

    [Tooltip("Renderers using All-in-1 3D Shader (URP variant).")]
    public Renderer[] targets;

    [Range(0f, 1f)] public float cutoff = 0f; // 0=opaque, 1=fully cut
    [Tooltip("All-in-1 3D Shader property name from the inspector tooltip.")]
    public string cutoffProperty = "_AlphaCutoffValue";

    MaterialPropertyBlock materialPropertyBlock;
    int cutoffId = -1;

    private float targetCutoff;

    void Awake()
    {
        materialPropertyBlock = new MaterialPropertyBlock();
        cutoffId = Shader.PropertyToID(cutoffProperty);

        // Optional: warn if first material doesn't expose it
        if (targets != null && targets.Length > 0 && targets[0])
        {
            var mat = targets[0].sharedMaterial;
            if (mat && !mat.HasProperty(cutoffId))
                Debug.LogWarning($"Material on {targets[0].name} has no '{cutoffProperty}' property.");
        }
    }

    void Update()
    {
        cutoff = Mathf.MoveTowards(cutoff, targetCutoff, Time.deltaTime * 4f);
    }

    void LateUpdate()
    {
        if (cutoffId == -1 || targets == null) return;

        foreach (var r in targets)
        {
            if (!r) continue;
            r.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetFloat(cutoffId, cutoff);
            r.SetPropertyBlock(materialPropertyBlock);
        }
    }

    public void SetCutoff(float value) {
        targetCutoff = Mathf.Clamp01(value);
    }
}
