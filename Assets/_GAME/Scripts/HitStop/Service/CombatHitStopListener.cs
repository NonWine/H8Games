using System;
using UnityEngine;
using Zenject;

// Turns "a fight resolved" into "time crawls". The only class that knows both a
// gameplay event and a hit-stop shape, so the service stays a dumb owner of
// Time.timeScale.
public class CombatHitStopListener : IInitializable, IDisposable
{
    private readonly SignalBus signalBus;
    private readonly IHitStopService hitStop;
    private readonly HitStopConfig config;

    private float lastKillFreezeTime = float.NegativeInfinity;

    public CombatHitStopListener(SignalBus signalBus, IHitStopService hitStop, HitStopConfig config)
    {
        this.signalBus = signalBus;
        this.hitStop = hitStop;
        this.config = config;
    }

    public void Initialize()
    {
        signalBus.Subscribe<UnitDiedSignal>(OnUnitDied);
        signalBus.Subscribe<HeroDefeatedSignal>(OnHeroDefeated);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<UnitDiedSignal>(OnUnitDied);
        signalBus.Unsubscribe<HeroDefeatedSignal>(OnHeroDefeated);
    }

    // Only enemy deaths freeze. A soldier going down is a loss, and rewarding it
    // with the same punch the player gets for a kill reads as a bug.
    private void OnUnitDied(UnitDiedSignal signal)
    {
        if (signal.Side != CombatSide.Enemy)
        {
            return;
        }

        // Unscaled, to match the service's own clock: a freeze must not let the
        // rate limit through early and chain into the next one.
        float now = Time.unscaledTime;
        if (now - lastKillFreezeTime < config.KillMinInterval)
        {
            return;
        }

        lastKillFreezeTime = now;
        hitStop.Request(config.KillTimeScale, config.KillDuration);
    }

    // Not rate limited and deliberately longer: this one is the end of the run,
    // not a beat inside it.
    private void OnHeroDefeated()
    {
        hitStop.Request(config.HeroDefeatTimeScale, config.HeroDefeatDuration);
    }
}
