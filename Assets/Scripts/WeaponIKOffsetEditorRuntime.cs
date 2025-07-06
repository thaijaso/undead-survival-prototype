using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WeaponIKOffsetEditorRuntime : MonoBehaviour
{
    public WeaponIKOffsets offsets;
    public Transform gunHoldTransform;
    public Transform leftHandTransform;
    public Camera playerCamera;

    // Recursively search for a child transform by name
    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            var result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
                Debug.LogWarning($"[{nameof(WeaponIKOffsetEditorRuntime)}] Could not find MainCamera in the scene.", this);
        }

        // Try to find the player root in the parent chain
        Transform playerRoot = null;
        var playerComponent = GetComponentInParent<Player>();
        if (playerComponent != null)
            playerRoot = playerComponent.transform;
        else
            playerRoot = transform.root;

        if (gunHoldTransform == null)
        {
            if (playerRoot != null)
                gunHoldTransform = FindDeepChild(playerRoot, "WeaponHand");
            if (gunHoldTransform == null)
                Debug.LogWarning($"[{nameof(WeaponIKOffsetEditorRuntime)}] Could not find 'WeaponHand' under player root (recursive search).", this);
        }
        if (leftHandTransform == null)
        {
            if (playerRoot != null)
                leftHandTransform = FindDeepChild(playerRoot, "hand.l");
            if (leftHandTransform == null)
                Debug.LogWarning($"[{nameof(WeaponIKOffsetEditorRuntime)}] Could not find 'hand.l' under player root (recursive search).", this);
        }
    }

    void OnDrawGizmos()
    {
    #if UNITY_EDITOR
        if (!Application.isPlaying || offsets == null || gunHoldTransform == null || playerCamera == null)
            return;

        // Draw handle for gun hold offset
        EditorGUI.BeginChangeCheck();
        Vector3 newGunHoldOffset = Handles.PositionHandle(gunHoldTransform.position, gunHoldTransform.rotation) - transform.position;
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(offsets, "Move Gun Hold Offset");
            offsets.gunHoldOffset = transform.InverseTransformPoint(newGunHoldOffset + transform.position);
            EditorUtility.SetDirty(offsets);
        }

        // Draw handle for left hand offset
        if (leftHandTransform != null)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newLeftHandOffset = Handles.PositionHandle(leftHandTransform.position, leftHandTransform.rotation) - transform.position;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(offsets, "Move Left Hand Offset");
                offsets.leftHandOffset = transform.InverseTransformPoint(newLeftHandOffset + transform.position);
                EditorUtility.SetDirty(offsets);
            }
        }

        // Draw debug line from gun to screen center
        Gizmos.color = Color.red;
        Gizmos.DrawLine(gunHoldTransform.position, playerCamera.transform.position + playerCamera.transform.forward * 100f);
    #endif
    }

    void Update()
    {
        if (offsets == null)
            return;
        if (gunHoldTransform != null)
            gunHoldTransform.localPosition = offsets.gunHoldOffset;
        if (leftHandTransform != null)
            leftHandTransform.localPosition = offsets.leftHandOffset;
    }
}
