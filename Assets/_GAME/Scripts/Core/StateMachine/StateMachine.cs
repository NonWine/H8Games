using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public abstract class StateMachine<TState> : IDisposable, IStateMachine<TState>
    where TState : State<TState>
{
    private readonly Dictionary<Type, TState> states = new Dictionary<Type, TState>();

    public TState CurrentState { get; private set; }

    protected StateMachine(List<TState> allStates)
    {
        foreach (var state in allStates)
        {
            states[state.GetType()] = state;
            state.SetStateMachine(this);
        }
    }

    public void Tick()
    {
        CurrentState?.Tick();
    }

    public void Dispose()
    {
        CurrentState?.Exit();
    }

    public void ChangeState<T>() where T : TState
    {
        var stateType = typeof(T);
        if (!states.TryGetValue(stateType, out var newState))
        {
            Debug.LogError($"State {stateType} is not registered in {GetType().Name}.");
            return;
        }

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
        
    }
}
