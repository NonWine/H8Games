using DG.Tweening;
using H8.UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LevelProgressView : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Graphic fillGraphic;

    [Header("Fill Animation")]
    [SerializeField, Min(0.01f)] private float fillDuration = 0.45f;
    [SerializeField] private Ease fillEase = Ease.InOutSine;

    [Header("Drain Animation")]
    [SerializeField, Min(0.1f)] private float drainDuration = 0.75f;
    [SerializeField] private Ease drainEase = Ease.InOutCubic;
    [SerializeField, Min(0.01f)] private float drainAnticipationDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float drainSettleDuration = 0.28f;
    [SerializeField] private float drainTiltAngle = -2.5f;
    [SerializeField] private Vector3 drainSquashScale = new Vector3(1.08f, 0.88f, 1f);
    [SerializeField] private Vector3 drainImpactScale = new Vector3(0.92f, 1.12f, 1f);

    [Header("Full Feedback")]
    [SerializeField] private float pulseScale = 1.06f;
    [SerializeField] private float pulseDuration = 0.4f;
    [SerializeField, Min(0f)] private float pulseInterval = 0.6f;
    [SerializeField] private Color pulseColor = new Color(1f, 0.88f, 0.25f, 1f);
    [SerializeField] private Vector3 shakeStrength = new Vector3(3f, 2f, 0f);

    [Header("Debug")]
    [SerializeField] private KeyCode debugKey = KeyCode.L;
    [SerializeField] private KeyCode debugFillKey = KeyCode.K;

    private LevelProgressTracker progressTracker;
    private SignalBus signalBus;
    private RectTransform rectTransform;
    private StylizedGraphic stylizedFill;
    private Vector3 baseScale;
    private Vector2 basePosition;
    private Quaternion baseRotation;
    private Color baseTopColor;
    private Color baseBottomColor;
    private Color baseGraphicColor;
    private bool colorsCaptured;

    private Tween fillTween;
    private Sequence drainSequence;
    private Sequence fullFeedbackSequence;
    private float targetProgress;
    private bool needsUpdate;
    private bool isFullFeedbackActive;
    private bool isTransitioning;

    [Inject]
    public void Construct(LevelProgressTracker progressTracker, SignalBus signalBus)
    {
        this.progressTracker = progressTracker;
        this.signalBus = signalBus;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            baseScale = rectTransform.localScale;
            basePosition = rectTransform.anchoredPosition;
            baseRotation = rectTransform.localRotation;
        }
        else
        {
            baseScale = transform.localScale;
            basePosition = transform.localPosition;
            baseRotation = transform.localRotation;
        }

        CaptureBaseColors();
    }

    private void Start()
    {
        CaptureBaseColors();

        signalBus?.Subscribe<LevelTransitionCompletedSignal>(HandleTransitionCompleted);
        signalBus?.Subscribe<LevelRestartedSignal>(HandleLevelRestarted);

        if (progressTracker != null)
        {
            progressTracker.ProgressChanged += SetTargetProgress;
            targetProgress = progressTracker.Progress;
            if (slider != null)
            {
                slider.value = targetProgress;
            }
        }
    }

    private void OnDestroy()
    {
        if (progressTracker != null)
        {
            progressTracker.ProgressChanged -= SetTargetProgress;
        }

        signalBus?.TryUnsubscribe<LevelTransitionCompletedSignal>(HandleTransitionCompleted);
        signalBus?.TryUnsubscribe<LevelRestartedSignal>(HandleLevelRestarted);

        fillTween?.Kill();
        drainSequence?.Kill();
        fullFeedbackSequence?.Kill();
        ResetFillColor();
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(debugFillKey))
        {
            drainSequence?.Kill();
            fillTween?.Kill();
            if (slider != null)
            {
                slider.value = 1f;
            }
            StartFullFeedback();
        }
        else if (Input.GetKeyDown(debugKey))
        {
            DebugPlayDrain();
        }
#endif
    }

    private void SetTargetProgress(float progress)
    {
        if (isTransitioning)
        {
            targetProgress = progress;
            return;
        }

        if (progress <= 0.0001f && slider != null && slider.value > 0.5f)
        {
            isTransitioning = true;
            targetProgress = progress;
            return;
        }

        targetProgress = progress;
        needsUpdate = true;
    }

    private void LateUpdate()
    {
        if (!needsUpdate || slider == null || isTransitioning)
        {
            return;
        }

        needsUpdate = false;
        fillTween?.Kill();

        if (targetProgress <= 0.0001f || targetProgress < slider.value)
        {
            StopFullFeedback();
            slider.value = targetProgress;
            return;
        }

        fillTween = slider.DOValue(targetProgress, fillDuration)
            .SetEase(fillEase)
            .SetLink(gameObject)
            .OnComplete(CheckFullStatus);
    }

    private void CheckFullStatus()
    {
        if (slider != null && slider.value >= 0.999f)
        {
            StartFullFeedback();
        }
        else
        {
            StopFullFeedback();
        }
    }

    private void StartFullFeedback()
    {
        if (isFullFeedbackActive || rectTransform == null)
        {
            return;
        }

        CaptureBaseColors();

        isFullFeedbackActive = true;
        fullFeedbackSequence?.Kill();

        fullFeedbackSequence = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(rectTransform.DOScale(baseScale * pulseScale, pulseDuration).SetEase(Ease.InOutSine))
            .Join(CreateColorTween(0f, 1f, pulseDuration))
            .Append(rectTransform.DOScale(baseScale, pulseDuration).SetEase(Ease.InOutSine))
            .Join(CreateColorTween(1f, 0f, pulseDuration))
            .Append(rectTransform.DOShakePosition(0.18f, shakeStrength, 12, 90f, false, true))
            .AppendInterval(pulseInterval)
            .SetLoops(-1);
    }

    private void StopFullFeedback()
    {
        if (!isFullFeedbackActive)
        {
            return;
        }

        isFullFeedbackActive = false;
        fullFeedbackSequence?.Kill();
        ResetTransformToStart();
        ResetFillColor();
    }

    private void HandleTransitionCompleted()
    {
        isTransitioning = true;
        PlayDrainAnimation(() =>
        {
            isTransitioning = false;
            if (targetProgress > 0f)
            {
                needsUpdate = true;
            }
        });
    }

    private void HandleLevelRestarted()
    {
        StopFullFeedback();
        isTransitioning = false;
        fillTween?.Kill();
        drainSequence?.Kill();
        ResetTransformToStart();
        ResetFillColor();

        if (slider != null)
        {
            slider.value = 0f;
        }

        targetProgress = 0f;
    }

    public void DebugPlayDrain()
    {
        isTransitioning = false;
        if (slider != null)
        {
            slider.value = 1f;
        }
        PlayDrainAnimation();
    }

    private void PlayDrainAnimation(System.Action onComplete = null)
    {
        StopFullFeedback();
        drainSequence?.Kill();
        fillTween?.Kill();
        ResetFillColor();

        if (slider == null)
        {
            onComplete?.Invoke();
            return;
        }

        ResetTransformToStart();

        drainSequence = DOTween.Sequence().SetLink(gameObject);

        if (rectTransform != null)
        {
            drainSequence.Append(rectTransform.DOScale(Vector3.Scale(baseScale, drainSquashScale), drainAnticipationDuration).SetEase(Ease.OutQuad));
        }

        drainSequence.Append(slider.DOValue(0f, drainDuration).SetEase(drainEase));

        if (rectTransform != null)
        {
            Sequence pourSequence = DOTween.Sequence();
            Vector3 suctionScale = new Vector3(baseScale.x * 0.96f, baseScale.y * 1.03f, baseScale.z);
            pourSequence.Append(rectTransform.DOScale(suctionScale, drainDuration * 0.45f).SetEase(Ease.InOutSine));
            pourSequence.Append(rectTransform.DOScale(baseScale, drainDuration * 0.55f).SetEase(Ease.InOutSine));

            Sequence tiltSequence = DOTween.Sequence();
            tiltSequence.Append(rectTransform.DOLocalRotate(new Vector3(0f, 0f, drainTiltAngle), drainDuration * 0.35f).SetEase(Ease.OutSine));
            tiltSequence.AppendInterval(drainDuration * 0.2f);
            tiltSequence.Append(rectTransform.DOLocalRotate(baseRotation.eulerAngles, drainDuration * 0.45f).SetEase(Ease.InOutSine));

            drainSequence.Join(pourSequence);
            drainSequence.Join(tiltSequence);
        }

        if (rectTransform != null)
        {
            drainSequence.Append(rectTransform.DOScale(Vector3.Scale(baseScale, drainImpactScale), 0.07f).SetEase(Ease.OutQuad));
            drainSequence.Append(rectTransform.DOScale(baseScale, drainSettleDuration).SetEase(Ease.OutBack));
            drainSequence.Join(rectTransform.DOPunchPosition(new Vector2(0f, -4f), drainSettleDuration, 8, 0.4f));
        }

        drainSequence.OnComplete(() =>
        {
            ResetTransformToStart();
            onComplete?.Invoke();
        });
    }

    private void ResetTransformToStart()
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.localScale = baseScale;
        rectTransform.anchoredPosition = basePosition;
        rectTransform.localRotation = baseRotation;
    }

    private void CaptureBaseColors()
    {
        if (colorsCaptured)
        {
            return;
        }

        if (fillGraphic == null && slider != null && slider.fillRect != null)
        {
            fillGraphic = slider.fillRect.GetComponent<Graphic>();
        }

        if (fillGraphic != null)
        {
            stylizedFill = fillGraphic as StylizedGraphic;
            if (stylizedFill == null)
            {
                stylizedFill = fillGraphic.GetComponent<StylizedGraphic>();
            }

            if (stylizedFill != null)
            {
                baseTopColor = stylizedFill.TopColor;
                baseBottomColor = stylizedFill.BottomColor;
            }

            baseGraphicColor = fillGraphic.color;
            colorsCaptured = true;
        }
    }

    private Tween CreateColorTween(float from, float to, float duration)
    {
        return DOTween.To(() => from, ApplyPulseColorBlend, to, duration).SetEase(Ease.InOutSine);
    }

    private void ApplyPulseColorBlend(float t)
    {
        if (stylizedFill != null)
        {
            stylizedFill.SetFill(
                Color.Lerp(baseTopColor, pulseColor, t),
                Color.Lerp(baseBottomColor, pulseColor, t)
            );
        }
        else if (fillGraphic != null)
        {
            fillGraphic.color = Color.Lerp(baseGraphicColor, pulseColor, t);
        }
    }

    private void ResetFillColor()
    {
        if (!colorsCaptured)
        {
            return;
        }

        if (stylizedFill != null)
        {
            stylizedFill.SetFill(baseTopColor, baseBottomColor);
        }
        else if (fillGraphic != null)
        {
            fillGraphic.color = baseGraphicColor;
        }
    }
}
