public abstract class AgentStateBase : State<AgentStateBase>
{
    protected readonly CombatUnitModules modules;
    protected readonly UnitStats unitStats;
    protected readonly BaseCombatAgentView baseCombatAgentView;
    protected readonly AgentAnimationController agentAnimationController;
}
