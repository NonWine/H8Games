using UnityEngine;

public class CombatTargetTracker : IResetModule, IDisposeModule
{
    private const float RotationSmoothness = 12f;
    private readonly float retargetInterval;
    private readonly float targetLockDuration;
    private readonly Component owner;

    public CombatTargetTracker(Component owner, float retargetInterval, float targetLockDuration)
    {
        this.owner = owner;
        this.retargetInterval = retargetInterval;
        this.targetLockDuration = targetLockDuration;
    }

    public ICombatTarget CurrentTarget { get; private set; }
    public float TargetLockUntil { get; private set; }
    public float NextRetargetTime { get; private set; }

    public bool ShouldRetarget()
    {
        return Time.time >= NextRetargetTime && Time.time >= TargetLockUntil;
    }

    public void MarkRetargetWindow()
    {
        NextRetargetTime = Time.time + retargetInterval;
        TargetLockUntil = Time.time + targetLockDuration;
    }

    public void ResetTargetingTimers()
    {
        NextRetargetTime = 0f;
        TargetLockUntil = 0f;
    }

    public void SetCurrentTarget(ICombatTarget newTarget)
    {
        if (ReferenceEquals(CurrentTarget, newTarget))
            return;

        ReleaseCurrentTarget();
        CurrentTarget = newTarget;

        if (CurrentTarget is ITargetReservation reservationTarget)
        {
            reservationTarget.TryRegisterAttacker(owner);
        }

        if (CurrentTarget == null)
        {
            NextRetargetTime = Time.time + retargetInterval;
            TargetLockUntil = 0f;
        }
    }

    public void ReleaseCurrentTarget()
    {
        if (CurrentTarget is ITargetReservation reservationTarget)
        {
            reservationTarget.TryUnregisterAttacker(owner);
        }

        CurrentTarget = null;
    }

    public void Reset()
    {
        ReleaseCurrentTarget();
        ResetTargetingTimers();
    }

    public void Dispose()
    {
        Reset();
    }

    public bool IsCurrentTargetValid(EnemyGroupViewController currentGroup = null)
    {
        if (CurrentTarget == null || !CurrentTarget.IsAlive)
            return false;

        if (CurrentTarget is EnemyCombatAgentController enemyCombatAgentController)
        {
            if (!enemyCombatAgentController.IsActive)
                return false;
        }
        else if (CurrentTarget is Component targetComponent)
        {
            if (!targetComponent.gameObject.activeInHierarchy)
                return false;
        }

        if (currentGroup != null && !currentGroup.ContainsEnemy(CurrentTarget))
            return false;

        return true;
    }

    public void RotateTowardsCurrentTarget(Transform selfTransform)
    {
        if (CurrentTarget == null)
            return;

        Vector3 direction = CurrentTarget.transform.position - selfTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        float lerpFactor = 1f - Mathf.Exp(-RotationSmoothness * Time.deltaTime);
        selfTransform.rotation = Quaternion.Slerp(selfTransform.rotation, targetRotation, lerpFactor);
    }
}
