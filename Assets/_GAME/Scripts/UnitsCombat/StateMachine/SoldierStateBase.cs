public abstract class SoldierStateBase : AgentStateBase<SoldierStateBase, SoldierRuntimeModel>
{
    protected SoldierStateBase(
        SoldierRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController)
        : base(model, modules, agentAnimationController)
    {
    }

    protected SoldierRuntimeModel Soldier => model;
}
