using UnityEngine;
using Zenject;

[RequireComponent(typeof(SoldierFollower))]
public class SoldierCombatAgent : BaseTargetingCombatAgent
{
    private SquadCombatCoordinator squadCombatCoordinator;
    public SoldierCombatState State { get; private set; } = SoldierCombatState.Idle;
    
    [Inject]
    public void Construct(SquadCombatCoordinator squadCombatCoordinator)
    {
        this.squadCombatCoordinator = squadCombatCoordinator;
        squadCombatCoordinator.RegisterSoldier(this);
    }
    
    private void Update()
    {
        if (!IsAlive)
            return;

        EnemyGroupFacade currentGroup = squadCombatCoordinator.CurrentTargetGroup;
        if (currentGroup == null || currentGroup.State != EnemyGroupState.Activated)
        {
            SetCurrentTarget(null);
            State = SoldierCombatState.Idle;
            return;
        }

        if (!IsCurrentTargetValidBase(currentGroup))
        {
            TryAcquireTarget(currentGroup);
        }
        else if (Time.time >= nextRetargetTime && Time.time >= targetLockUntil)
        {
            TryAcquireTarget(currentGroup);
        }

        if (!IsCurrentTargetValidBase(currentGroup))
        {
            SetCurrentTarget(null);
            State = SoldierCombatState.Idle;
            return;
        }

        Vector3 direction = currentTarget.transform.position - transform.position;
        direction.y = 0f;

        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);


        State = SoldierCombatState.Attack;
        if (attackAgent.Tick(Time.deltaTime, currentTarget, AttackOrigin.position))
            SpawnProjectileVisual(currentTarget.transform);
    }
    

    private void TryAcquireTarget(EnemyGroupFacade currentGroup)
    {
        if (currentGroup == null)
        {
            SetCurrentTarget(null);
        }

        ICombatTarget target = currentGroup.GetBestLivingEnemyTarget(transform.position, reservationPenalty);
        if (target == null)
        {
            nextRetargetTime = Time.time + retargetInterval;
            targetLockUntil = 0f;
        }

        SetCurrentTarget(target);
        nextRetargetTime = Time.time + retargetInterval;
        targetLockUntil = Time.time + targetLockDuration;
    }

    protected override void HandleDeath()
    {
        squadCombatCoordinator.UnregisterSoldier(this);
        base.HandleDeath();
    }
}