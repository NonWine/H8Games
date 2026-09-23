using UnityEngine;

[CreateAssetMenu(fileName = "CaptureBuildupConfig", menuName = "Config/CaptureBuildupConfig")]
public class CaptureBuildupConfig : ScriptableObject
{
    [Header("Engagement")]
    [SerializeField, Min(0.01f)] private float fadeInDuration = 0.15f;
    [SerializeField, Min(0.01f)] private float fadeOutDuration = 0.3f;

    [Header("Phases (sorted by progress, values blend linearly between rows)")]
    [SerializeField] private CaptureBuildupPhase[] phases =
    {
        new CaptureBuildupPhase
        {
            Progress = 0f, PulseInterval = 0.45f, PulseStrength = 0.45f, Tension = 0f, TickPitch = 0.85f,
            ShakePosition = 0f, ShakeRotation = 0f, ShakeFrequency = 14f,
        },
        new CaptureBuildupPhase
        {
            Progress = 0.3f, PulseInterval = 0.38f, PulseStrength = 0.6f, Tension = 0.05f, TickPitch = 1f,
            ShakePosition = 0.004f, ShakeRotation = 0.02f, ShakeFrequency = 16f,
        },
        new CaptureBuildupPhase
        {
            Progress = 0.75f, PulseInterval = 0.22f, PulseStrength = 0.85f, Tension = 0.25f, TickPitch = 1.25f,
            ShakePosition = 0.018f, ShakeRotation = 0.09f, ShakeFrequency = 20f,
        },
        new CaptureBuildupPhase
        {
            Progress = 1f, PulseInterval = 0.12f, PulseStrength = 1f, Tension = 0.6f, TickPitch = 1.5f,
            ShakePosition = 0.04f, ShakeRotation = 0.2f, ShakeFrequency = 26f,
        },
    };

    [Header("Shake")]
    [SerializeField] private bool shakeEnabled = true;
    [SerializeField, Range(0f, 3f)] private float shakePower = 1f;
    [SerializeField, Min(0.02f)] private float shakeInterval = 0.1f;
    [SerializeField, Min(0.02f)] private float shakeDuration = 0.2f;
    [SerializeField, Range(0f, 1f)] private float shakeDepthInfluence = 0.1f;
    [SerializeField] private AnimationCurve shakeEnvelope = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

    [Header("Pulse ring")]
    [SerializeField, Min(0f)] private float ringExpansion = 0.3f;
    [SerializeField, Range(0f, 1f)] private float ringAlpha = 0.7f;
    [SerializeField, Min(0.01f)] private float ringDuration = 0.45f;
    [SerializeField] private Color ringColor = new Color(1f, 0.95f, 0.7f, 1f);

    [Header("Fill")]
    [SerializeField] private Color tensionColor = new Color(0.75f, 1f, 0.25f, 1f);
    [SerializeField] private Color glowColor = Color.white;
    [SerializeField, Range(0f, 1f)] private float glowStrength = 0.35f;
    [SerializeField, Min(0.01f)] private float glowDecayDuration = 0.18f;

    [Header("Audio")]
    [SerializeField] private bool tickEnabled = true;

    [Header("Completion")]
    [SerializeField, Min(0.01f)] private float releaseDuration = 0.4f;
    [SerializeField, Min(1f)] private float releaseScale = 1.9f;
    [SerializeField] private Color releaseColor = new Color(1f, 0.95f, 0.7f, 1f);

    public float FadeInDuration => fadeInDuration;
    public float FadeOutDuration => fadeOutDuration;
    public bool ShakeEnabled => shakeEnabled;
    public float ShakePower => shakePower;
    public float ShakeInterval => shakeInterval;
    public float ShakeDuration => shakeDuration;
    public float ShakeDepthInfluence => shakeDepthInfluence;
    public AnimationCurve ShakeEnvelope => shakeEnvelope;
    public float RingExpansion => ringExpansion;
    public float RingAlpha => ringAlpha;
    public float RingDuration => ringDuration;
    public Color RingColor => ringColor;
    public Color TensionColor => tensionColor;
    public Color GlowColor => glowColor;
    public float GlowStrength => glowStrength;
    public float GlowDecayDuration => glowDecayDuration;
    public bool TickEnabled => tickEnabled;
    public float ReleaseDuration => releaseDuration;
    public float ReleaseScale => releaseScale;
    public Color ReleaseColor => releaseColor;

    public CaptureBuildupPhase Sample(float progress)
    {
        if (phases == null || phases.Length == 0)
            return default;

        progress = Mathf.Clamp01(progress);

        if (progress <= phases[0].Progress)
            return phases[0];

        for (int i = 1; i < phases.Length; i++)
        {
            CaptureBuildupPhase to = phases[i];
            if (progress > to.Progress)
                continue;

            CaptureBuildupPhase from = phases[i - 1];
            float span = to.Progress - from.Progress;
            float t = span > 0f ? (progress - from.Progress) / span : 1f;
            return CaptureBuildupPhase.Lerp(from, to, t);
        }

        return phases[phases.Length - 1];
    }
}
