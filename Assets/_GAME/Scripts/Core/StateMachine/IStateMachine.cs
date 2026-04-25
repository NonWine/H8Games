using System;

public interface IStateMachine<TState> where TState : State<TState>
{
    void ChangeState<T>() where T : TState;
}
