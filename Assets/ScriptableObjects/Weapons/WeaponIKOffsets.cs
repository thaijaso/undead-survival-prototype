using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "WeaponIKOffsets", menuName = "ScriptableObjects/Weapons/WeaponIKOffsets", order = 1)]
public class WeaponIKOffsets : ScriptableObject
{
	public Vector3 gunHoldOffset;
	public Vector3 leftHandOffset;
}
