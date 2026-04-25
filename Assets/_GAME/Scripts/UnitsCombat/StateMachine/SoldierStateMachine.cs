using System.Collections.Generic;

public class SoldierStateMachine : StateMachine<SoldierStateBase>
{
    public SoldierStateMachine(List<SoldierStateBase> allStates) : base(allStates)
    {
    }
}
