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
        if (direction.sqrMagnitude <= 0f)
            return;

        Vector3 motion = direction.normalized * Mathf.Max(0f, speed) * deltaTime;
        CharacterController characterController = heroView.CharacterController;

        if (characterController.enabled)
        {
            characterController.Move(motion);
            return;
        }

        heroView.transform.position += motion;
    }

    public void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Vector3 flatDirection = direction.normalized;
        flatDirection.y = 0f;
        if (flatDirection.sqrMagnitude <= 0.0001f)
            return;

        heroView.transform.forward = flatDirection;
    }
}
