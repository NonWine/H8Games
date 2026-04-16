using UnityEngine;
using Zenject;

public class PlayerController : ITickable
{
    private readonly PlayerView heroView;
    private readonly IHeroInputReader inputReader;
    private readonly IHeroMover heroMover;
    private readonly HeroStats runtime;
    
    public PlayerController(
        PlayerView heroView,
        IHeroInputReader inputReader,
        IHeroMover heroMover,
        HeroStats runtime)
    {
        this.runtime = runtime;
        this.heroView = heroView;
        this.inputReader = inputReader;
        this.heroMover = heroMover;
    }
    
    public void Tick()
    {
        Vector3 movementDirection = inputReader.ReadMovement();
        heroView.Animator.SetFloat("Speed", movementDirection.magnitude);

        if (movementDirection.sqrMagnitude > 0f)
        {
            heroMover.Move(movementDirection, runtime.Combat.MoveSpeed, Time.deltaTime);
            heroMover.FaceDirection(movementDirection);
        }
    }
    
    
}
