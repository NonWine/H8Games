public class SquadMoveToEnemyState : SquadRootStateBase
{
    private readonly IMoveProvider squadMoveProvider;
    private readonly IDestinationProvider enemyGroupProvider;

    public SquadMoveToEnemyState(IMoveProvider squadMoveProvider, IDestinationProvider enemyGroupProvider)
    {
        this.squadMoveProvider = squadMoveProvider;
        this.enemyGroupProvider = enemyGroupProvider;
    }

    public override void Enter()
    {
        if(enemyGroupProvider.HasDestination) squadMoveProvider.SetTarget(enemyGroupProvider.Destination, CompleteMove);
    }

    public override void Exit()
    {
    }

    private void CompleteMove()
    {
        signalBus.Fire(new SquadReachedEnemySignal());
    }
}
