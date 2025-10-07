using UnityEngine;

public class UpperBodyLayerWeightController : LayerWeightController
{
    [SerializeField]
    private float blendSpeedValue = 12f;
    protected override float blendSpeed => blendSpeedValue;

    [SerializeField]
    private string layerNameValue = "Upperbody Layer";
    protected override string layerName => layerNameValue;
}
