using System.Collections.Generic;
using UnityEngine;

public class CombatTargetTracker : ITargetTrackerHandler
{
    private readonly float retargetInterval;
    private readonly float targetLockDuration;

    private readonly ICombatTargetProvider targetProvider;
    private readonly ITargetReservation targetReservation;
    private readonly List<ICombatTargetValidator> targetValidators;

    private float nextRetargetTime;
    private float targetLockUntil;
    
    public ICombatTarget CurrentTarget { get; private set; }

    public CombatTargetTracker(ICombatTargetProvider targetProvider, List<ICombatTargetValidator> targetValidators, float retargetInterval, float targetLockDuration)
    {
        this.retargetInterval = retargetInterval;
        this.targetLockDuration = targetLockDuration;
        this.targetProvider = targetProvider;
        this.targetValidators = targetValidators;
    }

    

    public void UpdateTarget(UnitState state)
    {
        if (state != UnitState.Attack)
        {
            Reset();
            return;
        }

        if (!IsCurrentTargetValid() || CanRetarget())
        {
            SetCurrentTarget(targetProvider.GetTarget());
        }
    }

    public bool IsCurrentTargetValid()
    {
        if (CurrentTarget == null)
            return false;

        for (int i = 0; i < targetValidators.Count; i++)
        {
            if (!targetValidators[i].IsValid(CurrentTarget))
                return false;
        }

        return true;
    }

    public void Reset()
    {
        ReleaseCurrentTarget();

        nextRetargetTime = 0f;
        targetLockUntil = 0f;
    }

    private bool CanRetarget()
    {
        return Time.time >= nextRetargetTime && Time.time >= targetLockUntil;
    }

    private void MarkRetargetWindow()
    {
        nextRetargetTime = Time.time + retargetInterval;
        targetLockUntil = CurrentTarget == null
            ? 0f
            : Time.time + targetLockDuration;
    }

    private void SetCurrentTarget(ICombatTarget newTarget)
    {
        if (ReferenceEquals(CurrentTarget, newTarget))
            return;

        ReleaseCurrentTarget();
        CurrentTarget = newTarget;

        if (CurrentTarget != null)
        {
            targetReservation.TryRegisterAttacker(CurrentTarget);
            MarkRetargetWindow();
            return;
        }

        nextRetargetTime = Time.time + retargetInterval;
        targetLockUntil = 0f;
    }

    private void ReleaseCurrentTarget()
    {
        targetReservation.TryUnregisterAttacker(CurrentTarget);
        CurrentTarget = null;
    }
}
