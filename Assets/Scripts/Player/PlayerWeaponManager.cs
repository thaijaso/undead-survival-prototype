using UnityEngine;
using UndeadSurvivalGame.Player;
using System;

public class PlayerWeaponManager : MonoBehaviour
{
    private Player player;

    [SerializeField]
    private WeaponConfig currentWeaponConfig;
    public WeaponConfig CurrentWeaponConfig => currentWeaponConfig;

    public GameObject CurrentWeaponGameObject { get; private set; }
    public Weapon CurrentWeaponScript { get; private set; }

    public bool IsWeaponHolstered { get; private set; } = false;
    private GameObject lastSpawnedWeaponPrefab;

    // Fire timer logic
    public float FireTimer { get; private set; } = 0f;

    public event Action<Weapon, WeaponConfig> OnWeaponSetup;
    public event Action OnBulletLoaded;
    public event Action OnBulletFired;

    private void Awake()
    {
        player = GetComponent<Player>();

        if (player == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.Awake(): Player component is missing!");
        }
    }

    private void Start()
    {
        if (CurrentWeaponConfig == null)
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.Start(): CurrentWeaponConfig is not set!");
            return;
        }

        if (CurrentWeaponScript == null)
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.Start(): CurrentWeaponScript is null! Is there a weapon gameobject in WeaponHand?");
            return;
        }
    }

    private void Update()
    {
        // Decrement fireTimer every frame
        if (FireTimer > 0f)
        {
            FireTimer -= Time.deltaTime;
            if (FireTimer < 0f)
                FireTimer = 0f;
        }
    }

    public bool IsInitialized()
    {
        return CurrentWeaponConfig != null && CurrentWeaponScript != null;
    }

    public void ResetFireTimer()
    {
        if (CurrentWeaponConfig == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.ResetFireTimer(): CurrentWeaponData is not set!");
            return;
        }

        FireTimer = CurrentWeaponConfig.fireRate;
    }

    public bool IsFireCooldownComplete()
    {
        return FireTimer <= 0f;
    }

    public bool IsChamberEmpty()
    {
        return CurrentWeaponScript.currentLoadedAmmo <= 0;
    }

    public bool IsChamberFull()
    {
        return CurrentWeaponScript.currentLoadedAmmo == CurrentWeaponConfig.maxAmmo;
    }

    public void SetAimIKOffsets()
    {
        player.PlayerIKController.SetGunHoldOffset(CurrentWeaponConfig.aimIKOffsets);
    }

    public GameObject SpawnWeaponInWeaponHand()
    {
        if (CurrentWeaponConfig == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.SpawnWeaponInWeaponHand(): CurrentWeaponData is not set!");
            return null;
        }

        if (CurrentWeaponGameObject == null || lastSpawnedWeaponPrefab != CurrentWeaponConfig.weaponPrefab)
        {
            if (CurrentWeaponGameObject != null)
            {
                Destroy(CurrentWeaponGameObject);
            }

            CurrentWeaponGameObject = Instantiate(
                CurrentWeaponConfig.weaponPrefab,
                player.WeaponHand.transform
            );

            lastSpawnedWeaponPrefab = CurrentWeaponConfig.weaponPrefab;

            // Assign the Weapon script reference
            CurrentWeaponScript = CurrentWeaponGameObject.GetComponent<Weapon>();
            if (CurrentWeaponScript == null)
            {
                Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager: Spawned weapon prefab does not have a Weapon script attached!");
            }
        }

        OnWeaponSetup?.Invoke(CurrentWeaponScript, CurrentWeaponConfig);
        return CurrentWeaponGameObject;
    }

    public void DespawnWeaponInWeaponHand()
    {
        if (CurrentWeaponConfig == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.DespawnWeaponInWeaponHand(): CurrentWeaponData is not set!");
            return;
        }

        if (CurrentWeaponGameObject != null)
        {
            Weapon weaponScript = CurrentWeaponGameObject.GetComponent<Weapon>();

            if (weaponScript != null)
            {
                weaponScript.StopMuzzleEffect();
            }

            Destroy(CurrentWeaponGameObject);
            CurrentWeaponGameObject = null;
            CurrentWeaponScript = null; // Clear reference
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.DespawnWeaponInWeaponHand(): No weapon instance to despawn!");
        }
    }

    public void PlayMuzzleEffect()
    {
        if (CurrentWeaponGameObject == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.PlayMuzzleEffect(): No weapon instance to play muzzle effect on!");
            return;
        }

        Weapon weaponScript = CurrentWeaponGameObject.GetComponent<Weapon>();

        if (weaponScript == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.PlayMuzzleEffect(): Weapon script not found on the weapon instance!");
            return;
        }

        if (weaponScript.muzzleEffect == null)
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.PlayMuzzleEffect(): Muzzle effect is not assigned on weapon instance!\n" +
                $"  - Did you assign a prefab instead of a scene instance?\n" +
                $"  - The muzzle effect should be a child of the weapon in the hierarchy, not a prefab asset.\n" +
                $"  - Weapon instance: {CurrentWeaponGameObject.name} (active: {CurrentWeaponGameObject.activeInHierarchy})");
            return;
        }

        // Check if the assigned ParticleSystem is a prefab asset (not in the scene hierarchy)
        if (!weaponScript.muzzleEffect.gameObject.scene.IsValid())
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.PlayMuzzleEffect(): The assigned muzzle effect is not part of the active scene!\n" +
                $"  - You probably assigned a prefab from the Project window. Assign the child ParticleSystem from the weapon hierarchy instead.\n" +
                $"  - MuzzleEffect object: {weaponScript.muzzleEffect.name}");
            return;
        }

        weaponScript.PlayMuzzleEffect();
    }

    public void StopMuzzleEffect()
    {
        if (CurrentWeaponGameObject == null)
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.StopMuzzleEffect(): No weapon instance to stop muzzle effect on!");
            return;
        }

        Weapon weaponScript = CurrentWeaponGameObject.GetComponent<Weapon>();

        if (weaponScript == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.StopMuzzleEffect(): Weapon script not found on the weapon instance!");
            return;
        }

        weaponScript.StopMuzzleEffect();
    }

    public void PlayGunshotSound()
    {
        if (CurrentWeaponGameObject == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.PlayGunshotSound(): No weapon instance to play gunshot sound on!");
            return;
        }

        Weapon weaponScript = CurrentWeaponGameObject.GetComponent<Weapon>();

        if (weaponScript == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.PlayGunshotSound(): Weapon script not found on the weapon instance!");
            return;
        }

        if (weaponScript.gunshot == null)
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.PlayGunshotSound(): Gunshot AudioSource is not assigned on weapon instance!\n" +
                $"  - Did you assign a prefab instead of a scene instance?\n" +
                $"  - The gunshot AudioSource should be a child of the weapon in the hierarchy, not a prefab asset.\n" +
                $"  - Weapon instance: {CurrentWeaponGameObject.name} (active: {CurrentWeaponGameObject.activeInHierarchy})");
            return;
        }

        // Check if the assigned AudioSource is a prefab asset (not in the scene hierarchy)
        if (!weaponScript.gunshot.gameObject.scene.IsValid())
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.PlayGunshotSound(): The assigned gunshot AudioSource is not part of the active scene!\n" +
                $"  - You probably assigned a prefab from the Project window. Assign the child AudioSource from the weapon hierarchy instead.\n" +
                $"  - Gunshot object: {weaponScript.gunshot.name}");
            return;
        }

        weaponScript.PlayGunshotSound();
    }

    public void PlayEmptyGunClick()
    {
        if (CurrentWeaponGameObject == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.PlayEmptyGunClick(): No weapon instance to play empty gun click on!");
            return;
        }

        Weapon weaponScript = CurrentWeaponGameObject.GetComponent<Weapon>();

        if (weaponScript == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.PlayEmptyGunClick(): Weapon script not found on the weapon instance!");
            return;
        }

        if (weaponScript.emptyGunClick == null)
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.PlayEmptyGunClick(): Empty gun click sound is not assigned on weapon instance!");
        }

        weaponScript.PlayEmptyGunClick();
    }

    public void SetRecoilIKSettings()
    {
        if (CurrentWeaponConfig == null)
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
        var weaponData = CurrentWeaponConfig;
        recoil.ikRecoilWeight = weaponData.ikRecoilWeight;
        recoil.aimIKSolvedLast = weaponData.aimIKSolvedLast;
        recoil.handedness = (RecoilIK.Handedness)weaponData.handedness;
        recoil.twoHanded = weaponData.twoHanded;
        recoil.recoilWeight = weaponData.recoilWeight;
        recoil.magnitudeRandom = weaponData.magnitudeRandom;
        recoil.rotationRandom = weaponData.rotationRandom;
        recoil.handRotationOffset = weaponData.handRotationOffset;
        recoil.blendTime = weaponData.blendTime;
        // Direct assignment now that types match
        if (weaponData.offsets != null && weaponData.offsets.Length > 0)
        {
            recoil.offsets = new RecoilIK.RecoilOffset[weaponData.offsets.Length];
            for (int i = 0; i < weaponData.offsets.Length; i++)
            {
                var src = weaponData.offsets[i];
                var dst = new RecoilIK.RecoilOffset();
                dst.offset = src.offset;
                dst.additivity = src.additivity;
                dst.maxAdditiveOffsetMag = src.maxAdditiveOffsetMag;
                if (src.effectorLinks != null && src.effectorLinks.Length > 0)
                {
                    dst.effectorLinks = new RecoilIK.RecoilOffset.EffectorLink[src.effectorLinks.Length];
                    for (int j = 0; j < src.effectorLinks.Length; j++)
                    {
                        var srcLink = src.effectorLinks[j];
                        var dstLink = new RecoilIK.RecoilOffset.EffectorLink();
                        dstLink.effector = srcLink.effector;
                        dstLink.weight = srcLink.weight;
                        dst.effectorLinks[j] = dstLink;
                    }
                }
                else
                {
                    dst.effectorLinks = null;
                }
                recoil.offsets[i] = dst;
            }
        }
        else
        {
            recoil.offsets = null;
            Debug.LogWarning($"[PlayerWeaponManager] No offsets found in WeaponData for {gameObject.name}.");
        }
    }

    public void DecrementLoadedAmmoCount()
    {
        if (CurrentWeaponScript != null)
        {
            CurrentWeaponScript.currentLoadedAmmo--;
            OnBulletFired?.Invoke();
            Debug.Log($"[{gameObject.name}] PlayerWeaponManager.DecrementAmmoCount(): Ammo decremented. Current ammo: {CurrentWeaponScript.currentLoadedAmmo}");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.DecrementAmmoCount(): CurrentWeaponScript is null! Cannot decrement ammo count.");
        }
    }

    public void IncrementLoadedAmmoCount()
    {
        if (CurrentWeaponScript != null)
        {
            CurrentWeaponScript.currentLoadedAmmo++;
            OnBulletLoaded?.Invoke();
            Debug.Log($"[{gameObject.name}] PlayerWeaponManager.IncrementLoadedAmmoCount(): Ammo incremented. Current ammo: {CurrentWeaponScript.currentLoadedAmmo}");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.IncrementAmmoCount(): CurrentWeaponScript is null! Cannot increment ammo count.");
        }
    }

    public bool CanReload()
    {
        if (CurrentWeaponScript != null)
        {
            int ammoTypeQuantity = player.PlayerInventory.GetAmmoTypeQuantity(CurrentWeaponConfig.ammoType);
            return CurrentWeaponScript.currentLoadedAmmo < CurrentWeaponConfig.maxAmmo
                && ammoTypeQuantity > 0
                && !IsChamberFull();
        }
        return false;
    }
}
