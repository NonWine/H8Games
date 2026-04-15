using System;
using UnityEngine;
using Zenject;

public class SquadMovementFacade
{
    private readonly SquadRootStateMachine stateMachine;
    
    public SquadMovementFacade(SquadRootStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        stateMachine.Initialize();
    }

    public void MoveToEnemy( Action onReached = null)
    {
        stateMachine.ChangeState<SquadMoveToEnemyState>();
    }

    public void ReturnHome(Action onReached = null)
    {
        stateMachine.ChangeState<SquadReturnGroupState>();
    }

    public void EnterPreparationIdle()
    {
        stateMachine.ChangeState<SquadRootIdleState>();
    }

    public void Stop()
    {
        stateMachine.ChangeState<SquadRootIdleState>();
    }
}