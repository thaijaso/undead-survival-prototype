using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public class IconCaptureController : MonoBehaviour
{
    public Transform target;         // assign the prefab root here
    public Camera iconCamera;        // your orthographic camera
    public int size = 512;
    public float padding = 1.1f;     // 1.05–1.2 feels good
    public string outputFolder = "Assets/Icons";
    public string fileName = "AmmoBox_icon.png";

    [ContextMenu("Fit Camera To Target")]
    public void FitCameraToTarget()
    {
        if (!target || !iconCamera) return;

        int layer = LayerMask.NameToLayer("IconPreview");
        foreach (var t in target.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;

        var renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);

        iconCamera.orthographic = true;
        iconCamera.cullingMask = 1 << layer;
        iconCamera.clearFlags = CameraClearFlags.SolidColor; // alpha from camera bg
        iconCamera.backgroundColor = new Color(0,0,0,0);

        float extent = Mathf.Max(b.extents.x, b.extents.y, b.extents.z);
        iconCamera.orthographicSize = extent * padding;

        // Aim from +Z toward -Z; keep subject centered
        iconCamera.transform.position = b.center + Vector3.forward * 5f;
        iconCamera.transform.rotation = Quaternion.Euler(0,180f,0);
    }

#if UNITY_EDITOR
    [ContextMenu("Capture PNG")]
    public void CapturePng()
    {
        if (!iconCamera) return;

        var rt = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32)
        { antiAliasing = 4, useMipMap = false, autoGenerateMips = false };

        var prev = RenderTexture.active;
        iconCamera.targetTexture = rt;
        RenderTexture.active = rt;
        iconCamera.Render();

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
        tex.ReadPixels(new Rect(0,0,size,size), 0,0);
        tex.Apply();

        iconCamera.targetTexture = null;
        RenderTexture.active = prev;
        rt.Release();

        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            var parts = outputFolder.Split('/');
            string path = "";
            for (int i = 0; i < parts.Length; i++)
            {
                path = i == 0 ? parts[0] : $"{path}/{parts[i]}";
                if (i == 0) continue;
                if (!AssetDatabase.IsValidFolder(path))
                    AssetDatabase.CreateFolder(path.Substring(0, path.LastIndexOf('/')), parts[i]);
            }
        }

        string savePath = Path.Combine(outputFolder, fileName).Replace("\\","/");
        File.WriteAllBytes(savePath, tex.EncodeToPNG());
        DestroyImmediate(tex);

        AssetDatabase.ImportAsset(savePath);
        var ti = (TextureImporter)AssetImporter.GetAtPath(savePath);
        ti.textureType = TextureImporterType.Sprite;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.sRGBTexture = true;
        ti.spritePixelsPerUnit = 100;
        ti.SaveAndReimport();

        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Texture2D>(savePath));
        Debug.Log($"Icon saved to {savePath}");
    }
#endif
}