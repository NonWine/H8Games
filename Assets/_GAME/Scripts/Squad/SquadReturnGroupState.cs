public class SquadReturnGroupState : SquadRootStateBase
{
    private readonly IMoveProvider squadMoveProvider;

    public SquadReturnGroupState(IMoveProvider squadMoveProvider)
    {
        this.squadMoveProvider = squadMoveProvider;
    }

    public override void Enter()
    {
        squadMoveProvider.SetTarget(RootView.HomePosition, CompleteRegrouping);
    }

    public override void Exit()
    {
    }

    private void CompleteRegrouping()
    {
        signalBus.Fire(new SquadRegroupCompletedSignal());
    }
}
