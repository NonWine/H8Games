using Cysharp.Threading.Tasks;

public class AgentDeadState : AgentStateBase
{
    
    public override void Enter()
    {
        agentAnimationController.SetAnimationState(UnitState.Dead);
        modules.Death.HandleDeathAsync().Forget();
    }

    public override void Exit()
    {
    }
}
