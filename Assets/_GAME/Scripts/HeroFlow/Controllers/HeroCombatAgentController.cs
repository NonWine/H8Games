using System;
using UnityEngine;
using Zenject;

public class HeroCombatAgentController : IInitializable, ITickable, IDisposable, ITargetSelectionCandidate
{
    private readonly HeroRuntimeModel runtimeModel;
    private readonly PlayerView heroView;
    private readonly HeroStateMachine stateMachine;
    private readonly IHealthModule health;
    private readonly IHeroMover heroMover;
    private readonly ITargetTrackerHandler targetTracker;
    private readonly ITargetReservationHandler targetReservationHandler;
    private readonly SignalBus signalBus;
    private readonly HeroAutoAttackLogger logger;

    public Vector3 Position => runtimeModel.Transform.position;
    public bool IsAlive => runtimeModel.IsAlive;
    public bool HasCombatTarget => runtimeModel.IsAlive && targetTracker.IsCurrentTargetValid();
    public Transform CombatTargetTransform => HasCombatTarget ? targetTracker.CurrentTarget.transform : null;
    public int ReservationCount => targetReservationHandler.ReservationCount;
    public Transform transform => runtimeModel.Transform;

    public HeroCombatAgentController(
        HeroRuntimeModel runtimeModel,
        PlayerView heroView,
        HeroStateMachine stateMachine,
        IHealthModule health,
        IHeroMover heroMover,
        ITargetTrackerHandler targetTracker,
        ITargetReservationHandler targetReservationHandler,
        SignalBus signalBus,
        HeroAutoAttackLogger logger)
    {
        this.runtimeModel = runtimeModel;
        this.heroView = heroView;
        this.stateMachine = stateMachine;
        this.health = health;
        this.heroMover = heroMover;
        this.targetTracker = targetTracker;
        this.targetReservationHandler = targetReservationHandler;
        this.signalBus = signalBus;
        this.logger = logger;
    }

    public void Initialize()
    {
        health.Died += OnDied;
        signalBus.Subscribe<GameIdleStateSignal>(HandleGameIdle);

        logger.LogConfiguration();
        stateMachine.ChangeState<HeroIdleState>();
    }

    public void Tick()
    {
        if (!runtimeModel.IsAlive)
        {
            return;
        }

        targetTracker.UpdateTarget();
        stateMachine.Tick();
    }

    public void TakeDamage(float damage, Vector3 sourceWorldPosition)
    {
        if (!runtimeModel.IsAlive)
        {
            return;
        }

        ParticlePool.Instance.PlayHit(runtimeModel.Transform.position);

        // Same ordering rule the other agents use: the hit is announced before
        // the damage is applied, so it can never arrive after the death it
        // caused. HeroDefeatedSignal, not UnitDiedSignal, is what reports that
        // death - firing both would double every reaction to it.
        signalBus.Fire(new UnitDamagedSignal(
            runtimeModel.Transform.position,
            damage,
            CombatSide.Ally,
            health.CurrentHealth - damage <= 0f));

        health.ApplyDamage(damage);
        runtimeModel.View.PlayHitFeedback();

        // Fired after ApplyDamage so listeners see the post-hit health, and as a
        // signal rather than a direct call because the hero lives in its own
        // sub-container while the camera feedback lives at scene scope.
        signalBus.Fire(new HeroDamagedSignal(
            runtimeModel.Transform.position,
            sourceWorldPosition,
            damage,
            health.CurrentHealth / health.MaxHealth));
    }

    // Level-transition teleport only repositions the hero: unlike RestartAtSpawn,
    // he is not dead here, so Revive() (which also resets health and clears
    // combat/reservation state) must not run.
    public void TeleportTo(Vector3 position, Quaternion rotation)
    {
        heroMover.Teleport(position);
        heroView.transform.rotation = rotation;
    }

    // Restart-button path: unlike HandleGameIdle's automatic recovery, this one
    // also moves the hero, since clicking Restart means the player does not want
    // to walk back from wherever they died.
    public void RestartAtSpawn(Vector3 position, Quaternion rotation)
    {
        TeleportTo(position, rotation);
        Revive();
    }

    public void Dispose()
    {
        health.Died -= OnDied;
        signalBus.Unsubscribe<GameIdleStateSignal>(HandleGameIdle);
    }

    private void OnDied()
    {
        stateMachine.ChangeState<HeroDeadState>();
    }

    // Fires for both a squad-only wipe and a hero death. Only the squad-only case
    // should revive anything here: a soldiers-wiped-but-hero-alive fight reaches
    // this same idle window and needs its reservations/tracker cleared, but a
    // dead hero must stay dead - same Animator state, same non-target to every
    // enemy's IsAlive check - until RestartAtSpawn() explicitly brings him back.
    // Reviving on this timer regardless of IsAlive is exactly what used to pop
    // the Animator back to Idle and make enemies re-aggro before Restart was
    // ever pressed.
    private void HandleGameIdle()
    {
        if (!runtimeModel.IsAlive)
        {
            return;
        }

        Revive();
    }

    private void Revive()
    {
        targetReservationHandler.ClearReservations();
        targetTracker.Reset();
        ((IResetModule)health).Reset();
        stateMachine.ChangeState<HeroIdleState>();
    }
}
