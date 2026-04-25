using Cysharp.Threading.Tasks;

public class AgentStateBaseCombatAgentDeadState : AgentStateBase
{
    private readonly CombatUnitModules modules;


    public AgentStateBaseCombatAgentDeadState(CombatUnitModules modules)
    {
        this.modules = modules;
    }

    public override void Enter()
    {
        modules.Death.HandleDeathAsync().Forget();
    }

    public override void Exit()
    {
    }
}
