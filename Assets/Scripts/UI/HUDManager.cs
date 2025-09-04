using UndeadSurvivalGame.PlayerSystems;
using Unity.AppUI.UI;
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
        private ProximityUI[] proximityUIs;

        [SerializeField]
        private InteractionSensor interactionSensor;

        private void Awake()
        {
            SetupProximityUIs();
            SetupInteractionSensor();
            SetupPlayerMenuUIController();
            SetupCurrentWeaponUIController();
            SetupHealthUIController();
            SetupTogglePlayerMenuHandler();
        }

        private void SetupProximityUIs()
        {
            proximityUIs = FindObjectsByType<ProximityUI>(FindObjectsSortMode.None);
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
        }

        private void HideProximityUIs(bool isMenuOpen)
        {
            foreach (var proximityUI in proximityUIs)
            {
                if (isMenuOpen)
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
    }
}
