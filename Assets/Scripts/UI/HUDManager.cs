using UndeadSurvivalGame.PlayerSystems;
using UnityEngine;

namespace UndeadSurvivalGame.UI
{

    public class HUDManager : MonoBehaviour
    {
        [SerializeField]
        private PlayerMenuUIController playerMenuUIController;

        [SerializeField]
        private CurrentWeaponUIController currentWeaponUIController;

        [SerializeField]
        private HealthUIController healthUIController;

        [SerializeField]
        private InteractionSensor interactionSensor;

        [SerializeField]
        private MenuOverlayController menuOverlayController;

        private void Awake()
        {
            SetupInteractionSensor();
            SetupPlayerMenuUIController();
            SetupCurrentWeaponUIController();
            SetupHealthUIController();
            SetupTogglePlayerMenuHandler();
            SetupMenuOverlayController();
        }

        private void SetupPlayerMenuUIController()
        {
            if (playerMenuUIController == null)
            {
                playerMenuUIController = FindFirstObjectByType<PlayerMenuUIController>(FindObjectsInactive.Include);
            }

            if (playerMenuUIController == null)
            {
                Debug.LogWarning("HUDManager: No PlayerMenuUIController found in scene.");
            }
        }

        private void SetupCurrentWeaponUIController()
        {
            if (currentWeaponUIController == null)
            {
                currentWeaponUIController = FindFirstObjectByType<CurrentWeaponUIController>(FindObjectsInactive.Include);
            }

            if (currentWeaponUIController == null)
            {
                Debug.LogWarning("HUDManager: No CurrentWeaponUIController found in scene.");
            }
        }

        private void SetupHealthUIController()
        {
            if (healthUIController == null)
            {
                healthUIController = FindFirstObjectByType<HealthUIController>(FindObjectsInactive.Include);
            }

            if (healthUIController == null)
            {
                Debug.LogWarning("HUDManager: No HealthUIController found in scene.");
            }
        }

        private void SetupInteractionSensor()
        {
            if (interactionSensor == null)
            {
                interactionSensor = FindFirstObjectByType<InteractionSensor>(FindObjectsInactive.Include);
            }

            if (interactionSensor == null)
            {
                Debug.LogWarning("HUDManager: No InteractionSensor found in scene.");
            }
        }


        private void SetupTogglePlayerMenuHandler()
        {
            if (playerMenuUIController != null && interactionSensor != null)
            {
                playerMenuUIController.OnPlayerMenuToggled += HandlePlayerMenuToggled;
            }
        }

        private void HandlePlayerMenuToggled(bool isMenuOpen)
        {
            interactionSensor.enabled = !isMenuOpen;
            HideProximityUIs(isMenuOpen);
            TogglePlayerHealthUI(isMenuOpen);
            ToggleCurrentWeaponUI(isMenuOpen);
            ToggleMenuOverlay(isMenuOpen);
        }

        private void HideProximityUIs(bool isMenuOpen)
        {
            foreach (var proximityUI in interactionSensor.ProximityUIs)
            {
                if (isMenuOpen && proximityUI != null)
                {
                    proximityUI.HideAllPrompts();
                }
            }
        }

        private void TogglePlayerHealthUI(bool isMenuOpen)
        {
            if (healthUIController != null)
            {
                healthUIController.gameObject.SetActive(!isMenuOpen);
            }
        }

        private void ToggleCurrentWeaponUI(bool isMenuOpen)
        {
            if (currentWeaponUIController != null)
            {
                currentWeaponUIController.gameObject.SetActive(!isMenuOpen);
            }
        }

        private void SetupMenuOverlayController()
        {
            if (menuOverlayController == null)
            {
                menuOverlayController = FindFirstObjectByType<MenuOverlayController>(FindObjectsInactive.Include);
            }

            if (menuOverlayController == null)
            {
                Debug.LogWarning("HUDManager: No MenuOverlayController found in scene.");
            }
        }

        private void ToggleMenuOverlay(bool isMenuOpen)
        {
            if (menuOverlayController != null && menuOverlayController.OverlayImage != null)
            {
                menuOverlayController.OverlayImage.enabled = isMenuOpen;
            }
        }
    }
}
