using System.Collections.Generic;
using UnityEngine;

public class CombatTargetTracker : ITargetTrackerHandler
{
    private readonly float retargetInterval;
    private readonly float targetLockDuration;

    private readonly ICombatTargetProvider targetProvider;
    private readonly ITargetReservationHandler _targetReservationHandler;
    private readonly List<ICombatTargetValidator> targetValidators;

    private float nextRetargetTime;
    private float targetLockUntil;
    
    public ICombatTarget CurrentTarget { get; private set; }

    public CombatTargetTracker(
        ICombatTargetProvider targetProvider,
        ITargetReservationHandler targetReservationHandler,
        List<ICombatTargetValidator> targetValidators,
        TargetingData targetingData)
    {
        retargetInterval = targetingData.RetargetInterval;
        targetLockDuration = targetingData.TargetLockDuration;
        this.targetProvider = targetProvider;
        this._targetReservationHandler = targetReservationHandler;
        this.targetValidators = targetValidators;
    }

    

    public void UpdateTarget()
    {
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
            _targetReservationHandler.TryRegisterAttacker(CurrentTarget);
            MarkRetargetWindow();
            return;
        }

        nextRetargetTime = Time.time + retargetInterval;
        targetLockUntil = 0f;
    }

    private void ReleaseCurrentTarget()
    {
        _targetReservationHandler.TryUnregisterAttacker(CurrentTarget);
        CurrentTarget = null;
    }
}
