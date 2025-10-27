using UnityEngine;

public static class FrameCap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Apply()
    {
        QualitySettings.vSyncCount = 0; // targetFrameRate only works reliably when VSync is off
        int cap = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value); // e.g., 119.88 -> 120
        if (cap <= 0) cap = 60;
        Application.targetFrameRate = cap;
        Debug.Log($"[FrameCap] VSync=OFF, targetFrameRate={Application.targetFrameRate}");
    }
}
