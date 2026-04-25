public abstract class EnemyStateBase : AgentStateBase<EnemyStateBase, EnemyRuntimeModel>
{
    protected EnemyStateBase(
        EnemyRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController)
        : base(model, modules, agentAnimationController)
    {
    }
}
