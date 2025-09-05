using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace UndeadSurvivalGame.UI
{
    public class AnimateAlpha : MonoBehaviour
    {
        public Image Image;
        public float FadeDuration = 1f;
        public bool IsFading { get; private set; }

        private Coroutine fadeCoroutine;

        private void Awake()
        {
            if (Image == null)
            {
                Image = GetComponent<Image>();
            }

            if (Image == null)
            {
                Debug.LogWarning($"[{gameObject.name}] AnimateAlpha.Awake(): No Image component found!");
            }
        }

        private void OnDisable()
        {
            StopContinuousFade();
        }

        public void StartContinuousFade()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(FadeAlphaLoop());
            IsFading = true;
        }

        public void StopContinuousFade()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            IsFading = false;
        }

        private IEnumerator FadeAlpha(float from, float to, float duration)
        {
            if (Image != null)
            {
                float elapsed = 0f;
                Color color = Image.color;

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    color.a = Mathf.Lerp(from, to, elapsed / duration);
                    Image.color = color;
                    yield return null;
                }

                color.a = to;
                Image.color = color;
            }
        }

        private IEnumerator FadeAlphaLoop()
        {
            if (Image != null)
            {
                while (true)
                {
                    yield return FadeAlpha(0f, 1f, FadeDuration / 2f);
                    yield return FadeAlpha(1f, 0f, FadeDuration / 2f);
                }
            }
        }
    }
}
