using System;
using UnityEngine;
using Zenject;

public class EnemyCombatAgent : BaseTargetingCombatAgent
{
    [Inject] private IAllyTargetProvider allyTargetProvider;

    public event Action<EnemyCombatAgent> Died;
    

    public void ResetRunTimeState()
    {
        State = UnitState.Idle;
        targetReservation.ClearReservations();
        SetCurrentTarget(null);
    }

    public void Activate()
    {
        if (!IsAlive)
            return;
        attackAgent?.ResetCooldown();
        ResetTargetingTimers();
        State = UnitState.Attack;
    }

    protected override void Update()
    {
        base.Update();
        
        if (!IsAlive)
            return;
        if(State != UnitState.Attack)
            return;

        if (ShouldRetarget())
        {
            TryAcquireTarget();
        }
        else if (currentTarget == null)
        {
            TryAcquireTarget();
        }

        if(currentTarget == null) return;

        RotateTowardsCurrentTarget(transform);
        if (attackAgent.Tick(Time.deltaTime, currentTarget, AttackOrigin.position))
        {
            SpawnProjectileVisual(currentTarget.transform);
        }
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
