using System;
using UnityEngine;
using Zenject;

public class EnemyCombatAgent : BaseTargetingCombatAgent
{
    [Inject] private IAllyTargetProvider allyTargetProvider;
    public EnemyGroupFacade Group { get; private set; }
    public StaticEnemyState State { get; private set; } = StaticEnemyState.Idle;
    public event Action<EnemyCombatAgent> Died;

    public void ResetRunTimeState()
    {
        targetReservation.ClearReservations();
        unitHealthHandler.RestoreFull();
        State = StaticEnemyState.Idle;
    }
    
    public void SetGroup(EnemyGroupFacade group)
    {
        Group = group;
    }

    public void Activate()
    {
        if (!IsAlive)
            return;
        attackAgent?.ResetCooldown();
        ResetTargetingTimers();
        State = StaticEnemyState.Attack;
    }

    private void Update()
    {
        if (!IsAlive)
            return;
        if(State != StaticEnemyState.Attack)
            return;

        if (!IsCurrentTargetValidBase())
        {
            TryAcquireTarget();
        }
        else if (ShouldRetarget())
        {
            TryAcquireTarget();
        }
        if (!IsCurrentTargetValidBase())
            return;

        RotateTowardsCurrentTarget(transform);
        if (attackAgent.Tick(Time.deltaTime, currentTarget, AttackOrigin.position))
            SpawnProjectileVisual(currentTarget.transform);
    }

    private void TryAcquireTarget()
    {
        
        ICombatTarget target = allyTargetProvider.GetBestLivingAllyTarget(transform.position, reservationPenalty);

        if (target == null)
        {
            SetCurrentTarget(null);
            nextRetargetTime = Time.time + retargetInterval;
            targetLockUntil = 0f;
            return;
        }

        SetCurrentTarget(target);
        MarkRetargetWindow();
    }

    protected override void HandleDeath()
    {
        Died?.Invoke(this);
        base.HandleDeath();
    }
}