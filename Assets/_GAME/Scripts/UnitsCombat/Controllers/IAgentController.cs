using UnityEngine;

public interface IAgentController
{
    Transform Transform { get; }
    void Spawn(Vector3 position, Quaternion rotation);
    void Despawn();
}
