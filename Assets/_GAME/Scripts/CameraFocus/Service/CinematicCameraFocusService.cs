using System;
using DG.Tweening;
using UnityEngine;

public class CinematicCameraFocusService : ICinematicCameraFocus, ICameraFocusState, IDisposable
{
    private const float MinimumBlendDuration = 0.01f;

    private readonly CinematicFocusConfig config;

    private Transform target;
    private Tween weightTween;
    private Tween orbitTween;
    private float weight;
    private float orbitDegrees;

    public float Weight => weight;
    public bool IsActive => target != null;

    public CinematicCameraFocusService(CinematicFocusConfig config)
    {
        this.config = config;
    }

    public bool TryGetFrame(out CameraFocusFrame frame)
    {
        if (target == null || weight <= 0f)
        {
            frame = default;
            return false;
        }

        frame = new CameraFocusFrame(
            target.position + Vector3.up * config.HeightOffset,
            config.DistanceScale,
            orbitDegrees,
            config.FieldOfView);

        return true;
    }

    public Tween Focus(Transform focusTarget)
    {
        if (!config.Enabled)
        {
            return null;
        }

        KillTweens();

        target = focusTarget;
        orbitDegrees = 0f;

        orbitTween = DOTween
            .To(() => orbitDegrees, value => orbitDegrees = value, config.OrbitDegrees, config.OrbitDuration)
            .SetEase(config.OrbitEase)
            .SetUpdate(true);

        weightTween = CreateWeightTween(1f, config.FocusInDuration, config.FocusInEase);

        return weightTween;
    }

    public Tween Release()
    {
        if (target == null)
        {
            return null;
        }

        KillTweens();

        weightTween = CreateWeightTween(0f, config.ReleaseDuration, config.ReleaseEase)
            .OnComplete(ClearTarget);

        return weightTween;
    }

    public void Dispose()
    {
        KillTweens();
        weight = 0f;
        orbitDegrees = 0f;
        ClearTarget();
    }

    private Tween CreateWeightTween(float targetWeight, float fullDuration, Ease ease)
    {
        float duration = Mathf.Max(MinimumBlendDuration, fullDuration * Mathf.Abs(targetWeight - weight));

        return DOTween
            .To(() => weight, value => weight = value, targetWeight, duration)
            .SetEase(ease)
            .SetUpdate(true);
    }

    private void KillTweens()
    {
        weightTween?.Kill();
        weightTween = null;
        orbitTween?.Kill();
        orbitTween = null;
    }

    private void ClearTarget()
    {
        target = null;
    }
}
