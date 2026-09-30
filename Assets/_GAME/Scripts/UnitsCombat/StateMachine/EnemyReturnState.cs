using UnityEngine;

public class EnemyReturnState : EnemyStateBase
{
    private const float PostFacingTolerance = 3f;

    private readonly EnemySortiePlanner sortiePlanner;
    private readonly EnemyNavMeshMover mover;
    private readonly EnemySortieConfig sortieConfig;
    private readonly UnitRotatorService unitRotatorService;

    private bool isWalking;
    private float returnDeadline;

    public EnemyReturnState(
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
        sortiePlanner.EndFight();
        returnDeadline = Time.time + sortieConfig.MaxReturnDuration;
        isWalking = mover.TryMoveTo(Enemy.PostPosition, sortieConfig.ReturnSpeedMultiplier);
        agentAnimationController.SetAnimationState(isWalking ? UnitState.Move : UnitState.Idle);
    }

    public override void Tick()
    {
        if (Enemy.HasValidTarget)
        {
            ChangeState<EnemyAttackState>();
            return;
        }

        if (Time.time >= returnDeadline)
        {
            ChangeState<EnemyIdleState>();
            return;
        }

        if (isWalking)
        {
            TickWalking();
            return;
        }

        TickSettlingIntoPost();
    }

    public override void Exit()
    {
        mover.Stop();
    }

    private void TickWalking()
    {
        if (!mover.HasArrived(sortieConfig.ArriveThreshold))
        {
            unitRotatorService.RotateTowards(
                Enemy.Transform,
                mover.Heading,
                Time.deltaTime,
                sortieConfig.ReturnRotationSpeed);
            return;
        }

        isWalking = false;
        mover.Stop();
        agentAnimationController.SetAnimationState(UnitState.Idle);
    }

    private void TickSettlingIntoPost()
    {
        Vector3 postForward = Enemy.PostRotation * Vector3.forward;

        unitRotatorService.RotateTowards(
            Enemy.Transform,
            postForward,
            Time.deltaTime,
            sortieConfig.ReturnRotationSpeed);

        if (IsFacing(postForward))
        {
            ChangeState<EnemyIdleState>();
        }
    }

    private bool IsFacing(Vector3 direction)
    {
        Vector3 forward = Enemy.Transform.forward;
        forward.y = 0f;
        direction.y = 0f;

        return Vector3.Angle(forward, direction) <= PostFacingTolerance;
    }
}
