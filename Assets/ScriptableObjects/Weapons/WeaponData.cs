using UnityEngine;
using Sirenix.OdinInspector;


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

    // IK Recoil settings (moved from PlayerTemplate)
    [TabGroup("IK Recoil")]
    [Range(0f, 1f)]
    public float ikRecoilWeight = 1f;

    [TabGroup("IK Recoil")]
    [InfoBox("If true, Aim IK is solved after Full Body IK.")]
    public bool aimIKSolvedLast = false;

    public enum Handedness { Right, Left }
    [TabGroup("IK Recoil")]
    public Handedness handedness = Handedness.Right;

    [TabGroup("IK Recoil")]
    public bool twoHanded = true;

    [TabGroup("IK Recoil")]
    [InfoBox("Curve for blending recoil weight over time.")]
    public AnimationCurve recoilWeight = AnimationCurve.Linear(0, 0.1f, 1, 0.1f);

    [TabGroup("IK Recoil")]
    [InfoBox("Random magnitude applied to recoil.")]
    public float magnitudeRandom = 0.1f;

    [TabGroup("IK Recoil")]
    [InfoBox("Random rotation applied to recoil.")]
    public Vector3 rotationRandom = new Vector3(0, 30, 0);

    [TabGroup("IK Recoil")]
    [InfoBox("Offset for hand rotation during recoil.")]
    public Vector3 handRotationOffset = Vector3.zero;

    [TabGroup("IK Recoil")]
    [InfoBox("Blend time for recoil effect.")]
    public float blendTime = 0.1f;

    [TabGroup("IK Recoil")]
    public RecoilIK.RecoilOffset[] offsets;
}