public class AgentIdleState : AgentStateBase
{
    public override void Enter()
    {
        agentAnimationController.SetAnimationState(UnitState.Idle);
    }

    public override void Exit()
    {
    }
}
