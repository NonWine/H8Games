using UnityEngine;
using Zenject;

public class PlayerController : ITickable
{
    private readonly PlayerView heroView;
    private readonly HeroCombatRuntime runtime;
    private readonly IHeroInputReader inputReader;
    private readonly IHeroMover heroMover;

    public PlayerController(
        PlayerView heroView,
        HeroCombatRuntime runtime,
        IHeroInputReader inputReader,
        IHeroMover heroMover)
    {
        this.heroView = heroView;
        this.runtime = runtime;
        this.inputReader = inputReader;
        this.heroMover = heroMover;
    }
    
    public void Tick()
    {
        if (!runtime.IsAlive)
            return;

        Vector3 movementDirection = inputReader.ReadMovement();
        heroView.Animator.SetFloat("Speed", movementDirection.magnitude);

        if (movementDirection.sqrMagnitude > 0f)
        {
            heroMover.Move(movementDirection, runtime.RuntimeStats.Combat.MoveSpeed, Time.deltaTime);
            heroMover.FaceDirection(movementDirection);
        }
    }
    
    
}
