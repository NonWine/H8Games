using UnityEngine;

public interface IHeroMover
{
    void Move(Vector3 direction, float speed, float deltaTime);
    void FaceDirection(Vector3 direction);
}
