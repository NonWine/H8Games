using System;
using System.Collections.Generic;
using UnityEngine;

public class AgentStateMachine : StateMachine<AgentStateBase>
{
    public AgentStateMachine(List<AgentStateBase> allStates) : base(allStates)
    {
    }
}
