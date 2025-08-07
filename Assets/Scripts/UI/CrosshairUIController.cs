using UnityEngine;
using System.Collections;

public class CrosshairController : MonoBehaviour
{
    [Header("References")]
    public RectTransform topArm;
    public RectTransform bottomArm;
    public RectTransform leftArm;
    public RectTransform rightArm;

    [Header("Parameters")]
    public float baseDistance = 10f;
    public float spreadMultiplier = 50f;
    public float defaultDuration = 0.1f;

    private float currentSpread = 0f;
    private float targetSpread = 0f;
    private float bulletSpreadHorizontal = 0f;
    private float bulletSpreadVertical = 0f;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void SetCrosshair(float spread, float bulletSpreadH, float bulletSpreadV, float duration = -1f)
    {
        targetSpread = Mathf.Clamp01(spread);
        bulletSpreadHorizontal = bulletSpreadH;
        bulletSpreadVertical = bulletSpreadV;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateToTarget(duration > 0 ? duration : defaultDuration));
    }

    private IEnumerator AnimateToTarget(float duration)
    {
        float startSpread = currentSpread;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            currentSpread = Mathf.Lerp(startSpread, targetSpread, time / duration);
            UpdateArms();
            yield return null;
        }

        currentSpread = targetSpread;
        UpdateArms();
    }

    private void UpdateArms()
    {
        float horizontalOffset = baseDistance + (currentSpread * spreadMultiplier) + (bulletSpreadHorizontal * spreadMultiplier);
        float verticalOffset = baseDistance + (currentSpread * spreadMultiplier) + (bulletSpreadVertical * spreadMultiplier);

        if (topArm) topArm.anchoredPosition = new Vector2(0, verticalOffset);
        if (bottomArm) bottomArm.anchoredPosition = new Vector2(0, -verticalOffset);
        if (leftArm) leftArm.anchoredPosition = new Vector2(-horizontalOffset, 0);
        if (rightArm) rightArm.anchoredPosition = new Vector2(horizontalOffset, 0);
    }

    public void ResetCrosshair()
    {
        SetCrosshair(0f, 0f, 0f, 0.05f);
    }

    public bool IsCrosshairExpanded()
    {
        return currentSpread > 0.01f;
    }

    public void EnableCrosshair()
    {
        gameObject.SetActive(true);
    }

    public void DisableCrosshair()
    {
        gameObject.SetActive(false);
    }

    public void ExpandAndContractCrosshair(
        float expandSpread = 1f,
        float bulletSpreadH = 0f,
        float bulletSpreadV = 0f,
        float expandDuration = 0.1f,
        float holdDuration = 0.1f,
        float contractDuration = 0.1f
    )
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(ExpandAndContractRoutine(
            expandSpread,
            bulletSpreadH,
            bulletSpreadV,
            expandDuration,
            holdDuration,
            contractDuration
        ));
    }

    private IEnumerator ExpandAndContractRoutine(
        float expandSpread,
        float bulletSpreadH,
        float bulletSpreadV,
        float expandDuration,
        float holdDuration,
        float contractDuration
    )
    {
        // Expand
        SetCrosshair(expandSpread, bulletSpreadH, bulletSpreadV, expandDuration);
        yield return new WaitForSeconds(expandDuration + holdDuration);

        // Contract
        SetCrosshair(0f, 0f, 0f, contractDuration);
    }
}