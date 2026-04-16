using System;

public class SquadMovementFacade
{
    private readonly SquadRootStateMachine stateMachine;
    private readonly SquadMoveProvider squadMoveProvider;

    public SquadMovementFacade(SquadRootStateMachine stateMachine, SquadMoveProvider squadMoveProvider)
    {
        this.stateMachine = stateMachine;
        this.squadMoveProvider = squadMoveProvider;
        stateMachine.Initialize();
    }

    public void MoveToEnemy()
    {
        stateMachine.ChangeState<SquadMoveToEnemyState>();
    }

    public void ReturnHome()
    {
        stateMachine.ChangeState<SquadReturnGroupState>();
    }

    public void EnterPreparationIdle()
    {
        stateMachine.ChangeState<SquadRootIdleState>();
    }

    public void Stop()
    {
        squadMoveProvider.Stop();
    }
}
