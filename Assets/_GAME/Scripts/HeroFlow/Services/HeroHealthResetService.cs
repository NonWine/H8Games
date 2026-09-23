using System;
using UnityEngine;
using Zenject;

// Refills the hero between encounters. The restart button already restores
// through HeroCombatAgentController.RestartAtSpawn(), but advancing a level only
// teleported him, so the hero used to carry the previous fight's damage into the
// next one.
//
// Restoring the model - not the view - is what makes the slider follow: the bar
// redraws off IHealthModule.HealthChanged like it does for any other change.
public class HeroHealthResetService : IInitializable, IDisposable
{
    private readonly SignalBus signalBus;
    private readonly IHealthModule health;
    private readonly IAliveStateReader aliveState;

    public HeroHealthResetService(SignalBus signalBus, IHealthModule health, IAliveStateReader aliveState)
    {
        this.signalBus = signalBus;
        this.health = health;
        this.aliveState = aliveState;
    }

    public void Initialize()
    {
        signalBus.Subscribe<LoadNextLevelSignal>(RestoreHealth);
        signalBus.Subscribe<LevelRestartedSignal>(RestoreHealth);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<LoadNextLevelSignal>(RestoreHealth);
        signalBus.Unsubscribe<LevelRestartedSignal>(RestoreHealth);
    }

    private void RestoreHealth()
    {
        // A dead hero is deliberately left alone. HeroCombatAgentController owns
        // the full revive - health, reservations, target tracker and state machine
        // together - and runs it from RestartAtSpawn(). Flipping IAliveState back
        // to true from here would leave the animator stuck in the death state
        // while every enemy's IsAlive check starts treating him as a target again.
        if (!aliveState.IsAlive)
        {
            return;
        }

        // On the restart path RestartAtSpawn() -> Revive() has already refilled him
        // before SquadCombatStateController fires LevelRestartedSignal. Without
        // this guard that second restore would raise HealthChanged again and
        // restart the bar's tween on an already-full bar.
        if (Mathf.Approximately(health.CurrentHealth, health.MaxHealth))
        {
            return;
        }

        health.RestoreFull();
    }
}
