// EnemyIdleState is implemented in AgentIdleState.cs with the shared idle state definitions.

public class EnemyIdleState : EnemyStateBase
{
    public EnemyIdleState(
        EnemyRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController)
        : base(model, modules, agentAnimationController)
    {
    }

    public override void Enter()
    {
        Enemy.SetState(UnitState.Idle);
        agentAnimationController.SetAnimationState(UnitState.Idle);
    }

    public override void Exit()
    {
    }
}
