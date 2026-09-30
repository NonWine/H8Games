using DG.Tweening;
using H8.UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CaptureZoneFeedbackView : MonoBehaviour
{
    [SerializeField] private CaptureBuildupConfig config;
    [SerializeField] private StylizedGraphic fillGraphic;
    [SerializeField] private Image[] pulseRings;
    [SerializeField] private Image releaseRing;

    private ICameraShakeService cameraShake;
    private IAudioService audioService;

    private CaptureBuildupRhythm rhythm;
    private Tween[] ringTweens;
    private Tween releaseTween;
    private Color fillBaseTop;
    private Color fillBaseBottom;
    private float glow;
    private int nextRingIndex;

    [Inject]
    public void Construct(ICameraShakeService cameraShake, IAudioService audioService)
    {
        this.cameraShake = cameraShake;
        this.audioService = audioService;
    }

    private void Awake()
    {
        rhythm = new CaptureBuildupRhythm(config);
        ringTweens = new Tween[pulseRings.Length];
        fillBaseTop = fillGraphic.TopColor;
        fillBaseBottom = fillGraphic.BottomColor;

        HideRings();
        HideRing(releaseRing);
    }

    private void OnDestroy()
    {
        KillTweens();
    }

    public void Tick(float progress, bool engaged, float deltaTime)
    {
        CaptureBuildupPhase phase = config.Sample(progress);
        rhythm.Advance(phase, engaged, deltaTime);

        if (rhythm.PulseDue)
            EmitPulse(phase);

        if (rhythm.ShakeDue)
            EmitShake(phase);

        glow = Mathf.MoveTowards(glow, 0f, deltaTime / config.GlowDecayDuration);
        ApplyFillGradient(phase);
    }

    public void PlayCompleted()
    {
        ResetFeedback();

        releaseRing.gameObject.SetActive(true);
        releaseTween = DOTween
            .To(() => 0f, t => ApplyRing(releaseRing, t, config.ReleaseScale, config.ReleaseColor), 1f, config.ReleaseDuration)
            .SetEase(Ease.OutCubic)
            .SetLink(gameObject)
            .OnComplete(() => HideRing(releaseRing));
    }

    public void ResetFeedback()
    {
        KillTweens();
        HideRings();
        HideRing(releaseRing);

        rhythm.Reset();
        glow = 0f;
        fillGraphic.SetFill(fillBaseTop, fillBaseBottom);
    }

    private void EmitPulse(CaptureBuildupPhase phase)
    {
        int index = nextRingIndex;
        nextRingIndex = (nextRingIndex + 1) % pulseRings.Length;

        Image ring = pulseRings[index];
        Color color = config.RingColor;
        color.a = config.RingAlpha * phase.PulseStrength;
        float targetScale = 1f + config.RingExpansion * phase.PulseStrength;

        ringTweens[index]?.Kill();
        ring.gameObject.SetActive(true);
        ringTweens[index] = DOTween
            .To(() => 0f, t => ApplyRing(ring, t, targetScale, color), 1f, config.RingDuration)
            .SetEase(Ease.OutCubic)
            .SetLink(gameObject)
            .OnComplete(() => HideRing(ring));

        glow = phase.PulseStrength;

        if (config.TickEnabled)
            audioService.Play(SfxId.CaptureTick, phase.TickPitch);
    }

    private void EmitShake(CaptureBuildupPhase phase)
    {
        float scale = config.ShakePower * rhythm.Engagement;
        if (scale <= 0f)
            return;

        CameraShakeSettings settings = new CameraShakeSettings
        {
            Duration = config.ShakeDuration,
            PositionAmplitude = phase.ShakePosition,
            RotationAmplitude = phase.ShakeRotation,
            Frequency = phase.ShakeFrequency,
            DirectionalPunch = 0f,
            DepthInfluence = config.ShakeDepthInfluence,
            Envelope = config.ShakeEnvelope,
        };

        cameraShake.Shake(settings, scale, Vector3.zero);
    }

    // The stylized shader reads the gradient corners and ignores Graphic.color.rgb,
    // so the tension and glow tints have to be written into the fill itself. Both
    // ends are lerped by the same amount, which keeps the authored top-to-bottom
    // ramp intact instead of flattening it to one colour.
    private void ApplyFillGradient(CaptureBuildupPhase phase)
    {
        float engagement = rhythm.Engagement;
        float tension = phase.Tension * engagement;
        float glowAmount = glow * config.GlowStrength * engagement;

        fillGraphic.SetFill(
            BlendFillColor(fillBaseTop, tension, glowAmount),
            BlendFillColor(fillBaseBottom, tension, glowAmount));
    }

    private Color BlendFillColor(Color baseColor, float tension, float glowAmount)
    {
        Color tensed = Color.Lerp(baseColor, config.TensionColor, tension);
        return Color.Lerp(tensed, config.GlowColor, glowAmount);
    }

    private static void ApplyRing(Image ring, float t, float targetScale, Color color)
    {
        ring.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(1f, targetScale, t);
        color.a *= 1f - t;
        ring.color = color;
    }

    private void HideRings()
    {
        foreach (Image ring in pulseRings)
            HideRing(ring);
    }

    private static void HideRing(Image ring)
    {
        ring.rectTransform.localScale = Vector3.one;
        ring.gameObject.SetActive(false);
    }

    private void KillTweens()
    {
        releaseTween?.Kill();
        releaseTween = null;

        if (ringTweens == null)
            return;

        for (int i = 0; i < ringTweens.Length; i++)
        {
            ringTweens[i]?.Kill();
            ringTweens[i] = null;
        }
    }
}
