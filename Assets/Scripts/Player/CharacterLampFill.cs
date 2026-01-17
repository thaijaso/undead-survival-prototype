using System.Collections.Generic;
using UnityEngine;

public class CharacterLampFill : MonoBehaviour
{
    [Header("Refs (optional)")]
    [Tooltip("Directional light affecting ONLY characters (Rendering Layer). If null, we will try to auto-find it.")]
    public Light characterLampLight;

    [Tooltip("Optional: If set, we only consider lights under this root. If null, we scan the whole scene.")]
    public Transform lampsRoot;

    [Header("Search Filters")]
    public bool includeInactiveLights = false;

    [Tooltip("If true, point lights are considered lamp sources.")]
    public bool includePointLights = true;

    [Tooltip("If true, spot lights are considered lamp sources.")]
    public bool includeSpotLights = true;

    [Tooltip("Optional: if not empty, only lights whose name contains this (case-insensitive) are considered.")]
    public string nameMustContain = ""; // e.g. "Lamp"

    [Header("Tuning")]
    public float maxRange = 8f;        // beyond this, intensity -> 0
    public float maxIntensity = 0.6f;  // intensity when right under lamp
    public float rotationSpeed = 12f;
    public float intensitySpeed = 10f;

    [Header("Maintenance")]
    [Tooltip("How often to rescan the scene for lights (seconds). 0 = only once on enable.")]
    public float rescanInterval = 1.0f;

    private readonly List<Light> _candidates = new List<Light>();
    private float _nextScanTime;

    void OnEnable()
    {
        AutoAssignCharacterLampLightIfNeeded();
        RefreshCandidateLights();
        _nextScanTime = Time.time + Mathf.Max(0.01f, rescanInterval);
    }

    void Update()
    {
        if (!characterLampLight) AutoAssignCharacterLampLightIfNeeded();

        if (rescanInterval > 0f && Time.time >= _nextScanTime)
        {
            RefreshCandidateLights();
            _nextScanTime = Time.time + rescanInterval;
        }

        if (!characterLampLight || _candidates.Count == 0) return;

        Vector3 p = transform.position;

        // Find nearest *light* (not transform)
        Light nearest = null;
        float bestSqr = float.PositiveInfinity;

        for (int i = 0; i < _candidates.Count; i++)
        {
            Light L = _candidates[i];
            if (!L) continue; // destroyed
            if (!L.enabled) continue;

            // Distance from light position to player
            float sqr = (L.transform.position - p).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; nearest = L; }
        }

        if (!nearest) return;

        float dist = Mathf.Sqrt(bestSqr);

        // You can optionally also respect the real light range if you want:
        float effectiveMaxRange = maxRange;
        if (nearest.type == LightType.Point || nearest.type == LightType.Spot)
            effectiveMaxRange = Mathf.Min(maxRange, nearest.range);

        float t01 = Mathf.Clamp01(1f - dist / effectiveMaxRange);
        float targetIntensity = maxIntensity * t01;

        // Aim directional as if lamp is lighting the player
        Vector3 dir = (p - nearest.transform.position).normalized; // lamp -> player
        if (dir.sqrMagnitude < 1e-6f) dir = Vector3.down;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

        characterLampLight.transform.rotation =
            Quaternion.Slerp(characterLampLight.transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        characterLampLight.intensity =
            Mathf.Lerp(characterLampLight.intensity, targetIntensity, intensitySpeed * Time.deltaTime);
    }

    private void RefreshCandidateLights()
    {
        _candidates.Clear();

#if UNITY_2023_1_OR_NEWER
        var all = Object.FindObjectsByType<Light>(includeInactiveLights ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
                                                 FindObjectsSortMode.None);
#else
        // Older Unity fallback: FindObjectsOfType doesn't include inactive.
        var all = Object.FindObjectsOfType<Light>();
#endif

        for (int i = 0; i < all.Length; i++)
        {
            Light L = all[i];
            if (!L) continue;
            if (L == characterLampLight) continue;

            // If lampsRoot is set, only consider lights under it
            if (lampsRoot && !L.transform.IsChildOf(lampsRoot)) continue;

            // Filter by type
            if (L.type == LightType.Directional) continue; // your lamp sources should be point/spot
            if (!includePointLights && L.type == LightType.Point) continue;
            if (!includeSpotLights && L.type == LightType.Spot) continue;

            // Optional name filter
            if (!string.IsNullOrWhiteSpace(nameMustContain))
            {
                if (L.name.IndexOf(nameMustContain, System.StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
            }

            _candidates.Add(L);
        }
    }

    private void AutoAssignCharacterLampLightIfNeeded()
    {
        if (characterLampLight) return;

        // 1) Try by name (matches your hierarchy screenshot)
        var byName = GameObject.Find("Character Lamp Light");
        if (byName)
        {
            var l = byName.GetComponent<Light>();
            if (l && l.type == LightType.Directional)
            {
                characterLampLight = l;
                return;
            }
        }

        // 2) Fallback: find a directional light that is NOT the "Sun Light" (common naming)
#if UNITY_2023_1_OR_NEWER
        var all = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        var all = Object.FindObjectsOfType<Light>();
#endif
        for (int i = 0; i < all.Length; i++)
        {
            var l = all[i];
            if (!l) continue;
            if (l.type != LightType.Directional) continue;

            // skip obvious "Sun"
            if (l.name.IndexOf("sun", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;

            characterLampLight = l;
            return;
        }
    }
}
