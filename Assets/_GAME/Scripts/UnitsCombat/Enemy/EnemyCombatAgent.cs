using System;
using UnityEngine;
using Zenject;

public class EnemyCombatAgent : BaseTargetingCombatAgent
{
    [Inject] private IAllyTargetProvider allyTargetProvider;
    [Inject] private CurrencyService currencyService;


    public event Action<EnemyCombatAgent> Died;
    
    

    private void OnDisable()
    {
        modules.TargetTracker.SetCurrentTarget(null, this);
        modules.Reservation.ClearReservations();
    }

    public void ResetRunTimeState()
    {
        State = UnitState.Idle;
        modules.Reservation.ClearReservations();
        modules.TargetTracker.SetCurrentTarget(null, this);
        modules.TargetTracker.ResetTargetingTimers();
        modules.Attack.ResetCooldownWithRandomDelay();
    }

    public void Activate()
    {
        if (!IsAlive)
            return;

        modules.Attack.ResetCooldownWithRandomDelay();
        modules.TargetTracker.ResetTargetingTimers();
        State = UnitState.Attack;

    }

    protected override void Update()
    {
        base.Update();
        
        if (!IsAlive)
            return;

        if (State != UnitState.Attack)
            return;

        var tracker = modules.TargetTracker;

        if (!tracker.IsCurrentTargetValid())
        {
            TryAcquireTarget();
        }
        else if (tracker.ShouldRetarget())
        {
            TryAcquireTarget();
        }

        if (tracker.CurrentTarget == null)
            return;

        tracker.RotateTowardsCurrentTarget(transform);

        if (modules.Attack.Tick(Time.deltaTime))
        {
            ICombatTarget currentTarget = tracker.CurrentTarget;
            Vector3 attackOrigin = CombatView.AttackPoint.position;

            if (!modules.ProjectileSpawner.Spawn(
                    CombatView.AttackPoint,
                    currentTarget.transform,
                    unitStats.ProjectileSpeed,
                    () => modules.Attack.ApplyDamage(currentTarget, attackOrigin)))
            {
                modules.Attack.ApplyDamage(currentTarget, attackOrigin);
            }
        }
    }

    private void TryAcquireTarget()
    {
        ICombatTarget target = allyTargetProvider.GetBestLivingAllyTarget(
            transform.position,
            unitStats.ReservationPenalty);

        var tracker = modules.TargetTracker;

        if (target == null)
        {
            tracker.SetCurrentTarget(null, this);
            tracker.ResetTargetingTimers();
            return;
        }

        tracker.SetCurrentTarget(target, this);
        tracker.MarkRetargetWindow();
    }

    protected override void OnDied()
    {
        currencyService?.Add(unitStats.DeathReward);
        Died?.Invoke(this);
        base.OnDied();
    }
}
