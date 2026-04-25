public class AgentStateBaseCombatAgentAttackState : AgentStateBase
{
    private readonly ITargetTrackerHandler targetTracker;


    public AgentStateBaseCombatAgentAttackState(ITargetTrackerHandler targetTracker)
    {
        this.targetTracker = targetTracker;
    }

    public override void Enter()
    {
        
    }

    public override void Exit()
    {
    }

}
