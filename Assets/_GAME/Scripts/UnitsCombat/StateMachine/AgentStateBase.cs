public abstract class AgentStateBase<TState, TModel> : State<TState>
    where TState : AgentStateBase<TState, TModel>
    where TModel : AgentRuntimeModel
{
    protected readonly TModel model;
    protected readonly CombatUnitModules modules;
    protected readonly UnitStats unitStats;
    protected readonly IAgentView agentView;
    protected readonly AgentAnimationController agentAnimationController;

    protected AgentStateBase(TModel model, CombatUnitModules modules, AgentAnimationController agentAnimationController)
    {
        this.model = model;
        this.modules = modules;
        unitStats = model.UnitStats;
        agentView = model.View;
        this.agentAnimationController = agentAnimationController;
    }
}
