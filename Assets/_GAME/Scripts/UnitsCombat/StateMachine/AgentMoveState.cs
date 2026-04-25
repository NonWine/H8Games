public abstract class AgentMoveState<TState, TModel> : AgentStateBase<TState, TModel>
    where TState : AgentStateBase<TState, TModel>
    where TModel : AgentRuntimeModel
{
    protected AgentMoveState(
        TModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController)
        : base(model, modules, agentAnimationController)
    {
    }

    public override void Enter()
    {
        agentAnimationController.SetAnimationState(UnitState.Move);
    }

    public override void Exit()
    {
    }
}
