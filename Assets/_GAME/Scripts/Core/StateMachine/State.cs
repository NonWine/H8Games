using System;

public abstract class State<TState> : IState where TState : State<TState>
{
    protected IStateMachine<TState> stateMachine;
    

    public abstract void Enter();
    public abstract void Exit();

    public virtual void Tick()
    {
    }

    protected void ChangeState<T>() where T : TState
    {
        stateMachine.ChangeState<T>();
    }

    internal void SetStateMachine(IStateMachine<TState> owner)
    {
        stateMachine = owner;
    }
}
