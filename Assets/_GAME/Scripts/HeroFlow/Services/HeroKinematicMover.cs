using UnityEngine;

public sealed class HeroKinematicMover : IHeroMover
{
    private readonly PlayerView heroView;

    public HeroKinematicMover(PlayerView heroView)
    {
        this.heroView = heroView;
    }

    public void Move(Vector3 direction, float speed, float deltaTime)
    {
        Vector3 motion = direction.normalized * Mathf.Max(0f, speed) * deltaTime;
        CharacterController characterController = heroView.CharacterController;
        characterController.Move(motion);
        heroView.transform.position += motion;
    }

    public void FaceDirection(Vector3 direction)
    {
        Vector3 flatDirection = direction.normalized;
        flatDirection.y = 0f;
        heroView.transform.forward = flatDirection;
    }
}
