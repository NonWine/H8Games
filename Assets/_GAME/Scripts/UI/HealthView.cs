using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// World-space health bar. Ported from the Fusion project's HUD HealthView and
// merged with the billboarding this project's bar already did, so a single
// component still drives the HealthUI prefab that both the hero and UnitRoot
// nest.
//
// Everything under "Optional juice targets" is null-checked: the existing
// HealthUI prefab only wires the slider, and a bar with no fill image, label or
// heart must keep working exactly as the plain bar did.
public class HealthView : MonoBehaviour, IHealthView
{
    [SerializeField] private Slider slider;
    [SerializeField] private RectTransform canvasRoot;

    [Header("Optional juice targets")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private RectTransform barRoot;
    [SerializeField] private Transform heart;
    [SerializeField] private Sprite highColor;
    [SerializeField] private Sprite midColor;
    [SerializeField] private Sprite lowColor;

    [Header("Animation")]
    [SerializeField, Min(0f)] private float fillTweenTime = 0.35f;
    [SerializeField, Range(0f, 1f)] private float midHpThreshold = 0.5f;
    [SerializeField, Range(0f, 1f)] private float lowHpThreshold = 0.25f;
    [SerializeField, Min(0f)] private float damagePunchStrength = 0.12f;
    [SerializeField, Min(0f)] private float damageShakeStrength = 10f;
    [SerializeField, Min(0f)] private float damageFeedbackDuration = 0.3f;

    private float lastPercent = -1f;
    private Tween fillTween;
    private Tween heartPulse;

    private void Awake()
    {
        if (barRoot == null)
        {
            barRoot = transform as RectTransform;
        }

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
        ApplyFillSprite(percent);
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

    private void ApplyFillSprite(float percent)
    {
        if (fillImage == null)
        {
            return;
        }

        Sprite target = percent > midHpThreshold ? highColor
            : percent > lowHpThreshold ? midColor
            : lowColor;

        if (target != null)
        {
            fillImage.sprite = target;
        }
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

        barRoot.DOPunchScale(Vector3.one * damagePunchStrength, damageFeedbackDuration, 8, 0.7f)
            .SetLink(gameObject);
        barRoot.DOShakeAnchorPos(damageFeedbackDuration, damageShakeStrength, 18, 90, false, true)
            .SetLink(gameObject);
    }

    private void UpdateHeartPulse(float percent)
    {
        if (heart == null)
        {
            return;
        }

        bool isLow = percent > 0f && percent <= lowHpThreshold;

        if (isLow)
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

    private void LateUpdate()
    {
        if (canvasRoot == null || Camera.main == null)
        {
            return;
        }

        canvasRoot.forward = Camera.main.transform.forward;
    }

    private void OnDestroy()
    {
        fillTween?.Kill();
        heartPulse?.Kill();
    }
}
