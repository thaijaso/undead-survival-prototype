using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUIController : MonoBehaviour
{
    [SerializeField]
    private Image weaponIcon;
    public Image WeaponIcon => weaponIcon;

    [SerializeField]
    private TextMeshProUGUI currentLoadedAmmo;

    [SerializeField]
    private TextMeshProUGUI ammoTotal;

    [SerializeField]
    private Inventory playerInventory;

    [SerializeField]
    private PlayerWeaponManager playerWeaponManager;

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
    }

    void OnEnable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += UpdateTotalAmmoUI;
        }
    }

    void OnDisable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= UpdateTotalAmmoUI;
        }
    }

    public void UpdateWeaponDisplay(WeaponConfig weaponData, int currentLoadedAmmo, int ammoTotal)
    {
        // Update weapon icon if path is valid
        if (weaponData != null && !string.IsNullOrEmpty(weaponData.currentWeaponIconPath))
        {
            string resourcePath = GetResourcesRelativePath(weaponData.currentWeaponIconPath);
            Debug.Log($"[{gameObject.name}] WeaponUIController.UpdateWeaponDisplay(): Loading icon from path: {resourcePath}");
            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
            {
                WeaponIcon.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] WeaponUIController.UpdateWeaponDisplay(): Icon sprite not found at path: {weaponData.currentWeaponIconPath}");
            }
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] WeaponUIController.UpdateWeaponDisplay(): Invalid weapon data or icon path.");
        }

        // Update ammo text fields
        //this.currentLoadedAmmo.text = currentLoadedAmmo.ToString(); TODO: enable ammo list
        this.ammoTotal.text = $"{ammoTotal}";
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

    public void UpdateCurrentLoadedAmmoUI(int currentLoadedAmmo)
    {
        // Update ammo text fields
        this.currentLoadedAmmo.text = currentLoadedAmmo.ToString();
    }

    public void UpdateTotalAmmoUI()
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

        int totalAmmo = playerInventory.GetAmmoTypeQuantity(playerWeaponManager.CurrentWeaponConfig.ammoType);
        // Update total ammo text field
        ammoTotal.text = $"{totalAmmo}";
    }
}
