
public class EnemyIdleState : EnemyStateBase
{
    private readonly EnemySortiePlanner sortiePlanner;

    public EnemyIdleState(
        EnemyRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController,
        EnemySortiePlanner sortiePlanner)
        : base(model, modules, agentAnimationController)
    {
        this.sortiePlanner = sortiePlanner;
    }

    public override void Enter()
    {
        sortiePlanner.EndFight();
        agentAnimationController.SetAnimationState(UnitState.Idle);
    }

    public override void Tick()
    {
        if (Enemy.HasValidTarget)
            ChangeState<EnemyAttackState>();
    }

    public override void Exit()
    {
    }
}
