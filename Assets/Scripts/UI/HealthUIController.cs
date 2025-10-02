using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UndeadSurvivalGame.UI
{
    public class HealthUIController : MonoBehaviour
    {
        // Reference to the health bar UI element
        public MMProgressBar healthBar;

        [SerializeField]
        private CanvasGroup canvasGroup;

        // a value between 0 and 100, maybe in our game that'd be our main character's health value
        [Title("Debug Controls")]
        [Range(0f, 100f)]
        [PropertySpace]
        [InfoBox("These controls are for debugging the health bar in the Inspector.")]
        public float Value;

        // a test button to display in our inspector and let us call the ChangeBarValue method
        [MMInspectorButton("ChangeBarValue")]
        public bool ChangeBarValueBtn;

        void ChangeBarValue()
        {
            healthBar.UpdateBar(Value, 0f, 100f);
        }

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                Debug.LogError("HealthUIController: CanvasGroup component is not assigned and could not be found!", this);
            }
        }

        public void Initialize(float initialHealthPercentage)
        {
            if (healthBar == null)
            {
                Debug.LogError("HealthUIController: Health bar is not assigned!");
                return;
            }

            // Initialize the health bar with the initial health percentage
            UpdateHealthBar(initialHealthPercentage);
        }

        public void UpdateHealthBar(float value)
        {
            Value = value;
            healthBar.UpdateBar(value, 0f, 100f);
        }
        
        public void SetVisibility(bool isVisible)
        {
            if (canvasGroup == null)
            {
                Debug.LogError("HealthUIController: CanvasGroup is not assigned!");
                return;
            }

            canvasGroup.alpha = isVisible ? 1f : 0f;
            canvasGroup.interactable = isVisible;
            canvasGroup.blocksRaycasts = isVisible;
        }
    }
}
