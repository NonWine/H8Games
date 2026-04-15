using UnityEngine;
using Zenject;

[RequireComponent(typeof(SoldierFollower))]
public class SoldierCombatAgent : BaseTargetingCombatAgent
{
    private ICurrentEnemyGroupProvider currentEnemyGroupProvider;
    private ISoldierCombatRegistryProvider _soldierCombatRegistryProvider;

    public SoldierCombatState State { get; private set; } = SoldierCombatState.Idle;

    [Inject]
    public void Construct(ICurrentEnemyGroupProvider currentEnemyGroupProvider, ISoldierCombatRegistryProvider soldierCombatRegistryProvider)
    {
        this.currentEnemyGroupProvider = currentEnemyGroupProvider;
        this._soldierCombatRegistryProvider = soldierCombatRegistryProvider;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _soldierCombatRegistryProvider?.RegisterSoldier(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _soldierCombatRegistryProvider?.UnregisterSoldier(this);
        SetCurrentTarget(null);
    }

    private void Update()
    {
        if (!IsAlive)
            return;

        EnemyGroupFacade currentGroup = currentEnemyGroupProvider.CurrentTargetGroup;
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
        else if (ShouldRetarget())
        {
            TryAcquireTarget(currentGroup);
        }

        if (!IsCurrentTargetValidBase(currentGroup))
        {
            SetCurrentTarget(null);
            State = SoldierCombatState.Idle;
            return;
        }

        RotateTowardsCurrentTarget(transform);

        State = SoldierCombatState.Attack;
        if (attackAgent.Tick(Time.deltaTime, currentTarget, AttackOrigin.position))
            SpawnProjectileVisual(currentTarget.transform);
    }

    private void TryAcquireTarget(EnemyGroupFacade currentGroup)
    {
        if (currentGroup == null)
        {
            SetCurrentTarget(null);
            return;
        }

        ICombatTarget target = currentGroup.GetBestLivingEnemyTarget(transform.position, reservationPenalty);
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
        _soldierCombatRegistryProvider?.UnregisterSoldier(this);
        base.HandleDeath();
    }
}