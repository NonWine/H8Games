using UnityEngine;

public interface IAgentController
{
    Transform transform { get; }
    bool IsAlive { get; }

    void SetIdentity(string unitId);
    void ResetState();
}
