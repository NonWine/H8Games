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
        agentAnimationController.SetAnimationState(UnitState.Idle);
    }

    public override void Exit()
    {
    }
}
