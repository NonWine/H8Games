using UnityEngine;
using Zenject;

public sealed class HeroKinematicMover : IHeroMover
{
    private readonly PlayerView heroView;
    private readonly Transform plane;
    private readonly float minX;
    private readonly float maxX;
    private readonly float minZ;
    private readonly float maxZ;

    public HeroKinematicMover(PlayerView heroView, [Inject(Id = "Ground")] Transform ground)
    {
        this.heroView = heroView;
        plane = ground;

        Vector3 center = plane.position;
        Vector3 scale = plane.lossyScale;

        float halfWidth = 5f * scale.x;
        float halfLength = 5f * scale.z;

        minX = center.x - halfWidth;
        maxX = center.x + halfWidth;
        minZ = center.z - halfLength;
        maxZ = center.z + halfLength;
    }

    public void Move(Vector3 direction, float speed, float deltaTime)
    {
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Vector3 motion = direction.normalized * Mathf.Max(0f, speed) * deltaTime;
        CharacterController characterController = heroView.CharacterController;

        characterController.Move(motion);

        Vector3 position = heroView.transform.position;
        Vector3 clampedPosition = new Vector3(
            Mathf.Clamp(position.x, minX, maxX),
            position.y,
            Mathf.Clamp(position.z, minZ, maxZ));

        Vector3 correction = clampedPosition - position;
        if (correction.sqrMagnitude > 0.000001f)
        {
            characterController.Move(correction);
        }
    }

    public void FaceDirection(Vector3 direction)
    {
        Vector3 flatDirection = direction;
        flatDirection.y = 0f;

        if (flatDirection.sqrMagnitude <= 0.0001f)
            return;

        heroView.transform.forward = flatDirection.normalized;
    }
}