using System.Collections.Generic;

public class SoldierStateMachine : StateMachine<SoldierStateBase>
{
    public SoldierStateMachine(List<SoldierStateBase> allStates) : base(allStates)
    {
    }
}

public class EnemyStateMachine : StateMachine<EnemyStateBase>
{
    public EnemyStateMachine(List<EnemyStateBase> allStates) : base(allStates)
    {
    }
}
