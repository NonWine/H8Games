using System;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class CombatCameraFocusController : IInitializable, ITickable, IDisposable
{
    private readonly ICombatStateProvider combatState;
    private readonly CinemachineCamera followCamera;
    private readonly CinemachineFollow cameraFollow;
    private readonly CombatCameraFocusConfig config;

    private Tween focusTween;

    private Vector3 baseOffset;
    private Vector3 combatOffset;

    private float baseFieldOfView;
    private float focus;
    private bool isFocused;

    public CombatCameraFocusController(
        ICombatStateProvider combatState,
        CinemachineCamera followCamera,
        CinemachineFollow cameraFollow,
        CombatCameraFocusConfig config)
    {
        this.combatState = combatState;
        this.followCamera = followCamera;
        this.cameraFollow = cameraFollow;
        this.config = config;
    }

    public void Initialize()
    {
        baseOffset = cameraFollow.FollowOffset;
        baseFieldOfView = followCamera.Lens.FieldOfView;

        combatOffset = new Vector3(
            baseOffset.x * config.OffsetScale,
            baseOffset.y * config.OffsetScale * config.HeightScale,
            baseOffset.z * config.OffsetScale);
    }

    public void Tick()
    {
        bool shouldFocus = config.Enabled && combatState.State == CombatFlowState.FightingZone;

        if (shouldFocus == isFocused)
        {
            return;
        }

        isFocused = shouldFocus;
        PlayFocusTween(shouldFocus);
    }

    public void Dispose()
    {
        focusTween?.Kill();
        ApplyFocus(0f);
    }

    private void PlayFocusTween(bool focused)
    {
        focusTween?.Kill();

        float target = focused ? 1f : 0f;
        float duration = focused ? config.EnterDuration : config.ExitDuration;
        Ease ease = focused ? config.EnterEase : config.ExitEase;

        focusTween = DOTween.To(() => focus, ApplyFocus, target, duration)
            .SetEase(ease)
            .SetUpdate(true);
    }

    private void ApplyFocus(float value)
    {
        focus = value;

        cameraFollow.FollowOffset = Vector3.Lerp(baseOffset, combatOffset, value);

        LensSettings lens = followCamera.Lens;
        lens.FieldOfView = Mathf.Lerp(baseFieldOfView, config.FieldOfView, value);
        followCamera.Lens = lens;
    }
}
