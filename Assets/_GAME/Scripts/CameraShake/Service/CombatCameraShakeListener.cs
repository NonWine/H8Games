using System;
using UnityEngine;
using Zenject;

// The three shakes that are not hero damage: a tick per kill, a heavy one when
// the formation hits the enemy group, and the biggest one at the capture.
//
// A sibling of HeroDamageCameraShakeListener rather than an edit to it: that one
// owns a damage payload and a damage-scaled amplitude, this one owns flow events
// with no payload at all, and merging them would mean one class with two
// reasons to change.
public class CombatCameraShakeListener : IInitializable, IDisposable
{
    private readonly SignalBus signalBus;
    private readonly ICameraShakeService cameraShake;
    private readonly CameraShakeConfig config;

    private float lastKillShakeTime = float.NegativeInfinity;

    public CombatCameraShakeListener(
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
        signalBus.Subscribe<UnitDiedSignal>(OnUnitDied);
        signalBus.Subscribe<SquadReachedEnemySignal>(OnSquadClashed);
        signalBus.Subscribe<LevelCaptureCompletedSignal>(OnCaptureCompleted);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<UnitDiedSignal>(OnUnitDied);
        signalBus.Unsubscribe<SquadReachedEnemySignal>(OnSquadClashed);
        signalBus.Unsubscribe<LevelCaptureCompletedSignal>(OnCaptureCompleted);
    }

    // Enemies only, for the same reason the hit-stop skips friendly deaths: the
    // shake is the reward, and a loss must not feel like one.
    private void OnUnitDied(UnitDiedSignal signal)
    {
        if (!config.Enabled || signal.Side != CombatSide.Enemy)
        {
            return;
        }

        float now = Time.unscaledTime;
        if (now - lastKillShakeTime < config.EnemyKillMinInterval)
        {
            return;
        }

        lastKillShakeTime = now;

        // No direction: a kill can happen anywhere in the group and a punch
        // towards each one would read as the camera being shoved around.
        cameraShake.Shake(config.EnemyKill, 1f, Vector3.zero);
    }

    private void OnSquadClashed()
    {
        if (!config.Enabled)
        {
            return;
        }

        cameraShake.Shake(config.SquadClash, 1f, Vector3.zero);
    }

    private void OnCaptureCompleted()
    {
        if (!config.Enabled)
        {
            return;
        }

        cameraShake.Shake(config.CaptureComplete, 1f, Vector3.zero);
    }
}
