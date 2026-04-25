public class AgentMoveState : AgentStateBase
{
    public override void Enter()
    {
        agentAnimationController.SetAnimationState(UnitState.Move);
    }

    public override void Exit()
    {
    }
}
