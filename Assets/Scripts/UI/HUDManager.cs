using UndeadSurvivalGame.PlayerSystems;
using UnityEngine;

namespace UndeadSurvivalGame.UI
{

    public class HUDManager : MonoBehaviour
    {
        [SerializeField]
        private PlayerMenuUIController playerMenuUIController;

        [SerializeField]
        private ProximityUI[] proximityUIs;

        [SerializeField]
        private InteractionSensor interactionSensor;

        private void Awake()
        {
            SetupProximityUIs();
            SetupInteractionSensor();
            SetupPlayerMenuUIController();
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

            foreach (var proximityUI in proximityUIs)
            {
                if (isMenuOpen)
                {
                    proximityUI.HideAllPrompts();
                }
            }
        }
    }
}
