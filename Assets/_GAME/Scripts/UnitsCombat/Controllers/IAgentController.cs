using UnityEngine;

public interface IAgentController
{
    void Spawn(Vector3 position, Quaternion rotation);
    void Despawn();
}
