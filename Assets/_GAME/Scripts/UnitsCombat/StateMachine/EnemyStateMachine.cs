using System.Collections.Generic;

public class EnemyStateMachine : StateMachine<EnemyStateBase>
{
    public EnemyStateMachine(List<EnemyStateBase> allStates) : base(allStates)
    {
    }
}
