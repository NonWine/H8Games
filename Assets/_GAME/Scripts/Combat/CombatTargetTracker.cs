using UnityEngine;

public class CombatTargetTracker
{
    private readonly float retargetInterval;
    private readonly float targetLockDuration;

    public CombatTargetTracker(float retargetInterval, float targetLockDuration)
    {
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

    public void SetCurrentTarget(ICombatTarget newTarget, Component attacker)
    {
        if (ReferenceEquals(CurrentTarget, newTarget))
            return;

        ReleaseCurrentTarget(attacker);
        CurrentTarget = newTarget;

        if (CurrentTarget is Component targetComponent &&
            targetComponent is ITargetReservation reservationTarget)
        {
            reservationTarget.TryRegisterAttacker(attacker);
        }

        if (CurrentTarget == null)
        {
            NextRetargetTime = Time.time + retargetInterval;
            TargetLockUntil = 0f;
        }
    }

    public void ReleaseCurrentTarget(Component attacker)
    {
        if (CurrentTarget is Component targetComponent &&
            targetComponent is ITargetReservation reservationTarget)
        {
            reservationTarget.TryUnregisterAttacker(attacker);
        }

        CurrentTarget = null;
    }

    public bool IsCurrentTargetValid(EnemyGroupViewController currentGroup = null)
    {
        if (CurrentTarget == null || !CurrentTarget.IsAlive)
            return false;

        if (CurrentTarget is not Component targetComponent || !targetComponent.gameObject.activeInHierarchy)
            return false;

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

        selfTransform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}