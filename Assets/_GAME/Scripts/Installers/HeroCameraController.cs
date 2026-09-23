using System;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class HeroCameraController : IInitializable, ITickable, IDisposable
{
    private enum FocusPhase
    {
        Normal,
        Entering,
        Holding,
        Exiting
    }

    private readonly PlayerView hero;
    private readonly CinemachineCamera followCamera;
    private readonly CombatCameraFocusConfig focusConfig;

    private Transform followTarget;
    private Transform lookAtTarget;
    private Vector3 lastAttentionPosition;
    private Vector3 followOffset;
    private Vector3 lookAtOffset;
    private Vector3 smoothedBias;
    private Vector3 biasVelocity;
    private float focusWeight;
    private float phaseStartWeight;
    private float phaseElapsed;
    private float phaseDuration;
    private FocusPhase phase;
    private bool combatPreviewForced;

    public HeroCameraController(
        PlayerView hero,
        CinemachineCamera followCamera,
        CombatCameraFocusConfig focusConfig)
    {
        this.hero = hero;
        this.followCamera = followCamera;
        this.focusConfig = focusConfig;
    }

    public void Initialize()
    {
        followTarget = new GameObject("HeroCameraFollowTarget").transform;
        lookAtTarget = new GameObject("HeroCameraLookAtTarget").transform;
        followOffset = hero.CameraAnchor.position - hero.transform.position;
        lookAtOffset = GetDefaultLookAtPosition() - hero.transform.position;
        followTarget.position = hero.transform.position + followOffset;
        lookAtTarget.position = hero.transform.position + lookAtOffset;
        followCamera.Follow = followTarget;
        followCamera.LookAt = lookAtTarget;
        followCamera.Lens.FieldOfView = focusConfig.DefaultFieldOfView;
    }

    public void Tick()
    {
        UpdateFocusPhase();
        UpdateTrackingTargets();
        UpdateLens();
    }

    public void Dispose()
    {
        if (followCamera != null && hero != null)
        {
            followCamera.Follow = hero.CameraAnchor;
            followCamera.LookAt = hero.CameraAnchor;
        }

        if (followTarget != null)
        {
            UnityEngine.Object.Destroy(followTarget.gameObject);
        }

        if (lookAtTarget != null)
        {
            UnityEngine.Object.Destroy(lookAtTarget.gameObject);
        }
    }

    public void SetCombatPreview(bool enabled)
    {
        if (combatPreviewForced == enabled)
        {
            return;
        }

        combatPreviewForced = enabled;
        if (enabled)
        {
            Vector3 previewPosition = hero.transform.position
                + hero.transform.forward * (focusConfig.MaxAttentionOffset
                    / Mathf.Max(0.01f, focusConfig.TargetInfluence));
            if (phase == FocusPhase.Normal)
            {
                smoothedBias = GetAttentionBias(previewPosition);
                biasVelocity = Vector3.zero;
            }

            lastAttentionPosition = previewPosition;
            BeginPhase(FocusPhase.Entering, focusConfig.FocusInDuration);
        }
        else if (phase != FocusPhase.Normal)
        {
            BeginPhase(FocusPhase.Exiting, focusConfig.FocusOutDuration);
        }
    }

    private void BeginPhase(FocusPhase nextPhase, float fullDuration)
    {
        phase = nextPhase;
        phaseStartWeight = focusWeight;
        phaseElapsed = 0f;
        float remaining = nextPhase == FocusPhase.Entering
            ? 1f - focusWeight
            : focusWeight;
        phaseDuration = Mathf.Max(0.01f, fullDuration * remaining);
    }

    private void UpdateFocusPhase()
    {
        if (phase == FocusPhase.Normal || phase == FocusPhase.Holding)
        {
            return;
        }

        phaseElapsed = Mathf.Min(
            phaseElapsed + Time.unscaledDeltaTime, phaseDuration);
        float progress = phaseElapsed / phaseDuration;
        float eased = phase == FocusPhase.Entering
            ? 1f - Mathf.Pow(1f - progress, 3f)
            : (1f - Mathf.Cos(progress * Mathf.PI)) * 0.5f;
        focusWeight = Mathf.Lerp(
            phaseStartWeight,
            phase == FocusPhase.Entering ? 1f : 0f,
            eased);

        if (phaseElapsed < phaseDuration)
        {
            return;
        }

        if (phase == FocusPhase.Entering)
        {
            phase = FocusPhase.Holding;
        }
        else
        {
            phase = FocusPhase.Normal;
        }
    }

    private void UpdateTrackingTargets()
    {
        Vector3 heroPosition = hero.transform.position;
        float deltaTime = Time.unscaledDeltaTime;
        Vector3 desiredBias = phase == FocusPhase.Normal
            ? Vector3.zero
            : GetAttentionBias(lastAttentionPosition);
        smoothedBias = Vector3.SmoothDamp(
            smoothedBias,
            desiredBias,
            ref biasVelocity,
            focusConfig.TargetSwitchSmoothTime,
            Mathf.Infinity,
            deltaTime);
        Vector3 attentionOffset = smoothedBias * focusWeight;

        followTarget.position = heroPosition + followOffset + attentionOffset;
        lookAtTarget.position = heroPosition + lookAtOffset + attentionOffset;
    }

    private Vector3 GetAttentionBias(Vector3 position)
    {
        Vector3 direction = position - hero.transform.position;
        direction.y = 0f;
        return Vector3.ClampMagnitude(
            direction * focusConfig.TargetInfluence,
            focusConfig.MaxAttentionOffset);
    }

    private Vector3 GetDefaultLookAtPosition()
    {
        Vector3 forward = hero.CameraLookAtAnchor.position - hero.CameraAnchor.position;
        forward.y = 0f;
        return hero.CameraLookAtAnchor.position + forward * 0.5f;
    }

    private void UpdateLens()
    {
        float baseFieldOfView = focusConfig.DefaultFieldOfView;
        float zoom = focusConfig.UseFocusZoom
            ? baseFieldOfView * focusConfig.FocusZoomPercent * focusWeight
            : 0f;
        followCamera.Lens.FieldOfView = Mathf.Clamp(
            baseFieldOfView + zoom, 10f, 120f);
    }
}
