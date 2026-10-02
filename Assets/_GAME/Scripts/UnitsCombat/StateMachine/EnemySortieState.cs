using UnityEngine;

public class EnemySortieState : EnemyStateBase
{
    private readonly EnemySortiePlanner sortiePlanner;
    private readonly EnemyNavMeshMover mover;
    private readonly EnemySortieConfig sortieConfig;
    private readonly UnitRotatorService unitRotatorService;

    private bool isMoving;
    private float moveDeadline;

    public EnemySortieState(
        EnemyRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController,
        EnemySortiePlanner sortiePlanner,
        EnemyNavMeshMover mover,
        EnemySortieConfig sortieConfig,
        UnitRotatorService unitRotatorService)
        : base(model, modules, agentAnimationController)
    {
        this.sortiePlanner = sortiePlanner;
        this.mover = mover;
        this.sortieConfig = sortieConfig;
        this.unitRotatorService = unitRotatorService;
    }

    public override void Enter()
    {
        moveDeadline = Time.time + sortieConfig.MaxSortieDuration;
        EnemySortieCoordinator.RegisterSortie(sortiePlanner.Destination);
        isMoving = mover.TryMoveTo(sortiePlanner.Destination, sortieConfig.SortieSpeedMultiplier);

        if (isMoving)
        {
            agentAnimationController.SetAnimationState(UnitState.Move);
        }
    }

    public override void Tick()
    {
        if (!Enemy.HasValidTarget)
        {
            ChangeState<EnemyReturnState>();
            return;
        }

        if (!isMoving || Time.time >= moveDeadline || mover.HasArrived(sortieConfig.ArriveThreshold))
        {
            ChangeState<EnemyAttackState>();
            return;
        }

        unitRotatorService.RotateTowards(Enemy.Transform, Enemy.CurrentTarget.transform);
    }

    public override void Exit()
    {
        EnemySortieCoordinator.UnregisterSortie(sortiePlanner.Destination);
        mover.Stop();
    }
}
