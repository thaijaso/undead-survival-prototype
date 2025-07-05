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
