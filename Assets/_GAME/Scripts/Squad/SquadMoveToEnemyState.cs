using UnityEngine;
using Zenject;

public class SquadMoveToEnemyState: SquadRootStateBase
{
    private readonly SignalBus signalBus;
    private readonly SquadMoveProvider _squadMoveProvider;
    private readonly IEnemyGroupProvider enemyGroupProvider;
    private readonly Transform homePosition;


    public override void Enter()
    {
        _squadMoveProvider.SetTarget(enemyGroupProvider.CurrentTargetGroup.transform.position,CompleteRegrouping);
    }

    public override void Exit()
    {
        
    }

    private void CompleteRegrouping()
    {
        signalBus.Fire(new SquadReachedEnemySignal());
    }
}