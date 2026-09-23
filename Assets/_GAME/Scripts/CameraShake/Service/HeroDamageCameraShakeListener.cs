using System;
using UnityEngine;
using Zenject;

// Turns "the hero was hit" into "the camera shakes this hard, in this direction".
// The only class that knows both the gameplay event and the shake config, so
// adding another trigger is a new listener plus one wiring line rather than an
// edit here.
public class HeroDamageCameraShakeListener : IInitializable, IDisposable
{
    private const float MinimumDirectionSqrMagnitude = 0.0001f;

    private readonly SignalBus signalBus;
    private readonly ICameraShakeService cameraShake;
    private readonly CameraShakeConfig config;

    private float lastShakeTime = float.NegativeInfinity;

    public HeroDamageCameraShakeListener(
        SignalBus signalBus,
        ICameraShakeService cameraShake,
        CameraShakeConfig config)
    {
        this.signalBus = signalBus;
        this.cameraShake = cameraShake;
        this.config = config;
    }

    public void Initialize()
    {
        signalBus.Subscribe<HeroDamagedSignal>(OnHeroDamaged);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<HeroDamagedSignal>(OnHeroDamaged);
    }

    private void OnHeroDamaged(HeroDamagedSignal signal)
    {
        if (!config.Enabled || signal.Damage <= 0f)
        {
            return;
        }

        // Unscaled, to match the shaker's own clock: a hit-stop must not let the
        // rate limit through early.
        float now = Time.unscaledTime;
        if (now - lastShakeTime < config.HeroDamageMinInterval)
        {
            return;
        }

        lastShakeTime = now;
        cameraShake.Shake(config.HeroDamage, ResolveScale(), ResolveDirection(signal));
    }

    private float ResolveScale()
    {
        float min = Mathf.Max(0f,
            Mathf.Min(config.HeroDamageStrengthMin, config.HeroDamageStrengthMax));
        float max = Mathf.Max(min,
            Mathf.Max(config.HeroDamageStrengthMin, config.HeroDamageStrengthMax));
        return UnityEngine.Random.Range(min, max);
    }

    // Points from the attacker towards the hero, so the camera is pushed the way
    // the blow travelled. Flattened on Y because a vertical kick on a top-down
    // follow camera reads as the ground dropping out, not as an impact.
    private Vector3 ResolveDirection(HeroDamagedSignal signal)
    {
        if (!config.UseHitDirection)
        {
            return Vector3.zero;
        }

        Vector3 direction = signal.HeroWorldPosition - signal.SourceWorldPosition;
        direction.y = 0f;

        return direction.sqrMagnitude < MinimumDirectionSqrMagnitude
            ? Vector3.zero
            : direction.normalized;
    }
}
