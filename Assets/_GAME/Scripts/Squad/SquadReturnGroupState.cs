using System;
using Zenject;

public class SquadReturnGroupState: SquadRootStateBase
{
    private readonly SignalBus signalBus;
    private readonly SquadMoveProvider _squadMoveProvider;
    private SquadHomeController  homePosition;


    public override void Enter()
    {
        _squadMoveProvider.SetTarget(homePosition.HomePosition,CompleteRegrouping);
    }

    public override void Exit()
    {
        
    }

    private void CompleteRegrouping()
    {
        signalBus.Fire(new SquadRegroupCompletedSignal());
    }
}