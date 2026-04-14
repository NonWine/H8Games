using UnityEngine;

public sealed class EnemyCombatAgent : MonoBehaviour
{
    private StaticEnemyAgent owner;
    private SquadCombatCoordinator squadCombatCoordinator;
    private AttackService attackService;
    private ICombatTarget currentTarget;
    private float nextRetargetTime;
    private float targetLockUntil;
    private const float reservationPenalty = 3f;
    private const float retargetInterval = 0.35f;
    private const float targetLockDuration = 0.35f;

    public void Initialize(StaticEnemyAgent owner)
    {
        this.owner = owner;
        attackService = new AttackService(
            () => owner.RuntimeStats.Damage,
            () => owner.RuntimeStats.AttackCooldown,
            () => owner.AttackOrigin.position);
    }

    public void Activate(SquadCombatCoordinator squadCombatCoordinator)
    {
        this.squadCombatCoordinator = squadCombatCoordinator;
        attackService?.ResetCooldown();
        nextRetargetTime = 0f;
        targetLockUntil = 0f;
    }

    public void Deactivate()
    {
        SetCurrentTarget(null);
        squadCombatCoordinator = null;
        nextRetargetTime = 0f;
        targetLockUntil = 0f;
    }

    private void OnDisable()
    {
        Deactivate();
    }

    private void Update()
    {
        if (!owner.IsAlive || squadCombatCoordinator == null)
            return;

        if (!IsCurrentTargetValid())
        {
            TryAcquireTarget();
        }
        else if (Time.time >= nextRetargetTime && Time.time >= targetLockUntil)
        {
            TryAcquireTarget();
        }

        if (!IsCurrentTargetValid())
            return;

        Vector3 direction = currentTarget.transform.position - owner.transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        

        if (attackService.Tick(Time.deltaTime, currentTarget))
            owner.SpawnProjectileVisual(currentTarget.transform);
    }

    private bool TryAcquireTarget()
    {
        if (squadCombatCoordinator == null)
        {
            SetCurrentTarget(null);
            return false;
        }

        ICombatTarget target = squadCombatCoordinator.GetBestLivingAllyTarget(owner.transform.position, reservationPenalty);
        if (target == null)
        {
            nextRetargetTime = Time.time + retargetInterval;
            targetLockUntil = 0f;
            return false;
        }

        SetCurrentTarget(target);
        nextRetargetTime = Time.time + retargetInterval;
        targetLockUntil = Time.time + targetLockDuration;
        return true;
    }

    private void SetCurrentTarget(ICombatTarget newTarget)
    {
        if (ReferenceEquals(currentTarget, newTarget))
            return;

        ReleaseCurrentTarget();
        currentTarget = newTarget;

        if (currentTarget is Component targetComponent && targetComponent is ITargetReservation reservationTarget)
            reservationTarget.TryRegisterAttacker(this);
    }

    private void ReleaseCurrentTarget()
    {
        if (currentTarget is Component targetComponent && targetComponent is ITargetReservation reservationTarget)
            reservationTarget.TryUnregisterAttacker(this);
    }

    private bool IsCurrentTargetValid()
    {
        if (currentTarget == null || !currentTarget.IsAlive)
            return false;

        if (currentTarget is not Component targetComponent || !targetComponent.gameObject.activeInHierarchy)
            return false;


        return true;
    }
}
