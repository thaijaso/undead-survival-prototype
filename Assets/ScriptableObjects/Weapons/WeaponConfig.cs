#if UNITY_EDITOR
using UnityEditor;
#endif

using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivalGame.Gameplay
{

    // WeaponData.cs
    // This ScriptableObject holds all static configuration, stats, and IK/recoil settings for a weapon type.
    // It is referenced by Weapon MonoBehaviours and can be reused across multiple weapon prefabs/instances.

    [CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/Weapons/Weapon Data", order = 0)]
    public class WeaponConfig : ScriptableObject
    {
        public enum WeaponType { Pistol, Rifle, Melee }
        [Header("Weapon Type")]
        public WeaponType weaponType = WeaponType.Pistol;
        public string weaponName; // Display name for the weapon

        public GameObject weaponPrefab; // Prefab to instantiate for this weapon
        public WeaponIKOffsets aimIKOffsets; // IK offsets for aiming pose

        // Animation / weapon visual recoil settings
        public float animationRecoilMagnitude = 0.5f; // Used for animation-based recoil

        // Camera recoil settings
        public float cameraRecoilX = 0f; // Camera kick X
        public float cameraRecoilY = 0f; // Camera kick Y
        public float cameraRecoilZ = 0f; // Camera kick Z
        public float cameraRecoilSnapiness = 0f; // How quickly camera snaps
        public float cameraRecoilReturnSpeed = 0f; // How quickly camera returns

        // Aim offsets for recoil
        public float bulletSpreadHorizontal = 0.5f; // Horizontal bullet spread
        public float bulletSpreadVertical = 0.5f;   // Vertical bullet spread

        public float fireRate = 0.1f; // Shots per second
        public float range = 100f;     // Max bullet range
        public int damage = 1;         // Damage per shot
        public float weaponSway = 1f;  // Sway amount
        public float impactForce = 10f;// Force applied on hit
        public float bulletSpeed = 20f;// Bullet velocity
        public int maxAmmo = 6; // Max ammo capacity

        public AmmoType ammoType; // Ammo type used by this weapon

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllWeaponIconPaths))]
#endif
        public string currentWeaponIconPath;

#if UNITY_EDITOR
        // This method provides the dropdown options
        private static IEnumerable<string> GetAllWeaponIconPaths()
        {
            // Example: Search for all sprites in a folder and return their asset paths
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Resources/WeaponIcons" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                yield return path;
            }
        }
#endif

        // Weapon firing mode
        [Header("Firing Mode")]
        [Tooltip("If true, weapon fires continuously while held. If false, fires only on click.")]
        public bool isAutomatic = true;

        // IK Recoil settings (used by RecoilIK)
        [TabGroup("IK Recoil")]
        [Range(0f, 1f)]
        public float ikRecoilWeight = 1f; // Master weight for IK recoil

        [TabGroup("IK Recoil")]
        [InfoBox("If true, Aim IK is solved after Full Body IK.")]
        public bool aimIKSolvedLast = false;

        public enum Handedness { Right, Left }
        [TabGroup("IK Recoil")]
        public Handedness handedness = Handedness.Right; // Which hand holds the weapon

        [TabGroup("IK Recoil")]
        public bool twoHanded = true; // Is this a two-handed weapon?

        [TabGroup("IK Recoil")]
        [InfoBox("Curve for blending recoil weight over time.")]
        public AnimationCurve recoilWeight = AnimationCurve.Linear(0, 0.1f, 1, 0.1f); // Recoil weight curve

        [TabGroup("IK Recoil")]
        [InfoBox("Random magnitude applied to recoil.")]
        public float magnitudeRandom = 0.1f; // Randomness in recoil magnitude

        [TabGroup("IK Recoil")]
        [InfoBox("Random rotation applied to recoil.")]
        public Vector3 rotationRandom = new Vector3(0, 30, 0); // Randomness in recoil rotation

        [TabGroup("IK Recoil")]
        [InfoBox("Offset for hand rotation during recoil.")]
        public Vector3 handRotationOffset = Vector3.zero; // Hand rotation offset during recoil

        [TabGroup("IK Recoil")]
        [InfoBox("Blend time for recoil effect.")]
        public float blendTime = 0.1f; // How quickly recoil blends in

        [TabGroup("IK Recoil")]
        public RecoilIK.RecoilOffset[] offsets; // Array of offsets for IK recoil (per effector)

        private void OnValidate()
        {
            if (offsets == null || offsets.Length == 0)
            {
                var defaultEffectorLinks = new RecoilIK.RecoilOffset.EffectorLink[]
                {
                    new() { effector = FullBodyBipedEffector.RightHand, weight = 1f },
                    new() { effector = FullBodyBipedEffector.RightShoulder, weight = 0.5f },
                    new() { effector = FullBodyBipedEffector.Body, weight = 0.1f }
                };
                var defaultOffset = new RecoilIK.RecoilOffset
                {
                    offset = new Vector3(0.1f, 0.6f, 0.1f),
                    additivity = 1f,
                    maxAdditiveOffsetMag = 0.2f,
                    effectorLinks = defaultEffectorLinks
                };
                offsets = new RecoilIK.RecoilOffset[] { defaultOffset };
            }
        }
    }
}