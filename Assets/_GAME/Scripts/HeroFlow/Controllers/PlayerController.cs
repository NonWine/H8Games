using UnityEngine;
using Zenject;

public class PlayerController : ITickable
{
    private readonly PlayerView heroView;
    private readonly IHeroInputReader inputReader;
    private readonly IHeroMover heroMover;
    private readonly IAliveStateReader aliveState;
    private readonly HeroStats runtime;

    public PlayerController(
        PlayerView heroView,
        IHeroInputReader inputReader,
        IHeroMover heroMover,
        IAliveStateReader aliveState,
        HeroStats runtime)
    {
        this.runtime = runtime;
        this.heroView = heroView;
        this.inputReader = inputReader;
        this.heroMover = heroMover;
        this.aliveState = aliveState;
    }

    public void Tick()
    {
        Vector3 movementDirection = aliveState.IsAlive ? inputReader.ReadMovement() : Vector3.zero;
        heroView.Animator.SetFloat("Speed", movementDirection.magnitude);

        if (movementDirection.sqrMagnitude > 0f)
        {
            heroMover.Move(movementDirection, runtime.Combat.MoveSpeed, Time.deltaTime);
        }
    }
    
    
}
