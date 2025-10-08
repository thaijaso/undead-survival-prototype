using Synty.Interface.Extensions;
using UnityEngine;

namespace UndeadSurvivalGame.UI
{ 
    public class OverlayUIController : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem bloodParticleSystem;

        void Awake()
        {
            if (bloodParticleSystem != null)
            {
                var rectTransform = GetComponent<RectTransform>();
                var particleSystem = bloodParticleSystem.GetComponent<ParticleSystem>();
                if (rectTransform != null && particleSystem != null)
                {
                    var shape = particleSystem.shape;
                    shape.scale = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 1f);
                    Debug.Log($"Blood particle system shape scale set to: {shape.scale}");
                }
            }
        }

        public void PlayBloodEffect()
        {
            if (bloodParticleSystem != null)
            {
                UIParticleSystem uiParticleSystem = bloodParticleSystem.GetComponent<UIParticleSystem>();
                uiParticleSystem.StartParticleEmission();
                Debug.Log("Blood particle system played.");
            }
            else
            {
                Debug.LogWarning("Blood particle system is not assigned.");
            }
        }
    }
}
