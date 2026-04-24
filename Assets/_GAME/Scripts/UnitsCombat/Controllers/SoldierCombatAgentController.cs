using UnityEngine;

public sealed class SoldierCombatAgentController : BaseCombatAgentController
{
    private readonly IEnemyGroupProvider currentEnemyGroupProvider;
    private readonly ISquadMovementStateReader stateReader;
    public SoldierFollower SoldierFollower { get; private set; }

    public SoldierCombatAgentController(
        BaseCombatAgentView baseCombatAgentView,
        ModulesFactoryCollection modulesFactoryCollection,
        SquadFollowSettings settings,
        ISquadSlotPositionProvider squadSlotPositionProvider,
        ISquadMovementStateReader movementStateReader,
        SquadRootView squadRootView,
        IEnemyGroupProvider currentEnemyGroupProvider)
        : base(baseCombatAgentView, modulesFactoryCollection)
    {
        this.currentEnemyGroupProvider = currentEnemyGroupProvider;
        stateReader = movementStateReader;
    }
    

    public override void Tick()
    {
        base.Tick();

        if (!IsAlive || SoldierFollower == null || currentEnemyGroupProvider == null || stateReader == null)
            return;

        EnemyGroupViewController currentGroup = currentEnemyGroupProvider.CurrentTargetGroup;
        var tracker = modules.TargetTracker;

        if (currentGroup == null || currentGroup.State != EnemyGroupState.Activated)
        {
            tracker.SetCurrentTarget(null, baseCombatAgentView);
            State = UnitState.Idle;
            return;
        }

        if (!tracker.IsCurrentTargetValid(currentGroup))
        {
            TryAcquireTarget(currentGroup);
        }
        else if (tracker.ShouldRetarget())
        {
            TryAcquireTarget(currentGroup);
        }

        if (!tracker.IsCurrentTargetValid(currentGroup))
        {
            tracker.SetCurrentTarget(null, baseCombatAgentView);
            State = UnitState.Idle;
            return;
        }

        tracker.RotateTowardsCurrentTarget(baseCombatAgentView.transform);
        State = UnitState.Attack;
        UpdateFormation();
    }

    private void UpdateFormation()
    {
        if (State == UnitState.Attack || State == UnitState.Dead)
            return;

        SoldierFollower.UpdateFormation();
        if (!stateReader.IsMoving)
        {
            if (SoldierFollower.State == SoldierFormationState.WaitingInFormation)
            {
                State = UnitState.Idle;
            }
            else if (SoldierFollower.State == SoldierFormationState.MovingToSlot)
            {
                State = UnitState.Move;
            }
        }
        else
        {
            State = UnitState.Move;
        }
    }

    private void TryAcquireTarget(EnemyGroupViewController currentGroup)
    {
        var tracker = modules.TargetTracker;

        if (currentGroup == null)
        {
            tracker.SetCurrentTarget(null, baseCombatAgentView);
            return;
        }

        ICombatTarget target = currentGroup.GetBestLivingEnemyTarget(baseCombatAgentView.transform.position, unitStats.ReservationPenalty);

        if (target == null)
        {
            tracker.SetCurrentTarget(null, agentTransform);
            tracker.ResetTargetingTimers();
            return;
        }

        tracker.SetCurrentTarget(target, agentTransform);
        tracker.MarkRetargetWindow();
    }
}
