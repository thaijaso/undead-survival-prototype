// SmoothFramePacing.cs
using UnityEngine;

/// <summary>
/// Stabilizes frame pacing for smoother camera motion and less aliasing.
/// Works only in builds (ignored in Editor).
/// </summary>
public class SmoothFramePacing : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplyFrameCap()
    {
        QualitySettings.vSyncCount = 0;         // disable vsync completely
        Application.targetFrameRate = 120;      // pick a sane cap (60 / 100 / 120)
        Debug.Log("[FrameCap] vSync off, targetFrameRate=120");
    }
}
