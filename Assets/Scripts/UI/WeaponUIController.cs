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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateWeaponDisplay(WeaponData weaponData, int currentLoadedAmmo, int ammoTotal)
    {
        // Update weapon icon if path is valid
        if (weaponData != null && !string.IsNullOrEmpty(weaponData.weaponIconPath))
        {
            string resourcePath = GetResourcesRelativePath(weaponData.weaponIconPath);
            Debug.Log($"[{gameObject.name}] WeaponUIController.UpdateWeaponDisplay(): Loading icon from path: {resourcePath}");
            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
            {
                WeaponIcon.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] WeaponUIController.UpdateWeaponDisplay(): Icon sprite not found at path: {weaponData.weaponIconPath}");
            }
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] WeaponUIController.UpdateWeaponDisplay(): Invalid weapon data or icon path.");
        }

        // Update ammo text fields
        this.currentLoadedAmmo.text = currentLoadedAmmo.ToString();
        this.ammoTotal.text = $"/ {ammoTotal}";
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

    public void UpdateTotalAmmoUI(int ammoTotal)
    {
        // Update total ammo text field
        this.ammoTotal.text = $"/ {ammoTotal}";
    }
}
