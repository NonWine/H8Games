using System;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(SoldierFollower))]
public class SoldierCombatAgent : BaseTargetingCombatAgent
{
    [field:SerializeField] public SoldierFollower SoldierFollower { get; private set; }
    private IEnemyGroupProvider currentEnemyGroupProvider;
    private ISoldierCombatRegistryProvider soldierCombatRegistryProvider;
    private ISquadMovementStateReader stateReader;

    [Inject]
    public void Construct(IEnemyGroupProvider currentEnemyGroupProvider,
        ISoldierCombatRegistryProvider soldierCombatRegistryProvider,
        ISquadMovementStateReader stateReader )
    {
        this.currentEnemyGroupProvider = currentEnemyGroupProvider;
        this.soldierCombatRegistryProvider = soldierCombatRegistryProvider;
        this.stateReader = stateReader;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        soldierCombatRegistryProvider?.UnregisterSoldier(this);
        SetCurrentTarget(null);
    }

    protected override void Update()
    {
        base.Update();
        if (!IsAlive)
            return;
        
        EnemyGroupViewController currentGroup = currentEnemyGroupProvider.CurrentTargetGroup;
        if (currentGroup == null || currentGroup.State != EnemyGroupState.Activated)
        {
            SetCurrentTarget(null);
            State = UnitState.Idle;
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
            State = UnitState.Idle;
            return;
        }

        RotateTowardsCurrentTarget(transform);

        State = UnitState.Attack;
        if (attackAgent.Tick(Time.deltaTime, currentTarget, AttackOrigin.position))
            SpawnProjectileVisual(currentTarget.transform);
    }

    private void LateUpdate()
    {
                
        if(stateReader.IsMoving) 
            State = UnitState.Move;
        
        if(currentEnemyGroupProvider.CurrentTargetGroup != null) return;
        
        if(SoldierFollower.State == SoldierFormationState.WaitingInFormation)
            State = UnitState.Idle;
        else if (SoldierFollower.State == SoldierFormationState.MovingToSlot)
            State = UnitState.Move;


    }

    private void TryAcquireTarget(EnemyGroupViewController currentGroup)
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
        soldierCombatRegistryProvider?.UnregisterSoldier(this);
        base.HandleDeath();
    }
}