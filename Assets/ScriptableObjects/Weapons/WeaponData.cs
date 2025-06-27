using UnityEngine;


[CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/Weapons/Weapon Data", order = 0)]
public class WeaponData : ScriptableObject
{
    public string weaponName;

    public GameObject weaponPrefab;
    public WeaponIKOffsets aimIKOffsets;

    // Animation / weapon visual recoil settings
    public float animationRecoilMagnitude = 0.5f;

    // Camera recoil settings
    public float cameraRecoilX = 0f;

    public float cameraRecoilY = 0f;

    public float cameraRecoilZ = 0f;

    public float cameraRecoilSnapiness = 0f;
    public float cameraRecoilReturnSpeed = 0f;

    // Aim offsets for recoil
    public float bulletSpreadHorizontal = 0.5f;
    public float bulletSpreadVertical = 0.5f;

    public float fireRate = 0.1f;

    public float range = 100f;
    public int damage = 1;
    public float weaponSway = 1f;

    public float impactForce = 10f;

    public float bulletSpeed = 20f;
}