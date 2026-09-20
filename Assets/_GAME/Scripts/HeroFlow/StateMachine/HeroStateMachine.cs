using System.Collections.Generic;

public class HeroStateMachine : StateMachine<HeroStateBase>
{
    public HeroStateMachine(List<HeroStateBase> allStates) : base(allStates)
    {
    }
}
