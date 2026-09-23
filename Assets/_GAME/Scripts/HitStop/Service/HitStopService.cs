using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

// Owns Time.timeScale for the whole game. Nothing else in the project writes to
// it, and nothing else should: a second writer is how a project ends up stuck at
// 0.05 after an edge case.
public class HitStopService : IHitStopService, IInitializable, IDisposable
{
    private readonly HitStopConfig config;

    private CancellationTokenSource restoreCts;
    private float defaultFixedDeltaTime;
    private bool isFrozen;

    public HitStopService(HitStopConfig config)
    {
        this.config = config;
    }

    public void Initialize()
    {
        // Captured once at full speed. Reading it later would read a value this
        // service had already scaled and shrink the physics step permanently.
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void Request(float timeScale, float durationSeconds)
    {
        if (!config.Enabled || durationSeconds <= 0f)
        {
            return;
        }

        CancelPendingRestore();
        ApplyTimeScale(Mathf.Clamp01(timeScale));

        restoreCts = new CancellationTokenSource();
        RestoreAfterAsync(durationSeconds, restoreCts.Token).Forget();
    }

    // Dispose is the safety net: a scene teardown in the middle of a freeze must
    // not leave the next scene running at 5% speed.
    public void Dispose()
    {
        CancelPendingRestore();

        if (isFrozen)
        {
            ApplyTimeScale(1f);
        }
    }

    private async UniTaskVoid RestoreAfterAsync(float durationSeconds, CancellationToken token)
    {
        try
        {
            // Realtime: the whole point is that the clock this waits on is not
            // the clock it just slowed down.
            await UniTask.Delay(
                TimeSpan.FromSeconds(durationSeconds),
                DelayType.Realtime,
                PlayerLoopTiming.Update,
                token);
        }
        catch (OperationCanceledException)
        {
            // A newer request took over and owns the restore now.
            return;
        }

        ApplyTimeScale(1f);
    }

    private void ApplyTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;

        // Physics has to be stepped down with the clock, otherwise ragdolls keep
        // simulating at full rate through the freeze and the moment reads as a
        // stutter rather than as impact.
        Time.fixedDeltaTime = defaultFixedDeltaTime * timeScale;

        isFrozen = timeScale < 1f;
    }

    private void CancelPendingRestore()
    {
        if (restoreCts == null)
        {
            return;
        }

        restoreCts.Cancel();
        restoreCts.Dispose();
        restoreCts = null;
    }
}
