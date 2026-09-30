using DG.Tweening;
using H8.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthView : MonoBehaviour, IHealthView
{
    [SerializeField] private Slider slider;
    [SerializeField] private RectTransform canvasRoot;

    [Header("Optional juice targets")]
    [SerializeField] private StylizedGraphic fillGraphic;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private RectTransform barRoot;
    [SerializeField] private Transform heart;

    [Header("Fill palette per health band")]
    [SerializeField] private StylizedPalette highPalette = StylizedPalette.GoGreen;
    [SerializeField] private StylizedPalette midPalette = StylizedPalette.CoinGold;
    [SerializeField] private StylizedPalette lowPalette = StylizedPalette.Coral;
    [SerializeField] private StylizedStyle fillStyle = StylizedStyle.Juicy;

    [Header("Low health effect")]
    [SerializeField] private StylizedEffect lowHpEffect = StylizedEffect.Pulse;
    [SerializeField, Range(0f, 1f)] private float lowHpEffectStrength = 0.45f;
    [SerializeField, Range(0f, 6f)] private float lowHpEffectSpeed = 2.4f;

    [Header("Animation")]
    [SerializeField, Min(0f)] private float fillTweenTime = 0.35f;
    [SerializeField, Range(0f, 1f)] private float midHpThreshold = 0.5f;
    [SerializeField, Range(0f, 1f)] private float lowHpThreshold = 0.25f;
    [SerializeField, Range(0f, 1f)] private float damagePunchStrength = 0.12f;
    [SerializeField, Range(0f, 1f)] private float damageShakeStrength = 0.04f;
    [SerializeField, Min(0f)] private float damageFeedbackDuration = 0.3f;

    private Canvas canvas;
    private Camera billboardCamera;
    private Tween fillTween;
    private Tween heartPulse;
    private Tween damagePunch;
    private Tween damageShake;
    private Vector3 barBaseScale = Vector3.one;
    private Vector2 barBaseAnchoredPosition;
    private StylizedPalette appliedPalette;
    private float lastPercent = -1f;
    private bool hasAppliedPalette;
    private bool isLowEffectActive;

    private void Awake()
    {
        if (barRoot == null)
        {
            barRoot = transform as RectTransform;
        }

        if (barRoot != null)
        {
            barBaseScale = barRoot.localScale;
            barBaseAnchoredPosition = barRoot.anchoredPosition;
        }

        canvas = canvasRoot != null
            ? canvasRoot.GetComponent<Canvas>()
            : GetComponent<Canvas>();

        // The prefab's slider is authored in absolute HP units; the view works in
        // 0..1 so the fill never depends on whatever max the prefab was saved with.
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
        }
    }

    public void SetHealth(float current, float max)
    {
        float percent = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        bool isFirstValue = lastPercent < 0f;

        ApplyFill(percent, isFirstValue);
        ApplyFillPalette(percent);
        ApplyLowHpEffect(percent);
        ApplyLabel(current, max);

        // Suppressed on the first value: spawning at full health is not a hit.
        if (!isFirstValue && percent < lastPercent - 0.0001f)
        {
            PlayDamageFeedback();
        }

        UpdateHeartPulse(percent);
        lastPercent = percent;
    }

    private void ApplyFill(float percent, bool instant)
    {
        if (slider == null)
        {
            return;
        }

        fillTween?.Kill();

        if (instant || fillTweenTime <= 0f)
        {
            slider.value = percent;
            return;
        }

        fillTween = slider.DOValue(percent, fillTweenTime)
            .SetEase(Ease.OutCubic)
            .SetLink(gameObject);
    }

    private void ApplyFillPalette(float percent)
    {
        if (fillGraphic == null)
        {
            return;
        }

        StylizedPalette target = percent > midHpThreshold ? highPalette
            : percent > lowHpThreshold ? midPalette
            : lowPalette;

        if (hasAppliedPalette && appliedPalette == target)
        {
            return;
        }

        fillGraphic.SetPalette(target, fillStyle);
        appliedPalette = target;
        hasAppliedPalette = true;
    }

    private void ApplyLowHpEffect(float percent)
    {
        if (fillGraphic == null || lowHpEffect == StylizedEffect.None)
        {
            return;
        }

        bool shouldRun = IsLow(percent);

        if (shouldRun == isLowEffectActive)
        {
            return;
        }

        if (shouldRun)
        {
            fillGraphic.SetEffect(lowHpEffect, lowHpEffectStrength, lowHpEffectSpeed);
        }
        else
        {
            fillGraphic.SetEffect(StylizedEffect.None);
        }

        isLowEffectActive = shouldRun;
    }

    private void ApplyLabel(float current, float max)
    {
        if (healthText == null)
        {
            return;
        }

        healthText.text = $"{Mathf.CeilToInt(Mathf.Max(0f, current))}/{Mathf.CeilToInt(max)}";
    }

    private void PlayDamageFeedback()
    {
        if (barRoot == null)
        {
            return;
        }

        damagePunch?.Kill();
        damageShake?.Kill();
        barRoot.localScale = barBaseScale;
        barRoot.anchoredPosition = barBaseAnchoredPosition;

        damagePunch = barRoot
            .DOPunchScale(barBaseScale * damagePunchStrength, damageFeedbackDuration, 8, 0.7f)
            .SetLink(gameObject);

        float shakeDistance = barRoot.rect.width * barBaseScale.x * damageShakeStrength;
        damageShake = barRoot
            .DOShakeAnchorPos(damageFeedbackDuration, shakeDistance, 18, 90, false, true)
            .SetLink(gameObject);
    }

    private void UpdateHeartPulse(float percent)
    {
        if (heart == null)
        {
            return;
        }

        if (IsLow(percent))
        {
            if (heartPulse == null || !heartPulse.IsActive())
            {
                heartPulse = heart.DOScale(1.2f, 0.45f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject);
            }

            return;
        }

        if (heartPulse != null)
        {
            heartPulse.Kill();
            heartPulse = null;
            heart.localScale = Vector3.one;
        }
    }

    private bool IsLow(float percent)
    {
        return percent > 0f && percent <= lowHpThreshold;
    }

    private void LateUpdate()
    {
        if (canvasRoot == null)
        {
            return;
        }

        Camera cam = ResolveCamera();

        if (cam == null)
        {
            return;
        }

        canvasRoot.rotation = cam.transform.rotation;
    }

    private Camera ResolveCamera()
    {
        if (billboardCamera != null)
        {
            return billboardCamera;
        }

        billboardCamera = Camera.main;

        if (billboardCamera != null && canvas != null &&
            canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
        {
            canvas.worldCamera = billboardCamera;
        }

        return billboardCamera;
    }

    private void OnDestroy()
    {
        fillTween?.Kill();
        heartPulse?.Kill();
        damagePunch?.Kill();
        damageShake?.Kill();
    }
}
