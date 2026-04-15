using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Zenject;

public class SquadRootStateMachine
{
    private IState currentState;
    private readonly Dictionary<Type, IState> states = new Dictionary<Type, IState>();
    private readonly SignalBus signalBus;

    public SquadRootStateMachine(SignalBus signalBus, List<IState> allStates)
    {
        this.signalBus = signalBus;
        foreach (var state in allStates)
        {
            states[state.GetType()] = state;
        }
    }
    
    public void ChangeState<T>() where T : IState
    {
        ChangeState(typeof(T));
    }

    private void ChangeState(Type stateType)
    {

        if (!states.TryGetValue(stateType, out var newState))
        {
            UnityEngine.Debug.LogError($"State {stateType} not registered!");
            return;
        }

        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void Tick()
    {
        currentState?.Tick();
    }

    public void Dispose()
    {
        currentState?.Exit();
    }
    
    public void Initialize()
    {
        ChangeState<SquadRootIdleState>();
    }
}