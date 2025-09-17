using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

namespace UndeadSurvivalGame.UI
{
    public class ContextMenuButtonUI : MonoBehaviour
    {
        [SerializeField]
        private ContextMenuButtonEventHandler contextMenuButtonEventHandler;

        [SerializeField]
        private Image hoverBackgroundImage;

        [SerializeField]
        private AnimateAlpha animateAlpha;

        [SerializeField]
        private MMF_Player hoverSoundFeedback;

        public ContextMenuButtonEventHandler EventHandler => contextMenuButtonEventHandler;

        private void Awake()
        {
            SetupHoverBackgroundImage();
            SetupHoverSoundFeedback();
        }

        private void SetupHoverBackgroundImage()
        {
            if (hoverBackgroundImage == null)
            {
                hoverBackgroundImage = transform.Find("HoverBackgroundImage").GetComponent<Image>();
                animateAlpha = hoverBackgroundImage.GetComponent<AnimateAlpha>();
            }

            if (hoverBackgroundImage != null)
            {
                hoverBackgroundImage.enabled = false;
            }
            else
            {
                Debug.LogWarning("ContextMenuButtonUI.SetupHoverBackgroundImage() - HoverBackgroundImage component is not assigned and could not be found.", this);
            }

            if (animateAlpha == null)
            {
                Debug.LogWarning("ContextMenuButtonUI.SetupHoverBackgroundImage() - AnimateAlpha component is not assigned and could not be found.", this);
            }
        }

        private void SetupHoverSoundFeedback()
        {
            if (hoverSoundFeedback == null)
            {
                hoverSoundFeedback = GameObject.Find("PlayerMenu/SoundFeedbacks/HoverSound").GetComponent<MMF_Player>();
            }

            if (hoverSoundFeedback == null)
            {
                Debug.LogWarning("ContextMenuButtonUI.SetupHoverSoundFeedback() - HoverSoundFeedback component is not assigned and could not be found.", this);
            }
        }

        private void AnimateAlpha()
        {
            if (animateAlpha != null)
            {
                animateAlpha.StartContinuousFade();
            }
            else
            {
                Debug.LogWarning("AnimateAlpha component is not assigned.", this);
            }
        }

        public void PlayHoverSound()
        {
            if (hoverSoundFeedback != null)
            {
                hoverSoundFeedback.PlayFeedbacks();
            }
            else
            {
                Debug.LogWarning("HoverSoundFeedback component is not assigned.", this);
            }
        }

        private void EnableHoverBackground()
        {
            if (hoverBackgroundImage != null)
            {
                hoverBackgroundImage.enabled = true;
            }
            else
            {
                Debug.LogWarning("HoverBackgroundImage component is not assigned.", this);
            }
        }

        public void PlayHoverAnimation()
        {
            EnableHoverBackground();
            AnimateAlpha();
        }

        public void StopHoverAnimation()
        {
            DisableHoverBackground();
            if (animateAlpha != null)
            {
                animateAlpha.StopContinuousFade();
            }
            else
            {
                Debug.LogWarning("AnimateAlpha component is not assigned.", this);
            }
        }

        private void DisableHoverBackground()
        {
            if (hoverBackgroundImage != null)
            {
                hoverBackgroundImage.enabled = false;
            }
            else
            {
                Debug.LogWarning("HoverBackgroundImage component is not assigned.", this);
            }
        }
    }
}
