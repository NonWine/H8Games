using Zenject;

public abstract class SquadRootStateBase : IState
{
    [Inject]  protected readonly SquadRoot root;
    [Inject] protected SignalBus signalBus;
   
    
    public abstract void Enter();

    public abstract void Exit();

    public virtual void Tick()
    {
        
    }
}

