using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    private Player player;

    [SerializeField] private WeaponData currentWeaponData;
    public WeaponData CurrentWeaponData => currentWeaponData;

    public GameObject CurrentWeaponInstance { get; private set; }

    private GameObject lastSpawnedWeaponPrefab;

    private void Awake()
    {
        player = GetComponent<Player>();

        if (player == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.Awake(): Player component is missing!");
        }
    }

    public void SetAimIKOffsets()
    {
        player.PlayerIKController.SetGunHoldOffset(CurrentWeaponData.aimIKOffsets);
    }

    public GameObject SpawnWeaponInWeaponHand()
    {
        if (CurrentWeaponData == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.SpawnWeaponInWeaponHand(): CurrentWeaponData is not set!");
            return null;
        }

        if (CurrentWeaponInstance == null || lastSpawnedWeaponPrefab != CurrentWeaponData.weaponPrefab)
        {
            if (CurrentWeaponInstance != null)
            {
                Destroy(CurrentWeaponInstance);
            }

            CurrentWeaponInstance = Instantiate(
                CurrentWeaponData.weaponPrefab,
                player.WeaponHand.transform
            );

            lastSpawnedWeaponPrefab = CurrentWeaponData.weaponPrefab;
        }

        return CurrentWeaponInstance;
    }

    public void DespawnWeaponInWeaponHand()
    {
        if (CurrentWeaponData == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.DespawnWeaponInWeaponHand(): CurrentWeaponData is not set!");
            return;
        }

        if (CurrentWeaponInstance != null)
        {
            Weapon weaponScript = CurrentWeaponInstance.GetComponent<Weapon>();

            if (weaponScript != null)
            {
                weaponScript.StopMuzzleEffect();
            }

            Destroy(CurrentWeaponInstance);
            CurrentWeaponInstance = null;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.DespawnWeaponInWeaponHand(): No weapon instance to despawn!");
        }
    }

    public void PlayMuzzleEffect()
    {
        if (CurrentWeaponInstance == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.PlayMuzzleEffect(): No weapon instance to play muzzle effect on!");
            return;
        }

        Weapon weaponScript = CurrentWeaponInstance.GetComponent<Weapon>();

        if (weaponScript == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.PlayMuzzleEffect(): Weapon script not found on the weapon instance!");
            return;
        }

        weaponScript.PlayMuzzleEffect();
    }

    public void StopMuzzleEffect()
    {
        if (CurrentWeaponInstance == null)
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.StopMuzzleEffect(): No weapon instance to stop muzzle effect on!");
            return;
        }

        Weapon weaponScript = CurrentWeaponInstance.GetComponent<Weapon>();

        if (weaponScript == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.StopMuzzleEffect(): Weapon script not found on the weapon instance!");
            return;
        }

        weaponScript.StopMuzzleEffect();
    }

    public void PlayGunshotSound()
    {
        if (CurrentWeaponInstance == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.PlayGunshotSound(): No weapon instance to play gunshot sound on!");
            return;
        }

        Weapon weaponScript = CurrentWeaponInstance.GetComponent<Weapon>();

        if (weaponScript == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.PlayGunshotSound(): Weapon script not found on the weapon instance!");
            return;
        }

        weaponScript.PlayGunshotSound();
    }

    public void SetRecoilIKSettings()
    {
        if (CurrentWeaponData == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.ApplyIKRecoilSettingsToComponent(): CurrentWeaponData is not set!");
            return;
        }
        if (player == null || player.Recoil == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.ApplyIKRecoilSettingsToComponent(): Player or IKRecoil component is missing!");
            return;
        }

        var recoil = player.Recoil;
        var data = CurrentWeaponData;
        recoil.ikRecoilWeight = data.ikRecoilWeight;
        recoil.aimIKSolvedLast = data.aimIKSolvedLast;
        recoil.handedness = (IKRecoil.Handedness)data.handedness;
        recoil.twoHanded = data.twoHanded;
        recoil.recoilWeight = data.recoilWeight;
        recoil.magnitudeRandom = data.magnitudeRandom;
        recoil.rotationRandom = data.rotationRandom;
        recoil.handRotationOffset = data.handRotationOffset;
        recoil.blendTime = data.blendTime;
        recoil.offsetSettings = new IKRecoil.OffsetSettings {
            offset = data.offsets.offset,
            additivity = data.offsets.additivity,
            maxAdditiveOffsetMag = data.offsets.maxAdditiveOffsetMag
        };
        // Map EffectorLinks
        if (data.effectorLinks != null)
        {
            recoil.effectorLinks = new IKRecoil.EffectorLink[data.effectorLinks.Length];
            for (int i = 0; i < data.effectorLinks.Length; i++)
            {
                var src = data.effectorLinks[i];
                recoil.effectorLinks[i] = new IKRecoil.EffectorLink {
                    effector = src.effector,
                    weight = src.weight
                };
            }
        }
    }
}
