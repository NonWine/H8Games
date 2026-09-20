using UnityEngine;
using Zenject;

// Single owner of where the hero looks, so joystick steering and auto-aim can no
// longer fight over the transform on the same frame. Combat wins whenever a
// target is held: the stick keeps full authority over where the hero *moves*,
// but not over where he faces, which is what makes the auto attack read as
// aiming instead of drifting. With no target, facing falls back to the input
// direction exactly as before.
public class HeroFacingController : ITickable
{
    private readonly ITargetTrackerHandler targetTracker;
    private readonly IHeroInputReader inputReader;
    private readonly IHeroMover heroMover;
    private readonly IAliveStateReader aliveState;
    private readonly UnitRotatorService unitRotatorService;
    private readonly Transform heroTransform;

    public HeroFacingController(
        ITargetTrackerHandler targetTracker,
        IHeroInputReader inputReader,
        IHeroMover heroMover,
        IAliveStateReader aliveState,
        UnitRotatorService unitRotatorService,
        Transform heroTransform)
    {
        this.targetTracker = targetTracker;
        this.inputReader = inputReader;
        this.heroMover = heroMover;
        this.aliveState = aliveState;
        this.unitRotatorService = unitRotatorService;
        this.heroTransform = heroTransform;
    }

    public void Tick()
    {
        if (!aliveState.IsAlive)
        {
            return;
        }

        if (TryFaceCurrentTarget())
        {
            return;
        }

        FaceInputDirection();
    }

    private bool TryFaceCurrentTarget()
    {
        ICombatTarget target = targetTracker.CurrentTarget;
        if (target == null || !target.IsAlive)
        {
            return false;
        }

        unitRotatorService.RotateTowards(heroTransform, target.transform);
        return true;
    }

    private void FaceInputDirection()
    {
        Vector3 movementDirection = inputReader.ReadMovement();
        if (movementDirection.sqrMagnitude <= 0f)
        {
            return;
        }

        heroMover.FaceDirection(movementDirection);
    }
}
