using System;
using Zenject;

public class SquadMovementFacade : ITickable
{
    private readonly SquadRootStateMachine stateMachine;
    private readonly IMoveProvider squadMoveProvider;

    public SquadMovementFacade(SquadRootStateMachine stateMachine, IMoveProvider squadMoveProvider)
    {
        this.stateMachine = stateMachine;
        this.squadMoveProvider = squadMoveProvider;
        stateMachine.Initialize();
    }

    public void MoveToEnemy()
    {
        stateMachine.ChangeState<SquadMoveToEnemyState>();
    }

    public void FollowHero()
    {
        stateMachine.ChangeState<SquadFollowHeroState>();
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

    public void Tick()
    {
        stateMachine.Tick();
    }
}
