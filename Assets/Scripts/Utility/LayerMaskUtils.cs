using System.Collections.Generic;
using UnityEngine;


public static class LayerMaskUtils
{
    /// Returns a comma-separated list of layer names in this mask, e.g. "Wall, CameraCollision, CameraDoor".
    public static string ToNames(LayerMask mask)
    {
        if (mask.value == 0) return "<None>";
        var names = new List<string>(8);

        int bits = mask.value;
        for (int i = 0; i < 32; i++)
        {
            int bit = 1 << i;
            if ((bits & bit) == 0) continue;

            string name = LayerMask.LayerToName(i);
            // LayerToName returns "" if the layer slot has no name assigned
            names.Add(string.IsNullOrEmpty(name) ? $"Layer{i}" : name);
        }

        return string.Join(", ", names);
    }

    /// Handy one-liner for logs: "[label]  (value=1536)  Wall, CameraCollision"
    public static string Describe(string label, LayerMask mask)
    {
        return $"[{label}] (value={mask.value}) {ToNames(mask)}";
    }
}
