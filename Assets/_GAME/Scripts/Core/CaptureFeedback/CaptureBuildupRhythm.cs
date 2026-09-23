using UnityEngine;

public class CaptureBuildupRhythm
{
    private readonly CaptureBuildupConfig config;

    private float pulseTimer;
    private float shakeTimer;

    public CaptureBuildupRhythm(CaptureBuildupConfig config)
    {
        this.config = config;
    }

    public float Engagement { get; private set; }
    public bool PulseDue { get; private set; }
    public bool ShakeDue { get; private set; }

    public void Advance(CaptureBuildupPhase phase, bool engaged, float deltaTime)
    {
        PulseDue = false;
        ShakeDue = false;

        float fadeDuration = engaged ? config.FadeInDuration : config.FadeOutDuration;
        Engagement = Mathf.MoveTowards(Engagement, engaged ? 1f : 0f, deltaTime / fadeDuration);

        if (!engaged)
            return;

        pulseTimer -= deltaTime;
        if (pulseTimer <= 0f)
        {
            PulseDue = true;
            pulseTimer = Mathf.Max(0f, pulseTimer + phase.PulseInterval);
        }

        shakeTimer -= deltaTime;
        if (shakeTimer <= 0f)
        {
            ShakeDue = config.ShakeEnabled && phase.HasShake;
            shakeTimer = Mathf.Max(0f, shakeTimer + config.ShakeInterval);
        }
    }

    public void Reset()
    {
        Engagement = 0f;
        PulseDue = false;
        ShakeDue = false;
        pulseTimer = 0f;
        shakeTimer = 0f;
    }
}
