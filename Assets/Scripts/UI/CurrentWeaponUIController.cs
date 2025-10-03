using System;
using System.Collections.Generic;
using TMPro;
using UndeadSurvivalGame.Gameplay;
using UndeadSurvivalGame.PlayerSystems;
using UnityEngine;
using UnityEngine.UI;

namespace UndeadSurvivalGame.UI
{
    public class CurrentWeaponUIController : MonoBehaviour
    {
        [SerializeField]
        private Image weaponIcon;
        public Image WeaponIcon => weaponIcon;

        [SerializeField]
        private List<GameObject> ammoListUI;

        [SerializeField]
        private TextMeshProUGUI ammoTotal;

        [SerializeField]
        private Inventory playerInventory;

        [SerializeField]
        private PlayerWeaponManager playerWeaponManager;

        [SerializeField]
        private CanvasGroup canvasGroup;

        void Awake()
        {
            if (playerInventory == null)
            {
                playerInventory = FindFirstObjectByType<Inventory>();

                if (playerInventory == null)
                {
                    Debug.LogError($"[{gameObject.name}] WeaponUIController: No Inventory found in scene!");
                }
            }

            if (playerWeaponManager == null)
            {
                playerWeaponManager = FindFirstObjectByType<PlayerWeaponManager>();

                if (playerWeaponManager == null)
                {
                    Debug.LogError($"[{gameObject.name}] WeaponUIController: No PlayerWeaponManager found in scene!");
                }
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                Debug.LogWarning($"[{gameObject.name}] WeaponUIController: No CanvasGroup component found on this GameObject.");
            }

            if (playerInventory != null)
            {
                playerInventory.OnInventoryChanged += UpdateCurrentWeaponTotalAmmoUI;
            }

            if (playerWeaponManager != null)
            {
                playerWeaponManager.OnWeaponSetup += UpdateCurrentWeaponUI;
                playerWeaponManager.OnBulletLoaded += AddBulletInBulletList;
                playerWeaponManager.OnBulletFired += RemoveBulletInBulletList;
            }
        }
        void OnDisable()
        {
            if (playerInventory != null)
            {
                playerInventory.OnInventoryChanged -= UpdateCurrentWeaponTotalAmmoUI;
            }

            if (playerWeaponManager != null)
            {
                playerWeaponManager.OnWeaponSetup -= UpdateCurrentWeaponUI;
                playerWeaponManager.OnBulletLoaded -= AddBulletInBulletList;
                playerWeaponManager.OnBulletFired -= RemoveBulletInBulletList;
            }
        }

        public void UpdateCurrentWeaponUI(Weapon weaponScript, WeaponConfig weaponConfig)
        {
            UpdateCurrentWeaponIcon(weaponConfig);
            UpdateBulletListUI(weaponScript.currentLoadedAmmo, weaponConfig.maxAmmo);
            UpdateCurrentWeaponTotalAmmoUI();
        }

        private void UpdateCurrentWeaponIcon(WeaponConfig weaponConfig)
        {
            if (weaponConfig != null && !string.IsNullOrEmpty(weaponConfig.currentWeaponIconPath))
            {
                string resourcePath = GetResourcesRelativePath(weaponConfig.currentWeaponIconPath);
                Debug.Log($"[{gameObject.name}] WeaponUIController.UpdateWeaponDisplay(): Loading icon from path: {resourcePath}");
                var sprite = Resources.Load<Sprite>(resourcePath);
                if (sprite != null)
                {
                    WeaponIcon.sprite = sprite;
                }
                else
                {
                    Debug.LogWarning($"[{gameObject.name}] WeaponUIController.UpdateWeaponDisplay(): Icon sprite not found at path: {weaponConfig.currentWeaponIconPath}");
                }
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] WeaponUIController.UpdateWeaponDisplay(): Invalid weapon data or icon path.");
            }
        }

        private void UpdateBulletListUI(int currentLoadedAmmo, int maxAmmo)
        {
            if (playerWeaponManager == null || playerWeaponManager.CurrentWeaponConfig == null)
            {
                Debug.LogWarning($"[{gameObject.name}] WeaponUIController.UpdateBulletListUI(): PlayerWeaponManager or CurrentWeaponConfig is not set.");
                return;
            }

            Debug.Log($"[{gameObject.name}] WeaponUIController.UpdateBulletListUI(): Updating ammo list UI with current loaded ammo: {currentLoadedAmmo}, max ammo: {maxAmmo}");

            // Enable toggles
            for (int index = 0; index < ammoListUI.Count; index++)
            {
                ammoListUI[index].SetActive(index < maxAmmo);
            }

            // Set toggle state 
            for (int index = 0; index < currentLoadedAmmo; index++)
            {
                if (index < ammoListUI.Count)
                {
                    Toggle toggle = ammoListUI[index].GetComponent<Toggle>();
                    if (toggle != null)
                    {
                        toggle.isOn = true;
                    }
                }
                else
                {
                    Debug.LogWarning($"[{gameObject.name}] WeaponUIController.UpdateBulletListUI(): Ammo index {index} exceeds ammo list UI count.");
                }
            }
        }

        private string GetResourcesRelativePath(string assetPath)
        {
            // Example: assetPath = "Assets/Resources/WeaponIcons/ICON_SM_Wep_Pistol_Revolver_01.png"
            const string resourcesPrefix = "Resources/";
            int startIndex = assetPath.IndexOf(resourcesPrefix, StringComparison.Ordinal);
            if (startIndex >= 0)
            {
                // +10 skips "Resources/"
                string relative = assetPath.Substring(startIndex + resourcesPrefix.Length);
                // Remove extension if present
                int extIndex = relative.LastIndexOf('.');
                if (extIndex > 0)
                    relative = relative.Substring(0, extIndex);
                return relative;
            }
            // If not found, fallback to original (may already be relative)
            return assetPath;
        }

        private void AddBulletInBulletList()
        {
            if (playerWeaponManager == null)
            {
                Debug.LogWarning($"[{gameObject.name}] WeaponUIController.AddBulletInBulletList(): PlayerWeaponManager is not set.");
                return;
            }

            int currentLoadedAmmo = playerWeaponManager.CurrentWeaponScript.currentLoadedAmmo;
            Debug.Log($"[{gameObject.name}] WeaponUIController.AddBulletInBulletList(): Current loaded ammo: {currentLoadedAmmo}");

            if (currentLoadedAmmo > 0 && currentLoadedAmmo < ammoListUI.Count)
            {
                Toggle toggle = ammoListUI[currentLoadedAmmo - 1].GetComponent<Toggle>();

                if (toggle != null)
                {
                    toggle.isOn = true;
                }
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] WeaponUIController.AddBulletInBulletList(): Invalid current loaded ammo: {currentLoadedAmmo}");
            }
        }

        private void DecrementTotalAmmo()
        {
            Debug.Log($"[{gameObject.name}] WeaponUIController.DecrementTotalAmmo(): Decrementing total ammo.");
        }

        public void RemoveBulletInBulletList()
        {
            if (playerWeaponManager == null)
            {
                Debug.LogWarning($"[{gameObject.name}] WeaponUIController.RemoveBulletInBulletList(): PlayerWeaponManager is not set.");
                return;
            }

            int currentLoadedAmmo = playerWeaponManager.CurrentWeaponScript.currentLoadedAmmo;
            Debug.Log($"[{gameObject.name}] WeaponUIController.RemoveBulletInBulletList(): Current loaded ammo: {currentLoadedAmmo}");

            if (currentLoadedAmmo >= 0 && currentLoadedAmmo < ammoListUI.Count)
            {
                Toggle toggle = ammoListUI[currentLoadedAmmo].GetComponent<Toggle>();

                if (toggle != null)
                {
                    toggle.isOn = false;
                }
                else
                {
                    Debug.LogWarning($"[{gameObject.name}] WeaponUIController.RemoveBulletInBulletList(): Toggle component not found on ammo UI at index {currentLoadedAmmo}.");
                }
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] WeaponUIController.RemoveBulletInBulletList(): Invalid current loaded ammo: {currentLoadedAmmo}");
            }
        }

        public void UpdateCurrentWeaponTotalAmmoUI()
        {
            if (playerInventory == null)
            {
                Debug.LogWarning($"[{gameObject.name}] WeaponUIController.UpdateTotalAmmoUI(): Player inventory is not set.");
                return;
            }

            if (playerWeaponManager == null)
            {
                Debug.LogWarning($"[{gameObject.name}] WeaponUIController.UpdateTotalAmmoUI(): PlayerWeaponManager is not set.");
                return;
            }

            if (playerWeaponManager.CurrentWeaponConfig == null)
            {
                Debug.Log($"[{gameObject.name}] WeaponUIController.UpdateTotalAmmoUI(): CurrentWeaponConfig is not set.");
                return;
            }

            int totalAmmo = playerInventory.GetAmmoTypeQuantity(playerWeaponManager.CurrentWeaponConfig.ammoType);
            // Update total ammo text field
            ammoTotal.text = $"{totalAmmo}";
            Debug.Log($"[{gameObject.name}] WeaponUIController.UpdateTotalAmmoUI(): Updating total ammo UI. Current total ammo: {totalAmmo}");
        }
        
        public void SetVisibility(bool isVisible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = isVisible ? 1f : 0f;
                canvasGroup.interactable = isVisible;
                canvasGroup.blocksRaycasts = isVisible;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] WeaponUIController.SetVisibility(): CanvasGroup component is not set.");
            }
        }
    }
}
