using System;
using UndeadSurvivalGame.Gameplay;
using UnityEngine;

namespace UndeadSurvivalGame.PlayerSystems
{
    public class PlayerWeaponManager : MonoBehaviour
    {
        public Item CurrentWeaponItem { get; private set; }
        public WeaponConfig CurrentWeaponConfig { get; private set; }
        public GameObject CurrentWeaponGameObject { get; private set; }
        public Weapon CurrentWeaponScript { get; private set; }
        public bool IsWeaponHolstered { get; private set; } = false;
        public float FireTimer { get; private set; } = 0f;

        public event Action<Weapon, WeaponConfig> OnWeaponSetup;
        public event Action OnBulletLoaded;
        public event Action OnBulletFired;

        private GameObject lastSpawnedWeaponPrefab;
        private Player player;

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

        private GameObject SpawnWeaponPrefab(Item weapon)
        {
            if (weapon.prefab == null)
            {
                Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.SpawnWeaponPrefab(): Weapon prefab is null!");
                return null;
            }

            return Instantiate(weapon.prefab, player.WeaponHand.transform);
        }

        private Weapon GetWeaponScript(GameObject weaponObject)
        {
            var weaponScript = weaponObject.GetComponent<Weapon>();
            if (weaponScript == null)
            {
                Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.GetWeaponScript(): Weapon script not found on prefab!");
            }
            return weaponScript;
        }

        private WeaponConfig GetWeaponConfig(Weapon weaponScript)
        {
            if (weaponScript == null)
                return null;

            if (weaponScript.WeaponConfig == null)
            {
                Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.GetWeaponConfig(): WeaponConfig not assigned in Weapon script!");
                return null;
            }
            return weaponScript.WeaponConfig;
        }

        private void AssignCurrentWeapon(GameObject weaponObject, Weapon weaponScript, WeaponConfig weaponConfig)
        {
            CurrentWeaponGameObject = weaponObject;
            CurrentWeaponScript = weaponScript;
            CurrentWeaponConfig = weaponConfig;
            lastSpawnedWeaponPrefab = weaponConfig.weaponPrefab;
        }

        private void CleanupPreviousWeapon()
        {
            if (CurrentWeaponGameObject != null)
            {
                Destroy(CurrentWeaponGameObject);
                CurrentWeaponGameObject = null;
                CurrentWeaponScript = null;
            }
        }

        private GameObject SpawnWeaponInWeaponHand(Item weapon)
        {
            CleanupPreviousWeapon();

            var weaponObject = SpawnWeaponPrefab(weapon);
            if (weaponObject == null) return null;

            var weaponScript = GetWeaponScript(weaponObject);
            if (weaponScript == null) return weaponObject;

            var weaponConfig = GetWeaponConfig(weaponScript);
            if (weaponConfig == null) return weaponObject;

            AssignCurrentWeapon(weaponObject, weaponScript, weaponConfig);
            DisableInteractionScripts(weaponScript);
            DisableCollider(weaponObject);

            OnWeaponSetup?.Invoke(CurrentWeaponScript, CurrentWeaponConfig);
            return weaponObject;
        }

        private void DisableInteractionScripts(Weapon weaponScript)
        {
            DisableProximityUI(weaponScript);
            DisableItemPickupInteractable(weaponScript);
        }

        private void DisableProximityUI(Weapon weaponScript)
        {
            if (weaponScript.proximityUI != null)
            {
                weaponScript.proximityUI.enabled = false;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.DisableProximityUI(): ProximityUI component not found on weapon prefab!");
            }
        }

        private void DisableItemPickupInteractable(Weapon weaponScript)
        {
            if (weaponScript.itemPickupInteractable != null)
            {
                weaponScript.itemPickupInteractable.enabled = false;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.DisableItemPickupInteractable(): ItemPickupInteractable component not found on weapon prefab!");
            }
        }

        private void DisableCollider(GameObject weaponInstance)
        {
            Collider weaponCollider = weaponInstance.GetComponent<Collider>();

            if (weaponCollider != null)
            {
                weaponCollider.enabled = false;
                Debug.Log($"[{gameObject.name}] Disabled collider on weapon instance: {weaponInstance.name}");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.DisableCollider(): Collider not found on weapon prefab!");
            }
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

        public void EquipFirstWeaponFound()
        {
            Item weapon = player.PlayerInventory.GetFirstWeapon();
            if (weapon != null)
            {
                EquipWeaponItem(weapon);
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] PlayerWeaponManager.EquipFirstWeaponFound(): No weapon found in inventory to equip.");
            }
        }

        public void EquipWeaponItem(Item weapon)
        {
            if (weapon == null)
            {
                Debug.LogError($"[{gameObject.name}] PlayerWeaponManager.EquipWeaponItem(): Provided weapon item is null!");
                return;
            }

            CurrentWeaponItem = weapon;
            SpawnWeaponInWeaponHand(weapon);
        }
        
        public bool IsItemEquipped(Item item)
        {
            bool isEquipped = item != null && CurrentWeaponItem != null && item == CurrentWeaponItem;
            Debug.Log($"[{gameObject.name}] PlayerWeaponManager.IsItemEquipped(): Checking if item '{item?.itemName}' is equipped. CurrentWeaponItem: '{CurrentWeaponItem?.itemName}' isEquipped: {isEquipped}");
            return isEquipped;
        }
    }
}
