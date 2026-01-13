using UnityEngine;

public class CharacterLampFill : MonoBehaviour
{
    [Header("Refs")]
    public Light characterLampLight;     // Directional light affecting ONLY characters (Rendering Layer)
    public Transform lampsRoot;            // Parent of lamp transforms (or anchors)

    [Header("Tuning")]
    public float maxRange = 8f;            // beyond this, intensity -> 0
    public float maxIntensity = 0.6f;      // intensity when right under lamp
    public float rotationSpeed = 12f;
    public float intensitySpeed = 10f;

    void Update()
    {
        if (!characterLampLight || !lampsRoot || lampsRoot.childCount == 0) return;

        // Find nearest lamp anchor
        Transform nearest = null;
        float bestSqr = float.PositiveInfinity;
        Vector3 p = transform.position;

        for (int i = 0; i < lampsRoot.childCount; i++)
        {
            var t = lampsRoot.GetChild(i);
            float sqr = (t.position - p).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; nearest = t; }
        }

        if (!nearest) return;

        float dist = Mathf.Sqrt(bestSqr);
        float t01 = Mathf.Clamp01(1f - dist / maxRange);          // 1 near, 0 far
        float targetIntensity = maxIntensity * t01;

        // Aim directional as if lamp is lighting the player
        Vector3 dir = (p - nearest.position).normalized;          // lamp -> player
        if (dir.sqrMagnitude < 1e-6f) dir = Vector3.down;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

        characterLampLight.transform.rotation =
            Quaternion.Slerp(characterLampLight.transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        characterLampLight.intensity =
            Mathf.Lerp(characterLampLight.intensity, targetIntensity, intensitySpeed * Time.deltaTime);
    }
}
